using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace HowsYourDay.Server.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var idString = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(idString, out var id)
                ? id
                : throw new InvalidOperationException("Invalid user ID format.");
        }
    }
}
