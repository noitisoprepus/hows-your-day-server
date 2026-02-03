using HowsYourDayApi.DTOs.Authentication;
using HowsYourDayApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HowsYourDayApi.Controllers
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

            var refreshTokenDTO = new TokenDTO(string.Empty, refreshToken);
            var newTokenDTO = await _tokenService.RefreshToken(refreshTokenDTO);
            _tokenService.StoreTokensToCookie(newTokenDTO, HttpContext);
            
            return Ok();
        }
    }
}