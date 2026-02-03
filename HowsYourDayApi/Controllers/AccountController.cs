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
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var result = await _accountService.RegisterAsync(registerDTO, HttpContext);

            if (result.Succeeded)
                return Ok();
            else
                return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var result = await _accountService.LoginAsync(login, HttpContext);

            if (result.Succeeded)
                return Ok();
            else
                return Unauthorized("Email or password not found");
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