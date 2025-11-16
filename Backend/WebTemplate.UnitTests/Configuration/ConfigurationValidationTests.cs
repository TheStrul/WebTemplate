using FluentAssertions;
using WebTemplate.Core.Common;
using WebTemplate.Core.Configuration;
using Xunit;

namespace WebTemplate.UnitTests.Configuration;

/// <summary>
/// Unit tests for configuration validation using Result pattern.
/// Tests validation logic for all IBaseConfiguration implementations.
/// </summary>
public class ConfigurationValidationTests
{
    #region JwtSettings Validation Tests

    [Fact]
    public void JwtSettings_Validate_WithValidSettings_ReturnsSuccess()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVerySecureSecretKeyWith256BitsOrMore!@#$%",
            Issuer = "https://api.example.com",
            Audience = "https://app.example.com",
            AccessTokenExpiryMinutes = 15,
            RefreshTokenExpiryDays = 7
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void JwtSettings_Validate_WithMissingSecretKey_ReturnsFailure()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "",
            Issuer = "https://api.example.com",
            Audience = "https://app.example.com",
            AccessTokenExpiryMinutes = 15,
            RefreshTokenExpiryDays = 7
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "CONFIG.REQUIRED_FIELD_MISSING");
        result.Errors[0].Description.Should().Contain("Jwt:SecretKey");
    }

    [Fact]
    public void JwtSettings_Validate_WithShortSecretKey_ReturnsFailure()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "TooShort",
            Issuer = "https://api.example.com",
            Audience = "https://app.example.com",
            AccessTokenExpiryMinutes = 15,
            RefreshTokenExpiryDays = 7
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "CONFIG.VALUE_OUT_OF_RANGE");
        result.Errors[0].Description.Should().Contain("at least 32 characters");
    }

    [Fact]
    public void JwtSettings_Validate_WithMultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "",
            Issuer = "",
            Audience = "",
            AccessTokenExpiryMinutes = 0,
            RefreshTokenExpiryDays = 0
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCountGreaterThan(3);
        result.Errors.Should().Contain(e => e.Description.Contains("SecretKey"));
        result.Errors.Should().Contain(e => e.Description.Contains("Issuer"));
        result.Errors.Should().Contain(e => e.Description.Contains("Audience"));
    }

    [Fact]
    public void JwtSettings_Validate_WithInvalidExpiryTimes_ReturnsFailure()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVerySecureSecretKeyWith256BitsOrMore!@#$%",
            Issuer = "https://api.example.com",
            Audience = "https://app.example.com",
            AccessTokenExpiryMinutes = -5,
            RefreshTokenExpiryDays = -1
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Description.Contains("AccessTokenExpiryMinutes"));
        result.Errors.Should().Contain(e => e.Description.Contains("RefreshTokenExpiryDays"));
    }

    #endregion

    #region AppUrls Validation Tests

    [Fact]
    public void AppUrls_Validate_WithValidUrls_ReturnsSuccess()
    {
        // Arrange
        var settings = new AppUrls
        {
            FrontendBaseUrl = "https://app.example.com",
            ConfirmEmailPath = "/auth/confirm-email"
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void AppUrls_Validate_WithInvalidFrontendBaseUrl_ReturnsFailure()
    {
        // Arrange
        var settings = new AppUrls
        {
            FrontendBaseUrl = "not-a-valid-url",
            ConfirmEmailPath = "/auth/confirm-email"
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "CONFIG.INVALID_FORMAT");
        result.Errors[0].Description.Should().Contain("valid absolute URL");
    }

    #endregion

    #region PasswordSettings Validation Tests

    [Fact]
    public void PasswordSettings_Validate_WithValidSettings_ReturnsSuccess()
    {
        // Arrange
        var settings = new PasswordSettings
        {
            RequireDigit = true,
            RequireLowercase = true,
            RequireUppercase = true,
            RequireNonAlphanumeric = true,
            RequiredLength = 8,
            RequiredUniqueChars = 1,
            ResetTokenExpiryHours = 24
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void PasswordSettings_Validate_WithInvalidRequiredLength_ReturnsFailure()
    {
        // Arrange
        var settings = new PasswordSettings
        {
            RequireDigit = true,
            RequireLowercase = true,
            RequireUppercase = true,
            RequireNonAlphanumeric = true,
            RequiredLength = 3,
            RequiredUniqueChars = 1
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "CONFIG.VALUE_OUT_OF_RANGE");
        result.Errors[0].Description.Should().Contain("between 6 and 128");
    }

    #endregion

    #region EmailSettings Validation Tests

    [Fact]
    public void EmailSettings_Validate_WithValidSettings_ReturnsSuccess()
    {
        // Arrange
        var settings = new EmailSettings
        {
            Provider = "Smtp",
            FromName = "Test Application",
            From = "test@example.com",
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            SmtpUser = "user@example.com",
            SmtpPassword = "password123",
            SmtpEnableSsl = true
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void EmailSettings_Validate_WithMissingProvider_ReturnsFailure()
    {
        // Arrange
        var settings = new EmailSettings
        {
            Provider = "",
            FromName = "Test Application",
            From = "test@example.com",
            SmtpHost = "smtp.example.com",
            SmtpPort = 587
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "CONFIG.REQUIRED_FIELD_MISSING");
        result.Errors.Should().Contain(e => e.Description.Contains("Provider"));
    }

    [Fact]
    public void EmailSettings_Validate_WithMissingFromName_ReturnsFailure()
    {
        // Arrange
        var settings = new EmailSettings
        {
            Provider = "Smtp",
            FromName = "",
            From = "test@example.com",
            SmtpHost = "smtp.example.com",
            SmtpPort = 587
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "CONFIG.REQUIRED_FIELD_MISSING");
        result.Errors.Should().Contain(e => e.Description.Contains("FromName"));
    }

    #endregion

    #region AuthSettings Validation Tests

    [Fact]
    public void AuthSettings_Validate_WithValidSettings_ReturnsSuccess()
    {
        // Arrange
        var settings = new AuthSettings
        {
            Password = new PasswordSettings
            {
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = true,
                RequiredLength = 8,
                RequiredUniqueChars = 1,
                ResetTokenExpiryHours = 24
            },
            Jwt = new JwtSettings
            {
                SecretKey = "ThisIsAVerySecureSecretKeyWith256BitsOrMore!@#$%",
                Issuer = "https://api.example.com",
                Audience = "https://app.example.com",
                AccessTokenExpiryMinutes = 15,
                RefreshTokenExpiryDays = 7
            },
            AppUrls = new AppUrls
            {
                FrontendBaseUrl = "https://app.example.com",
                ConfirmEmailPath = "/auth/confirm-email"
            },
            User = new UserSettings
            {
                RequireConfirmedEmail = false,
                RequireConfirmedPhoneNumber = false,
                RequireUniqueEmail = true,
                AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+",
                SessionTimeoutMinutes = 480
            },
            Lockout = new LockoutSettings
            {
                DefaultLockoutEnabled = true,
                DefaultLockoutTimeSpanMinutes = 15,
                MaxFailedAccessAttempts = 5
            },
            EmailConfirmation = new EmailConfirmationSettings
            {
                TokenExpiryHours = 24,
                SendWelcomeEmail = true,
                MaxEmailsPerHour = 3
            },
            UserModuleFeatures = new UserModuleFeatures { EnableLogin = true }
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void AuthSettings_Validate_WithInvalidNestedSettings_CollectsAllErrors()
    {
        // Arrange
        var settings = new AuthSettings
        {
            Password = new PasswordSettings
            {
                RequiredLength = 3  // Invalid: too short
            },
            Jwt = new JwtSettings
            {
                SecretKey = "",  // Invalid: missing
                Issuer = "",     // Invalid: missing
                Audience = "",   // Invalid: missing
                AccessTokenExpiryMinutes = 0,
                RefreshTokenExpiryDays = 0
            },
            AppUrls = new AppUrls
            {
                FrontendBaseUrl = "not-a-valid-url",  // Invalid: bad format
                ConfirmEmailPath = "/auth/confirm-email"
            },
            User = new UserSettings { RequireConfirmedEmail = false },
            UserModuleFeatures = new UserModuleFeatures { EnableLogin = true }
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCountGreaterThan(5);
        result.Errors.Should().Contain(e => e.Description.Contains("Password"));
        result.Errors.Should().Contain(e => e.Description.Contains("SecretKey"));
        result.Errors.Should().Contain(e => e.Description.Contains("BaseUrl"));
    }

    #endregion

    #region Error Type Tests

    [Fact]
    public void ConfigurationErrors_HaveCorrectErrorType()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "",
            Issuer = "",
            Audience = "",
            AccessTokenExpiryMinutes = 0,
            RefreshTokenExpiryDays = 0
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.Errors.Should().AllSatisfy(error =>
            error.Type.Should().Be(ErrorType.Configuration));
    }

    [Fact]
    public void ConfigurationErrors_HaveCorrectErrorCodes()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "",  // REQUIRED_FIELD_MISSING
            Issuer = "https://api.example.com",
            Audience = "https://app.example.com",
            AccessTokenExpiryMinutes = -5,  // VALUE_OUT_OF_RANGE
            RefreshTokenExpiryDays = 7
        };

        // Act
        var result = settings.Validate();

        // Assert
        result.Errors.Should().Contain(e => e.Code == "CONFIG.REQUIRED_FIELD_MISSING");
        result.Errors.Should().Contain(e => e.Code == "CONFIG.VALUE_OUT_OF_RANGE");
    }

    #endregion

    #region Result Pattern Tests

    [Fact]
    public void Result_ImplicitConversionFromError_CreatesFailureResult()
    {
        // Arrange
        var error = Errors.Configuration.RequiredFieldMissing("TestField");

        // Act
        Result result = error;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().Be(error);
    }

    [Fact]
    public void Result_FailureWithMultipleErrors_ContainsAllErrors()
    {
        // Arrange
        var errors = new List<Error>
        {
            Errors.Configuration.RequiredFieldMissing("Field1"),
            Errors.Configuration.RequiredFieldMissing("Field2")
        };

        // Act
        var result = Result.Failure(errors);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Result_SuccessFactory_CreatesSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Result_FailureFactory_CreatesFailureResult()
    {
        // Arrange
        var error = Errors.Configuration.RequiredFieldMissing("TestField");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
    }

    #endregion
}
