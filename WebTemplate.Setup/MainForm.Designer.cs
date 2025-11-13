namespace WebTemplate.Setup;

partial class MainForm
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

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();

        // Main container
        this.toolStrip = new ToolStrip();
        this.btnNew = new ToolStripButton();
        this.btnLoad = new ToolStripButton();
        this.btnSave = new ToolStripButton();
        this.btnDelete = new ToolStripButton();
        this.toolStripSeparator1 = new ToolStripSeparator();
        this.lblConfigLabel = new ToolStripLabel();
        this.cmbConfigurations = new ToolStripComboBox();
        this.toolStripSeparator2 = new ToolStripSeparator();
        this.btnValidate = new ToolStripButton();
        this.btnGenerate = new ToolStripButton();

        // Tab control
        this.tabControl = new TabControl();
        this.tabProject = new TabPage();
        this.tabFeatures = new TabPage();
        this.tabSecrets = new TabPage();
        this.tabDatabase = new TabPage();
        this.tabServer = new TabPage();
        this.tabEmail = new TabPage();
        this.tabAuth = new TabPage();

        // Status bar
        this.statusStrip = new StatusStrip();
        this.lblStatus = new ToolStripStatusLabel();
        this.progressBar = new ToolStripProgressBar();

        // Log panel
        this.splitContainer = new SplitContainer();
        this.txtLog = new TextBox();
        this.lblLog = new Label();

        this.SuspendLayout();

        //
        // toolStrip
        //
        this.toolStrip.ImageScalingSize = new Size(24, 24);
        this.toolStrip.Items.AddRange(new ToolStripItem[] {
            this.btnNew,
            this.btnLoad,
            this.btnSave,
            this.btnDelete,
            this.toolStripSeparator1,
            this.lblConfigLabel,
            this.cmbConfigurations,
            this.toolStripSeparator2,
            this.btnValidate,
            this.btnGenerate
        });
        this.toolStrip.Location = new Point(0, 0);
        this.toolStrip.Name = "toolStrip";
        this.toolStrip.Size = new Size(1200, 31);
        this.toolStrip.TabIndex = 0;

        //
        // btnNew
        //
        this.btnNew.DisplayStyle = ToolStripItemDisplayStyle.Text;
        this.btnNew.Name = "btnNew";
        this.btnNew.Size = new Size(50, 28);
        this.btnNew.Text = "New";
        this.btnNew.ToolTipText = "Create new configuration (Ctrl+N)";

        //
        // btnLoad
        //
        this.btnLoad.DisplayStyle = ToolStripItemDisplayStyle.Text;
        this.btnLoad.Name = "btnLoad";
        this.btnLoad.Size = new Size(50, 28);
        this.btnLoad.Text = "Load";
        this.btnLoad.ToolTipText = "Load selected configuration (Ctrl+O)";

        //
        // btnSave
        //
        this.btnSave.DisplayStyle = ToolStripItemDisplayStyle.Text;
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new Size(50, 28);
        this.btnSave.Text = "Save";
        this.btnSave.ToolTipText = "Save current configuration (Ctrl+S)";

        //
        // btnDelete
        //
        this.btnDelete.DisplayStyle = ToolStripItemDisplayStyle.Text;
        this.btnDelete.Name = "btnDelete";
        this.btnDelete.Size = new Size(60, 28);
        this.btnDelete.Text = "Delete";
        this.btnDelete.ToolTipText = "Delete selected configuration (Delete)";

        //
        // toolStripSeparator1
        //
        this.toolStripSeparator1.Name = "toolStripSeparator1";
        this.toolStripSeparator1.Size = new Size(6, 31);

        //
        // lblConfigLabel
        //
        this.lblConfigLabel.Name = "lblConfigLabel";
        this.lblConfigLabel.Size = new Size(100, 28);
        this.lblConfigLabel.Text = "Configuration:";

        //
        // cmbConfigurations
        //
        this.cmbConfigurations.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbConfigurations.Name = "cmbConfigurations";
        this.cmbConfigurations.Size = new Size(250, 31);
        this.cmbConfigurations.ToolTipText = "Select configuration to load";

        //
        // toolStripSeparator2
        //
        this.toolStripSeparator2.Name = "toolStripSeparator2";
        this.toolStripSeparator2.Size = new Size(6, 31);

        //
        // btnValidate
        //
        this.btnValidate.DisplayStyle = ToolStripItemDisplayStyle.Text;
        this.btnValidate.Name = "btnValidate";
        this.btnValidate.Size = new Size(70, 28);
        this.btnValidate.Text = "Validate";
        this.btnValidate.ToolTipText = "Validate current configuration (F5)";

        //
        // btnGenerate
        //
        this.btnGenerate.DisplayStyle = ToolStripItemDisplayStyle.Text;
        this.btnGenerate.Name = "btnGenerate";
        this.btnGenerate.Size = new Size(80, 28);
        this.btnGenerate.Text = "Generate";
        this.btnGenerate.ToolTipText = "Generate project from configuration (F6)";

        //
        // splitContainer
        //
        this.splitContainer.Dock = DockStyle.Fill;
        this.splitContainer.Location = new Point(0, 31);
        this.splitContainer.Name = "splitContainer";
        this.splitContainer.Orientation = Orientation.Horizontal;
        this.splitContainer.Size = new Size(1200, 669);
        this.splitContainer.SplitterDistance = 500;
        this.splitContainer.TabIndex = 1;

        //
        // splitContainer.Panel1
        //
        this.splitContainer.Panel1.Controls.Add(this.tabControl);

        //
        // splitContainer.Panel2
        //
        this.splitContainer.Panel2.Controls.Add(this.txtLog);
        this.splitContainer.Panel2.Controls.Add(this.lblLog);

        //
        // tabControl
        //
        this.tabControl.Controls.Add(this.tabProject);
        this.tabControl.Controls.Add(this.tabFeatures);
        this.tabControl.Controls.Add(this.tabSecrets);
        this.tabControl.Controls.Add(this.tabDatabase);
        this.tabControl.Controls.Add(this.tabServer);
        this.tabControl.Controls.Add(this.tabEmail);
        this.tabControl.Controls.Add(this.tabAuth);
        this.tabControl.Dock = DockStyle.Fill;
        this.tabControl.Location = new Point(0, 0);
        this.tabControl.Name = "tabControl";
        this.tabControl.SelectedIndex = 0;
        this.tabControl.Size = new Size(1200, 500);
        this.tabControl.TabIndex = 0;

        //
        // tabProject
        //
        this.tabProject.Location = new Point(4, 29);
        this.tabProject.Name = "tabProject";
        this.tabProject.Padding = new Padding(3);
        this.tabProject.Size = new Size(1192, 467);
        this.tabProject.TabIndex = 0;
        this.tabProject.Text = "Project Settings";
        this.tabProject.UseVisualStyleBackColor = true;

        //
        // tabFeatures
        //
        this.tabFeatures.Location = new Point(4, 29);
        this.tabFeatures.Name = "tabFeatures";
        this.tabFeatures.Padding = new Padding(3);
        this.tabFeatures.Size = new Size(1192, 467);
        this.tabFeatures.TabIndex = 1;
        this.tabFeatures.Text = "Features";
        this.tabFeatures.UseVisualStyleBackColor = true;

        //
        // tabSecrets
        //
        this.tabSecrets.Location = new Point(4, 29);
        this.tabSecrets.Name = "tabSecrets";
        this.tabSecrets.Size = new Size(1192, 467);
        this.tabSecrets.TabIndex = 2;
        this.tabSecrets.Text = "Secrets";
        this.tabSecrets.UseVisualStyleBackColor = true;

        //
        // tabDatabase
        //
        this.tabDatabase.Location = new Point(4, 29);
        this.tabDatabase.Name = "tabDatabase";
        this.tabDatabase.Size = new Size(1192, 467);
        this.tabDatabase.TabIndex = 3;
        this.tabDatabase.Text = "Database";
        this.tabDatabase.UseVisualStyleBackColor = true;

        //
        // tabServer
        //
        this.tabServer.Location = new Point(4, 29);
        this.tabServer.Name = "tabServer";
        this.tabServer.Size = new Size(1192, 467);
        this.tabServer.TabIndex = 4;
        this.tabServer.Text = "Server";
        this.tabServer.UseVisualStyleBackColor = true;

        //
        // tabEmail
        //
        this.tabEmail.Location = new Point(4, 29);
        this.tabEmail.Name = "tabEmail";
        this.tabEmail.Size = new Size(1192, 467);
        this.tabEmail.TabIndex = 5;
        this.tabEmail.Text = "Email";
        this.tabEmail.UseVisualStyleBackColor = true;

        //
        // tabAuth
        //
        this.tabAuth.Location = new Point(4, 29);
        this.tabAuth.Name = "tabAuth";
        this.tabAuth.Size = new Size(1192, 467);
        this.tabAuth.TabIndex = 6;
        this.tabAuth.Text = "Authentication";
        this.tabAuth.UseVisualStyleBackColor = true;

        //
        // lblLog
        //
        this.lblLog.AutoSize = true;
        this.lblLog.Dock = DockStyle.Top;
        this.lblLog.Location = new Point(0, 0);
        this.lblLog.Name = "lblLog";
        this.lblLog.Padding = new Padding(5);
        this.lblLog.Size = new Size(100, 30);
        this.lblLog.TabIndex = 0;
        this.lblLog.Text = "Generation Log:";

        //
        // txtLog
        //
        this.txtLog.Dock = DockStyle.Fill;
        this.txtLog.Font = new Font("Consolas", 9F);
        this.txtLog.Location = new Point(0, 30);
        this.txtLog.Multiline = true;
        this.txtLog.Name = "txtLog";
        this.txtLog.ReadOnly = true;
        this.txtLog.ScrollBars = ScrollBars.Vertical;
        this.txtLog.Size = new Size(1200, 135);
        this.txtLog.TabIndex = 1;

        //
        // statusStrip
        //
        this.statusStrip.ImageScalingSize = new Size(20, 20);
        this.statusStrip.Items.AddRange(new ToolStripItem[] {
            this.lblStatus,
            this.progressBar
        });
        this.statusStrip.Location = new Point(0, 700);
        this.statusStrip.Name = "statusStrip";
        this.statusStrip.Size = new Size(1200, 26);
        this.statusStrip.TabIndex = 2;

        //
        // lblStatus
        //
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new Size(1085, 20);
        this.lblStatus.Spring = true;
        this.lblStatus.Text = "Ready";
        this.lblStatus.TextAlign = ContentAlignment.MiddleLeft;

        //
        // progressBar
        //
        this.progressBar.Name = "progressBar";
        this.progressBar.Size = new Size(100, 20);
        this.progressBar.Visible = false;

        //
        // MainForm
        //
        this.AutoScaleDimensions = new SizeF(8F, 20F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(1200, 726);
        this.Controls.Add(this.splitContainer);
        this.Controls.Add(this.toolStrip);
        this.Controls.Add(this.statusStrip);
        this.MinimumSize = new Size(1000, 600);
        this.Name = "MainForm";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "WebTemplate Setup";

        this.splitContainer.Panel1.ResumeLayout(false);
        this.splitContainer.Panel2.ResumeLayout(false);
        this.splitContainer.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
        this.splitContainer.ResumeLayout(false);
        this.toolStrip.ResumeLayout(false);
        this.toolStrip.PerformLayout();
        this.tabControl.ResumeLayout(false);
        this.statusStrip.ResumeLayout(false);
        this.statusStrip.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private ToolStrip toolStrip;
    private ToolStripButton btnNew;
    private ToolStripButton btnLoad;
    private ToolStripButton btnSave;
    private ToolStripButton btnDelete;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripLabel lblConfigLabel;
    private ToolStripComboBox cmbConfigurations;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripButton btnValidate;
    private ToolStripButton btnGenerate;

    private SplitContainer splitContainer;
    private TabControl tabControl;
    private TabPage tabProject;
    private TabPage tabFeatures;
    private TabPage tabSecrets;
    private TabPage tabDatabase;
    private TabPage tabServer;
    private TabPage tabEmail;
    private TabPage tabAuth;

    private Label lblLog;
    private TextBox txtLog;

    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblStatus;
    private ToolStripProgressBar progressBar;
}
