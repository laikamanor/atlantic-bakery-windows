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
    public partial class ItemRequestTransfer_Tab : Form
    {
        public ItemRequestTransfer_Tab()
        {
            InitializeComponent();
        }
        public static bool isEnter = false;
        private void TargetForDelivery_Tab_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            //ItemRequestTransfer frm = new ItemRequestTransfer("O", "");
            //showForm(panelOpen, frm);
            PendingItemTransferRequest frm = new PendingItemTransferRequest();
            showForm(panelPendingITR, frm);
        }

 

        public void showForm(Panel panel, Form form)
        {
            panel.Controls.Clear();
            form.TopLevel = false;
            panel.Controls.Add(form);
            form.BringToFront();
            form.Show();
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string docStatus = "";
            Panel pn = tcForSAPITR.SelectedIndex <= 0 ? panelForSAP : panelWithSAP ;

            string sForSAP = tcForSAPITR.SelectedIndex <= 0 ? "0" : "1";

            ItemRequestTransfer frm = new ItemRequestTransfer(docStatus, sForSAP);
            showForm(pn, frm);
        }



        private void ItemRequestTransfer_Tab_SizeChanged(object sender, EventArgs e)
        {
       
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Invalidate();
            this.Refresh();
        }

        private void ItemRequestTransfer_Tab_Leave(object sender, EventArgs e)
        {
            ItemRequestTransfer.adornerUIManager1.Hide();
        }

        private void ItemRequestTransfer_Tab_Enter(object sender, EventArgs e)
        {
            ItemRequestTransfer.adornerUIManager1.Show();
        }

        private void tcProd_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tcProd.SelectedIndex == 0)
            {
                PendingItemTransferRequest frm = new PendingItemTransferRequest();
                showForm(panelPendingITR, frm);
            }
            else
            {
                if(tcITR.SelectedIndex != 0)
                {
                    tcITR.SelectedIndex = 0;
                }else
                {
                    ItemRequestTransfer frm = new ItemRequestTransfer("O", "");
                    showForm(panelOpen, frm);
                }
            }
            //string docStatus = tcProd.SelectedIndex <= 0 ? "O" : tcProd.SelectedIndex == 1 ? "C" : tcProd.SelectedIndex == 2 ? "N" : "";

            //if (tcProd.SelectedIndex == 3 && tcForSAPITR.SelectedIndex == 1)
            //{
            //    tcForSAPITR.SelectedIndex = 0;
            //}

            //Panel pn = tcProd.SelectedIndex <= 0 ? panelOpen : tcProd.SelectedIndex == 1 ? panelClosed : tcProd.SelectedIndex == 2 ? panelCanceled : panelForSAP;

            //string sForSAP = tcProd.SelectedIndex == 3 ? "0" : "";

            //ItemRequestTransfer frm = new ItemRequestTransfer(docStatus, sForSAP);
            //showForm(pn, frm);
        }

        private void tcITR_SelectedIndexChanged(object sender, EventArgs e)
        {
            string docStatus = tcITR.SelectedIndex <= 0 ? "O" : tcITR.SelectedIndex == 1 ? "C" : tcITR.SelectedIndex == 2 ? "N" : "";

            if (tcITR.SelectedIndex == 3 && tcForSAPITR.SelectedIndex == 1)
            {
                tcForSAPITR.SelectedIndex = 0;
            }

            Panel pn = tcITR.SelectedIndex <= 0 ? panelOpen : tcITR.SelectedIndex == 1 ? panelClosed : tcITR.SelectedIndex == 2 ? panelCanceled : panelForSAP;

            string sForSAP = tcITR.SelectedIndex == 3 ? "0" : "";

            ItemRequestTransfer frm = new ItemRequestTransfer(docStatus, sForSAP);
            showForm(pn, frm);
        }

        //private void ItemRequestTransfer_Tab_Activated(object sender, EventArgs e)
        //{

        //}
    }
}
