using Newtonsoft.Json.Linq;
using RestSharp;
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
using AB.API_Class.User;
using AB.API_Class.Payment_Type;
using AB.API_Class.Branch;
using AB.API_Class.Warehouse;
using Newtonsoft.Json;

namespace AB
{
    public partial class CashTransactionReport : Form
    {
        DataTable dtBranch = new DataTable(), dtWarehouse = new DataTable();
        branch_class branchc = new branch_class();
        warehouse_class warehousec = new warehouse_class();
        utility_class utilityc = new utility_class();
        api_class apic = new api_class();
        paymenttype_class paymenttypec = new paymenttype_class();
        user_clas userc = new user_clas();
        DataTable dtUsers = new DataTable();
        DataTable dtCashier = new DataTable();
        public CashTransactionReport()
        {
            InitializeComponent();
        }

        public async Task loadBranch()
        {
            string currentBranch = "";
            bool isAdmin = false;
            cmbBranch.Items.Clear();
            AutoCompleteStringCollection auto = new AutoCompleteStringCollection();
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
            dtBranch = await branchc.returnBranches();
            if (isAdmin)
            {
                dtBranch = await branchc.returnBranches();
                cmbBranch.Items.Add("All");
                foreach (DataRow row in dtBranch.Rows)
                {
                    auto.Add(row["name"].ToString());
                    cmbBranch.Items.Add(row["name"]);
                }
            }
            else
            {
                foreach (DataRow row in dtBranch.Rows)
                {
                    if (row["code"].ToString() == currentBranch)
                    {
                        auto.Add(row["name"].ToString());
                        cmbBranch.Items.Add(row["name"]);
                        break;
                    }
                }
            }
            //default text 
            //kapag admin or to whse all yung lalabas
            //kapag hindi kung ano yung current whse nya yun yung lalabas
            string branchName = "";
            foreach (DataRow row in dtBranch.Rows)
            {
                if (row["code"].ToString().Trim().ToLower() == currentBranch.Trim().ToLower())
                {
                    branchName = row["name"].ToString();
                    break;
                }
            }
            cmbBranch.SelectedIndex = cmbBranch.Items.IndexOf(branchName);  
            cmbBranch.AutoCompleteCustomSource = auto;
        }

        public async void loadWarehouse()
        {
            string branchCode = "";
            string warehouse = "";
            AutoCompleteStringCollection auto = new AutoCompleteStringCollection();
            foreach (DataRow row in dtBranch.Rows)
            {
                if (cmbBranch.Text.Equals(row["name"].ToString()))
                {
                    branchCode = row["code"].ToString();
                    break;
                }
            }
            dtWarehouse = await Task.Run(() => warehousec.returnWarehouse(branchCode,""));
            cmbWhse.Items.Clear();
            int isAdmin = 0;
            if (Login.jsonResult != null)
            {
                foreach (var x in Login.jsonResult)
                {
                    if (x.Key.Equals("data"))
                    {
                        JObject jObjectData = JObject.Parse(x.Value.ToString());
                        foreach (var y in jObjectData)
                        {
                            if (y.Key.Equals("whse"))
                            {
                                warehouse = y.Value.ToString();
                            }
                            else if (y.Key.Equals("isAdmin") || y.Key.Equals("isManager") || y.Key.Equals("isAccounting"))
                            {
                                if (y.Value.ToString().ToLower() == "true")
                                {
                                    cmbWhse.Items.Add("All");
                                    foreach (DataRow row in dtWarehouse.Rows)
                                    {
                                        auto.Add(row["whsename"].ToString());
                                        cmbWhse.Items.Add(row["whsename"].ToString());
                                        cmbWhse.SelectedIndex = 0;
                                    }
                                    return;
                                }
                                else
                                {
                                    isAdmin += 1;
                                }
                            }
                        }
                    }
                }
            }
            if (isAdmin > 0)
            {
                string whseName = "";
                foreach (DataRow row in dtWarehouse.Rows)
                {
                    if (row["whsecode"].ToString() == warehouse)
                    {
                        auto.Add(row["whsename"].ToString());
                        whseName = row["whsename"].ToString();
                        cmbWhse.Items.Add(whseName);
                    }
                }
                cmbWhse.SelectedIndex = cmbWhse.Items.IndexOf(whseName);
                cmbWhse.AutoCompleteCustomSource = auto;
            }
        }

        public string findWarehouseCode()
        {
            string result = "";
            foreach(DataRow row in dtWarehouse.Rows)
            {
                if(row["whsename"].ToString() == cmbWhse.Text)
                {
                    result = row["whsecode"].ToString();
                    break;
                }
            }
            return result;
        }

        public string findBranchCode()
        {
            string result = "";
            foreach (DataRow row in dtBranch.Rows)
            {
                if (row["name"].ToString() == cmbBranch.Text)
                {
                    result = row["code"].ToString();
                    break;
                }
            }
            return result;
        }

        public void loadUsers(ComboBox cmb, bool isCashier)
        {
            DataTable adtUsers = new DataTable();
            string sBranch = "?branch=" + findBranchCode();
            string sWhse = "&whse=" +findWarehouseCode();
            string sCashier = "&isCashier=" + (isCashier ? "1" : "");
            adtUsers = userc.returnUsers(sBranch + sWhse + sCashier);
            if (isCashier)
            {
                dtUsers = userc.returnUsers(sBranch + sWhse + sCashier);
            }
            else if (isCashier)
            {
                dtCashier = userc.returnUsers(sBranch + sWhse + sCashier);
            }
    
            cmb.Items.Clear();
            cmb.Items.Add("All");
            foreach(DataRow r0w in adtUsers.Rows)
            {
                cmb.Items.Add(r0w["username"].ToString());
            }
            if(cmb.Items.Count > 0)
            {
                cmb.SelectedIndex = 0;
            }
        }

        private async void SalesReport_Load(object sender, EventArgs e)
        {
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgv.Columns["amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            await loadBranch();
            loadPaymentType("sales", cmbSalesType);
            loadPaymentType("payment", cmbPaymentType);
            bg();
        }

        public void bg()
        {
            if (!backgroundWorker1.IsBusy)
            {
                closeForm();
                Loading frm = new Loading();
                frm.BringToFront();
                frm.Show();
                Application.OpenForms[frm.Name].Activate();
                backgroundWorker1.RunWorkerAsync();
            }
        }

        public void loadPaymentType(string urlType, ComboBox cmb)
        {
            cmb.Items.Clear();
            cmb.Items.Add("All");
            DataTable dtPaymentTypes = new DataTable();
            dtPaymentTypes = paymenttypec.loadPaymentType(urlType);       
            if (dtPaymentTypes.Rows.Count > 0)
            {
                foreach (DataRow row in dtPaymentTypes.Rows)
                {
                    cmb.Items.Add(row["code"].ToString());
                }
            }
            cmb.SelectedIndex = 0;
        }

        public void loadData()
        {
            try
            {
                dgv.Invoke(new Action(delegate ()
                {
                    dgv.Rows.Clear();
                }));
                lblCashOnHand.Invoke(new Action(delegate ()
                {
                    lblCashOnHand.Text = "0.00";
                }));
                lblCashSales.Invoke(new Action(delegate ()
                {
                    lblCashSales.Text = "0.00";
                }));
                lblADVCash.Invoke(new Action(delegate ()
                {
                    lblADVCash.Text = "0.00";
                }));
                lblUsedADV.Invoke(new Action(delegate ()
                {
                    lblUsedADV.Text = "0.00";
                }));
                lblBankDeposit.Invoke(new Action(delegate ()
                {
                    lblBankDeposit.Text = "0.00";
                }));
                lblEpay.Invoke(new Action(delegate ()
                {
                    lblEpay.Text = "0.00";
                }));
                lblGCert.Invoke(new Action(delegate ()
                {
                    lblGCert.Text = "0.00";
                }));
                lblComission.Invoke(new Action(delegate ()
                {
                    lblComission.Text = "0.00";
                }));
                lblCashOut.Invoke(new Action(delegate ()
                {
                    lblCashOut.Text = "0.00";
                }));

                int cashierID = 0;
                foreach (DataRow r0wSales in dtUsers.Rows)
                {
                    cmbCashier.Invoke(new Action(delegate ()
                    {
                        if (r0wSales["username"].ToString() == cmbCashier.Text)
                        {
                            cashierID = Convert.ToInt32(r0wSales["userid"].ToString());
                        }
                    }));
                }

                string sCashier = (cashierID <= 0 ? "&cashier_id=" : "&cashier_id=" + cashierID);

                string sSalesType = "";
                cmbSalesType.Invoke(new Action(delegate ()
                {
                    sSalesType = (cmbSalesType.SelectedIndex <= 0 ? "&sales_type=" : "&sales_type=" + cmbSalesType.Text);
                }));

                string sPaymentType = "";
                cmbPaymentType.Invoke(new Action(delegate ()
                {
                    sPaymentType = (cmbPaymentType.SelectedIndex <= 0 ? "&payment_type=" : "&payment_type=" + cmbPaymentType.Text);
                }));

                string sBranch = "";
                cmbBranch.Invoke(new Action(delegate ()
                {
                    sBranch = "&branch=" + findBranchCode();
                }));

                string sWarehouse = "";
                cmbWhse.Invoke(new Action(delegate ()
                {
                    sWarehouse = "&whse=" + findWarehouseCode();
                }));

                string sFromDate = "";
                dtFrom.Invoke(new Action(delegate ()
                {
                    sFromDate = "?from_date=" + dtFrom.Value.ToString("yyyy-MM-dd");
                }));
                string sToDate = "";
                dtTo.Invoke(new Action(delegate ()
                {
                    sToDate = "&to_date=" + dtTo.Value.ToString("yyyy-MM-dd");
                }));

                string sURL = sFromDate + sToDate + sCashier + sSalesType + sPaymentType + sBranch + sWarehouse;
                string sResult = apic.loadData("/api/report/cs", sURL, "", "", Method.GET, true);
                if (!string.IsNullOrEmpty(sResult.Trim()) && sResult.Substring(0, 1).Equals("{"))
                {
                    JObject joResult = JObject.Parse(sResult);
                    JArray sCashTrans = (JArray)joResult["data"]["cash_trans"];
                    JArray sSalesRows = (JArray)joResult["data"]["sales_rows"];
                    DataTable dtSalesRows = (DataTable)JsonConvert.DeserializeObject(sSalesRows.ToString(), (typeof(DataTable)));
                    DataTable dtCashTrans = (DataTable)JsonConvert.DeserializeObject(sCashTrans.ToString(), (typeof(DataTable)));
                    foreach (DataRow row in dtSalesRows.Rows)
                    {
                        dgv.Invoke(new Action(delegate ()
                        {
                            string referenceNumber = row["reference"].ToString(),
                            custCode = row["cust_code"].ToString(), salesType = row["SalesType"].ToString(),
                            paymentType = row["PaymentType"].ToString(), username = row["username"].ToString(), replaceT = row["transdate"].ToString().Replace("T", " "), url = row["url"].ToString();
                            DateTime dtTransdate = Convert.ToDateTime(replaceT);
                            double amount = 0.00, doubleTemp = 0.00;
                            amount = double.TryParse(row["amount"].ToString(), out doubleTemp) ? Convert.ToDouble(row["amount"].ToString()) : doubleTemp;

                            dgv.Rows.Add(referenceNumber, Convert.ToDecimal(string.Format("{0:0.00}", amount)), custCode, salesType, paymentType, username, dtTransdate.ToString("yyyy-MM-dd HH:mm"), url);
                        }));
                    }
                    foreach (DataRow row in dtCashTrans.Rows)
                    {
                        double totalCashOnHand = 0.00, totalCashPayment = 0.00, depositCash = 0.00, bankDep, fromDep = 0.00, epay = 0.00, gCert = 0.00, comission = 0.00, cashOut = 0.00, doubleTemp = 0.00;
                        totalCashOnHand = double.TryParse(row["TotalCashOnHand"].ToString(), out doubleTemp) ? Convert.ToDouble(row["TotalCashOnHand"].ToString()) : doubleTemp;

                        totalCashPayment = double.TryParse(row["TotalCashPayment"].ToString(), out doubleTemp) ? Convert.ToDouble(row["TotalCashPayment"].ToString()) : doubleTemp;

                        depositCash = double.TryParse(row["DepositCash"].ToString(), out doubleTemp) ? Convert.ToDouble(row["DepositCash"].ToString()) : doubleTemp;

                        fromDep = double.TryParse(row["FromDep"].ToString(), out doubleTemp) ? Convert.ToDouble(row["FromDep"].ToString()) : doubleTemp;

                        bankDep = double.TryParse(row["BankDep"].ToString(), out doubleTemp) ? Convert.ToDouble(row["BankDep"].ToString()) : doubleTemp;

                        epay = double.TryParse(row["EPAY"].ToString(), out doubleTemp) ? Convert.ToDouble(row["EPAY"].ToString()) : doubleTemp;

                        gCert = double.TryParse(row["GCert"].ToString(), out doubleTemp) ? Convert.ToDouble(row["GCert"].ToString()) : doubleTemp;

                        //comission = double.TryParse(row["Commission"].ToString(), out doubleTemp) ? Convert.ToDouble(row["Commission"].ToString()) : doubleTemp;

                        cashOut = double.TryParse(row["Cashout"].ToString(), out doubleTemp) ? Convert.ToDouble(row["Cashout"].ToString()) : doubleTemp;

                        //format
                        lblCashOnHand.Invoke(new Action(delegate ()
                        {
                            lblCashOnHand.Text = totalCashOnHand.ToString("n2");
                        }));
                        lblCashSales.Invoke(new Action(delegate ()
                        {
                            lblCashSales.Text = totalCashPayment.ToString("n2");
                        }));
                        lblADVCash.Invoke(new Action(delegate ()
                        {
                            lblADVCash.Text = depositCash.ToString("n2");
                        }));
                        lblUsedADV.Invoke(new Action(delegate ()
                        {
                            lblUsedADV.Text = fromDep.ToString("n2");
                        }));
                        lblBankDeposit.Invoke(new Action(delegate ()
                        {
                            lblBankDeposit.Text = bankDep.ToString("n2");
                        }));
                        lblEpay.Invoke(new Action(delegate ()
                        {
                            lblEpay.Text = epay.ToString("n2");
                        }));
                        lblGCert.Invoke(new Action(delegate ()
                        {
                            lblGCert.Text = gCert.ToString("n2");
                        }));
                        lblComission.Invoke(new Action(delegate ()
                        {
                            lblComission.Text = comission.ToString("n2");
                        }));
                        lblCashOut.Invoke(new Action(delegate ()
                        {
                            lblCashOut.Text = cashOut.ToString("n2");
                        }));
                    }
                }
                lblNoDataFound.Invoke(new Action(delegate ()
                {
                    lblNoDataFound.Visible = (dgv.Rows.Count > 0 ? false : true);
                }));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void closeForm()
        {
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.Name == "Loading")
                {
                    frm.Hide();
                }
            }
        }

        private void btnrefresh_Click(object sender, EventArgs e)
        {
            bg();
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }
        private void cmbBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadWarehouse();
            loadUsers(cmbCashier, true);
        }
        private void btnSearchQuery2_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void dgv_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (dgv.Rows.Count > 0)
            {
                if (e.ColumnIndex == 0)
                {
                    CashTransactionReportItems cashTransactionReportItems = new CashTransactionReportItems();
                    cashTransactionReportItems.URLDetails = dgv.CurrentRow.Cells["url"].Value.ToString();
                    cashTransactionReportItems.ShowDialog();
                    if (CashTransactionReportItems.isSubmit)
                    {
                        loadData();
                    }
                }
            }
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg();
        }

    }
}
