using System.ComponentModel.DataAnnotations;

namespace HowsYourDay.Server.DTOs.Authentication
{
    public class RegisterDto
    {
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}