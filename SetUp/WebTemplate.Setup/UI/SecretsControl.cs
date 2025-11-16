using WebTemplate.Setup.Models;

namespace WebTemplate.Setup.UI;

/// <summary>
/// Control for configuring secrets strategy
/// </summary>
public partial class SecretsControl : UserControl
{
    private SecretsConfiguration? _config;

    public event EventHandler? SettingsChanged;

    public SecretsControl()
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
        // Apply monospace font to Key Vault URL (it's a URL/connection string-like)
        UITheme.ApplyMonospaceFont(txtKeyVaultUrl);

        // Set consistent spacing
        this.Padding = UITheme.Spacing.TabPadding;

        // GroupBox should use theme padding
        if (grpKeyVaultSettings != null)
        {
            grpKeyVaultSettings.Padding = UITheme.Spacing.GroupBoxPadding;
            grpKeyVaultSettings.Font = UITheme.Fonts.DefaultBold;
        }
    }

    private void WireUpEvents()
    {
        rdoUserSecrets.CheckedChanged += OnStrategyChanged;
        rdoKeyVault.CheckedChanged += OnStrategyChanged;
        rdoEnvironment.CheckedChanged += OnStrategyChanged;
        rdoMixed.CheckedChanged += OnStrategyChanged;

        txtKeyVaultUrl.TextChanged += OnSettingChanged;
        chkUploadSecretsNow.CheckedChanged += OnSettingChanged;
    }

    private void OnStrategyChanged(object? sender, EventArgs e)
    {
        UpdateKeyVaultPanel();
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnSettingChanged(object? sender, EventArgs e)
    {
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void LoadSettings(SecretsConfiguration config)
    {
        _config = config;

        // Set radio button
        switch (config.Strategy)
        {
            case SecretsStrategy.UserSecrets:
                rdoUserSecrets.Checked = true;
                break;
            case SecretsStrategy.AzureKeyVault:
                rdoKeyVault.Checked = true;
                break;
            case SecretsStrategy.EnvironmentVariables:
                rdoEnvironment.Checked = true;
                break;
            case SecretsStrategy.Mixed:
                rdoMixed.Checked = true;
                break;
        }

        // Key Vault settings
        txtKeyVaultUrl.Text = config.AzureKeyVault?.VaultUri ?? string.Empty;
        chkUploadSecretsNow.Checked = config.AzureKeyVault?.UploadSecretsNow ?? false;

        UpdateKeyVaultPanel();
    }

    public void SaveSettings(SecretsConfiguration config)
    {
        // Determine strategy
        if (rdoUserSecrets.Checked)
            config.Strategy = SecretsStrategy.UserSecrets;
        else if (rdoKeyVault.Checked)
            config.Strategy = SecretsStrategy.AzureKeyVault;
        else if (rdoEnvironment.Checked)
            config.Strategy = SecretsStrategy.EnvironmentVariables;
        else if (rdoMixed.Checked)
            config.Strategy = SecretsStrategy.Mixed;

        // Key Vault settings
        if (rdoKeyVault.Checked || rdoMixed.Checked)
        {
            config.AzureKeyVault = new AzureKeyVaultSettings
            {
                VaultUri = txtKeyVaultUrl.Text,
                UploadSecretsNow = chkUploadSecretsNow.Checked
            };
        }
        else
        {
            config.AzureKeyVault = null;
        }
    }

    private void UpdateKeyVaultPanel()
    {
        bool isKeyVault = rdoKeyVault.Checked || rdoMixed.Checked;
        grpKeyVaultSettings.Enabled = isKeyVault;
    }

    private void btnGenerateValidValues_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtKeyVaultUrl.Text))
        {
            txtKeyVaultUrl.Text = "https://yourapp-keyvault.vault.azure.net/";
        }
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
