using System.Security.Claims;

namespace HowsYourDayApi.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var idString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(idString, out var id)
                ? id
                : throw new InvalidOperationException("Invalid user ID format.");
        }
    }
}
