using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AB.API_Class.Payment_Type;
namespace AB
{
    public partial class forSAPIP : Form
    {
        string gForType = "", gSalesType = "";
        public forSAPIP(string salesType, string forType)
        {
            gForType = forType;
            gSalesType = salesType;
            InitializeComponent();
        }

        private void forSAPIP_Load(object sender, EventArgs e)
        {
            forSAPIP2 forsapip = new forSAPIP2("CASH", "for SAP IP","Open");
            showForm(panelOpen, forsapip);
        }



        private void tcPaymentTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            string status = tc.SelectedIndex <= 0 ? "Open" : "Close";
            forSAPIP2 forsapip = new forSAPIP2("CASH", "for SAP IP",status);
            showForm(tc.SelectedIndex <=0 ? panelOpen : panelClose, forsapip);
        }

        public void showForm(Panel panel, Form form)
        {
            panel.Controls.Clear();
            form.TopLevel = false;
            panel.Controls.Add(form);
            form.BringToFront();
            form.Show();
        }
    }
}
