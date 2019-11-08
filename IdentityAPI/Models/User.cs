using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityAPI.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name cannot be empty.")]
        public string Fullname { get; set; }

        [Required(ErrorMessage = "Username cannot be empty")]
        [MinLength(6, ErrorMessage = "Minimum 6 characters required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password cannot be empty")]
        [MinLength(8, ErrorMessage = "Minimum 8 characters required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email cannot be empty")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string Role { get; set; }

        public string Status { get; set; }
    }
}
