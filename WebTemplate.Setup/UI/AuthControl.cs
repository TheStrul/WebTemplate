using WebTemplate.Setup.Models;

namespace WebTemplate.Setup.UI;

public partial class AuthControl : UserControl
{
    public event EventHandler? SettingsChanged;

    public AuthControl()
    {
        InitializeComponent();
        ApplyTheme();
        WireUpEvents();
    }

    /// <summary>
    /// Apply centralized theme to this control
    /// </summary>
    private void ApplyTheme()
    {
        // Secret key should use monospace font (it's a base64 string)
        UITheme.ApplyMonospaceFont(txtSecretKey);

        // Set consistent spacing
        this.Padding = UITheme.Spacing.TabPadding;
    }

    private void WireUpEvents()
    {
        txtSecretKey.TextChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        txtIssuer.TextChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        txtAudience.TextChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        numAccessTokenExpiration.ValueChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        numRefreshTokenExpiration.ValueChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void LoadSettings(AuthConfiguration config)
    {
        txtSecretKey.Text = config.SecretKey;
        txtIssuer.Text = config.Issuer;
        txtAudience.Text = config.Audience;
        numAccessTokenExpiration.Value = config.ExpirationMinutes;
        numRefreshTokenExpiration.Value = config.RefreshTokenExpirationDays;
    }

    public void SaveSettings(AuthConfiguration config)
    {
        config.SecretKey = txtSecretKey.Text;
        config.Issuer = txtIssuer.Text;
        config.Audience = txtAudience.Text;
        config.ExpirationMinutes = (int)numAccessTokenExpiration.Value;
        config.RefreshTokenExpirationDays = (int)numRefreshTokenExpiration.Value;
    }

    private void btnGenerateKey_Click(object sender, EventArgs e)
    {
        txtSecretKey.Text = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
