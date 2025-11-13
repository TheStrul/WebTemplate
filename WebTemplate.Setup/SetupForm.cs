using System.Diagnostics;
using System.Text;
using System.IO;

namespace WebTemplate.Setup;

public partial class SetupForm : Form
{
    private const string ApiProjectRelativePath = "Backend/WebTemplate.API/WebTemplate.API.csproj"; // user can override later if needed
    private readonly List<SecretDescriptor> _requiredSecrets = new()
    {
        new SecretDescriptor("JwtSettings:SecretKey", "Development-JWT-Secret-Key-2024-Not-For-Production-Use-Only-Local-Development", true),
        new SecretDescriptor("AdminSeed:Password", "Admin123!", true)
    };
    private readonly List<SecretDescriptor> _optionalSecrets = new()
    {
        new SecretDescriptor("AdminSeed:Email", "admin@WebTemplate.com", false),
        new SecretDescriptor("AdminSeed:FirstName", "System", false),
        new SecretDescriptor("AdminSeed:LastName", "Administrator", false)
    };

    public SetupForm()
    {
        InitializeComponent();
        secretsGrid.AutoGenerateColumns = false;
        secretsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Key", DataPropertyName = nameof(SecretRow.Key), ReadOnly = true, Width = 280 });
        secretsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Value", DataPropertyName = nameof(SecretRow.Value), Width = 380 });
        secretsGrid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Required", DataPropertyName = nameof(SecretRow.Required), ReadOnly = true });
        LoadSecrets();
    }

    private string GetApiProjectPath()
    {
        var root = GetSolutionRoot();
        if (root is null)
        {
            throw new InvalidOperationException("Cannot locate solution root. Launch the setup from within the repository build output.");
        }
        var apiPath = Path.Combine(root, ApiProjectRelativePath);
        return apiPath;
    }

    private static string? GetSolutionRoot()
    {
        var dir = new DirectoryInfo(Application.StartupPath);
        // Walk up until we find WebTemplate.sln or a folder containing Backend
        while (dir != null)
        {
            var sln = Path.Combine(dir.FullName, "WebTemplate.sln");
            var backendDir = Path.Combine(dir.FullName, "Backend");
            if (File.Exists(sln) || Directory.Exists(backendDir))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return null;
    }

    private void LoadSecrets()
    {
        var list = new List<SecretRow>();
        foreach (var s in _requiredSecrets)
        {
            list.Add(new SecretRow(s.Key, GetExistingSecretValue(s.Key), s.Required));
        }
        foreach (var s in _optionalSecrets)
        {
            list.Add(new SecretRow(s.Key, GetExistingSecretValue(s.Key), s.Required));
        }
        secretsGrid.DataSource = list;
    }

    private string GetExistingSecretValue(string key)
    {
        var apiProject = GetApiProjectPath();
        var (exitCode, output) = RunDotnet($"user-secrets list --project \"{apiProject}\"");
        if (exitCode != 0 || string.IsNullOrWhiteSpace(output)) return string.Empty;
        using var reader = new StringReader(output);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var idx = line.IndexOf('=');
            if (idx <= 0) continue;
            var k = line.Substring(0, idx).Trim();
            if (k.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return line[(idx + 1)..].Trim();
            }
        }
        return string.Empty;
    }

    private (int exitCode, string output) RunDotnet(string arguments)
    {
        var psi = new ProcessStartInfo("dotnet", arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var proc = Process.Start(psi)!;
        var sb = new StringBuilder();
        proc.OutputDataReceived += (_, e) => { if (e.Data != null) sb.AppendLine(e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (e.Data != null) sb.AppendLine(e.Data); };
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();
        proc.WaitForExit();
        return (proc.ExitCode, sb.ToString());
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        if (secretsGrid.DataSource is not List<SecretRow> rows) return;
        var missingRequired = rows.Where(r => r.Required && string.IsNullOrWhiteSpace(r.Value)).Select(r => r.Key).ToList();
        if (missingRequired.Count > 0)
        {
            MessageBox.Show("Missing required secrets:\n" + string.Join('\n', missingRequired), "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var apiProject = GetApiProjectPath();
        // Ensure user-secrets are initialized for the API project
        RunDotnet($"user-secrets init --project \"{apiProject}\"");

        foreach (var row in rows)
        {
            if (!string.IsNullOrWhiteSpace(row.Value))
            {
                var (code, _) = RunDotnet($"user-secrets set --project \"{apiProject}\" \"{row.Key}\" \"{row.Value}\"");
                if (code != 0)
                {
                    MessageBox.Show($"Failed setting {row.Key}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }
        MessageBox.Show("Secrets saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnReload_Click(object? sender, EventArgs e)
    {
        LoadSecrets();
    }
}

public sealed record SecretDescriptor(string Key, string DefaultValue, bool Required);

public sealed class SecretRow
{
    public SecretRow(string key, string value, bool required)
    {
        Key = key;
        Value = value;
        Required = required;
    }
    public string Key { get; }
    public string Value { get; set; } = string.Empty;
    public bool Required { get; }
}
