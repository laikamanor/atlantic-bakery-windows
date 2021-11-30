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
using AB.API_Class.SOA;
namespace AB
{
    public partial class ForSOA2 : Form
    {
        public ForSOA2()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        ui_class uic = new ui_class();
        devexpress_class devc = new devexpress_class();
        soa_class soac = new soa_class();
        DataTable dtCustType = new DataTable(), dtBranch = new DataTable();
        private void ForSOA2_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            dtFromDate.EditValue = DateTime.Now;
            dtToDate.EditValue = DateTime.Now;
            loadCustType();
            loadBranch();
            bg();
        }

        public void loadBranch()
        {
            try
            {
                string plantCode = "";
                cmbBranch.Invoke(new Action(delegate ()
                {
                    cmbBranch.Properties.Items.Clear();
                }));
                string sResult = "";
                sResult = apic.loadData("/api/branch/get_all", "", "", "", Method.GET, true);
                if (sResult.Substring(0, 1).Equals("{"))
                {
                    dtBranch = apic.getDtDownloadResources(sResult, "data");
                    if (IsHandleCreated)
                    {
                        cmbBranch.Invoke(new Action(delegate ()
                        {
                            cmbBranch.Properties.Items.Add("All");
                        }));
                    }
                    foreach (DataRow row in dtBranch.Rows)
                    {
                        if (IsHandleCreated)
                        {
                            cmbBranch.Invoke(new Action(delegate ()
                            {
                                cmbBranch.Properties.Items.Add(row["name"].ToString());
                            }));
                        }

                    }
                    if (IsHandleCreated)
                    {
                        cmbBranch.Invoke(new Action(delegate ()
                        {
                            string branch = (string)Login.jsonResult["data"]["branch"];
                            string s = apic.findValueInDataTable(dtBranch, branch, "code", "name");
                            cmbBranch.SelectedIndex = cmbBranch.Properties.Items.IndexOf(s) <= 0 ? 0 : cmbBranch.Properties.Items.IndexOf(s);
                        }));
                    }
                }
                else
                {
                    apic.showCustomMsgBox("Validation", sResult);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void loadCustType()
        {
            try
            {
                string plantCode = "";
                cmbCustomerType.Invoke(new Action(delegate ()
                {
                    cmbCustomerType.Properties.Items.Clear();
                }));
                string sResult = "";
                sResult = apic.loadData("/api/custtype/get_all", "", "", "", Method.GET, true);
                if (sResult.Substring(0, 1).Equals("{"))
                {
                    dtCustType = apic.getDtDownloadResources(sResult, "data");
                    if (IsHandleCreated)
                    {
                        cmbCustomerType.Invoke(new Action(delegate ()
                        {
                            cmbCustomerType.Properties.Items.Add("All");
                        }));
                    }
                    foreach (DataRow row in dtCustType.Rows)
                    {
                        if (IsHandleCreated)
                        {
                            cmbCustomerType.Invoke(new Action(delegate ()
                            {
                                cmbCustomerType.Properties.Items.Add(row["name"].ToString());
                            }));
                        }

                    }
                    if (IsHandleCreated)
                    {
                        cmbCustomerType.Invoke(new Action(delegate ()
                        {
                            cmbCustomerType.SelectedIndex = 0;
                        }));
                    }
                }
                else
                {
                    apic.showCustomMsgBox("Validation", sResult);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void loadData()
        {
            gridControl1.Invoke(new Action(delegate ()
            {
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
            }));
            bool cFromDate = false, cToDate = false;
            string sCustCode = "?cust_type=",sBranch="&branch=", sFromDate = "&from_date=", sToDate = "&to_date=", sDocStatus = "&docstatus=";
            checkFromDate.Invoke(new Action(delegate ()
            {
                cFromDate = checkFromDate.Checked;
            }));
            checkToDate.Invoke(new Action(delegate ()
            {
                cToDate = checkToDate.Checked;
            }));
            cmbCustomerType.Invoke(new Action(delegate ()
            {
                string custID = apic.findValueInDataTable(dtCustType, cmbCustomerType.Text, "name", "id");
                sCustCode += custID;
            }));
            cmbBranch.Invoke(new Action(delegate ()
            {
                string branchCode = apic.findValueInDataTable(dtBranch, cmbBranch.Text, "name", "code");
                sBranch += branchCode;
            }));
            dtFromDate.Invoke(new Action(delegate ()
            {
                sFromDate += cFromDate ? dtFromDate.Text : "";
            }));
            dtToDate.Invoke(new Action(delegate ()
            {
                sToDate += cToDate ? dtToDate.Text : "";
            }));
            string sParams = sCustCode + sBranch + sFromDate + sToDate + sDocStatus;
            string sResult = apic.loadData("/soa/get_for_soa", sParams, "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
            {
                JObject joResponse = JObject.Parse(sResult);
                JArray jaData = (JArray)joResponse["data"];
                DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                if(dtData.Rows.Count > 0)
                {
                    foreach(DataRow row in dtData.Rows)
                    {
                        string decodeStatus = row["docstatus"].ToString() == "O" ? "Open" : row["docstatus"].ToString() == "C" ? "Closed" : row["docstatus"].ToString() == "N" ? "Cancelled" : "";
                        row["docstatus"] = decodeStatus;
                    }
                }
                if (IsHandleCreated)
                {
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        if (dtData.Rows.Count > 0)
                        {
                            string[] columnVisible = new string[]
{
                            "transdate", "reference", "cust_code","doctotal", "docstatus","remarks"
};
                            dtData.SetColumnsOrder(columnVisible);
                        }
                        gridControl1.DataSource = null;
                        gridControl1.DataSource = dtData;

                        gridView1.OptionsView.ColumnAutoWidth = false;
                        gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            string v = col.GetCaption();
                            string s = col.GetCaption().Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.ColumnEdit = fieldName.Equals("remarks") ? repositoryItemMemoEdit1 : repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("doctotal")? DevExpress.Utils.FormatType.Numeric : fieldName.Equals("transdate") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;
                            col.DisplayFormat.FormatString = fieldName.Equals("doctotal") ? "n2" : fieldName.Equals("transdate") ? "yyyy-MM-dd HH:mm:ss" : "";
                            col.Visible = fieldName.Equals("transdate") || fieldName.Equals("reference") || fieldName.Equals("cust_code") || fieldName.Equals("doctotal") || fieldName.Equals("docstatus") || fieldName.Equals("remarks");

                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }

                        //auto complete
                        string[] suggestions = { "reference", "cust_code" };
                        string suggestConcat = string.Join(";", suggestions);
                        gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                        devc.loadSuggestion(gridView1, gridControl1, suggestions);

                        var refCol = gridView1.Columns["reference"];
                        if (refCol != null)
                        {
                            gridView1.Columns["reference"].Summary.Clear();
                            gridView1.Columns["reference"].Summary.Add(DevExpress.Data.SummaryItemType.Count, "reference", "Count: {0:N0}");
                        }
                        //var balCol = gridView1.Columns["doctotal"];
                        //if (balCol != null)
                        //{
                        //    gridView1.Columns["doctotal"].Summary.Clear();
                        //    gridView1.Columns["doctotal"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "doctotal", "Total Amount: {0:n2}");
                        //}

                        gridView1.BestFitColumns();
                        var col2 = gridView1.Columns["remarks"];
                        if (col2 != null)
                        {
                            col2.Width = 200;
                        }
                    }));
                }
            }
        }

        public void bg()
        {
            if (!backgroundWorker1.IsBusy)
            {
                closeForm();
                Loading frm = new Loading();
                frm.Show();
                backgroundWorker1.RunWorkerAsync();
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

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void gridView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            try
            {
                double docTotal = 0.00, doubleTemp = 0.00;
                int[] rows = gridView1.GetSelectedRows();
                foreach (int row in rows)
                {
                    var docTotalCol = gridView1.Columns["doctotal"];
                    if (docTotalCol != null)
                    {
                        docTotal += double.TryParse(gridView1.GetRowCellValue(row, "doctotal").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(row, "doctotal").ToString()) : doubleTemp;
                    }
                }
                lblTotalSelected.Text = "Total Amount: " + docTotal.ToString("n2");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnCreateSOA_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to create SOA?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    gridView1.BeginSort();
                    int[] rows = gridView1.GetSelectedRows();
                    gridView1.EndSort();
                    DateTime dtTemp = new DateTime();
                    int haveDifferentCustomerCode = 0, haveFirstCustomerCode = 0, intTemp = 0;
                    JObject joBody = new JObject();
                    string custCode = "";
                    JObject joHeader = new JObject();
                    JArray jaRows = new JArray();
                    foreach (int row in rows)
                    {
                        if (haveFirstCustomerCode <= 0)
                        {
                            haveFirstCustomerCode++;
                            custCode = gridView1.GetRowCellValue(row, "cust_code").ToString();
                        }
                        if (custCode != gridView1.GetRowCellValue(row, "cust_code").ToString())
                        {
                            haveDifferentCustomerCode++;
                        }
                        JObject joRows = new JObject();
                        joRows.Add("base_id", int.TryParse(gridView1.GetRowCellValue(row, "id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(row, "id").ToString()) : 0);

                        DateTime dtBaseTransdate = DateTime.TryParse(gridView1.GetRowCellValue(row, "transdate").ToString(), out dtTemp) ? Convert.ToDateTime(gridView1.GetRowCellValue(row, "transdate").ToString()) : dtTemp;
                        string sBaseTransdate = dtBaseTransdate.Equals(DateTime.MinValue) ? "" : dtBaseTransdate.ToString("yyyy-MM-dd HH:mm:ss");
                        joRows.Add("base_transdate", string.IsNullOrEmpty(sBaseTransdate.Trim()) ? null : sBaseTransdate);
                        joRows.Add("base_reference", gridView1.GetRowCellValue(row, "reference").ToString());
                        joRows.Add("base_objtype", int.TryParse(gridView1.GetRowCellValue(row, "objtype").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(row, "objtype").ToString()) : 0);
                        joRows.Add("sales_remarks", gridView1.GetRowCellValue(row, "remarks").ToString());
                        joRows.Add("amount", Convert.ToDouble(gridView1.GetRowCellValue(row, "doctotal").ToString()));
                        jaRows.Add(joRows);
                    }
                    Console.WriteLine(jaRows);
                    if (haveDifferentCustomerCode > 0)
                    {
                        MessageBox.Show("You can't create SOA with different customer!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {

                        joHeader.Add("transdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        joHeader.Add("cust_code", custCode);
                        joBody.Add("header", joHeader);
                        joBody.Add("rows", jaRows);
                        string response = soac.createSOA(joBody);
                        if (response.Substring(0, 1).Equals("{"))
                        {
                            DataTable dt = new DataTable();
                            dt.Columns.Add("reference");
                            dt.Columns.Add("transdate");
                            dt.Columns.Add("cust_code");
                            dt.Columns.Add("docstatus");
                            dt.Columns.Add("total_amount");
                            dt.Columns.Add("id");
                            dt.Columns.Add("doc_id");
                            dt.Columns.Add("base_transdate");
                            dt.Columns.Add("base_id");
                            dt.Columns.Add("base_reference");
                            dt.Columns.Add("base_objtype");
                            dt.Columns.Add("sales_remarks");
                            dt.Columns.Add("amount");

                            JObject joResponse = JObject.Parse(response);
                            bool isSuccess = false;
                            string msg = "", reference = "", customerCode = "", docStatus = "";
                            DateTime dtTransDate = new DateTime();
                            double total = 0.00, doubleTemp = 0.00;
                            foreach (var q in joResponse)
                            {
                                if (q.Key.Equals("success"))
                                {
                                    isSuccess = Convert.ToBoolean(q.Value.ToString());
                                }
                                else if (q.Key.Equals("message"))
                                {
                                    msg = q.Value.ToString();
                                }
                                else if (q.Key.Equals("data"))
                                {
                                    JObject joData = JObject.Parse(q.Value.ToString());
                                    foreach (var x in joData)
                                    {
                                        if (x.Key.Equals("reference"))
                                        {
                                            reference = x.Value.ToString();
                                        }
                                        else if (x.Key.Equals("transdate"))
                                        {
                                            string replaceT = x.Value.ToString().Replace("T", "");
                                            dtTransDate = string.IsNullOrEmpty(replaceT) ? new DateTime() : Convert.ToDateTime(replaceT);
                                        }
                                        else if (x.Key.Equals("cust_code"))
                                        {
                                            customerCode = x.Value.ToString();
                                        }
                                        else if (x.Key.Equals("total_amount"))
                                        {
                                            total = Convert.ToDouble(x.Value.ToString());
                                        }
                                        else if (x.Key.Equals("docstatus"))
                                        {
                                            docStatus = q.Value.ToString().Equals("O") ? "Open" : q.Value.ToString().Equals("C") ? "Closed" : q.Value.ToString().Equals("N") ? "Cancelled" : "";
                                        }
                                        else if (x.Key.Equals("soa_rows"))
                                        {
                                            if (x.Value.ToString() != "[]")
                                            {
                                                JArray jsonArray = JArray.Parse(x.Value.ToString());
                                                for (int i = 0; i < jsonArray.Count(); i++)
                                                {
                                                    int soaID = 0, soaDocID = 0, soaBaseID = 0, soaBaseObjType = 0;
                                                    double soaAmount = 0.00;
                                                    string soaBaseReference = "", soaSalesRemarks = "";
                                                    DateTime soaBaseTransDate = new DateTime();
                                                    JObject joSoaRows = JObject.Parse(jsonArray[i].ToString());
                                                    foreach (var y in joSoaRows)
                                                    {
                                                        if (y.Key.Equals("id"))
                                                        {
                                                            soaID = Convert.ToInt32(y.Value.ToString());
                                                        }
                                                        else if (y.Key.Equals("doc_id"))
                                                        {
                                                            soaDocID = Convert.ToInt32(y.Value.ToString());
                                                        }

                                                        else if (y.Key.Equals("base_transdate"))
                                                        {
                                                            string replaceT = y.Value.ToString().Replace("T", "");
                                                            soaBaseTransDate = !string.IsNullOrEmpty(y.Value.ToString()) ? Convert.ToDateTime(replaceT) : new DateTime();
                                                        }
                                                        else if (y.Key.Equals("base_id"))
                                                        {
                                                            soaBaseID = Convert.ToInt32(y.Value.ToString());
                                                        }
                                                        else if (y.Key.Equals("base_reference"))
                                                        {
                                                            soaBaseReference = y.Value.ToString();
                                                        }
                                                        else if (y.Key.Equals("base_objtype"))
                                                        {
                                                            soaBaseObjType = Convert.ToInt32(y.Value.ToString());
                                                        }
                                                        else if (y.Key.Equals("sales_remarks"))
                                                        {
                                                            soaSalesRemarks = y.Value.ToString();
                                                        }
                                                        else if (y.Key.Equals("amount"))
                                                        {
                                                            soaAmount = Convert.ToDouble(y.Value.ToString());
                                                        }
                                                    }

                                                    dt.Rows.Add(reference, dtTransDate.ToString("MM/dd/yyyy"), customerCode, docStatus, total.ToString("n2"), soaID, soaDocID, soaBaseTransDate.ToString("MM/dd/yyyy"), soaBaseID, soaBaseReference, soaBaseObjType, soaSalesRemarks, soaAmount.ToString("n2"));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            MessageBox.Show(msg, isSuccess ? "Message" : "Validation", MessageBoxButtons.OK, isSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                            bg();
                            if (isSuccess)
                            {
                                DialogResult dialogResult1 = MessageBox.Show("Do you want to print the soa?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (dialogResult1 == DialogResult.Yes)
                                {
                                    printSOA frm = new printSOA();
                                    frm.dtResult = dt;
                                    frm.ShowDialog();
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(response, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bg();
        }
    }
}
