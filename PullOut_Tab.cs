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
    public partial class PullOut_Tab : Form
    {
        public PullOut_Tab()
        {
            InitializeComponent();
        }

        private void PullOut_Tab_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            PullOut frm = new PullOut("O");
            showForm(frm, panelOpen);
        }

        public void showForm(Form form, Panel panel)
        {
            form.TopLevel = false;
            panel.Controls.Add(form);
            form.BringToFront();
            form.Show();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex <= 0)
            {
                PullOut frm = new PullOut("O");
                showForm(frm, panelOpen);
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                PullOut frm = new PullOut("C");
                showForm(frm, panelClosed);
            }
            else if (tabControl1.SelectedIndex == 2)
            {
                PullOut frm = new PullOut("N");
                showForm(frm, panelCancelled);
                //ReceiveItem frm = new ReceiveItem("O");
            }
        }
    }
}
