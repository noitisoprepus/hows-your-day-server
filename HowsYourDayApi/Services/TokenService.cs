using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HowsYourDayApi.DTOs.Authentication;
using HowsYourDayApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HowsYourDayApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<AppUser> _userManager;

        public TokenService(UserManager<AppUser> userManager)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")));
            _userManager = userManager;
        }

        // Access Token
        public async Task<TokenDTO> CreateToken(AppUser user, bool populateExpiry)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = creds,
                Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
                Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            if (populateExpiry)
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var accessToken = tokenHandler.WriteToken(token);
            
            return new TokenDTO(accessToken, refreshToken);
        }

        public async Task<TokenDTO> RefreshToken(TokenDTO tokenDTO)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshToken == tokenDTO.RefreshToken);
            if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
                throw new Exception("Refresh token invalid. Please login again.");

            return await CreateToken(user, true);
        }

        private string GenerateRefreshToken()
        {
            var randomNum = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNum);
                return Convert.ToBase64String(randomNum);
            }
        }

        public void StoreTokensToCookie(TokenDTO tokenDTO, HttpContext context)
        {
            context.Response.Cookies.Append("accessToken", tokenDTO.AccessToken,
                new CookieOptions
            {
                Path = "/",
                Domain = "localhost",
                Expires = DateTime.UtcNow.AddMinutes(5),
                HttpOnly = true,
                IsEssential = true,
                Secure =  true,
                SameSite = SameSiteMode.None
            });
            
            context.Response.Cookies.Append("refreshToken", tokenDTO.RefreshToken,
                new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(7),
                HttpOnly = true,
                IsEssential = true,
                Secure =  true,
                SameSite = SameSiteMode.None
            });
        }

        public void ClearTokenCookie(HttpContext context)
        {
            context.Response.Cookies.Delete("accessToken",
                new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Secure =  true,
                SameSite = SameSiteMode.None
            });

            context.Response.Cookies.Delete("refreshToken",
                new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Secure =  true,
                SameSite = SameSiteMode.None
            });
        }
    }
}