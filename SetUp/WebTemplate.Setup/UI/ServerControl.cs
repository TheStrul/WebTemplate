using WebTemplate.Setup.Models;

namespace WebTemplate.Setup.UI;

public partial class ServerControl : UserControl
{
    public event EventHandler? SettingsChanged;

    public ServerControl()
    {
        InitializeComponent();
        txtUrl.TextChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        txtHttpsUrl.TextChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        numConnectionTimeout.ValueChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void LoadSettings(ServerConfiguration config)
    {
        txtUrl.Text = config.Url;
        txtHttpsUrl.Text = config.HttpsUrl;
        numConnectionTimeout.Value = config.ConnectionTimeoutSeconds;
    }

    public void SaveSettings(ServerConfiguration config)
    {
        config.Url = txtUrl.Text;
        config.HttpsUrl = txtHttpsUrl.Text;
        config.ConnectionTimeoutSeconds = (int)numConnectionTimeout.Value;
    }

    private void btnGenerateValidValues_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtUrl.Text))
        {
            txtUrl.Text = "http://localhost:5000";
        }
        if (string.IsNullOrWhiteSpace(txtHttpsUrl.Text))
        {
            txtHttpsUrl.Text = "https://localhost:5001";
        }
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
