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
