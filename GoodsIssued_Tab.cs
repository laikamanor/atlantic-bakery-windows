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
    public partial class GoodsIssued_Tab : Form
    {
        public GoodsIssued_Tab()
        {
            InitializeComponent();
        }

        private void IssueForProduction_Load(object sender, EventArgs e)
        {
            //GoodsIssued frm = new GoodsIssued("O");
            //showForm(frm, panelForSAP);
            GoodsIssued_ForIssue frm = new GoodsIssued_ForIssue();
            showForm(frm, panelForIssue);
        }

        public void showForm(Form form, Panel pn)
        {
            form.TopLevel = false;
            pn.Controls.Add(form);
            form.BringToFront();
            form.Show();
        }

        private void tcProd_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tcProd.SelectedIndex <= 0)
            {
                GoodsIssued_ForIssue frm = new GoodsIssued_ForIssue();
                showForm(frm, panelForIssue);
            }
            else if(tcProd.SelectedIndex== 1)
            {
                GoodsIssued_ReceiveGoodsIssue frm = new GoodsIssued_ReceiveGoodsIssue();
                showForm(frm, panelConfirmForIssue);
            }
            else if (tcProd.SelectedIndex ==2)
            {
                if(tcGoodsIssued.SelectedIndex == 0)
                {
                    GoodsIssued frm = new GoodsIssued("O");
                    showForm(frm, panelForSAP);
                }
                else
                {
                    tcGoodsIssued.SelectedIndex = 0;
                }
            }
        }

        private void GoodsIssued_Tab_Enter(object sender, EventArgs e)
        {
            GoodsIssued.adornerUIManager1.Show();
        }

        private void GoodsIssued_Tab_Leave(object sender, EventArgs e)
        {
            GoodsIssued.adornerUIManager1.Hide();
        }

        private void tcGoodsIssued_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcGoodsIssued.SelectedIndex == 0)
            {
                GoodsIssued frm = new GoodsIssued("O");
                showForm(frm, panelForSAP);
            }
            else if (tcGoodsIssued.SelectedIndex == 1)
            {
                GoodsIssued frm = new GoodsIssued("C");
                showForm(frm, panelIssueProdOrder);
            }
            else
            {
                GoodsIssued frm = new GoodsIssued("N");
                showForm(frm, panelCanceled);
            }
        }
    }
}
