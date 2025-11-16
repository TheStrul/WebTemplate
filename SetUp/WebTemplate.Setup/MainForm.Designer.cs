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
        toolStrip = new ToolStrip();
        btnNew = new ToolStripButton();
        btnLoad = new ToolStripButton();
        btnSave = new ToolStripButton();
        btnDelete = new ToolStripButton();
        toolStripSeparator1 = new ToolStripSeparator();
        lblConfigLabel = new ToolStripLabel();
        cmbConfigurations = new ToolStripComboBox();
        toolStripSeparator2 = new ToolStripSeparator();
        btnValidate = new ToolStripButton();
        btnGenerate = new ToolStripButton();
        tabControl = new TabControl();
        tabProject = new TabPage();
        tabFeatures = new TabPage();
        tabSecrets = new TabPage();
        tabDatabase = new TabPage();
        tabServer = new TabPage();
        tabEmail = new TabPage();
        tabAuth = new TabPage();
        statusStrip = new StatusStrip();
        lblStatus = new ToolStripStatusLabel();
        progressBar = new ToolStripProgressBar();
        splitContainer = new SplitContainer();
        txtLog = new TextBox();
        lblLog = new Label();
        toolStrip.SuspendLayout();
        tabControl.SuspendLayout();
        statusStrip.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        SuspendLayout();
        // 
        // toolStrip
        // 
        toolStrip.ImageScalingSize = new Size(24, 24);
        toolStrip.Items.AddRange(new ToolStripItem[] { btnNew, btnLoad, btnSave, btnDelete, toolStripSeparator1, lblConfigLabel, cmbConfigurations, toolStripSeparator2, btnValidate, btnGenerate });
        toolStrip.Location = new Point(0, 0);
        toolStrip.Name = "toolStrip";
        toolStrip.Size = new Size(1050, 25);
        toolStrip.TabIndex = 0;
        // 
        // btnNew
        // 
        btnNew.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnNew.Name = "btnNew";
        btnNew.Size = new Size(35, 22);
        btnNew.Text = "New";
        btnNew.ToolTipText = "Create new configuration (Ctrl+N)";
        // 
        // btnLoad
        // 
        btnLoad.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnLoad.Name = "btnLoad";
        btnLoad.Size = new Size(37, 22);
        btnLoad.Text = "Load";
        btnLoad.ToolTipText = "Load selected configuration (Ctrl+O)";
        // 
        // btnSave
        // 
        btnSave.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(35, 22);
        btnSave.Text = "Save";
        btnSave.ToolTipText = "Save current configuration (Ctrl+S)";
        // 
        // btnDelete
        // 
        btnDelete.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnDelete.Name = "btnDelete";
        btnDelete.Size = new Size(44, 22);
        btnDelete.Text = "Delete";
        btnDelete.ToolTipText = "Delete selected configuration (Delete)";
        // 
        // toolStripSeparator1
        // 
        toolStripSeparator1.Name = "toolStripSeparator1";
        toolStripSeparator1.Size = new Size(6, 25);
        // 
        // lblConfigLabel
        // 
        lblConfigLabel.Name = "lblConfigLabel";
        lblConfigLabel.Size = new Size(84, 22);
        lblConfigLabel.Text = "Configuration:";
        // 
        // cmbConfigurations
        // 
        cmbConfigurations.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbConfigurations.Name = "cmbConfigurations";
        cmbConfigurations.Size = new Size(219, 25);
        cmbConfigurations.ToolTipText = "Select configuration to load";
        // 
        // toolStripSeparator2
        // 
        toolStripSeparator2.Name = "toolStripSeparator2";
        toolStripSeparator2.Size = new Size(6, 25);
        // 
        // btnValidate
        // 
        btnValidate.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnValidate.Name = "btnValidate";
        btnValidate.Size = new Size(52, 22);
        btnValidate.Text = "Validate";
        btnValidate.ToolTipText = "Validate current configuration (F5)";
        // 
        // btnGenerate
        // 
        btnGenerate.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnGenerate.Name = "btnGenerate";
        btnGenerate.Size = new Size(58, 22);
        btnGenerate.Text = "Generate";
        btnGenerate.ToolTipText = "Generate project from configuration (F6)";
        // 
        // tabControl
        // 
        tabControl.Controls.Add(tabProject);
        tabControl.Controls.Add(tabFeatures);
        tabControl.Controls.Add(tabSecrets);
        tabControl.Controls.Add(tabDatabase);
        tabControl.Controls.Add(tabServer);
        tabControl.Controls.Add(tabEmail);
        tabControl.Controls.Add(tabAuth);
        tabControl.Dock = DockStyle.Fill;
        tabControl.Location = new Point(0, 0);
        tabControl.Margin = new Padding(3, 2, 3, 2);
        tabControl.Name = "tabControl";
        tabControl.SelectedIndex = 0;
        tabControl.Size = new Size(600, 497);
        tabControl.TabIndex = 0;
        // 
        // tabProject
        // 
        tabProject.Location = new Point(4, 24);
        tabProject.Margin = new Padding(3, 2, 3, 2);
        tabProject.Name = "tabProject";
        tabProject.Padding = new Padding(3, 2, 3, 2);
        tabProject.Size = new Size(592, 469);
        tabProject.TabIndex = 0;
        tabProject.Text = "Project Settings";
        tabProject.UseVisualStyleBackColor = true;
        // 
        // tabFeatures
        // 
        tabFeatures.Location = new Point(4, 24);
        tabFeatures.Margin = new Padding(3, 2, 3, 2);
        tabFeatures.Name = "tabFeatures";
        tabFeatures.Padding = new Padding(3, 2, 3, 2);
        tabFeatures.Size = new Size(592, 469);
        tabFeatures.TabIndex = 1;
        tabFeatures.Text = "Features";
        tabFeatures.UseVisualStyleBackColor = true;
        // 
        // tabSecrets
        // 
        tabSecrets.Location = new Point(4, 24);
        tabSecrets.Margin = new Padding(3, 2, 3, 2);
        tabSecrets.Name = "tabSecrets";
        tabSecrets.Size = new Size(592, 469);
        tabSecrets.TabIndex = 2;
        tabSecrets.Text = "Secrets";
        tabSecrets.UseVisualStyleBackColor = true;
        // 
        // tabDatabase
        // 
        tabDatabase.Location = new Point(4, 24);
        tabDatabase.Margin = new Padding(3, 2, 3, 2);
        tabDatabase.Name = "tabDatabase";
        tabDatabase.Size = new Size(592, 469);
        tabDatabase.TabIndex = 3;
        tabDatabase.Text = "Database";
        tabDatabase.UseVisualStyleBackColor = true;
        // 
        // tabServer
        // 
        tabServer.Location = new Point(4, 24);
        tabServer.Margin = new Padding(3, 2, 3, 2);
        tabServer.Name = "tabServer";
        tabServer.Size = new Size(592, 469);
        tabServer.TabIndex = 4;
        tabServer.Text = "Server";
        tabServer.UseVisualStyleBackColor = true;
        // 
        // tabEmail
        // 
        tabEmail.Location = new Point(4, 24);
        tabEmail.Margin = new Padding(3, 2, 3, 2);
        tabEmail.Name = "tabEmail";
        tabEmail.Size = new Size(592, 469);
        tabEmail.TabIndex = 5;
        tabEmail.Text = "Email";
        tabEmail.UseVisualStyleBackColor = true;
        // 
        // tabAuth
        // 
        tabAuth.Location = new Point(4, 24);
        tabAuth.Margin = new Padding(3, 2, 3, 2);
        tabAuth.Name = "tabAuth";
        tabAuth.Size = new Size(592, 469);
        tabAuth.TabIndex = 6;
        tabAuth.Text = "Authentication";
        tabAuth.UseVisualStyleBackColor = true;
        // 
        // statusStrip
        // 
        statusStrip.ImageScalingSize = new Size(20, 20);
        statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, progressBar });
        statusStrip.Location = new Point(0, 522);
        statusStrip.Name = "statusStrip";
        statusStrip.Padding = new Padding(1, 0, 12, 0);
        statusStrip.Size = new Size(1050, 22);
        statusStrip.TabIndex = 2;
        // 
        // lblStatus
        // 
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(1037, 17);
        lblStatus.Spring = true;
        lblStatus.Text = "Ready";
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // progressBar
        // 
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(88, 16);
        progressBar.Visible = false;
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Location = new Point(0, 25);
        splitContainer.Margin = new Padding(3, 2, 3, 2);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(tabControl);
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(txtLog);
        splitContainer.Panel2.Controls.Add(lblLog);
        splitContainer.Size = new Size(1050, 497);
        splitContainer.SplitterDistance = 600;
        splitContainer.SplitterWidth = 3;
        splitContainer.TabIndex = 1;
        // 
        // txtLog
        // 
        txtLog.AcceptsReturn = true;
        txtLog.Dock = DockStyle.Fill;
        txtLog.Font = new Font("Consolas", 9F);
        txtLog.Location = new Point(0, 23);
        txtLog.Margin = new Padding(3, 2, 3, 2);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.Size = new Size(447, 474);
        txtLog.TabIndex = 1;
        txtLog.WordWrap = false;
        // 
        // lblLog
        // 
        lblLog.AutoSize = true;
        lblLog.Dock = DockStyle.Top;
        lblLog.Location = new Point(0, 0);
        lblLog.Name = "lblLog";
        lblLog.Padding = new Padding(4);
        lblLog.Size = new Size(99, 23);
        lblLog.TabIndex = 0;
        lblLog.Text = "Generation Log:";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1050, 544);
        Controls.Add(splitContainer);
        Controls.Add(toolStrip);
        Controls.Add(statusStrip);
        Margin = new Padding(3, 2, 3, 2);
        MinimumSize = new Size(877, 460);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "WebTemplate Setup";
        toolStrip.ResumeLayout(false);
        toolStrip.PerformLayout();
        tabControl.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        splitContainer.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
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
