namespace WebTemplate.Setup.UI
{
    partial class ServerControl
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && (components != null)) { components.Dispose(); } base.Dispose(disposing); }

        #region Component Designer generated code
        private void InitializeComponent()
        {
            this.mainPanel = new Panel();
            this.grpUrls = new GroupBox();
            this.txtHttpsUrl = new TextBox();
            this.lblHttpsUrl = new Label();
            this.txtUrl = new TextBox();
            this.lblUrl = new Label();
            this.grpTimeouts = new GroupBox();
            this.numRequestTimeout = new NumericUpDown();
            this.lblRequestTimeout = new Label();
            this.numConnectionTimeout = new NumericUpDown();
            this.lblConnectionTimeout = new Label();
            this.mainPanel.SuspendLayout();
            this.grpUrls.SuspendLayout();
            this.grpTimeouts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRequestTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numConnectionTimeout)).BeginInit();
            this.SuspendLayout();
            // mainPanel
            this.mainPanel.AutoScroll = true;
            this.mainPanel.Controls.Add(this.grpTimeouts);
            this.mainPanel.Controls.Add(this.grpUrls);
            this.mainPanel.Dock = DockStyle.Fill;
            this.mainPanel.Location = new Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new Padding(10);
            this.mainPanel.Size = new Size(800, 600);
            this.mainPanel.TabIndex = 0;
            // grpUrls
            this.grpUrls.Controls.Add(this.txtHttpsUrl);
            this.grpUrls.Controls.Add(this.lblHttpsUrl);
            this.grpUrls.Controls.Add(this.txtUrl);
            this.grpUrls.Controls.Add(this.lblUrl);
            this.grpUrls.Dock = DockStyle.Top;
            this.grpUrls.Location = new Point(10, 10);
            this.grpUrls.Name = "grpUrls";
            this.grpUrls.Padding = new Padding(10);
            this.grpUrls.Size = new Size(780, 100);
            this.grpUrls.TabIndex = 0;
            this.grpUrls.TabStop = false;
            this.grpUrls.Text = "Server URLs";
            // lblUrl
            this.lblUrl.AutoSize = true;
            this.lblUrl.Location = new Point(20, 30);
            this.lblUrl.Name = "lblUrl";
            this.lblUrl.Size = new Size(62, 15);
            this.lblUrl.TabIndex = 0;
            this.lblUrl.Text = "HTTP URL:";
            // txtUrl
            this.txtUrl.Location = new Point(150, 27);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new Size(600, 23);
            this.txtUrl.TabIndex = 1;
            // lblHttpsUrl
            this.lblHttpsUrl.AutoSize = true;
            this.lblHttpsUrl.Location = new Point(20, 59);
            this.lblHttpsUrl.Name = "lblHttpsUrl";
            this.lblHttpsUrl.Size = new Size(68, 15);
            this.lblHttpsUrl.TabIndex = 2;
            this.lblHttpsUrl.Text = "HTTPS URL:";
            // txtHttpsUrl
            this.txtHttpsUrl.Location = new Point(150, 56);
            this.txtHttpsUrl.Name = "txtHttpsUrl";
            this.txtHttpsUrl.Size = new Size(600, 23);
            this.txtHttpsUrl.TabIndex = 3;
            // grpTimeouts
            this.grpTimeouts.Controls.Add(this.numRequestTimeout);
            this.grpTimeouts.Controls.Add(this.lblRequestTimeout);
            this.grpTimeouts.Controls.Add(this.numConnectionTimeout);
            this.grpTimeouts.Controls.Add(this.lblConnectionTimeout);
            this.grpTimeouts.Dock = DockStyle.Top;
            this.grpTimeouts.Location = new Point(10, 110);
            this.grpTimeouts.Name = "grpTimeouts";
            this.grpTimeouts.Padding = new Padding(10);
            this.grpTimeouts.Size = new Size(780, 100);
            this.grpTimeouts.TabIndex = 1;
            this.grpTimeouts.TabStop = false;
            this.grpTimeouts.Text = "Timeouts";
            // lblConnectionTimeout
            this.lblConnectionTimeout.AutoSize = true;
            this.lblConnectionTimeout.Location = new Point(20, 30);
            this.lblConnectionTimeout.Name = "lblConnectionTimeout";
            this.lblConnectionTimeout.Size = new Size(161, 15);
            this.lblConnectionTimeout.TabIndex = 0;
            this.lblConnectionTimeout.Text = "Connection Timeout (seconds):";
            // numConnectionTimeout
            this.numConnectionTimeout.Location = new Point(200, 28);
            this.numConnectionTimeout.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            this.numConnectionTimeout.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            this.numConnectionTimeout.Name = "numConnectionTimeout";
            this.numConnectionTimeout.Size = new Size(100, 23);
            this.numConnectionTimeout.TabIndex = 1;
            this.numConnectionTimeout.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // lblRequestTimeout
            this.lblRequestTimeout.AutoSize = true;
            this.lblRequestTimeout.Location = new Point(20, 59);
            this.lblRequestTimeout.Name = "lblRequestTimeout";
            this.lblRequestTimeout.Size = new Size(145, 15);
            this.lblRequestTimeout.TabIndex = 2;
            this.lblRequestTimeout.Text = "Request Timeout (seconds):";
            // numRequestTimeout
            this.numRequestTimeout.Location = new Point(200, 57);
            this.numRequestTimeout.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            this.numRequestTimeout.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            this.numRequestTimeout.Name = "numRequestTimeout";
            this.numRequestTimeout.Size = new Size(100, 23);
            this.numRequestTimeout.TabIndex = 3;
            this.numRequestTimeout.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // ServerControl
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.mainPanel);
            this.Name = "ServerControl";
            this.Size = new Size(800, 600);
            this.mainPanel.ResumeLayout(false);
            this.grpUrls.ResumeLayout(false);
            this.grpUrls.PerformLayout();
            this.grpTimeouts.ResumeLayout(false);
            this.grpTimeouts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRequestTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numConnectionTimeout)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private Panel mainPanel;
        private GroupBox grpUrls;
        private TextBox txtUrl;
        private Label lblUrl;
        private TextBox txtHttpsUrl;
        private Label lblHttpsUrl;
        private GroupBox grpTimeouts;
        private NumericUpDown numConnectionTimeout;
        private Label lblConnectionTimeout;
        private NumericUpDown numRequestTimeout;
        private Label lblRequestTimeout;
    }
}
