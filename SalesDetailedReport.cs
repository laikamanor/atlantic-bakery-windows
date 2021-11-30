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
    public partial class SalesDetailedReport : Form
    {
        public SalesDetailedReport()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        DataTable dtBranch = new DataTable(), dtWhse = new DataTable(), dtTransType = new DataTable(), dtCustType = new DataTable(), dtSalesAgent = new DataTable();

        private void cmbBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadWarehouse();
            loadSalesAgent();
        }

        private void SalesDetailedReport_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            dtFromDate.Visible = true;
            checkFromDate.Checked = true;
            checkToDate.Checked = true;
            cmbFromTime.SelectedIndex = 0;
            cmbToTime.SelectedIndex = cmbToTime.Properties.Items.Count - 1;
            dtFromDate.EditValue = dtToDate.EditValue = DateTime.Now;
            loadBranch();
            loadTransType();
            loadCustType();
            bg(backgroundWorker1);
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

        private void checkFromDate_CheckedChanged(object sender, EventArgs e)
        {
            dtFromDate.Visible = checkFromDate.Checked;
        }

        private void checkToDate_CheckedChanged(object sender, EventArgs e)
        {
            dtToDate.Visible = checkToDate.Checked;
        }

        private void checkFromTime_CheckedChanged(object sender, EventArgs e)
        {
            cmbFromTime.Visible = checkFromTime.Checked;
        }

        private void checkToTime_CheckedChanged(object sender, EventArgs e)
        {
            cmbToTime.Visible = checkToTime.Checked;
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

        public void loadSalesAgent()
        {
            cmbSalesAgent.Properties.Items.Clear();
            if (apic.haveSalesAccess())
            {
                string branchCode = apic.findValueInDataTable(dtBranch, cmbBranch.Text, "name", "code");
                cmbSalesAgent.Properties.Items.Add("All");

                string sBranch = "?branch=" + branchCode;
                string sIsSalesAgent = "&isSales=1";
                string sParams = sBranch + sIsSalesAgent;

                string sResult = apic.loadData("api/auth/user/get_all", sParams, "", "", RestSharp.Method.GET, true);
                if (!string.IsNullOrEmpty(sResult.Trim()))
                {
                    if (sResult.StartsWith("{"))
                    {
                        JObject joResult = JObject.Parse(sResult);
                        JArray jaData = (JArray)joResult["data"];
                        dtSalesAgent = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
                        foreach (DataRow row in dtSalesAgent.Rows)
                        {
                            cmbSalesAgent.Properties.Items.Add(row["username"].ToString());
                        }
                    }
                }
                cmbSalesAgent.SelectedIndex = cmbSalesAgent.Properties.Items.Count > 0 ? 0 : -1;
            }
            else
            {
                string currentUser = Login.jsonResult["data"]["username"] == null ? "" : Login.jsonResult["data"]["username"].ToString();
                cmbSalesAgent.Properties.Items.Add(currentUser);
                cmbSalesAgent.SelectedIndex = 0;
            }
        }

        public void loadTransType()
        {
            cmbTransType.Properties.Items.Clear();
            cmbTransType.Properties.Items.Add("All");

            string sParams = "";

            string sResult = apic.loadData("/api/sales/type/get_all", sParams, "", "", RestSharp.Method.GET, true);
            if (!string.IsNullOrEmpty(sResult.Trim()))
            {
                if (sResult.StartsWith("{"))
                {
                    JObject joResult = JObject.Parse(sResult);
                    JArray jaData = (JArray)joResult["data"];
                    dtTransType = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
                    foreach (DataRow row in dtTransType.Rows)
                    {
                        cmbTransType.Properties.Items.Add(row["code"].ToString());
                    }
                }
            }
            int indexOf = cmbTransType.Properties.Items.IndexOf("CASH");
            cmbTransType.SelectedIndex = cmbTransType.Properties.Items.Count > 0 ? indexOf : cmbTransType.Properties.Items.Count > 0 ? 0 : -1;
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

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg(backgroundWorker1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bg(backgroundWorker1);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        public void loadCustType()
        {
            cmbCustType.Properties.Items.Clear();
            cmbCustType.Properties.Items.Add("All");

            string sParams = "";

            string sResult = apic.loadData("/api/custtype/get_all", sParams, "", "", RestSharp.Method.GET, true);
            if (!string.IsNullOrEmpty(sResult.Trim()))
            {
                if (sResult.StartsWith("{"))
                {
                    JObject joResult = JObject.Parse(sResult);
                    JArray jaData = (JArray)joResult["data"];
                    dtCustType = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
                    foreach (DataRow row in dtCustType.Rows)
                    {
                        cmbCustType.Properties.Items.Add(row["code"].ToString());
                    }
                }
            }
            int indexOf = cmbCustType.Properties.Items.IndexOf("Cash Sales");
            cmbCustType.SelectedIndex = cmbCustType.Properties.Items.Count > 0 ? indexOf : cmbCustType.Properties.Items.Count > 0 ? 0 : -1;
        }

        private void gridView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            bg(backgroundWorker2);
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            loadItems();
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
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

        public void loadData()
        {
            try
            {
                gridControl1.Invoke(new Action(delegate ()
                {
                    gridControl1.DataSource = null;
                    gridView1.Columns.Clear();
                }));
                gridControl2.Invoke(new Action(delegate ()
                {
                    gridControl2.DataSource = null;
                    gridView2.Columns.Clear();
                    clearBills();
                }));

                //decalre checkbox
                bool cCheckFromDate = delegateControlCheck(checkFromDate), cCheckToDate = delegateControlCheck(checkToDate), cCheckFromTime = delegateControlCheck(checkFromTime), cCheckToTime = delegateControlCheck(checkToTime);


                //delcare filters
                string sFromDate = "?from_date=", sToDate = "&to_date=", sFromTime = "&from_time=", sToTime = "&to_time=", sBranch = "&branch=", sWhse = "&whse=", sSalesAgent = "&user_id=", sTransType = "&transtype=", sCustType = "&cust_type=";

                //from date
                sFromDate += cCheckFromDate ? delegateControlText(dtFromDate) : "";
                //to date
                sToDate += cCheckToDate ? delegateControlText(dtToDate) : "";
                //from time
                sFromTime += cCheckFromTime ? delegateControlText(cmbFromTime) : "";
                //to time
                sToTime += cCheckToTime ? delegateControlText(cmbToTime) : "";
                //branch
                sBranch += apic.haveSalesAccess() ? apic.findValueInDataTable(dtBranch, delegateControlText(cmbBranch), "name", "code") : delegateControlText(cmbBranch);
                //whse
                sWhse += apic.haveSalesAccess() ? apic.findValueInDataTable(dtWhse, delegateControlText(cmbWhse), "whsename", "whsecode") : delegateControlText(cmbWhse);
                //sales agent
                sSalesAgent += apic.haveSalesAccess() ? apic.findValueInDataTable(dtSalesAgent, delegateControlText(cmbSalesAgent), "username", "id") : Login.jsonResult["data"]["id"].ToString();
                //transtype
                sTransType += delegateControlText(cmbTransType).Equals("All") ? "" : delegateControlText(cmbTransType);
                //custtype
                sCustType += apic.findValueInDataTable(dtCustType, delegateControlText(cmbCustType), "code", "id");


                string sParams = sFromDate + sToDate + sFromTime + sToTime + sBranch + sWhse + sSalesAgent + sTransType + sCustType;
                string sResult = apic.loadData("/api/sales/report", sParams, "", "", RestSharp.Method.GET, true);
                if (!string.IsNullOrEmpty(sResult.Trim()))
                {
                    if (sResult.StartsWith("{"))
                    {
                        JObject joResult = JObject.Parse(sResult);
                        JObject joData = (JObject)joResult["data"];
                        JArray jaRow = (JArray)joData["row"];
                        DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaRow.ToString(), typeof(DataTable));
                        gridControl1.Invoke(new Action(delegate ()
                        {
                            dtData.SetColumnsOrder("reference", "gross", "disctype", "disc_amount", "doctotal", "cust_code", "transtype", "remarks", "user", "transdate");
                            gridControl1.DataSource = dtData;
                            gridView1.OptionsView.ColumnAutoWidth = false;
                            gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;

                            foreach (GridColumn col in gridView1.Columns)
                            {
                                string fieldName = col.FieldName;
                                string v = col.GetCaption();
                                string s = col.GetCaption().Replace("_", " ");
                                col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                                col.ColumnEdit = fieldName.Equals("remarks") || fieldName.Equals("cust_code") || fieldName.Equals("disctype") ? repositoryItemMemoEdit1 : repositoryItemTextEdit1;
                                col.DisplayFormat.FormatType = fieldName.Equals("disc_amount") || fieldName.Equals("doctotal") || fieldName.Equals("gross") ? DevExpress.Utils.FormatType.Numeric : fieldName.Equals("transdate") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;
                                col.DisplayFormat.FormatString = fieldName.Equals("disc_amount") || fieldName.Equals("doctotal") || fieldName.Equals("gross") ? "n2" : fieldName.Equals("transdate") ? "yyyy-MM-dd HH:mm:ss" : "";
                                col.Visible = !(fieldName.Equals("id") || fieldName.Equals("transnumber") || fieldName.Equals("delfee"));

                                //fonts
                                FontFamily fontArial = new FontFamily("Arial");
                                col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                                col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                            }

                            //auto complete
                            string[] suggestions = { "reference", "cust_code", "remarks" };
                            string suggestConcat = string.Join(";", suggestions);
                            gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                            devc.loadSuggestion(gridView1, gridControl1, suggestions);

                            var colRef = gridView1.Columns["reference"];
                            if (colRef != null)
                            {
                                gridView1.Columns["reference"].Summary.Clear();
                                gridView1.Columns["reference"].Summary.Add(DevExpress.Data.SummaryItemType.Count, "reference", "Total Transaction: {0:N0}");
                            }

                            gridView1.BestFitColumns();
                            var colDiscType = gridView1.Columns["disctype"];
                            var col1 = gridView1.Columns["cust_code"];
                            var col2 = gridView1.Columns["remarks"];
                            if (colDiscType != null)
                            {
                                colDiscType.Width = 100;
                            }
                            if (col1 != null)
                            {
                                col1.Width = 120;
                            }
                            if (col2 != null)
                            {
                                col2.Width = 200;
                            }
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void gridView1_RowStyle(object sender, RowStyleEventArgs e)
        {
            if(e.RowHandle >= 0)
            {
                double discAmt = 0.00, doubleTemp = 0.00;
                discAmt = double.TryParse(gridView1.GetRowCellValue(e.RowHandle, "disc_amount").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(e.RowHandle, "disc_amount").ToString()) : doubleTemp;
                if(discAmt > 0)
                {
                    e.Appearance.BackColor = Color.FromArgb(234, 245, 140);
                }
            }
        }

        private void repositoryItemTextEdit2_Click(object sender, EventArgs e)
        {

        }

        private void repositoryItemMemoEdit1_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedColumnfieldName = gridView2.FocusedColumn.FieldName;
                //int id = 0, baseID = 0, intTemp = 0;
                //id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
                if (selectedColumnfieldName.Equals("item_code"))
                {
                    int[] selectedRowIndexes = gridView1.GetSelectedRows();
                    int[] selectedIds = new int[selectedRowIndexes.Count()];
                    int counter = 0;
                    foreach (int index in selectedRowIndexes)
                    {
                        int id = 0, intTemp = 0;
                        id = gridView1.GetRowCellValue(index, "id") == null ? 0 : int.TryParse(gridView1.GetRowCellValue(index, "id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(index, "id").ToString()) : intTemp;
                        selectedIds[counter] = id;
                        counter++;
                    }
                    JObject joBody = new JObject();
                    JArray jaIds = new JArray();
                    foreach (int id in selectedIds)
                    {
                        jaIds.Add(id);
                    }
                    joBody.Add("ids", jaIds);
                    //current discount amount
                    double currentDiscAmount = 0.00, doubleTemp = 0.00;
                    currentDiscAmount = double.TryParse(gridView2.GetFocusedRowCellValue("discprcnt").ToString(), out doubleTemp) ? Convert.ToDouble(gridView2.GetFocusedRowCellValue("discprcnt").ToString()) : doubleTemp;
                    string currentItemCode = gridView2.GetFocusedRowCellValue("item_code").ToString();
                    joBody.Add("discount", currentDiscAmount);
                    joBody.Add("item_code", currentItemCode);

                    Console.WriteLine(joBody);

                    string sParams = "";
                    string sResult = apic.loadData("/api/sales/item/transaction/details", sParams, "application/json", joBody.ToString(), RestSharp.Method.PUT, true);


                    JObject joResult = JObject.Parse(sResult);
                    JArray jaData = (JArray)joResult["data"];
                    DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));

                    ItemDiscount2 frm = new ItemDiscount2(dtData);
                    frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        public void loadItems()
        {
            try
            {
                gridControl2.Invoke(new Action(delegate ()
                {
                    gridControl2.DataSource = null;
                    gridView2.Columns.Clear();
                    clearBills();
                }));
                int[] selectedRowIndexes = gridView1.GetSelectedRows();
                int[] selectedIds = new int[selectedRowIndexes.Count()];
                int counter = 0;
                foreach (int index in selectedRowIndexes)
                {
                    int id = 0, intTemp = 0;
                    id = gridView1.GetRowCellValue(index, "id") == null ? 0 : int.TryParse(gridView1.GetRowCellValue(index, "id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(index, "id").ToString()) : intTemp;
                    selectedIds[counter] = id;
                    counter++;
                }
                JObject joBody = new JObject();
                JArray jaIds = new JArray();
                foreach (int id in selectedIds)
                {
                    jaIds.Add(id);
                }
                joBody.Add("ids", jaIds);
                string sParams = "";
                string sResult = apic.loadData("/api/sales/summary_trans", sParams, "application/json", joBody.ToString(), RestSharp.Method.PUT, true);
                if (!string.IsNullOrEmpty(sResult.Trim()))
                {
                    if (sResult.StartsWith("{"))
                    {
                        JObject joResult = JObject.Parse(sResult);
                        JObject joData = (JObject)joResult["data"];


                        JObject joHeader = (JObject)joData["header"];

                        //instance BILLS
                        delegateControlDouble(joHeader["disc_amount"].ToString(), txtDiscountAmount);
                        delegateControlDouble(joHeader["delfee"].ToString(), txtDelFee);
                        delegateControlDouble(joHeader["gross"].ToString(), txtGrossPrice);
                        delegateControlDouble(joHeader["doctotal"].ToString(), txtTotalPayment);
                        delegateControlDouble(joHeader["tenderamt"].ToString(), txtTenderAmount);
                        delegateControlDouble(joHeader["amount_due"].ToString(), txtlAmountPayable);
                        delegateControlDouble(joHeader["change"].ToString(), txtChange);

                        JArray jaRow = (JArray)joData["row"];

                        DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaRow.ToString(), typeof(DataTable));
                        gridControl2.Invoke(new Action(delegate ()
                        {
                            dtData.SetColumnsOrder("item_code", "quantity", "unit_price", "gross", "disc_amount", "discprcnt", "linetotal", "free");
                            gridControl2.DataSource = dtData;
                            gridView2.OptionsView.ColumnAutoWidth = false;
                            gridView2.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;

                            foreach (GridColumn col in gridView2.Columns)
                            {
                                string fieldName = col.FieldName;
                                string v = col.GetCaption();
                                string s = col.GetCaption().Replace("_", " ");
                                col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());

                                col.Caption = fieldName.Equals("linetotal") ? "Total Price" : col.Caption;

                                col.ColumnEdit = fieldName.Equals("item_code") ? repositoryItemMemoEdit1 : repositoryItemTextEdit1;
                                col.DisplayFormat.FormatType = fieldName.Equals("disc_amount") || fieldName.Equals("linetotal") || fieldName.Equals("gross") || fieldName.Equals("unit_price") || fieldName.Equals("quantity") || fieldName.Equals("discprcnt") ? DevExpress.Utils.FormatType.Numeric : fieldName.Equals("transdate") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;
                                col.DisplayFormat.FormatString = fieldName.Equals("disc_amount") || fieldName.Equals("linetotal") || fieldName.Equals("gross") || fieldName.Equals("unit_price") || fieldName.Equals("quantity") || fieldName.Equals("discprcnt") ? "n2" : fieldName.Equals("transdate") ? "yyyy-MM-dd HH:mm:ss" : "";
                                col.Visible = !(fieldName.Equals("id") || fieldName.Equals("free"));

                                col.Fixed = fieldName.Equals("linetotal") ? FixedStyle.Right : FixedStyle.None;

                                //fonts
                                FontFamily fontArial = new FontFamily("Arial");
                                col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                                col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                            }

                            //auto complete
                            string[] suggestions = { "item_code" };
                            string suggestConcat = string.Join(";", suggestions);
                            gridView2.OptionsFind.FindFilterColumns = suggestConcat;
                            devc.loadSuggestion(gridView2, gridControl2, suggestions);

                            var colRef = gridView2.Columns["item_code"];
                            if (colRef != null)
                            {
                                gridView2.Columns["item_code"].Summary.Clear();
                                gridView2.Columns["item_code"].Summary.Add(DevExpress.Data.SummaryItemType.Count, "item_code", "Total Item: {0:N0}");
                            }
                            gridView2.BestFitColumns();
                            var col1 = gridView2.Columns["item_code"];
                            if (col1 != null)
                            {
                                col1.Width = 150;
                            }
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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

        public void clearLabelText(Label c)
        {
            c.Invoke(new Action(delegate ()
            {
                c.Text = "";
            }));
        }
        public void clearBills()
        {
            clearLabelText(txtChange);
            clearLabelText(txtDelFee);
            clearLabelText(txtDiscountAmount);
            clearLabelText(txtGrossPrice);
            clearLabelText(txtlAmountPayable);
            clearLabelText(txtTenderAmount);
            clearLabelText(txtTotalPayment);
        }
    }
}
