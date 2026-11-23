using HowsYourDay.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace HowsYourDay.Server.Services
{
    public interface IAccountService
    {
        Task<AppUser?> GetUserAsync(Guid userId);
        Task<IdentityResult> RegisterAsync(string username, string password);
        Task<TokenResult?> LoginApiAsync(string username, string password);
        Task<SignInResult> LoginUiAsync(string username, string password);
        Task LogoutUiAsync();
        Task DeleteUserAsync(AppUser user);
    }
}
