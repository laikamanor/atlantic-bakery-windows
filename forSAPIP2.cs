using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AB.UI_Class;
using RestSharp;
using AB.API_Class.Payment_Type;
using AB.API_Class.Customer_Type;
using AB.API_Class.Branch;
namespace AB
{
    public partial class forSAPIP2 : Form
    {
        utility_class utilityc = new utility_class();
        paymenttype_class paymenttypec = new paymenttype_class();
        customertype_class customertypec = new customertype_class();
        branch_class branchc = new branch_class();
        string gForType = "", gSalesType = "", gStatus = "";
        DataTable dtPaymentTypes, dtBranches = new DataTable();
        DataTable dtCustType = new DataTable();
        public forSAPIP2(string salesType, string forType, string status)
        {
            gForType = forType;
            gSalesType = salesType;
            gStatus = status;
            InitializeComponent();
        }

        private void checkTransDate_CheckedChanged(object sender, EventArgs e)
        {
            dtFromDate.Visible = checkTransDate.Checked;
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        public bool haveSelected()
        {
            int int_result = 0;
            bool result = false;
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                if (Convert.ToBoolean(dgv.Rows[i].Cells["selectt"].Value.ToString()) == true)
                {
                    int_result += 1;
                }
            }
            result = int_result > 0;
            return result;
        }

        private void btnProceed_Click(object sender, EventArgs e)
        {
            if (dgv.Rows.Count <= 0)
            {
                MessageBox.Show("No data found", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!haveSelected())
            {
                MessageBox.Show("No data selected", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                JArray jarrayBody = new JArray();
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    if (Convert.ToBoolean(dgv.Rows[i].Cells["selectt"].Value.ToString()) == true)
                    {
                        jarrayBody.Add(Convert.ToInt32(dgv.Rows[i].Cells["id"].Value));
                    }
                }
                JObject jObjectBody = new JObject();
                SAP_Remarks sAPNumber = new SAP_Remarks();
                sAPNumber.isOptional = false;
                sAPNumber.ShowDialog();
                if (SAP_Remarks.isSubmit)
                {
                    jObjectBody.Add("sap_number", SAP_Remarks.sap_number);
                    jObjectBody.Add("remarks", SAP_Remarks.rem);
                    jObjectBody.Add("ids", jarrayBody);
                    apiPUT(jObjectBody, "/api/sap_num/payment/update");
                }
            }

        }

        public void apiPUT(JObject body, string URL)
        {
            if (Login.jsonResult != null)
            {
                string token = "";
                foreach (var x in Login.jsonResult)
                {
                    if (x.Key.Equals("token"))
                    {
                        token = x.Value.ToString();
                    }
                }
                if (!token.Equals(""))
                {
                    var client = new RestClient(utilityc.URL);
                    client.Timeout = -1;
                    var request = new RestRequest(URL);
                    Console.WriteLine(URL);
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.Method = Method.PUT;


                    request.AddParameter("application/json", body, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    //Console.WriteLine(response.Content);
                    JObject jObjectResponse = JObject.Parse(response.Content);

                    foreach (var x in jObjectResponse)
                    {
                        if (x.Key.Equals("success"))
                        {
                            loadData();
                            break;
                        }
                    }

                    string msg = "No message response found";
                    foreach (var x in jObjectResponse)
                    {
                        if (x.Key.Equals("message"))
                        {
                            msg = x.Value.ToString();
                        }
                    }
                    MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }
        }

        public async void loadBranches()
        {
            int isAdmin = 0;
            string branch = "";
            dtBranches = await Task.Run(() => branchc.returnBranches());
            cmbBranches.Items.Clear();
            cmbBranches.Items.Add("All");
            if (Login.jsonResult != null)
            {
                foreach (var x in Login.jsonResult)
                {
                    if (x.Key.Equals("data"))
                    {
                        JObject jObjectData = JObject.Parse(x.Value.ToString());
                        foreach (var y in jObjectData)
                        {
                            if (y.Key.Equals("branch"))
                            {
                                branch = y.Value.ToString();
                            }
                            else if (y.Key.Equals("isAdmin"))
                            {

                                if (y.Value.ToString().ToLower() == "false" || y.Value.ToString() == "")
                                {
                                    foreach (DataRow row in dtBranches.Rows)
                                    {
                                        if (row["code"].ToString() == branch)
                                        {
                                            cmbBranches.Items.Add(row["name"].ToString());
                                            if (cmbBranches.Items.Count > 1)
                                            {
                                                cmbBranches.SelectedIndex = 0;
                                            }
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    isAdmin += 1;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (cmbBranches.Items.Count <= 1)
                {
                    foreach (DataRow row in dtBranches.Rows)
                    {
                        cmbBranches.Items.Add(row["name"]);
                    }
                }
            }
            if (cmbBranches.Items.Count > 1)
            {
                string branchName = "";
                foreach (DataRow row in dtBranches.Rows)
                {
                    if (row["code"].ToString() == branch)
                    {
                        branchName = row["name"].ToString();
                        break;
                    }
                }
                cmbBranches.SelectedIndex = cmbBranches.Items.IndexOf(branchName);
            }
        }

        private void forSAPIP2_Load(object sender, EventArgs e)
        {
            btnProceed.Visible = gStatus.Equals("Close") ? false : true;
            dgv.Columns["sap_number"].Visible = dgv.Columns["transdate_close"].Visible = gStatus.Equals("Open") ? false : true;
            dgv.Columns["amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cmbFromTime.SelectedIndex = 0;
            cmbToTime.SelectedIndex = cmbToTime.Properties.Items.Count - 1;
            dtPaymentTypes = new DataTable();
            dtFromDate.Value = DateTime.Now;
            dtToDate.Value = DateTime.Now;
            loadPaymentTypes();
            loadCustomerType();
            loadBranches();
            cmbPaymentType.SelectedIndex = 0;
            checkTransDate.Checked = true;
            checkToDate.Checked = true;
            loadData();
        }

        public void loadCustomerType()
        {
            cmbCustType.Items.Clear();
            dtCustType = customertypec.loadCustomerTypes();
            if (dtCustType.Rows.Count > 0)
            {
                cmbCustType.Items.Add("All");
                foreach (DataRow row in dtCustType.Rows)
                {
                    cmbCustType.Items.Add(row["code"].ToString());
                }
                cmbCustType.SelectedIndex = 0;
            }
        }


        private void dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgv.Rows.Count > 0)
            {
                if (e.ColumnIndex.Equals(0))
                {
                    selectData();
                }
            }
        }

        public void selectData()
        {
            if (dgv.Rows.Count > 0)
            {
                dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
                if (Convert.ToBoolean(dgv.CurrentRow.Cells["selectt"].Value.ToString()) == true)
                {
                    double amount = 0.00;
                    for (int i = 0; i < dgv.Rows.Count; i++)
                    {
                        if (Convert.ToBoolean(dgv.Rows[i].Cells["selectt"].Value.ToString()) == true)
                        {
                            amount += Convert.ToDouble(dgv.Rows[i].Cells["amount"].Value.ToString());
                        }
                    }
                    lblTotalAmount.Text = amount.ToString("n2");
                }
            }
        }

        private void checkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv.Rows.Count > 0)
            {
                toggleSelectAll(checkSelectAll.Checked);
                selectData();
                if (!checkSelectAll.Checked)
                {
                    lblTotalAmount.Text = "0.00";
                }
            }
            else if (dgv.Rows.Count <= 0 && checkSelectAll.Checked)
            {
                lblTotalAmount.Text = "0.00";
                MessageBox.Show("No data to select", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void toggleSelectAll(bool value)
        {
            if (dgv.Rows.Count > 0)
            {
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    dgv.Rows[i].Cells["selectt"].Value = value;
                }
            }
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text.ToLower().Equals("search cust. code"))
            {
                txtSearch.Text = string.Empty;
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text.Trim()))
            {
                txtSearch.Text ="Search Cust. Code";
                txtSearch.ForeColor = Color.DimGray;
            }
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            loadData();
        }

        private void btnSearchQuery2_Click(object sender, EventArgs e)
        {
            loadData();
        }

        private void checkToDate_CheckedChanged(object sender, EventArgs e)
        {
            dtToDate.Visible = checkToDate.Checked;
        }


        public void loadPaymentTypes()
        {
            dtPaymentTypes = paymenttypec.loadPaymentType("payment");
            cmbPaymentType.Items.Clear();
            cmbPaymentType.Items.Add("All");
            foreach (DataRow row in dtPaymentTypes.Rows)
            {
                cmbPaymentType.Items.Add(row["description"].ToString());
            }
        }

        public void loadData()
        {
            Cursor.Current = Cursors.WaitCursor;
            lblTotalAmount.Text = "0.00";
            if (Login.jsonResult != null)
            {
                string token = "", branch = "";
                foreach (var x in Login.jsonResult)
                {
                    if (x.Key.Equals("token"))
                    {
                        token = x.Value.ToString();
                    }
                }
                if (!token.Equals(""))
                {
                    var client = new RestClient(utilityc.URL);
                    client.Timeout = -1;
                    string sTransDate = !checkTransDate.Checked ? "?from_date=" : "?from_date=" + dtFromDate.Value.ToString("yyyy-MM-dd");
                    string sFromDate = !checkToDate.Checked ? "&to_date=" : "&to_date=" + dtToDate.Value.ToString("yyyy-MM-dd");
                    string sSalesType = string.IsNullOrEmpty(gSalesType) ? "" : "&sales_type=" + gSalesType;
                    string sTime = "&from_time=" + cmbFromTime.Text + "&to_time=" + cmbToTime.Text;

                    string paymentCode = "";
                    foreach (DataRow row in dtPaymentTypes.Rows)
                    {
                        if (cmbPaymentType.SelectedIndex != 0 || !cmbPaymentType.Text.Equals("All"))
                        {
                            if (cmbPaymentType.Text.Equals(row["description"].ToString()))
                            {
                                paymentCode = row["code"].ToString();
                                break;
                            }
                        }
                    }

                    string sPaymentType = string.IsNullOrEmpty(paymentCode) ? "&payment_type=" : "&payment_type=" + paymentCode;
                    string sSearch = "&search=" + (txtSearch.Text.Trim().ToLower().Equals("search cust. code") ? "" : txtSearch.Text);
                    string sSAPNumber = "&sap_number=";
                    string cCustType = "";
                    if (cmbCustType.Text == "All" || cmbCustType.SelectedIndex == 0)
                    {
                        cCustType = "&cust_type=";
                    }
                    else
                    {
                        foreach (DataRow row in dtCustType.Rows)
                        {
                            if (row["code"].ToString() == cmbCustType.Text)
                            {
                                cCustType = "&cust_type=" + row["id"].ToString();
                                break;
                            }
                        }
                    }

                    foreach (DataRow row in dtBranches.Rows)
                    {
                        if (row["name"].ToString() == cmbBranches.Text)
                        {
                            branch = row["code"].ToString();
                            break;
                        }
                    }

                    var request = new RestRequest("/api/sap_num/payment/update" + sTransDate + sFromDate + sSalesType + sPaymentType + sSearch + sSAPNumber + cCustType + "&branch=" + branch + "&has_sap_num=" + (gStatus.Equals("Open") ? 0 : 1) + sTime);
                    request.AddHeader("Authorization", "Bearer " + token);
                    var response = client.Execute(request);
                    if (response.ErrorMessage == null)
                    {
                        if (response.Content.Substring(0, 1).Equals("{"))
                        {
                            JObject jObjectResponse = JObject.Parse(response.Content);
                            bool isSuccess = false;
                            AutoCompleteStringCollection auto = new AutoCompleteStringCollection();
                            dgv.Rows.Clear();
                            foreach (var x in jObjectResponse)
                            {
                                if (x.Key.Equals("success"))
                                {
                                    isSuccess = Convert.ToBoolean(x.Value.ToString());
                                }
                            }
                            if (isSuccess)
                            {
                                foreach (var z in jObjectResponse)
                                {
                                    if (z.Key.Equals("data"))
                                    {
                                        if (z.Value.ToString() != "[]")
                                        {
                                            JArray jsonArray = JArray.Parse(z.Value.ToString());
                                            for (int i = 0; i < jsonArray.Count(); i++)
                                            {
                                                JObject jObjectData = JObject.Parse(jsonArray[i].ToString());
                                                int id = 0, paymentID = 0;
                                                string paymentType = "", customerCode = "", paymentReference = "", sapNumber = "";
                                                double amount = 0.00;
                                                DateTime dtTransDatee = new DateTime(), dtTransDateClose = new DateTime();
                                                foreach (var y in jObjectData)
                                                {
                                                    if (y.Key.Equals("id"))
                                                    {
                                                        id = Convert.ToInt32(y.Value.ToString());
                                                    }
                                                    else if (y.Key.Equals("payment_id"))
                                                    {
                                                        paymentID = Convert.ToInt32(y.Value.ToString());
                                                    }
                                                    else if (y.Key.Equals("payment_type"))
                                                    {
                                                        paymentType = y.Value.ToString();
                                                    }
                                                    else if (y.Key.Equals("cust_code"))
                                                    {
                                                        customerCode = y.Value.ToString();
                                                    }
                                                    else if (y.Key.Equals("amount"))
                                                    {
                                                        amount = Convert.ToDouble(y.Value.ToString());
                                                    }
                                                    else if (y.Key.Equals("reference"))
                                                    {
                                                        paymentReference = y.Value.ToString();
                                                    }
                                                    else if (y.Key.Equals("transdate"))
                                                    {
                                                        string replaceT = y.Value.ToString().Replace("T", "");
                                                        dtTransDatee = string.IsNullOrEmpty(replaceT) ? new DateTime() : Convert.ToDateTime(replaceT);
                                                    }
                                                    else if (y.Key.Equals("sap_number"))
                                                    {
                                                        sapNumber = y.Value.ToString();
                                                    }
                                                    else if (y.Key.Equals("sap_date_updated"))
                                                    {
                                                        string replaceT = y.Value.ToString().Replace("T", "");
                                                        dtTransDateClose = string.IsNullOrEmpty(replaceT) ? new DateTime() : Convert.ToDateTime(replaceT);
                                                    }
                                                }
                                                auto.Add(customerCode);
                                                dgv.Rows.Add(false, id, paymentID, customerCode, dtTransDatee == DateTime.MinValue ? "" : dtTransDatee.ToString("yyyy-MM-dd HH:mm:ss"), Convert.ToDecimal(string.Format("{0:0.00}", amount)), paymentType, paymentReference, sapNumber, dtTransDateClose == DateTime.MinValue ? "" : dtTransDateClose.ToString("yyyy-MM-dd HH:mm:ss"));
                                            }
                                        }
                                    }
                                }
                                txtSearch.AutoCompleteCustomSource = auto;
                                lblCount.Text = "COUNT (" + dgv.Rows.Count.ToString("N0") + ")";
                            }
                            else
                            {
                                string msg = (string)jObjectResponse["message"];
                                if (!msg.Trim().Equals(""))
                                {
                                    MessageBox.Show(msg, "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(response.Content.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show(response.ErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            lblNoDataFound.Visible = (dgv.Rows.Count > 0 ? false : true);
        }
    }
}
