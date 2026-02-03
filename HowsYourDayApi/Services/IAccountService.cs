using HowsYourDayApi.DTOs.Authentication;
using Microsoft.AspNetCore.Identity;

namespace HowsYourDayApi.Services
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterAsync(RegisterDTO registerDTO, HttpContext httpContext);
        Task<SignInResult> LoginAsync(LoginDTO login, HttpContext httpContext);
        Task LogoutAsync(HttpContext httpContext);
    }
}
