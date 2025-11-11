using Microsoft.Extensions.Configuration;

namespace WebTemplate.Core.Configuration;

/// <summary>
/// Concrete implementation of ICoreConfiguration that loads configuration
/// from IConfiguration and provides strongly-typed access.
/// Validates all required configuration on construction to ensure NO FALLBACKS.
/// </summary>
public class CoreConfiguration : ICoreConfiguration
{
    public JwtSettings Jwt { get; }
    public AuthSettings Auth { get; }
    public EmailSettings Email { get; }
    public AppUrls AppUrls { get; }
    public AdminSeedSettings AdminSeed { get; }
    public UserModuleFeatures UserModuleFeatures { get; }
    public ResponseMessages ResponseMessages { get; }

    public CoreConfiguration(IConfiguration configuration)
    {
        // Bind and validate JWT settings
        Jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException($"Required configuration section '{JwtSettings.SectionName}' is missing or invalid.");
        ValidateJwtSettings(Jwt);

        // Bind and validate Auth settings
        Auth = configuration.GetSection(AuthSettings.SectionName).Get<AuthSettings>()
            ?? throw new InvalidOperationException($"Required configuration section '{AuthSettings.SectionName}' is missing or invalid.");
        ValidateAuthSettings(Auth);

        // Bind and validate Email settings
        Email = configuration.GetSection(EmailSettings.SectionName).Get<EmailSettings>()
            ?? throw new InvalidOperationException($"Required configuration section '{EmailSettings.SectionName}' is missing or invalid.");
        ValidateEmailSettings(Email);

        // Bind and validate AppUrls
        AppUrls = configuration.GetSection(AppUrls.SectionName).Get<AppUrls>()
            ?? throw new InvalidOperationException($"Required configuration section '{AppUrls.SectionName}' is missing or invalid.");
        ValidateAppUrls(AppUrls);

        // Bind and validate AdminSeed settings
        AdminSeed = configuration.GetSection(AdminSeedSettings.SectionName).Get<AdminSeedSettings>()
            ?? throw new InvalidOperationException($"Required configuration section '{AdminSeedSettings.SectionName}' is missing or invalid.");
        ValidateAdminSeedSettings(AdminSeed);

        // Bind UserModule features (optional defaults allowed)
        UserModuleFeatures = configuration.GetSection("UserModule:Features").Get<UserModuleFeatures>()
            ?? new UserModuleFeatures();

        // Bind Response messages (optional defaults allowed)
        ResponseMessages = configuration.GetSection("ResponseMessages").Get<ResponseMessages>()
            ?? new ResponseMessages();
    }

    private static void ValidateJwtSettings(JwtSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.SecretKey))
            throw new InvalidOperationException("JwtSettings:SecretKey is required but not configured. Please set it in User Secrets or environment variables.");

        if (string.IsNullOrWhiteSpace(settings.Issuer))
            throw new InvalidOperationException("JwtSettings:Issuer is required but not configured.");

        if (string.IsNullOrWhiteSpace(settings.Audience))
            throw new InvalidOperationException("JwtSettings:Audience is required but not configured.");

        if (settings.AccessTokenExpiryMinutes <= 0)
            throw new InvalidOperationException("JwtSettings:AccessTokenExpiryMinutes must be greater than 0.");

        if (settings.RefreshTokenExpiryDays <= 0)
            throw new InvalidOperationException("JwtSettings:RefreshTokenExpiryDays must be greater than 0.");
    }

    private static void ValidateAuthSettings(AuthSettings settings)
    {
        if (settings.Password == null)
            throw new InvalidOperationException("AuthSettings:Password is required but not configured.");

        if (settings.Password.RequiredLength < 6)
            throw new InvalidOperationException("AuthSettings:Password:RequiredLength must be at least 6.");
    }

    private static void ValidateEmailSettings(EmailSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Provider))
            throw new InvalidOperationException("EmailSettings:Provider is required but not configured.");

        if (string.IsNullOrWhiteSpace(settings.FromName))
            throw new InvalidOperationException("EmailSettings:FromName is required but not configured.");

        // From, SmtpHost, etc. can be validated based on provider type if needed
    }

    private static void ValidateAppUrls(AppUrls settings)
    {
        // FrontendBaseUrl can be empty in some scenarios, but validate format if present
        if (!string.IsNullOrWhiteSpace(settings.FrontendBaseUrl) &&
            !Uri.TryCreate(settings.FrontendBaseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException($"AppUrls:FrontendBaseUrl '{settings.FrontendBaseUrl}' is not a valid absolute URL.");
        }

        if (string.IsNullOrWhiteSpace(settings.ConfirmEmailPath))
            throw new InvalidOperationException("AppUrls:ConfirmEmailPath is required but not configured.");
    }

    private static void ValidateAdminSeedSettings(AdminSeedSettings settings)
    {
        if (settings.Enabled)
        {
            if (string.IsNullOrWhiteSpace(settings.Email))
                throw new InvalidOperationException("AdminSeed:Email is required when AdminSeed:Enabled is true. Please set it in User Secrets or environment variables.");

            if (string.IsNullOrWhiteSpace(settings.Password))
                throw new InvalidOperationException("AdminSeed:Password is required when AdminSeed:Enabled is true. Please set it in User Secrets or environment variables.");

            if (string.IsNullOrWhiteSpace(settings.FirstName))
                throw new InvalidOperationException("AdminSeed:FirstName is required when AdminSeed:Enabled is true.");

            if (string.IsNullOrWhiteSpace(settings.LastName))
                throw new InvalidOperationException("AdminSeed:LastName is required when AdminSeed:Enabled is true.");
        }
    }
}
