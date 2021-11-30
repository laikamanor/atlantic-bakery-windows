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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
namespace AB
{
    public partial class ProductionOrder : Form
    {
        public ProductionOrder(string docStatus)
        {
            InitializeComponent();
            this.docStatus = docStatus;
        }
        string docStatus = "";
        DataTable dtBranch = new DataTable(), dtWarehouse = new DataTable();
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        ui_class uic = new ui_class();
        utility_class utilityc = new utility_class();
        private void ProductionOrder_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            dtFromDate.EditValue = DateTime.Now;
            dtToDate.EditValue = DateTime.Now;
            cmbFromTime.SelectedIndex = 0;
            cmbToTime.SelectedIndex = cmbToTime.Properties.Items.Count - 1;
            loadBranches();
            bg(backgroundWorker1);
        }

        public void bg(BackgroundWorker bg1)
        {
            if (!bg1.IsBusy)
            {
                closeForm();
                Loading frm = new Loading();
                frm.Show();
                bg1.RunWorkerAsync();
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

        public void loadData()
        {
            string sBranch = "?branch=" + apic.findValueInDataTable(dtBranch, uic.delegateControl(cmbBranch), "name", "code");
            string sWhse = "&whsecode=" + apic.findValueInDataTable(dtWarehouse, uic.delegateControl(cmbWhse), "whsename", "whsecode");
            string sDocStatus = "&docstatus=" + docStatus;
            bool cCheckFromDate = false, cCheckToDate = false;
            checkFromDate.Invoke(new Action(delegate ()
            {
                cCheckFromDate = checkFromDate.Checked;
            }));
            checkToDate.Invoke(new Action(delegate ()
            {
                cCheckToDate = checkToDate.Checked;
            }));
            string sFromDate = "&from_date=" + (cCheckFromDate ? uic.delegateControl(dtFromDate) : "");
            string sToDate = "&to_date=" + (cCheckToDate ? uic.delegateControl(dtToDate) : "");
            string sFromTime = "&from_time=" + uic.delegateControl(cmbFromTime);
            string sToTime = "&to_time=" + uic.delegateControl(cmbToTime);

            string sParams = sBranch + sWhse + sDocStatus + sToDate  + sFromDate + sToDate + sFromTime + sToTime;
            string sResult = apic.loadData("/api/production/order/get_all", sParams, "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult.Trim()))
            {
                if (sResult.Substring(0, 1).Equals("{"))
                {
                    JObject joResponse = JObject.Parse(sResult);
                    JArray jaData = (JArray)joResponse["data"];
                    DataTable dt = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));

                    if (dt.Rows.Count > 0)
                    {
                        if (docStatus.Equals("O"))
                        {
                            dt.Columns.Add("close");
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            string rDocStatus = row["docstatus"].ToString();
                            string decodeStatus = rDocStatus.Equals("O") ? "Open" : rDocStatus.Equals("C") ? "Closed" : rDocStatus.Equals("N") ? "Cancelled" : "";
                            row["docstatus"] = decodeStatus;
                        }
                    }
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        gridControl1.DataSource = null;

                        dt.SetColumnsOrder("transdate", "reference", "production_date","sap_number", "docstatus", "production_status", "remarks","close");

                        gridControl1.DataSource = dt;
                        gridView1.OptionsView.ColumnAutoWidth = false;
                        gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            string v = col.GetCaption();
                            string s = v.Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.ColumnEdit = fieldName.Equals("remarks") ? repositoryItemMemoEdit1 : fieldName.Equals("close") && docStatus.Equals("O") ? repositoryItemButtonEdit1 : fieldName.Equals("reference") || fieldName.Equals("production_status") ? repositoryItemTextEdit1 : repositoryItemTextEdit2;

                            col.DisplayFormat.FormatType =  fieldName.Equals("transdate") || fieldName.Equals("production_date") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;

                            //col.Fixed = fieldName.Equals("transdate") || fieldName.Equals("reference") ? FixedStyle.Left : FixedStyle.None;


                            col.DisplayFormat.FormatString = fieldName.Equals("transdate") || fieldName.Equals("production_date") ? "yyyy-MM-dd HH:mm:ss" : "";

                            //col.Visible = fieldName.Equals("reference") || fieldName.Equals("balance_due") || fieldName.Equals("amount_due") || fieldName.Equals("cust_code") || fieldName.Equals("tenderamt") || fieldName.Equals("disctype") || fieldName.Equals("disc_amount") || fieldName.Equals("delfee") || fieldName.Equals("remarks") || fieldName.Equals("user") || fieldName.Equals("days_due") || fieldName.Equals("transdate") || fieldName.Equals("edit_amount_payable");

                            col.Visible = !(fieldName.Equals("id") || fieldName.Equals("issued") || fieldName.Equals("issue_for_prod") || fieldName.Equals("receipt_from_prod") || fieldName.Equals("docstatus") || fieldName.Equals("balance_count"));

                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }
                        //auto complete
                        string[] suggestions = { "reference" };
                        string suggestConcat = string.Join(";", suggestions);
                        gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                        devc.loadSuggestion(gridView1, gridControl1, suggestions);
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

        public void loadBranches()
        {
            try
            {
                cmbBranch.Invoke(new Action(delegate ()
                {
                    cmbBranch.Properties.Items.Clear();
                }));
                if (apic.haveAccess())
                {
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
                else
                {
                    if (IsHandleCreated)
                    {
                        cmbBranch.Invoke(new Action(delegate ()
                        {
                            cmbBranch.Properties.Items.Add(Login.jsonResult["data"]["branch"]);
                            cmbBranch.SelectedIndex = 0;
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cmbBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadWarehouse();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg(backgroundWorker1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bg(backgroundWorker1);
        }

        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            int id = 0, baseID = 0, intTemp = 0;
            id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
            string reference = gridView1.GetFocusedRowCellValue("reference").ToString();
            if (selectedColumnfieldName.Equals("reference"))
            {
                ProductionOrder_Details frm = new ProductionOrder_Details(id, reference);
                frm.ShowDialog();
                //Production_ProductionOrder_Items items = new Production_ProductionOrder_Items();
                //items.referenceNumber = reference;
                //items.selectedID = id;
                //items.ShowDialog();
            }
            else if (selectedColumnfieldName.Equals("production_status"))
            {
                ProductionOrder_History frm = new ProductionOrder_History(id);
                frm.ShowDialog();
            }
        }

        private void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            if (selectedColumnfieldName.Equals("close"))
            {
                int id = 0, baseID = 0, intTemp = 0;
                id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
                string reference = gridView1.GetFocusedRowCellValue("reference").ToString();
                string docStatus = gridView1.GetFocusedRowCellValue("docstatus").ToString();
                if (docStatus.Equals("Open"))
                {
                    Remarks remarks = new Remarks();
                    remarks.ShowDialog();
                    if (Remarks.isSubmit)
                    {
                        string rem = Remarks.rem;
                        JObject joBody = new JObject();
                        joBody.Add("remarks", rem);
                        int selectediD = id;
                        string URL = "/api/production/order/close/" + selectediD;
                        apiPUT(joBody, URL);
                        bg(backgroundWorker1);
                    }
                }
                else
                {
                    MessageBox.Show(reference + " is already " + docStatus + "!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                    Console.WriteLine(body);
                    request.AddParameter("application/json", body, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    if (response.ErrorMessage == null)
                    {
                        if (response.Content.Substring(0, 1).Equals("{"))
                        {
                            JObject jObjectResponse = JObject.Parse(response.Content);
                            bool isSubmit = false;
                            foreach (var x in jObjectResponse)
                            {
                                if (x.Key.Equals("success"))
                                {
                                    isSubmit = string.IsNullOrEmpty(x.Value.ToString()) ? false : Convert.ToBoolean(x.Value.ToString());
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
                            MessageBox.Show(msg, "", MessageBoxButtons.OK, isSubmit ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show(response.Content.ToString(), "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show(response.ErrorMessage, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }
            }
        }

        private void btnSelectStatus_Click(object sender, EventArgs e)
        {
            if (gridView1.Columns["production_status"] != null)
            {
                gridView1.ShowFilterPopup(gridView1.Columns["production_status"]);
            }
        }
        private int hotTrackRow = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        private int HotTrackRow
        {
            get
            {
                return hotTrackRow;
            }
            set
            {
                if (hotTrackRow != value)
                {
                    int prevHotTrackRow = hotTrackRow;
                    hotTrackRow = value;
                    gridView1.RefreshRow(prevHotTrackRow);
                    gridView1.RefreshRow(hotTrackRow);

                    if (hotTrackRow >= 0)
                        gridControl1.Cursor = Cursors.Hand;
                    else
                        gridControl1.Cursor = Cursors.Default;
                }
            }
        }
        private void gridView1_MouseMove(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(new Point(e.X, e.Y));

            if (info.InRowCell)
                HotTrackRow = info.RowHandle;
            else
                HotTrackRow = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        }

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.RowHandle == HotTrackRow)
                e.Appearance.BackColor = gridView1.PaintAppearance.SelectedRow.BackColor;
            else
                e.Appearance.BackColor = e.Appearance.BackColor;
        }

        private void gridView1_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (e.RowHandle >= 0)
            {
                var colVar = gridView1.Columns["balance_count"];
                //252, 101, 101
                if (gridView1.Columns.Contains(colVar))
                {
                    double balanceCount = 0.00, doubleTemp = 0.00;
                    balanceCount = gridView1.GetRowCellValue(e.RowHandle,"balance_count") == null ? doubleTemp : double.TryParse(gridView1.GetRowCellValue(e.RowHandle, "balance_count").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(e.RowHandle, "balance_count").ToString()) : doubleTemp;
                    if(balanceCount > 0)
                    {
                        e.Appearance.BackColor = Color.FromArgb(252, 101, 101);
                    }
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
            try
            {
                cmbWhse.Invoke(new Action(delegate ()
                {
                    cmbWhse.Properties.Items.Clear();
                }));
                if (apic.haveAccess())
                {
                    string sBranch = "?branch=" + apic.findValueInDataTable(dtBranch, uic.delegateControl(cmbBranch), "name", "code");
                    string sParams = sBranch;
                    string sResult = "";
                    sResult = apic.loadData("/api/whse/get_all", sParams, "", "", Method.GET, true);
                    if (sResult.Substring(0, 1).Equals("{"))
                    {
                        dtWarehouse = apic.getDtDownloadResources(sResult, "data");
                        if (IsHandleCreated)
                        {
                            cmbWhse.Invoke(new Action(delegate ()
                            {
                                cmbWhse.Properties.Items.Add("All");
                            }));
                        }
                        foreach (DataRow row in dtWarehouse.Rows)
                        {
                            if (IsHandleCreated)
                            {
                                cmbWhse.Invoke(new Action(delegate ()
                                {
                                    cmbWhse.Properties.Items.Add(row["whsename"].ToString());
                                }));
                            }

                        }
                        if (IsHandleCreated)
                        {
                            cmbWhse.Invoke(new Action(delegate ()
                            {
                                string branch = (string)Login.jsonResult["data"]["whse"];
                                string s = apic.findValueInDataTable(dtWarehouse, branch, "whsecode", "whsename");
                                cmbWhse.SelectedIndex = cmbWhse.Properties.Items.IndexOf(s) <= 0 ? 0 : cmbWhse.Properties.Items.IndexOf(s);
                            }));
                        }
                    }
                    else
                    {
                        apic.showCustomMsgBox("Validation", sResult);
                    }
                }
                else
                {
                    if (IsHandleCreated)
                    {
                        cmbWhse.Invoke(new Action(delegate ()
                        {
                            cmbWhse.Properties.Items.Add(Login.jsonResult["data"]["whse"]);
                            cmbWhse.SelectedIndex = 0;
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
