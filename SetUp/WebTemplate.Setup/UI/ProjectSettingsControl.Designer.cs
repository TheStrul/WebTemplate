namespace WebTemplate.Setup.UI;

partial class ProjectSettingsControl
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    private void InitializeComponent()
    {
        lblProjectName = new Label();
        txtProjectName = new TextBox();
        lblTargetPath = new Label();
        txtTargetPath = new TextBox();
        btnBrowse = new Button();
        lblCompanyName = new Label();
        txtCompanyName = new TextBox();
        lblAuthorName = new Label();
        txtAuthorName = new TextBox();
        grpGitOptions = new GroupBox();
        chkInitializeGit = new CheckBox();
        chkCreateInitialCommit = new CheckBox();
        grpValidation = new GroupBox();
        chkRunValidation = new CheckBox();
        lblInfo = new Label();
        groupBoxGeneral = new GroupBox();
        btnGenerateValidValues = new Button();
        grpGitOptions.SuspendLayout();
        grpValidation.SuspendLayout();
        groupBoxGeneral.SuspendLayout();
        SuspendLayout();
        // 
        // lblProjectName
        // 
        lblProjectName.AutoSize = true;
        lblProjectName.Location = new Point(17, 35);
        lblProjectName.Name = "lblProjectName";
        lblProjectName.Size = new Size(87, 15);
        lblProjectName.TabIndex = 0;
        lblProjectName.Text = "Project Name:*";
        // 
        // txtProjectName
        // 
        txtProjectName.Location = new Point(150, 26);
        txtProjectName.Margin = new Padding(3, 2, 3, 2);
        txtProjectName.Name = "txtProjectName";
        txtProjectName.PlaceholderText = "e.g., MyAwesomeApp";
        txtProjectName.Size = new Size(350, 23);
        txtProjectName.TabIndex = 1;
        // 
        // lblTargetPath
        // 
        lblTargetPath.AutoSize = true;
        lblTargetPath.Location = new Point(17, 65);
        lblTargetPath.Name = "lblTargetPath";
        lblTargetPath.Size = new Size(75, 15);
        lblTargetPath.TabIndex = 2;
        lblTargetPath.Text = "Target Path:*";
        // 
        // txtTargetPath
        // 
        txtTargetPath.Location = new Point(150, 56);
        txtTargetPath.Margin = new Padding(3, 2, 3, 2);
        txtTargetPath.Name = "txtTargetPath";
        txtTargetPath.PlaceholderText = "C:\\Projects\\MyAwesomeApp";
        txtTargetPath.Size = new Size(438, 23);
        txtTargetPath.TabIndex = 3;
        // 
        // btnBrowse
        // 
        btnBrowse.Location = new Point(596, 55);
        btnBrowse.Margin = new Padding(3, 2, 3, 2);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(79, 22);
        btnBrowse.TabIndex = 4;
        btnBrowse.Text = "Browse...";
        btnBrowse.UseVisualStyleBackColor = true;
        // 
        // lblCompanyName
        // 
        lblCompanyName.AutoSize = true;
        lblCompanyName.Location = new Point(17, 95);
        lblCompanyName.Name = "lblCompanyName";
        lblCompanyName.Size = new Size(97, 15);
        lblCompanyName.TabIndex = 5;
        lblCompanyName.Text = "Company Name:";
        // 
        // txtCompanyName
        // 
        txtCompanyName.Location = new Point(150, 86);
        txtCompanyName.Margin = new Padding(3, 2, 3, 2);
        txtCompanyName.Name = "txtCompanyName";
        txtCompanyName.PlaceholderText = "Your Company Name";
        txtCompanyName.Size = new Size(350, 23);
        txtCompanyName.TabIndex = 6;
        // 
        // lblAuthorName
        // 
        lblAuthorName.AutoSize = true;
        lblAuthorName.Location = new Point(17, 125);
        lblAuthorName.Name = "lblAuthorName";
        lblAuthorName.Size = new Size(82, 15);
        lblAuthorName.TabIndex = 7;
        lblAuthorName.Text = "Author Name:";
        // 
        // txtAuthorName
        // 
        txtAuthorName.Location = new Point(150, 116);
        txtAuthorName.Margin = new Padding(3, 2, 3, 2);
        txtAuthorName.Name = "txtAuthorName";
        txtAuthorName.PlaceholderText = "Your Name";
        txtAuthorName.Size = new Size(350, 23);
        txtAuthorName.TabIndex = 8;
        // 
        // grpGitOptions
        // 
        grpGitOptions.Controls.Add(chkInitializeGit);
        grpGitOptions.Controls.Add(chkCreateInitialCommit);
        grpGitOptions.Dock = DockStyle.Top;
        grpGitOptions.Location = new Point(10, 172);
        grpGitOptions.Margin = new Padding(3, 2, 3, 2);
        grpGitOptions.Name = "grpGitOptions";
        grpGitOptions.Padding = new Padding(10);
        grpGitOptions.Size = new Size(737, 75);
        grpGitOptions.TabIndex = 9;
        grpGitOptions.TabStop = false;
        grpGitOptions.Text = "Git Options";
        // 
        // chkInitializeGit
        // 
        chkInitializeGit.AutoSize = true;
        chkInitializeGit.Checked = true;
        chkInitializeGit.CheckState = CheckState.Checked;
        chkInitializeGit.Location = new Point(25, 30);
        chkInitializeGit.Margin = new Padding(3, 2, 3, 2);
        chkInitializeGit.Name = "chkInitializeGit";
        chkInitializeGit.Size = new Size(188, 19);
        chkInitializeGit.TabIndex = 0;
        chkInitializeGit.Text = "Initialize Git repository (git init)";
        chkInitializeGit.UseVisualStyleBackColor = true;
        // 
        // chkCreateInitialCommit
        // 
        chkCreateInitialCommit.AutoSize = true;
        chkCreateInitialCommit.Checked = true;
        chkCreateInitialCommit.CheckState = CheckState.Checked;
        chkCreateInitialCommit.Location = new Point(25, 53);
        chkCreateInitialCommit.Margin = new Padding(3, 2, 3, 2);
        chkCreateInitialCommit.Name = "chkCreateInitialCommit";
        chkCreateInitialCommit.Size = new Size(266, 19);
        chkCreateInitialCommit.TabIndex = 1;
        chkCreateInitialCommit.Text = "Create initial commit (git add . && git commit)";
        chkCreateInitialCommit.UseVisualStyleBackColor = true;
        // 
        // grpValidation
        // 
        grpValidation.Controls.Add(chkRunValidation);
        grpValidation.Dock = DockStyle.Top;
        grpValidation.Location = new Point(10, 247);
        grpValidation.Margin = new Padding(3, 2, 3, 2);
        grpValidation.Name = "grpValidation";
        grpValidation.Padding = new Padding(10);
        grpValidation.Size = new Size(737, 52);
        grpValidation.TabIndex = 10;
        grpValidation.TabStop = false;
        grpValidation.Text = "Post-Generation";
        // 
        // chkRunValidation
        // 
        chkRunValidation.AutoSize = true;
        chkRunValidation.Checked = true;
        chkRunValidation.CheckState = CheckState.Checked;
        chkRunValidation.Location = new Point(25, 30);
        chkRunValidation.Margin = new Padding(3, 2, 3, 2);
        chkRunValidation.Name = "chkRunValidation";
        chkRunValidation.Size = new Size(248, 19);
        chkRunValidation.TabIndex = 0;
        chkRunValidation.Text = "Run DbContext validation after generation";
        chkRunValidation.UseVisualStyleBackColor = true;
        // 
        // lblInfo
        // 
        lblInfo.AutoSize = true;
        lblInfo.Dock = DockStyle.Top;
        lblInfo.ForeColor = SystemColors.GrayText;
        lblInfo.Location = new Point(10, 299);
        lblInfo.Name = "lblInfo";
        lblInfo.Size = new Size(471, 15);
        lblInfo.TabIndex = 11;
        lblInfo.Text = "* Required fields. Project name must be a valid C# identifier (letters, digits, underscores).";
        // 
        // groupBoxGeneral
        // 
        groupBoxGeneral.Controls.Add(lblProjectName);
        groupBoxGeneral.Controls.Add(txtProjectName);
        groupBoxGeneral.Controls.Add(lblTargetPath);
        groupBoxGeneral.Controls.Add(txtTargetPath);
        groupBoxGeneral.Controls.Add(txtAuthorName);
        groupBoxGeneral.Controls.Add(btnBrowse);
        groupBoxGeneral.Controls.Add(lblAuthorName);
        groupBoxGeneral.Controls.Add(lblCompanyName);
        groupBoxGeneral.Controls.Add(txtCompanyName);
        groupBoxGeneral.Dock = DockStyle.Top;
        groupBoxGeneral.Location = new Point(10, 10);
        groupBoxGeneral.Name = "groupBoxGeneral";
        groupBoxGeneral.Padding = new Padding(10);
        groupBoxGeneral.Size = new Size(737, 162);
        groupBoxGeneral.TabIndex = 12;
        groupBoxGeneral.TabStop = false;
        groupBoxGeneral.Text = "General";
        // 
        // btnGenerateValidValues
        // 
        btnGenerateValidValues.Dock = DockStyle.Top;
        btnGenerateValidValues.Location = new Point(10, 314);
        btnGenerateValidValues.Name = "btnGenerateValidValues";
        btnGenerateValidValues.Size = new Size(737, 35);
        btnGenerateValidValues.TabIndex = 13;
        btnGenerateValidValues.Text = "Generate Valid Values";
        btnGenerateValidValues.UseVisualStyleBackColor = true;
        btnGenerateValidValues.Click += btnGenerateValidValues_Click;
        // 
        // ProjectSettingsControl
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(btnGenerateValidValues);
        Controls.Add(lblInfo);
        Controls.Add(grpValidation);
        Controls.Add(grpGitOptions);
        Controls.Add(groupBoxGeneral);
        Margin = new Padding(3, 2, 3, 2);
        Name = "ProjectSettingsControl";
        Padding = new Padding(10);
        Size = new Size(757, 589);
        grpGitOptions.ResumeLayout(false);
        grpGitOptions.PerformLayout();
        grpValidation.ResumeLayout(false);
        grpValidation.PerformLayout();
        groupBoxGeneral.ResumeLayout(false);
        groupBoxGeneral.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
    private Label lblProjectName;
    private TextBox txtProjectName;
    private Label lblTargetPath;
    private TextBox txtTargetPath;
    private Button btnBrowse;
    private Label lblCompanyName;
    private TextBox txtCompanyName;
    private Label lblAuthorName;
    private TextBox txtAuthorName;
    private GroupBox grpGitOptions;
    private CheckBox chkInitializeGit;
    private CheckBox chkCreateInitialCommit;
    private GroupBox grpValidation;
    private CheckBox chkRunValidation;
    private Label lblInfo;
    private GroupBox groupBoxGeneral;
    private Button btnGenerateValidValues;
}
