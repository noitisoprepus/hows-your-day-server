using HowsYourDay.Server.DTOs.Authentication;
using HowsYourDay.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace HowsYourDay.Server.Services
{
    public interface IAccountService
    {
        Task<AppUser?> GetUserAsync(Guid userId);
        Task<IdentityResult> RegisterAsync(string username, string password);
        Task<SignInResult> LoginAsync(string username, string password, HttpContext httpContext);
        Task LogoutAsync(HttpContext httpContext);
        Task DeleteUserAsync(AppUser user);
    }
}
