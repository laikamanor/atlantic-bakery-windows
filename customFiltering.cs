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
    public partial class customFiltering : Form
    {
        public customFiltering()
        {
            InitializeComponent();
        }
        public DataTable dt;
        private void customFiltering_Load(object sender, EventArgs e)
        {
            this.Focus();
            try
            {
                label1.Text = this.Text;
                loadData();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void loadData()
        {
            dgv.DataSource = null;
            DataView view = new DataView(dt);
            DataTable distinctValues = view.ToTable(true, "select", "name");
            AutoCompleteStringCollection auto = new AutoCompleteStringCollection();
            foreach (DataRow row in dt.Rows)
            {
                auto.Add(row["name"].ToString());
            }
            txtSearch.AutoCompleteCustomSource = auto;
   
            if (!string.IsNullOrEmpty(txtSearch.Text.Trim()))
            {
                string s = txtSearch.Text.Replace(@"'", "''");
                DataRow[] rows = distinctValues.Select("name='" + s + "'");
                if(rows.Length > 0)
                {
                    dgv.DataSource = rows.CopyToDataTable();
                }
            }
            else
            {
                dgv.DataSource = distinctValues;
            }
            
            //dgv.DataSource = distinctValues;
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                if (dgv.Columns[i].Name.Equals("name"))
                {
                    dgv.Columns[i].ReadOnly = true;
                }
            }
            bool isAll = false;
            int isAllcounter = 0;
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                if (dgv.Rows[i].Cells["select"].Value != null)
                {
                    if (Convert.ToBoolean(dgv.Rows[i].Cells["select"].Value.ToString().Trim()))
                    {
                        isAllcounter += 1;
                    }
                }
                foreach (DataRow row in this.Tag.ToString().Equals("ItemSalesSummaryTab") ? ItemSalesQuantityTab.dtSelectedBranches.Rows : this.Tag.ToString().Equals("ComputedForecast") ? ComputedForecast.dtSelectedBranches.Rows : ItemSalesValueTab.dtSelectedBranches.Rows)
                {
                    if (dgv.Rows[i].Cells["name"].Value.ToString() == row["name"].ToString())
                    {
                        dgv.Rows[i].Cells["select"].Value = true;
                    }
                }
                //if (string.IsNullOrEmpty(dgv.Rows[i].Cells["name"].Value.ToString().Trim()))
                //{
                //    Console.WriteLine("hehe");
                //    //dgv.Rows.RemoveAt(i);
                //}


            }
            Console.WriteLine(isAllcounter + "/" + dgv.Rows.Count);
            checkSelectAll.Checked = isAllcounter == dgv.Rows.Count ? true : false;

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DataTable dtTemp = this.Tag.ToString().Equals("ItemSalesSummaryTab") ? ItemSalesQuantityTab.dtSelectedBranches.Clone() : this.Tag.ToString().Equals("ComputedForecast") ? ComputedForecast.dtSelectedBranches.Clone() : ItemSalesValueTab.dtSelectedBranches.Clone();

            foreach (DataRow drtableOld in this.Tag.ToString().Equals("ItemSalesSummaryTab") ? ItemSalesQuantityTab.dtSelectedBranches.Rows : this.Tag.ToString().Equals("ComputedForecast") ? ComputedForecast.dtSelectedBranches.Rows : ItemSalesValueTab.dtSelectedBranches.Rows)
            {
                dtTemp.ImportRow(drtableOld);
            }
            DataTable dtDuplicate = dtTemp;
            foreach (DataRow row in dtDuplicate.Select("type='" + this.Text + "'")) {
                dtTemp.Rows.Remove(row);
            }
            dtTemp.AcceptChanges();
            if (this.Tag.ToString().Equals("ItemSalesSummaryTab"))
            {
                ItemSalesQuantityTab.dtSelectedBranches = dtTemp;
            }
            else if (this.Tag.ToString().Equals("ComputedForecast"))
            {
                ComputedForecast.dtSelectedBranches = dtTemp;
            }
            else
            {
                ItemSalesValueTab.dtSelectedBranches = dtTemp;
            }
            int count = 0;
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                bool isSelect = false, boolTemp = false;
                isSelect = dgv.Rows[i].Cells["select"].Value == null ? false : bool.TryParse(dgv.Rows[i].Cells["select"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgv.Rows[i].Cells["select"].Value.ToString()) : boolTemp;
                if (isSelect)
                {
                    count += 1;
                    if (this.Tag.ToString().Equals("ItemSalesSummaryTab"))
                    {
                        bool isAll = dgv.Rows.Count == count;
                        ItemSalesQuantityTab.dtSelectedBranches.Rows.Add(isSelect, dgv.Rows[i].Cells[1].Value.ToString(), this.Text,isAll);
                    }
                    else if (this.Tag.ToString().Equals("ComputedForecast"))
                    {
                        bool isAll = dgv.Rows.Count == count;
                        ComputedForecast.dtSelectedBranches.Rows.Add(isSelect, dgv.Rows[i].Cells[1].Value.ToString(), this.Text, isAll);
                    }
                    else
                    {
                        bool isAll = dgv.Rows.Count == count;
                        ItemSalesValueTab.dtSelectedBranches.Rows.Add(isSelect, dgv.Rows[i].Cells[1].Value.ToString(), this.Text,isAll);
                    }
                }
            }
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            checkSelectAll.Checked = false;
            checkUnChecked(false);
        }

        public void checkUnChecked(bool value)
        {
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                dgv.Rows[i].Cells["select"].Value = value;
            }
        }

        private void checkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            checkUnChecked(checkSelectAll.Checked);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            loadData();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Enter))
            {
                loadData();
            }
        }
    }
}
