using HowsYourDay.Server.DTOs.Authentication;
using HowsYourDay.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;

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
        [EnableRateLimiting("Auth")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Refresh([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RefreshTokenDto? request)
        {
            var tokens = await _tokenService.RefreshTokenAsync(request?.RefreshToken);
            return tokens is null ? Unauthorized() : Ok(tokens);
        }
    }
}
