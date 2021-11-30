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
    public partial class SOATab2 : Form
    {
        public SOATab2()
        {
            InitializeComponent();
        }

        private void SOATab2_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            ForSOA2 frm = new ForSOA2();
            showForm(panelForSOA, frm);
        }
        public void showForm(Panel panel, Form form)
        {
            panel.Controls.Clear();
            form.TopLevel = false;
            panel.Controls.Add(form);
            form.BringToFront();
            form.Show();
        }

        private void tcSOA_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcSOA.SelectedIndex.Equals(0))
            {
                ForSOA2 frm = new ForSOA2();
                showForm(panelForSOA, frm);
            }
            else if (tcSOA.SelectedIndex.Equals(1))
            {
                SOA2 frm = new SOA2("O");
                showForm(panelSOA, frm);
            }
            else if (tcSOA.SelectedIndex.Equals(2))
            {
                SOA2 frm = new SOA2("C");
                showForm(panelClosedSOA, frm);
            }
        }
    }
}
