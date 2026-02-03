using System.ComponentModel.DataAnnotations;

namespace HowsYourDayApi.DTOs.Authentication
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = "";
        [Required]
        public string Password { get; set; } = "";
    }   
}