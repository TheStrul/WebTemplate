namespace WebTemplate.Setup.UI
{
    partial class SecretsControl
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
            this.btnGenerateValidValues = new Button();
            this.grpKeyVaultSettings = new GroupBox();
            this.chkUploadSecretsNow = new CheckBox();
            this.txtKeyVaultUrl = new TextBox();
            this.lblKeyVaultUrl = new Label();
            this.grpStrategy = new GroupBox();
            this.lblMixedDescription = new Label();
            this.rdoMixed = new RadioButton();
            this.lblEnvironmentDescription = new Label();
            this.rdoEnvironment = new RadioButton();
            this.lblKeyVaultDescription = new Label();
            this.rdoKeyVault = new RadioButton();
            this.lblUserSecretsDescription = new Label();
            this.rdoUserSecrets = new RadioButton();
            this.mainPanel.SuspendLayout();
            this.grpKeyVaultSettings.SuspendLayout();
            this.grpStrategy.SuspendLayout();
            this.SuspendLayout();
            //
            // mainPanel
            //
            this.mainPanel.AutoScroll = true;
            this.mainPanel.Controls.Add(this.btnGenerateValidValues);
            this.mainPanel.Controls.Add(this.grpKeyVaultSettings);
            this.mainPanel.Controls.Add(this.grpStrategy);
            this.mainPanel.Dock = DockStyle.Fill;
            this.mainPanel.Location = new Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new Padding(10);
            this.mainPanel.Size = new Size(800, 600);
            this.mainPanel.TabIndex = 0;
            //
            // btnGenerateValidValues
            //
            this.btnGenerateValidValues.Dock = DockStyle.Top;
            this.btnGenerateValidValues.Location = new Point(10, 10);
            this.btnGenerateValidValues.Name = "btnGenerateValidValues";
            this.btnGenerateValidValues.Size = new Size(780, 35);
            this.btnGenerateValidValues.TabIndex = 2;
            this.btnGenerateValidValues.Text = "Generate Valid Values";
            this.btnGenerateValidValues.UseVisualStyleBackColor = true;
            this.btnGenerateValidValues.Click += new EventHandler(this.btnGenerateValidValues_Click);
            //
            // grpStrategy
            //
            this.grpStrategy.Controls.Add(this.lblMixedDescription);
            this.grpStrategy.Controls.Add(this.rdoMixed);
            this.grpStrategy.Controls.Add(this.lblEnvironmentDescription);
            this.grpStrategy.Controls.Add(this.rdoEnvironment);
            this.grpStrategy.Controls.Add(this.lblKeyVaultDescription);
            this.grpStrategy.Controls.Add(this.rdoKeyVault);
            this.grpStrategy.Controls.Add(this.lblUserSecretsDescription);
            this.grpStrategy.Controls.Add(this.rdoUserSecrets);
            this.grpStrategy.Dock = DockStyle.Top;
            this.grpStrategy.Location = new Point(10, 10);
            this.grpStrategy.Name = "grpStrategy";
            this.grpStrategy.Padding = new Padding(10);
            this.grpStrategy.Size = new Size(780, 250);
            this.grpStrategy.TabIndex = 0;
            this.grpStrategy.TabStop = false;
            this.grpStrategy.Text = "Secrets Strategy";
            //
            // rdoUserSecrets
            //
            this.rdoUserSecrets.AutoSize = true;
            this.rdoUserSecrets.Location = new Point(20, 30);
            this.rdoUserSecrets.Name = "rdoUserSecrets";
            this.rdoUserSecrets.Size = new Size(171, 19);
            this.rdoUserSecrets.TabIndex = 0;
            this.rdoUserSecrets.TabStop = true;
            this.rdoUserSecrets.Text = "User Secrets (Development)";
            this.rdoUserSecrets.UseVisualStyleBackColor = true;
            //
            // lblUserSecretsDescription
            //
            this.lblUserSecretsDescription.AutoSize = true;
            this.lblUserSecretsDescription.ForeColor = SystemColors.GrayText;
            this.lblUserSecretsDescription.Location = new Point(40, 52);
            this.lblUserSecretsDescription.Name = "lblUserSecretsDescription";
            this.lblUserSecretsDescription.Size = new Size(450, 15);
            this.lblUserSecretsDescription.TabIndex = 1;
            this.lblUserSecretsDescription.Text = "Store secrets in user secrets manager (best for local development)";
            //
            // rdoKeyVault
            //
            this.rdoKeyVault.AutoSize = true;
            this.rdoKeyVault.Location = new Point(20, 80);
            this.rdoKeyVault.Name = "rdoKeyVault";
            this.rdoKeyVault.Size = new Size(188, 19);
            this.rdoKeyVault.TabIndex = 2;
            this.rdoKeyVault.TabStop = true;
            this.rdoKeyVault.Text = "Azure Key Vault (Production)";
            this.rdoKeyVault.UseVisualStyleBackColor = true;
            //
            // lblKeyVaultDescription
            //
            this.lblKeyVaultDescription.AutoSize = true;
            this.lblKeyVaultDescription.ForeColor = SystemColors.GrayText;
            this.lblKeyVaultDescription.Location = new Point(40, 102);
            this.lblKeyVaultDescription.Name = "lblKeyVaultDescription";
            this.lblKeyVaultDescription.Size = new Size(450, 15);
            this.lblKeyVaultDescription.TabIndex = 3;
            this.lblKeyVaultDescription.Text = "Store secrets in Azure Key Vault (best for production)";
            //
            // rdoEnvironment
            //
            this.rdoEnvironment.AutoSize = true;
            this.rdoEnvironment.Location = new Point(20, 130);
            this.rdoEnvironment.Name = "rdoEnvironment";
            this.rdoEnvironment.Size = new Size(153, 19);
            this.rdoEnvironment.TabIndex = 4;
            this.rdoEnvironment.TabStop = true;
            this.rdoEnvironment.Text = "Environment Variables";
            this.rdoEnvironment.UseVisualStyleBackColor = true;
            //
            // lblEnvironmentDescription
            //
            this.lblEnvironmentDescription.AutoSize = true;
            this.lblEnvironmentDescription.ForeColor = SystemColors.GrayText;
            this.lblEnvironmentDescription.Location = new Point(40, 152);
            this.lblEnvironmentDescription.Name = "lblEnvironmentDescription";
            this.lblEnvironmentDescription.Size = new Size(450, 15);
            this.lblEnvironmentDescription.TabIndex = 5;
            this.lblEnvironmentDescription.Text = "Store secrets in environment variables (generates secrets.env file)";
            //
            // rdoMixed
            //
            this.rdoMixed.AutoSize = true;
            this.rdoMixed.Location = new Point(20, 180);
            this.rdoMixed.Name = "rdoMixed";
            this.rdoMixed.Size = new Size(243, 19);
            this.rdoMixed.TabIndex = 6;
            this.rdoMixed.TabStop = true;
            this.rdoMixed.Text = "Mixed (User Secrets + Azure Key Vault)";
            this.rdoMixed.UseVisualStyleBackColor = true;
            //
            // lblMixedDescription
            //
            this.lblMixedDescription.AutoSize = true;
            this.lblMixedDescription.ForeColor = SystemColors.GrayText;
            this.lblMixedDescription.Location = new Point(40, 202);
            this.lblMixedDescription.Name = "lblMixedDescription";
            this.lblMixedDescription.Size = new Size(550, 15);
            this.lblMixedDescription.TabIndex = 7;
            this.lblMixedDescription.Text = "User Secrets for development, Azure Key Vault for production (recommended)";
            //
            // grpKeyVaultSettings
            //
            this.grpKeyVaultSettings.Controls.Add(this.chkUploadSecretsNow);
            this.grpKeyVaultSettings.Controls.Add(this.txtKeyVaultUrl);
            this.grpKeyVaultSettings.Controls.Add(this.lblKeyVaultUrl);
            this.grpKeyVaultSettings.Dock = DockStyle.Top;
            this.grpKeyVaultSettings.Location = new Point(10, 260);
            this.grpKeyVaultSettings.Name = "grpKeyVaultSettings";
            this.grpKeyVaultSettings.Padding = new Padding(10);
            this.grpKeyVaultSettings.Size = new Size(780, 100);
            this.grpKeyVaultSettings.TabIndex = 1;
            this.grpKeyVaultSettings.TabStop = false;
            this.grpKeyVaultSettings.Text = "Azure Key Vault Settings";
            //
            // lblKeyVaultUrl
            //
            this.lblKeyVaultUrl.AutoSize = true;
            this.lblKeyVaultUrl.Location = new Point(20, 30);
            this.lblKeyVaultUrl.Name = "lblKeyVaultUrl";
            this.lblKeyVaultUrl.Size = new Size(91, 15);
            this.lblKeyVaultUrl.TabIndex = 0;
            this.lblKeyVaultUrl.Text = "Key Vault URL:";
            //
            // txtKeyVaultUrl
            //
            this.txtKeyVaultUrl.Location = new Point(150, 27);
            this.txtKeyVaultUrl.Name = "txtKeyVaultUrl";
            this.txtKeyVaultUrl.PlaceholderText = "https://your-keyvault.vault.azure.net/";
            this.txtKeyVaultUrl.Size = new Size(500, 23);
            this.txtKeyVaultUrl.TabIndex = 1;
            //
            // chkUploadSecretsNow
            //
            this.chkUploadSecretsNow.AutoSize = true;
            this.chkUploadSecretsNow.Location = new Point(150, 56);
            this.chkUploadSecretsNow.Name = "chkUploadSecretsNow";
            this.chkUploadSecretsNow.Size = new Size(380, 19);
            this.chkUploadSecretsNow.TabIndex = 2;
            this.chkUploadSecretsNow.Text = "Upload secrets to Key Vault now (requires Azure CLI authentication)";
            this.chkUploadSecretsNow.UseVisualStyleBackColor = true;
            //
            // SecretsControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.mainPanel);
            this.Name = "SecretsControl";
            this.Size = new Size(800, 600);
            this.mainPanel.ResumeLayout(false);
            this.grpKeyVaultSettings.ResumeLayout(false);
            this.grpKeyVaultSettings.PerformLayout();
            this.grpStrategy.ResumeLayout(false);
            this.grpStrategy.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private Panel mainPanel;
        private Button btnGenerateValidValues;
        private GroupBox grpStrategy;
        private RadioButton rdoUserSecrets;
        private Label lblUserSecretsDescription;
        private RadioButton rdoKeyVault;
        private Label lblKeyVaultDescription;
        private RadioButton rdoEnvironment;
        private Label lblEnvironmentDescription;
        private RadioButton rdoMixed;
        private Label lblMixedDescription;
        private GroupBox grpKeyVaultSettings;
        private Label lblKeyVaultUrl;
        private TextBox txtKeyVaultUrl;
        private CheckBox chkUploadSecretsNow;
    }
}
