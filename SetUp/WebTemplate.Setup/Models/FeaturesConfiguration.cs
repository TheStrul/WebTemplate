namespace WebTemplate.Setup.Models;

/// <summary>
/// Features configuration - maps to FeaturesOptions in WebTemplate.Core
/// Allows users to enable/disable features and configure their settings
/// </summary>
public class FeaturesConfiguration
{
    public SwaggerFeature Swagger { get; set; } = new();
    public CorsFeature Cors { get; set; } = new();
    public RateLimitingFeature RateLimiting { get; set; } = new();
    public SecurityHeadersFeature SecurityHeaders { get; set; } = new();
    public HealthChecksFeature HealthChecks { get; set; } = new();
    public IdentityAuthFeature IdentityAuth { get; set; } = new();
    public RefreshTokensFeature RefreshTokens { get; set; } = new();
    public AdminSeedFeature AdminSeed { get; set; } = new();
    public ExceptionHandlingFeature ExceptionHandling { get; set; } = new();
    public SerilogFeature Serilog { get; set; } = new();

    public ValidationResult Validate()
    {
        var errors = new List<string>();

        // Validate CORS if enabled
        if (Cors.Enabled && (Cors.AllowedOrigins == null || Cors.AllowedOrigins.Length == 0))
            errors.Add("CORS: At least one allowed origin is required when CORS is enabled");

        // Validate Rate Limiting if enabled
        if (RateLimiting.Enabled)
        {
            if (RateLimiting.PermitLimit <= 0)
                errors.Add("Rate Limiting: Permit limit must be greater than 0");
            if (RateLimiting.WindowSeconds <= 0)
                errors.Add("Rate Limiting: Window seconds must be greater than 0");
        }

        // Validate Admin Seed if enabled
        if (AdminSeed.Enabled)
        {
            if (string.IsNullOrWhiteSpace(AdminSeed.Email))
                errors.Add("Admin Seed: Email is required");
            if (string.IsNullOrWhiteSpace(AdminSeed.Password))
                errors.Add("Admin Seed: Password is required");
            if (string.IsNullOrWhiteSpace(AdminSeed.FirstName))
                errors.Add("Admin Seed: First name is required");
            if (string.IsNullOrWhiteSpace(AdminSeed.LastName))
                errors.Add("Admin Seed: Last name is required");
        }

        // Validate Health Checks if enabled
        if (HealthChecks.Enabled && string.IsNullOrWhiteSpace(HealthChecks.Path))
            errors.Add("Health Checks: Path is required when health checks are enabled");

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}

// Feature classes matching FeaturesOptions structure
public class SwaggerFeature
{
    public bool Enabled { get; set; } = true;
    public bool RequireJwtForTryItOut { get; set; } = false;
}

public class CorsFeature
{
    public bool Enabled { get; set; } = false;
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS" };
    public string[] AllowedHeaders { get; set; } = new[] { "Content-Type", "Authorization" };
}

public class RateLimitingFeature
{
    public bool Enabled { get; set; } = false;
    public string PolicyName { get; set; } = "fixed";
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 0;
}

public class SecurityHeadersFeature
{
    public bool Enabled { get; set; } = true;
    public string? ContentSecurityPolicy { get; set; }
}

public class HealthChecksFeature
{
    public bool Enabled { get; set; } = true;
    public string Path { get; set; } = "/health";
}

public class IdentityAuthFeature
{
    public bool Enabled { get; set; } = true;
}

public class RefreshTokensFeature
{
    public bool Enabled { get; set; } = true;
    public bool RotateOnUse { get; set; } = true;
    public int CleanupIntervalMinutes { get; set; } = 360;
}

public class AdminSeedFeature
{
    public bool Enabled { get; set; } = false;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class ExceptionHandlingFeature
{
    public bool Enabled { get; set; } = true;
    public bool UseProblemDetails { get; set; } = true;
}

public class SerilogFeature
{
    public bool Enabled { get; set; } = false;
    public string MinimumLevel { get; set; } = "Information";
}
