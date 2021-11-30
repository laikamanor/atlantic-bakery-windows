using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AB.API_Class.SOA;
using AB.API_Class.Customer_Type;
using Newtonsoft.Json.Linq;
namespace AB
{
    public partial class SOA : Form
    {
        public SOA(string docStatus)
        {
            InitializeComponent();
            gDocStatus = docStatus;
        }
        string gDocStatus = "";
        soa_class soac = new soa_class();
        customertype_class customertypec = new customertype_class();
        DataTable dtSOA = new DataTable();
        DataTable dtCustType = new DataTable();
        int cDate = 1, cToDate = 1, cCustType = 1;
        bool gIsSuperAdmin = false;
        public async Task loadSOA()
        {
            string fromDate = checkDate.Checked ? "&from_date=" + dtFromDate.Value.ToString("yyyy-MM-dd") : "&from_date=",
                toDate = checkToDate.Checked ? "&to_date=" + dtToDate.Value.ToString("yyyy-MM-dd") : "&to_date=";

            string custTypeParam = "&cust_type=";
            foreach (DataRow row in dtCustType.Rows)
            {
                if (row["code"].ToString() == cmbCustomerType.Text)
                {
                    custTypeParam += row["id"].ToString();
                    break;
                }
            }

            dtSOA = await Task.Run(() => soac.getSOA(gDocStatus, fromDate+toDate + custTypeParam));
            AutoCompleteStringCollection auto = new AutoCompleteStringCollection();
            dgv.Rows.Clear();
            if (dtSOA.Rows.Count > 0)
            {
                foreach (DataRow row in dtSOA.Rows)
                {
                    auto.Add(row["cust_code"].ToString());
                    if (!string.IsNullOrEmpty(txtSearch.Text.ToString().Trim()) && !txtSearch.Text.Trim().ToLower().Equals("Search Customer".ToLower()))
                    {
                        if (txtSearch.Text.ToString().Trim().ToLower().Contains(row["cust_code"].ToString().ToLower()))
                        {
                            dgv.Rows.Add(row["id"].ToString(), row["transdate"].ToString(), row["reference"].ToString(), row["cust_code"].ToString(), Convert.ToDecimal(string.Format("{0:0.00}", row["balance"].ToString())), Convert.ToDecimal(string.Format("{0:0.00}", row["total_amount"].ToString())), row["docstatus"].ToString(), Convert.ToInt32(row["age"].ToString()));
                        }
                    }
                    else
                    {
                        dgv.Rows.Add(row["id"].ToString(), row["transdate"].ToString(), row["reference"].ToString(), row["cust_code"].ToString(), Convert.ToDecimal(string.Format("{0:0.00}", row["balance"].ToString())), Convert.ToDecimal(string.Format("{0:0.00}", row["total_amount"].ToString())), row["docstatus"].ToString(), Convert.ToInt32(row["age"].ToString()));
                    }
                }
                txtSearch.AutoCompleteCustomSource = auto;
            }
            dgv.Columns["balance"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns["total_amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            lblCount.Text = "Count: " + dgv.Rows.Count.ToString("N0");
            if (gIsSuperAdmin)
            {
                getTotal();
            }
        }

        public void loadCustomerType()
        {
            cmbCustomerType.Items.Clear();
            dtCustType = customertypec.loadCustomerTypes();
            if (dtCustType.Rows.Count > 0)
            {
                cmbCustomerType.Items.Add("All");
                foreach (DataRow row in dtCustType.Rows)
                {
                    cmbCustomerType.Items.Add(row["code"].ToString());
                }
                cmbCustomerType.SelectedIndex = 0;
            }
        }

        private async void SOA_Load(object sender, EventArgs e)
        {
            checkDate.Checked = false;
            label1.Visible = false;
            dtFromDate.Visible = false;
            loadCustomerType();
            await loadSOA();
            cDate = 0;
            cToDate = 0;
            cCustType = 0;
            gIsSuperAdmin = isSuperAdmin();
            lblTotalAmount.Visible = gIsSuperAdmin;
            label2.Visible = gIsSuperAdmin;
            if (gIsSuperAdmin)
            {
                getTotal();
            }
        }

        public bool isSuperAdmin()
        {
            bool isSuperAdmin = false, boolTemp = false;
            if (Login.jsonResult != null)
            {
                Cursor.Current = Cursors.WaitCursor;
                foreach (var x in Login.jsonResult)
                {
                    if (x.Key.Equals("data"))
                    {
                        if (x.Value.ToString().Substring(0, 1).Equals("{"))
                        {
                            JObject joData = JObject.Parse(x.Value.ToString());
                            foreach (var q in joData)
                            {
                                if (q.Key.Equals("isSuperAdmin"))
                                {
                                    isSuperAdmin = bool.TryParse(q.Value.ToString(), out boolTemp) ? Convert.ToBoolean(q.Value.ToString()) : false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return isSuperAdmin;
        }


        public void getTotal()
        {
            double balance = 0.00, doubleTemp = 0.00;
            for(int i = 0; i < dgv.Rows.Count; i++)
            {
                balance += double.TryParse(dgv.Rows[i].Cells["balance"].Value.ToString(), out doubleTemp) ? Convert.ToDouble(dgv.Rows[i].Cells["balance"].Value.ToString()) : doubleTemp;
            }
            lblTotalAmount.Text = balance.ToString("n2");
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await loadSOA();
        }

        private void dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                if(e.RowIndex >= 0)
                {
                    if(dgv.Rows.Count > 0)
                    {
                        SOA_Details frm = new SOA_Details();
                        int intTemp = 0;
                        frm.selectedID = int.TryParse(dgv.CurrentRow.Cells["id"].Value.ToString(), out intTemp) ? Convert.ToInt32(dgv.CurrentRow.Cells["id"].Value.ToString()) : 0;
                        frm.ShowDialog();
                    }
                }
            }
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if(txtSearch.Text.Trim().ToLower().Equals("Search Customer".Trim().ToLower()))
            {
                txtSearch.ForeColor = Color.Black;
                txtSearch.Text = "";
            }
        }


        private void checkToDate_CheckedChanged(object sender, EventArgs e)
        {
            dtToDate.Visible = checkToDate.Checked;
        }

        private async void btnSearchQuery2_Click(object sender, EventArgs e)
        {
            await loadSOA();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await loadSOA();
        }

        private void checkDate_CheckedChanged(object sender, EventArgs e)
        {
            dtFromDate.Visible = checkDate.Checked;
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text.Trim()))
            {
                txtSearch.ForeColor = Color.DimGray;
                txtSearch.Text = "Search Customer";
            }
        }
    }
}
