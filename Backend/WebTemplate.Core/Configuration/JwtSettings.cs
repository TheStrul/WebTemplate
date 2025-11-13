namespace WebTemplate.Core.Configuration
{
    using System.ComponentModel.DataAnnotations;
    using WebTemplate.Core.Common;

    /// <summary>
    /// JWT authentication settings configuration
    /// All values configurable through appsettings.json - NO hard-coded values!
    /// </summary>
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        [Required]
        public string SecretKey { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Access token expiry in minutes (default: 15 minutes)
        /// </summary>
        [Range(1, 1440)] // 1 minute to 24 hours
        public int AccessTokenExpiryMinutes { get; set; } = 15;

        /// <summary>
        /// Refresh token expiry in days (default: 7 days)
        /// </summary>
        [Range(1, 365)] // 1 day to 1 year
        public int RefreshTokenExpiryDays { get; set; } = 7;

        /// <summary>
        /// Validates JWT settings
        /// </summary>
        public Result Validate()
        {
            var errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(SecretKey))
                errors.Add(Errors.Configuration.RequiredFieldMissingWithGuidance(
                    "AuthSettings:Jwt:SecretKey",
                    "Please set it in User Secrets or environment variables."
                ));
            else if (SecretKey.Length < 32)
                errors.Add(Errors.Configuration.ValueOutOfRange(
                    "AuthSettings:Jwt:SecretKey",
                    "must be at least 32 characters for secure encryption."
                ));

            if (string.IsNullOrWhiteSpace(Issuer))
                errors.Add(Errors.Configuration.RequiredFieldMissing("AuthSettings:Jwt:Issuer"));

            if (string.IsNullOrWhiteSpace(Audience))
                errors.Add(Errors.Configuration.RequiredFieldMissing("AuthSettings:Jwt:Audience"));

            if (AccessTokenExpiryMinutes <= 0)
                errors.Add(Errors.Configuration.ValueOutOfRange(
                    "AuthSettings:Jwt:AccessTokenExpiryMinutes",
                    "must be greater than 0."
                ));

            if (RefreshTokenExpiryDays <= 0)
                errors.Add(Errors.Configuration.ValueOutOfRange(
                    "AuthSettings:Jwt:RefreshTokenExpiryDays",
                    "must be greater than 0."
                ));

            return errors.Any() ? Result.Failure(errors) : Result.Success();
        }

        /// <summary>
        /// Whether to validate token lifetime
        /// </summary>
        public bool ValidateLifetime { get; set; } = true;

        /// <summary>
        /// Whether to validate issuer
        /// </summary>
        public bool ValidateIssuer { get; set; } = true;

        /// <summary>
        /// Whether to validate audience
        /// </summary>
        public bool ValidateAudience { get; set; } = true;

        /// <summary>
        /// Clock skew allowance in minutes (default: 5 minutes)
        /// </summary>
        public int ClockSkewMinutes { get; set; } = 5;

        /// <summary>
        /// Whether to require HTTPS for metadata
        /// </summary>
        public bool RequireHttpsMetadata { get; set; } = true;

        /// <summary>
        /// Whether to save tokens in auth properties
        /// </summary>
        public bool SaveToken { get; set; } = true;

        /// <summary>
        /// Maximum number of refresh tokens per user (default: 5)
        /// </summary>
        [Range(1, 20)]
        public int MaxRefreshTokensPerUser { get; set; } = 5;

        /// <summary>
        /// Whether to allow multiple sessions per user
        /// </summary>
        public bool AllowMultipleSessions { get; set; } = true;
    }
}
