using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.Services
{
    public interface ITokenService
    {
        Task<TokenResult> CreateToken(AppUser user, bool populateExpiry);
        Task<TokenResult> RefreshToken(string refreshToken);
        void StoreTokensToCookie(string accessToken, string refreshToken, HttpContext context);
        void ClearTokenCookie(HttpContext context);
    }
}