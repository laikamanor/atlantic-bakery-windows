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
    public partial class customFiltering_ForDelivery : Form
    {
        public customFiltering_ForDelivery(string[] appendParams,string title, DataTable dt)
        {
            InitializeComponent();
            this.appendParams = appendParams;
            this.dt = dt;
            this.title = title;
        }
        string title = "";
        string[] appendParams;
        DataTable dt = new DataTable();
        public static string[] selectedValues;
        public static bool isAll = false;
        public static string forDeliveryName = "";
        private void button1_Click(object sender, EventArgs e)
        {
            gridView1.ClearSelection();
        }

        private void customFiltering_ForDelivery_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            label1.Text = title;
            gridControl1.DataSource = null;
            gridControl1.DataSource = dt;
            if(appendParams != null)
            {
                foreach(string param in appendParams)
                {
                    if(gridView1.DataRowCount > 0)
                    {
                        for (int i = 0; i < gridView1.DataRowCount; i++)
                        {
                            string sItemCode = gridView1.GetRowCellValue(i, "name").ToString();
                            if (sItemCode == param)
                            {
                                gridView1.SelectRow(i);
                            }
                        }
                    }
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            int[] rows = gridView1.GetSelectedRows();
            MessageBox.Show(rows.Count().ToString() + "/" + title);
            if (title.Equals("Item") && rows.Count() > 1)
            {
                MessageBox.Show("You can pick only 1 Item!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;   
            }
            selectedValues = new string[rows.Count()];
            int counter = 0;
            foreach(int row in rows)
            {
                selectedValues[counter] = gridView1.GetRowCellValue(row, "name").ToString();
                counter++;
            }
            isAll = gridView1.DataRowCount == rows.Count();
            this.Hide();
        }
    }
}
