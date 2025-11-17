using WebTemplate.Setup.Models;
using Microsoft.Data.SqlClient;

namespace WebTemplate.Setup.UI;

public partial class DatabaseControl : UserControl
{
    private DatabaseConfiguration? _config;
    private string _projectName = "";
    public event EventHandler? SettingsChanged;

    public DatabaseControl()
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
        // Connection string should use monospace font
        UITheme.ApplyMonospaceFont(txtConnectionString);

        // Set consistent spacing
        this.Padding = UITheme.Spacing.TabPadding;
    }

    private void WireUpEvents()
    {
        txtConnectionString.TextChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        chkExecuteInitScript.CheckedChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        txtInitScriptPath.TextChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        chkTestConnection.CheckedChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        chkCreateDatabaseIfNotExists.CheckedChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetProjectName(string projectName)
    {
        _projectName = projectName;
    }

    public void LoadSettings(DatabaseConfiguration config)
    {
        _config = config;
        txtConnectionString.Text = config.ConnectionString;
        chkExecuteInitScript.Checked = config.ExecuteInitScript;
        txtInitScriptPath.Text = config.InitScriptPath;
        chkTestConnection.Checked = config.TestConnection;
        chkCreateDatabaseIfNotExists.Checked = config.CreateDatabaseIfNotExists;
    }

    public void SaveSettings(DatabaseConfiguration config)
    {
        config.ConnectionString = txtConnectionString.Text;
        config.ExecuteInitScript = chkExecuteInitScript.Checked;
        config.InitScriptPath = txtInitScriptPath.Text;
        config.TestConnection = chkTestConnection.Checked;
        config.CreateDatabaseIfNotExists = chkCreateDatabaseIfNotExists.Checked;
    }

    private async void btnTestConnection_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtConnectionString.Text))
        {
            MessageBox.Show("Please enter a connection string", "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnTestConnection.Enabled = false;
        lblTestResult.Text = "Testing...";
        lblTestResult.ForeColor = Color.Blue;

        try
        {
            using var connection = new SqlConnection(txtConnectionString.Text);
            await connection.OpenAsync();
            lblTestResult.Text = "✓ Connection successful";
            lblTestResult.ForeColor = Color.Green;
        }
        catch (Exception ex)
        {
            lblTestResult.Text = $"✗ Connection failed: {ex.Message}";
            lblTestResult.ForeColor = Color.Red;
        }
        finally
        {
            btnTestConnection.Enabled = true;
        }
    }

    private void btnGenerateValidValues_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtConnectionString.Text))
        {
            var dbName = string.IsNullOrWhiteSpace(_projectName) ? "WebTemplateDb" : $"{_projectName}Db";
            txtConnectionString.Text = $"Server=(localdb)\\mssqllocaldb;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";
        }
        if (string.IsNullOrWhiteSpace(txtInitScriptPath.Text))
        {
            txtInitScriptPath.Text = "Backend/WebTemplate.Data/Migrations/db-init.sql";
        }
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
