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
    public partial class ProductionOrder_Tab : Form
    {
        public ProductionOrder_Tab()
        {
            InitializeComponent();
        }

        private void ProductionOrder_Tab_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            ProductionOrder frm = new ProductionOrder("O");
            showForm(frm, panelOpen);
        }

        public void showForm(Form form, Panel pn)
        {
            form.TopLevel = false;
            pn.Controls.Add(form);
            form.BringToFront();
            form.Show();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tabControl1.SelectedIndex == 0)
            {
                ProductionOrder frm = new ProductionOrder("O");
                showForm(frm, panelOpen);
            }else if (tabControl1.SelectedIndex == 1)
            {
                ProductionOrder frm = new ProductionOrder("C");
                showForm(frm, panelClosed);
            }
            else
            {
                ProductionOrder frm = new ProductionOrder("N");
                showForm(frm, panelCancelled);
            }
        }
    }
}
