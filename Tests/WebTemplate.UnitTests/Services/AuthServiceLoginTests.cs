using Microsoft.Extensions.Options;
namespace WebTemplate.UnitTests.Services
{
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
    /// Comprehensive unit tests for AuthService - Login functionality
    /// </summary>
    public class AuthServiceLoginTests : BaseTestFixture
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

        public AuthServiceLoginTests()
        {
            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Setup SignInManager mock
            var contextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                contextAccessorMock.Object,
                claimsPrincipalFactoryMock.Object,
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

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Login_WithValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var loginDto = new LoginDtoBuilder()
                .WithEmail("test@example.com")
                .WithPassword("Test123!")
                .Build();

            var user = new ApplicationUserBuilder()
                .WithEmail(loginDto.Email)
                .WithIsActive(true)
                .WithEmailConfirmed(true)
                .Build();

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });
            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);
            _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                null))
                .ReturnsAsync("valid-access-token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(
                It.IsAny<string>(),
                null))
                .ReturnsAsync("valid-refresh-token");

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.AccessToken.Should().Be("valid-access-token");
            result.Data.RefreshToken.Should().Be("valid-refresh-token");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Login_WithInvalidCredentials_ReturnsFail()
        {
            // Arrange
            var loginDto = new LoginDtoBuilder()
                .WithEmail("test@example.com")
                .WithPassword("WrongPassword123!")
                .Build();

            var user = new ApplicationUserBuilder()
                .WithEmail(loginDto.Email)
                .Build();

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid");
            result.Data.Should().BeNull();

            _tokenServiceMock.Verify(
                x => x.GenerateAccessTokenAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    null),
                Times.Never);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Login_WithNonExistentUser_ReturnsFail()
        {
            // Arrange
            var loginDto = new LoginDtoBuilder()
                .WithEmail("nonexistent@example.com")
                .WithPassword("Test123!")
                .Build();

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid");
            result.Data.Should().BeNull();

            _signInManagerMock.Verify(
                x => x.CheckPasswordSignInAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()),
                Times.Never);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Login_WithInactiveUser_ReturnsFail()
        {
            // Arrange
            var loginDto = new LoginDtoBuilder().Build();
            var user = new ApplicationUserBuilder()
                .WithEmail(loginDto.Email)
                .AsInactive()
                .Build();

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid");
            result.Data.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Login_WithPermissionsEnabled_IncludesUserTypePermissions()
        {
            // Arrange - Create AuthService with permissions enabled
            var configMock = new Mock<ICoreConfiguration>();
            var localAuthSettings = new AuthSettings
            {
                User = new UserSettings { RequireConfirmedEmail = false },
                Jwt = new JwtSettings { SecretKey = "test-key", Issuer = "test", Audience = "test", AccessTokenExpiryMinutes = 15, RefreshTokenExpiryDays = 7 },
                AppUrls = new AppUrls { FrontendBaseUrl = "http://localhost:3000" },
                UserModuleFeatures = new UserModuleFeatures { IncludeUserTypePermissionsInResponses = true },
                Password = new PasswordSettings(),
                Lockout = new LockoutSettings { DefaultLockoutEnabled = true, MaxFailedAccessAttempts = 5, DefaultLockoutTimeSpanMinutes = 15 },
                EmailConfirmation = new EmailConfirmationSettings { TokenExpiryHours = 24 }
            };
            configMock.Setup(c => c.Auth).Returns(localAuthSettings);

            var authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                configMock.Object,
                _userTypeRepoMock.Object,
                _emailSenderMock.Object
            );

            var loginDto = new LoginDtoBuilder().Build();
            var user = new ApplicationUserBuilder()
                .WithEmail(loginDto.Email)
                .Build();

            var userType = new UserType
            {
                Id = 2,
                Name = "User",
                Description = "Standard User",
                IsActive = true,
                Permissions = "[\"read:posts\", \"write:comments\"]"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });
            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);
            _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                null))
                .ReturnsAsync("valid-access-token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(
                It.IsAny<string>(),
                null))
                .ReturnsAsync("valid-refresh-token");
            _userTypeRepoMock.Setup(x => x.GetByIdAsync(user.UserTypeId))
                .ReturnsAsync(userType);

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.User.UserType.Permissions.Should().HaveCount(2);
            result.Data.User.UserType.Permissions.Should().Contain("read:posts");
            result.Data.User.UserType.Permissions.Should().Contain("write:comments");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Login_WithPermissionsEnabled_InvalidJson_ReturnsEmptyPermissions()
        {
            // Arrange - Create AuthService with permissions enabled
            var configMock = new Mock<ICoreConfiguration>();
            var localAuthSettings = new AuthSettings
            {
                User = new UserSettings { RequireConfirmedEmail = false },
                Jwt = new JwtSettings { SecretKey = "test-key", Issuer = "test", Audience = "test", AccessTokenExpiryMinutes = 15, RefreshTokenExpiryDays = 7 },
                AppUrls = new AppUrls { FrontendBaseUrl = "http://localhost:3000" },
                UserModuleFeatures = new UserModuleFeatures { IncludeUserTypePermissionsInResponses = true },
                Password = new PasswordSettings(),
                Lockout = new LockoutSettings { DefaultLockoutEnabled = true, MaxFailedAccessAttempts = 5, DefaultLockoutTimeSpanMinutes = 15 },
                EmailConfirmation = new EmailConfirmationSettings { TokenExpiryHours = 24 }
            };
            configMock.Setup(c => c.Auth).Returns(localAuthSettings);

            var authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                configMock.Object,
                _userTypeRepoMock.Object,
                _emailSenderMock.Object
            );

            var loginDto = new LoginDtoBuilder().Build();
            var user = new ApplicationUserBuilder()
                .WithEmail(loginDto.Email)
                .Build();

            var userType = new UserType
            {
                Id = 2,
                Name = "User",
                Permissions = "invalid-json{not-array}"  // Invalid JSON
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());
            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);
            _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                null))
                .ReturnsAsync("token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(
                It.IsAny<string>(),
                null))
                .ReturnsAsync("refresh");
            _userTypeRepoMock.Setup(x => x.GetByIdAsync(user.UserTypeId))
                .ReturnsAsync(userType);

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.User.UserType.Permissions.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Login_WithDeletedUser_ReturnsFail()
        {
            // Arrange
            var loginDto = new LoginDtoBuilder().Build();
            var user = new ApplicationUserBuilder()
                .WithEmail(loginDto.Email)
                .Build();
            user.IsDeleted = true;  // Mark user as deleted

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid");
            result.Data.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Login_WithLockedAccount_ReturnsLockedMessage()
        {
            // Arrange
            var loginDto = new LoginDtoBuilder().Build();
            var user = new ApplicationUserBuilder()
                .WithEmail(loginDto.Email)
                .Build();

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, true))
                .ReturnsAsync(SignInResult.LockedOut);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Account is locked");
            result.Data.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Login_WithPermissionsEnabled_UserTypeNull_IncludesUserId()
        {
            // Arrange - Permissions enabled but UserType not found
            var configMock = new Mock<ICoreConfiguration>();
            var localAuthSettings = new AuthSettings
            {
                User = new UserSettings { RequireConfirmedEmail = false },
                Jwt = new JwtSettings { SecretKey = "test-key", Issuer = "test", Audience = "test", AccessTokenExpiryMinutes = 15, RefreshTokenExpiryDays = 7 },
                AppUrls = new AppUrls { FrontendBaseUrl = "http://localhost:3000" },
                UserModuleFeatures = new UserModuleFeatures { IncludeUserTypePermissionsInResponses = true },
                Password = new PasswordSettings(),
                Lockout = new LockoutSettings { DefaultLockoutEnabled = true, MaxFailedAccessAttempts = 5, DefaultLockoutTimeSpanMinutes = 15 },
                EmailConfirmation = new EmailConfirmationSettings { TokenExpiryHours = 24 }
            };
            configMock.Setup(c => c.Auth).Returns(localAuthSettings);

            var authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                configMock.Object,
                _userTypeRepoMock.Object,
                _emailSenderMock.Object
            );

            var loginDto = new LoginDtoBuilder().Build();
            var user = new ApplicationUserBuilder()
                .WithEmail(loginDto.Email)
                .Build();

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, true))
                .ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());
            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);
            _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                null))
                .ReturnsAsync("token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(
                It.IsAny<string>(),
                null))
                .ReturnsAsync("refresh");
            _userTypeRepoMock.Setup(x => x.GetByIdAsync(user.UserTypeId))
                .ReturnsAsync((UserType?)null);  // UserType not found

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.User.UserType.Should().NotBeNull();
            result.Data.User.UserType.Id.Should().Be(user.UserTypeId);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Login_WithPermissionsEnabled_UserTypeFound_IncludesFullUserType()
        {
            // Arrange - Permissions enabled and UserType IS found
            var configMock = new Mock<ICoreConfiguration>();
            var localAuthSettings = new AuthSettings
            {
                User = new UserSettings { RequireConfirmedEmail = false },
                Jwt = new JwtSettings { SecretKey = "test-key", Issuer = "test", Audience = "test", AccessTokenExpiryMinutes = 15, RefreshTokenExpiryDays = 7 },
                AppUrls = new AppUrls { FrontendBaseUrl = "http://localhost" },
                UserModuleFeatures = new UserModuleFeatures { IncludeUserTypePermissionsInResponses = true },
                Password = new PasswordSettings(),
                Lockout = new LockoutSettings { DefaultLockoutEnabled = true, MaxFailedAccessAttempts = 5, DefaultLockoutTimeSpanMinutes = 15 },
                EmailConfirmation = new EmailConfirmationSettings { TokenExpiryHours = 24 }
            };
            configMock.Setup(c => c.Auth).Returns(localAuthSettings);

            var authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                configMock.Object,
                _userTypeRepoMock.Object,
                _emailSenderMock.Object
            );

            var loginDto = new LoginDtoBuilder()
                .WithEmail("test@example.com")
                .WithPassword("Password123!")
                .Build();

            var userTypeId = 2;
            var user = new ApplicationUserBuilder()
                .WithEmail(loginDto.Email)
                .WithUserTypeId(userTypeId)
                .Build();

            var userType = new UserType
            {
                Id = userTypeId,
                Name = "Premium",
                Description = "Premium user",
                Permissions = "[\"read\",\"write\",\"delete\"]"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "User" });
            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);
            _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                null))
                .ReturnsAsync("access");
            _tokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(
                It.IsAny<string>(),
                null))
                .ReturnsAsync("refresh");
            _userTypeRepoMock.Setup(x => x.GetByIdAsync(userTypeId))
                .ReturnsAsync(userType);  // UserType found

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.User.UserType.Should().NotBeNull();
            result.Data.User.UserType.Name.Should().Be("Premium");
            result.Data.User.UserType.Permissions.Should().HaveCount(3);
            result.Data.User.UserType.Permissions.Should().Contain(new[] { "read", "write", "delete" });
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Login_WithNullPermissionsJson_ReturnsEmptyList()
        {
            // Arrange - UserType with null permissions JSON
            var configMock = new Mock<ICoreConfiguration>();
            var localAuthSettings = new AuthSettings
            {
                User = new UserSettings { RequireConfirmedEmail = false },
                Jwt = new JwtSettings { SecretKey = "test-key", Issuer = "test", Audience = "test", AccessTokenExpiryMinutes = 15, RefreshTokenExpiryDays = 7 },
                AppUrls = new AppUrls { FrontendBaseUrl = "http://localhost" },
                UserModuleFeatures = new UserModuleFeatures { IncludeUserTypePermissionsInResponses = true },
                Password = new PasswordSettings(),
                Lockout = new LockoutSettings { DefaultLockoutEnabled = true, MaxFailedAccessAttempts = 5, DefaultLockoutTimeSpanMinutes = 15 },
                EmailConfirmation = new EmailConfirmationSettings { TokenExpiryHours = 24 }
            };
            configMock.Setup(c => c.Auth).Returns(localAuthSettings);

            var authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                configMock.Object,
                _userTypeRepoMock.Object,
                _emailSenderMock.Object
            );

            var loginDto = new LoginDtoBuilder()
                .WithEmail("test@example.com")
                .WithPassword("Password123!")
                .Build();

            var userTypeId = 2;
            var user = new ApplicationUserBuilder()
                .WithEmail(loginDto.Email)
                .WithUserTypeId(userTypeId)
                .Build();

            var userType = new UserType
            {
                Id = userTypeId,
                Name = "Basic",
                Description = "Basic user",
                Permissions = null  // Null permissions
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "User" });
            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);
            _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                null))
                .ReturnsAsync("access");
            _tokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(
                It.IsAny<string>(),
                null))
                .ReturnsAsync("refresh");
            _userTypeRepoMock.Setup(x => x.GetByIdAsync(userTypeId))
                .ReturnsAsync(userType);

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.User.UserType.Should().NotBeNull();
            result.Data.User.UserType.Permissions.Should().BeEmpty();
        }
    }
}
