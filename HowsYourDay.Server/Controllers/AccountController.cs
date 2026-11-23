using HowsYourDay.Server.DTOs.Authentication;
using HowsYourDay.Server.Extensions;
using HowsYourDay.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HowsYourDay.Server.Controllers
{
    [ApiController]
    [Route("account")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IDayEntryService _dayEntryService;
        private readonly ITokenService _tokenService;

        public AccountController(IAccountService accountService, IDayEntryService dayEntryService, ITokenService tokenService)
        {
            _accountService = accountService;
            _dayEntryService = dayEntryService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var result = await _accountService.RegisterAsync(registerDto.UserName, registerDto.Password);

            if (result.Succeeded)
                return Ok();
            else
                return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var tokens = await _accountService.LoginApiAsync(loginDto.UserName, loginDto.Password);
            return tokens is null ? Unauthorized("Invalid username or password.") : Ok(tokens);
        }

        [HttpPost("logout")]
        [Authorize(Policy = "Api")]
        public async Task<IActionResult> Logout()
        {
            await _tokenService.RevokeAsync(User.GetUserId());
            return Ok();
        }

        [HttpDelete("data")]
        [Authorize(Policy = "Api")]
        public async Task<IActionResult> Delete()
        {
            var userId = User.GetUserId();
            
            // Get current user
            var user = await _accountService.GetUserAsync(userId);

            if (user == null)
                return NotFound();
            
            // Delete user
            await _accountService.DeleteUserAsync(user);
                
            // Delete all day entries of user
            await _dayEntryService.DeleteDayEntriesOfUserAsync(userId);
            
            return Ok();
        }
    }
}