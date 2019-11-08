using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityAPI.Helpers;
using IdentityAPI.Infrastructure;
using IdentityAPI.Models;
using IdentityAPI.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace IdentityAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private IdentityDbContext context;
        private IConfiguration config;

        public IdentityController(IdentityDbContext dbContext, IConfiguration configuration)
        {
            context = dbContext;
            config = configuration;
        }

        [HttpPost("register", Name = "RegisterUser")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<dynamic>> Register(User user)
        {
            TryValidateModel(user);
            if (ModelState.IsValid)
            {
                user.Status = "Not Verified";
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
                await SendVerificationMailAsync(user);
                return Created("", new
                {
                    user.Id,
                    user.Fullname,
                    user.Username,
                    user.Email
                });
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPost("token", Name = "GetToken")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public ActionResult<dynamic> GetToken(LoginModel model)
        {
            TryValidateModel(model);
            if (ModelState.IsValid)
            {
                var user = context.Users.SingleOrDefault(s => s.Username.Equals(model.Username) 
                && s.Password.Equals(model.Password) && s.Status.Equals("Verified"));
                if (user != null)
                {
                    var token = GenerateToken(user);
                    return Ok(new { user.Fullname, user.Email, user.Username,user.Role, Token = token });
                }
                else
                {
                    return Unauthorized();
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [NonAction]
        private string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim( JwtRegisteredClaimNames.Sub,user.Fullname),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud,"catalogapi"),
                new Claim(JwtRegisteredClaimNames.Aud,"orderapi"),
                new Claim(JwtRegisteredClaimNames.Aud,"paymentapi"),
                new Claim(JwtRegisteredClaimNames.Aud,"basketapi"),
                new Claim(ClaimTypes.Role,user.Role)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetValue<string>("Jwt:secret")));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: config.GetValue<string>("Jwt:issuer"),
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
                );
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }

        [NonAction]
        private async Task SendVerificationMailAsync(User user)
        {
            var userObj = new { user.Id,user.Fullname,user.Email,user.Username};
            var messageText = JsonConvert.SerializeObject(userObj);
            StorageAccountHelper accountHelper = new StorageAccountHelper();
            accountHelper.StorageConnectionString = config.GetConnectionString("StorageConnection");
            await accountHelper.SendMessageAsync(messageText, "users");
        }
    }
}