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
    /// Unit tests for AuthService - Password management and email confirmation
    /// </summary>
    public class AuthServicePasswordTests : BaseTestFixture
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

        public AuthServicePasswordTests()
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
                Password = new PasswordSettings()
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

        #region ForgotPassword Tests

        [Fact]
        public async Task ForgotPasswordAsync_WithValidEmail_SendsEmail()
        {
            // Arrange
            var user = new ApplicationUserBuilder()
                .WithEmail("user@example.com")
                .Build();

            var forgotPasswordDto = new ForgotPasswordDto { Email = user.Email! };

            _userManagerMock.Setup(x => x.FindByEmailAsync(forgotPasswordDto.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");
            _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, default))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("sent");
            result.Data.Should().BeTrue();
            _emailSenderMock.Verify(x => x.SendAsync(
                user.Email!,
                It.Is<string>(s => s.Contains("Password")),
                It.IsAny<string>(),
                null,
                default), Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_WithNonExistentEmail_ReturnsSuccessWithoutRevealingUser()
        {
            // Arrange
            var forgotPasswordDto = new ForgotPasswordDto { Email = "nonexistent@example.com" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(forgotPasswordDto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);

            // Assert
            result.Success.Should().BeTrue("Should not reveal user existence");
            result.Message.Should().Contain("If an account exists");
            result.Data.Should().BeTrue();
            _emailSenderMock.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, default), Times.Never);
        }

        [Fact]
        public async Task ForgotPasswordAsync_EmailSendFailure_StillReturnsSuccess()
        {
            // Arrange
            var user = new ApplicationUserBuilder()
                .WithEmail("user@example.com")
                .Build();

            var forgotPasswordDto = new ForgotPasswordDto { Email = user.Email! };

            _userManagerMock.Setup(x => x.FindByEmailAsync(forgotPasswordDto.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");
            _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, default))
                .ThrowsAsync(new InvalidOperationException("Email service down"));

            // Act
            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);

            // Assert
            result.Success.Should().BeTrue("Should not fail due to email issues");
            result.Data.Should().BeTrue();
        }

        #endregion

        #region ResetPassword Tests

        [Fact]
        public async Task ResetPasswordAsync_WithValidToken_ResetsPassword()
        {
            // Arrange
            var user = new ApplicationUserBuilder()
                .WithEmail("user@example.com")
                .Build();

            var resetPasswordDto = new ResetPasswordDto
            {
                Email = user.Email!,
                Token = "valid-reset-token",
                NewPassword = "NewSecurePassword123!"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(resetPasswordDto.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.ResetPasswordAsync(resetPasswordDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Password has been reset");
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task ResetPasswordAsync_WithInvalidToken_ReturnsFail()
        {
            // Arrange
            var user = new ApplicationUserBuilder()
                .WithEmail("user@example.com")
                .Build();

            var resetPasswordDto = new ResetPasswordDto
            {
                Email = user.Email!,
                Token = "invalid-token",
                NewPassword = "NewPassword123!"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(resetPasswordDto.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

            // Act
            var result = await _authService.ResetPasswordAsync(resetPasswordDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Password reset failed");
            result.Errors.Should().Contain("Invalid token");
            result.Data.Should().BeFalse();
        }

        [Fact]
        public async Task ResetPasswordAsync_WithNonExistentUser_ReturnsSuccessWithoutRevealingUser()
        {
            // Arrange
            var resetPasswordDto = new ResetPasswordDto
            {
                Email = "nonexistent@example.com",
                Token = "any-token",
                NewPassword = "NewPassword123!"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(resetPasswordDto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authService.ResetPasswordAsync(resetPasswordDto);

            // Assert
            result.Success.Should().BeTrue("Should not reveal user existence");
            result.Message.Should().Contain("if the account exists");
            result.Data.Should().BeTrue();
        }

        #endregion

        #region ChangePassword Tests

        [Fact]
        public async Task ChangePasswordAsync_WithCorrectCurrentPassword_ChangesPassword()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUserBuilder()
                .WithId(userId)
                .WithEmail("user@example.com")
                .Build();

            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword123!"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Password changed");
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_ReturnsFail()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUserBuilder()
                .WithId(userId)
                .Build();

            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "WrongPassword",
                NewPassword = "NewPassword123!"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password" }));

            // Act
            var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Change password failed");
            result.Errors.Should().Contain("Incorrect password");
            result.Data.Should().BeFalse();
        }

        [Fact]
        public async Task ChangePasswordAsync_WithNonExistentUser_ReturnsFail()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "Old123!",
                NewPassword = "New123!"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User not found");
            result.Data.Should().BeFalse();
        }

        #endregion

        #region ConfirmEmail Tests

        [Fact]
        public async Task ConfirmEmailAsync_WithValidToken_ConfirmsEmail()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUserBuilder()
                .WithId(userId)
                .AsUnconfirmed()
                .Build();

            var confirmEmailDto = new ConfirmEmailDto
            {
                UserId = userId,
                Token = "valid-confirmation-token"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, confirmEmailDto.Token))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.ConfirmEmailAsync(confirmEmailDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Email confirmed");
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithInvalidToken_ReturnsFail()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUserBuilder()
                .WithId(userId)
                .AsUnconfirmed()
                .Build();

            var confirmEmailDto = new ConfirmEmailDto
            {
                UserId = userId,
                Token = "invalid-token"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, confirmEmailDto.Token))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

            // Act
            var result = await _authService.ConfirmEmailAsync(confirmEmailDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Email confirmation failed");
            result.Errors.Should().Contain("Invalid token");
            result.Data.Should().BeFalse();
        }

        [Fact]
        public async Task ConfirmEmailAsync_AlreadyConfirmed_ReturnsSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUserBuilder()
                .WithId(userId)
                .WithEmailConfirmed(true)
                .Build();

            var confirmEmailDto = new ConfirmEmailDto
            {
                UserId = userId,
                Token = "any-token"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.ConfirmEmailAsync(confirmEmailDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Email already confirmed");
            result.Data.Should().BeTrue();
            _userManagerMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithNonExistentUser_ReturnsFail()
        {
            // Arrange
            var confirmEmailDto = new ConfirmEmailDto
            {
                UserId = Guid.NewGuid().ToString(),
                Token = "token"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(confirmEmailDto.UserId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authService.ConfirmEmailAsync(confirmEmailDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User not found");
            result.Data.Should().BeFalse();
        }

        #endregion
    }
}
