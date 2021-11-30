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
    public partial class GoodsReceipt_Tab : Form
    {
        public GoodsReceipt_Tab()
        {
            InitializeComponent();
        }

        private void ReceiptFromProduction_Load(object sender, EventArgs e)
        {
            GoodsReceipt_FinishGoodsReceive frm = new GoodsReceipt_FinishGoodsReceive();
            showForm(panelFG, frm);
        }

        public void showForm(Panel panel, Form form)
        {
            panel.Controls.Clear();
            form.TopLevel = false;
            panel.Controls.Add(form);
            form.BringToFront();
            form.Show();
        }

        private void tcProd_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcProd.SelectedIndex <= 0)
            {
                GoodsReceipt_FinishGoodsReceive frm = new GoodsReceipt_FinishGoodsReceive();
                showForm(panelFG, frm);
            }
            else if (tcProd.SelectedIndex == 1)
            {
                if (tcGR.SelectedIndex == 0)
                {
                    GoodsReceipt frm = new GoodsReceipt("O");
                    showForm(panelForSAP, frm);
                }
                else
                {
                    tcGR.SelectedIndex = 0;
                }
            }
        }

        private void GoodsReceipt_Tab_Enter(object sender, EventArgs e)
        {
            GoodsReceipt.adornerUIManager1.Show();
        }

        private void GoodsReceipt_Tab_Leave(object sender, EventArgs e)
        {
            GoodsReceipt.adornerUIManager1.Hide();
        }

        private void tcGR_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcGR.SelectedIndex <= 0)
            {
                GoodsReceipt frm = new GoodsReceipt("O");
                showForm(panelForSAP, frm);
            }
            else if (tcGR.SelectedIndex == 1)
            {
                GoodsReceipt frm = new GoodsReceipt("C");
                showForm(panelIssueProdOrder, frm);
            }
            else
            {
                GoodsReceipt frm = new GoodsReceipt("N");
                showForm(panelCanceled, frm);
            }
        }
    }
}
