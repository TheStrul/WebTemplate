using System.Drawing;
using System.Windows.Forms;

namespace WebTemplate.Setup.UI;

/// <summary>
/// Centralized UI theme configuration for all WinForms controls
/// This is the SINGLE SOURCE OF TRUTH for all visual properties
/// Changing values here will affect the entire application
/// </summary>
public static class UITheme
{
    // ══════════════════════════════════════════════════════
    // COLORS
    // ══════════════════════════════════════════════════════

    /// <summary>Color scheme for the application</summary>
    public static class Colors
    {
        // Background Colors
        public static readonly Color WindowBackground = SystemColors.Control;
        public static readonly Color PanelBackground = Color.CadetBlue;
        public static readonly Color GroupBoxBackground = Color.Red;

        // Text Colors
        public static readonly Color PrimaryText = SystemColors.ControlText;
        public static readonly Color SecondaryText = SystemColors.GrayText;
        public static readonly Color DisabledText = SystemColors.GrayText;
        public static readonly Color PlaceholderText = Color.Gray;

        // Status Colors
        public static readonly Color Success = Color.FromArgb(0, 128, 0);      // Dark Green
        public static readonly Color Error = Color.FromArgb(192, 0, 0);        // Dark Red
        public static readonly Color Warning = Color.FromArgb(192, 128, 0);    // Orange
        public static readonly Color Info = Color.FromArgb(0, 102, 204);       // Blue

        // Border Colors
        public static readonly Color BorderLight = Color.FromArgb(224, 224, 224);
        public static readonly Color BorderMedium = Color.FromArgb(160, 160, 160);
        public static readonly Color BorderDark = Color.FromArgb(128, 128, 128);

        // Control Colors
        public static readonly Color ButtonFace = SystemColors.ButtonFace;
        public static readonly Color ButtonHighlight = SystemColors.Highlight;
        public static readonly Color TextBoxBackground = SystemColors.Window;
        public static readonly Color TextBoxDisabled = SystemColors.Control;
    }

    // ══════════════════════════════════════════════════════
    // FONTS
    // ══════════════════════════════════════════════════════

    /// <summary>Font settings for different UI elements</summary>
    public static class Fonts
    {
        private const string FontFamily = "Segoe UI";

        // Standard Fonts
        public static readonly Font Default = new(FontFamily, 9F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font DefaultBold = new(FontFamily, 9F, FontStyle.Bold, GraphicsUnit.Point);

        // Headers
        public static readonly Font Header = new(FontFamily, 10F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font SubHeader = new(FontFamily, 9.75F, FontStyle.Bold, GraphicsUnit.Point);

        // Special Purpose
        public static readonly Font Small = new(FontFamily, 8.25F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font SmallBold = new(FontFamily, 8.25F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font Large = new(FontFamily, 11F, FontStyle.Regular, GraphicsUnit.Point);

        // Status Bar
        public static readonly Font StatusBar = new(FontFamily, 8.25F, FontStyle.Regular, GraphicsUnit.Point);

        // Code/Monospace (for secret keys, connection strings, etc.)
        public static readonly Font Monospace = new("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
    }

    // ══════════════════════════════════════════════════════
    // SPACING & LAYOUT
    // ══════════════════════════════════════════════════════

    /// <summary>Consistent spacing and padding values</summary>
    public static class Spacing
    {
        // Margins
        public static readonly Padding ControlMargin = new(3, 3, 3, 3);
        public static readonly Padding PanelMargin = new(0, 0, 0, 0);
        public static readonly Padding FormMargin = new(10, 10, 10, 10);

        // Padding
        public static readonly Padding ControlPadding = new(5, 5, 5, 5);
        public static readonly Padding GroupBoxPadding = new(10, 10, 10, 10);
        public static readonly Padding PanelPadding = new(10, 10, 10, 10);
        public static readonly Padding TabPadding = new(10, 10, 10, 10);

        // Gaps between controls
        public const int SmallGap = 3;
        public const int MediumGap = 6;
        public const int LargeGap = 10;
        public const int SectionGap = 15;

        // Control spacing
        public const int LabelToControl = 3;
        public const int ControlToControl = 6;
        public const int GroupToGroup = 10;
    }

    // ══════════════════════════════════════════════════════
    // CONTROL SIZES
    // ══════════════════════════════════════════════════════

    /// <summary>Standard sizes for controls</summary>
    public static class Sizes
    {
        // Button Sizes
        public static readonly Size ButtonSmall = new(75, 23);
        public static readonly Size ButtonMedium = new(100, 30);
        public static readonly Size ButtonLarge = new(120, 35);
        public static readonly Size ButtonIcon = new(30, 23);

        // Input Controls
        public const int TextBoxHeight = 23;
        public const int TextBoxWidth = 250;
        public const int TextBoxWidthLarge = 400;

        public const int ComboBoxHeight = 23;
        public const int NumericUpDownHeight = 23;

        // Label Sizes
        public const int LabelWidthShort = 80;
        public const int LabelWidthMedium = 120;
        public const int LabelWidthLong = 180;

        // GroupBox minimum sizes
        public static readonly Size GroupBoxMinimum = new(200, 100);

        // Icon sizes
        public const int IconSmall = 16;
        public const int IconMedium = 24;
        public const int IconLarge = 32;
    }

    // ══════════════════════════════════════════════════════
    // TOOLBAR & STATUS BAR
    // ══════════════════════════════════════════════════════

    /// <summary>Toolbar and status bar specific settings</summary>
    public static class ToolbarSettings
    {
        public static readonly Size IconSize = new(24, 24);
        public static readonly Size ButtonSize = new(60, 55);
        public const int Padding = 5;
        public const int ImageScalingSize = 24;
    }

    public static class StatusBarSettings
    {
        public const int Height = 22;
        public static readonly Padding Padding = new(2, 2, 2, 2);
    }

    // ══════════════════════════════════════════════════════
    // FORM SETTINGS
    // ══════════════════════════════════════════════════════

    /// <summary>MainForm and dialog settings</summary>
    public static class FormSettings
    {
        // MainForm
        public static readonly Size MainFormMinimumSize = new(1000, 700);
        public static readonly Size MainFormDefaultSize = new(1200, 800);

        // Dialogs
        public static readonly Size DialogSmall = new(400, 250);
        public static readonly Size DialogMedium = new(600, 400);
        public static readonly Size DialogLarge = new(800, 600);

        // Positioning
        public const FormStartPosition DefaultStartPosition = FormStartPosition.CenterScreen;
    }

    // ══════════════════════════════════════════════════════
    // THEME APPLICATION METHODS
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// Apply theme to a form and all its controls
    /// </summary>
    public static void Apply(Form form)
    {
        if (form is null) throw new ArgumentNullException(nameof(form));

        form.Font = Fonts.Default;
        form.AutoScaleMode = AutoScaleMode.Dpi;
        form.BackColor = Colors.WindowBackground;

        ApplyToChildren(form.Controls);
    }

    /// <summary>
    /// Apply theme to a UserControl and all its controls
    /// </summary>
    public static void Apply(UserControl control)
    {
        if (control is null) throw new ArgumentNullException(nameof(control));

        control.Font = Fonts.Default;
        control.BackColor = Colors.WindowBackground;
        control.Padding = Spacing.TabPadding;

        ApplyToChildren(control.Controls);
    }

    /// <summary>
    /// Apply theme to all controls recursively
    /// </summary>
    private static void ApplyToChildren(Control.ControlCollection controls)
    {
        foreach (Control c in controls)
        {
            ApplyControlTheme(c);

            if (c.HasChildren)
            {
                ApplyToChildren(c.Controls);
            }
        }
    }

    /// <summary>
    /// Apply theme to a specific control based on its type
    /// </summary>
    private static void ApplyControlTheme(Control control)
    {
        // Apply base font
        control.Font = Fonts.Default;

        // Type-specific theming
        switch (control)
        {
            case GroupBox groupBox:
                groupBox.Font = Fonts.DefaultBold;
                groupBox.ForeColor = Colors.PrimaryText;
                groupBox.Padding = Spacing.GroupBoxPadding;
                break;

            case Panel panel:
                panel.Padding = Spacing.PanelPadding;
                break;

            case Label label when label.Name.StartsWith("lbl") && label.Text.EndsWith(":"):
                // Labels that act as field labels (e.g., "Name:")
                label.Font = Fonts.Default;
                label.ForeColor = Colors.PrimaryText;
                break;

            case Label label:
                label.Font = Fonts.Default;
                label.ForeColor = Colors.PrimaryText;
                break;

            case TextBox textBox:
                textBox.Font = Fonts.Default;
                textBox.BackColor = Colors.TextBoxBackground;
                break;

            case Button button:
                button.Font = Fonts.Default;
                button.BackColor = Colors.ButtonFace;
                break;

            case CheckBox checkBox:
                checkBox.Font = Fonts.Default;
                checkBox.ForeColor = Colors.PrimaryText;
                break;

            case RadioButton radioButton:
                radioButton.Font = Fonts.Default;
                radioButton.ForeColor = Colors.PrimaryText;
                break;

            case ComboBox comboBox:
                comboBox.Font = Fonts.Default;
                break;

            case NumericUpDown numericUpDown:
                numericUpDown.Font = Fonts.Default;
                break;

            case TabControl tabControl:
                tabControl.Font = Fonts.Default;
                break;
        }
    }

    // ══════════════════════════════════════════════════════
    // HELPER METHODS
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// Create a status message with appropriate color
    /// </summary>
    public static (string Message, Color Color) CreateStatusMessage(string message, StatusType type)
    {
        var color = type switch
        {
            StatusType.Success => Colors.Success,
            StatusType.Error => Colors.Error,
            StatusType.Warning => Colors.Warning,
            StatusType.Info => Colors.Info,
            _ => Colors.PrimaryText
        };

        return (message, color);
    }

    /// <summary>
    /// Apply monospace font to a TextBox (for secrets, connection strings, etc.)
    /// </summary>
    public static void ApplyMonospaceFont(TextBox textBox)
    {
        if (textBox is null) throw new ArgumentNullException(nameof(textBox));
        textBox.Font = Fonts.Monospace;
    }

    /// <summary>
    /// Mark a label as a section header
    /// </summary>
    public static void MakeSectionHeader(Label label)
    {
        if (label is null) throw new ArgumentNullException(nameof(label));
        label.Font = Fonts.Header;
        label.ForeColor = Colors.PrimaryText;
    }

    /// <summary>
    /// Status message types
    /// </summary>
    public enum StatusType
    {
        Normal,
        Success,
        Error,
        Warning,
        Info
    }
}
