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
            mainPanel = new Panel();
            btnGenerateValidValues = new Button();
            grpEmail = new GroupBox();
            grpEmailOptions = new GroupBox();
            txtFromName = new TextBox();
            lblFromName = new Label();
            txtFromEmail = new TextBox();
            lblFromEmail = new Label();
            txtSmtpPassword = new TextBox();
            lblSmtpPassword = new Label();
            txtSmtpUsername = new TextBox();
            lblSmtpUsername = new Label();
            chkEnableSsl = new CheckBox();
            numSmtpPort = new NumericUpDown();
            lblSmtpPort = new Label();
            txtSmtpHost = new TextBox();
            lblSmtpHost = new Label();
            chkEmail = new CheckBox();
            grpCors = new GroupBox();
            grpCorsOptions = new GroupBox();
            txtAllowedOrigins = new TextBox();
            lblAllowedOrigins = new Label();
            chkCors = new CheckBox();
            grpAdminSeed = new GroupBox();
            grpAdminSeedOptions = new GroupBox();
            txtAdminPassword = new TextBox();
            lblAdminPassword = new Label();
            txtAdminEmail = new TextBox();
            lblAdminEmail = new Label();
            chkCreateDefaultAdmin = new CheckBox();
            chkAdminSeed = new CheckBox();
            grpExceptionHandling = new GroupBox();
            grpExceptionHandlingOptions = new GroupBox();
            chkLogExceptions = new CheckBox();
            chkUseDetailedErrors = new CheckBox();
            chkExceptionHandling = new CheckBox();
            mainPanel.SuspendLayout();
            grpEmail.SuspendLayout();
            grpEmailOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numSmtpPort).BeginInit();
            grpCors.SuspendLayout();
            grpCorsOptions.SuspendLayout();
            grpAdminSeed.SuspendLayout();
            grpAdminSeedOptions.SuspendLayout();
            grpExceptionHandling.SuspendLayout();
            grpExceptionHandlingOptions.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.AutoScroll = true;
            mainPanel.Controls.Add(btnGenerateValidValues);
            mainPanel.Controls.Add(grpEmail);
            mainPanel.Controls.Add(grpCors);
            mainPanel.Controls.Add(grpAdminSeed);
            mainPanel.Controls.Add(grpExceptionHandling);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Padding = new Padding(10);
            mainPanel.Size = new Size(800, 705);
            mainPanel.TabIndex = 0;
            // 
            // btnGenerateValidValues
            // 
            btnGenerateValidValues.Dock = DockStyle.Top;
            btnGenerateValidValues.Location = new Point(10, 660);
            btnGenerateValidValues.Name = "btnGenerateValidValues";
            btnGenerateValidValues.Size = new Size(780, 35);
            btnGenerateValidValues.TabIndex = 4;
            btnGenerateValidValues.Text = "Generate Valid Values";
            btnGenerateValidValues.UseVisualStyleBackColor = true;
            btnGenerateValidValues.Click += btnGenerateValidValues_Click;
            // 
            // grpEmail
            // 
            grpEmail.Controls.Add(grpEmailOptions);
            grpEmail.Controls.Add(chkEmail);
            grpEmail.Dock = DockStyle.Top;
            grpEmail.Location = new Point(10, 380);
            grpEmail.Name = "grpEmail";
            grpEmail.Padding = new Padding(10);
            grpEmail.Size = new Size(780, 280);
            grpEmail.TabIndex = 3;
            grpEmail.TabStop = false;
            grpEmail.Text = "Email Configuration";
            // 
            // grpEmailOptions
            // 
            grpEmailOptions.Controls.Add(txtFromName);
            grpEmailOptions.Controls.Add(lblFromName);
            grpEmailOptions.Controls.Add(txtFromEmail);
            grpEmailOptions.Controls.Add(lblFromEmail);
            grpEmailOptions.Controls.Add(txtSmtpPassword);
            grpEmailOptions.Controls.Add(lblSmtpPassword);
            grpEmailOptions.Controls.Add(txtSmtpUsername);
            grpEmailOptions.Controls.Add(lblSmtpUsername);
            grpEmailOptions.Controls.Add(chkEnableSsl);
            grpEmailOptions.Controls.Add(numSmtpPort);
            grpEmailOptions.Controls.Add(lblSmtpPort);
            grpEmailOptions.Controls.Add(txtSmtpHost);
            grpEmailOptions.Controls.Add(lblSmtpHost);
            grpEmailOptions.Dock = DockStyle.Fill;
            grpEmailOptions.Location = new Point(10, 45);
            grpEmailOptions.Name = "grpEmailOptions";
            grpEmailOptions.Padding = new Padding(10);
            grpEmailOptions.Size = new Size(760, 225);
            grpEmailOptions.TabIndex = 1;
            grpEmailOptions.TabStop = false;
            // 
            // txtFromName
            // 
            txtFromName.Location = new Point(150, 187);
            txtFromName.Name = "txtFromName";
            txtFromName.PlaceholderText = "Your Application Name";
            txtFromName.Size = new Size(300, 23);
            txtFromName.TabIndex = 12;
            // 
            // lblFromName
            // 
            lblFromName.AutoSize = true;
            lblFromName.Location = new Point(10, 190);
            lblFromName.Name = "lblFromName";
            lblFromName.Size = new Size(73, 15);
            lblFromName.TabIndex = 11;
            lblFromName.Text = "From Name:";
            // 
            // txtFromEmail
            // 
            txtFromEmail.Location = new Point(150, 158);
            txtFromEmail.Name = "txtFromEmail";
            txtFromEmail.PlaceholderText = "noreply@yourapp.com";
            txtFromEmail.Size = new Size(300, 23);
            txtFromEmail.TabIndex = 10;
            // 
            // lblFromEmail
            // 
            lblFromEmail.AutoSize = true;
            lblFromEmail.Location = new Point(10, 161);
            lblFromEmail.Name = "lblFromEmail";
            lblFromEmail.Size = new Size(70, 15);
            lblFromEmail.TabIndex = 9;
            lblFromEmail.Text = "From Email:";
            // 
            // txtSmtpPassword
            // 
            txtSmtpPassword.Location = new Point(150, 129);
            txtSmtpPassword.Name = "txtSmtpPassword";
            txtSmtpPassword.PlaceholderText = "Your SMTP password or app-specific password";
            txtSmtpPassword.Size = new Size(300, 23);
            txtSmtpPassword.TabIndex = 8;
            txtSmtpPassword.UseSystemPasswordChar = true;
            // 
            // lblSmtpPassword
            // 
            lblSmtpPassword.AutoSize = true;
            lblSmtpPassword.Location = new Point(10, 132);
            lblSmtpPassword.Name = "lblSmtpPassword";
            lblSmtpPassword.Size = new Size(94, 15);
            lblSmtpPassword.TabIndex = 7;
            lblSmtpPassword.Text = "SMTP Password:";
            // 
            // txtSmtpUsername
            // 
            txtSmtpUsername.Location = new Point(150, 100);
            txtSmtpUsername.Name = "txtSmtpUsername";
            txtSmtpUsername.PlaceholderText = "your-email@example.com";
            txtSmtpUsername.Size = new Size(300, 23);
            txtSmtpUsername.TabIndex = 6;
            // 
            // lblSmtpUsername
            // 
            lblSmtpUsername.AutoSize = true;
            lblSmtpUsername.Location = new Point(10, 103);
            lblSmtpUsername.Name = "lblSmtpUsername";
            lblSmtpUsername.Size = new Size(97, 15);
            lblSmtpUsername.TabIndex = 5;
            lblSmtpUsername.Text = "SMTP Username:";
            // 
            // chkEnableSsl
            // 
            chkEnableSsl.AutoSize = true;
            chkEnableSsl.Location = new Point(150, 74);
            chkEnableSsl.Name = "chkEnableSsl";
            chkEnableSsl.Size = new Size(82, 19);
            chkEnableSsl.TabIndex = 4;
            chkEnableSsl.Text = "Enable SSL";
            chkEnableSsl.UseVisualStyleBackColor = true;
            // 
            // numSmtpPort
            // 
            numSmtpPort.Location = new Point(150, 45);
            numSmtpPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSmtpPort.Name = "numSmtpPort";
            numSmtpPort.Size = new Size(100, 23);
            numSmtpPort.TabIndex = 3;
            numSmtpPort.Value = new decimal(new int[] { 587, 0, 0, 0 });
            // 
            // lblSmtpPort
            // 
            lblSmtpPort.AutoSize = true;
            lblSmtpPort.Location = new Point(10, 48);
            lblSmtpPort.Name = "lblSmtpPort";
            lblSmtpPort.Size = new Size(66, 15);
            lblSmtpPort.TabIndex = 2;
            lblSmtpPort.Text = "SMTP Port:";
            // 
            // txtSmtpHost
            // 
            txtSmtpHost.Location = new Point(150, 16);
            txtSmtpHost.Name = "txtSmtpHost";
            txtSmtpHost.PlaceholderText = "smtp.gmail.com";
            txtSmtpHost.Size = new Size(300, 23);
            txtSmtpHost.TabIndex = 1;
            // 
            // lblSmtpHost
            // 
            lblSmtpHost.AutoSize = true;
            lblSmtpHost.Location = new Point(10, 19);
            lblSmtpHost.Name = "lblSmtpHost";
            lblSmtpHost.Size = new Size(69, 15);
            lblSmtpHost.TabIndex = 0;
            lblSmtpHost.Text = "SMTP Host:";
            // 
            // chkEmail
            // 
            chkEmail.AutoSize = true;
            chkEmail.Dock = DockStyle.Top;
            chkEmail.Location = new Point(10, 26);
            chkEmail.Name = "chkEmail";
            chkEmail.Size = new Size(760, 19);
            chkEmail.TabIndex = 0;
            chkEmail.Text = "Enable Email Module";
            chkEmail.UseVisualStyleBackColor = true;
            // 
            // grpCors
            // 
            grpCors.Controls.Add(grpCorsOptions);
            grpCors.Controls.Add(chkCors);
            grpCors.Dock = DockStyle.Top;
            grpCors.Location = new Point(10, 280);
            grpCors.Name = "grpCors";
            grpCors.Padding = new Padding(10);
            grpCors.Size = new Size(780, 100);
            grpCors.TabIndex = 2;
            grpCors.TabStop = false;
            grpCors.Text = "CORS Configuration";
            // 
            // grpCorsOptions
            // 
            grpCorsOptions.Controls.Add(txtAllowedOrigins);
            grpCorsOptions.Controls.Add(lblAllowedOrigins);
            grpCorsOptions.Dock = DockStyle.Fill;
            grpCorsOptions.Location = new Point(10, 45);
            grpCorsOptions.Name = "grpCorsOptions";
            grpCorsOptions.Padding = new Padding(10);
            grpCorsOptions.Size = new Size(760, 45);
            grpCorsOptions.TabIndex = 1;
            grpCorsOptions.TabStop = false;
            // 
            // txtAllowedOrigins
            // 
            txtAllowedOrigins.Location = new Point(180, 16);
            txtAllowedOrigins.Name = "txtAllowedOrigins";
            txtAllowedOrigins.PlaceholderText = "http://localhost:3000,https://yourapp.com";
            txtAllowedOrigins.Size = new Size(500, 23);
            txtAllowedOrigins.TabIndex = 1;
            // 
            // lblAllowedOrigins
            // 
            lblAllowedOrigins.AutoSize = true;
            lblAllowedOrigins.Location = new Point(10, 19);
            lblAllowedOrigins.Name = "lblAllowedOrigins";
            lblAllowedOrigins.Size = new Size(202, 15);
            lblAllowedOrigins.TabIndex = 0;
            lblAllowedOrigins.Text = "Allowed Origins (comma-separated):";
            // 
            // chkCors
            // 
            chkCors.AutoSize = true;
            chkCors.Dock = DockStyle.Top;
            chkCors.Location = new Point(10, 26);
            chkCors.Name = "chkCors";
            chkCors.Size = new Size(760, 19);
            chkCors.TabIndex = 0;
            chkCors.Text = "Enable CORS";
            chkCors.UseVisualStyleBackColor = true;
            chkCors.CheckedChanged += chkCors_CheckedChanged;
            // 
            // grpAdminSeed
            // 
            grpAdminSeed.Controls.Add(grpAdminSeedOptions);
            grpAdminSeed.Controls.Add(chkAdminSeed);
            grpAdminSeed.Dock = DockStyle.Top;
            grpAdminSeed.Location = new Point(10, 130);
            grpAdminSeed.Name = "grpAdminSeed";
            grpAdminSeed.Padding = new Padding(10);
            grpAdminSeed.Size = new Size(780, 150);
            grpAdminSeed.TabIndex = 1;
            grpAdminSeed.TabStop = false;
            grpAdminSeed.Text = "Admin User Seeding";
            // 
            // grpAdminSeedOptions
            // 
            grpAdminSeedOptions.Controls.Add(txtAdminPassword);
            grpAdminSeedOptions.Controls.Add(lblAdminPassword);
            grpAdminSeedOptions.Controls.Add(txtAdminEmail);
            grpAdminSeedOptions.Controls.Add(lblAdminEmail);
            grpAdminSeedOptions.Controls.Add(chkCreateDefaultAdmin);
            grpAdminSeedOptions.Dock = DockStyle.Fill;
            grpAdminSeedOptions.Location = new Point(10, 45);
            grpAdminSeedOptions.Name = "grpAdminSeedOptions";
            grpAdminSeedOptions.Padding = new Padding(10);
            grpAdminSeedOptions.Size = new Size(760, 95);
            grpAdminSeedOptions.TabIndex = 1;
            grpAdminSeedOptions.TabStop = false;
            // 
            // txtAdminPassword
            // 
            txtAdminPassword.Location = new Point(150, 74);
            txtAdminPassword.Name = "txtAdminPassword";
            txtAdminPassword.PlaceholderText = "Strong password (min 8 chars, mixed case, numbers, symbols)";
            txtAdminPassword.Size = new Size(300, 23);
            txtAdminPassword.TabIndex = 4;
            txtAdminPassword.UseSystemPasswordChar = true;
            // 
            // lblAdminPassword
            // 
            lblAdminPassword.AutoSize = true;
            lblAdminPassword.Location = new Point(10, 77);
            lblAdminPassword.Name = "lblAdminPassword";
            lblAdminPassword.Size = new Size(99, 15);
            lblAdminPassword.TabIndex = 3;
            lblAdminPassword.Text = "Admin Password:";
            // 
            // txtAdminEmail
            // 
            txtAdminEmail.Location = new Point(150, 45);
            txtAdminEmail.Name = "txtAdminEmail";
            txtAdminEmail.PlaceholderText = "admin@yourapp.com";
            txtAdminEmail.Size = new Size(300, 23);
            txtAdminEmail.TabIndex = 2;
            // 
            // lblAdminEmail
            // 
            lblAdminEmail.AutoSize = true;
            lblAdminEmail.Location = new Point(10, 48);
            lblAdminEmail.Name = "lblAdminEmail";
            lblAdminEmail.Size = new Size(78, 15);
            lblAdminEmail.TabIndex = 1;
            lblAdminEmail.Text = "Admin Email:";
            // 
            // chkCreateDefaultAdmin
            // 
            chkCreateDefaultAdmin.AutoSize = true;
            chkCreateDefaultAdmin.Dock = DockStyle.Top;
            chkCreateDefaultAdmin.Location = new Point(10, 26);
            chkCreateDefaultAdmin.Name = "chkCreateDefaultAdmin";
            chkCreateDefaultAdmin.Size = new Size(740, 19);
            chkCreateDefaultAdmin.TabIndex = 0;
            chkCreateDefaultAdmin.Text = "Create Default Admin User";
            chkCreateDefaultAdmin.UseVisualStyleBackColor = true;
            // 
            // chkAdminSeed
            // 
            chkAdminSeed.AutoSize = true;
            chkAdminSeed.Dock = DockStyle.Top;
            chkAdminSeed.Location = new Point(10, 26);
            chkAdminSeed.Name = "chkAdminSeed";
            chkAdminSeed.Size = new Size(760, 19);
            chkAdminSeed.TabIndex = 0;
            chkAdminSeed.Text = "Enable Admin Seed Module";
            chkAdminSeed.UseVisualStyleBackColor = true;
            chkAdminSeed.CheckedChanged += chkAdminSeed_CheckedChanged;
            // 
            // grpExceptionHandling
            // 
            grpExceptionHandling.Controls.Add(grpExceptionHandlingOptions);
            grpExceptionHandling.Controls.Add(chkExceptionHandling);
            grpExceptionHandling.Dock = DockStyle.Top;
            grpExceptionHandling.Location = new Point(10, 10);
            grpExceptionHandling.Name = "grpExceptionHandling";
            grpExceptionHandling.Padding = new Padding(10);
            grpExceptionHandling.Size = new Size(780, 120);
            grpExceptionHandling.TabIndex = 0;
            grpExceptionHandling.TabStop = false;
            grpExceptionHandling.Text = "Exception Handling";
            // 
            // grpExceptionHandlingOptions
            // 
            grpExceptionHandlingOptions.Controls.Add(chkLogExceptions);
            grpExceptionHandlingOptions.Controls.Add(chkUseDetailedErrors);
            grpExceptionHandlingOptions.Dock = DockStyle.Fill;
            grpExceptionHandlingOptions.Location = new Point(10, 45);
            grpExceptionHandlingOptions.Name = "grpExceptionHandlingOptions";
            grpExceptionHandlingOptions.Padding = new Padding(10);
            grpExceptionHandlingOptions.Size = new Size(760, 65);
            grpExceptionHandlingOptions.TabIndex = 1;
            grpExceptionHandlingOptions.TabStop = false;
            // 
            // chkLogExceptions
            // 
            chkLogExceptions.AutoSize = true;
            chkLogExceptions.Dock = DockStyle.Top;
            chkLogExceptions.Location = new Point(10, 45);
            chkLogExceptions.Name = "chkLogExceptions";
            chkLogExceptions.Size = new Size(740, 19);
            chkLogExceptions.TabIndex = 1;
            chkLogExceptions.Text = "Log Exceptions";
            chkLogExceptions.UseVisualStyleBackColor = true;
            // 
            // chkUseDetailedErrors
            // 
            chkUseDetailedErrors.AutoSize = true;
            chkUseDetailedErrors.Dock = DockStyle.Top;
            chkUseDetailedErrors.Location = new Point(10, 26);
            chkUseDetailedErrors.Name = "chkUseDetailedErrors";
            chkUseDetailedErrors.Size = new Size(740, 19);
            chkUseDetailedErrors.TabIndex = 0;
            chkUseDetailedErrors.Text = "Use Detailed Errors (Development only)";
            chkUseDetailedErrors.UseVisualStyleBackColor = true;
            // 
            // chkExceptionHandling
            // 
            chkExceptionHandling.AutoSize = true;
            chkExceptionHandling.Dock = DockStyle.Top;
            chkExceptionHandling.Location = new Point(10, 26);
            chkExceptionHandling.Name = "chkExceptionHandling";
            chkExceptionHandling.Size = new Size(760, 19);
            chkExceptionHandling.TabIndex = 0;
            chkExceptionHandling.Text = "Enable Exception Handling";
            chkExceptionHandling.UseVisualStyleBackColor = true;
            chkExceptionHandling.CheckedChanged += chkExceptionHandling_CheckedChanged;
            // 
            // FeaturesControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(mainPanel);
            Name = "FeaturesControl";
            Size = new Size(800, 705);
            mainPanel.ResumeLayout(false);
            grpEmail.ResumeLayout(false);
            grpEmail.PerformLayout();
            grpEmailOptions.ResumeLayout(false);
            grpEmailOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numSmtpPort).EndInit();
            grpCors.ResumeLayout(false);
            grpCors.PerformLayout();
            grpCorsOptions.ResumeLayout(false);
            grpCorsOptions.PerformLayout();
            grpAdminSeed.ResumeLayout(false);
            grpAdminSeed.PerformLayout();
            grpAdminSeedOptions.ResumeLayout(false);
            grpAdminSeedOptions.PerformLayout();
            grpExceptionHandling.ResumeLayout(false);
            grpExceptionHandling.PerformLayout();
            grpExceptionHandlingOptions.ResumeLayout(false);
            grpExceptionHandlingOptions.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel mainPanel;
        private Button btnGenerateValidValues;
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
