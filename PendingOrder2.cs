using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AB.UI_Class;
using AB.API_Class.Branch;
using AB.API_Class.Warehouse;
using AB.API_Class.Customer_Type;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;

namespace AB
{
    public partial class PendingOrder2 : Form
    {
        UI_Class.utility_class utilityc = new utility_class();
        api_class apic = new api_class();
        customertype_class customertypec = new customertype_class();
        DataTable dtSalesAgent = new DataTable();
        public static DataTable dtSelectedDeposit;
        DataTable dtPayment = new DataTable();
        string gSalesType = "", gForType = "", gStatus = "";
        branch_class branchc = new branch_class();
        warehouse_class warehousec = new warehouse_class();
        DataTable dtBranches = new DataTable();
        DataTable dtWarehouse = new DataTable();
        DataTable dtSearch = new DataTable();
        DataTable dtCustType = new DataTable();
        int cCheck = 0;
        public PendingOrder2(string salesType, string forType,string status)
        {
            gSalesType = salesType;
            gForType = forType;
            gStatus = status;
            InitializeComponent();
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

        private async void PendingOrder_Load(object sender, EventArgs e)
        {
            dgvitems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvitems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dtSearch = new DataTable();
            dtSearch.Rows.Clear();
            dtSearch.Columns.Add("search", typeof(string));
            dtSearch.Columns.Add("type", typeof(string));
            btnConfirm.Visible = gForType.Equals("for SAP") && gStatus.Equals("Close") ? false : true;

            btnrefresh.Location = gForType.Equals("for SAP") && gStatus.Equals("Close") ? btnConfirm.Location : btnrefresh.Location;
            dtFromDate.Visible = true;
            label1.Visible = true;
            dtFromDate.Value = DateTime.Now;
            dtToDate.Value = DateTime.Now;
            checkDate.Checked = true;
            checkToDate.Checked = true;
        
            cmbSearchType.SelectedIndex = 0;
            cmbFromTime.SelectedIndex = 0;
            cmbToTime.SelectedIndex = cmbToTime.Items.Count - 1;
            if (gForType == "for Payment")
            {
                btnConfirm.Text = "PAY";
                button1.Visible = true;
                btnPaymentMethod.Visible = true;
            }
            else
            {
                btnConfirm.Text = "CONFIRM";
                button1.Visible = false;
                //btnVoid.Visible = false;
                btnPaymentMethod.Visible = false;
            }
            //if (gForType.Equals("for Payment") || gForType.Equals("for Confirmation"))
            //{
            //    btnVoid.Visible = true;
            //}
            //else
            //{
            //    btnVoid.Visible = false;
            //}
            dtSelectedDeposit = new DataTable();
            dtSelectedDeposit.Columns.Clear();
            dtSelectedDeposit.Columns.Add("id");
            dtSelectedDeposit.Columns.Add("amount");
            dtSelectedDeposit.Columns.Add("payment_type");
            dtSelectedDeposit.Columns.Add("sapnum");
            dtSelectedDeposit.Columns.Add("reference2");
            dtSelectedDeposit.Columns.Add("type");
            dtPayment.Columns.Add("id");
            dtPayment.Columns.Add("amount");
            dtPayment.Columns.Add("payment_type");
            dtPayment.Columns.Add("sapnum");
            dtPayment.Columns.Add("reference2");

            dtSelectedDeposit.Rows.Clear();
            label7.Visible = true;
            cmbBranches.Visible = true;
            loadBranch();

            loadCustomerType();
            string subURL = gForType == "for Payment" ? "/api/payment/new" : "/api/sales/for_confirm";
            loadData(subURL);
            searchFilter();
        }

        public void loadSalesAgent()
        {
            Cursor.Current = Cursors.WaitCursor;
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
                    dtSalesAgent.Columns.Clear();
                    dtSalesAgent.Columns.Add("id");
                    dtSalesAgent.Columns.Add("username");
                    dtSalesAgent.Rows.Clear();
                    cmbsales.Items.Clear();
                    var client = new RestClient(utilityc.URL);
                    client.Timeout = -1;
                    var request = new RestRequest("/api/auth/user/get_all?isSales=1&branch=" + findCode(cmbBranches.Text, "Branch"));
                    Console.WriteLine("/api/auth/user/get_all?isSales=1&branch=" + findCode(cmbBranches.Text, "Branch"));
                    request.AddHeader("Authorization", "Bearer " + token);
                    var response = client.Execute(request);
                    if (response.ErrorMessage == null)
                    {
                        JObject jObjectResponse = JObject.Parse(response.Content);
                        cmbsales.Items.Add("All");

                        bool isSuccess = false;
                        foreach (var x in jObjectResponse)
                        {
                            if (x.Key.Equals("success"))
                            {
                                isSuccess = Convert.ToBoolean(x.Value.ToString());
                            }
                        }
                        if (isSuccess)
                        {
                            foreach (var x in jObjectResponse)
                            {
                                if (x.Key.Equals("data"))
                                {
                                    if (x.Value.ToString() != "[]")
                                    {
                                        lblNoDataFound.Visible = false;
                                        JArray jsonArray = JArray.Parse(x.Value.ToString());
                                        for (int i = 0; i < jsonArray.Count(); i++)
                                        {
                                            JObject jObjectData = JObject.Parse(jsonArray[i].ToString());
                                            int id = 0;
                                            string username = "";
                                            foreach (var y in jObjectData)
                                            {
                                                if (y.Key.Equals("id"))
                                                {
                                                    id = Convert.ToInt32(y.Value.ToString());
                                                }
                                                else if (y.Key.Equals("username"))
                                                {
                                                    username = y.Value.ToString();
                                                }
                                            }
                                            dtSalesAgent.Rows.Add(id, username);
                                            cmbsales.Items.Add(username);

                                        }
                                    }
                                }
                            }
                            cmbsales.SelectedIndex = 0;
                        }
                        else
                        {
                            string msg = "No message response found";
                            foreach (var x in jObjectResponse)
                            {
                                if (x.Key.Equals("message"))
                                {
                                    msg = x.Value.ToString();
                                }
                            }
                            if (msg.Equals("Token is invalid"))
                            {
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show("Your login session is expired. Please login again", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    else
                    {
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show(response.ErrorMessage, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            Cursor.Current = Cursors.Default;
        }

        public async void loadBranch()
        {
            string currentBranch = "";
            bool isAdmin = false;
            cmbBranches.Items.Clear();

            //get muna whse and check kung admin , superadmin or manager
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
                                currentBranch = y.Value.ToString();
                            }
                            else if (y.Key.Equals("isAdmin") || y.Key.Equals("isSuperAdmin") || y.Key.Equals("isManager") || y.Key.Equals("isCashier") || y.Key.Equals("isAccounting") || y.Key.Equals("isSalesAgent"))
                            {
                                isAdmin = string.IsNullOrEmpty(y.Value.ToString()) ? false : Convert.ToBoolean(y.Value.ToString());
                                if (isAdmin)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            dtBranches = await branchc.returnBranches();
            if (isAdmin)
            {
                dtBranches = await branchc.returnBranches();
                cmbBranches.Items.Add("All");
                foreach (DataRow row in dtBranches.Rows)
                {
                    cmbBranches.Items.Add(row["name"]);
                }
            }
            else
            {
                foreach (DataRow row in dtBranches.Rows)
                {
                    if (row["code"].ToString() == currentBranch)
                    {
                        cmbBranches.Items.Add(row["name"]);
                        break;
                    }
                }
            }
            //default text 
            //kapag admin or to whse all yung lalabas
            //kapag hindi kung ano yung current whse nya yun yung lalabas
            string branchName = "";
            foreach (DataRow row in dtBranches.Rows)
            {
                if (row["code"].ToString().Trim().ToLower() == currentBranch.Trim().ToLower())
                {
                    branchName = row["name"].ToString();
                    break;
                }
            }
            cmbBranches.SelectedIndex = cmbBranches.Items.IndexOf(branchName);
        }

        public string findCode(string value, string typee)
        {
            string result = "";
            if (typee.Equals("Warehouse"))
            {
                foreach (DataRow row in dtWarehouse.Rows)
                {
                    if (row["whsename"].ToString() == value)
                    {
                        result = row["whsecode"].ToString();
                        break;
                    }
                }
            }
            else
            {
                foreach (DataRow row in dtBranches.Rows)
                {
                    if (row["name"].ToString() == value)
                    {
                        result = row["code"].ToString();
                        break;
                    }
                }
            }
            return result;
        }

        public void loadData(string subURL)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (Login.jsonResult != null)
            {
                string token = "", branch = "";
                foreach (var x in Login.jsonResult)
                {
                    if (x.Key.Equals("token"))
                    {
                        token = x.Value.ToString();
                    }
                    else if (x.Key.Equals("data"))
                    {
                        JObject jObjectData = JObject.Parse(x.Value.ToString());
                        foreach (var y in jObjectData)
                        {
                            if (y.Key.Equals("branch"))
                            {
                                branch = y.Value.ToString();
                            }
                        }
                    }
                }
                if (!token.Equals(""))
                {
                    dgvitems.Rows.Clear();
                    var client = new RestClient(utilityc.URL);
                    client.Timeout = -1;

                    int selectedSalesAgentID = 0;
                    foreach (DataRow r0w in dtSalesAgent.Rows)
                    {
                        if (r0w["username"].Equals(cmbsales.Text))
                        {
                            selectedSalesAgentID = Convert.ToInt32(r0w["id"]);
                        }
                    }
                    string sUserID = "";
                    if (selectedSalesAgentID != 0)
                    {
                        sUserID = selectedSalesAgentID.ToString();
                    }
                    string arSalesDate = "";
                    string sSearch = "";
                    if (gSalesType == "AR Sales" && label1.Visible == true && dtFromDate.Visible == true)
                    {
                        arSalesDate = !checkDate.Checked ? "&from_date=" : "&from_date=" + dtFromDate.Value.ToString("yyyy-MM-dd");
                        arSalesDate += !checkToDate.Checked ? "&to_date=" : "&to_date=" + dtToDate.Value.ToString("yyyy-MM-dd");
                    }
                    else if (gForType.ToLower().Equals("for payment"))
                    {
                        arSalesDate = !checkDate.Checked ? "&from_date=" : "&from_date=" + dtFromDate.Value.ToString("yyyy-MM-dd");
                        arSalesDate += !checkToDate.Checked ? "&to_date=" : "&to_date=" + dtToDate.Value.ToString("yyyy-MM-dd");
                    }
                    sSearch = "&search=" + (txtsearch.Text.Trim().ToLower().Equals("search trans. #") || txtsearch.Text.Trim().ToLower().Equals("search cust. code") ? "" : txtsearch.Text);
                    string sBranch = "&branch=" + findCode(cmbBranches.Text, "Branch");

                    string sFromTime = "&from_time=" + cmbFromTime.Text;

                    string sToTime = "&to_time=" + cmbToTime.Text;

                    string forQuery = subURL + "?transtype=" + gSalesType + "&user_id=" + sUserID + sSearch + arSalesDate + sBranch + sFromTime + sToTime;

                    string sCustType = "";
                    if (cmbCustomerType.Text == "All" || cmbCustomerType.SelectedIndex == 0)
                    {
                        sCustType = "&cust_type=";
                    }
                    else
                    {
                        foreach (DataRow row in dtCustType.Rows)
                        {
                            if (row["code"].ToString() == cmbCustomerType.Text)
                            {
                                sCustType = "&cust_type=" + row["id"].ToString();
                                break;
                            }
                        }
                    }

                    string resultQuery;
                    if (gForType.Equals("for SAP"))
                    {
                        string sSearch2 = txtsearch.Text.Trim().ToLower().Equals("search trans. #") || txtsearch.Text.Trim().ToLower().Equals("search cust. code") ? "" : txtsearch.Text;
                        resultQuery = "/api/sales/for_sap/get_all?from_date=" + (!checkDate.Checked ? "" : dtFromDate.Value.ToString("yyyy-MM-dd")) + "&to_date=" + (!checkToDate.Checked ? "" : dtToDate.Value.ToString("yyyy-MM-dd")) + "&transtype=" + gSalesType + "&search=" + sSearch2 + "&created_by=" + sUserID + sFromTime + sToTime + sBranch + "&has_sap_num=" + (gStatus.Equals("Open") ? 0 : 1);
                    }
                    else
                    {
                        resultQuery = forQuery;
                    }
                    resultQuery += sCustType;
                    string result = apic.loadData(resultQuery, "", "", "", Method.GET, true);
                    loadUI(result);
                }
            }
            //dgvOrders.Columns["amountdue"].HeaderText = (gForType.Equals("for SAP") ? "Doc. Total" : "Balance Due");
            highlightHaveDiscount();
            searchFilter();
            lblOrderCount.Text = "Orders (" + dgvOrders.Rows.Count.ToString("N0") + ")";
            Cursor.Current = Cursors.Default;
        }

        public void loadUI(string value1)
        {
            dgvOrders.AutoGenerateColumns = true;
            dgvOrders.DataSource = null;
            dgvOrders.Columns.Clear();
            try
            {
                string forSAPAmountColumnName = (gForType.Equals("for SAP") ? "doctotal" : "amount_due");
                if (!string.IsNullOrEmpty(value1.ToString().Trim()) && value1.Substring(0, 1).Equals("{"))
                {
                    JObject jo = JObject.Parse(value1);
                    DataTable dt = JsonConvert.DeserializeObject<DataTable>(jo["data"].ToString());
                    if (dt.Rows.Count > 0)
                    {
                        DataGridViewCheckBoxColumn newCol = new DataGridViewCheckBoxColumn();
                        newCol.HeaderText = "Select";
                        newCol.Name = "selectt";
                        newCol.Visible = true;
                        newCol.Width = 50;
                        newCol.MinimumWidth = 50;

                        DataGridViewButtonColumn newColBtn = new DataGridViewButtonColumn();
                        newColBtn.HeaderText = "Action";
                        newColBtn.Name = "btnEditAmountPayable";
                        newColBtn.Visible = true;
                        newColBtn.Width = 150;
                        newColBtn.UseColumnTextForButtonValue = true;
                        newColBtn.Text = "Edit Amount Payable";
                        newColBtn.MinimumWidth = 150;
                        newColBtn.FlatStyle = FlatStyle.Flat;
                        newColBtn.DefaultCellStyle.BackColor = Color.FromArgb(199, 65, 240);
                        newColBtn.DefaultCellStyle.ForeColor = Color.White;

                        dgvOrders.Columns.Add(newCol);
                        DataColumn dc = new DataColumn("balance_due", typeof(double));
                        dt.Columns.Add(dc);
                        foreach (DataRow dr in dt.Rows)
                        {
                            dr["balance_due"] = dr[forSAPAmountColumnName];

                            dtSearch.Rows.Add(dr["cust_code"], "Customer");
                            dtSearch.Rows.Add(dr["reference"], "Transnum");
                        }

                        dgvOrders.DataSource = dt;

                        for (int i = 0; i < dgvOrders.Columns.Count; i++)
                        {
                            dgvOrders.AutoGenerateColumns = false;
                            string q = dgvOrders.Columns[i].HeaderText; ;
                            dgvOrders.Columns[i].ReadOnly = q.Equals("selectt") ? true : false;
                            //visible
                            dgvOrders.Columns[i].Visible = q.Equals("reference") || q.Equals("user") || q.Equals("disctype") || q.Equals("cust_code") || q.Equals("remarks") || q.Equals("transdate") || q.Equals("disc_amount") || q.Equals(forSAPAmountColumnName) || q.Equals("tenderamt") || q.Equals("days_due") || q.Equals("delfee") || q.Equals("Select") || q.Equals("balance_due") ||q.Equals("id")  ? true : false;

                            //fomat
                            dgvOrders.Columns[i].DefaultCellStyle.Format = q.Equals("disc_amount") || q.Equals(forSAPAmountColumnName) || q.Equals("tenderamt") || q.Equals("days_due") || q.Equals("delfee") || q.Equals("balance_due") ? "n2" : q.Equals("transdate") ? "yyyy-MM-dd HH:mm:ss" : "";

                            //align
                            dgvOrders.Columns[i].DefaultCellStyle.Alignment = q.Equals("disc_amount") || q.Equals(forSAPAmountColumnName) || q.Equals("tenderamt") || q.Equals("days_due") || q.Equals("delfee") || q.Equals("balance_due") ? DataGridViewContentAlignment.MiddleRight : q.Equals("Select") ? DataGridViewContentAlignment.MiddleCenter : DataGridViewContentAlignment.MiddleLeft;

                            dgvOrders.Columns[i].MinimumWidth = q.Equals("reference") || q.Equals("cust_code") || q.Equals("remarks") || q.Equals("transdate") || q.Equals("sap_number") ? 150 : 100;
                            string w = q.Replace("_", " ");
                            string e = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(w.ToLower());
                            dgvOrders.Columns[i].HeaderText = e.Equals("Tenderamt") ? "Tender Amount" : e.Equals("Amount Due") ? "Amount Payable" : e.Equals("Delfee") ? "Delivery Fee" : e.Equals("Days Due") ? "Aging" : e;
                        }
                        dgvOrders.Columns.Add(newColBtn);
                        if (dgvOrders.Columns.Contains("reference")) dgvOrders.Columns["reference"].DisplayIndex = 1;
                        if(dgvOrders.Columns.Contains("balance_due")) dgvOrders.Columns["balance_due"].DisplayIndex = 2;
                        if (dgvOrders.Columns.Contains("cust_code")) dgvOrders.Columns["cust_code"].DisplayIndex = 3;
                        if (dgvOrders.Columns.Contains("tenderamt")) dgvOrders.Columns["tenderamt"].DisplayIndex = 4;
                        if (dgvOrders.Columns.Contains("disctype")) dgvOrders.Columns["disctype"].DisplayIndex = 5;
                        if (dgvOrders.Columns.Contains("disc_amount")) dgvOrders.Columns["disc_amount"].DisplayIndex = 6;
                        if (dgvOrders.Columns.Contains(forSAPAmountColumnName)) dgvOrders.Columns[forSAPAmountColumnName].DisplayIndex = 7;
                        if (dgvOrders.Columns.Contains("delfee")) dgvOrders.Columns["delfee"].DisplayIndex = 8;
                        if (dgvOrders.Columns.Contains("remarks")) dgvOrders.Columns["remarks"].DisplayIndex = 9;
                        if (dgvOrders.Columns.Contains("user")) dgvOrders.Columns["user"].DisplayIndex = 10;
                        if(dgvOrders.Columns.Contains("transdate")) dgvOrders.Columns["transdate"].DisplayIndex = 11;
                        if (dgvOrders.Columns.Contains("days_due")) dgvOrders.Columns["days_due"].DisplayIndex = 12;
                        if (dgvOrders.Columns.Contains("sap_number")) dgvOrders.Columns["sap_number"].DisplayIndex = 13;
                        dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void highlightHaveDiscount()
        {
            if (dgvOrders.Rows.Count > 0)
            {
                for (int i = 0; i < dgvOrders.Rows.Count; i++)
                {
                    if (dgvOrders.Columns.Contains("disc_amount"))
                    {
                        if (Convert.ToDouble(dgvOrders.Rows[i].Cells["disc_amount"].Value.ToString()) > 0)
                        {
                            dgvOrders.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                            dgvOrders.Rows[i].Cells["btnEditAmountPayable"].Style.BackColor = Color.FromArgb(199, 65, 240);
                            dgvOrders.Rows[i].Cells["selectt"].Style.BackColor = Color.White;
                        }
                    }
                }
            }
        }

        public void searchFilter()
        {
            AutoCompleteStringCollection auto = new AutoCompleteStringCollection();
            if (cmbSearchType.SelectedIndex == 0)
            {
                foreach (DataRow row in dtSearch.Rows)
                {
                    if (row["type"].ToString().Equals("Transnum"))
                    {
                        auto.Add(row["search"].ToString());
                    }
                }
            }
            else
            {
                foreach (DataRow row in dtSearch.Rows)
                {
                    if (row["type"].ToString().Equals("Customer"))
                    {
                        auto.Add(row["search"].ToString());
                    }
                }
            }
            txtsearch.Text = cmbSearchType.SelectedIndex <= 0 ? "Search Trans. #" : "Search Cust. Code";
            txtsearch.ForeColor = string.IsNullOrEmpty(txtsearch.Text.Trim()) ? Color.DimGray : Color.Black;
            txtsearch.AutoCompleteCustomSource = auto;
        }

        public void clearBillsField()
        {
            txtGrossPrice.Text = "0.00";
            txtDiscountAmount.Text = "0.00";
            txtlAmountPayable.Text = "0.00";
            txtTotalPayment.Text = "0.00";
            txtChange.Text = "0.00";
            checkSelectAll.Checked = false;
        }

        public void selectOrders(bool value)
        {
            Cursor.Current = Cursors.WaitCursor;
            //DataTable dt = new DataTable();
            if (Login.jsonResult != null)
            {
                dgvitems.Rows.Clear();
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

                    dgvOrders.CommitEdit(DataGridViewDataErrorContexts.Commit);

                    double totalPendingAmount = 0.00;
                    //string ids /*= "";*/
                    //int isCheckAll_int = 0;
                    if (value)
                    {
                        for (int i = 0; i < dgvOrders.Rows.Count; i++)
                        {
                            dgvOrders.Rows[i].Cells["selectt"].Value = checkSelectAll.Checked;
                        }
                    }
                    else
                    {
                        int isCheckAll_int = 0;
                        for (int i = 0; i < dgvOrders.Rows.Count; i++)
                        {
                            bool isSelect = false, boolTemp = false;
                            isSelect = dgvOrders.Rows[i].Cells["selectt"].Value == null ? false : bool.TryParse(dgvOrders.Rows[i].Cells["selectt"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvOrders.Rows[i].Cells["selectt"].Value.ToString()) : boolTemp;
                            if (isSelect)
                            {
                                isCheckAll_int += 1;
                            }
                        }
                        if (checkSelectAll.Checked && !isCheckAll_int.Equals(dgvOrders.Rows.Count))
                        {
                            cCheck = 1;
                            checkSelectAll.Checked = false;
                        }
                        else if (!checkSelectAll.Checked && isCheckAll_int.Equals(dgvOrders.Rows.Count))
                        {
                            checkSelectAll.Checked = true;
                        }
                    }

                    JArray jarrayBody = new JArray();
                    for (int i = 0; i < dgvOrders.Rows.Count; i++)
                    {
                        bool isSelect = false, boolTemp = false;
                        isSelect = dgvOrders.Rows[i].Cells["selectt"].Value == null ? false : bool.TryParse(dgvOrders.Rows[i].Cells["selectt"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvOrders.Rows[i].Cells["selectt"].Value.ToString()) : boolTemp;
                        if (isSelect)
                        {
                            string forSAPAmountColumnName = (gForType.Equals("for SAP") ? "doctotal" : "amount_due");
                            totalPendingAmount += Convert.ToDouble(dgvOrders.Rows[i].Cells[forSAPAmountColumnName].Value.ToString());
                            //ids += dgvOrders.Rows[i].Cells["base_id"].Value.ToString() + ",";
                            jarrayBody.Add(Convert.ToInt32(dgvOrders.Rows[i].Cells["id"].Value));
                            //isCheckAll_int += 1;
                        }
                    }
                    //Console.WriteLine(jarrayBody);
                    //checkSelectAll.Checked = isCheckAll_int.Equals(dgvOrders.Rows.Count) ? true : false;

                    //MessageBox.Show(isCheckAll_int.ToString() + "/" + dgvOrders.Rows.Count.ToString());

                    //ids = (string.IsNullOrEmpty(ids) ? "" : ids.Substring(0, ids.Length - 1));
                    lblpendingamount.Text = "Pending Amount: " + totalPendingAmount.ToString("n2");
                    dgvitems.Rows.Clear();
                    string sDate = gForType.Equals("for Payment") ? "&transdate=" : "&transdate=" + dtFromDate.Value.ToString("yyyy-MM-dd");
                    JObject jsonObjectBody = new JObject();
                    jsonObjectBody.Add("ids", jarrayBody);
                    var client = new RestClient(utilityc.URL);
                    client.Timeout = -1;
                    var request = new RestRequest("/api/sales/summary_trans?transtype=" + gSalesType + sDate);
                    Console.WriteLine("/api/sales/summary_trans?transtype=" + gSalesType + sDate);
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.AddParameter("application/json", jsonObjectBody, ParameterType.RequestBody);
                    request.Method = Method.PUT;
                    var response = client.Execute(request);
                    //Console.WriteLine("/api/sales/summary_trans?transtype=" + gSalesType + sDate);
                    //Console.WriteLine(jsonObjectBody);
                    if (response.ErrorMessage == null)
                    {
                        JObject jObject = new JObject();
                        jObject = JObject.Parse(response.Content.ToString());
                        bool isSuccess = false;
                        foreach (var x in jObject)
                        {
                            if (x.Key.Equals("success"))
                            {
                                isSuccess = Convert.ToBoolean(x.Value.ToString());
                            }
                        }
                        if (isSuccess)
                        {
                            string forSAPAmountColumnName = (gForType.Equals("for SAP") ? "doctotal" : "amount_due");
                            foreach (var x in jObject)
                            {
                                if (x.Key.Equals("data"))
                                {
                                    if (x.Value.ToString() != "{}")
                                    {
                                        JObject jObjectData = JObject.Parse(x.Value.ToString());
                                        foreach (var y in jObjectData)
                                        {

                                            if (y.Key.Equals("header"))
                                            {
                                                JObject jObjectHeader = JObject.Parse(y.Value.ToString());
                                                foreach (var z in jObjectHeader)
                                                {
                                                    if (z.Key.Equals("gross"))
                                                    {
                                                        txtGrossPrice.Text = string.IsNullOrEmpty(z.Value.ToString()) ? "0.00" : Convert.ToDouble(z.Value.ToString()).ToString("n2");
                                                    }
                                                    else if (z.Key.Equals("delfee"))
                                                    {
                                                        txtDelFee.Text = string.IsNullOrEmpty(z.Value.ToString()) ? "0.00" : Convert.ToDouble(z.Value.ToString()).ToString("n2");
                                                    }
                                                    else if (z.Key.Equals("disc_amount"))
                                                    {
                                                        txtDiscountAmount.Text = string.IsNullOrEmpty(z.Value.ToString()) ? "0.00" : Convert.ToDouble(z.Value.ToString()).ToString("n2");
                                                    }
                                                    else if (z.Key.Equals("disc_amount"))
                                                    {
                                                        txtDiscountAmount.Text = string.IsNullOrEmpty(z.Value.ToString()) ? "0.00" : Convert.ToDouble(z.Value.ToString()).ToString("n2");
                                                    }
                                                    else if (z.Key.Equals(forSAPAmountColumnName))
                                                    {
                                                        txtlAmountPayable.Text = string.IsNullOrEmpty(z.Value.ToString()) ? "0.00" : Convert.ToDouble(z.Value.ToString()).ToString("n2");
                                                    }
                                                    else if (z.Key.Equals("change"))
                                                    {
                                                        txtChange.Text = string.IsNullOrEmpty(z.Value.ToString()) ? "0.00" : Convert.ToDouble(z.Value.ToString()).ToString("n2");
                                                    }
                                                }
                                            }
                                            else if (y.Key.Equals("row"))
                                            {
                                                JArray jArrayRow = JArray.Parse(y.Value.ToString());
                                                for (int i = 0; i < jArrayRow.Count(); i++)
                                                {
                                                    JObject data = JObject.Parse(jArrayRow[i].ToString());
                                                    String itemName = "";
                                                    double quantity = 0.00, price = 0.00, discountPercent = 0.00, totalPrice = 0.00, discamt = 0.00;
                                                    bool free = false;
                                                    foreach (var z in data)
                                                    {
                                                        if (z.Key.Equals("item_code"))
                                                        {
                                                            itemName = z.Value.ToString();
                                                        }
                                                        else if (z.Key.Equals("quantity"))
                                                        {
                                                            quantity = Convert.ToDouble(z.Value.ToString());
                                                        }
                                                        else if (z.Key.Equals("unit_price"))
                                                        {
                                                            price = Convert.ToDouble(z.Value.ToString());
                                                        }
                                                        else if (z.Key.Equals("discprcnt"))
                                                        {
                                                            discountPercent = Convert.ToDouble(z.Value.ToString());
                                                        }
                                                        else if (z.Key.Equals("linetotal"))
                                                        {
                                                            totalPrice = Convert.ToDouble(z.Value.ToString());
                                                        }
                                                        else if (z.Key.Equals("free"))
                                                        {
                                                            free = Convert.ToBoolean(z.Value.ToString());
                                                        }

                                                        else if (z.Key.Equals("disc_amount"))
                                                        {
                                                            discamt = Convert.ToDouble(z.Value.ToString());
                                                        }
                                                    }
                                                    dgvitems.Rows.Add(itemName, Convert.ToDecimal(string.Format("{0:0.00}", quantity)), Convert.ToDecimal(string.Format("{0:0.00}", price)), Convert.ToDecimal(string.Format("{0:0.00}", discountPercent)), Convert.ToDecimal(string.Format("{0:0.00}", discamt)), Convert.ToDecimal(string.Format("{0:0.00}", totalPrice)), free);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            dgvitems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                            dgvitems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                            lblItemsCount.Text = "Items (" + dgvitems.Rows.Count.ToString("N0") + ")";
                        }
                        else
                        {
                            string msg = "No message response found";
                            foreach (var x in jObject)
                            {
                                if (x.Key.Equals("message"))
                                {
                                    msg = x.Value.ToString();
                                }
                            }
                            if (msg.Equals("Token is invalid"))
                            {
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show("Your login session is expired. Please login again", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    else
                    {
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show(response.ErrorMessage, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            Cursor.Current = Cursors.Default;
            computeChange("POS", "Cell Click");

            //int int_selectAll = 0;
            //for (int i = 0; i < dgvOrders.Rows.Count; i++)
            //{
            //    if (Convert.ToBoolean(dgvOrders.Rows[i].Cells["selectt"].Value.ToString()) == true)
            //    {
            //        int_selectAll += 1;
            //    }
            //}
            //if (int_selectAll <= 0 && checkSelectAll.Checked)
            //{
            //    checkSelectAll.Checked = false;
            //}
        }

        private void dgvOrders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int btnEditAmountPayableIndex = dgvOrders.Columns["btnEditAmountPayable"].Index; 
            if (dgvOrders.Rows.Count > 0)
            {
                if (e.ColumnIndex == 0)
                {
                    selectOrders(false);
                }
                else if (e.ColumnIndex == btnEditAmountPayableIndex)
                {
                    string forSAPAmountColumnName = (gForType.Equals("for SAP") ? "doctotal" : "amount_due");
                    dgvOrders.CurrentCell = dgvOrders[0, dgvOrders.CurrentRow.Index];
                    dgvOrders.ClearSelection();
                    dgvOrders.CurrentRow.Cells[forSAPAmountColumnName].Selected = true;
                    EnterAmount frm = new EnterAmount();
                    EnterAmount.amount = 0.00;
                    EnterAmount.amount = Convert.ToDouble(dgvOrders.CurrentRow.Cells[forSAPAmountColumnName].Value.ToString());
                    frm.reference = dgvOrders.CurrentRow.Cells["reference"].Value.ToString();
                    frm.ShowDialog();
                    dgvOrders.CurrentRow.Cells[forSAPAmountColumnName].Value = Convert.ToDecimal(string.Format("{0:0.00}", EnterAmount.amount));
                    computeChange("POS", "");
                }
                else if (e.ColumnIndex == 17)
                {
                    int id = 0, intTemp = 0;
                    id = Int32.TryParse(dgvOrders.CurrentRow.Cells["id"].Value.ToString(), out intTemp) ? Convert.ToInt32(dgvOrders.CurrentRow.Cells["id"].Value.ToString()) : intTemp;
                    JObject jo = JObject.Parse(salesDetails(id));

                    DataTable dt = populateToPay(jo);
                    print_toPay frm = new print_toPay();
                    frm.dtResult = dt;
                    frm.ShowDialog();
                }
            }
        }

        public DataTable populateToPay(JObject jo)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("cust_code", typeof(string));
            dt.Columns.Add("reference", typeof(string));
            dt.Columns.Add("transdate", typeof(string));
            dt.Columns.Add("item_code", typeof(string));

            dt.Columns.Add("quantity", typeof(double));
            dt.Columns.Add("price", typeof(double));
            dt.Columns.Add("discount_amount", typeof(double));
            dt.Columns.Add("net_amount", typeof(double));
            dt.Columns.Add("gross_amount", typeof(double));

            dt.Columns.Add("total_gross_amount", typeof(double));
            dt.Columns.Add("total_discount_amount", typeof(double));
            dt.Columns.Add("total_net_amount", typeof(double));


            bool isSuccess = false, boolTemp = false;
            foreach (var q in jo)
            {
                if (q.Key.Equals("success"))
                {
                    isSuccess = bool.TryParse(q.Value.ToString(), out boolTemp) ? Convert.ToBoolean(q.Value.ToString()) : boolTemp;
                    break;
                }
            }
            if (isSuccess)
            {
                JArray jaSalesRow = new JArray();
                string customerCode = "", referenceNumber = "";
                DateTime dtTransDate = new DateTime(), dateTImeTemp = new DateTime();
                double totalGross = 0.00, totalDiscount = 0.00, totalNet = 0.00, doubleTemp = 0.00;
                foreach (var q in jo)
                {
                    if (q.Key.Equals("data"))
                    {
                        if (q.Value.ToString().Substring(0, 1).Equals("{"))
                        {
                            JObject joData = JObject.Parse(q.Value.ToString());
                            foreach (var w in joData)
                            {
                                if (w.Key.Equals("salesrow"))
                                {
                                    if (w.Value.ToString().Substring(0, 1).Equals("["))
                                    {
                                        jaSalesRow = JArray.Parse(w.Value.ToString());
                                    }
                                }
                                else if (w.Key.Equals("reference"))
                                {
                                    referenceNumber = w.Value.ToString();
                                }
                                else if (w.Key.Equals("transdate"))
                                {
                                    string replaceT = w.Value.ToString().Replace("T", "");
                                    dtTransDate = DateTime.TryParse(replaceT, out dateTImeTemp) ? Convert.ToDateTime(replaceT) : new DateTime();
                                }
                                else if (w.Key.Equals("cust_code"))
                                {
                                    customerCode = w.Value.ToString();
                                }
                                else if (w.Key.Equals("gross"))
                                {
                                    totalGross = double.TryParse(w.Value.ToString(), out doubleTemp) ? Convert.ToDouble(w.Value.ToString()) : doubleTemp;
                                }
                                else if (w.Key.Equals("disc_amount"))
                                {
                                    totalDiscount = double.TryParse(w.Value.ToString(), out doubleTemp) ? Convert.ToDouble(w.Value.ToString()) : doubleTemp;
                                }
                                else if (w.Key.Equals("gross"))
                                {
                                    totalGross = double.TryParse(w.Value.ToString(), out doubleTemp) ? Convert.ToDouble(w.Value.ToString()) : doubleTemp;
                                }
                                else if (w.Key.Equals("doctotal"))
                                {
                                    totalNet = double.TryParse(w.Value.ToString(), out doubleTemp) ? Convert.ToDouble(w.Value.ToString()) : doubleTemp;
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < jaSalesRow.Count(); i++)
                {
                    JObject joData = JObject.Parse(jaSalesRow[i].ToString());
                    string itemCode = "";
                    double quantity = 0.00, price = 0.00, discAmount = 0.00, netAmount = 0.00, grossAmount = 0.00;
                    foreach (var q in joData)
                    {
                        if (q.Key.Equals("item_code"))
                        {
                            itemCode = q.Value.ToString();
                        }
                        else if (q.Key.Equals("quantity"))
                        {
                            quantity = double.TryParse(q.Value.ToString(), out doubleTemp) ? Convert.ToDouble(q.Value.ToString()) : doubleTemp;
                        }
                        else if (q.Key.Equals("unit_price"))
                        {
                            price = double.TryParse(q.Value.ToString(), out doubleTemp) ? Convert.ToDouble(q.Value.ToString()) : doubleTemp;
                        }
                        else if (q.Key.Equals("disc_amount"))
                        {
                            discAmount = double.TryParse(q.Value.ToString(), out doubleTemp) ? Convert.ToDouble(q.Value.ToString()) : doubleTemp;
                        }
                        else if (q.Key.Equals("gross"))
                        {
                            grossAmount = double.TryParse(q.Value.ToString(), out doubleTemp) ? Convert.ToDouble(q.Value.ToString()) : doubleTemp;
                        }
                        else if (q.Key.Equals("linetotal"))
                        {
                            netAmount = double.TryParse(q.Value.ToString(), out doubleTemp) ? Convert.ToDouble(q.Value.ToString()) : doubleTemp;
                        }
                    }
                    dt.Rows.Add(customerCode, referenceNumber, dtTransDate.ToString("MM/dd/yyyy"), itemCode, quantity, price, discAmount, netAmount, grossAmount, totalGross, totalDiscount, totalNet);
                }
            }
            return dt;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (dgvOrders.Rows.Count <= 0)
            {
                MessageBox.Show("No order found", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                int countSelected = 0;
                for (int i = 0; i < dgvOrders.Rows.Count; i++)
                {
                    bool isSelect = false, boolTemp = false;
                    isSelect = dgvOrders.Rows[i].Cells["selectt"].Value == null ? false : bool.TryParse(dgvOrders.Rows[i].Cells["selectt"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvOrders.Rows[i].Cells["selectt"].Value.ToString()) : boolTemp;
                    if (isSelect)
                    {
                        countSelected += 1;
                    }
                }
                if (countSelected <= 0)
                {
                    MessageBox.Show("No order selected", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    DialogResult dialogResult = MessageBox.Show("Are you sure you want to " + (gForType == "for Payment" ? "pay" : "confirm") + "?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        if (gForType == "for Payment")
                        {
                            if (insertTransaction(true, 0))
                            {
                                dtSelectedDeposit.Rows.Clear();
                                string subURL = gForType == "for Payment" ? "/api/payment/new" : "/api/sales/for_confirm";
                                loadData(subURL);
                            }
                        }
                        else if (gForType == "for Confirmation")
                        {
                            confirmARSales();
                        }
                        else if (gForType == "for SAP")
                        {
                            string ids = "";
                            int int_hasSelected = 0;
                            for (int i = 0; i < dgvOrders.Rows.Count; i++)
                            {
                                bool isSelect = false, boolTemp = false;
                                isSelect = dgvOrders.Rows[i].Cells["selectt"].Value == null ? false : bool.TryParse(dgvOrders.Rows[i].Cells["selectt"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvOrders.Rows[i].Cells["selectt"].Value.ToString()) : boolTemp;
                                if (isSelect)
                                {
                                    ids = ids + "," + dgvOrders.Rows[i].Cells["id"].Value.ToString();
                                    int_hasSelected += 1;
                                }
                            }
                            ids = (string.IsNullOrEmpty(ids) ? "" : ids.Substring(1));
                            if (int_hasSelected > 0)
                            {
                                forSAPAR_SAPNumber forSAPAR_SAPNumber = new forSAPAR_SAPNumber();
                                forSAPAR_SAPNumber.ids = ids;
                                forSAPAR_SAPNumber.ShowDialog();
                                if (forSAPAR_SAPNumber.isSubmit)
                                {
                                    string subURL = gForType == "for Payment" ? "/api/payment/new" : "/api/sales/for_confirm";
                                    loadData(subURL);
                                }
                            }
                        }
                    }
                }
            }
        }

        public string  salesDetails(int selectedID)
        {
            string result = "";
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

                    var request = new RestRequest("/api/sales/details/" + selectedID);
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.Method = Method.GET;

                    var response = client.Execute(request);

                    if (response.ErrorMessage == null)
                    {
                        result = response.Content;
                    }
                    else
                    {
                        result = response.ErrorMessage;
                    }
                }
            }
            return result;
        }

        public void confirmARSales()
        {
            if (dgvOrders.Rows.Count <= 0)
            {
                MessageBox.Show("No order found", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                int countSelected = 0;
                for (int i = 0; i < dgvOrders.Rows.Count; i++)
                {
                    bool isSelect = false, boolTemp = false;
                    isSelect = dgvOrders.Rows[i].Cells["selectt"].Value == null ? false : bool.TryParse(dgvOrders.Rows[i].Cells["selectt"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvOrders.Rows[i].Cells["selectt"].Value.ToString()) : boolTemp;
                    if (isSelect)
                    {
                        countSelected += 1;
                    }
                }
                if (countSelected <= 0)
                {
                    MessageBox.Show("No order selected", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    string ids = "";
                    for (int i = 0; i < dgvOrders.Rows.Count; i++)
                    {
                        bool isSelect = false, boolTemp = false;
                        isSelect = dgvOrders.Rows[i].Cells["selectt"].Value == null ? false : bool.TryParse(dgvOrders.Rows[i].Cells["selectt"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvOrders.Rows[i].Cells["selectt"].Value.ToString()) : boolTemp;
                        if (isSelect)
                        {
                            //totalPendingAmount += Convert.ToDouble(dgvOrders.Rows[i].Cells["amountdue"].Value.ToString());
                            ids = ids + "," + dgvOrders.Rows[i].Cells["id"].Value.ToString();
                        }
                    }
                    ids = (string.IsNullOrEmpty(ids) ? "" : ids.Substring(1));

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
                            var request = new RestRequest("/api/sales/confirm?ids=%5B" + ids + "%5D");
                            request.AddHeader("Authorization", "Bearer " + token);
                            request.Method = Method.PUT;

                            var response = client.Execute(request);

                            if (response.ErrorMessage == null)
                            {
                                JObject jObjectResponse = JObject.Parse(response.Content);
                                bool isSuccess = false;
                                foreach (var x in jObjectResponse)
                                {
                                    if (x.Key.Equals("success"))
                                    {
                                        isSuccess = true;
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
                                MessageBox.Show(msg, isSuccess ? "Success" : "Validation", MessageBoxButtons.OK, isSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                                if (isSuccess)
                                {
                                    string subURL = gForType == "for Payment" ? "/api/payment/new" : "/api/sales/for_confirm";
                                    loadData(subURL);
                                }
                            }
                            else
                            {
                                MessageBox.Show(response.ErrorMessage, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
            }
        }

        public bool insertTransaction(bool isMultipleTransaction, double ARSalesAmount)
        {
            bool result = false;
            Cursor.Current = Cursors.WaitCursor;
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
                    var request = new RestRequest("/api/payment/new");
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.Method = Method.POST;
                    JObject joBody = new JObject();
                    if (isMultipleTransaction)
                    {
                        DataTable dtSelectedDepositTEMP = new DataTable();
                        dtSelectedDepositTEMP.Columns.Add("id");
                        dtSelectedDepositTEMP.Columns.Add("amount");
                        dtSelectedDepositTEMP.Columns.Add("payment_type");
                        dtSelectedDepositTEMP.Columns.Add("sapnum");
                        dtSelectedDepositTEMP.Columns.Add("reference2");

                        foreach (DataRow row in dtSelectedDeposit.Rows)
                        {
                            dtSelectedDepositTEMP.Rows.Add(row["id"], row["amount"], row["payment_type"], row["sapnum"], row["reference2"]);
                        }
                        JObject joHeader = new JObject();
                        JArray jaPaymentRows = new JArray();
                        JArray jaSalesDetails = new JArray();
                        string headerCustCode = "";
                        //LOOP payment types
                        double paymentMethodAmount = 0.00;
                        foreach (DataRow row in dtSelectedDepositTEMP.Rows)
                        {
                            JObject joPaymentRows = new JObject();
                            if (row["id"].ToString() != "")
                            {
                                joPaymentRows.Add("deposit_id", Convert.ToInt32(row["id"].ToString()));
                            }
                            joPaymentRows.Add("payment_type", row["payment_type"].ToString());
                            double amt = string.IsNullOrEmpty(row["amount"].ToString()) ? 0.00 : Convert.ToDouble(row["amount"].ToString());

                            joPaymentRows.Add("amount", amt);
                            paymentMethodAmount += Convert.ToDouble(row["amount"].ToString());
                            if (string.IsNullOrEmpty(row["sapnum"].ToString()))
                            {
                                joPaymentRows.Add("sap_number", null);
                            }
                            else
                            {
                                joPaymentRows.Add("sap_number", Convert.ToInt32(row["sapnum"].ToString()));
                            }
                            joPaymentRows.Add("reference2", string.IsNullOrEmpty(row["reference2"].ToString()) ? null : row["reference2"].ToString());
                            jaPaymentRows.Add(joPaymentRows);
                        }
                        double totalAmount = 0.00;
                        int firstLoop = 0;
                        for (int i = 0; i < dgvOrders.Rows.Count; i++)
                        {
                            bool isSelect = false, boolTemp = false;
                            isSelect = dgvOrders.Rows[i].Cells["selectt"].Value == null ? false : bool.TryParse(dgvOrders.Rows[i].Cells["selectt"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvOrders.Rows[i].Cells["selectt"].Value.ToString()) : boolTemp;
                            if (isSelect)
                            {
                                firstLoop += 1;
                                if (firstLoop == 1)
                                {
                                    //MessageBox.Show(dgvOrders.Rows[i].Cells["cust_code"].Value.ToString());
                                    headerCustCode = dgvOrders.Rows[i].Cells["cust_code"].Value.ToString();
                                }
                                string forSAPAmountColumnName = (gForType.Equals("for SAP") ? "doctotal" : "amount_due");
                                JObject joSalesDetails = new JObject();
                                joSalesDetails.Add("sales_id", Convert.ToInt32(dgvOrders.Rows[i].Cells["id"].Value.ToString()));
                                joSalesDetails.Add("total_payment", Convert.ToDouble(dgvOrders.Rows[i].Cells[forSAPAmountColumnName].Value.ToString()));
                                totalAmount += Convert.ToDouble(dgvOrders.Rows[i].Cells[forSAPAmountColumnName].Value.ToString());
                                jaSalesDetails.Add(joSalesDetails);
                            }
                        }
                        joHeader.Add("cust_code", headerCustCode);
                        joHeader.Add("transdate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                        joHeader.Add("remarks", null);
                        joHeader.Add("reference", null);
                        joBody.Add("header", joHeader);
                        joBody.Add("payment_rows", jaPaymentRows);
                        joBody.Add("sales_details", jaSalesDetails);
                    }
                    Console.WriteLine(joBody);
                    request.AddParameter("application/json", joBody, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    if (response.Content.StartsWith("{"))
                    {
                        JObject jObjectResponse = JObject.Parse(response.Content);

                        foreach (var x in jObjectResponse)
                        {
                            if (x.Key.Equals("success"))
                            {
                                if (Convert.ToBoolean(x.Value.ToString()))
                                {
                                    result = true;
                                }
                            }
                        }

                        if (!result)
                        {
                            string msg = "Object message key not found";
                            foreach (var x in jObjectResponse)
                            {
                                if (x.Key.Equals("message"))
                                {
                                    msg = x.Value.ToString();
                                }
                            }
                            if (msg.Equals("Token is invalid"))
                            {
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show("Your login session is expired. Please login again", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        else
                        {
                            string msg = "Object message key not found";
                            foreach (var x in jObjectResponse)
                            {
                                if (x.Key.Equals("message"))
                                {
                                    msg = x.Value.ToString();
                                }
                            }
                            Cursor.Current = Cursors.Default;
                            MessageBox.Show(msg, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show(response.Content, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            Cursor.Current = Cursors.Default;
            return result;
        }

        private void btnrefresh_Click(object sender, EventArgs e)
        {
            string subURL = gForType == "for Payment" ? "/api/payment/new" : "/api/sales/for_confirm";
            loadData(subURL);
        }

        private void btnVoid_Click(object sender, EventArgs e)
        {
            if (dgvOrders.Rows.Count <= 0)
            {
                MessageBox.Show("No order found", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                int countSelected = 0;
                for (int i = 0; i < dgvOrders.Rows.Count; i++)
                {
                    bool isSelect = false, boolTemp = false;
                    isSelect = dgvOrders.Rows[i].Cells["selectt"].Value == null ? false : bool.TryParse(dgvOrders.Rows[i].Cells["selectt"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvOrders.Rows[i].Cells["selectt"].Value.ToString()) : boolTemp;
                    if (isSelect)
                    {
                        countSelected += 1;
                    }
                }
                if (countSelected <= 0)
                {
                    MessageBox.Show("No order selected", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    string ids = "";
                    int count = 0;
                    string selectedRef = "Selected #: " + Environment.NewLine;
                    for (int i = 0; i < dgvOrders.Rows.Count; i++)
                    {
                        bool isSelect = false, boolTemp = false;
                        isSelect = dgvOrders.Rows[i].Cells["selectt"].Value == null ? false : bool.TryParse(dgvOrders.Rows[i].Cells["selectt"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvOrders.Rows[i].Cells["selectt"].Value.ToString()) : boolTemp;
                        if (isSelect)
                        {
                            //totalPendingAmount += Convert.ToDouble(dgvOrders.Rows[i].Cells["amountdue"].Value.ToString());
                            ids = ids + "," + dgvOrders.Rows[i].Cells["id"].Value.ToString();
                            selectedRef += dgvOrders.Rows[i].Cells["reference"].Value.ToString() + Environment.NewLine;
                            count += 1;
                        }
                    }
                    ids = (string.IsNullOrEmpty(ids) ? "" : ids.Substring(1));

                    voidForm voidd = new voidForm();
                    //voidd.baseNum = Convert.ToInt32(dgvOrders.CurrentRow.Cells["transnumber"].Value.ToString());
                    voidd.lblOrderNumber.Text = "(" + count.ToString("N0") + ")";
                    voidd.selectedReference = selectedRef;
                    voidd.selectedID = ids;
                    voidd.ShowDialog();
                    if (voidForm.isSubmit)
                    {
                        string subURL = gForType == "for Payment" ? "/api/payment/new" : "/api/sales/for_confirm";
                        loadData(subURL);
                        lblpendingamount.Text = "Selected Amount: 0.00";
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectAdvancePayment selectAdvancePayment = new SelectAdvancePayment(this.Name);
            selectAdvancePayment.ShowDialog();
            computeChange("POS", "");
        }

        public void computeChange(string type, string type2)
        {
            //dtSelectedDeposit.Columns.Add("id");
            //dtSelectedDeposit.Columns.Add("amount");
            //dtSelectedDeposit.Columns.Add("payment_type");
            //dtSelectedDeposit.Columns.Add("sapnum");
            //dtSelectedDeposit.Columns.Add("reference2");
            DataTable dtTemp = dtSelectedDeposit;
            double selectedAmount = 0.00, totalAmtPayable = 0.00;
            if (type.Equals("POS"))
            {
                string forSAPAmountColumnName = (gForType.Equals("for SAP") ? "doctotal" : "amount_due");
                for (int i = 0; i < dgvOrders.Rows.Count; i++)
                {
                    bool isSelect = false, boolTemp = false;
                    isSelect = dgvOrders.Rows[i].Cells["selectt"].Value == null ? false : bool.TryParse(dgvOrders.Rows[i].Cells["selectt"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvOrders.Rows[i].Cells["selectt"].Value.ToString()) : boolTemp;
                    if (isSelect)
                    {
                        selectedAmount += string.IsNullOrEmpty(dgvOrders.Rows[i].Cells["tenderamt"].Value.ToString()) ? 0.00 : Convert.ToDouble(dgvOrders.Rows[i].Cells["tenderamt"].Value.ToString());
                        totalAmtPayable += string.IsNullOrEmpty(dgvOrders.Rows[i].Cells[forSAPAmountColumnName].Value.ToString()) ? 0.00 : Convert.ToDouble(dgvOrders.Rows[i].Cells[forSAPAmountColumnName].Value.ToString());
                    }
                }
            }
            else
            {
                string forSAPAmountColumnName = (gForType.Equals("for SAP") ? "doctotal" : "amount_due");
                for (int i = 0; i < dgvOrders.Rows.Count; i++)
                {
                    bool isSelect = false, boolTemp = false;
                    isSelect = dgvOrders.Rows[i].Cells["selectt"].Value == null ? false : bool.TryParse(dgvOrders.Rows[i].Cells["selectt"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvOrders.Rows[i].Cells["selectt"].Value.ToString()) : boolTemp;
                    if (isSelect)
                    {
                        totalAmtPayable += string.IsNullOrEmpty(dgvOrders.Rows[i].Cells[forSAPAmountColumnName].Value.ToString()) ? 0.00 : Convert.ToDouble(dgvOrders.Rows[i].Cells[forSAPAmountColumnName].Value.ToString());
                    }
                }
                foreach (DataRow row in dtTemp.Rows)
                {
                    if (row["type"].ToString().Equals("POS"))
                    {
                        selectedAmount += string.IsNullOrEmpty(row["amount"].ToString()) ? 0.00 : Convert.ToDouble(row["amount"].ToString());
                    }
                }
            }
            txtlAmountPayable.Text = totalAmtPayable.ToString("n2");
            double deletedAmount = 0.00;
            bool hasPOS = false;
            if (dtTemp.Rows.Count > 0)
            {
                for (int i = 0; i < dtTemp.Rows.Count; i++)
                {
                    DataRow row = dtTemp.Rows[i];
                    if (row["type"].ToString().Equals("POS"))
                    {
                        deletedAmount = Convert.ToDouble(row["amount"].ToString());
                        dtSelectedDeposit.Rows.RemoveAt(i);
                        hasPOS = true;
                    }
                }
            }

            if (type2.Equals("Cell Click") && selectedAmount > 0)
            {
                dtSelectedDeposit.Rows.Add(null, selectedAmount, "CASH", "", "", "POS");
            }
            else if ((selectedAmount > 0 && hasPOS))
            {
                selectedAmount -= deletedAmount == selectedAmount ? 0.00 : deletedAmount;
                dtSelectedDeposit.Rows.Add(null, selectedAmount, "CASH", "", "", "POS");
            }

            double totalPayment = 0.00;
            if (dtTemp.Rows.Count > 0)
            {
                foreach (DataRow row in dtTemp.Rows)
                {
                    totalPayment += (string.IsNullOrEmpty(row["amount"].ToString()) ? 0.00 : Convert.ToDouble(row["amount"].ToString()));
                }
            }
            txtTotalPayment.Text = totalPayment.ToString("n2");
            double num1 = (string.IsNullOrEmpty(txtTotalPayment.Text) ? 0.00 : Convert.ToDouble(txtTotalPayment.Text));
            double amountDue = (string.IsNullOrEmpty(txtlAmountPayable.Text) ? 0.00 : Convert.ToDouble(txtlAmountPayable.Text));
            double change = num1 - amountDue;
            txtChange.Text = (change > 0 ? change : 0.00).ToString("n2");
        }

        private void dgvitems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvitems.Rows.Count > 0 && dgvOrders.Rows.Count > 0)
            {
                if (e.RowIndex >= 0)
                {
                    try
                    {

                        JArray jarrayBody = new JArray();
                        for (int i = 0; i < dgvOrders.Rows.Count; i++)
                        {
                            bool isSelect = false, boolTemp = false;
                            isSelect = dgvOrders.Rows[i].Cells["selectt"].Value == null ? false : bool.TryParse(dgvOrders.Rows[i].Cells["selectt"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvOrders.Rows[i].Cells["selectt"].Value.ToString()) : boolTemp;
                            if (isSelect)
                            {
                                jarrayBody.Add(Convert.ToInt32(dgvOrders.Rows[i].Cells["id"].Value));
                            }
                        }

                        Cursor.Current = Cursors.WaitCursor;
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
                                JObject jsonObjectBody = new JObject();
                                jsonObjectBody.Add("ids", jarrayBody);
                                jsonObjectBody.Add("discount", Convert.ToDouble(dgvitems.CurrentRow.Cells["discpercent"].Value.ToString()));
                                jsonObjectBody.Add("item_code", dgvitems.CurrentRow.Cells["item"].Value.ToString());
                                Console.WriteLine(jsonObjectBody);
                                var request = new RestRequest("/api/sales/item/transaction/details");
                                request.AddHeader("Authorization", "Bearer " + token);

                                request.AddParameter("application/json", jsonObjectBody, ParameterType.RequestBody);
                                request.Method = Method.PUT;
                                var response = client.Execute(request);
                                ItemDiscount itemDisc = new ItemDiscount();
                                if (response.ErrorMessage == null)
                                {
                                    MessageBox.Show(response.Content.ToString());
                                    itemDisc.jsonResponse = response.Content.ToString();
                                }
                                else
                                {
                                    itemDisc.jsonResponse = response.ErrorMessage;
                                }
                                itemDisc.ShowDialog();
                                Cursor.Current = Cursors.Default;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }

        private void checkDate_CheckedChanged(object sender, EventArgs e)
        {
            dtFromDate.Visible = checkDate.Checked;
        }
        private void checkToDate_CheckedChanged(object sender, EventArgs e)
        {
            dtToDate.Visible = checkToDate.Checked;
        }

        public bool IsNumeric(string text)
        {
            double test;
            return double.TryParse(text, out test);
        }

        private void dgvOrders_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string forSAPAmountColumnName = (gForType.Equals("for SAP") ? "doctotal" : "amount_due");
            if (e.ColumnIndex == 6)
            {
                if (!IsNumeric(dgvOrders.CurrentRow.Cells[forSAPAmountColumnName].Value.ToString()))
                {
                    MessageBox.Show("Total Payment must be a number", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgvOrders.CurrentRow.Cells[forSAPAmountColumnName].Value = "0.00";
                }
            }
        }

        private void btnSearchQuery2_Click(object sender, EventArgs e)
        {
            string subURL = gForType == "for Payment" ? "/api/payment/new" : "/api/sales/for_confirm";
            loadData(subURL);
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            string subURL = gForType == "for Payment" ? "/api/payment/new" : "/api/sales/for_confirm";
            loadData(subURL);
        }

        private void dgvOrders_Sorted(object sender, EventArgs e)
        {
            highlightHaveDiscount();
        }

        private void cmbSearchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            searchFilter();
        }

        public void pay(JObject jObjectPay, DataTable dtDeposit)
        {
            int id = 0;
            double amount = 0.00;
            foreach (var q in jObjectPay)
            {
                if (q.Key.Equals("id"))
                {
                    id = Convert.ToInt32(q.Value.ToString());
                }
                else if (q.Key.Equals("amount"))
                {
                    amount = Convert.ToDouble(q.Value.ToString());
                }
            }
            //DataTable dtRows = new DataTable();
            //dtRows.Columns.Add("id");
            //dtRows.Columns.Add("amount");

            while (amount > 0)
            {
                foreach (DataRow row in dtDeposit.Rows)
                {
                    if (Convert.ToDouble(row["amount"].ToString()) != 0.00)
                    {
                        double payment = 0.00;
                        if (Convert.ToDouble(row["amount"]) >= amount)
                        {
                            payment += amount;
                            row["amount"] = (Convert.ToDouble(row["amount"].ToString()) - amount);
                            dtPayment.Rows.Add(row["id"], payment, row["payment_type"], row["sapnum"], row["reference2"]);
                            amount = 0;
                            break;
                        }
                        else if (Convert.ToDouble(row["amount"]) < amount)
                        {
                            payment += Convert.ToDouble(row["amount"]);
                            amount -= Convert.ToDouble(row["amount"].ToString());
                            dtPayment.Rows.Add(row["id"], payment, row["payment_type"], row["sapnum"], row["reference2"]);
                            row["amount"] = 0;
                        }
                    }
                }
                break;
            }
        }

        private void cmbBranches_SelectedValueChanged(object sender, EventArgs e)
        {
            loadSalesAgent();
        }

        public DataTable getSelectedRows()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("base_id");
            dt.Columns.Add("base_num");
            dt.Columns.Add("cust_code");
            dt.Columns.Add("amountdue");
            dt.Columns.Add("tenderamount");
            dt.Columns.Add("tendertype");
            string forSAPAmountColumnName = (gForType.Equals("for SAP") ? "doctotal" : "amount_due");
            for (int i = 0; i < dgvOrders.Rows.Count; i++)
            {
                bool isSelect = false, boolTemp = false;
                isSelect = dgvOrders.Rows[i].Cells["selectt"].Value == null ? false : bool.TryParse(dgvOrders.Rows[i].Cells["selectt"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvOrders.Rows[i].Cells["selectt"].Value.ToString()) : boolTemp;
                if (isSelect)
                {
                    dt.Rows.Add(Convert.ToInt32(dgvOrders.Rows[i].Cells["id"].Value.ToString()), dgvOrders.Rows[i].Cells["transnumber"].Value.ToString(), dgvOrders.Rows[i].Cells["cust_code"].Value.ToString(), Convert.ToDouble(dgvOrders.Rows[i].Cells[forSAPAmountColumnName].Value.ToString()), Convert.ToDouble(dgvOrders.Rows[i].Cells["tenderamount"].Value.ToString()), dgvOrders.Rows[i].Cells["tendertype"].Value.ToString());
                }
            }
            return dt;
        }

        private void txtsearch_Enter(object sender, EventArgs e)
        {
            if (txtsearch.Text.ToLower().Equals("search trans. #") || txtsearch.Text.ToLower().Equals("search cust. code"))
            {
                txtsearch.Text = string.Empty;
                txtsearch.ForeColor = Color.Black;
            }
        }

        private void txtsearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtsearch.Text.Trim()))
            {
                txtsearch.Text = cmbSearchType.Text.Equals("Trans. #") ? "Search Trans. #" : "Search Cust. Code";
                txtsearch.ForeColor = Color.DimGray;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnPaymentMethod_Click(object sender, EventArgs e)
        {
            PaymentMethodList paymentMethodList = new PaymentMethodList(this.Name);
            paymentMethodList.ShowDialog();
            computeChange("Payment Method", "");
        }

        private void checkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (dgvOrders.Rows.Count > 0)
            {
                if (cCheck == 0)
                {
                    selectOrders(true);
                }
                else
                {
                    cCheck = 0;
                }
            }
        }
    }
}