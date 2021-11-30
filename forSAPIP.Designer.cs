namespace AB
{
    partial class forSAPIP
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(forSAPIP));
            this.tc = new System.Windows.Forms.TabControl();
            this.tpOpen = new System.Windows.Forms.TabPage();
            this.panelOpen = new System.Windows.Forms.Panel();
            this.tpClose = new System.Windows.Forms.TabPage();
            this.panelClose = new System.Windows.Forms.Panel();
            this.tc.SuspendLayout();
            this.tpOpen.SuspendLayout();
            this.tpClose.SuspendLayout();
            this.SuspendLayout();
            // 
            // tc
            // 
            this.tc.Controls.Add(this.tpOpen);
            this.tc.Controls.Add(this.tpClose);
            this.tc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tc.Location = new System.Drawing.Point(0, 0);
            this.tc.Name = "tc";
            this.tc.SelectedIndex = 0;
            this.tc.Size = new System.Drawing.Size(740, 521);
            this.tc.TabIndex = 0;
            this.tc.SelectedIndexChanged += new System.EventHandler(this.tcPaymentTypes_SelectedIndexChanged);
            // 
            // tpOpen
            // 
            this.tpOpen.Controls.Add(this.panelOpen);
            this.tpOpen.Location = new System.Drawing.Point(4, 22);
            this.tpOpen.Name = "tpOpen";
            this.tpOpen.Padding = new System.Windows.Forms.Padding(3);
            this.tpOpen.Size = new System.Drawing.Size(732, 495);
            this.tpOpen.TabIndex = 0;
            this.tpOpen.Text = "For SAP";
            this.tpOpen.UseVisualStyleBackColor = true;
            // 
            // panelOpen
            // 
            this.panelOpen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelOpen.Location = new System.Drawing.Point(3, 3);
            this.panelOpen.Name = "panelOpen";
            this.panelOpen.Size = new System.Drawing.Size(726, 489);
            this.panelOpen.TabIndex = 0;
            // 
            // tpClose
            // 
            this.tpClose.Controls.Add(this.panelClose);
            this.tpClose.Location = new System.Drawing.Point(4, 22);
            this.tpClose.Name = "tpClose";
            this.tpClose.Size = new System.Drawing.Size(732, 495);
            this.tpClose.TabIndex = 1;
            this.tpClose.Text = "Done";
            this.tpClose.UseVisualStyleBackColor = true;
            // 
            // panelClose
            // 
            this.panelClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelClose.Location = new System.Drawing.Point(0, 0);
            this.panelClose.Name = "panelClose";
            this.panelClose.Size = new System.Drawing.Size(732, 495);
            this.panelClose.TabIndex = 1;
            // 
            // forSAPIP
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(740, 521);
            this.Controls.Add(this.tc);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "forSAPIP";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "For SAP IP";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.forSAPIP_Load);
            this.tc.ResumeLayout(false);
            this.tpOpen.ResumeLayout(false);
            this.tpClose.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tc;
        private System.Windows.Forms.TabPage tpOpen;
        private System.Windows.Forms.Panel panelOpen;
        private System.Windows.Forms.TabPage tpClose;
        private System.Windows.Forms.Panel panelClose;
    }
}