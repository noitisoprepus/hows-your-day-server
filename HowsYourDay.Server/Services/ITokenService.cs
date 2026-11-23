using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.Services
{
    public interface ITokenService
    {
        Task<TokenResult> CreateTokenAsync(AppUser user);
        Task<TokenResult?> RefreshTokenAsync(string? refreshToken);
        Task RevokeAsync(Guid userId);
    }
}
