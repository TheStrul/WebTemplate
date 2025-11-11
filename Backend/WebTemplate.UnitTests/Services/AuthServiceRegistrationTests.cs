namespace WebTemplate.UnitTests.Services
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using WebTemplate.Core.Configuration;
    using WebTemplate.Core.Entities;
    using WebTemplate.Core.Interfaces;
    using WebTemplate.Core.Services;
    using WebTemplate.UnitTests.Builders;
    using WebTemplate.UnitTests.Fixtures;
    using Xunit;

    /// <summary>
    /// Comprehensive unit tests for AuthService - Registration functionality
    /// </summary>
    public class AuthServiceRegistrationTests : BaseTestFixture
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IUserTypeRepository> _userTypeRepoMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;
        private readonly AuthSettings _authSettings;
        private readonly JwtSettings _jwtSettings;

        public AuthServiceRegistrationTests()
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

            _authSettings = new AuthSettings
            {
                User = new UserSettings { RequireConfirmedEmail = false },
                EmailConfirmation = new EmailConfirmationSettings { TokenExpiryHours = 24 }
            };

            _jwtSettings = new JwtSettings
            {
                SecretKey = "test-secret-key-for-unit-tests-at-least-32-chars",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                AccessTokenExpiryMinutes = 15,
                RefreshTokenExpiryDays = 7
            };

            _authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                Options.Create(_authSettings),
                Options.Create(_jwtSettings),
                Options.Create(new UserModuleFeatures { IncludeUserTypePermissionsInResponses = false }),
                _userTypeRepoMock.Object,
                _emailSenderMock.Object,
                Options.Create(new AppUrls { FrontendBaseUrl = "http://localhost:3000", ConfirmEmailPath = "/confirm-email" })
            );
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var registerDto = new RegisterDtoBuilder()
                .WithEmail("newuser@example.com")
                .WithPassword("SecurePass123!")
                .WithFirstName("John")
                .WithLastName("Doe")
                .Build();

            _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "User" });
            _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), null))
                .ReturnsAsync("access-token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(It.IsAny<string>(), null))
                .ReturnsAsync("refresh-token");

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Registration successful");
            result.Data.Should().NotBeNull();
            result.Data!.AccessToken.Should().Be("access-token");
            result.Data.RefreshToken.Should().Be("refresh-token");
            result.Data.User.Email.Should().Be(registerDto.Email);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ReturnsFail()
        {
            // Arrange
            var registerDto = new RegisterDtoBuilder()
                .WithEmail("existing@example.com")
                .Build();

            var existingUser = new ApplicationUserBuilder()
                .WithEmail(registerDto.Email)
                .Build();

            _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("already exists");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_WithWeakPassword_ReturnsFail()
        {
            // Arrange
            var registerDto = new RegisterDtoBuilder()
                .WithEmail("newuser@example.com")
                .WithPassword("weak")
                .Build();

            _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Registration failed");
            result.Errors.Should().Contain("Password too weak");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_WithEmailConfirmationRequired_SendsEmail()
        {
            // Arrange
            _authSettings.User.RequireConfirmedEmail = true;

            var registerDto = new RegisterDtoBuilder()
                .WithEmail("newuser@example.com")
                .Build();

            _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("confirmation-token");
            _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, default))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("check your email");
            result.Data.Should().BeNull();
            _emailSenderMock.Verify(x => x.SendAsync(
                registerDto.Email,
                It.Is<string>(s => s.Contains("Confirm")),
                It.Is<string>(s => s.Contains("confirm")),
                null,
                default), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_EmailSendFailure_DoesNotBlockRegistration()
        {
            // Arrange
            _authSettings.User.RequireConfirmedEmail = true;

            var registerDto = new RegisterDtoBuilder()
                .WithEmail("newuser@example.com")
                .Build();

            _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("confirmation-token");
            _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, default))
                .ThrowsAsync(new InvalidOperationException("Email service unavailable"));

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeTrue("Registration should succeed even if email fails");
            result.Message.Should().Contain("check your email");
        }

        [Fact]
        public async Task RegisterAsync_WithoutEmailConfirmation_AutoLogsIn()
        {
            // Arrange
            _authSettings.User.RequireConfirmedEmail = false;

            var registerDto = new RegisterDtoBuilder()
                .WithEmail("newuser@example.com")
                .Build();

            _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "User" });
            _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), null))
                .ReturnsAsync("access-token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(It.IsAny<string>(), null))
                .ReturnsAsync("refresh-token");

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.AccessToken.Should().NotBeNullOrEmpty();
            result.Data.RefreshToken.Should().NotBeNullOrEmpty();
            _emailSenderMock.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, default), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WithEmptyFrontendBaseUrl_BuildsConfirmUrlWithoutBase()
        {
            // Arrange - Create AuthService with empty FrontendBaseUrl
            var authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                Options.Create(new AuthSettings { User = new UserSettings { RequireConfirmedEmail = true } }),
                Options.Create(_jwtSettings),
                Options.Create(new UserModuleFeatures { IncludeUserTypePermissionsInResponses = false }),
                _userTypeRepoMock.Object,
                _emailSenderMock.Object,
                Options.Create(new AppUrls { FrontendBaseUrl = "", ConfirmEmailPath = "/confirm" })
            );

            var registerDto = new RegisterDtoBuilder().Build();

            _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("confirmation-token");
            _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, default))
                .Returns(Task.CompletedTask);

            // Act
            var result = await authService.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("check your email");
            _emailSenderMock.Verify(x => x.SendAsync(
                registerDto.Email,
                It.IsAny<string>(),
                It.Is<string>(body => body.Contains("/confirm")),
                null,
                default), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithPermissionsEnabled_UserTypeNull_ReturnsWithoutUserType()
        {
            // Arrange - Permissions enabled but UserType not found
            var authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                Options.Create(new AuthSettings { User = new UserSettings { RequireConfirmedEmail = false } }),
                Options.Create(_jwtSettings),
                Options.Create(new UserModuleFeatures { IncludeUserTypePermissionsInResponses = true }),
                _userTypeRepoMock.Object,
                _emailSenderMock.Object,
                Options.Create(new AppUrls { FrontendBaseUrl = "http://localhost" })
            );

            var registerDto = new RegisterDtoBuilder().Build();

            _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string>());
            _signInManagerMock.Setup(x => x.SignInAsync(It.IsAny<ApplicationUser>(), false, null))
                .Returns(Task.CompletedTask);
            _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), null))
                .ReturnsAsync("token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(It.IsAny<string>(), null))
                .ReturnsAsync("refresh");
            _userTypeRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((UserType?)null);  // UserType not found

            // Act
            var result = await authService.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.User.UserType.Should().NotBeNull();
            result.Data.User.UserType.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task RegisterAsync_WithCustomConfirmEmailPath_BuildsUrlWithCustomPath()
        {
            // Arrange - Create AuthService with custom ConfirmEmailPath
            var authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                Options.Create(new AuthSettings { User = new UserSettings { RequireConfirmedEmail = true } }),
                Options.Create(_jwtSettings),
                Options.Create(new UserModuleFeatures { IncludeUserTypePermissionsInResponses = false }),
                _userTypeRepoMock.Object,
                _emailSenderMock.Object,
                Options.Create(new AppUrls { FrontendBaseUrl = "http://localhost:3000", ConfirmEmailPath = "/custom/verify" })
            );

            var registerDto = new RegisterDtoBuilder()
                .WithEmail("newuser@example.com")
                .WithPassword("SecurePass123!")
                .WithFirstName("John")
                .WithLastName("Doe")
                .Build();

            string? capturedHtmlMessage = null;
            _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("token123");
            _emailSenderMock.Setup(x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
                .Callback<string, string, string, string?, CancellationToken>((to, subject, htmlBody, plainText, ct) =>
                {
                    capturedHtmlMessage = htmlBody;
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await authService.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeTrue();
            capturedHtmlMessage.Should().NotBeNull();
            capturedHtmlMessage.Should().Contain("http://localhost:3000/custom/verify?userId=");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task RegisterAsync_WithPermissionsEnabled_UserTypeFound_IncludesFullUserType()
        {
            // Arrange - Permissions enabled and UserType IS found
            var authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                Options.Create(new AuthSettings { User = new UserSettings { RequireConfirmedEmail = false } }),
                Options.Create(_jwtSettings),
                Options.Create(new UserModuleFeatures { IncludeUserTypePermissionsInResponses = true }),
                _userTypeRepoMock.Object,
                _emailSenderMock.Object,
                Options.Create(new AppUrls { FrontendBaseUrl = "http://localhost" })
            );

            var registerDto = new RegisterDtoBuilder()
                .WithEmail("newuser@example.com")
                .WithPassword("SecurePass123!")
                .WithFirstName("John")
                .WithLastName("Doe")
                .Build();

            var userType = new UserType
            {
                Id = 1,
                Name = "Standard",
                Description = "Standard user type",
                Permissions = "[\"read\",\"write\"]"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "User" });
            _tokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                null))
                .ReturnsAsync("access");
            _tokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(It.IsAny<string>(), null))
                .ReturnsAsync("refresh");
            _userTypeRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(userType);  // UserType found

            // Act
            var result = await authService.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.User.UserType.Should().NotBeNull();
            result.Data.User.UserType.Name.Should().Be("Standard");
            result.Data.User.UserType.Permissions.Should().HaveCount(2);
            result.Data.User.UserType.Permissions.Should().Contain(new[] { "read", "write" });
        }
    }
}
