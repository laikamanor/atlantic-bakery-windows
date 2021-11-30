namespace AB
{
    partial class SalesTransactions_Items2
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
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.lblDocStatus = new System.Windows.Forms.Label();
            this.lblReference = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblTransDate = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblSAPNumber = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblAppliedAmount = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTenderAmount = new System.Windows.Forms.Label();
            this.lblAmountDue = new System.Windows.Forms.Label();
            this.lblGross = new System.Windows.Forms.Label();
            this.lblDiscAmount = new System.Windows.Forms.Label();
            this.lblDocTotal = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.Label11 = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.Label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridControl1
            // 
            this.gridControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridControl1.Location = new System.Drawing.Point(12, 82);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemTextEdit1});
            this.gridControl1.Size = new System.Drawing.Size(765, 251);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.gridView1_RowCellStyle);
            this.gridView1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gridView1_MouseMove);
            // 
            // repositoryItemTextEdit1
            // 
            this.repositoryItemTextEdit1.AutoHeight = false;
            this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
            this.repositoryItemTextEdit1.ReadOnly = true;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // lblDocStatus
            // 
            this.lblDocStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDocStatus.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDocStatus.Location = new System.Drawing.Point(674, 27);
            this.lblDocStatus.Name = "lblDocStatus";
            this.lblDocStatus.Size = new System.Drawing.Size(103, 18);
            this.lblDocStatus.TabIndex = 19;
            this.lblDocStatus.Text = "Open";
            this.lblDocStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblReference
            // 
            this.lblReference.AutoSize = true;
            this.lblReference.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReference.Location = new System.Drawing.Point(156, 27);
            this.lblReference.Name = "lblReference";
            this.lblReference.Size = new System.Drawing.Size(32, 18);
            this.lblReference.TabIndex = 18;
            this.lblReference.Text = "N/A";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.DimGray;
            this.label3.Location = new System.Drawing.Point(542, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(137, 18);
            this.label3.TabIndex = 17;
            this.label3.Text = "Document Status: ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.DimGray;
            this.label1.Location = new System.Drawing.Point(9, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 18);
            this.label1.TabIndex = 16;
            this.label1.Text = "Reference:";
            // 
            // lblTransDate
            // 
            this.lblTransDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTransDate.AutoSize = true;
            this.lblTransDate.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTransDate.Location = new System.Drawing.Point(635, 57);
            this.lblTransDate.Name = "lblTransDate";
            this.lblTransDate.Size = new System.Drawing.Size(142, 18);
            this.lblTransDate.TabIndex = 21;
            this.lblTransDate.Text = "0000-00-00 00:00:00";
            this.lblTransDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.DimGray;
            this.label4.Location = new System.Drawing.Point(542, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 18);
            this.label4.TabIndex = 20;
            this.label4.Text = "Transdate:";
            // 
            // lblSAPNumber
            // 
            this.lblSAPNumber.AutoSize = true;
            this.lblSAPNumber.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSAPNumber.Location = new System.Drawing.Point(156, 57);
            this.lblSAPNumber.Name = "lblSAPNumber";
            this.lblSAPNumber.Size = new System.Drawing.Size(32, 18);
            this.lblSAPNumber.TabIndex = 23;
            this.lblSAPNumber.Text = "N/A";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.DimGray;
            this.label5.Location = new System.Drawing.Point(9, 57);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 18);
            this.label5.TabIndex = 22;
            this.label5.Text = "SAP #:";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.lblAppliedAmount);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.lblTenderAmount);
            this.panel1.Controls.Add(this.lblAmountDue);
            this.panel1.Controls.Add(this.lblGross);
            this.panel1.Controls.Add(this.lblDiscAmount);
            this.panel1.Controls.Add(this.lblDocTotal);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.Label11);
            this.panel1.Controls.Add(this.Label6);
            this.panel1.Controls.Add(this.label14);
            this.panel1.Controls.Add(this.Label9);
            this.panel1.Location = new System.Drawing.Point(12, 339);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(765, 180);
            this.panel1.TabIndex = 38;
            // 
            // lblAppliedAmount
            // 
            this.lblAppliedAmount.AutoSize = true;
            this.lblAppliedAmount.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAppliedAmount.ForeColor = System.Drawing.Color.Black;
            this.lblAppliedAmount.Location = new System.Drawing.Point(177, 89);
            this.lblAppliedAmount.Name = "lblAppliedAmount";
            this.lblAppliedAmount.Size = new System.Drawing.Size(36, 18);
            this.lblAppliedAmount.TabIndex = 75;
            this.lblAppliedAmount.Text = "0.00";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Gray;
            this.label2.Location = new System.Drawing.Point(13, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(121, 18);
            this.label2.TabIndex = 74;
            this.label2.Text = "Applied Amount:";
            // 
            // lblTenderAmount
            // 
            this.lblTenderAmount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTenderAmount.AutoSize = true;
            this.lblTenderAmount.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTenderAmount.ForeColor = System.Drawing.Color.Black;
            this.lblTenderAmount.Location = new System.Drawing.Point(177, 117);
            this.lblTenderAmount.Name = "lblTenderAmount";
            this.lblTenderAmount.Size = new System.Drawing.Size(36, 18);
            this.lblTenderAmount.TabIndex = 73;
            this.lblTenderAmount.Text = "0.00";
            // 
            // lblAmountDue
            // 
            this.lblAmountDue.AutoSize = true;
            this.lblAmountDue.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAmountDue.ForeColor = System.Drawing.Color.Black;
            this.lblAmountDue.Location = new System.Drawing.Point(177, 147);
            this.lblAmountDue.Name = "lblAmountDue";
            this.lblAmountDue.Size = new System.Drawing.Size(36, 18);
            this.lblAmountDue.TabIndex = 71;
            this.lblAmountDue.Text = "0.00";
            // 
            // lblGross
            // 
            this.lblGross.AutoSize = true;
            this.lblGross.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGross.ForeColor = System.Drawing.Color.Black;
            this.lblGross.Location = new System.Drawing.Point(177, 9);
            this.lblGross.Name = "lblGross";
            this.lblGross.Size = new System.Drawing.Size(36, 18);
            this.lblGross.TabIndex = 69;
            this.lblGross.Text = "0.00";
            // 
            // lblDiscAmount
            // 
            this.lblDiscAmount.AutoSize = true;
            this.lblDiscAmount.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDiscAmount.ForeColor = System.Drawing.Color.Black;
            this.lblDiscAmount.Location = new System.Drawing.Point(177, 35);
            this.lblDiscAmount.Name = "lblDiscAmount";
            this.lblDiscAmount.Size = new System.Drawing.Size(36, 18);
            this.lblDiscAmount.TabIndex = 72;
            this.lblDiscAmount.Text = "0.00";
            // 
            // lblDocTotal
            // 
            this.lblDocTotal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDocTotal.AutoSize = true;
            this.lblDocTotal.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDocTotal.ForeColor = System.Drawing.Color.Black;
            this.lblDocTotal.Location = new System.Drawing.Point(177, 61);
            this.lblDocTotal.Name = "lblDocTotal";
            this.lblDocTotal.Size = new System.Drawing.Size(36, 18);
            this.lblDocTotal.TabIndex = 70;
            this.lblDocTotal.Text = "0.00";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Gray;
            this.label8.Location = new System.Drawing.Point(13, 117);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(119, 18);
            this.label8.TabIndex = 68;
            this.label8.Text = "Tender Amount:";
            // 
            // Label11
            // 
            this.Label11.AutoSize = true;
            this.Label11.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label11.ForeColor = System.Drawing.Color.Gray;
            this.Label11.Location = new System.Drawing.Point(13, 147);
            this.Label11.Name = "Label11";
            this.Label11.Size = new System.Drawing.Size(98, 18);
            this.Label11.TabIndex = 66;
            this.Label11.Text = "Amount Due:";
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label6.ForeColor = System.Drawing.Color.Gray;
            this.Label6.Location = new System.Drawing.Point(13, 9);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(96, 18);
            this.Label6.TabIndex = 63;
            this.Label6.Text = "Gross Price:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.Gray;
            this.label14.Location = new System.Drawing.Point(13, 35);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(131, 18);
            this.label14.TabIndex = 67;
            this.label14.Text = "Discount Amount:";
            // 
            // Label9
            // 
            this.Label9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Label9.AutoSize = true;
            this.Label9.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label9.ForeColor = System.Drawing.Color.Gray;
            this.Label9.Location = new System.Drawing.Point(13, 61);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(83, 18);
            this.Label9.TabIndex = 64;
            this.Label9.Text = "Doc. Total:";
            // 
            // SalesTransactions_Items2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(789, 531);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblSAPNumber);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblTransDate);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblDocStatus);
            this.Controls.Add(this.lblReference);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.gridControl1);
            this.Name = "SalesTransactions_Items2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sales Transactions Details";
            this.Load += new System.EventHandler(this.SalesTransactions_Items2_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit1;
        private System.Windows.Forms.Label lblDocStatus;
        private System.Windows.Forms.Label lblReference;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblTransDate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblSAPNumber;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel1;
        internal System.Windows.Forms.Label lblAppliedAmount;
        internal System.Windows.Forms.Label label2;
        internal System.Windows.Forms.Label lblTenderAmount;
        internal System.Windows.Forms.Label lblAmountDue;
        internal System.Windows.Forms.Label lblGross;
        internal System.Windows.Forms.Label lblDiscAmount;
        internal System.Windows.Forms.Label lblDocTotal;
        internal System.Windows.Forms.Label label8;
        internal System.Windows.Forms.Label Label11;
        internal System.Windows.Forms.Label Label6;
        internal System.Windows.Forms.Label label14;
        internal System.Windows.Forms.Label Label9;
    }
}