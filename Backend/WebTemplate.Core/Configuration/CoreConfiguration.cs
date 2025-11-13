using Microsoft.Extensions.Configuration;
using WebTemplate.Core.Common;

namespace WebTemplate.Core.Configuration;

/// <summary>
/// Concrete implementation of ICoreConfiguration that loads configuration
/// from IConfiguration and provides strongly-typed access.
/// Validates all required configuration on construction to ensure NO FALLBACKS.
/// </summary>
public class CoreConfiguration : ICoreConfiguration
{
    public AuthSettings Auth { get; }
    public EmailSettings Email { get; }
    public UserModuleFeatures UserModuleFeatures { get; }
    public ResponseMessages ResponseMessages { get; }

    public CoreConfiguration(IConfiguration configuration)
    {
        // Bind Auth settings
        Auth = configuration.GetSection(AuthSettings.SectionName).Get<AuthSettings>()
            ?? throw new InvalidOperationException($"Required configuration section '{AuthSettings.SectionName}' is missing or invalid.");

        // Bind Email settings
        Email = configuration.GetSection(EmailSettings.SectionName).Get<EmailSettings>()
            ?? throw new InvalidOperationException($"Required configuration section '{EmailSettings.SectionName}' is missing or invalid.");

        // Bind User Module Features
        UserModuleFeatures = configuration.GetSection(UserModuleFeatures.SectionName).Get<UserModuleFeatures>()
            ?? new UserModuleFeatures(); // Use defaults if not configured

        // Bind Response Messages
        ResponseMessages = configuration.GetSection("ResponseMessages").Get<ResponseMessages>()
            ?? new ResponseMessages(); // Use defaults if not configured

        // Validate all settings
        var validationResult = ValidateInternal();
        if (validationResult.IsFailure)
        {
            // Aggregate all errors into a single exception message
            var errorMessages = validationResult.Errors.Select(e => e.Description);
            throw new InvalidOperationException(
                $"Configuration validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errorMessages)}"
            );
        }
    }

    public virtual Result Validate() => ValidateInternal();

    private Result ValidateInternal()
    {
        var errors = new List<Error>();

        // Validate Auth settings
        var authResult = Auth.Validate();
        if (authResult.IsFailure)
            errors.AddRange(authResult.Errors);

        // Validate Email settings
        var emailResult = Email.Validate();
        if (emailResult.IsFailure)
            errors.AddRange(emailResult.Errors);

        // UserModuleFeatures and ResponseMessages don't need validation (just boolean flags and strings with defaults)

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }
}
