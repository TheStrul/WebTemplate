namespace WebTemplate.Setup.UI
{
    partial class DatabaseControl
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
            this.mainPanel = new Panel();
            this.grpConnection = new GroupBox();
            this.lblTestResult = new Label();
            this.btnTestConnection = new Button();
            this.txtConnectionString = new TextBox();
            this.lblConnectionString = new Label();
            this.grpOptions = new GroupBox();
            this.chkCreateDatabaseIfNotExists = new CheckBox();
            this.chkTestConnection = new CheckBox();
            this.txtInitScriptPath = new TextBox();
            this.lblInitScriptPath = new Label();
            this.chkExecuteInitScript = new CheckBox();
            this.mainPanel.SuspendLayout();
            this.grpConnection.SuspendLayout();
            this.grpOptions.SuspendLayout();
            this.SuspendLayout();
            //
            // mainPanel
            //
            this.mainPanel.AutoScroll = true;
            this.mainPanel.Controls.Add(this.grpOptions);
            this.mainPanel.Controls.Add(this.grpConnection);
            this.mainPanel.Dock = DockStyle.Fill;
            this.mainPanel.Location = new Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new Padding(10);
            this.mainPanel.Size = new Size(800, 600);
            this.mainPanel.TabIndex = 0;
            //
            // grpConnection
            //
            this.grpConnection.Controls.Add(this.lblTestResult);
            this.grpConnection.Controls.Add(this.btnTestConnection);
            this.grpConnection.Controls.Add(this.txtConnectionString);
            this.grpConnection.Controls.Add(this.lblConnectionString);
            this.grpConnection.Dock = DockStyle.Top;
            this.grpConnection.Location = new Point(10, 10);
            this.grpConnection.Name = "grpConnection";
            this.grpConnection.Padding = new Padding(10);
            this.grpConnection.Size = new Size(780, 150);
            this.grpConnection.TabIndex = 0;
            this.grpConnection.TabStop = false;
            this.grpConnection.Text = "Connection";
            //
            // lblConnectionString
            //
            this.lblConnectionString.AutoSize = true;
            this.lblConnectionString.Location = new Point(20, 30);
            this.lblConnectionString.Name = "lblConnectionString";
            this.lblConnectionString.Size = new Size(109, 15);
            this.lblConnectionString.TabIndex = 0;
            this.lblConnectionString.Text = "Connection String:";
            //
            // txtConnectionString
            //
            this.txtConnectionString.Location = new Point(20, 48);
            this.txtConnectionString.Multiline = true;
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.Size = new Size(740, 50);
            this.txtConnectionString.TabIndex = 1;
            //
            // btnTestConnection
            //
            this.btnTestConnection.Location = new Point(20, 104);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new Size(120, 30);
            this.btnTestConnection.TabIndex = 2;
            this.btnTestConnection.Text = "Test Connection";
            this.btnTestConnection.UseVisualStyleBackColor = true;
            this.btnTestConnection.Click += new EventHandler(this.btnTestConnection_Click);
            //
            // lblTestResult
            //
            this.lblTestResult.AutoSize = true;
            this.lblTestResult.Location = new Point(150, 112);
            this.lblTestResult.Name = "lblTestResult";
            this.lblTestResult.Size = new Size(0, 15);
            this.lblTestResult.TabIndex = 3;
            //
            // grpOptions
            //
            this.grpOptions.Controls.Add(this.chkCreateDatabaseIfNotExists);
            this.grpOptions.Controls.Add(this.chkTestConnection);
            this.grpOptions.Controls.Add(this.txtInitScriptPath);
            this.grpOptions.Controls.Add(this.lblInitScriptPath);
            this.grpOptions.Controls.Add(this.chkExecuteInitScript);
            this.grpOptions.Dock = DockStyle.Top;
            this.grpOptions.Location = new Point(10, 160);
            this.grpOptions.Name = "grpOptions";
            this.grpOptions.Padding = new Padding(10);
            this.grpOptions.Size = new Size(780, 150);
            this.grpOptions.TabIndex = 1;
            this.grpOptions.TabStop = false;
            this.grpOptions.Text = "Options";
            //
            // chkExecuteInitScript
            //
            this.chkExecuteInitScript.AutoSize = true;
            this.chkExecuteInitScript.Location = new Point(20, 30);
            this.chkExecuteInitScript.Name = "chkExecuteInitScript";
            this.chkExecuteInitScript.Size = new Size(234, 19);
            this.chkExecuteInitScript.TabIndex = 0;
            this.chkExecuteInitScript.Text = "Execute db-init.sql after generation";
            this.chkExecuteInitScript.UseVisualStyleBackColor = true;
            //
            // lblInitScriptPath
            //
            this.lblInitScriptPath.AutoSize = true;
            this.lblInitScriptPath.Location = new Point(40, 55);
            this.lblInitScriptPath.Name = "lblInitScriptPath";
            this.lblInitScriptPath.Size = new Size(98, 15);
            this.lblInitScriptPath.TabIndex = 1;
            this.lblInitScriptPath.Text = "Init Script Path:";
            //
            // txtInitScriptPath
            //
            this.txtInitScriptPath.Location = new Point(150, 52);
            this.txtInitScriptPath.Name = "txtInitScriptPath";
            this.txtInitScriptPath.Size = new Size(600, 23);
            this.txtInitScriptPath.TabIndex = 2;
            //
            // chkTestConnection
            //
            this.chkTestConnection.AutoSize = true;
            this.chkTestConnection.Location = new Point(20, 85);
            this.chkTestConnection.Name = "chkTestConnection";
            this.chkTestConnection.Size = new Size(207, 19);
            this.chkTestConnection.TabIndex = 3;
            this.chkTestConnection.Text = "Test connection before generation";
            this.chkTestConnection.UseVisualStyleBackColor = true;
            //
            // chkCreateDatabaseIfNotExists
            //
            this.chkCreateDatabaseIfNotExists.AutoSize = true;
            this.chkCreateDatabaseIfNotExists.Location = new Point(20, 110);
            this.chkCreateDatabaseIfNotExists.Name = "chkCreateDatabaseIfNotExists";
            this.chkCreateDatabaseIfNotExists.Size = new Size(217, 19);
            this.chkCreateDatabaseIfNotExists.TabIndex = 4;
            this.chkCreateDatabaseIfNotExists.Text = "Create database if it doesn't exist";
            this.chkCreateDatabaseIfNotExists.UseVisualStyleBackColor = true;
            //
            // DatabaseControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.mainPanel);
            this.Name = "DatabaseControl";
            this.Size = new Size(800, 600);
            this.mainPanel.ResumeLayout(false);
            this.grpConnection.ResumeLayout(false);
            this.grpConnection.PerformLayout();
            this.grpOptions.ResumeLayout(false);
            this.grpOptions.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private Panel mainPanel;
        private GroupBox grpConnection;
        private Label lblConnectionString;
        private TextBox txtConnectionString;
        private Button btnTestConnection;
        private Label lblTestResult;
        private GroupBox grpOptions;
        private CheckBox chkExecuteInitScript;
        private Label lblInitScriptPath;
        private TextBox txtInitScriptPath;
        private CheckBox chkTestConnection;
        private CheckBox chkCreateDatabaseIfNotExists;
    }
}
