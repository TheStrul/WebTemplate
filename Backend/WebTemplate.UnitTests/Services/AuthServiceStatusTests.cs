namespace WebTemplate.UnitTests.Services
{
    using System.Security.Claims;
    using FluentAssertions;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using Moq;
    using WebTemplate.Core.Configuration;
    using WebTemplate.Core.Entities;
    using WebTemplate.Core.Interfaces;
    using WebTemplate.Core.Services;
    using WebTemplate.UnitTests.Builders;
    using WebTemplate.UnitTests.Fixtures;
    using Xunit;

    /// <summary>
    /// Unit tests for AuthService - Authentication status and token validation
    /// </summary>
    public class AuthServiceStatusTests : BaseTestFixture
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IUserTypeRepository> _userTypeRepoMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly Mock<ICoreConfiguration> _configMock;
        private readonly AuthService _authService;
        private readonly JwtSettings _jwtSettings;

        public AuthServiceStatusTests()
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

            _jwtSettings = new JwtSettings
            {
                SecretKey = "test-secret-key-for-unit-tests-at-least-32-chars",
                Issuer = "Test",
                Audience = "Test",
                AccessTokenExpiryMinutes = 15,
                RefreshTokenExpiryDays = 7
            };

            _configMock = new Mock<ICoreConfiguration>();
            _configMock.Setup(c => c.Auth).Returns(new AuthSettings { User = new UserSettings { RequireConfirmedEmail = false } });
            _configMock.Setup(c => c.Jwt).Returns(_jwtSettings);
            _configMock.Setup(c => c.UserModuleFeatures).Returns(new UserModuleFeatures { IncludeUserTypePermissionsInResponses = false });
            _configMock.Setup(c => c.AppUrls).Returns(new AppUrls { FrontendBaseUrl = "http://localhost:3000" });

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

        #region GetAuthStatus Tests

        [Fact]
        public async Task GetAuthStatusAsync_WithValidUser_ReturnsUserDetails()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUserBuilder()
                .WithId(userId)
                .WithEmail("user@example.com")
                .WithFirstName("John")
                .WithLastName("Doe")
                .Build();

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });
            _userManagerMock.Setup(x => x.GetTwoFactorEnabledAsync(user))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.GetAuthStatusAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.IsAuthenticated.Should().BeTrue();
            result.Data.User.Should().NotBeNull();
            result.Data.User!.Id.Should().Be(userId);
            result.Data.User.Email.Should().Be(user.Email);
            result.Data.User.FirstName.Should().Be(user.FirstName);
            result.Data.User.LastName.Should().Be(user.LastName);
            result.Data.EmailConfirmed.Should().BeTrue();
            result.Data.RequiresTwoFactor.Should().BeFalse();
        }

        [Fact]
        public async Task GetAuthStatusAsync_WithNonExistentUser_ReturnsUnauthenticated()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authService.GetAuthStatusAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("User not authenticated");
            result.Data.Should().NotBeNull();
            result.Data!.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task GetAuthStatusAsync_WithUnconfirmedEmail_ReturnsStatusWithEmailConfirmedFalse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUserBuilder()
                .WithId(userId)
                .AsUnconfirmed()
                .Build();

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());
            _userManagerMock.Setup(x => x.GetTwoFactorEnabledAsync(user))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.GetAuthStatusAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.IsAuthenticated.Should().BeTrue();
            result.Data.EmailConfirmed.Should().BeFalse();
        }

        [Fact]
        public async Task GetAuthStatusAsync_WithInactiveUser_StillReturnsStatus()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUserBuilder()
                .WithId(userId)
                .AsInactive()
                .Build();

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());
            _userManagerMock.Setup(x => x.GetTwoFactorEnabledAsync(user))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.GetAuthStatusAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.IsAuthenticated.Should().BeTrue();
            result.Data.User!.IsActive.Should().BeFalse();
        }

        #endregion

        #region ValidateToken Tests

        [Fact]
        public async Task ValidateTokenAsync_WithValidToken_ReturnsClaimsDictionary()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "user@example.com";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("nameid", userId),
                new Claim("email", email)
            }));

            _tokenServiceMock.Setup(x => x.ValidateAccessTokenAsync("valid-token"))
                .ReturnsAsync(claimsPrincipal);

            // Act
            var result = await _authService.ValidateTokenAsync("valid-token");

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Token is valid");
            result.Data.Should().NotBeNull();
            result.Data.Should().ContainKey("nameid");
            result.Data.Should().ContainKey("email");
        }

        [Fact]
        public async Task ValidateTokenAsync_WithInvalidToken_ReturnsFail()
        {
            // Arrange
            _tokenServiceMock.Setup(x => x.ValidateAccessTokenAsync("invalid-token"))
                .ReturnsAsync((ClaimsPrincipal?)null);

            // Act
            var result = await _authService.ValidateTokenAsync("invalid-token");

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid token");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithExpiredToken_ReturnsFail()
        {
            // Arrange
            _tokenServiceMock.Setup(x => x.ValidateAccessTokenAsync("expired-token"))
                .ReturnsAsync((ClaimsPrincipal?)null);

            // Act
            var result = await _authService.ValidateTokenAsync("expired-token");

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid token");
        }

        [Fact]
        public async Task ValidateTokenAsync_WithMalformedToken_ReturnsError()
        {
            // Arrange
            _tokenServiceMock.Setup(x => x.ValidateAccessTokenAsync("malformed"))
                .ThrowsAsync(new ArgumentException("Invalid JWT"));

            // Act
            var result = await _authService.ValidateTokenAsync("malformed");

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("An error occurred during token validation");
        }

        #endregion

        #region GetAuthStatus with Permissions Tests

        [Fact]
        public async Task GetAuthStatusAsync_WithPermissionsEnabled_UserTypeFound_IncludesFullUserType()
        {
            // Arrange - Create AuthService with permissions enabled
            var configMock = new Mock<ICoreConfiguration>();
            configMock.Setup(c => c.Auth).Returns(new AuthSettings { User = new UserSettings { RequireConfirmedEmail = false } });
            configMock.Setup(c => c.Jwt).Returns(new JwtSettings
            {
                SecretKey = "test-secret-key-for-unit-tests-at-least-32-chars",
                Issuer = "Test",
                Audience = "Test",
                AccessTokenExpiryMinutes = 15,
                RefreshTokenExpiryDays = 7
            });
            configMock.Setup(c => c.UserModuleFeatures).Returns(new UserModuleFeatures { IncludeUserTypePermissionsInResponses = true });
            configMock.Setup(c => c.AppUrls).Returns(new AppUrls { FrontendBaseUrl = "http://localhost:3000" });

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
            var userTypeId = 4;
            var user = new ApplicationUserBuilder()
                .WithId(userId)
                .WithEmail("test@example.com")
                .WithUserTypeId(userTypeId)
                .Build();

            var userType = new UserType
            {
                Id = userTypeId,
                Name = "Moderator",
                Description = "Moderator role",
                Permissions = "[\"read\",\"write\",\"moderate\"]"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Moderator" });
            _userTypeRepoMock.Setup(x => x.GetByIdAsync(userTypeId))
                .ReturnsAsync(userType);  // UserType found

            // Act
            var result = await authService.GetAuthStatusAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.User!.UserType.Should().NotBeNull();
            result.Data.User.UserType.Name.Should().Be("Moderator");
            result.Data.User.UserType.Permissions.Should().HaveCount(3);
            result.Data.User.UserType.Permissions.Should().Contain(new[] { "read", "write", "moderate" });
        }

        #endregion
    }
}
