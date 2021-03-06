using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AB.API_Class.Transfer;
using Newtonsoft.Json.Linq;
using AB.API_Class.User;
namespace AB
{
    public partial class Transfer : Form
    {
        public Transfer()
        {
            InitializeComponent();
        }

        private void Transfer_Load(object sender, EventArgs e)
        {
            Transfer2 transfer2 = new Transfer2("Open");
            transfer2.Text = this.Text;
            showForm(panelTransactions, transfer2);
            if(!this.Text.Equals("Transfer Transactions"))
            {
                tabControl1.TabPages.Remove(tpSAPIT);
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

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex.Equals(0))
            {
                Transfer2 transfer2 = new Transfer2("Open");
                transfer2.Text = this.Text;
                showForm(panelTransactions, transfer2);
            }
            else if (tabControl1.SelectedIndex.Equals(1))
            {
                Transfer2 transfer2 = new Transfer2("Closed");
                transfer2.Text = this.Text;
                showForm(panelSAP, transfer2);
            }
            else if (tabControl1.SelectedIndex.Equals(2))
            {
                Transfer2 transfer2 = new Transfer2("Cancelled");
                transfer2.Text = this.Text;
                showForm(panelCancelled, transfer2);
            }
            else if (tabControl1.SelectedIndex.Equals(3))
            {
                TransferTransaction_SAPTab frm = new TransferTransaction_SAPTab();
                frm.Text = this.Text;
                showForm(panelSAPIT, frm);
            }
        }
    }
}
