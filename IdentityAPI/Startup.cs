using IdentityAPI.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Swagger;

namespace IdentityAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<IdentityDbContext>(config =>
            {
                config.UseSqlServer(Configuration.GetConnectionString("IdentityConnection"));
            });

            services.AddCors(c =>
            {
                c.AddDefaultPolicy(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Identity API",
                    Version = "1.0",
                    Description = "Authentication methods for Eshop Application"
                });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();

            app.UseSwagger();

            InitializeDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API");
                    options.RoutePrefix = "";
                });
            }

            app.UseMvc();
        }

        private void InitializeDatabase(IApplicationBuilder application)
        {
            using (var serviceScope = application.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var dbContext = serviceScope.ServiceProvider.GetRequiredService<IdentityDbContext>())
                {
                    dbContext.Database.Migrate();
                }
            }
        }
    }
}
