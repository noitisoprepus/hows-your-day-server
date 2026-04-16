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

        public AccountController(IAccountService accountService, IDayEntryService dayEntryService)
        {
            _accountService = accountService;
            _dayEntryService = dayEntryService;
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
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var result = await _accountService.LoginAsync(loginDto.UserName, loginDto.Password, HttpContext);

            if (result.Succeeded)
                return Ok();
            else
                return Unauthorized("Username or password not found");
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync(HttpContext);
            return Ok("User logged out successfully");
        }
        
        [HttpDelete("data")]
        [Authorize]
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