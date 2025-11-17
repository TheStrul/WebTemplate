using WebTemplate.Setup.Models;

namespace WebTemplate.Setup.UI;

/// <summary>
/// User control for Project Settings tab
/// </summary>
public partial class ProjectSettingsControl : UserControl
{
    public ProjectSettings Settings { get; private set; } = new();

    public ProjectSettingsControl()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        txtProjectName.TextChanged += (s, e) => { OnSettingsChanged(); ProjectNameChanged?.Invoke(this, txtProjectName.Text); };
        txtTargetPath.TextChanged += (s, e) => OnSettingsChanged();
        txtCompanyName.TextChanged += (s, e) => OnSettingsChanged();
        txtAuthorName.TextChanged += (s, e) => OnSettingsChanged();
        chkInitializeGit.CheckedChanged += (s, e) => OnSettingsChanged();
        chkCreateInitialCommit.CheckedChanged += (s, e) => OnSettingsChanged();
        chkRunValidation.CheckedChanged += (s, e) => OnSettingsChanged();

        btnBrowse.Click += BtnBrowse_Click;
    }

    public void LoadSettings(ProjectSettings settings)
    {
        Settings = settings;

        txtProjectName.Text = settings.ProjectName;
        txtTargetPath.Text = settings.TargetPath;
        txtCompanyName.Text = settings.CompanyName;
        txtAuthorName.Text = settings.AuthorName;
        chkInitializeGit.Checked = settings.InitializeGit;
        chkCreateInitialCommit.Checked = settings.CreateInitialCommit;
        chkRunValidation.Checked = settings.RunValidation;
    }

    public void SaveSettings(ProjectSettings settings)
    {
        settings.ProjectName = txtProjectName.Text;
        settings.TargetPath = txtTargetPath.Text;
        settings.CompanyName = txtCompanyName.Text;
        settings.AuthorName = txtAuthorName.Text;
        settings.InitializeGit = chkInitializeGit.Checked;
        settings.CreateInitialCommit = chkCreateInitialCommit.Checked;
        settings.RunValidation = chkRunValidation.Checked;
    }

    private void BtnBrowse_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select target folder for new project",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (!string.IsNullOrEmpty(txtTargetPath.Text))
            dialog.SelectedPath = txtTargetPath.Text;

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtTargetPath.Text = dialog.SelectedPath;
        }
    }

    public event EventHandler? SettingsChanged;
    public event EventHandler<string>? ProjectNameChanged;

    private void OnSettingsChanged()
    {
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void btnGenerateValidValues_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtProjectName.Text))
        {
            txtProjectName.Text = "JitonClubApp";
        }
        if (string.IsNullOrWhiteSpace(txtTargetPath.Text))
        {
            txtTargetPath.Text = Path.Combine("C:\\Users\\avist\\source\\repos\\GitHubLocal\\Customers", txtProjectName.Text);
        }
        if (string.IsNullOrWhiteSpace(txtCompanyName.Text))
        {
            txtCompanyName.Text = "Jiton Club";
        }
        if (string.IsNullOrWhiteSpace(txtAuthorName.Text))
        {
            txtAuthorName.Text = Environment.UserName;
        }
        OnSettingsChanged();
    }

}
