using System.Drawing;
using System.Windows.Forms;

namespace WebTemplate.Setup.UI;

/// <summary>
/// Central factory for creating UI controls with pre-configured properties from theme
/// This is the SINGLE LOCATION where ALL control definitions are retrieved
/// </summary>
public static class UIControlFactory
{
    // ══════════════════════════════════════════════════════
    // CONTROL DEFINITIONS - Based on Theme
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// Control type definitions that can be created
    /// </summary>
    public enum ControlType
    {
        // Text Input
        StandardTextBox,
        MonospaceTextBox,
        MultilineTextBox,
        PasswordTextBox,

        // Numeric Input
        NumericUpDown,

        // Buttons
        PrimaryButton,
        SecondaryButton,
        DangerButton,
        IconButton,

        // Labels
        StandardLabel,
        FieldLabel,
        HeaderLabel,
        SubHeaderLabel,
        StatusLabel,
        ErrorLabel,
        SuccessLabel,

        // Checkboxes & Radio
        CheckBox,
        RadioButton,

        // Containers
        GroupBox,
        Panel,

        // Dropdowns
        ComboBox
    }

    // ══════════════════════════════════════════════════════
    // FACTORY METHODS - Create controls with theme applied
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// Create a control of specified type with all theme properties applied
    /// </summary>
    public static Control Create(ControlType type, string name = "", string text = "")
    {
        var control = CreateControlInstance(type);

        if (!string.IsNullOrEmpty(name))
            control.Name = name;

        if (!string.IsNullOrEmpty(text))
            control.Text = text;

        ApplyThemeDefinition(control, type);

        return control;
    }

    /// <summary>
    /// Create a TextBox with theme-based configuration
    /// </summary>
    public static TextBox CreateTextBox(ControlType type = ControlType.StandardTextBox, string name = "", string text = "")
    {
        return (TextBox)Create(type, name, text);
    }

    /// <summary>
    /// Create a Button with theme-based configuration
    /// </summary>
    public static Button CreateButton(ControlType type = ControlType.PrimaryButton, string name = "", string text = "")
    {
        return (Button)Create(type, name, text);
    }

    /// <summary>
    /// Create a Label with theme-based configuration
    /// </summary>
    public static Label CreateLabel(ControlType type = ControlType.StandardLabel, string name = "", string text = "")
    {
        return (Label)Create(type, name, text);
    }

    /// <summary>
    /// Create a GroupBox with theme-based configuration
    /// </summary>
    public static GroupBox CreateGroupBox(string name = "", string text = "")
    {
        return (GroupBox)Create(ControlType.GroupBox, name, text);
    }

    /// <summary>
    /// Create a NumericUpDown with theme-based configuration
    /// </summary>
    public static NumericUpDown CreateNumericUpDown(string name = "", decimal value = 0)
    {
        var control = (NumericUpDown)Create(ControlType.NumericUpDown, name);
        control.Value = value;
        return control;
    }

    // ══════════════════════════════════════════════════════
    // INTERNAL FACTORY LOGIC
    // ══════════════════════════════════════════════════════

    private static Control CreateControlInstance(ControlType type)
    {
        return type switch
        {
            // TextBoxes
            ControlType.StandardTextBox => new TextBox(),
            ControlType.MonospaceTextBox => new TextBox(),
            ControlType.MultilineTextBox => new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical },
            ControlType.PasswordTextBox => new TextBox { UseSystemPasswordChar = true },

            // Numeric
            ControlType.NumericUpDown => new NumericUpDown(),

            // Buttons
            ControlType.PrimaryButton => new Button(),
            ControlType.SecondaryButton => new Button(),
            ControlType.DangerButton => new Button(),
            ControlType.IconButton => new Button(),

            // Labels
            ControlType.StandardLabel => new Label(),
            ControlType.FieldLabel => new Label { AutoSize = true },
            ControlType.HeaderLabel => new Label { AutoSize = true },
            ControlType.SubHeaderLabel => new Label { AutoSize = true },
            ControlType.StatusLabel => new Label { AutoSize = true },
            ControlType.ErrorLabel => new Label { AutoSize = true },
            ControlType.SuccessLabel => new Label { AutoSize = true },

            // Checkboxes & Radio
            ControlType.CheckBox => new CheckBox { AutoSize = true },
            ControlType.RadioButton => new RadioButton { AutoSize = true },

            // Containers
            ControlType.GroupBox => new GroupBox(),
            ControlType.Panel => new Panel(),

            // Dropdowns
            ControlType.ComboBox => new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList },

            _ => throw new ArgumentException($"Unknown control type: {type}")
        };
    }

    private static void ApplyThemeDefinition(Control control, ControlType type)
    {
        switch (type)
        {
            // ═══════════════════════════════════════════
            // TEXT INPUT CONTROLS
            // ═══════════════════════════════════════════

            case ControlType.StandardTextBox:
                ApplyStandardTextBoxTheme((TextBox)control);
                break;

            case ControlType.MonospaceTextBox:
                ApplyMonospaceTextBoxTheme((TextBox)control);
                break;

            case ControlType.MultilineTextBox:
                ApplyMultilineTextBoxTheme((TextBox)control);
                break;

            case ControlType.PasswordTextBox:
                ApplyPasswordTextBoxTheme((TextBox)control);
                break;

            // ═══════════════════════════════════════════
            // NUMERIC CONTROLS
            // ═══════════════════════════════════════════

            case ControlType.NumericUpDown:
                ApplyNumericUpDownTheme((NumericUpDown)control);
                break;

            // ═══════════════════════════════════════════
            // BUTTON CONTROLS
            // ═══════════════════════════════════════════

            case ControlType.PrimaryButton:
                ApplyPrimaryButtonTheme((Button)control);
                break;

            case ControlType.SecondaryButton:
                ApplySecondaryButtonTheme((Button)control);
                break;

            case ControlType.DangerButton:
                ApplyDangerButtonTheme((Button)control);
                break;

            case ControlType.IconButton:
                ApplyIconButtonTheme((Button)control);
                break;

            // ═══════════════════════════════════════════
            // LABEL CONTROLS
            // ═══════════════════════════════════════════

            case ControlType.StandardLabel:
                ApplyStandardLabelTheme((Label)control);
                break;

            case ControlType.FieldLabel:
                ApplyFieldLabelTheme((Label)control);
                break;

            case ControlType.HeaderLabel:
                ApplyHeaderLabelTheme((Label)control);
                break;

            case ControlType.SubHeaderLabel:
                ApplySubHeaderLabelTheme((Label)control);
                break;

            case ControlType.StatusLabel:
                ApplyStatusLabelTheme((Label)control);
                break;

            case ControlType.ErrorLabel:
                ApplyErrorLabelTheme((Label)control);
                break;

            case ControlType.SuccessLabel:
                ApplySuccessLabelTheme((Label)control);
                break;

            // ═══════════════════════════════════════════
            // CHECKBOX & RADIO CONTROLS
            // ═══════════════════════════════════════════

            case ControlType.CheckBox:
                ApplyCheckBoxTheme((CheckBox)control);
                break;

            case ControlType.RadioButton:
                ApplyRadioButtonTheme((RadioButton)control);
                break;

            // ═══════════════════════════════════════════
            // CONTAINER CONTROLS
            // ═══════════════════════════════════════════

            case ControlType.GroupBox:
                ApplyGroupBoxTheme((GroupBox)control);
                break;

            case ControlType.Panel:
                ApplyPanelTheme((Panel)control);
                break;

            // ═══════════════════════════════════════════
            // DROPDOWN CONTROLS
            // ═══════════════════════════════════════════

            case ControlType.ComboBox:
                ApplyComboBoxTheme((ComboBox)control);
                break;
        }
    }

    // ══════════════════════════════════════════════════════
    // THEME APPLICATION METHODS - TextBoxes
    // ══════════════════════════════════════════════════════

    private static void ApplyStandardTextBoxTheme(TextBox textBox)
    {
        textBox.Font = UITheme.Fonts.Default;
        textBox.BackColor = UITheme.Colors.TextBoxBackground;
        textBox.ForeColor = UITheme.Colors.PrimaryText;
        textBox.Size = new Size(UITheme.Sizes.TextBoxWidth, UITheme.Sizes.TextBoxHeight);
        textBox.Margin = UITheme.Spacing.ControlMargin;
    }

    private static void ApplyMonospaceTextBoxTheme(TextBox textBox)
    {
        textBox.Font = UITheme.Fonts.Monospace;
        textBox.BackColor = UITheme.Colors.TextBoxBackground;
        textBox.ForeColor = UITheme.Colors.PrimaryText;
        textBox.Size = new Size(UITheme.Sizes.TextBoxWidthLarge, UITheme.Sizes.TextBoxHeight);
        textBox.Margin = UITheme.Spacing.ControlMargin;
    }

    private static void ApplyMultilineTextBoxTheme(TextBox textBox)
    {
        textBox.Font = UITheme.Fonts.Default;
        textBox.BackColor = UITheme.Colors.TextBoxBackground;
        textBox.ForeColor = UITheme.Colors.PrimaryText;
        textBox.Size = new Size(UITheme.Sizes.TextBoxWidthLarge, 100); // Height for multiline
        textBox.Margin = UITheme.Spacing.ControlMargin;
    }

    private static void ApplyPasswordTextBoxTheme(TextBox textBox)
    {
        textBox.Font = UITheme.Fonts.Default;
        textBox.BackColor = UITheme.Colors.TextBoxBackground;
        textBox.ForeColor = UITheme.Colors.PrimaryText;
        textBox.Size = new Size(UITheme.Sizes.TextBoxWidth, UITheme.Sizes.TextBoxHeight);
        textBox.Margin = UITheme.Spacing.ControlMargin;
    }

    // ══════════════════════════════════════════════════════
    // THEME APPLICATION METHODS - Numeric Controls
    // ══════════════════════════════════════════════════════

    private static void ApplyNumericUpDownTheme(NumericUpDown numericUpDown)
    {
        numericUpDown.Font = UITheme.Fonts.Default;
        numericUpDown.Size = new Size(100, UITheme.Sizes.NumericUpDownHeight);
        numericUpDown.Margin = UITheme.Spacing.ControlMargin;
    }

    // ══════════════════════════════════════════════════════
    // THEME APPLICATION METHODS - Buttons
    // ══════════════════════════════════════════════════════

    private static void ApplyPrimaryButtonTheme(Button button)
    {
        button.Font = UITheme.Fonts.Default;
        button.BackColor = UITheme.Colors.ButtonFace;
        button.ForeColor = UITheme.Colors.PrimaryText;
        button.Size = UITheme.Sizes.ButtonMedium;
        button.Margin = UITheme.Spacing.ControlMargin;
        button.FlatStyle = FlatStyle.Standard;
    }

    private static void ApplySecondaryButtonTheme(Button button)
    {
        button.Font = UITheme.Fonts.Default;
        button.BackColor = SystemColors.Control;
        button.ForeColor = UITheme.Colors.PrimaryText;
        button.Size = UITheme.Sizes.ButtonMedium;
        button.Margin = UITheme.Spacing.ControlMargin;
        button.FlatStyle = FlatStyle.Standard;
    }

    private static void ApplyDangerButtonTheme(Button button)
    {
        button.Font = UITheme.Fonts.DefaultBold;
        button.BackColor = UITheme.Colors.Error;
        button.ForeColor = Color.White;
        button.Size = UITheme.Sizes.ButtonMedium;
        button.Margin = UITheme.Spacing.ControlMargin;
        button.FlatStyle = FlatStyle.Standard;
    }

    private static void ApplyIconButtonTheme(Button button)
    {
        button.Font = UITheme.Fonts.Default;
        button.BackColor = UITheme.Colors.ButtonFace;
        button.Size = UITheme.Sizes.ButtonIcon;
        button.Margin = UITheme.Spacing.ControlMargin;
        button.FlatStyle = FlatStyle.Standard;
    }

    // ══════════════════════════════════════════════════════
    // THEME APPLICATION METHODS - Labels
    // ══════════════════════════════════════════════════════

    private static void ApplyStandardLabelTheme(Label label)
    {
        label.Font = UITheme.Fonts.Default;
        label.ForeColor = UITheme.Colors.PrimaryText;
        label.Margin = UITheme.Spacing.ControlMargin;
        label.AutoSize = true;
    }

    private static void ApplyFieldLabelTheme(Label label)
    {
        label.Font = UITheme.Fonts.Default;
        label.ForeColor = UITheme.Colors.PrimaryText;
        label.Margin = UITheme.Spacing.ControlMargin;
        label.AutoSize = true;
        label.TextAlign = ContentAlignment.MiddleLeft;
    }

    private static void ApplyHeaderLabelTheme(Label label)
    {
        label.Font = UITheme.Fonts.Header;
        label.ForeColor = UITheme.Colors.PrimaryText;
        label.Margin = new Padding(0, 0, 0, UITheme.Spacing.MediumGap);
        label.AutoSize = true;
    }

    private static void ApplySubHeaderLabelTheme(Label label)
    {
        label.Font = UITheme.Fonts.SubHeader;
        label.ForeColor = UITheme.Colors.PrimaryText;
        label.Margin = new Padding(0, 0, 0, UITheme.Spacing.SmallGap);
        label.AutoSize = true;
    }

    private static void ApplyStatusLabelTheme(Label label)
    {
        label.Font = UITheme.Fonts.StatusBar;
        label.ForeColor = UITheme.Colors.PrimaryText;
        label.AutoSize = true;
    }

    private static void ApplyErrorLabelTheme(Label label)
    {
        label.Font = UITheme.Fonts.Default;
        label.ForeColor = UITheme.Colors.Error;
        label.Margin = UITheme.Spacing.ControlMargin;
        label.AutoSize = true;
    }

    private static void ApplySuccessLabelTheme(Label label)
    {
        label.Font = UITheme.Fonts.Default;
        label.ForeColor = UITheme.Colors.Success;
        label.Margin = UITheme.Spacing.ControlMargin;
        label.AutoSize = true;
    }

    // ══════════════════════════════════════════════════════
    // THEME APPLICATION METHODS - CheckBox & RadioButton
    // ══════════════════════════════════════════════════════

    private static void ApplyCheckBoxTheme(CheckBox checkBox)
    {
        checkBox.Font = UITheme.Fonts.Default;
        checkBox.ForeColor = UITheme.Colors.PrimaryText;
        checkBox.Margin = UITheme.Spacing.ControlMargin;
    }

    private static void ApplyRadioButtonTheme(RadioButton radioButton)
    {
        radioButton.Font = UITheme.Fonts.Default;
        radioButton.ForeColor = UITheme.Colors.PrimaryText;
        radioButton.Margin = UITheme.Spacing.ControlMargin;
    }

    // ══════════════════════════════════════════════════════
    // THEME APPLICATION METHODS - Containers
    // ══════════════════════════════════════════════════════

    private static void ApplyGroupBoxTheme(GroupBox groupBox)
    {
        groupBox.Font = UITheme.Fonts.DefaultBold;
        groupBox.ForeColor = UITheme.Colors.PrimaryText;
        groupBox.Padding = UITheme.Spacing.GroupBoxPadding;
        groupBox.Margin = new Padding(0, 0, 0, UITheme.Spacing.GroupToGroup);
    }

    private static void ApplyPanelTheme(Panel panel)
    {
        panel.BackColor = UITheme.Colors.PanelBackground;
        panel.Padding = UITheme.Spacing.PanelPadding;
        panel.Margin = UITheme.Spacing.PanelMargin;
    }

    // ══════════════════════════════════════════════════════
    // THEME APPLICATION METHODS - Dropdowns
    // ══════════════════════════════════════════════════════

    private static void ApplyComboBoxTheme(ComboBox comboBox)
    {
        comboBox.Font = UITheme.Fonts.Default;
        comboBox.Size = new Size(UITheme.Sizes.TextBoxWidth, UITheme.Sizes.ComboBoxHeight);
        comboBox.Margin = UITheme.Spacing.ControlMargin;
    }

    // ══════════════════════════════════════════════════════
    // CONVENIENCE METHODS FOR COMMON PATTERNS
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// Create a field row (Label + TextBox) with proper spacing and alignment
    /// </summary>
    public static (Label Label, TextBox TextBox) CreateFieldRow(string labelText, string textBoxName, ControlType textBoxType = ControlType.StandardTextBox)
    {
        var label = CreateLabel(ControlType.FieldLabel, $"lbl{textBoxName}", labelText + ":");
        label.Width = UITheme.Sizes.LabelWidthMedium;
        label.TextAlign = ContentAlignment.MiddleLeft;

        var textBox = CreateTextBox(textBoxType, textBoxName);

        return (label, textBox);
    }

    /// <summary>
    /// Create a browse button for file/folder selection
    /// </summary>
    public static Button CreateBrowseButton(string name = "btnBrowse")
    {
        var button = CreateButton(ControlType.SecondaryButton, name, "Browse...");
        button.Size = new Size(80, 23);
        return button;
    }

    /// <summary>
    /// Create a validation error label (hidden by default)
    /// </summary>
    public static Label CreateValidationLabel(string name = "lblError")
    {
        var label = CreateLabel(ControlType.ErrorLabel, name, "");
        label.Visible = false;
        return label;
    }
}
