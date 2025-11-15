namespace WebTemplate.Core.Configuration
{
    using System.ComponentModel.DataAnnotations;
    using WebTemplate.Core.Common;

    /// <summary>
    /// Authentication and security settings configuration
    /// All values configurable through appsettings.json - NO hard-coded values!
    /// NO DEFAULTS - all values must be explicitly provided in configuration.
    /// App will fail to start if any required setting is missing.
    /// </summary>
    public class AuthSettings : IBaseConfiguration
    {
        public const string SectionName = "AuthSettings";

        /// <summary>
        /// Password requirements - REQUIRED
        /// </summary>
        public PasswordSettings Password { get; set; }

        /// <summary>
        /// Lockout settings - REQUIRED
        /// </summary>
        public LockoutSettings Lockout { get; set; }

        /// <summary>
        /// User settings - REQUIRED
        /// </summary>
        public UserSettings User { get; set; }

        /// <summary>
        /// Email confirmation settings - REQUIRED
        /// </summary>
        public EmailConfirmationSettings EmailConfirmation { get; set; }

        /// <summary>
        /// User module feature flags - REQUIRED
        /// </summary>
        public UserModuleFeatures UserModuleFeatures { get; set; }

        /// <summary>
        /// Application URLs for email links, etc. - REQUIRED
        /// </summary>
        public AppUrls AppUrls { get; set; }

        /// <summary>
        /// JWT token settings - REQUIRED
        /// </summary>
        public JwtSettings Jwt { get; set; }

        /// <summary>
        /// Validates all authentication settings and nested structures.
        /// Fails if any required value is missing or invalid.
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

            if (Lockout == null)
            {
                errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:Lockout"));
            }
            else
            {
                var lockoutResult = Lockout.Validate();
                if (lockoutResult.IsFailure)
                    errors.AddRange(lockoutResult.Errors);
            }

            if (User == null)
            {
                errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:User"));
            }
            else
            {
                var userResult = User.Validate();
                if (userResult.IsFailure)
                    errors.AddRange(userResult.Errors);
            }

            if (EmailConfirmation == null)
            {
                errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:EmailConfirmation"));
            }
            else
            {
                var emailResult = EmailConfirmation.Validate();
                if (emailResult.IsFailure)
                    errors.AddRange(emailResult.Errors);
            }

            if (UserModuleFeatures == null)
            {
                errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:UserModuleFeatures"));
            }
            else
            {
                var featuresResult = UserModuleFeatures.Validate();
                if (featuresResult.IsFailure)
                    errors.AddRange(featuresResult.Errors);
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

            return errors.Any() ? Result.Failure(errors) : Result.Success();
        }
    }

    /// <summary>
    /// Password policy settings - all values required, no defaults.
    /// </summary>
    public class PasswordSettings
    {
        /// <summary>
        /// Minimum password length - REQUIRED (must be 6-128)
        /// </summary>
        [Range(6, 128)]
        public int RequiredLength { get; set; }

        /// <summary>
        /// Require at least one digit - REQUIRED
        /// </summary>
        public bool RequireDigit { get; set; }

        /// <summary>
        /// Require at least one lowercase letter - REQUIRED
        /// </summary>
        public bool RequireLowercase { get; set; }

        /// <summary>
        /// Require at least one uppercase letter - REQUIRED
        /// </summary>
        public bool RequireUppercase { get; set; }

        /// <summary>
        /// Require at least one non-alphanumeric character - REQUIRED
        /// </summary>
        public bool RequireNonAlphanumeric { get; set; }

        /// <summary>
        /// Number of unique characters required - REQUIRED (must be 1-128)
        /// </summary>
        [Range(1, 128)]
        public int RequiredUniqueChars { get; set; }

        /// <summary>
        /// Password reset token expiry in hours - REQUIRED (must be 1-168)
        /// </summary>
        [Range(1, 168)] // 1 hour to 1 week
        public int ResetTokenExpiryHours { get; set; }

        /// <summary>
        /// Validates password settings - fails if any required value is missing or invalid.
        /// </summary>
        public Result Validate()
        {
            var errors = new List<Error>();

            if (RequiredLength == 0)
                errors.Add(Errors.Configuration.RequiredFieldMissing("AuthSettings:Password:RequiredLength"));

            if (RequiredLength < 6 || RequiredLength > 128)
                errors.Add(Errors.Configuration.ValueOutOfRange(
                    "AuthSettings:Password:RequiredLength",
                    "must be between 6 and 128"
                ));

            if (RequiredUniqueChars == 0)
                errors.Add(Errors.Configuration.RequiredFieldMissing("AuthSettings:Password:RequiredUniqueChars"));

            if (ResetTokenExpiryHours == 0)
                errors.Add(Errors.Configuration.RequiredFieldMissing("AuthSettings:Password:ResetTokenExpiryHours"));

            return errors.Any() ? Result.Failure(errors) : Result.Success();
        }
    }

    /// <summary>
    /// Account lockout settings - all values required, no defaults.
    /// </summary>
    public class LockoutSettings
    {
        /// <summary>
        /// Whether lockout is enabled for new users - REQUIRED
        /// </summary>
        public bool DefaultLockoutEnabled { get; set; }

        /// <summary>
        /// Lockout duration in minutes - REQUIRED (must be 1-1440)
        /// </summary>
        [Range(1, 1440)] // 1 minute to 24 hours
        public int DefaultLockoutTimeSpanMinutes { get; set; }

        /// <summary>
        /// Maximum number of access attempts before lockout - REQUIRED (must be 3-20)
        /// </summary>
        [Range(3, 20)]
        public int MaxFailedAccessAttempts { get; set; }

        /// <summary>
        /// Validates lockout settings - fails if any required value is missing or invalid.
        /// </summary>
        public Result Validate()
        {
            var errors = new List<Error>();

            if (DefaultLockoutTimeSpanMinutes == 0)
                errors.Add(Errors.Configuration.RequiredFieldMissing("AuthSettings:Lockout:DefaultLockoutTimeSpanMinutes"));

            if (MaxFailedAccessAttempts == 0)
                errors.Add(Errors.Configuration.RequiredFieldMissing("AuthSettings:Lockout:MaxFailedAccessAttempts"));

            return errors.Any() ? Result.Failure(errors) : Result.Success();
        }
    }

    /// <summary>
    /// User account settings - all values required, no defaults.
    /// </summary>
    public class UserSettings
    {
        /// <summary>
        /// Whether email confirmation is required for new accounts - REQUIRED
        /// </summary>
        public bool RequireConfirmedEmail { get; set; }

        /// <summary>
        /// Whether phone number confirmation is required - REQUIRED
        /// </summary>
        public bool RequireConfirmedPhoneNumber { get; set; }

        /// <summary>
        /// Whether users can have duplicate email addresses - REQUIRED
        /// </summary>
        public bool RequireUniqueEmail { get; set; }

        /// <summary>
        /// Allowed username characters - REQUIRED
        /// </summary>
        public string AllowedUserNameCharacters { get; set; }

        /// <summary>
        /// Session timeout in minutes - REQUIRED (must be 30-1440)
        /// </summary>
        [Range(30, 1440)] // 30 minutes to 24 hours
        public int SessionTimeoutMinutes { get; set; }

        /// <summary>
        /// Validates user settings - fails if any required value is missing or invalid.
        /// </summary>
        public Result Validate()
        {
            var errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(AllowedUserNameCharacters))
                errors.Add(Errors.Configuration.RequiredFieldMissing("AuthSettings:User:AllowedUserNameCharacters"));

            if (SessionTimeoutMinutes == 0)
                errors.Add(Errors.Configuration.RequiredFieldMissing("AuthSettings:User:SessionTimeoutMinutes"));

            if (SessionTimeoutMinutes < 30 || SessionTimeoutMinutes > 1440)
                errors.Add(Errors.Configuration.ValueOutOfRange(
                    "AuthSettings:User:SessionTimeoutMinutes",
                    "must be between 30 and 1440"
                ));

            return errors.Any() ? Result.Failure(errors) : Result.Success();
        }
    }

    /// <summary>
    /// Email confirmation settings - all values required, no defaults.
    /// </summary>
    public class EmailConfirmationSettings
    {
        /// <summary>
        /// Email confirmation token expiry in hours - REQUIRED (must be 1-168)
        /// </summary>
        [Range(1, 168)] // 1 hour to 1 week
        public int TokenExpiryHours { get; set; }

        /// <summary>
        /// Whether to send welcome email after confirmation - REQUIRED
        /// </summary>
        public bool SendWelcomeEmail { get; set; }

        /// <summary>
        /// Maximum number of confirmation emails per hour - REQUIRED (must be 1-10)
        /// </summary>
        [Range(1, 10)]
        public int MaxEmailsPerHour { get; set; }

        /// <summary>
        /// Validates email confirmation settings - fails if any required value is missing or invalid.
        /// </summary>
        public Result Validate()
        {
            var errors = new List<Error>();

            if (TokenExpiryHours == 0)
                errors.Add(Errors.Configuration.RequiredFieldMissing("AuthSettings:EmailConfirmation:TokenExpiryHours"));

            if (MaxEmailsPerHour == 0)
                errors.Add(Errors.Configuration.RequiredFieldMissing("AuthSettings:EmailConfirmation:MaxEmailsPerHour"));

            return errors.Any() ? Result.Failure(errors) : Result.Success();
        }
    }
}
