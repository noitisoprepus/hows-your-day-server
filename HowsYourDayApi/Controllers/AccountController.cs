using HowsYourDayApi.DTOs.Authentication;
using HowsYourDayApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HowsYourDayApi.Controllers
{
    [ApiController]
    [Route("account")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
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
    }
}