namespace AB
{
    partial class ReceiveItem_Tab
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpClosed = new System.Windows.Forms.TabPage();
            this.panelClosed = new System.Windows.Forms.Panel();
            this.tpCancelled = new System.Windows.Forms.TabPage();
            this.panelCancelled = new System.Windows.Forms.Panel();
            this.tpOpen = new System.Windows.Forms.TabPage();
            this.panelOpen = new System.Windows.Forms.Panel();
            this.tabControl1.SuspendLayout();
            this.tpClosed.SuspendLayout();
            this.tpCancelled.SuspendLayout();
            this.tpOpen.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpOpen);
            this.tabControl1.Controls.Add(this.tpClosed);
            this.tabControl1.Controls.Add(this.tpCancelled);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(790, 480);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tpClosed
            // 
            this.tpClosed.Controls.Add(this.panelClosed);
            this.tpClosed.Location = new System.Drawing.Point(4, 26);
            this.tpClosed.Name = "tpClosed";
            this.tpClosed.Padding = new System.Windows.Forms.Padding(3);
            this.tpClosed.Size = new System.Drawing.Size(782, 450);
            this.tpClosed.TabIndex = 0;
            this.tpClosed.Text = "Closed";
            this.tpClosed.UseVisualStyleBackColor = true;
            // 
            // panelClosed
            // 
            this.panelClosed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelClosed.Location = new System.Drawing.Point(3, 3);
            this.panelClosed.Name = "panelClosed";
            this.panelClosed.Size = new System.Drawing.Size(776, 444);
            this.panelClosed.TabIndex = 0;
            // 
            // tpCancelled
            // 
            this.tpCancelled.Controls.Add(this.panelCancelled);
            this.tpCancelled.Location = new System.Drawing.Point(4, 26);
            this.tpCancelled.Name = "tpCancelled";
            this.tpCancelled.Padding = new System.Windows.Forms.Padding(3);
            this.tpCancelled.Size = new System.Drawing.Size(782, 450);
            this.tpCancelled.TabIndex = 1;
            this.tpCancelled.Text = "Cancelled";
            this.tpCancelled.UseVisualStyleBackColor = true;
            // 
            // panelCancelled
            // 
            this.panelCancelled.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCancelled.Location = new System.Drawing.Point(3, 3);
            this.panelCancelled.Name = "panelCancelled";
            this.panelCancelled.Size = new System.Drawing.Size(776, 444);
            this.panelCancelled.TabIndex = 1;
            // 
            // tpOpen
            // 
            this.tpOpen.Controls.Add(this.panelOpen);
            this.tpOpen.Location = new System.Drawing.Point(4, 26);
            this.tpOpen.Name = "tpOpen";
            this.tpOpen.Size = new System.Drawing.Size(782, 450);
            this.tpOpen.TabIndex = 2;
            this.tpOpen.Text = "Open";
            this.tpOpen.UseVisualStyleBackColor = true;
            // 
            // panelOpen
            // 
            this.panelOpen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelOpen.Location = new System.Drawing.Point(0, 0);
            this.panelOpen.Name = "panelOpen";
            this.panelOpen.Size = new System.Drawing.Size(782, 450);
            this.panelOpen.TabIndex = 1;
            // 
            // ReceiveItem_Tab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(790, 480);
            this.Controls.Add(this.tabControl1);
            this.Name = "ReceiveItem_Tab";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Receive Transactions";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.ReceiveItem_Tab_Load);
            this.tabControl1.ResumeLayout(false);
            this.tpClosed.ResumeLayout(false);
            this.tpCancelled.ResumeLayout(false);
            this.tpOpen.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpClosed;
        private System.Windows.Forms.TabPage tpCancelled;
        private System.Windows.Forms.Panel panelClosed;
        private System.Windows.Forms.Panel panelCancelled;
        private System.Windows.Forms.TabPage tpOpen;
        private System.Windows.Forms.Panel panelOpen;
    }
}