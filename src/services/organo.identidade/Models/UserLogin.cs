using System.ComponentModel.DataAnnotations;

namespace Organo.Auth.Models
{
    public class UserLogin
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid e-mail formate")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }
    }
}