using HowsYourDayApi.DTOs.Authentication;
using HowsYourDayApi.Models;
using Microsoft.AspNetCore.Identity;

namespace HowsYourDayApi.Services
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterAsync(string username, string password);
        Task<SignInResult> LoginAsync(string username, string password, HttpContext httpContext);
        Task LogoutAsync(HttpContext httpContext);
        Task DeleteUserAsync(AppUser user);
    }
}
