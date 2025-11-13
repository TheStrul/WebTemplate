namespace WebTemplate.Setup.UI
{
    partial class AuthControl
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && (components != null)) { components.Dispose(); } base.Dispose(disposing); }

        #region Component Designer generated code
        private void InitializeComponent()
        {
            this.mainPanel = new Panel();
            this.grpJwt = new GroupBox();
            this.btnGenerateKey = new Button();
            this.txtSecretKey = new TextBox();
            this.lblSecretKey = new Label();
            this.txtAudience = new TextBox();
            this.lblAudience = new Label();
            this.txtIssuer = new TextBox();
            this.lblIssuer = new Label();
            this.grpExpiration = new GroupBox();
            this.numRefreshTokenExpiration = new NumericUpDown();
            this.lblRefreshTokenExpiration = new Label();
            this.numAccessTokenExpiration = new NumericUpDown();
            this.lblAccessTokenExpiration = new Label();
            this.mainPanel.SuspendLayout();
            this.grpJwt.SuspendLayout();
            this.grpExpiration.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRefreshTokenExpiration)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAccessTokenExpiration)).BeginInit();
            this.SuspendLayout();
            // mainPanel
            this.mainPanel.AutoScroll = true;
            this.mainPanel.Controls.Add(this.grpExpiration);
            this.mainPanel.Controls.Add(this.grpJwt);
            this.mainPanel.Dock = DockStyle.Fill;
            this.mainPanel.Location = new Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new Padding(10);
            this.mainPanel.Size = new Size(800, 600);
            this.mainPanel.TabIndex = 0;
            // grpJwt
            this.grpJwt.Controls.Add(this.btnGenerateKey);
            this.grpJwt.Controls.Add(this.txtSecretKey);
            this.grpJwt.Controls.Add(this.lblSecretKey);
            this.grpJwt.Controls.Add(this.txtAudience);
            this.grpJwt.Controls.Add(this.lblAudience);
            this.grpJwt.Controls.Add(this.txtIssuer);
            this.grpJwt.Controls.Add(this.lblIssuer);
            this.grpJwt.Dock = DockStyle.Top;
            this.grpJwt.Location = new Point(10, 10);
            this.grpJwt.Name = "grpJwt";
            this.grpJwt.Padding = new Padding(10);
            this.grpJwt.Size = new Size(780, 150);
            this.grpJwt.TabIndex = 0;
            this.grpJwt.TabStop = false;
            this.grpJwt.Text = "JWT Configuration";
            // lblSecretKey
            this.lblSecretKey.AutoSize = true;
            this.lblSecretKey.Location = new Point(20, 30);
            this.lblSecretKey.Name = "lblSecretKey";
            this.lblSecretKey.Size = new Size(103, 15);
            this.lblSecretKey.TabIndex = 0;
            this.lblSecretKey.Text = "Secret Key (min 32 chars):";
            // txtSecretKey
            this.txtSecretKey.Location = new Point(180, 27);
            this.txtSecretKey.Name = "txtSecretKey";
            this.txtSecretKey.Size = new Size(450, 23);
            this.txtSecretKey.TabIndex = 1;
            this.txtSecretKey.UseSystemPasswordChar = true;
            // btnGenerateKey
            this.btnGenerateKey.Location = new Point(640, 26);
            this.btnGenerateKey.Name = "btnGenerateKey";
            this.btnGenerateKey.Size = new Size(120, 25);
            this.btnGenerateKey.TabIndex = 2;
            this.btnGenerateKey.Text = "Generate Random";
            this.btnGenerateKey.UseVisualStyleBackColor = true;
            this.btnGenerateKey.Click += new EventHandler(this.btnGenerateKey_Click);
            // lblIssuer
            this.lblIssuer.AutoSize = true;
            this.lblIssuer.Location = new Point(20, 63);
            this.lblIssuer.Name = "lblIssuer";
            this.lblIssuer.Size = new Size(40, 15);
            this.lblIssuer.TabIndex = 3;
            this.lblIssuer.Text = "Issuer:";
            // txtIssuer
            this.txtIssuer.Location = new Point(180, 60);
            this.txtIssuer.Name = "txtIssuer";
            this.txtIssuer.Size = new Size(450, 23);
            this.txtIssuer.TabIndex = 4;
            // lblAudience
            this.lblAudience.AutoSize = true;
            this.lblAudience.Location = new Point(20, 96);
            this.lblAudience.Name = "lblAudience";
            this.lblAudience.Size = new Size(60, 15);
            this.lblAudience.TabIndex = 5;
            this.lblAudience.Text = "Audience:";
            // txtAudience
            this.txtAudience.Location = new Point(180, 93);
            this.txtAudience.Name = "txtAudience";
            this.txtAudience.Size = new Size(450, 23);
            this.txtAudience.TabIndex = 6;
            // grpExpiration
            this.grpExpiration.Controls.Add(this.numRefreshTokenExpiration);
            this.grpExpiration.Controls.Add(this.lblRefreshTokenExpiration);
            this.grpExpiration.Controls.Add(this.numAccessTokenExpiration);
            this.grpExpiration.Controls.Add(this.lblAccessTokenExpiration);
            this.grpExpiration.Dock = DockStyle.Top;
            this.grpExpiration.Location = new Point(10, 160);
            this.grpExpiration.Name = "grpExpiration";
            this.grpExpiration.Padding = new Padding(10);
            this.grpExpiration.Size = new Size(780, 100);
            this.grpExpiration.TabIndex = 1;
            this.grpExpiration.TabStop = false;
            this.grpExpiration.Text = "Token Expiration";
            // lblAccessTokenExpiration
            this.lblAccessTokenExpiration.AutoSize = true;
            this.lblAccessTokenExpiration.Location = new Point(20, 30);
            this.lblAccessTokenExpiration.Name = "lblAccessTokenExpiration";
            this.lblAccessTokenExpiration.Size = new Size(185, 15);
            this.lblAccessTokenExpiration.TabIndex = 0;
            this.lblAccessTokenExpiration.Text = "Access Token Expiration (minutes):";
            // numAccessTokenExpiration
            this.numAccessTokenExpiration.Location = new Point(220, 28);
            this.numAccessTokenExpiration.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
            this.numAccessTokenExpiration.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            this.numAccessTokenExpiration.Name = "numAccessTokenExpiration";
            this.numAccessTokenExpiration.Size = new Size(100, 23);
            this.numAccessTokenExpiration.TabIndex = 1;
            this.numAccessTokenExpiration.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // lblRefreshTokenExpiration
            this.lblRefreshTokenExpiration.AutoSize = true;
            this.lblRefreshTokenExpiration.Location = new Point(20, 63);
            this.lblRefreshTokenExpiration.Name = "lblRefreshTokenExpiration";
            this.lblRefreshTokenExpiration.Size = new Size(177, 15);
            this.lblRefreshTokenExpiration.TabIndex = 2;
            this.lblRefreshTokenExpiration.Text = "Refresh Token Expiration (days):";
            // numRefreshTokenExpiration
            this.numRefreshTokenExpiration.Location = new Point(220, 61);
            this.numRefreshTokenExpiration.Maximum = new decimal(new int[] { 90, 0, 0, 0 });
            this.numRefreshTokenExpiration.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numRefreshTokenExpiration.Name = "numRefreshTokenExpiration";
            this.numRefreshTokenExpiration.Size = new Size(100, 23);
            this.numRefreshTokenExpiration.TabIndex = 3;
            this.numRefreshTokenExpiration.Value = new decimal(new int[] { 7, 0, 0, 0 });
            // AuthControl
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.mainPanel);
            this.Name = "AuthControl";
            this.Size = new Size(800, 600);
            this.mainPanel.ResumeLayout(false);
            this.grpJwt.ResumeLayout(false);
            this.grpJwt.PerformLayout();
            this.grpExpiration.ResumeLayout(false);
            this.grpExpiration.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRefreshTokenExpiration)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAccessTokenExpiration)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private Panel mainPanel;
        private GroupBox grpJwt;
        private Label lblSecretKey;
        private TextBox txtSecretKey;
        private Button btnGenerateKey;
        private Label lblIssuer;
        private TextBox txtIssuer;
        private Label lblAudience;
        private TextBox txtAudience;
        private GroupBox grpExpiration;
        private Label lblAccessTokenExpiration;
        private NumericUpDown numAccessTokenExpiration;
        private Label lblRefreshTokenExpiration;
        private NumericUpDown numRefreshTokenExpiration;
    }
}
