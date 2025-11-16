namespace WebTemplate.UnitTests.Services
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using Moq;
    using WebTemplate.Core.Configuration;
    using WebTemplate.Core.DTOs.Auth;
    using WebTemplate.Core.Entities;
    using WebTemplate.Core.Interfaces;
    using WebTemplate.Core.Services;
    using WebTemplate.UnitTests.Builders;
    using WebTemplate.UnitTests.Fixtures;
    using Xunit;

    /// <summary>
    /// Unit tests for AuthService - Token refresh and logout functionality
    /// </summary>
    public class AuthServiceTokenTests : BaseTestFixture
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IUserTypeRepository> _userTypeRepoMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly Mock<ICoreConfiguration> _configMock;
        private readonly AuthSettings _authSettings;
        private readonly EmailSettings _emailSettings;
        private readonly UserModuleFeatures _userModuleFeatures;
        private readonly ResponseMessages _responseMessages;
        private readonly AuthService _authService;

        public AuthServiceTokenTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            var contextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object, contextAccessorMock.Object, claimsPrincipalFactoryMock.Object,
                null, null, null, null);

            _tokenServiceMock = new Mock<ITokenService>();
            _userTypeRepoMock = new Mock<IUserTypeRepository>();
            _emailSenderMock = new Mock<IEmailSender>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            // Create real configuration objects
            _authSettings = new AuthSettings
            {
                Jwt = new JwtSettings
                {
                    SecretKey = "test-secret-key-for-unit-tests-at-least-32-chars",
                    Issuer = "TestIssuer",
                    Audience = "TestAudience",
                    AccessTokenExpiryMinutes = 15,
                    RefreshTokenExpiryDays = 7
                },
                AppUrls = new AppUrls
                {
                    FrontendBaseUrl = "http://localhost:3000"
                },
                User = new UserSettings
                {
                    RequireConfirmedEmail = false
                },
                Password = new PasswordSettings(),
                UserModuleFeatures = new UserModuleFeatures
                {
                    IncludeUserTypePermissionsInResponses = false
                },
                Lockout = new LockoutSettings { DefaultLockoutEnabled = true, MaxFailedAccessAttempts = 5, DefaultLockoutTimeSpanMinutes = 15 },
                EmailConfirmation = new EmailConfirmationSettings { TokenExpiryHours = 24 }
            };

            _emailSettings = new EmailSettings
            {
                From = "test@example.com",
                FromName = "Test"
            };

            _userModuleFeatures = new UserModuleFeatures
            {
                IncludeUserTypePermissionsInResponses = false
            };

            _responseMessages = new ResponseMessages();

            _configMock = new Mock<ICoreConfiguration>();
            _configMock.Setup(c => c.Auth).Returns(_authSettings);
            _configMock.Setup(c => c.Email).Returns(_emailSettings);
            _configMock.Setup(c => c.UserModuleFeatures).Returns(_userModuleFeatures);
            _configMock.Setup(c => c.ResponseMessages).Returns(_responseMessages);

            _authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                _configMock.Object,
                _userTypeRepoMock.Object,
                _emailSenderMock.Object
            );
        }

        #region RefreshToken Tests

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUserBuilder()
                .WithId(userId)
                .WithEmail("user@example.com")
                .WithIsActive(true)
                .Build();

            var refreshTokenDto = new RefreshTokenDto { RefreshToken = "valid-refresh-token" };

            _tokenServiceMock.Setup(x => x.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken))
                .ReturnsAsync(userId);
            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });
            _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), null))
                .ReturnsAsync("new-access-token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(It.IsAny<string>(), null))
                .ReturnsAsync("new-refresh-token");

            // Act
            var result = await _authService.RefreshTokenAsync(refreshTokenDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Token refreshed successfully");
            result.Data.Should().NotBeNull();
            result.Data!.AccessToken.Should().Be("new-access-token");
            result.Data.RefreshToken.Should().Be("new-refresh-token");
        }

        [Fact]
        public async Task RefreshTokenAsync_WithInvalidToken_ReturnsFail()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenDto { RefreshToken = "invalid-token" };

            _tokenServiceMock.Setup(x => x.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _authService.RefreshTokenAsync(refreshTokenDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid refresh token");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_WithInactiveUser_ReturnsFail()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUserBuilder()
                .WithId(userId)
                .AsInactive()
                .Build();

            var refreshTokenDto = new RefreshTokenDto { RefreshToken = "valid-token" };

            _tokenServiceMock.Setup(x => x.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken))
                .ReturnsAsync(userId);
            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.RefreshTokenAsync(refreshTokenDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User not found or inactive");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_WithDeletedUser_ReturnsFail()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUserBuilder()
                .WithId(userId)
                .Build();
            user.IsDeleted = true;

            var refreshTokenDto = new RefreshTokenDto { RefreshToken = "valid-token" };

            _tokenServiceMock.Setup(x => x.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken))
                .ReturnsAsync(userId);
            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.RefreshTokenAsync(refreshTokenDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User not found or inactive");
        }

        #endregion

        #region Logout Tests

        [Fact]
        public async Task LogoutAsync_WithValidToken_SingleDevice_RevokesToken()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var logoutDto = new LogoutDto
            {
                RefreshToken = "valid-refresh-token",
                LogoutFromAllDevices = false
            };

            _tokenServiceMock.Setup(x => x.ValidateRefreshTokenAsync(logoutDto.RefreshToken))
                .ReturnsAsync(userId);
            _tokenServiceMock.Setup(x => x.RevokeRefreshTokenAsync(logoutDto.RefreshToken))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.LogoutAsync(logoutDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Logout successful");
            result.Data.Should().BeTrue();
            _tokenServiceMock.Verify(x => x.RevokeRefreshTokenAsync(logoutDto.RefreshToken), Times.Once);
            _tokenServiceMock.Verify(x => x.RevokeAllRefreshTokensAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LogoutAsync_WithValidToken_AllDevices_RevokesAllTokens()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var logoutDto = new LogoutDto
            {
                RefreshToken = "valid-refresh-token",
                LogoutFromAllDevices = true
            };

            _tokenServiceMock.Setup(x => x.ValidateRefreshTokenAsync(logoutDto.RefreshToken))
                .ReturnsAsync(userId);
            _tokenServiceMock.Setup(x => x.RevokeAllRefreshTokensAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.LogoutAsync(logoutDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Logout successful");
            result.Data.Should().BeTrue();
            _tokenServiceMock.Verify(x => x.RevokeAllRefreshTokensAsync(userId), Times.Once);
            _tokenServiceMock.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LogoutAsync_WithInvalidToken_ReturnsFail()
        {
            // Arrange
            var logoutDto = new LogoutDto
            {
                RefreshToken = "invalid-token",
                LogoutFromAllDevices = false
            };

            _tokenServiceMock.Setup(x => x.ValidateRefreshTokenAsync(logoutDto.RefreshToken))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _authService.LogoutAsync(logoutDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid refresh token");
            result.Data.Should().BeFalse();
        }

        [Fact]
        public async Task LogoutAsync_RevokeFailure_ReturnsFail()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var logoutDto = new LogoutDto
            {
                RefreshToken = "valid-token",
                LogoutFromAllDevices = false
            };

            _tokenServiceMock.Setup(x => x.ValidateRefreshTokenAsync(logoutDto.RefreshToken))
                .ReturnsAsync(userId);
            _tokenServiceMock.Setup(x => x.RevokeRefreshTokenAsync(logoutDto.RefreshToken))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.LogoutAsync(logoutDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Failed to revoke");
            result.Data.Should().BeFalse();
        }

        [Fact]
        public async Task LogoutAsync_WithEmptyToken_ReturnsSuccess()
        {
            // Arrange
            var logoutDto = new LogoutDto
            {
                RefreshToken = string.Empty,
                LogoutFromAllDevices = false
            };

            // Act
            var result = await _authService.LogoutAsync(logoutDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Logout successful");
            _tokenServiceMock.Verify(x => x.ValidateRefreshTokenAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region RevokeAllTokens Tests

        [Fact]
        public async Task RevokeAllTokensAsync_Success_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            _tokenServiceMock.Setup(x => x.RevokeAllRefreshTokensAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.RevokeAllTokensAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("All tokens revoked");
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task RevokeAllTokensAsync_Failure_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            _tokenServiceMock.Setup(x => x.RevokeAllRefreshTokensAsync(userId))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.RevokeAllTokensAsync(userId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Failed to revoke tokens");
            result.Data.Should().BeFalse();
        }

        [Fact]
        public async Task RefreshTokenAsync_WithPermissionsEnabled_UserTypeNull_ReturnsUserIdOnly()
        {
            // Arrange - Permissions enabled but UserType not found
            var configMock = new Mock<ICoreConfiguration>();
            var localAuthSettings = new AuthSettings
            {
                User = new UserSettings { RequireConfirmedEmail = false },
                Jwt = _authSettings.Jwt,
                AppUrls = new AppUrls { FrontendBaseUrl = "http://localhost" },
                UserModuleFeatures = new UserModuleFeatures { IncludeUserTypePermissionsInResponses = true },
                Password = new PasswordSettings(),
                Lockout = new LockoutSettings { DefaultLockoutEnabled = true, MaxFailedAccessAttempts = 5, DefaultLockoutTimeSpanMinutes = 15 },
                EmailConfirmation = new EmailConfirmationSettings { TokenExpiryHours = 24 }
            };
            configMock.Setup(c => c.Auth).Returns(localAuthSettings);
            configMock.Setup(c => c.UserModuleFeatures).Returns(new UserModuleFeatures { IncludeUserTypePermissionsInResponses = true });

            var authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                configMock.Object,
                _userTypeRepoMock.Object,
                _emailSenderMock.Object
            );

            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUserBuilder()
                .WithId(userId)
                .Build();

            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = "valid-token",
                AccessToken = "old-access-token"
            };

            _tokenServiceMock.Setup(x => x.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken))
                .ReturnsAsync(userId);
            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());
            _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(userId, user.Email!, It.IsAny<IEnumerable<string>>(), null))
                .ReturnsAsync("new-access-token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(userId, null))
                .ReturnsAsync("new-refresh-token");
            _userTypeRepoMock.Setup(x => x.GetByIdAsync(user.UserTypeId))
                .ReturnsAsync((UserType?)null);  // UserType not found

            // Act
            var result = await authService.RefreshTokenAsync(refreshTokenDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.User.UserType.Should().NotBeNull();
            result.Data.User.UserType.Id.Should().Be(user.UserTypeId);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task RefreshTokenAsync_WithPermissionsEnabled_UserTypeFound_IncludesFullUserType()
        {
            // Arrange - Permissions enabled and UserType IS found
            var configMock = new Mock<ICoreConfiguration>();
            var localAuthSettings = new AuthSettings
            {
                User = new UserSettings { RequireConfirmedEmail = false },
                Jwt = _authSettings.Jwt,
                AppUrls = new AppUrls { FrontendBaseUrl = "http://localhost" },
                UserModuleFeatures = new UserModuleFeatures { IncludeUserTypePermissionsInResponses = true },
                Password = new PasswordSettings(),
                Lockout = new LockoutSettings { DefaultLockoutEnabled = true, MaxFailedAccessAttempts = 5, DefaultLockoutTimeSpanMinutes = 15 },
                EmailConfirmation = new EmailConfirmationSettings { TokenExpiryHours = 24 }
            };
            configMock.Setup(c => c.Auth).Returns(localAuthSettings);
            configMock.Setup(c => c.UserModuleFeatures).Returns(new UserModuleFeatures { IncludeUserTypePermissionsInResponses = true });

            var authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                configMock.Object,
                _userTypeRepoMock.Object,
                _emailSenderMock.Object
            );

            var userId = Guid.NewGuid().ToString();
            var userTypeId = 3;
            var user = new ApplicationUserBuilder()
                .WithId(userId)
                .WithUserTypeId(userTypeId)
                .Build();

            var userType = new UserType
            {
                Id = userTypeId,
                Name = "Admin",
                Description = "Administrator",
                Permissions = "[\"read\",\"write\",\"delete\",\"admin\"]"
            };

            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = "valid-token",
                AccessToken = "old-access-token"
            };

            _tokenServiceMock.Setup(x => x.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken))
                .ReturnsAsync(userId);
            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });
            _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(userId, user.Email!, It.IsAny<IEnumerable<string>>(), null))
                .ReturnsAsync("new-access-token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(userId, null))
                .ReturnsAsync("new-refresh-token");
            _userTypeRepoMock.Setup(x => x.GetByIdAsync(userTypeId))
                .ReturnsAsync(userType);  // UserType found

            // Act
            var result = await authService.RefreshTokenAsync(refreshTokenDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.User.UserType.Should().NotBeNull();
            result.Data.User.UserType.Name.Should().Be("Admin");
            result.Data.User.UserType.Permissions.Should().HaveCount(4);
            result.Data.User.UserType.Permissions.Should().Contain(new[] { "read", "write", "delete", "admin" });
        }

        #endregion
    }
}
