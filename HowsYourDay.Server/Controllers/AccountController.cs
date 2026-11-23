using HowsYourDay.Server.DTOs.Authentication;
using HowsYourDay.Server.Extensions;
using HowsYourDay.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HowsYourDay.Server.Controllers
{
    [ApiController]
    [Route("account")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ITokenService _tokenService;

        public AccountController(IAccountService accountService, ITokenService tokenService)
        {
            _accountService = accountService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        [EnableRateLimiting("Auth")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _accountService.RegisterAsync(registerDto.UserName, registerDto.Password);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        [HttpPost("login")]
        [EnableRateLimiting("Auth")]
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
            var user = await _accountService.GetUserAsync(User.GetUserId());
            if (user is null)
                return NotFound();

            var result = await _accountService.DeleteUserAsync(user);
            return result.Succeeded
                ? Ok()
                : Problem(statusCode: StatusCodes.Status409Conflict, title: "The account could not be deleted.");
        }
    }
}
