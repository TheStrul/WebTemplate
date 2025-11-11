namespace WebTemplate.UnitTests.Services
{
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using WebTemplate.Core.Configuration;
    using WebTemplate.Core.Entities;
    using WebTemplate.Core.Interfaces;
    using WebTemplate.Core.Services;
    using WebTemplate.UnitTests.Fixtures;
    using Xunit;

    public class TokenServiceTests : BaseTestFixture
    {
        private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
        private readonly JwtSettings _jwtSettings;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();

            // Valid JWT settings for testing
            _jwtSettings = new JwtSettings
            {
                SecretKey = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly123456789!",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                AccessTokenExpiryMinutes = 30,
                RefreshTokenExpiryDays = 7,
                MaxRefreshTokensPerUser = 5,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkewMinutes = 5
            };

            var jwtOptions = Options.Create(_jwtSettings);

            _tokenService = new TokenService(jwtOptions, _mockRefreshTokenRepository.Object);
        }

        #region GenerateAccessTokenAsync Tests

        [Fact]
        public async Task GenerateAccessTokenAsync_WithValidData_ReturnsValidJwtToken()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = RandomEmail();
            var roles = new List<string> { "User" };

            // Act
            var token = await _tokenService.GenerateAccessTokenAsync(userId, email, roles);

            // Assert
            token.Should().NotBeNullOrEmpty();
            var handler = new JwtSecurityTokenHandler();
            var canRead = handler.CanReadToken(token);
            canRead.Should().BeTrue();
        }

        [Fact]
        public async Task GenerateAccessTokenAsync_WithMultipleRoles_IncludesAllRoleClaims()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = RandomEmail();
            var roles = new List<string> { "User", "Admin", "Moderator" };

            // Act
            var token = await _tokenService.GenerateAccessTokenAsync(userId, email, roles);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            // JWT uses short claim types like "role" instead of full URI
            var roleClaims = jwtToken.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();

            roleClaims.Should().HaveCount(3);
            roleClaims.Should().Contain(roles);
        }

        [Fact]
        public async Task GenerateAccessTokenAsync_WithAdditionalClaims_IncludesCustomClaims()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = RandomEmail();
            var roles = new List<string> { "User" };
            var additionalClaims = new Dictionary<string, object>
            {
                { "CustomClaim1", "Value1" },
                { "CustomClaim2", 12345 }
            };

            // Act
            var token = await _tokenService.GenerateAccessTokenAsync(userId, email, roles, additionalClaims);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            jwtToken.Claims.Should().Contain(c => c.Type == "CustomClaim1" && c.Value == "Value1");
            jwtToken.Claims.Should().Contain(c => c.Type == "CustomClaim2" && c.Value == "12345");
        }

        [Fact]
        public async Task GenerateAccessTokenAsync_ContainsRequiredStandardClaims()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = RandomEmail();
            var roles = new List<string> { "User" };

            // Act
            var token = await _tokenService.GenerateAccessTokenAsync(userId, email, roles);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // JWT tokens use short claim type names in the actual JWT payload
            jwtToken.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == userId);
            jwtToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == email);
            jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId);
            jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
            jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Iat);
        }

        [Fact]
        public async Task GenerateAccessTokenAsync_TokenHasCorrectExpiration()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = RandomEmail();
            var roles = new List<string> { "User" };
            var beforeGeneration = DateTime.UtcNow;

            // Act
            var token = await _tokenService.GenerateAccessTokenAsync(userId, email, roles);

            // Assert
            var expiration = _tokenService.GetTokenExpiration(token);
            expiration.Should().NotBeNull();
            expiration.Should().BeCloseTo(
                beforeGeneration.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                TimeSpan.FromSeconds(5)
            );
        }

        #endregion

        #region GenerateRefreshTokenAsync Tests

        [Fact]
        public async Task GenerateRefreshTokenAsync_ReturnsNonEmptyToken()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            _mockRefreshTokenRepository.Setup(r => r.GetTokensByUserIdAsync(userId))
                .ReturnsAsync(new List<RefreshToken>());
            _mockRefreshTokenRepository.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var token = await _tokenService.GenerateRefreshTokenAsync(userId);

            // Assert
            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GenerateRefreshTokenAsync_GeneratesUniqueCryptographicToken()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            _mockRefreshTokenRepository.Setup(r => r.GetTokensByUserIdAsync(userId))
                .ReturnsAsync(new List<RefreshToken>());
            _mockRefreshTokenRepository.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var token1 = await _tokenService.GenerateRefreshTokenAsync(userId);
            var token2 = await _tokenService.GenerateRefreshTokenAsync(userId);

            // Assert
            token1.Should().NotBe(token2, "Each token should be cryptographically unique");
            token1.Length.Should().BeGreaterThan(50, "Token should be long enough for security");
        }

        [Fact]
        public async Task GenerateRefreshTokenAsync_WithDeviceId_IncludesDeviceInfo()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var deviceId = "TestDevice123";
            RefreshToken? capturedToken = null;

            _mockRefreshTokenRepository.Setup(r => r.GetTokensByUserIdAsync(userId))
                .ReturnsAsync(new List<RefreshToken>());
            _mockRefreshTokenRepository.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(token => capturedToken = token)
                .Returns(Task.CompletedTask);

            // Act
            await _tokenService.GenerateRefreshTokenAsync(userId, deviceId);

            // Assert
            capturedToken.Should().NotBeNull();
            capturedToken!.DeviceId.Should().Be(deviceId);
        }

        [Fact]
        public async Task GenerateRefreshTokenAsync_CleansUpExcessTokens_WhenMaxLimitReached()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var existingTokens = Enumerable.Range(0, _jwtSettings.MaxRefreshTokensPerUser)
                .Select(i => new RefreshToken
                {
                    Id = i + 1,
                    Token = $"Token{i}",
                    UserId = userId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow.AddDays(-i) // Oldest first
                })
                .ToList();

            _mockRefreshTokenRepository.Setup(r => r.GetTokensByUserIdAsync(userId))
                .ReturnsAsync(existingTokens);
            _mockRefreshTokenRepository.Setup(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()))
                .Returns(Task.CompletedTask);
            _mockRefreshTokenRepository.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _tokenService.GenerateRefreshTokenAsync(userId);

            // Assert
            _mockRefreshTokenRepository.Verify(
                r => r.DeleteRangeAsync(It.Is<IEnumerable<RefreshToken>>(tokens => tokens.Any())),
                Times.Once,
                "Should delete old tokens when at max limit"
            );
        }

        [Fact]
        public async Task GenerateRefreshTokenAsync_StoresHashedToken_NotPlaintext()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            RefreshToken? capturedToken = null;

            _mockRefreshTokenRepository.Setup(r => r.GetTokensByUserIdAsync(userId))
                .ReturnsAsync(new List<RefreshToken>());
            _mockRefreshTokenRepository.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(token => capturedToken = token)
                .Returns(Task.CompletedTask);

            // Act
            var rawToken = await _tokenService.GenerateRefreshTokenAsync(userId);

            // Assert
            capturedToken.Should().NotBeNull();
            capturedToken!.Token.Should().NotBe(rawToken, "Stored token should be hashed, not plaintext");
        }

        #endregion

        #region ValidateAccessTokenAsync Tests

        [Fact]
        public async Task ValidateAccessTokenAsync_WithValidToken_ReturnsClaimsPrincipal()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = RandomEmail();
            var roles = new List<string> { "User" };
            var token = await _tokenService.GenerateAccessTokenAsync(userId, email, roles);

            // Act
            var principal = await _tokenService.ValidateAccessTokenAsync(token);

            // Assert
            principal.Should().NotBeNull();
            principal!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId);
            principal!.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
        }

        [Fact]
        public async Task ValidateAccessTokenAsync_WithInvalidToken_ReturnsNull()
        {
            // Arrange
            var invalidToken = "this.is.not.a.valid.jwt.token";

            // Act
            var principal = await _tokenService.ValidateAccessTokenAsync(invalidToken);

            // Assert
            principal.Should().BeNull();
        }

        [Fact]
        public async Task ValidateAccessTokenAsync_WithMalformedToken_ReturnsNull()
        {
            // Arrange
            var malformedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.InvalidPayload.InvalidSignature";

            // Act
            var principal = await _tokenService.ValidateAccessTokenAsync(malformedToken);

            // Assert
            principal.Should().BeNull();
        }

        #endregion

        #region ValidateRefreshTokenAsync Tests

        [Fact]
        public async Task ValidateRefreshTokenAsync_WithValidToken_ReturnsUserId()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var rawToken = "RawRefreshToken123";
            var refreshToken = new RefreshToken
            {
                Id = 1,
                Token = "HashedToken",
                UserId = userId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                RevokedAt = null
            };

            _mockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(refreshToken);

            // Act
            var result = await _tokenService.ValidateRefreshTokenAsync(rawToken);

            // Assert
            result.Should().Be(userId);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_WithRevokedToken_ReturnsNull()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var rawToken = "RawRefreshToken123";
            var refreshToken = new RefreshToken
            {
                Id = 1,
                Token = "HashedToken",
                UserId = userId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                RevokedAt = DateTime.UtcNow.AddHours(-1) // Revoked
            };

            _mockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(refreshToken);

            // Act
            var result = await _tokenService.ValidateRefreshTokenAsync(rawToken);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_WithExpiredToken_ReturnsNull()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var rawToken = "RawRefreshToken123";
            var refreshToken = new RefreshToken
            {
                Id = 1,
                Token = "HashedToken",
                UserId = userId,
                ExpiryDate = DateTime.UtcNow.AddDays(-1), // Expired
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                RevokedAt = null
            };

            _mockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(refreshToken);

            // Act
            var result = await _tokenService.ValidateRefreshTokenAsync(rawToken);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_WithNonExistentToken_ReturnsNull()
        {
            // Arrange
            var rawToken = "NonExistentToken123";

            _mockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
                .ReturnsAsync((RefreshToken?)null);

            // Act
            var result = await _tokenService.ValidateRefreshTokenAsync(rawToken);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region RevokeRefreshTokenAsync Tests

        [Fact]
        public async Task RevokeRefreshTokenAsync_WithValidToken_RevokesAndReturnsTrue()
        {
            // Arrange
            var rawToken = "RawRefreshToken123";
            var refreshToken = new RefreshToken
            {
                Id = 1,
                Token = "HashedToken",
                UserId = Guid.NewGuid().ToString(),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                RevokedAt = null
            };

            _mockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(refreshToken);
            _mockRefreshTokenRepository.Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _tokenService.RevokeRefreshTokenAsync(rawToken);

            // Assert
            result.Should().BeTrue();
            refreshToken.RevokedAt.Should().NotBeNull();
            _mockRefreshTokenRepository.Verify(r => r.UpdateAsync(refreshToken), Times.Once);
        }

        [Fact]
        public async Task RevokeRefreshTokenAsync_WithNonExistentToken_ReturnsFalse()
        {
            // Arrange
            var rawToken = "NonExistentToken123";

            _mockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
                .ReturnsAsync((RefreshToken?)null);

            // Act
            var result = await _tokenService.RevokeRefreshTokenAsync(rawToken);

            // Assert
            result.Should().BeFalse();
            _mockRefreshTokenRepository.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        #endregion

        #region RevokeAllRefreshTokensAsync Tests

        [Fact]
        public async Task RevokeAllRefreshTokensAsync_WithMultipleTokens_RevokesAll()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var tokens = new List<RefreshToken>
            {
                new RefreshToken
                {
                    Id = 1,
                    Token = "Token1",
                    UserId = userId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    RevokedAt = null
                },
                new RefreshToken
                {
                    Id = 2,
                    Token = "Token2",
                    UserId = userId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    RevokedAt = null
                }
            };

            _mockRefreshTokenRepository.Setup(r => r.GetActiveTokensByUserIdAsync(userId))
                .ReturnsAsync(tokens);
            _mockRefreshTokenRepository.Setup(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _tokenService.RevokeAllRefreshTokensAsync(userId);

            // Assert
            result.Should().BeTrue();
            tokens.Should().AllSatisfy(t => t.RevokedAt.Should().NotBeNull());
            _mockRefreshTokenRepository.Verify(r => r.UpdateRangeAsync(tokens), Times.Once);
        }

        [Fact]
        public async Task RevokeAllRefreshTokensAsync_WithNoTokens_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            _mockRefreshTokenRepository.Setup(r => r.GetActiveTokensByUserIdAsync(userId))
                .ReturnsAsync(new List<RefreshToken>());
            _mockRefreshTokenRepository.Setup(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _tokenService.RevokeAllRefreshTokensAsync(userId);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region GetTokenExpiration Tests

        [Fact]
        public async Task GetTokenExpiration_WithValidToken_ReturnsExpirationDate()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = RandomEmail();
            var roles = new List<string> { "User" };
            var beforeGeneration = DateTime.UtcNow;
            var token = await _tokenService.GenerateAccessTokenAsync(userId, email, roles);

            // Act
            var expiration = _tokenService.GetTokenExpiration(token);

            // Assert
            expiration.Should().NotBeNull();
            expiration.Should().BeCloseTo(
                beforeGeneration.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                TimeSpan.FromSeconds(5)
            );
        }

        [Fact]
        public void GetTokenExpiration_WithInvalidToken_ReturnsNull()
        {
            // Arrange
            var invalidToken = "not.a.valid.token";

            // Act
            var expiration = _tokenService.GetTokenExpiration(invalidToken);

            // Assert
            expiration.Should().BeNull();
        }

        #endregion

        #region GetUserIdFromToken Tests

        [Fact]
        public async Task GetUserIdFromToken_WithValidToken_ReturnsUserId()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = RandomEmail();
            var roles = new List<string> { "User" };
            var token = await _tokenService.GenerateAccessTokenAsync(userId, email, roles);

            // Act
            var extractedUserId = _tokenService.GetUserIdFromToken(token);

            // Assert
            extractedUserId.Should().Be(userId);
        }

        [Fact]
        public void GetUserIdFromToken_WithInvalidToken_ReturnsNull()
        {
            // Arrange
            var invalidToken = "not.a.valid.token";

            // Act
            var userId = _tokenService.GetUserIdFromToken(invalidToken);

            // Assert
            userId.Should().BeNull();
        }

        #endregion

        #region CleanupExpiredTokensAsync Tests

        [Fact]
        public async Task CleanupExpiredTokensAsync_WithExpiredTokens_DeletesThem()
        {
            // Arrange
            var expiredTokens = new List<RefreshToken>
            {
                new RefreshToken
                {
                    Id = 1,
                    Token = "ExpiredToken1",
                    UserId = Guid.NewGuid().ToString(),
                    ExpiryDate = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new RefreshToken
                {
                    Id = 2,
                    Token = "ExpiredToken2",
                    UserId = Guid.NewGuid().ToString(),
                    ExpiryDate = DateTime.UtcNow.AddDays(-2),
                    CreatedAt = DateTime.UtcNow.AddDays(-9)
                }
            };

            _mockRefreshTokenRepository.Setup(r => r.GetExpiredTokensAsync())
                .ReturnsAsync(expiredTokens);
            _mockRefreshTokenRepository.Setup(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()))
                .Returns(Task.CompletedTask);

            // Act
            var deletedCount = await _tokenService.CleanupExpiredTokensAsync();

            // Assert
            deletedCount.Should().Be(2);
            _mockRefreshTokenRepository.Verify(r => r.DeleteRangeAsync(expiredTokens), Times.Once);
        }

        [Fact]
        public async Task CleanupExpiredTokensAsync_WithNoExpiredTokens_ReturnsZero()
        {
            // Arrange
            _mockRefreshTokenRepository.Setup(r => r.GetExpiredTokensAsync())
                .ReturnsAsync(new List<RefreshToken>());

            // Act
            var deletedCount = await _tokenService.CleanupExpiredTokensAsync();

            // Assert
            deletedCount.Should().Be(0);
            _mockRefreshTokenRepository.Verify(
                r => r.DeleteRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()),
                Times.Never
            );
        }

        #endregion
    }
}
