using System.Windows.Forms;

namespace WebTemplate.Setup.Services;

public interface INotificationService
{
    DialogResult Info(string message, string title);
    DialogResult Warn(string message, string title);
    DialogResult Error(string message, string title);
    DialogResult Confirm(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.YesNo, MessageBoxIcon icon = MessageBoxIcon.Question);
}

public sealed class NotificationService : INotificationService
{
    public DialogResult Info(string message, string title)
        => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public DialogResult Warn(string message, string title)
        => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public DialogResult Error(string message, string title)
        => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

    public DialogResult Confirm(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.YesNo, MessageBoxIcon icon = MessageBoxIcon.Question)
        => MessageBox.Show(message, title, buttons, icon);
}
