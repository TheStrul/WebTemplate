namespace WebTemplate.Core.Configuration;

/// <summary>
/// User module configuration options
/// </summary>
public class UserModuleOptions
{
    public string BasePath { get; set; } = "/api";
    public JwtOptions Jwt { get; set; } = new();
    public AuthOptions Auth { get; set; } = new();
    public SeedOptions Seed { get; set; } = new();
}

/// <summary>
/// JWT authentication options
/// </summary>
public class JwtOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
    public int ClockSkewMinutes { get; set; } = 5;
}

/// <summary>
/// Authentication and authorization options
/// </summary>
public class AuthOptions
{
    public bool RequireEmailConfirmation { get; set; } = false;
    public string[] AllowedOrigins { get; set; } = [];
    public PasswordRequirementsOptions PasswordRequirements { get; set; } = new();
}

/// <summary>
/// Password requirements configuration
/// </summary>
public class PasswordRequirementsOptions
{
    public bool RequireDigit { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = false;
    public int RequiredLength { get; set; } = 8;
    public int RequiredUniqueChars { get; set; } = 1;
}

/// <summary>
/// Seed data options for initial database setup
/// </summary>
public class SeedOptions
{
    public bool CreateDefaultAdmin { get; set; } = true;
    public string AdminEmail { get; set; } = "admin@corewebapp.com";
    public string AdminPassword { get; set; } = "Admin123!";
}
