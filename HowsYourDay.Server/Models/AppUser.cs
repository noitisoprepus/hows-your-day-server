using Microsoft.AspNetCore.Identity;

namespace HowsYourDay.Server.Models
{
    public class AppUser : IdentityUser
    {
        public DateTime CreatedAtUtc { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}