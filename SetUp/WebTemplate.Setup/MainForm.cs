using WebTemplate.Setup.Models;
using WebTemplate.Setup.Services;
using WebTemplate.Setup.UI;
using WebTemplate.Setup.Resources;

namespace WebTemplate.Setup;

/// <summary>
/// Main form for WebTemplate Setup application
/// </summary>
public partial class MainForm : Form
{
    private readonly ConfigurationPersistenceService _persistenceService;
    private readonly ProjectGenerationService _generationService;
    private readonly INotificationService _notifications;

    private WorkspaceConfiguration? _currentConfiguration;
    private bool _isDirty = false;

    // Tab controls
    private ProjectSettingsControl? _projectControl;
    private FeaturesControl? _featuresControl;
    private SecretsControl? _secretsControl;
    private DatabaseControl? _databaseControl;
    private ServerControl? _serverControl;
    private AuthControl? _authControl;

    public MainForm(ConfigurationPersistenceService persistenceService, ProjectGenerationService generationService, INotificationService notifications)
    {
        InitializeComponent();

        _persistenceService = persistenceService ?? throw new ArgumentNullException(nameof(persistenceService));
        _generationService = generationService ?? throw new ArgumentNullException(nameof(generationService));
        _notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));

        // Apply centralized theme FIRST, before any other initialization
        ApplyTheme();

        InitializeTabControls();
        LoadConfigurationsList();
        SetupEventHandlers();
        UpdateUIState();
    }

    /// <summary>
    /// Apply centralized UI theme to the entire form
    /// </summary>
    private void ApplyTheme()
    {
        // Apply theme to MainForm
        UITheme.Apply(this);

        // Set form-specific properties from theme
        this.MinimumSize = UITheme.FormSettings.MainFormMinimumSize;
        this.Size = UITheme.FormSettings.MainFormDefaultSize;
        this.StartPosition = UITheme.FormSettings.DefaultStartPosition;

        // Apply to status bar
        statusStrip.Font = UITheme.Fonts.StatusBar;
        statusStrip.Height = UITheme.StatusBarSettings.Height;
        statusStrip.Padding = UITheme.StatusBarSettings.Padding;

        // Apply to toolbar
        toolStrip.Font = UITheme.Fonts.Default;
        toolStrip.ImageScalingSize = UITheme.ToolbarSettings.IconSize;

        // Apply to tab control
        tabControl.Font = UITheme.Fonts.Default;
    }

    private void InitializeTabControls()
    {
        // Create and dock tab controls
        _projectControl = new ProjectSettingsControl { Dock = DockStyle.Fill };
        _featuresControl = new FeaturesControl { Dock = DockStyle.Fill };
        _secretsControl = new SecretsControl { Dock = DockStyle.Fill };
        _databaseControl = new DatabaseControl { Dock = DockStyle.Fill };
        _serverControl = new ServerControl { Dock = DockStyle.Fill };
        _authControl = new AuthControl { Dock = DockStyle.Fill };

        // Apply theme to each control
        UITheme.Apply(_projectControl);
        UITheme.Apply(_featuresControl);
        UITheme.Apply(_secretsControl);
        UITheme.Apply(_databaseControl);
        UITheme.Apply(_serverControl);
        UITheme.Apply(_authControl);

        // Add to tabs
        tabProject.Controls.Add(_projectControl);
        tabFeatures.Controls.Add(_featuresControl);
        tabSecrets.Controls.Add(_secretsControl);
        tabDatabase.Controls.Add(_databaseControl);
        tabServer.Controls.Add(_serverControl);
        tabAuth.Controls.Add(_authControl);

        // Wire up change events
        _projectControl.SettingsChanged += (s, e) => MarkDirty();
        _featuresControl.SettingsChanged += (s, e) => MarkDirty();
        _secretsControl.SettingsChanged += (s, e) => MarkDirty();
        _databaseControl.SettingsChanged += (s, e) => MarkDirty();
        _serverControl.SettingsChanged += (s, e) => MarkDirty();
        _authControl.SettingsChanged += (s, e) => MarkDirty();
    }

    private void SetupEventHandlers()
    {
        // Configuration management
        btnNew.Click += BtnNew_Click;
        btnLoad.Click += BtnLoad_Click;
        btnSave.Click += BtnSave_Click;
        btnDelete.Click += BtnDelete_Click;
        btnValidate.Click += BtnValidate_Click;
        btnGenerate.Click += BtnGenerate_Click;

        // Configuration selection
        cmbConfigurations.SelectedIndexChanged += CmbConfigurations_SelectedIndexChanged;

        // Track changes
        tabControl.SelectedIndexChanged += (s, e) => MarkDirty();
    }

    private void LoadConfigurationsList()
    {
        var configurations = _persistenceService.ListConfigurations();

        // Project to binding-friendly items with Id/Name properties
        var items = configurations
            .Select(c => new { Id = c.ConfigurationId, Name = c.ConfigurationName })
            .ToList();

        cmbConfigurations.ComboBox.DisplayMember = "Name";
        cmbConfigurations.ComboBox.ValueMember = "Id";
        cmbConfigurations.ComboBox.DataSource = items;
    }

    private void BtnNew_Click(object? sender, EventArgs e)
    {
        if (_isDirty && !ConfirmDiscardChanges())
            return;

        _currentConfiguration = new WorkspaceConfiguration
        {
            ConfigurationId = string.Empty,
            ConfigurationName = "New Configuration",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            Project = new ProjectSettings(),
            Features = new FeaturesConfiguration(),
            Secrets = new SecretsConfiguration(),
            Database = new DatabaseConfiguration(),
            Server = new ServerConfiguration(),
            Email = new EmailConfiguration(),
            Auth = new AuthConfiguration()
        };

        BindConfigurationToUI();
        _isDirty = false;
        UpdateUIState();
    }

    private async void BtnLoad_Click(object? sender, EventArgs e)
    {
        if (_isDirty && !ConfirmDiscardChanges())
            return;

        if (cmbConfigurations.ComboBox.SelectedValue is not string configId)
        {
            _notifications.Info(SR.Get("Msg_Select_Config_To_Load"), SR.Get("Title_Load_Configuration"));
            return;
        }

        var result = await _persistenceService.LoadConfigurationAsync(configId);
        if (result.Success && result.Config != null)
        {
            _currentConfiguration = result.Config;
            BindConfigurationToUI();
            _isDirty = false;
            UpdateUIState();
            lblStatus.Text = string.Format(SR.Get("Status_Loaded"), _currentConfiguration.ConfigurationName);
        }
        else
        {
            _notifications.Error($"{SR.Get("Msg_Failed_Load_Prefix")}\n{result.Message}", SR.Get("Title_Load_Failed"));
        }
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_currentConfiguration == null)
        {
            _notifications.Info(SR.Get("Msg_No_Config_To_Save"), SR.Get("Title_Save_Configuration"));
            return;
        }

        // Get values from UI
        BindUIToConfiguration();

        // Validate before saving
        var validation = _currentConfiguration.Validate();
        if (!validation.IsValid)
        {
            _notifications.Warn($"{SR.Get("Msg_Config_Validation_Failed")}\n\n{string.Join("\n", validation.Errors)}", SR.Get("Title_Validation_Failed"));
            return;
        }

        // Generate ID if new configuration
        if (string.IsNullOrWhiteSpace(_currentConfiguration.ConfigurationId))
        {
            _currentConfiguration.ConfigurationId = _persistenceService.GenerateConfigurationId(
                _currentConfiguration.ConfigurationName);
        }

        var result = await _persistenceService.SaveConfigurationAsync(_currentConfiguration);
        if (result.Success)
        {
            _isDirty = false;
            LoadConfigurationsList();
            UpdateUIState();
            lblStatus.Text = string.Format(SR.Get("Status_Saved"), _currentConfiguration.ConfigurationName);
            _notifications.Info(SR.Get("Msg_Config_Saved_Success"), SR.Get("Title_Save_Complete"));
        }
        else
        {
            _notifications.Error($"{SR.Get("Msg_Failed_Save_Prefix")}\n{result.Message}", SR.Get("Title_Save_Failed"));
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (cmbConfigurations.ComboBox.SelectedValue is not string configId)
        {
            _notifications.Info(SR.Get("Msg_Select_Config_To_Delete"), SR.Get("Title_Delete_Configuration"));
            return;
        }

        var configName = cmbConfigurations.Text;
        var confirmResult = _notifications.Confirm(
            string.Format(SR.Get("Msg_Confirm_Delete"), configName),
            SR.Get("Title_Delete_Configuration"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (confirmResult != DialogResult.Yes)
            return;

        var result = _persistenceService.DeleteConfiguration(configId);
        if (result.Success)
        {
            LoadConfigurationsList();
            _currentConfiguration = null;
            ClearUI();
            UpdateUIState();
            lblStatus.Text = string.Format(SR.Get("Status_Deleted"), configName);
        }
        else
        {
            _notifications.Error($"{SR.Get("Msg_Failed_Delete_Prefix")}\n{result.Message}", SR.Get("Title_Delete_Configuration"));
        }
    }

    private void BtnValidate_Click(object? sender, EventArgs e)
    {
        if (_currentConfiguration == null)
        {
            _notifications.Info(SR.Get("Msg_No_Config_To_Validate"), SR.Get("Title_Validate_Configuration"));
            return;
        }

        BindUIToConfiguration();
        var validation = _currentConfiguration.Validate();

        if (validation.IsValid)
        {
            _notifications.Info(SR.Get("Msg_Config_Valid"), SR.Get("Title_Validation_Passed"));
        }
        else
        {
            _notifications.Warn($"{SR.Get("Msg_Config_Validation_Failed")}\n\n{string.Join("\n", validation.Errors)}", SR.Get("Title_Validation_Failed"));
        }
    }

    private async void BtnGenerate_Click(object? sender, EventArgs e)
    {
        if (_currentConfiguration == null)
        {
            _notifications.Info(SR.Get("Msg_No_Config_To_Generate"), SR.Get("Title_Generate_Project"));
            return;
        }

        // Get latest values from UI
        BindUIToConfiguration();

        // Validate first
        var validation = _currentConfiguration.Validate();
        if (!validation.IsValid)
        {
            _notifications.Warn($"{SR.Get("Msg_Config_Validation_Failed")}\n\n{string.Join("\n", validation.Errors)}", SR.Get("Title_Validation_Failed"));
            return;
        }

        // Confirm generation
        var confirmResult = _notifications.Confirm(
            string.Format(SR.Get("Msg_Confirm_Generate"), _currentConfiguration.Project.ProjectName, _currentConfiguration.Project.TargetPath),
            SR.Get("Title_Generate_Project"));

        if (confirmResult != DialogResult.Yes)
            return;

        // Disable UI during generation
        SetGenerationUIState(true);

        try
        {
            var progress = new Progress<string>(message =>
            {
                // Update UI on UI thread
                this.Invoke(() =>
                {
                    lblStatus.Text = message;
                    txtLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\r\n");
                    progressBar.Value = Math.Min(progressBar.Value + 10, 100);
                });
            });

            progressBar.Value = 0;
            txtLog.Clear();

            var result = await _generationService.GenerateProjectAsync(_currentConfiguration, progress);

            progressBar.Value = 100;

            if (result.Success)
            {
                lblStatus.Text = "✅ " + SR.Get("Status_Generation_Success");
                _notifications.Info(string.Format(SR.Get("Msg_Generation_Complete"), result.GeneratedPath), SR.Get("Title_Generation_Complete"));
            }
            else
            {
                lblStatus.Text = "❌ " + SR.Get("Status_Generation_Failed");
                _notifications.Error($"{SR.Get("Msg_Failed_Generation_Prefix")}\n\n{result.Message}", SR.Get("Title_Generation_Failed"));
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = "❌ " + SR.Get("Status_Generation_Error");
            _notifications.Error($"{SR.Get("Msg_Generation_Error_Prefix")}\n\n{ex.Message}", SR.Get("Title_Error"));
        }
        finally
        {
            SetGenerationUIState(false);
        }
    }

    private void CmbConfigurations_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateUIState();
    }

    private void BindConfigurationToUI()
    {
        if (_currentConfiguration == null)
            return;

        _projectControl?.LoadSettings(_currentConfiguration.Project);
        _featuresControl?.LoadSettings(_currentConfiguration.Features);
        _secretsControl?.LoadSettings(_currentConfiguration.Secrets);
        _databaseControl?.LoadSettings(_currentConfiguration.Database);
        _serverControl?.LoadSettings(_currentConfiguration.Server);
        _authControl?.LoadSettings(_currentConfiguration.Auth);

        lblStatus.Text = string.Format(SR.Get("Status_Loaded"), _currentConfiguration.ConfigurationName);
    }

    private void BindUIToConfiguration()
    {
        if (_currentConfiguration == null)
            return;

        _projectControl?.SaveSettings(_currentConfiguration.Project);
        _featuresControl?.SaveSettings(_currentConfiguration.Features);
        _secretsControl?.SaveSettings(_currentConfiguration.Secrets);
        _databaseControl?.SaveSettings(_currentConfiguration.Database);
        _serverControl?.SaveSettings(_currentConfiguration.Server);
        _authControl?.SaveSettings(_currentConfiguration.Auth);

        // Use Project Name as Configuration Name if available
        if (!string.IsNullOrWhiteSpace(_currentConfiguration.Project.ProjectName))
        {
            _currentConfiguration.ConfigurationName = _currentConfiguration.Project.ProjectName;
        }

        // Update modified timestamp
        _currentConfiguration.ModifiedAt = DateTime.UtcNow;
    }

    private void ClearUI()
    {
        _projectControl?.LoadSettings(new ProjectSettings());
        _featuresControl?.LoadSettings(new FeaturesConfiguration());
        _secretsControl?.LoadSettings(new SecretsConfiguration());
        _databaseControl?.LoadSettings(new DatabaseConfiguration());
        _serverControl?.LoadSettings(new ServerConfiguration());
        _authControl?.LoadSettings(new AuthConfiguration());

        lblStatus.Text = SR.Get("Status_Ready");
    }

    private void MarkDirty()
    {
        if (_currentConfiguration != null && !_isDirty)
        {
            _isDirty = true;
            UpdateUIState();
        }
    }

    private void UpdateUIState()
    {
        var hasConfig = _currentConfiguration != null;
        var hasSelection = cmbConfigurations.ComboBox.SelectedValue != null;

        btnSave.Enabled = hasConfig && _isDirty;
        btnValidate.Enabled = hasConfig;
        btnGenerate.Enabled = hasConfig;
        btnDelete.Enabled = hasSelection;
        btnLoad.Enabled = hasSelection;

        // Update title
        var dirtyIndicator = _isDirty ? "*" : string.Empty;
        var configName = _currentConfiguration?.ConfigurationName ?? SR.Get("No_Configuration");
        this.Text = string.Format(SR.Get("App_Title"), configName, dirtyIndicator);
    }

    private void SetGenerationUIState(bool isGenerating)
    {
        btnNew.Enabled = !isGenerating;
        btnLoad.Enabled = !isGenerating;
        btnSave.Enabled = !isGenerating;
        btnDelete.Enabled = !isGenerating;
        btnValidate.Enabled = !isGenerating;
        btnGenerate.Enabled = !isGenerating;
        tabControl.Enabled = !isGenerating;
        cmbConfigurations.Enabled = !isGenerating;

        progressBar.Visible = isGenerating;
        if (!isGenerating)
            progressBar.Value = 0;
    }

    private bool ConfirmDiscardChanges()
    {
        var result = _notifications.Confirm(
            SR.Get("Msg_Discard_Unsaved_Changes"),
            SR.Get("Title_Unsaved_Changes"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        return result == DialogResult.Yes;
    }

    // ══════════════════════════════════════════════════════
    // STATUS MESSAGE HELPERS - Using centralized UITheme
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// Display a success status message (green)
    /// </summary>
    private void ShowSuccessStatus(string message)
    {
        var (msg, color) = UITheme.CreateStatusMessage(message, UITheme.StatusType.Success);
        lblStatus.Text = msg;
        lblStatus.ForeColor = color;
    }

    /// <summary>
    /// Display an error status message (red)
    /// </summary>
    private void ShowErrorStatus(string message)
    {
        var (msg, color) = UITheme.CreateStatusMessage(message, UITheme.StatusType.Error);
        lblStatus.Text = msg;
        lblStatus.ForeColor = color;
    }

    /// <summary>
    /// Display a warning status message (orange)
    /// </summary>
    private void ShowWarningStatus(string message)
    {
        var (msg, color) = UITheme.CreateStatusMessage(message, UITheme.StatusType.Warning);
        lblStatus.Text = msg;
        lblStatus.ForeColor = color;
    }

    /// <summary>
    /// Display an info status message (blue)
    /// </summary>
    private void ShowInfoStatus(string message)
    {
        var (msg, color) = UITheme.CreateStatusMessage(message, UITheme.StatusType.Info);
        lblStatus.Text = msg;
        lblStatus.ForeColor = color;
    }

    /// <summary>
    /// Display a normal status message (default color)
    /// </summary>
    private void ShowNormalStatus(string message)
    {
        var (msg, color) = UITheme.CreateStatusMessage(message, UITheme.StatusType.Normal);
        lblStatus.Text = msg;
        lblStatus.ForeColor = color;
    }
}
