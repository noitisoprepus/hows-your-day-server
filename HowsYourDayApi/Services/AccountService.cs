using HowsYourDayApi.DTOs.Authentication;
using HowsYourDayApi.Models;
using Microsoft.AspNetCore.Identity;

namespace HowsYourDayApi.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AccountService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public async Task<IdentityResult> RegisterAsync(string username, string password)
        {
            var user = new AppUser
            {
                // Only store username and no email for user anonymity
                UserName = username
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
                await _userManager.AddToRoleAsync(user, "User");

            return result;
        }

        public async Task<SignInResult> LoginAsync(string username, string password, HttpContext httpContext)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return SignInResult.Failed;

            var result = await _signInManager.PasswordSignInAsync(user, password, true, false);
            if (result.Succeeded)
            {
                var tokenDto = await _tokenService.CreateToken(user, true);
                _tokenService.StoreTokensToCookie(tokenDto, httpContext);
            }

            return result;
        }

        public async Task LogoutAsync(HttpContext httpContext)
        {
            await _signInManager.SignOutAsync();
            _tokenService.ClearTokenCookie(httpContext);
        }

        public async Task DeleteUserAsync(AppUser user)
        {
            await _userManager.DeleteAsync(user);
        }
    }
}
