using HowsYourDay.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace HowsYourDay.Server.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AccountService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public async Task<AppUser?> GetUserAsync(Guid userId)
        {
            return await _userManager.FindByIdAsync(userId.ToString());
        }

        public async Task<IdentityResult> RegisterAsync(string username, string password)
        {
            var user = new AppUser { UserName = username };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
                await _userManager.AddToRoleAsync(user, "User");

            return result;
        }

        public async Task<TokenResult?> LoginApiAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user is null)
                return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            return result.Succeeded ? await _tokenService.CreateTokenAsync(user) : null;
        }

        public Task<SignInResult> LoginUiAsync(string username, string password)
        {
            return _signInManager.PasswordSignInAsync(username, password, isPersistent: true, lockoutOnFailure: true);
        }

        public Task LogoutUiAsync()
        {
            return _signInManager.SignOutAsync();
        }

        public Task<IdentityResult> DeleteUserAsync(AppUser user)
        {
            return _userManager.DeleteAsync(user);
        }
    }
}
