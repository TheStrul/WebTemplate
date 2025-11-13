namespace WebTemplate.Setup;

partial class SetupForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.DataGridView secretsGrid = null!;
    private System.Windows.Forms.Button btnSave = null!;
    private System.Windows.Forms.Button btnReload = null!;
    private System.Windows.Forms.Label lblTitle = null!;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.secretsGrid = new System.Windows.Forms.DataGridView();
        this.btnSave = new System.Windows.Forms.Button();
        this.btnReload = new System.Windows.Forms.Button();
        this.lblTitle = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.secretsGrid)).BeginInit();
        this.SuspendLayout();
        // 
        // lblTitle
        // 
        this.lblTitle.AutoSize = true;
        this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblTitle.Location = new System.Drawing.Point(12, 9);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(279, 21);
        this.lblTitle.TabIndex = 3;
        this.lblTitle.Text = "WebTemplate Setup - User Secrets";
        // 
        // secretsGrid
        // 
        this.secretsGrid.AllowUserToAddRows = false;
        this.secretsGrid.AllowUserToDeleteRows = false;
        this.secretsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.secretsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.secretsGrid.Location = new System.Drawing.Point(12, 43);
        this.secretsGrid.MultiSelect = false;
        this.secretsGrid.Name = "secretsGrid";
        this.secretsGrid.RowTemplate.Height = 25;
        this.secretsGrid.Size = new System.Drawing.Size(776, 358);
        this.secretsGrid.TabIndex = 0;
        // 
        // btnReload
        // 
        this.btnReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.btnReload.Location = new System.Drawing.Point(592, 415);
        this.btnReload.Name = "btnReload";
        this.btnReload.Size = new System.Drawing.Size(90, 27);
        this.btnReload.TabIndex = 1;
        this.btnReload.Text = "Reload";
        this.btnReload.UseVisualStyleBackColor = true;
        this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
        // 
        // btnSave
        // 
        this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.btnSave.Location = new System.Drawing.Point(688, 415);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(100, 27);
        this.btnSave.TabIndex = 2;
        this.btnSave.Text = "Save";
        this.btnSave.UseVisualStyleBackColor = true;
        this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
        // 
        // SetupForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.btnSave);
        this.Controls.Add(this.btnReload);
        this.Controls.Add(this.secretsGrid);
        this.MinimumSize = new System.Drawing.Size(640, 360);
        this.Name = "SetupForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "WebTemplate Setup";
        ((System.ComponentModel.ISupportInitialize)(this.secretsGrid)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
}
