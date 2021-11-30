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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DevExpress.XtraGrid.Columns;
using System.Globalization;
using RestSharp;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
namespace AB
{
    public partial class PaymentTransactionReport : Form
    {
        public PaymentTransactionReport()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        DataTable dtBranch = new DataTable(), dtWhse = new DataTable(), dtCashier = new DataTable(), dtSalesType = new DataTable(), dtPaymentType = new DataTable();
        private void PaymentTransactionReport_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            dtFromDate.EditValue = dtToDate.EditValue = DateTime.Now;
            loadSalesType();
            loadPaymentType();
            loadBranch();
            bg(backgroundWorker1);
        }

        public void loadPaymentType()
        {
            cmbPaymentType.Properties.Items.Clear();

            cmbPaymentType.Properties.Items.Add("All");
            string sResult = apic.loadData("/api/payment/type/get_all", "", "", "", RestSharp.Method.GET, true);
            if (!string.IsNullOrEmpty(sResult.Trim()))
            {
                if (sResult.StartsWith("{"))
                {
                    JObject joResult = JObject.Parse(sResult);
                    JArray jaData = (JArray)joResult["data"];
                    dtPaymentType = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));

                    foreach (DataRow row in dtPaymentType.Rows)
                    {
                        cmbPaymentType.Properties.Items.Add(row["description"].ToString());
                    }
                    cmbPaymentType.SelectedIndex = 0;
                }
            }
            else
            {
                cmbPaymentType.SelectedIndex = 0;
            }
        }

        public void loadSalesType()
        {
            cmbSalesType.Properties.Items.Clear();

            cmbSalesType.Properties.Items.Add("All");
            string sResult = apic.loadData("/api/sales/type/get_all", "", "", "", RestSharp.Method.GET, true);
            if (!string.IsNullOrEmpty(sResult.Trim()))
            {
                if (sResult.StartsWith("{"))
                {
                    JObject joResult = JObject.Parse(sResult);
                    JArray jaData = (JArray)joResult["data"];
                    dtSalesType = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));

                    foreach (DataRow row in dtSalesType.Rows)
                    {
                        cmbSalesType.Properties.Items.Add(row["code"].ToString());
                    }
                    cmbSalesType.SelectedIndex = 0;
                }
            }
            else
            {
                cmbSalesType.SelectedIndex = 0;
            }
        }

        public void loadBranch()
        {
            cmbBranch.Properties.Items.Clear();

            if (apic.haveSalesAccess())
            {
                cmbBranch.Properties.Items.Add("All");
                string sResult = apic.loadData("/api/branch/get_all", "", "", "", RestSharp.Method.GET, true);
                if (!string.IsNullOrEmpty(sResult.Trim()))
                {
                    if (sResult.StartsWith("{"))
                    {
                        JObject joResult = JObject.Parse(sResult);
                        JArray jaData = (JArray)joResult["data"];
                        dtBranch = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));

                        foreach (DataRow row in dtBranch.Rows)
                        {
                            cmbBranch.Properties.Items.Add(row["name"].ToString());
                        }
                        string currentBranchCode = Login.jsonResult["data"]["branch"].ToString();
                        string currentBranchName = apic.findValueInDataTable(dtBranch, currentBranchCode, "code", "name");
                        cmbBranch.SelectedIndex = cmbBranch.Properties.Items.IndexOf(currentBranchName) <= 0 ? 0 : cmbBranch.Properties.Items.IndexOf(currentBranchName);
                    }
                }
                else
                {
                    cmbBranch.SelectedIndex = 0;
                }
            }
            else
            {
                string currentBranch = Login.jsonResult["data"]["branch"] == null ? "" : Login.jsonResult["data"]["branch"].ToString();
                cmbBranch.Properties.Items.Add(currentBranch);
                cmbBranch.SelectedIndex = 0;
            }
        }

        private void cmbBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadWarehouse();
            loadCashier();
        }

        public void bg(BackgroundWorker bgw)
        {
            if (!bgw.IsBusy)
            {
                closeForm();
                Loading frm = new Loading();
                frm.Show();
                bgw.RunWorkerAsync();
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

        public string delegateControlText(Control c)
        {
            string result = "";
            c.Invoke(new Action(delegate ()
            {
                result = c.Text;
            }));
            return result;
        }

        public bool delegateControlCheck(CheckBox c)
        {
            bool result = false;
            c.Invoke(new Action(delegate ()
            {
                result = c.Checked;
            }));
            return result;
        }

        public string delegateControlDouble(string value, Label c)
        {
            string result = "";
            c.Invoke(new Action(delegate ()
            {
                double dVal = 0.00, doubleTemp = 0.00;
                dVal = double.TryParse(value, out doubleTemp) ? Convert.ToDouble(value) : doubleTemp;
                c.Text = dVal.ToString("n2");
            }));
            return result;
        }

        public void loadData()
        {
            try
            {
                gridControl1.Invoke(new Action(delegate ()
                {
                    gridControl1.DataSource = null;
                }));
                bool cCheckFromDate = delegateControlCheck(checkFromDate), cCheckToDate = delegateControlCheck(checkToDate);

                string sFromToDate = "?from_date=", sToDate = "&to_date=", sBranch = "&branch=", sWhse = "&whse=", sPaymentType = "&payment_type=", sSalesType = "&sales_type=", sCashier="&cashier_id=";

                //from date
                sFromToDate += cCheckFromDate ? delegateControlText(dtFromDate) : "";
                //to date
                sToDate += cCheckToDate ? delegateControlText(dtToDate) : "";
                //branch
                sBranch += apic.haveSalesAccess() ? apic.findValueInDataTable(dtBranch, delegateControlText(cmbBranch), "name", "code") : delegateControlText(cmbBranch);
                //whse
                sWhse += apic.haveSalesAccess() ? apic.findValueInDataTable(dtWhse, delegateControlText(cmbWhse), "whsename", "whsecode") : delegateControlText(cmbWhse);
                //payment type
                sPaymentType += apic.findValueInDataTable(dtPaymentType, delegateControlText(cmbPaymentType), "description", "code");
                //sales type
                sSalesType += apic.findValueInDataTable(dtSalesType, delegateControlText(cmbSalesType), "description", "code");
                //cashier
                sCashier += apic.findValueInDataTable(dtCashier, delegateControlText(cmbCashier), "username", "id");

                string sParams = sFromToDate + sToDate + sBranch + sWhse + sPaymentType + sSalesType + sCashier;

                string sResult = apic.loadData("/api/report/cs", sParams, "", "", RestSharp.Method.GET, true);
                Console.WriteLine("/api/report/cs" + sParams);
                if (!string.IsNullOrEmpty(sResult.Trim()))
                {
                    if (sResult.StartsWith("{"))
                    {
                        JObject joResult = JObject.Parse(sResult);
                        JObject joData = (JObject)joResult["data"];

                        JArray jaCashTrans = (JArray)joData["cash_trans"];
                        JObject joCashTrans = (JObject)jaCashTrans[0];


                        //instance cash trans
                        delegateControlDouble(joCashTrans["TotalCashOnHand"].ToString(), lblCashOnHand);
                        delegateControlDouble(joCashTrans["TotalCashPayment"].ToString(), lblCashSales);
                        delegateControlDouble(joCashTrans["DepositCash"].ToString(), lblADVCash);
                        delegateControlDouble(joCashTrans["FromDep"].ToString(), lblUsedADV);
                        delegateControlDouble(joCashTrans["BankDep"].ToString(), lblBankDeposit);
                        delegateControlDouble(joCashTrans["EPAY"].ToString(), lblEpay);
                        delegateControlDouble(joCashTrans["GCert"].ToString(), lblGCert);
                        delegateControlDouble(joCashTrans["Cashout"].ToString(), lblCashOut);

                        JArray jaSalesRows = (JArray)joData["sales_rows"];
                        DataTable dt = (DataTable)JsonConvert.DeserializeObject(jaSalesRows.ToString(), typeof(DataTable));

                        dt.SetColumnsOrder("transdate", "reference", "amount", "cust_code", "SalesType", "PaymentType", "username","url");

                        foreach(DataColumn col in dt.Columns)
                        {
                            Console.WriteLine(col.ColumnName);
                        }

                        gridControl1.Invoke(new Action(delegate ()
                        {
                            gridControl1.DataSource = dt;
                            gridView1.OptionsView.ColumnAutoWidth = false;
                            gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;

                            foreach (GridColumn col in gridView1.Columns)
                            {
                                string fieldName = col.FieldName;
                                string v = col.GetCaption();
                                string s = col.GetCaption().Replace("_", " ");
                                col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());

                                col.Caption = fieldName.Equals("username") ? "Processed By" : col.Caption;

                                col.ColumnEdit = repositoryItemTextEdit1;
                                col.DisplayFormat.FormatType = fieldName.Equals("amount") ? DevExpress.Utils.FormatType.Numeric : fieldName.Equals("transdate") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;
                                col.DisplayFormat.FormatString = fieldName.Equals("amount") ? "n2" : fieldName.Equals("transdate") ? "yyyy-MM-dd HH:mm:ss" : "";
                                col.Visible = !(fieldName.Equals("url"));

                                //fonts
                                FontFamily fontArial = new FontFamily("Arial");
                                col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                                col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                            }
                            gridView1.BestFitColumns();
                            //auto complete
                            string[] suggestions = { "reference", "cust_code" };
                            string suggestConcat = string.Join(";", suggestions);
                            gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                            devc.loadSuggestion(gridView1, gridControl1, suggestions);
                        }));

                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bg(backgroundWorker1);
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg(backgroundWorker1);
        }

        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            string currentURL = gridView1.GetFocusedRowCellValue("url").ToString();
            if (selectedColumnfieldName.Equals("reference"))
            {
                CashTransactionReportItems.isSubmit = false;
                CashTransactionReportItems cashTransactionReportItems = new CashTransactionReportItems();
                cashTransactionReportItems.URLDetails = currentURL;
                cashTransactionReportItems.ShowDialog();
                if (CashTransactionReportItems.isSubmit)
                {
                    bg(backgroundWorker1);
                }
            }
        }

        private void checkFromDate_CheckedChanged(object sender, EventArgs e)
        {
            dtFromDate.Visible = checkFromDate.Checked;
        }

        private void checkToDate_CheckedChanged(object sender, EventArgs e)
        {
            dtToDate.Visible = checkToDate.Checked;
        }

        public void loadWarehouse()
        {
            cmbWhse.Properties.Items.Clear();

            if (apic.haveSalesAccess())
            {
                string branchCode = apic.findValueInDataTable(dtBranch, cmbBranch.Text, "name", "code");
                cmbWhse.Properties.Items.Add("All");
                string sResult = apic.loadData("/api/whse/get_all", "?branch=" + branchCode, "", "", RestSharp.Method.GET, true);
                if (!string.IsNullOrEmpty(sResult.Trim()))
                {
                    if (sResult.StartsWith("{"))
                    {
                        JObject joResult = JObject.Parse(sResult);
                        JArray jaData = (JArray)joResult["data"];
                        dtWhse = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
                        foreach (DataRow row in dtWhse.Rows)
                        {
                            cmbWhse.Properties.Items.Add(row["whsename"].ToString());
                        }
                        string plantCode = Login.jsonResult["data"]["whse"].ToString();
                        string plantName = apic.findValueInDataTable(dtWhse, plantCode, "whsecode", "whsename");
                        cmbWhse.SelectedIndex = cmbWhse.Properties.Items.IndexOf(plantName) <= 0 ? 0 : cmbWhse.Properties.Items.IndexOf(plantName);
                    }
                }
                else
                {
                    cmbWhse.SelectedIndex = 0;
                }
            }
            else
            {
                string currentWhse = Login.jsonResult["data"]["whse"] == null ? "" : Login.jsonResult["data"]["whse"].ToString();
                cmbWhse.Properties.Items.Add(currentWhse);
                cmbWhse.SelectedIndex = 0;
            }
        }

        public void loadCashier()
        {
            cmbCashier.Properties.Items.Clear();
            if (apic.haveSalesAccess())
            {
                string branchCode = apic.findValueInDataTable(dtBranch, cmbBranch.Text, "name", "code");
                cmbCashier.Properties.Items.Add("All");

                string sBranch = "?branch=" + branchCode;
                string sIsSalesAgent = "&isCashier=1";
                string sParams = sBranch + sIsSalesAgent;

                string sResult = apic.loadData("api/auth/user/get_all", sParams, "", "", RestSharp.Method.GET, true);
                if (!string.IsNullOrEmpty(sResult.Trim()))
                {
                    if (sResult.StartsWith("{"))
                    {
                        JObject joResult = JObject.Parse(sResult);
                        JArray jaData = (JArray)joResult["data"];
                        dtCashier = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
                        foreach (DataRow row in dtCashier.Rows)
                        {
                            cmbCashier.Properties.Items.Add(row["username"].ToString());
                        }
                    }
                }
                cmbCashier.SelectedIndex = cmbCashier.Properties.Items.Count > 0 ? 0 : -1;
            }
            else
            {
                string currentUser = Login.jsonResult["data"]["username"] == null ? "" : Login.jsonResult["data"]["username"].ToString();
                cmbCashier.Properties.Items.Add(currentUser);
                cmbCashier.SelectedIndex = 0;
            }
        }
    }
}
