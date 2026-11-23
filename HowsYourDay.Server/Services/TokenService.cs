using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HowsYourDay.Server.Models;
using HowsYourDay.Server.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HowsYourDay.Server.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly JwtOptions _jwtOptions;
        private readonly UserManager<AppUser> _userManager;
        private readonly TimeProvider _timeProvider;

        public TokenService(UserManager<AppUser> userManager, IOptions<JwtOptions> jwtOptions, TimeProvider timeProvider)
        {
            _jwtOptions = jwtOptions.Value;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            _userManager = userManager;
            _timeProvider = timeProvider;
        }

        public async Task<TokenResult> CreateTokenAsync(AppUser user)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var tokens = CreateTokens(user.Id, now);
            user.RefreshTokenHash = Hash(tokens.RefreshToken);
            user.RefreshTokenExpiry = now.AddDays(7);
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new InvalidOperationException("Could not persist the refresh token.");

            return tokens;
        }

        public async Task<TokenResult?> RefreshTokenAsync(string? refreshToken)
        {
            if (refreshToken is not { Length: 44 })
                return null;

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var currentHash = Hash(refreshToken);
            var validSession = _userManager.Users.Where(user =>
                user.RefreshTokenHash == currentHash && user.RefreshTokenExpiry > now);
            var userId = await validSession.Select(user => (Guid?)user.Id).SingleOrDefaultAsync();
            if (userId is null)
                return null;

            var tokens = CreateTokens(userId.Value, now);
            var nextHash = Hash(tokens.RefreshToken);
            var nextExpiry = now.AddDays(7);
            var concurrencyStamp = Guid.NewGuid().ToString();

            // The old hash must still match when the update acquires the row lock.
            var updated = await validSession.Where(user => user.Id == userId.Value)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(user => user.RefreshTokenHash, nextHash)
                    .SetProperty(user => user.RefreshTokenExpiry, nextExpiry)
                    .SetProperty(user => user.ConcurrencyStamp, concurrencyStamp));

            return updated == 1 ? tokens : null;
        }

        public async Task RevokeAsync(Guid userId)
        {
            var concurrencyStamp = Guid.NewGuid().ToString();

            await _userManager.Users.Where(user => user.Id == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(user => user.RefreshTokenHash, (string?)null)
                    .SetProperty(user => user.RefreshTokenExpiry, DateTime.UnixEpoch)
                    .SetProperty(user => user.ConcurrencyStamp, concurrencyStamp));
        }

        private TokenResult CreateTokens(Guid userId, DateTime now)
        {
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())]),
                Expires = now.AddMinutes(5),
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512),
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience
            };

            var handler = new JwtSecurityTokenHandler();
            var accessToken = handler.WriteToken(handler.CreateToken(descriptor));
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            return new TokenResult(accessToken, refreshToken);
        }

        private static string Hash(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }
    }
}
