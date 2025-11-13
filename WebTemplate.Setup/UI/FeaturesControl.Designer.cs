namespace WebTemplate.Setup.UI
{
    partial class FeaturesControl
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
            this.grpEmail = new GroupBox();
            this.grpEmailOptions = new GroupBox();
            this.txtFromName = new TextBox();
            this.lblFromName = new Label();
            this.txtFromEmail = new TextBox();
            this.lblFromEmail = new Label();
            this.txtSmtpPassword = new TextBox();
            this.lblSmtpPassword = new Label();
            this.txtSmtpUsername = new TextBox();
            this.lblSmtpUsername = new Label();
            this.chkEnableSsl = new CheckBox();
            this.numSmtpPort = new NumericUpDown();
            this.lblSmtpPort = new Label();
            this.txtSmtpHost = new TextBox();
            this.lblSmtpHost = new Label();
            this.chkEmail = new CheckBox();
            this.grpCors = new GroupBox();
            this.grpCorsOptions = new GroupBox();
            this.txtAllowedOrigins = new TextBox();
            this.lblAllowedOrigins = new Label();
            this.chkCors = new CheckBox();
            this.grpAdminSeed = new GroupBox();
            this.grpAdminSeedOptions = new GroupBox();
            this.txtAdminPassword = new TextBox();
            this.lblAdminPassword = new Label();
            this.txtAdminEmail = new TextBox();
            this.lblAdminEmail = new Label();
            this.chkCreateDefaultAdmin = new CheckBox();
            this.chkAdminSeed = new CheckBox();
            this.grpExceptionHandling = new GroupBox();
            this.grpExceptionHandlingOptions = new GroupBox();
            this.chkLogExceptions = new CheckBox();
            this.chkUseDetailedErrors = new CheckBox();
            this.chkExceptionHandling = new CheckBox();
            this.mainPanel.SuspendLayout();
            this.grpEmail.SuspendLayout();
            this.grpEmailOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSmtpPort)).BeginInit();
            this.grpCors.SuspendLayout();
            this.grpCorsOptions.SuspendLayout();
            this.grpAdminSeed.SuspendLayout();
            this.grpAdminSeedOptions.SuspendLayout();
            this.grpExceptionHandling.SuspendLayout();
            this.grpExceptionHandlingOptions.SuspendLayout();
            this.SuspendLayout();
            //
            // mainPanel
            //
            this.mainPanel.AutoScroll = true;
            this.mainPanel.Controls.Add(this.grpEmail);
            this.mainPanel.Controls.Add(this.grpCors);
            this.mainPanel.Controls.Add(this.grpAdminSeed);
            this.mainPanel.Controls.Add(this.grpExceptionHandling);
            this.mainPanel.Dock = DockStyle.Fill;
            this.mainPanel.Location = new Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new Padding(10);
            this.mainPanel.Size = new Size(800, 600);
            this.mainPanel.TabIndex = 0;
            //
            // grpExceptionHandling
            //
            this.grpExceptionHandling.Controls.Add(this.grpExceptionHandlingOptions);
            this.grpExceptionHandling.Controls.Add(this.chkExceptionHandling);
            this.grpExceptionHandling.Dock = DockStyle.Top;
            this.grpExceptionHandling.Location = new Point(10, 10);
            this.grpExceptionHandling.Name = "grpExceptionHandling";
            this.grpExceptionHandling.Padding = new Padding(10);
            this.grpExceptionHandling.Size = new Size(780, 120);
            this.grpExceptionHandling.TabIndex = 0;
            this.grpExceptionHandling.TabStop = false;
            this.grpExceptionHandling.Text = "Exception Handling";
            //
            // chkExceptionHandling
            //
            this.chkExceptionHandling.AutoSize = true;
            this.chkExceptionHandling.Dock = DockStyle.Top;
            this.chkExceptionHandling.Location = new Point(10, 26);
            this.chkExceptionHandling.Name = "chkExceptionHandling";
            this.chkExceptionHandling.Size = new Size(760, 19);
            this.chkExceptionHandling.TabIndex = 0;
            this.chkExceptionHandling.Text = "Enable Exception Handling";
            this.chkExceptionHandling.UseVisualStyleBackColor = true;
            this.chkExceptionHandling.CheckedChanged += new EventHandler(this.chkExceptionHandling_CheckedChanged);
            //
            // grpExceptionHandlingOptions
            //
            this.grpExceptionHandlingOptions.Controls.Add(this.chkLogExceptions);
            this.grpExceptionHandlingOptions.Controls.Add(this.chkUseDetailedErrors);
            this.grpExceptionHandlingOptions.Dock = DockStyle.Fill;
            this.grpExceptionHandlingOptions.Location = new Point(10, 45);
            this.grpExceptionHandlingOptions.Name = "grpExceptionHandlingOptions";
            this.grpExceptionHandlingOptions.Padding = new Padding(10);
            this.grpExceptionHandlingOptions.Size = new Size(760, 65);
            this.grpExceptionHandlingOptions.TabIndex = 1;
            this.grpExceptionHandlingOptions.TabStop = false;
            //
            // chkUseDetailedErrors
            //
            this.chkUseDetailedErrors.AutoSize = true;
            this.chkUseDetailedErrors.Dock = DockStyle.Top;
            this.chkUseDetailedErrors.Location = new Point(10, 26);
            this.chkUseDetailedErrors.Name = "chkUseDetailedErrors";
            this.chkUseDetailedErrors.Size = new Size(740, 19);
            this.chkUseDetailedErrors.TabIndex = 0;
            this.chkUseDetailedErrors.Text = "Use Detailed Errors (Development only)";
            this.chkUseDetailedErrors.UseVisualStyleBackColor = true;
            //
            // chkLogExceptions
            //
            this.chkLogExceptions.AutoSize = true;
            this.chkLogExceptions.Dock = DockStyle.Top;
            this.chkLogExceptions.Location = new Point(10, 45);
            this.chkLogExceptions.Name = "chkLogExceptions";
            this.chkLogExceptions.Size = new Size(740, 19);
            this.chkLogExceptions.TabIndex = 1;
            this.chkLogExceptions.Text = "Log Exceptions";
            this.chkLogExceptions.UseVisualStyleBackColor = true;
            //
            // grpAdminSeed
            //
            this.grpAdminSeed.Controls.Add(this.grpAdminSeedOptions);
            this.grpAdminSeed.Controls.Add(this.chkAdminSeed);
            this.grpAdminSeed.Dock = DockStyle.Top;
            this.grpAdminSeed.Location = new Point(10, 130);
            this.grpAdminSeed.Name = "grpAdminSeed";
            this.grpAdminSeed.Padding = new Padding(10);
            this.grpAdminSeed.Size = new Size(780, 150);
            this.grpAdminSeed.TabIndex = 1;
            this.grpAdminSeed.TabStop = false;
            this.grpAdminSeed.Text = "Admin User Seeding";
            //
            // chkAdminSeed
            //
            this.chkAdminSeed.AutoSize = true;
            this.chkAdminSeed.Dock = DockStyle.Top;
            this.chkAdminSeed.Location = new Point(10, 26);
            this.chkAdminSeed.Name = "chkAdminSeed";
            this.chkAdminSeed.Size = new Size(760, 19);
            this.chkAdminSeed.TabIndex = 0;
            this.chkAdminSeed.Text = "Enable Admin Seed Module";
            this.chkAdminSeed.UseVisualStyleBackColor = true;
            this.chkAdminSeed.CheckedChanged += new EventHandler(this.chkAdminSeed_CheckedChanged);
            //
            // grpAdminSeedOptions
            //
            this.grpAdminSeedOptions.Controls.Add(this.txtAdminPassword);
            this.grpAdminSeedOptions.Controls.Add(this.lblAdminPassword);
            this.grpAdminSeedOptions.Controls.Add(this.txtAdminEmail);
            this.grpAdminSeedOptions.Controls.Add(this.lblAdminEmail);
            this.grpAdminSeedOptions.Controls.Add(this.chkCreateDefaultAdmin);
            this.grpAdminSeedOptions.Dock = DockStyle.Fill;
            this.grpAdminSeedOptions.Location = new Point(10, 45);
            this.grpAdminSeedOptions.Name = "grpAdminSeedOptions";
            this.grpAdminSeedOptions.Padding = new Padding(10);
            this.grpAdminSeedOptions.Size = new Size(760, 95);
            this.grpAdminSeedOptions.TabIndex = 1;
            this.grpAdminSeedOptions.TabStop = false;
            //
            // chkCreateDefaultAdmin
            //
            this.chkCreateDefaultAdmin.AutoSize = true;
            this.chkCreateDefaultAdmin.Dock = DockStyle.Top;
            this.chkCreateDefaultAdmin.Location = new Point(10, 26);
            this.chkCreateDefaultAdmin.Name = "chkCreateDefaultAdmin";
            this.chkCreateDefaultAdmin.Size = new Size(740, 19);
            this.chkCreateDefaultAdmin.TabIndex = 0;
            this.chkCreateDefaultAdmin.Text = "Create Default Admin User";
            this.chkCreateDefaultAdmin.UseVisualStyleBackColor = true;
            //
            // lblAdminEmail
            //
            this.lblAdminEmail.AutoSize = true;
            this.lblAdminEmail.Location = new Point(10, 48);
            this.lblAdminEmail.Name = "lblAdminEmail";
            this.lblAdminEmail.Size = new Size(76, 15);
            this.lblAdminEmail.TabIndex = 1;
            this.lblAdminEmail.Text = "Admin Email:";
            //
            // txtAdminEmail
            //
            this.txtAdminEmail.Location = new Point(150, 45);
            this.txtAdminEmail.Name = "txtAdminEmail";
            this.txtAdminEmail.Size = new Size(300, 23);
            this.txtAdminEmail.TabIndex = 2;
            //
            // lblAdminPassword
            //
            this.lblAdminPassword.AutoSize = true;
            this.lblAdminPassword.Location = new Point(10, 77);
            this.lblAdminPassword.Name = "lblAdminPassword";
            this.lblAdminPassword.Size = new Size(100, 15);
            this.lblAdminPassword.TabIndex = 3;
            this.lblAdminPassword.Text = "Admin Password:";
            //
            // txtAdminPassword
            //
            this.txtAdminPassword.Location = new Point(150, 74);
            this.txtAdminPassword.Name = "txtAdminPassword";
            this.txtAdminPassword.Size = new Size(300, 23);
            this.txtAdminPassword.TabIndex = 4;
            this.txtAdminPassword.UseSystemPasswordChar = true;
            //
            // grpCors
            //
            this.grpCors.Controls.Add(this.grpCorsOptions);
            this.grpCors.Controls.Add(this.chkCors);
            this.grpCors.Dock = DockStyle.Top;
            this.grpCors.Location = new Point(10, 280);
            this.grpCors.Name = "grpCors";
            this.grpCors.Padding = new Padding(10);
            this.grpCors.Size = new Size(780, 100);
            this.grpCors.TabIndex = 2;
            this.grpCors.TabStop = false;
            this.grpCors.Text = "CORS Configuration";
            //
            // chkCors
            //
            this.chkCors.AutoSize = true;
            this.chkCors.Dock = DockStyle.Top;
            this.chkCors.Location = new Point(10, 26);
            this.chkCors.Name = "chkCors";
            this.chkCors.Size = new Size(760, 19);
            this.chkCors.TabIndex = 0;
            this.chkCors.Text = "Enable CORS";
            this.chkCors.UseVisualStyleBackColor = true;
            this.chkCors.CheckedChanged += new EventHandler(this.chkCors_CheckedChanged);
            //
            // grpCorsOptions
            //
            this.grpCorsOptions.Controls.Add(this.txtAllowedOrigins);
            this.grpCorsOptions.Controls.Add(this.lblAllowedOrigins);
            this.grpCorsOptions.Dock = DockStyle.Fill;
            this.grpCorsOptions.Location = new Point(10, 45);
            this.grpCorsOptions.Name = "grpCorsOptions";
            this.grpCorsOptions.Padding = new Padding(10);
            this.grpCorsOptions.Size = new Size(760, 45);
            this.grpCorsOptions.TabIndex = 1;
            this.grpCorsOptions.TabStop = false;
            //
            // lblAllowedOrigins
            //
            this.lblAllowedOrigins.AutoSize = true;
            this.lblAllowedOrigins.Location = new Point(10, 19);
            this.lblAllowedOrigins.Name = "lblAllowedOrigins";
            this.lblAllowedOrigins.Size = new Size(165, 15);
            this.lblAllowedOrigins.TabIndex = 0;
            this.lblAllowedOrigins.Text = "Allowed Origins (comma-separated):";
            //
            // txtAllowedOrigins
            //
            this.txtAllowedOrigins.Location = new Point(180, 16);
            this.txtAllowedOrigins.Name = "txtAllowedOrigins";
            this.txtAllowedOrigins.Size = new Size(500, 23);
            this.txtAllowedOrigins.TabIndex = 1;
            //
            // grpEmail
            //
            this.grpEmail.Controls.Add(this.grpEmailOptions);
            this.grpEmail.Controls.Add(this.chkEmail);
            this.grpEmail.Dock = DockStyle.Top;
            this.grpEmail.Location = new Point(10, 380);
            this.grpEmail.Name = "grpEmail";
            this.grpEmail.Padding = new Padding(10);
            this.grpEmail.Size = new Size(780, 280);
            this.grpEmail.TabIndex = 3;
            this.grpEmail.TabStop = false;
            this.grpEmail.Text = "Email Configuration";
            //
            // chkEmail
            //
            this.chkEmail.AutoSize = true;
            this.chkEmail.Dock = DockStyle.Top;
            this.chkEmail.Location = new Point(10, 26);
            this.chkEmail.Name = "chkEmail";
            this.chkEmail.Size = new Size(760, 19);
            this.chkEmail.TabIndex = 0;
            this.chkEmail.Text = "Enable Email Module";
            this.chkEmail.UseVisualStyleBackColor = true;
            // Removed: this.chkEmail.CheckedChanged += new EventHandler(this.chkEmail_CheckedChanged);
            //
            // grpEmailOptions
            //
            this.grpEmailOptions.Controls.Add(this.txtFromName);
            this.grpEmailOptions.Controls.Add(this.lblFromName);
            this.grpEmailOptions.Controls.Add(this.txtFromEmail);
            this.grpEmailOptions.Controls.Add(this.lblFromEmail);
            this.grpEmailOptions.Controls.Add(this.txtSmtpPassword);
            this.grpEmailOptions.Controls.Add(this.lblSmtpPassword);
            this.grpEmailOptions.Controls.Add(this.txtSmtpUsername);
            this.grpEmailOptions.Controls.Add(this.lblSmtpUsername);
            this.grpEmailOptions.Controls.Add(this.chkEnableSsl);
            this.grpEmailOptions.Controls.Add(this.numSmtpPort);
            this.grpEmailOptions.Controls.Add(this.lblSmtpPort);
            this.grpEmailOptions.Controls.Add(this.txtSmtpHost);
            this.grpEmailOptions.Controls.Add(this.lblSmtpHost);
            this.grpEmailOptions.Dock = DockStyle.Fill;
            this.grpEmailOptions.Location = new Point(10, 45);
            this.grpEmailOptions.Name = "grpEmailOptions";
            this.grpEmailOptions.Padding = new Padding(10);
            this.grpEmailOptions.Size = new Size(760, 225);
            this.grpEmailOptions.TabIndex = 1;
            this.grpEmailOptions.TabStop = false;
            //
            // lblSmtpHost
            //
            this.lblSmtpHost.AutoSize = true;
            this.lblSmtpHost.Location = new Point(10, 19);
            this.lblSmtpHost.Name = "lblSmtpHost";
            this.lblSmtpHost.Size = new Size(71, 15);
            this.lblSmtpHost.TabIndex = 0;
            this.lblSmtpHost.Text = "SMTP Host:";
            //
            // txtSmtpHost
            //
            this.txtSmtpHost.Location = new Point(150, 16);
            this.txtSmtpHost.Name = "txtSmtpHost";
            this.txtSmtpHost.Size = new Size(300, 23);
            this.txtSmtpHost.TabIndex = 1;
            //
            // lblSmtpPort
            //
            this.lblSmtpPort.AutoSize = true;
            this.lblSmtpPort.Location = new Point(10, 48);
            this.lblSmtpPort.Name = "lblSmtpPort";
            this.lblSmtpPort.Size = new Size(67, 15);
            this.lblSmtpPort.TabIndex = 2;
            this.lblSmtpPort.Text = "SMTP Port:";
            //
            // numSmtpPort
            //
            this.numSmtpPort.Location = new Point(150, 45);
            this.numSmtpPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            this.numSmtpPort.Name = "numSmtpPort";
            this.numSmtpPort.Size = new Size(100, 23);
            this.numSmtpPort.TabIndex = 3;
            this.numSmtpPort.Value = 587;
            //
            // chkEnableSsl
            //
            this.chkEnableSsl.AutoSize = true;
            this.chkEnableSsl.Location = new Point(150, 74);
            this.chkEnableSsl.Name = "chkEnableSsl";
            this.chkEnableSsl.Size = new Size(84, 19);
            this.chkEnableSsl.TabIndex = 4;
            this.chkEnableSsl.Text = "Enable SSL";
            this.chkEnableSsl.UseVisualStyleBackColor = true;
            //
            // lblSmtpUsername
            //
            this.lblSmtpUsername.AutoSize = true;
            this.lblSmtpUsername.Location = new Point(10, 103);
            this.lblSmtpUsername.Name = "lblSmtpUsername";
            this.lblSmtpUsername.Size = new Size(102, 15);
            this.lblSmtpUsername.TabIndex = 5;
            this.lblSmtpUsername.Text = "SMTP Username:";
            //
            // txtSmtpUsername
            //
            this.txtSmtpUsername.Location = new Point(150, 100);
            this.txtSmtpUsername.Name = "txtSmtpUsername";
            this.txtSmtpUsername.Size = new Size(300, 23);
            this.txtSmtpUsername.TabIndex = 6;
            //
            // lblSmtpPassword
            //
            this.lblSmtpPassword.AutoSize = true;
            this.lblSmtpPassword.Location = new Point(10, 132);
            this.lblSmtpPassword.Name = "lblSmtpPassword";
            this.lblSmtpPassword.Size = new Size(99, 15);
            this.lblSmtpPassword.TabIndex = 7;
            this.lblSmtpPassword.Text = "SMTP Password:";
            //
            // txtSmtpPassword
            //
            this.txtSmtpPassword.Location = new Point(150, 129);
            this.txtSmtpPassword.Name = "txtSmtpPassword";
            this.txtSmtpPassword.Size = new Size(300, 23);
            this.txtSmtpPassword.TabIndex = 8;
            this.txtSmtpPassword.UseSystemPasswordChar = true;
            //
            // lblFromEmail
            //
            this.lblFromEmail.AutoSize = true;
            this.lblFromEmail.Location = new Point(10, 161);
            this.lblFromEmail.Name = "lblFromEmail";
            this.lblFromEmail.Size = new Size(72, 15);
            this.lblFromEmail.TabIndex = 9;
            this.lblFromEmail.Text = "From Email:";
            //
            // txtFromEmail
            //
            this.txtFromEmail.Location = new Point(150, 158);
            this.txtFromEmail.Name = "txtFromEmail";
            this.txtFromEmail.Size = new Size(300, 23);
            this.txtFromEmail.TabIndex = 10;
            //
            // lblFromName
            //
            this.lblFromName.AutoSize = true;
            this.lblFromName.Location = new Point(10, 190);
            this.lblFromName.Name = "lblFromName";
            this.lblFromName.Size = new Size(73, 15);
            this.lblFromName.TabIndex = 11;
            this.lblFromName.Text = "From Name:";
            //
            // txtFromName
            //
            this.txtFromName.Location = new Point(150, 187);
            this.txtFromName.Name = "txtFromName";
            this.txtFromName.Size = new Size(300, 23);
            this.txtFromName.TabIndex = 12;
            //
            // FeaturesControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.mainPanel);
            this.Name = "FeaturesControl";
            this.Size = new Size(800, 600);
            this.mainPanel.ResumeLayout(false);
            this.grpEmail.ResumeLayout(false);
            this.grpEmail.PerformLayout();
            this.grpEmailOptions.ResumeLayout(false);
            this.grpEmailOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSmtpPort)).EndInit();
            this.grpCors.ResumeLayout(false);
            this.grpCors.PerformLayout();
            this.grpCorsOptions.ResumeLayout(false);
            this.grpCorsOptions.PerformLayout();
            this.grpAdminSeed.ResumeLayout(false);
            this.grpAdminSeed.PerformLayout();
            this.grpAdminSeedOptions.ResumeLayout(false);
            this.grpAdminSeedOptions.PerformLayout();
            this.grpExceptionHandling.ResumeLayout(false);
            this.grpExceptionHandling.PerformLayout();
            this.grpExceptionHandlingOptions.ResumeLayout(false);
            this.grpExceptionHandlingOptions.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private Panel mainPanel;
        private GroupBox grpExceptionHandling;
        private CheckBox chkExceptionHandling;
        private GroupBox grpExceptionHandlingOptions;
        private CheckBox chkUseDetailedErrors;
        private CheckBox chkLogExceptions;
        private GroupBox grpAdminSeed;
        private CheckBox chkAdminSeed;
        private GroupBox grpAdminSeedOptions;
        private CheckBox chkCreateDefaultAdmin;
        private Label lblAdminEmail;
        private TextBox txtAdminEmail;
        private Label lblAdminPassword;
        private TextBox txtAdminPassword;
        private GroupBox grpCors;
        private CheckBox chkCors;
        private GroupBox grpCorsOptions;
        private Label lblAllowedOrigins;
        private TextBox txtAllowedOrigins;
        private GroupBox grpEmail;
        private CheckBox chkEmail;
        private GroupBox grpEmailOptions;
        private Label lblSmtpHost;
        private TextBox txtSmtpHost;
        private Label lblSmtpPort;
        private NumericUpDown numSmtpPort;
        private CheckBox chkEnableSsl;
        private Label lblSmtpUsername;
        private TextBox txtSmtpUsername;
        private Label lblSmtpPassword;
        private TextBox txtSmtpPassword;
        private Label lblFromEmail;
        private TextBox txtFromEmail;
        private Label lblFromName;
        private TextBox txtFromName;
    }
}
