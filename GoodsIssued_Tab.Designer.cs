namespace AB
{
    partial class GoodsIssued_Tab
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GoodsIssued_Tab));
            this.tcProd = new System.Windows.Forms.TabControl();
            this.tpForIssue = new System.Windows.Forms.TabPage();
            this.panelForIssue = new System.Windows.Forms.Panel();
            this.tpConfirmForIssue = new System.Windows.Forms.TabPage();
            this.panelConfirmForIssue = new System.Windows.Forms.Panel();
            this.tpGoodsIssued = new System.Windows.Forms.TabPage();
            this.tcGoodsIssued = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panelForSAP = new System.Windows.Forms.Panel();
            this.panelIssueProdOrder = new System.Windows.Forms.Panel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.panelCanceled = new System.Windows.Forms.Panel();
            this.tcProd.SuspendLayout();
            this.tpForIssue.SuspendLayout();
            this.tpConfirmForIssue.SuspendLayout();
            this.tpGoodsIssued.SuspendLayout();
            this.tcGoodsIssued.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcProd
            // 
            this.tcProd.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tcProd.Controls.Add(this.tpForIssue);
            this.tcProd.Controls.Add(this.tpConfirmForIssue);
            this.tcProd.Controls.Add(this.tpGoodsIssued);
            this.tcProd.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tcProd.Location = new System.Drawing.Point(12, 6);
            this.tcProd.Name = "tcProd";
            this.tcProd.SelectedIndex = 0;
            this.tcProd.Size = new System.Drawing.Size(803, 483);
            this.tcProd.TabIndex = 2;
            this.tcProd.SelectedIndexChanged += new System.EventHandler(this.tcProd_SelectedIndexChanged);
            // 
            // tpForIssue
            // 
            this.tpForIssue.Controls.Add(this.panelForIssue);
            this.tpForIssue.Location = new System.Drawing.Point(4, 26);
            this.tpForIssue.Name = "tpForIssue";
            this.tpForIssue.Size = new System.Drawing.Size(795, 453);
            this.tpForIssue.TabIndex = 3;
            this.tpForIssue.Text = "Goods Issue";
            this.tpForIssue.UseVisualStyleBackColor = true;
            // 
            // panelForIssue
            // 
            this.panelForIssue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelForIssue.Location = new System.Drawing.Point(0, 0);
            this.panelForIssue.Name = "panelForIssue";
            this.panelForIssue.Size = new System.Drawing.Size(795, 453);
            this.panelForIssue.TabIndex = 2;
            // 
            // tpConfirmForIssue
            // 
            this.tpConfirmForIssue.Controls.Add(this.panelConfirmForIssue);
            this.tpConfirmForIssue.Location = new System.Drawing.Point(4, 26);
            this.tpConfirmForIssue.Name = "tpConfirmForIssue";
            this.tpConfirmForIssue.Size = new System.Drawing.Size(795, 453);
            this.tpConfirmForIssue.TabIndex = 4;
            this.tpConfirmForIssue.Text = "Receive Goods Issue";
            this.tpConfirmForIssue.UseVisualStyleBackColor = true;
            // 
            // panelConfirmForIssue
            // 
            this.panelConfirmForIssue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelConfirmForIssue.Location = new System.Drawing.Point(0, 0);
            this.panelConfirmForIssue.Name = "panelConfirmForIssue";
            this.panelConfirmForIssue.Size = new System.Drawing.Size(795, 453);
            this.panelConfirmForIssue.TabIndex = 2;
            // 
            // tpGoodsIssued
            // 
            this.tpGoodsIssued.Controls.Add(this.tcGoodsIssued);
            this.tpGoodsIssued.Location = new System.Drawing.Point(4, 26);
            this.tpGoodsIssued.Name = "tpGoodsIssued";
            this.tpGoodsIssued.Size = new System.Drawing.Size(795, 453);
            this.tpGoodsIssued.TabIndex = 5;
            this.tpGoodsIssued.Text = "Goods Issued";
            this.tpGoodsIssued.UseVisualStyleBackColor = true;
            // 
            // tcGoodsIssued
            // 
            this.tcGoodsIssued.Controls.Add(this.tabPage1);
            this.tcGoodsIssued.Controls.Add(this.tabPage2);
            this.tcGoodsIssued.Controls.Add(this.tabPage3);
            this.tcGoodsIssued.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcGoodsIssued.Location = new System.Drawing.Point(0, 0);
            this.tcGoodsIssued.Name = "tcGoodsIssued";
            this.tcGoodsIssued.SelectedIndex = 0;
            this.tcGoodsIssued.Size = new System.Drawing.Size(795, 453);
            this.tcGoodsIssued.TabIndex = 1;
            this.tcGoodsIssued.SelectedIndexChanged += new System.EventHandler(this.tcGoodsIssued_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panelForSAP);
            this.tabPage1.Location = new System.Drawing.Point(4, 26);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(787, 423);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "For SAP";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panelIssueProdOrder);
            this.tabPage2.Location = new System.Drawing.Point(4, 26);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(787, 423);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "With SAP";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panelForSAP
            // 
            this.panelForSAP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelForSAP.Location = new System.Drawing.Point(3, 3);
            this.panelForSAP.Name = "panelForSAP";
            this.panelForSAP.Size = new System.Drawing.Size(781, 417);
            this.panelForSAP.TabIndex = 2;
            // 
            // panelIssueProdOrder
            // 
            this.panelIssueProdOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelIssueProdOrder.AutoScroll = true;
            this.panelIssueProdOrder.Location = new System.Drawing.Point(-1, -9);
            this.panelIssueProdOrder.Name = "panelIssueProdOrder";
            this.panelIssueProdOrder.Size = new System.Drawing.Size(789, 441);
            this.panelIssueProdOrder.TabIndex = 2;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.panelCanceled);
            this.tabPage3.Location = new System.Drawing.Point(4, 26);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(787, 423);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Canceled";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // panelCanceled
            // 
            this.panelCanceled.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelCanceled.AutoScroll = true;
            this.panelCanceled.Location = new System.Drawing.Point(-1, -9);
            this.panelCanceled.Name = "panelCanceled";
            this.panelCanceled.Size = new System.Drawing.Size(789, 441);
            this.panelCanceled.TabIndex = 3;
            // 
            // GoodsIssued_Tab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(827, 501);
            this.Controls.Add(this.tcProd);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "GoodsIssued_Tab";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Goods Issue";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.IssueForProduction_Load);
            this.Enter += new System.EventHandler(this.GoodsIssued_Tab_Enter);
            this.Leave += new System.EventHandler(this.GoodsIssued_Tab_Leave);
            this.tcProd.ResumeLayout(false);
            this.tpForIssue.ResumeLayout(false);
            this.tpConfirmForIssue.ResumeLayout(false);
            this.tpGoodsIssued.ResumeLayout(false);
            this.tcGoodsIssued.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tcProd;
        private System.Windows.Forms.TabPage tpForIssue;
        private System.Windows.Forms.TabPage tpConfirmForIssue;
        private System.Windows.Forms.Panel panelForIssue;
        private System.Windows.Forms.Panel panelConfirmForIssue;
        private System.Windows.Forms.TabPage tpGoodsIssued;
        private System.Windows.Forms.TabControl tcGoodsIssued;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Panel panelForSAP;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panelIssueProdOrder;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Panel panelCanceled;
    }
}