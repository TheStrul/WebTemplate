using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebTemplate.Core.Configuration;
using WebTemplate.Core.Entities;
using WebTemplate.Core.Interfaces;

namespace WebTemplate.Core.Services
{
    /// <summary>
    /// JWT Token service implementation
    /// NO hard-coded values - all configuration from settings!
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public TokenService(
            IOptions<JwtSettings> jwtSettings,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _jwtSettings = jwtSettings.Value;
            _refreshTokenRepository = refreshTokenRepository;

            // Create token validation parameters from settings
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = _jwtSettings.ValidateIssuer,
                ValidateAudience = _jwtSettings.ValidateAudience,
                ValidateLifetime = _jwtSettings.ValidateLifetime,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewMinutes),
                RequireExpirationTime = true,
                RequireSignedTokens = true
            };
        }

        public Task<string> GenerateAccessTokenAsync(
            string userId,
            string email,
            IEnumerable<string> roles,
            Dictionary<string, object>? additionalClaims = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Email, email),
                new(JwtRegisteredClaimNames.Sub, userId),
                new(JwtRegisteredClaimNames.Email, email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add role claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Add additional claims if provided
            if (additionalClaims != null)
            {
                foreach (var claim in additionalClaims)
                {
                    claims.Add(new Claim(claim.Key, claim.Value.ToString() ?? string.Empty));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Task.FromResult(tokenHandler.WriteToken(token));
        }

        public async Task<string> GenerateRefreshTokenAsync(string userId, string? deviceId = null)
        {
            // Generate cryptographically secure random token
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var rawToken = Convert.ToBase64String(randomBytes);

            // Hash token for storage
            var hashedToken = HashToken(rawToken);

            // Create refresh token entity (store only hash)
            var refreshToken = new RefreshToken
            {
                Token = hashedToken,
                UserId = userId,
                ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                DeviceId = deviceId,
                CreatedAt = DateTime.UtcNow
            };

            // Clean up old tokens if user has too many
            await CleanupUserTokensAsync(userId);

            // Save refresh token
            await _refreshTokenRepository.AddAsync(refreshToken);

            // Return raw token to client
            return rawToken;
        }

        public Task<ClaimsPrincipal?> ValidateAccessTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                // Validate token
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out SecurityToken validatedToken);

                // Ensure it's a JWT token with correct algorithm
                if (validatedToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return Task.FromResult<ClaimsPrincipal?>(principal);
                }

                return Task.FromResult<ClaimsPrincipal?>(null);
            }
            catch
            {
                return Task.FromResult<ClaimsPrincipal?>(null);
            }
        }        public async Task<string?> ValidateRefreshTokenAsync(string refreshToken)
        {
            var hashed = HashToken(refreshToken);
            var token = await _refreshTokenRepository.GetByTokenAsync(hashed);

            if (token == null || !token.IsActive)
            {
                return null;
            }

            return token.UserId;
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var hashed = HashToken(refreshToken);
            var token = await _refreshTokenRepository.GetByTokenAsync(hashed);

            if (token == null)
            {
                return false;
            }

            token.Revoke();
            await _refreshTokenRepository.UpdateAsync(token);

            return true;
        }

        public async Task<bool> RevokeAllRefreshTokensAsync(string userId)
        {
            var tokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);

            foreach (var token in tokens)
            {
                token.Revoke();
            }

            await _refreshTokenRepository.UpdateRangeAsync(tokens);

            return true;
        }

        public DateTime? GetTokenExpiration(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(token);

                return jwt.ValidTo;
            }
            catch
            {
                return null;
            }
        }

        public string? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(token);

                return jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            }
            catch
            {
                return null;
            }
        }

        public async Task<int> CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _refreshTokenRepository.GetExpiredTokensAsync();

            if (expiredTokens.Any())
            {
                await _refreshTokenRepository.DeleteRangeAsync(expiredTokens);
            }

            return expiredTokens.Count();
        }

        /// <summary>
        /// Clean up excess tokens for a user to maintain the maximum limit
        /// </summary>
        private async Task CleanupUserTokensAsync(string userId)
        {
            var userTokens = await _refreshTokenRepository.GetTokensByUserIdAsync(userId);

            if (userTokens.Count() >= _jwtSettings.MaxRefreshTokensPerUser)
            {
                // Remove oldest tokens, keeping only (max - 1) to make room for the new one
                var tokensToRemove = userTokens
                    .OrderBy(t => t.CreatedAt)
                    .Take(userTokens.Count() - _jwtSettings.MaxRefreshTokensPerUser + 1);

                await _refreshTokenRepository.DeleteRangeAsync(tokensToRemove);
            }
        }

        private static string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
