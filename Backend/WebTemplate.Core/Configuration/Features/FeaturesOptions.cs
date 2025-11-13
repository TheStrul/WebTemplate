using WebTemplate.Core.Common;

namespace WebTemplate.Core.Configuration.Features
{
    public class FeaturesOptions
    {
        public const string SectionName = "Features";
        public SwaggerFeatureOptions Swagger { get; set; } = new();
        public CorsFeatureOptions Cors { get; set; } = new();
        public RateLimitingFeatureOptions RateLimiting { get; set; } = new();
        public SecurityHeadersFeatureOptions SecurityHeaders { get; set; } = new();
        public HealthChecksFeatureOptions HealthChecks { get; set; } = new();
        public IdentityAuthFeatureOptions IdentityAuth { get; set; } = new();
        public RefreshTokensFeatureOptions RefreshTokens { get; set; } = new();
        public AdminSeedFeatureOptions AdminSeed { get; set; } = new();
        public ExceptionHandlingFeatureOptions ExceptionHandling { get; set; } = new();
        public SerilogFeatureOptions Serilog { get; set; } = new();

        /// <summary>
        /// Validates all feature settings
        /// </summary>
        public Result Validate()
        {
            var errors = new List<Error>();

            // Validate CORS settings if enabled
            if (Cors.Enabled && (Cors.AllowedOrigins == null || Cors.AllowedOrigins.Length == 0))
                errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:Cors:AllowedOrigins (required when CORS is enabled)"));

            // Validate Rate Limiting settings if enabled
            if (RateLimiting.Enabled && RateLimiting.PermitLimit <= 0)
                errors.Add(Errors.Configuration.ValueOutOfRange(
                    $"{SectionName}:RateLimiting:PermitLimit",
                    "must be greater than 0 when rate limiting is enabled"
                ));

            if (RateLimiting.Enabled && RateLimiting.WindowSeconds <= 0)
                errors.Add(Errors.Configuration.ValueOutOfRange(
                    $"{SectionName}:RateLimiting:WindowSeconds",
                    "must be greater than 0 when rate limiting is enabled"
                ));

            // Validate Admin Seed if enabled
            if (AdminSeed.Enabled)
            {
                if (string.IsNullOrWhiteSpace(AdminSeed.Email))
                    errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:AdminSeed:Email (required when AdminSeed is enabled)"));

                if (string.IsNullOrWhiteSpace(AdminSeed.Password))
                    errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:AdminSeed:Password (required when AdminSeed is enabled)"));

                if (string.IsNullOrWhiteSpace(AdminSeed.FirstName))
                    errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:AdminSeed:FirstName (required when AdminSeed is enabled)"));

                if (string.IsNullOrWhiteSpace(AdminSeed.LastName))
                    errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:AdminSeed:LastName (required when AdminSeed is enabled)"));
            }

            // Validate Refresh Tokens cleanup interval
            if (RefreshTokens.CleanupIntervalMinutes <= 0)
                errors.Add(Errors.Configuration.ValueOutOfRange(
                    $"{SectionName}:RefreshTokens:CleanupIntervalMinutes",
                    "must be greater than 0"
                ));

            // Validate Health Checks path
            if (HealthChecks.Enabled && string.IsNullOrWhiteSpace(HealthChecks.Path))
                errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:HealthChecks:Path (required when HealthChecks is enabled)"));

            return errors.Any() ? Result.Failure(errors) : Result.Success();
        }
    }

    public class SwaggerFeatureOptions
    {
        public bool Enabled { get; set; } = true;
        public bool RequireJwtForTryItOut { get; set; } = false;
    }

    public class CorsFeatureOptions
    {
        public bool Enabled { get; set; } = false;
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS" };
        public string[] AllowedHeaders { get; set; } = new[] { "Content-Type", "Authorization" };
    }

    public class RateLimitingFeatureOptions
    {
        public bool Enabled { get; set; } = false;
        public string PolicyName { get; set; } = "fixed";
        public int PermitLimit { get; set; } = 100; // requests
        public int WindowSeconds { get; set; } = 60; // per window
        public int QueueLimit { get; set; } = 0;
    }

    public class SecurityHeadersFeatureOptions
    {
        public bool Enabled { get; set; } = true;
        public string? ContentSecurityPolicy { get; set; }
    }

    public class HealthChecksFeatureOptions
    {
        public bool Enabled { get; set; } = true;
        public string Path { get; set; } = "/health";
    }

    public class IdentityAuthFeatureOptions { public bool Enabled { get; set; } = true; }
    public class RefreshTokensFeatureOptions
    {
        public bool Enabled { get; set; } = true;
        public bool RotateOnUse { get; set; } = true;
        public int CleanupIntervalMinutes { get; set; } = 360;
    }
    public class AdminSeedFeatureOptions
    {
        public bool Enabled { get; set; } = false;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
    public class ExceptionHandlingFeatureOptions { public bool Enabled { get; set; } = true; public bool UseProblemDetails { get; set; } = true; }
    public class SerilogFeatureOptions { public bool Enabled { get; set; } = false; public string? MinimumLevel { get; set; } = "Information"; }
}
