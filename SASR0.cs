using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AB
{
    public partial class SASR0 : Form
    {
        public SASR0()
        {
            InitializeComponent();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex.Equals(0))
            {
                salesAmountSummaryReport frm = new salesAmountSummaryReport();
                showForm(panelBranch, frm);
            }
            else if (tabControl1.SelectedIndex.Equals(1))
            {
                salesAmountSummaryReport_customer frm = new salesAmountSummaryReport_customer();
                showForm(panelCustomer, frm);
            }
            else if (tabControl1.SelectedIndex.Equals(2))
            {
                salesAmountSummaryPrintedReport frm = new salesAmountSummaryPrintedReport();
                showForm(panelPrintedReport, frm);
            }
        }


        public void showForm(Panel panel, Form form)
        {
            panel.Controls.Clear();
            form.TopLevel = false;
            panel.Controls.Add(form);
            form.BringToFront();
            form.Show();
        }

        private void SASR0_Load(object sender, EventArgs e)
        {
            salesAmountSummaryReport pendingOrder = new salesAmountSummaryReport();
            showForm(panelBranch, pendingOrder);
        }
    }
}
