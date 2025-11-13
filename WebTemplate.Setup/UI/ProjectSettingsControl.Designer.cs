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
        this.lblProjectName = new Label();
        this.txtProjectName = new TextBox();
        this.lblTargetPath = new Label();
        this.txtTargetPath = new TextBox();
        this.btnBrowse = new Button();
        this.lblCompanyName = new Label();
        this.txtCompanyName = new TextBox();
        this.lblAuthorName = new Label();
        this.txtAuthorName = new TextBox();
        this.grpGitOptions = new GroupBox();
        this.chkInitializeGit = new CheckBox();
        this.chkCreateInitialCommit = new CheckBox();
        this.grpValidation = new GroupBox();
        this.chkRunValidation = new CheckBox();
        this.lblInfo = new Label();

        this.grpGitOptions.SuspendLayout();
        this.grpValidation.SuspendLayout();
        this.SuspendLayout();

        //
        // lblProjectName
        //
        this.lblProjectName.AutoSize = true;
        this.lblProjectName.Location = new Point(20, 20);
        this.lblProjectName.Name = "lblProjectName";
        this.lblProjectName.Size = new Size(110, 20);
        this.lblProjectName.TabIndex = 0;
        this.lblProjectName.Text = "Project Name:*";

        //
        // txtProjectName
        //
        this.txtProjectName.Location = new Point(180, 17);
        this.txtProjectName.Name = "txtProjectName";
        this.txtProjectName.Size = new Size(400, 27);
        this.txtProjectName.TabIndex = 1;
        this.txtProjectName.PlaceholderText = "e.g., MyAwesomeApp";

        //
        // lblTargetPath
        //
        this.lblTargetPath.AutoSize = true;
        this.lblTargetPath.Location = new Point(20, 60);
        this.lblTargetPath.Name = "lblTargetPath";
        this.lblTargetPath.Size = new Size(95, 20);
        this.lblTargetPath.TabIndex = 2;
        this.lblTargetPath.Text = "Target Path:*";

        //
        // txtTargetPath
        //
        this.txtTargetPath.Location = new Point(180, 57);
        this.txtTargetPath.Name = "txtTargetPath";
        this.txtTargetPath.Size = new Size(500, 27);
        this.txtTargetPath.TabIndex = 3;
        this.txtTargetPath.PlaceholderText = "C:\\Projects\\MyAwesomeApp";

        //
        // btnBrowse
        //
        this.btnBrowse.Location = new Point(690, 56);
        this.btnBrowse.Name = "btnBrowse";
        this.btnBrowse.Size = new Size(90, 29);
        this.btnBrowse.TabIndex = 4;
        this.btnBrowse.Text = "Browse...";
        this.btnBrowse.UseVisualStyleBackColor = true;

        //
        // lblCompanyName
        //
        this.lblCompanyName.AutoSize = true;
        this.lblCompanyName.Location = new Point(20, 100);
        this.lblCompanyName.Name = "lblCompanyName";
        this.lblCompanyName.Size = new Size(125, 20);
        this.lblCompanyName.TabIndex = 5;
        this.lblCompanyName.Text = "Company Name:";

        //
        // txtCompanyName
        //
        this.txtCompanyName.Location = new Point(180, 97);
        this.txtCompanyName.Name = "txtCompanyName";
        this.txtCompanyName.Size = new Size(400, 27);
        this.txtCompanyName.TabIndex = 6;
        this.txtCompanyName.PlaceholderText = "Your Company Name";

        //
        // lblAuthorName
        //
        this.lblAuthorName.AutoSize = true;
        this.lblAuthorName.Location = new Point(20, 140);
        this.lblAuthorName.Name = "lblAuthorName";
        this.lblAuthorName.Size = new Size(108, 20);
        this.lblAuthorName.TabIndex = 7;
        this.lblAuthorName.Text = "Author Name:";

        //
        // txtAuthorName
        //
        this.txtAuthorName.Location = new Point(180, 137);
        this.txtAuthorName.Name = "txtAuthorName";
        this.txtAuthorName.Size = new Size(400, 27);
        this.txtAuthorName.TabIndex = 8;
        this.txtAuthorName.PlaceholderText = "Your Name";

        //
        // grpGitOptions
        //
        this.grpGitOptions.Controls.Add(this.chkInitializeGit);
        this.grpGitOptions.Controls.Add(this.chkCreateInitialCommit);
        this.grpGitOptions.Location = new Point(20, 180);
        this.grpGitOptions.Name = "grpGitOptions";
        this.grpGitOptions.Size = new Size(760, 100);
        this.grpGitOptions.TabIndex = 9;
        this.grpGitOptions.TabStop = false;
        this.grpGitOptions.Text = "Git Options";

        //
        // chkInitializeGit
        //
        this.chkInitializeGit.AutoSize = true;
        this.chkInitializeGit.Checked = true;
        this.chkInitializeGit.CheckState = CheckState.Checked;
        this.chkInitializeGit.Location = new Point(20, 30);
        this.chkInitializeGit.Name = "chkInitializeGit";
        this.chkInitializeGit.Size = new Size(300, 24);
        this.chkInitializeGit.TabIndex = 0;
        this.chkInitializeGit.Text = "Initialize Git repository (git init)";
        this.chkInitializeGit.UseVisualStyleBackColor = true;

        //
        // chkCreateInitialCommit
        //
        this.chkCreateInitialCommit.AutoSize = true;
        this.chkCreateInitialCommit.Checked = true;
        this.chkCreateInitialCommit.CheckState = CheckState.Checked;
        this.chkCreateInitialCommit.Location = new Point(20, 60);
        this.chkCreateInitialCommit.Name = "chkCreateInitialCommit";
        this.chkCreateInitialCommit.Size = new Size(350, 24);
        this.chkCreateInitialCommit.TabIndex = 1;
        this.chkCreateInitialCommit.Text = "Create initial commit (git add . && git commit)";
        this.chkCreateInitialCommit.UseVisualStyleBackColor = true;

        //
        // grpValidation
        //
        this.grpValidation.Controls.Add(this.chkRunValidation);
        this.grpValidation.Location = new Point(20, 290);
        this.grpValidation.Name = "grpValidation";
        this.grpValidation.Size = new Size(760, 70);
        this.grpValidation.TabIndex = 10;
        this.grpValidation.TabStop = false;
        this.grpValidation.Text = "Post-Generation";

        //
        // chkRunValidation
        //
        this.chkRunValidation.AutoSize = true;
        this.chkRunValidation.Checked = true;
        this.chkRunValidation.CheckState = CheckState.Checked;
        this.chkRunValidation.Location = new Point(20, 30);
        this.chkRunValidation.Name = "chkRunValidation";
        this.chkRunValidation.Size = new Size(400, 24);
        this.chkRunValidation.TabIndex = 0;
        this.chkRunValidation.Text = "Run DbContext validation after generation";
        this.chkRunValidation.UseVisualStyleBackColor = true;

        //
        // lblInfo
        //
        this.lblInfo.AutoSize = true;
        this.lblInfo.ForeColor = SystemColors.GrayText;
        this.lblInfo.Location = new Point(20, 380);
        this.lblInfo.Name = "lblInfo";
        this.lblInfo.Size = new Size(600, 20);
        this.lblInfo.TabIndex = 11;
        this.lblInfo.Text = "* Required fields. Project name must be a valid C# identifier (letters, digits, underscores).";

        //
        // ProjectSettingsControl
        //
        this.AutoScaleDimensions = new SizeF(8F, 20F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.Controls.Add(this.lblInfo);
        this.Controls.Add(this.grpValidation);
        this.Controls.Add(this.grpGitOptions);
        this.Controls.Add(this.txtAuthorName);
        this.Controls.Add(this.lblAuthorName);
        this.Controls.Add(this.txtCompanyName);
        this.Controls.Add(this.lblCompanyName);
        this.Controls.Add(this.btnBrowse);
        this.Controls.Add(this.txtTargetPath);
        this.Controls.Add(this.lblTargetPath);
        this.Controls.Add(this.txtProjectName);
        this.Controls.Add(this.lblProjectName);
        this.Name = "ProjectSettingsControl";
        this.Size = new Size(800, 450);
        this.grpGitOptions.ResumeLayout(false);
        this.grpGitOptions.PerformLayout();
        this.grpValidation.ResumeLayout(false);
        this.grpValidation.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
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
}
