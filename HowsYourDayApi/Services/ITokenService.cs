using HowsYourDayApi.DTOs.Authentication;
using HowsYourDayApi.Models;

namespace HowsYourDayApi.Services
{
    public interface ITokenService
    {
        Task<TokenDTO> CreateToken(AppUser user, bool populateExpiry);
        Task<TokenDTO> RefreshToken(TokenDTO tokenDTO);
        void StoreTokensToCookie(TokenDTO tokenDTO, HttpContext context);
        void ClearTokenCookie(HttpContext context);
    }
}