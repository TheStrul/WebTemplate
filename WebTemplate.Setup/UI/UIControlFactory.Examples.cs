using WebTemplate.Setup.UI;

namespace WebTemplate.Setup.Examples;

/// <summary>
/// Demonstrates how to use UIControlFactory to create controls with centralized theme
/// ALL control definitions are retrieved from a single location based on theme
/// </summary>
public class DemoControl : UserControl
{
    public DemoControl()
    {
        BuildUIFromFactory();
    }

    /// <summary>
    /// Build the entire UI using the control factory
    /// Every control gets its definition from the centralized theme
    /// </summary>
    private void BuildUIFromFactory()
    {
        // ═══════════════════════════════════════════════════════════════
        // EXAMPLE 1: Create individual controls
        // ═══════════════════════════════════════════════════════════════

        // Create a standard TextBox - gets font, size, colors from theme
        var txtName = UIControlFactory.CreateTextBox(
            UIControlFactory.ControlType.StandardTextBox,
            "txtName"
        );

        // Create a monospace TextBox - perfect for connection strings, secrets
        var txtConnectionString = UIControlFactory.CreateTextBox(
            UIControlFactory.ControlType.MonospaceTextBox,
            "txtConnectionString"
        );

        // Create a password TextBox - masked input
        var txtPassword = UIControlFactory.CreateTextBox(
            UIControlFactory.ControlType.PasswordTextBox,
            "txtPassword"
        );

        // ═══════════════════════════════════════════════════════════════
        // EXAMPLE 2: Create buttons with different styles
        // ═══════════════════════════════════════════════════════════════

        // Primary button - default action
        var btnSave = UIControlFactory.CreateButton(
            UIControlFactory.ControlType.PrimaryButton,
            "btnSave",
            "Save"
        );

        // Secondary button - cancel/back actions
        var btnCancel = UIControlFactory.CreateButton(
            UIControlFactory.ControlType.SecondaryButton,
            "btnCancel",
            "Cancel"
        );

        // Danger button - destructive actions (delete, remove)
        var btnDelete = UIControlFactory.CreateButton(
            UIControlFactory.ControlType.DangerButton,
            "btnDelete",
            "Delete"
        );

        // ═══════════════════════════════════════════════════════════════
        // EXAMPLE 3: Create labels with different purposes
        // ═══════════════════════════════════════════════════════════════

        // Header label - section titles
        var lblHeader = UIControlFactory.CreateLabel(
            UIControlFactory.ControlType.HeaderLabel,
            "lblHeader",
            "Configuration Settings"
        );

        // Field label - next to input fields
        var lblName = UIControlFactory.CreateLabel(
            UIControlFactory.ControlType.FieldLabel,
            "lblName",
            "Project Name:"
        );

        // Error label - validation messages
        var lblError = UIControlFactory.CreateLabel(
            UIControlFactory.ControlType.ErrorLabel,
            "lblError",
            "This field is required"
        );

        // Success label - confirmation messages
        var lblSuccess = UIControlFactory.CreateLabel(
            UIControlFactory.ControlType.SuccessLabel,
            "lblSuccess",
            "Configuration saved successfully!"
        );

        // ═══════════════════════════════════════════════════════════════
        // EXAMPLE 4: Create containers
        // ═══════════════════════════════════════════════════════════════

        // GroupBox - groups related controls
        var grpDatabase = UIControlFactory.CreateGroupBox(
            "grpDatabase",
            "Database Settings"
        );

        // Panel - flexible container
        var pnlSettings = UIControlFactory.Create(
            UIControlFactory.ControlType.Panel,
            "pnlSettings"
        );

        // ═══════════════════════════════════════════════════════════════
        // EXAMPLE 5: Create field rows (Label + TextBox together)
        // ═══════════════════════════════════════════════════════════════

        // Standard field
        var (lblProjectName, txtProjectName) = UIControlFactory.CreateFieldRow(
            "Project Name",
            "ProjectName"
        );

        // Monospace field for technical data
        var (lblApiKey, txtApiKey) = UIControlFactory.CreateFieldRow(
            "API Key",
            "ApiKey",
            UIControlFactory.ControlType.MonospaceTextBox
        );

        // Password field
        var (lblDbPassword, txtDbPassword) = UIControlFactory.CreateFieldRow(
            "Database Password",
            "DbPassword",
            UIControlFactory.ControlType.PasswordTextBox
        );

        // ═══════════════════════════════════════════════════════════════
        // EXAMPLE 6: Create other controls
        // ═══════════════════════════════════════════════════════════════

        // CheckBox
        var chkEnabled = UIControlFactory.Create(
            UIControlFactory.ControlType.CheckBox,
            "chkEnabled",
            "Enable this feature"
        );

        // RadioButton
        var rdoOption1 = UIControlFactory.Create(
            UIControlFactory.ControlType.RadioButton,
            "rdoOption1",
            "Option 1"
        );

        // NumericUpDown
        var numTimeout = UIControlFactory.CreateNumericUpDown("numTimeout", 30);

        // ComboBox
        var cboStrategy = UIControlFactory.Create(
            UIControlFactory.ControlType.ComboBox,
            "cboStrategy"
        );

        // Browse button
        var btnBrowse = UIControlFactory.CreateBrowseButton("btnBrowse");

        // Validation label
        var lblValidation = UIControlFactory.CreateValidationLabel("lblValidation");

        // ═══════════════════════════════════════════════════════════════
        // EXAMPLE 7: Build a complete form section
        // ═══════════════════════════════════════════════════════════════

        var formSection = BuildDatabaseConfigSection();

        // Add to your layout
        // this.Controls.Add(formSection);
    }

    /// <summary>
    /// Example: Build a complete section using only the factory
    /// EVERY control comes from the centralized theme definition
    /// </summary>
    private GroupBox BuildDatabaseConfigSection()
    {
        // Create the container
        var grpDatabase = UIControlFactory.CreateGroupBox("grpDatabase", "Database Configuration");
        grpDatabase.Size = new Size(500, 250);

        // Create a panel for layout
        var pnlContent = UIControlFactory.Create(UIControlFactory.ControlType.Panel, "pnlContent") as Panel;
        pnlContent!.Dock = DockStyle.Fill;

        // Create fields using factory
        var (lblServer, txtServer) = UIControlFactory.CreateFieldRow("Server", "Server");
        var (lblDatabase, txtDatabase) = UIControlFactory.CreateFieldRow("Database", "Database");
        var (lblUserId, txtUserId) = UIControlFactory.CreateFieldRow("User ID", "UserId");
        var (lblPassword, txtPassword) = UIControlFactory.CreateFieldRow("Password", "Password", UIControlFactory.ControlType.PasswordTextBox);

        // Create connection string field (monospace)
        var (lblConnString, txtConnString) = UIControlFactory.CreateFieldRow("Connection String", "ConnectionString", UIControlFactory.ControlType.MonospaceTextBox);

        // Create checkbox
        var chkIntegratedSecurity = UIControlFactory.Create(
            UIControlFactory.ControlType.CheckBox,
            "chkIntegratedSecurity",
            "Use Integrated Security"
        ) as CheckBox;

        // Create buttons
        var btnTestConnection = UIControlFactory.CreateButton(
            UIControlFactory.ControlType.PrimaryButton,
            "btnTestConnection",
            "Test Connection"
        );

        var btnBuildConnectionString = UIControlFactory.CreateButton(
            UIControlFactory.ControlType.SecondaryButton,
            "btnBuildConnectionString",
            "Build Connection String"
        );

        // Create validation label
        var lblValidation = UIControlFactory.CreateValidationLabel("lblValidation");

        // Layout controls (simple vertical stack for demo)
        int y = 10;
        int labelX = 10;
        int controlX = 140;
        int spacing = 30;

        // Position Server
        lblServer.Location = new Point(labelX, y);
        txtServer.Location = new Point(controlX, y);
        y += spacing;

        // Position Database
        lblDatabase.Location = new Point(labelX, y);
        txtDatabase.Location = new Point(controlX, y);
        y += spacing;

        // Position User ID
        lblUserId.Location = new Point(labelX, y);
        txtUserId.Location = new Point(controlX, y);
        y += spacing;

        // Position Password
        lblPassword.Location = new Point(labelX, y);
        txtPassword.Location = new Point(controlX, y);
        y += spacing;

        // Position Integrated Security checkbox
        chkIntegratedSecurity!.Location = new Point(controlX, y);
        y += spacing;

        // Position Connection String
        lblConnString.Location = new Point(labelX, y);
        txtConnString.Location = new Point(controlX, y);
        y += spacing + 10;

        // Position buttons
        btnTestConnection.Location = new Point(controlX, y);
        btnBuildConnectionString.Location = new Point(controlX + 110, y);
        y += 40;

        // Position validation label
        lblValidation.Location = new Point(controlX, y);

        // Add all controls to panel
        pnlContent.Controls.AddRange(new Control[]
        {
            lblServer, txtServer,
            lblDatabase, txtDatabase,
            lblUserId, txtUserId,
            lblPassword, txtPassword,
            chkIntegratedSecurity,
            lblConnString, txtConnString,
            btnTestConnection, btnBuildConnectionString,
            lblValidation
        });

        // Add panel to group box
        grpDatabase.Controls.Add(pnlContent);

        return grpDatabase;
    }

    /// <summary>
    /// Example: Show validation errors using factory-created labels
    /// </summary>
    private void ShowValidationError(Label errorLabel, string message)
    {
        errorLabel.Text = message;
        errorLabel.Visible = true;
    }

    /// <summary>
    /// Example: Clear validation errors
    /// </summary>
    private void ClearValidationError(Label errorLabel)
    {
        errorLabel.Text = "";
        errorLabel.Visible = false;
    }

    /// <summary>
    /// Example: Dynamic control creation based on configuration
    /// </summary>
    private void BuildDynamicForm(List<FieldDefinition> fields)
    {
        var panel = new Panel { AutoScroll = true, Dock = DockStyle.Fill };
        int y = 10;

        foreach (var field in fields)
        {
            // Determine control type based on field type
            var controlType = field.IsSecret
                ? UIControlFactory.ControlType.PasswordTextBox
                : field.IsTechnical
                    ? UIControlFactory.ControlType.MonospaceTextBox
                    : UIControlFactory.ControlType.StandardTextBox;

            // Create field row from factory
            var (label, textBox) = UIControlFactory.CreateFieldRow(
                field.DisplayName,
                field.Name,
                controlType
            );

            // Position controls
            label.Location = new Point(10, y);
            textBox.Location = new Point(140, y);

            // Add to panel
            panel.Controls.Add(label);
            panel.Controls.Add(textBox);

            y += 30;
        }

        this.Controls.Add(panel);
    }
}

/// <summary>
/// Example field definition for dynamic form building
/// </summary>
public class FieldDefinition
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsSecret { get; set; }
    public bool IsTechnical { get; set; }
}
