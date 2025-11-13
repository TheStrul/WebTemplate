using WebTemplate.Setup.Models;

namespace WebTemplate.Setup.UI;

/// <summary>
/// Control for configuring feature flags
/// </summary>
public partial class FeaturesControl : UserControl
{
    private FeaturesConfiguration? _config;

    public event EventHandler? SettingsChanged;

    public FeaturesControl()
    {
        InitializeComponent();
        WireUpEvents();
    }

    private void WireUpEvents()
    {
        // Exception Handling
        chkExceptionHandling.CheckedChanged += OnSettingChanged;
        chkUseDetailedErrors.CheckedChanged += OnSettingChanged;
        chkLogExceptions.CheckedChanged += OnSettingChanged;

        // Admin Seed
        chkAdminSeed.CheckedChanged += OnSettingChanged;
        chkCreateDefaultAdmin.CheckedChanged += OnSettingChanged;
        txtAdminEmail.TextChanged += OnSettingChanged;
        txtAdminPassword.TextChanged += OnSettingChanged;

        // CORS
        chkCors.CheckedChanged += OnSettingChanged;
        txtAllowedOrigins.TextChanged += OnSettingChanged;
    }

    private void OnSettingChanged(object? sender, EventArgs e)
    {
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void LoadSettings(FeaturesConfiguration config)
    {
        _config = config;

        // Exception Handling
        chkExceptionHandling.Checked = config.ExceptionHandling.Enabled;
        chkUseDetailedErrors.Checked = config.ExceptionHandling.UseProblemDetails;
        chkLogExceptions.Checked = false; // Not in model, always false

        // Admin Seed
        chkAdminSeed.Checked = config.AdminSeed.Enabled;
        chkCreateDefaultAdmin.Checked = config.AdminSeed.Enabled; // Simplified
        txtAdminEmail.Text = config.AdminSeed.Email;
        txtAdminPassword.Text = config.AdminSeed.Password;

        // CORS
        chkCors.Checked = config.Cors.Enabled;
        txtAllowedOrigins.Text = config.Cors.AllowedOrigins != null ? string.Join(", ", config.Cors.AllowedOrigins) : string.Empty;

        UpdateControlStates();
    }

    public void SaveSettings(FeaturesConfiguration config)
    {
        // Exception Handling
        config.ExceptionHandling.Enabled = chkExceptionHandling.Checked;
        config.ExceptionHandling.UseProblemDetails = chkUseDetailedErrors.Checked;
        // LogToConsole is not in the model, so we don't save it

        // Admin Seed
        config.AdminSeed.Enabled = chkAdminSeed.Checked;
        config.AdminSeed.Email = txtAdminEmail.Text;
        config.AdminSeed.Password = txtAdminPassword.Text;

        // CORS
        config.Cors.Enabled = chkCors.Checked;
        config.Cors.AllowedOrigins = txtAllowedOrigins.Text
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
    }

    private void UpdateControlStates()
    {
        // Exception Handling sub-options
        grpExceptionHandlingOptions.Enabled = chkExceptionHandling.Checked;

        // Admin Seed sub-options
        grpAdminSeedOptions.Enabled = chkAdminSeed.Checked;

        // CORS sub-options
        grpCorsOptions.Enabled = chkCors.Checked;
    }

    private void chkExceptionHandling_CheckedChanged(object sender, EventArgs e)
    {
        UpdateControlStates();
    }

    private void chkAdminSeed_CheckedChanged(object sender, EventArgs e)
    {
        UpdateControlStates();
    }

    private void chkCors_CheckedChanged(object sender, EventArgs e)
    {
        UpdateControlStates();
    }
}
