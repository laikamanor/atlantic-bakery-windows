namespace AB
{
    partial class GoodsReceipt_Tab
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GoodsReceipt_Tab));
            this.tcProd = new System.Windows.Forms.TabControl();
            this.tpFGR = new System.Windows.Forms.TabPage();
            this.tpGoodsReceipt = new System.Windows.Forms.TabPage();
            this.tcGR = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.panelForSAP = new System.Windows.Forms.Panel();
            this.panelIssueProdOrder = new System.Windows.Forms.Panel();
            this.panelCanceled = new System.Windows.Forms.Panel();
            this.panelFG = new System.Windows.Forms.Panel();
            this.tcProd.SuspendLayout();
            this.tpFGR.SuspendLayout();
            this.tpGoodsReceipt.SuspendLayout();
            this.tcGR.SuspendLayout();
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
            this.tcProd.Controls.Add(this.tpFGR);
            this.tcProd.Controls.Add(this.tpGoodsReceipt);
            this.tcProd.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tcProd.Location = new System.Drawing.Point(12, 9);
            this.tcProd.Name = "tcProd";
            this.tcProd.SelectedIndex = 0;
            this.tcProd.Size = new System.Drawing.Size(803, 483);
            this.tcProd.TabIndex = 3;
            this.tcProd.SelectedIndexChanged += new System.EventHandler(this.tcProd_SelectedIndexChanged);
            // 
            // tpFGR
            // 
            this.tpFGR.Controls.Add(this.panelFG);
            this.tpFGR.Location = new System.Drawing.Point(4, 26);
            this.tpFGR.Name = "tpFGR";
            this.tpFGR.Size = new System.Drawing.Size(795, 453);
            this.tpFGR.TabIndex = 3;
            this.tpFGR.Text = "Finish Goods Receive";
            this.tpFGR.UseVisualStyleBackColor = true;
            // 
            // tpGoodsReceipt
            // 
            this.tpGoodsReceipt.Controls.Add(this.tcGR);
            this.tpGoodsReceipt.Location = new System.Drawing.Point(4, 26);
            this.tpGoodsReceipt.Name = "tpGoodsReceipt";
            this.tpGoodsReceipt.Size = new System.Drawing.Size(795, 453);
            this.tpGoodsReceipt.TabIndex = 4;
            this.tpGoodsReceipt.Text = "Goods Receipt";
            this.tpGoodsReceipt.UseVisualStyleBackColor = true;
            // 
            // tcGR
            // 
            this.tcGR.Controls.Add(this.tabPage1);
            this.tcGR.Controls.Add(this.tabPage2);
            this.tcGR.Controls.Add(this.tabPage3);
            this.tcGR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcGR.Location = new System.Drawing.Point(0, 0);
            this.tcGR.Name = "tcGR";
            this.tcGR.SelectedIndex = 0;
            this.tcGR.Size = new System.Drawing.Size(795, 453);
            this.tcGR.TabIndex = 0;
            this.tcGR.SelectedIndexChanged += new System.EventHandler(this.tcGR_SelectedIndexChanged);
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
            // panelCanceled
            // 
            this.panelCanceled.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCanceled.Location = new System.Drawing.Point(0, 0);
            this.panelCanceled.Name = "panelCanceled";
            this.panelCanceled.Size = new System.Drawing.Size(787, 423);
            this.panelCanceled.TabIndex = 3;
            // 
            // panelFG
            // 
            this.panelFG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelFG.Location = new System.Drawing.Point(0, 0);
            this.panelFG.Name = "panelFG";
            this.panelFG.Size = new System.Drawing.Size(795, 453);
            this.panelFG.TabIndex = 4;
            // 
            // GoodsReceipt_Tab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(827, 501);
            this.Controls.Add(this.tcProd);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "GoodsReceipt_Tab";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Goods Receipt";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.ReceiptFromProduction_Load);
            this.Enter += new System.EventHandler(this.GoodsReceipt_Tab_Enter);
            this.Leave += new System.EventHandler(this.GoodsReceipt_Tab_Leave);
            this.tcProd.ResumeLayout(false);
            this.tpFGR.ResumeLayout(false);
            this.tpGoodsReceipt.ResumeLayout(false);
            this.tcGR.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tcProd;
        private System.Windows.Forms.TabPage tpFGR;
        private System.Windows.Forms.TabPage tpGoodsReceipt;
        private System.Windows.Forms.TabControl tcGR;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Panel panelForSAP;
        private System.Windows.Forms.Panel panelIssueProdOrder;
        private System.Windows.Forms.Panel panelCanceled;
        private System.Windows.Forms.Panel panelFG;
    }
}