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

        public async Task<IdentityResult> RegisterAsync(RegisterDTO registerDTO, HttpContext httpContext)
        {
            var user = new AppUser
            {
                UserName = registerDTO.EmailAddress,
                Email = registerDTO.EmailAddress
            };

            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            // TODO: Have the user confirm their email address.
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(user, "User");

            return result;
        }

        // TODO: Require email confirmation to sign-in
        public async Task<SignInResult> LoginAsync(LoginDTO login, HttpContext httpContext)
        {
            var user = await _userManager.FindByEmailAsync(login.EmailAddress);
            if (user == null)
                return SignInResult.Failed;

            var result = await _signInManager.PasswordSignInAsync(user, login.Password, false, false);
            if (result.Succeeded)
            {
                var tokenDTO = await _tokenService.CreateToken(user, true);
                _tokenService.StoreTokensToCookie(tokenDTO, httpContext);
            }

            return result;
        }

        public async Task LogoutAsync(HttpContext httpContext)
        {
            await _signInManager.SignOutAsync();
            _tokenService.ClearTokenCookie(httpContext);
        }
    }
}
