using HowsYourDay.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace HowsYourDay.Server.Controllers
{
    [ApiController]
    [Route("token")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (HttpContext.Request.Cookies.TryGetValue("accessToken", out var accessToken))
                return Ok();

            if (!HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
                return Unauthorized("No user logged in. Refresh token unavailable.");

            var tokenResult = await _tokenService.RefreshToken(refreshToken);
            
            _tokenService.StoreTokensToCookie(tokenResult.AccessToken, tokenResult.RefreshToken, HttpContext);
            
            return Ok();
        }
    }
}