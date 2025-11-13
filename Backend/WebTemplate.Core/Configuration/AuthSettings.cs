namespace WebTemplate.Core.Configuration
{
    using System.ComponentModel.DataAnnotations;
    using WebTemplate.Core.Common;

    /// <summary>
    /// Authentication and security settings configuration
    /// All values configurable through appsettings.json - NO hard-coded values!
    /// </summary>
    public class AuthSettings : IBaseConfiguration
    {
        public const string SectionName = "AuthSettings";

        /// <summary>
        /// Password requirements
        /// </summary>
        public PasswordSettings Password { get; set; } = new();

        /// <summary>
        /// Lockout settings
        /// </summary>
        public LockoutSettings Lockout { get; set; } = new();

        /// <summary>
        /// User settings
        /// </summary>
        public UserSettings User { get; set; } = new();

        /// <summary>
        /// Email confirmation settings
        /// </summary>
        public EmailConfirmationSettings EmailConfirmation { get; set; } = new();

        /// <summary>
        /// User module feature flags
        /// </summary>
        public UserModuleFeatures UserModuleFeatures { get; set; } = new();

        /// <summary>
        /// Application URLs for email links, etc.
        /// </summary>
        public AppUrls AppUrls { get; set; } = new();

        /// <summary>
        /// JWT token settings
        /// </summary>
        public JwtSettings Jwt { get; set; } = new();

        /// <summary>
        /// Validates all authentication settings and nested structures
        /// </summary>
        public Result Validate()
        {
            var errors = new List<Error>();

            if (Password == null)
            {
                errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:Password"));
            }
            else
            {
                var passwordResult = Password.Validate();
                if (passwordResult.IsFailure)
                    errors.AddRange(passwordResult.Errors);
            }

            if (Jwt == null)
            {
                errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:Jwt"));
            }
            else
            {
                var jwtResult = Jwt.Validate();
                if (jwtResult.IsFailure)
                    errors.AddRange(jwtResult.Errors);
            }

            if (AppUrls == null)
            {
                errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:AppUrls"));
            }
            else
            {
                var appUrlsResult = AppUrls.Validate();
                if (appUrlsResult.IsFailure)
                    errors.AddRange(appUrlsResult.Errors);
            }

            // Lockout, User, EmailConfirmation, UserModuleFeatures are optional with defaults

            return errors.Any() ? Result.Failure(errors) : Result.Success();
        }
    }

    public class PasswordSettings
    {
        /// <summary>
        /// Minimum password length (default: 8)
        /// </summary>
        [Range(6, 128)]
        public int RequiredLength { get; set; } = 8;

        /// <summary>
        /// Require at least one digit
        /// </summary>
        public bool RequireDigit { get; set; } = true;

        /// <summary>
        /// Require at least one lowercase letter
        /// </summary>
        public bool RequireLowercase { get; set; } = true;

        /// <summary>
        /// Require at least one uppercase letter
        /// </summary>
        public bool RequireUppercase { get; set; } = true;

        /// <summary>
        /// Require at least one non-alphanumeric character
        /// </summary>
        public bool RequireNonAlphanumeric { get; set; } = true;

        /// <summary>
        /// Number of unique characters required (default: 6)
        /// </summary>
        [Range(1, 128)]
        public int RequiredUniqueChars { get; set; } = 6;

        /// <summary>
        /// Password reset token expiry in hours (default: 24 hours)
        /// </summary>
        [Range(1, 168)] // 1 hour to 1 week
        public int ResetTokenExpiryHours { get; set; } = 24;

        /// <summary>
        /// Validates password settings
        /// </summary>
        public Result Validate()
        {
            if (RequiredLength < 6)
                return Errors.Configuration.ValueOutOfRange(
                    "AuthSettings:Password:RequiredLength",
                    "must be at least 6."
                );

            return Result.Success();
        }
    }

    public class LockoutSettings
    {
        /// <summary>
        /// Whether lockout is enabled for new users
        /// </summary>
        public bool DefaultLockoutEnabled { get; set; } = true;

        /// <summary>
        /// Lockout duration in minutes (default: 15 minutes)
        /// </summary>
        [Range(1, 1440)] // 1 minute to 24 hours
        public int DefaultLockoutTimeSpanMinutes { get; set; } = 15;

        /// <summary>
        /// Maximum number of access attempts before lockout (default: 5)
        /// </summary>
        [Range(3, 20)]
        public int MaxFailedAccessAttempts { get; set; } = 5;
    }

    public class UserSettings
    {
        /// <summary>
        /// Whether email confirmation is required for new accounts
        /// </summary>
        public bool RequireConfirmedEmail { get; set; } = true;

        /// <summary>
        /// Whether phone number confirmation is required
        /// </summary>
        public bool RequireConfirmedPhoneNumber { get; set; } = false;

        /// <summary>
        /// Whether users can have duplicate email addresses
        /// </summary>
        public bool RequireUniqueEmail { get; set; } = true;

        /// <summary>
        /// Allowed username characters
        /// </summary>
        public string AllowedUserNameCharacters { get; set; } =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

        /// <summary>
        /// Session timeout in minutes (default: 480 minutes = 8 hours)
        /// </summary>
        [Range(30, 1440)] // 30 minutes to 24 hours
        public int SessionTimeoutMinutes { get; set; } = 480;
    }

    public class EmailConfirmationSettings
    {
        /// <summary>
        /// Email confirmation token expiry in hours (default: 72 hours)
        /// </summary>
        [Range(1, 168)] // 1 hour to 1 week
        public int TokenExpiryHours { get; set; } = 72;

        /// <summary>
        /// Whether to send welcome email after confirmation
        /// </summary>
        public bool SendWelcomeEmail { get; set; } = true;

        /// <summary>
        /// Maximum number of confirmation emails per hour (default: 3)
        /// </summary>
        [Range(1, 10)]
        public int MaxEmailsPerHour { get; set; } = 3;
    }
}
