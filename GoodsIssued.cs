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
using DevExpress.Utils.Menu;

namespace AB
{
    public partial class GoodsIssued : Form
    {
        public GoodsIssued(string docStatus)
        {
            InitializeComponent();
            this.docStatus = docStatus;
        }
        string docStatus = "";
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        DataTable dtWarehouse = new DataTable(), dtBranch = new DataTable();
        public static DevExpress.Utils.VisualEffects.AdornerUIManager adornerUIManager1 = new DevExpress.Utils.VisualEffects.AdornerUIManager();
        private void IssueForProduction2_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            checkToDate.Checked = true;
            cmbFromTime.SelectedIndex = 0;
            cmbToTime.SelectedIndex = cmbToTime.Properties.Items.Count - 1;
            dtFromDate.EditValue = DateTime.Now;
            dtToDate.EditValue = DateTime.Now;
            adornerUIManager1.Owner = this;
            loadBranch();
            bg();
        }

        public void showBadge()
        {
            GridColumn col1 = gridView1.Columns["view_comments"];
            if (col1 != null)
            {
                adornerUIManager1.Elements.Clear();

                try
                {
                    for (int i = 0; i <= gridView1.RowCount; i++)
                    {

                        string refff = gridView1.GetRowCellValue(i, "reference") == null ? "" : gridView1.GetRowCellValue(i, "reference").ToString();
                        if (!string.IsNullOrEmpty(refff.Trim()))
                        {

                            DevExpress.Utils.VisualEffects.Badge bad = new DevExpress.Utils.VisualEffects.Badge();

                            int commentCount = 0, intTemp = 0;



                            commentCount = int.TryParse(gridView1.GetRowCellValue(i, "comment_count").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(i, "comment_count").ToString()) : intTemp;


                            //string hello = string.Format("",commentCount);

                            bad.Properties.Text = commentCount.ToString();
                            bad.TargetElement = gridControl1;
                            bad.Appearance.BackColor = Color.Red;
                            bad.Visible = true;


                            GridViewInfo viewInfo = gridView1.GetViewInfo() as GridViewInfo;
                            GridCellInfo info = viewInfo.GetGridCellInfo(i, col1);
                            if (info != null)
                            {
                                Rectangle cellBounds = info.CellValueRect;
                                bad.Properties.Offset = new Point(cellBounds.X + 50, cellBounds.Y + 9);

                                if (i != GetLastVisibleRowHandle(gridView1))
                                {
                                    adornerUIManager1.Elements.Add(bad);
                                }
                                else if ((gridView1.RowCount - 1) == GetLastVisibleRowHandle(gridView1))
                                {
                                    adornerUIManager1.Elements.Add(bad);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
        }

        private int GetLastVisibleRowHandle(GridView view)
        {
            GridViewInfo viewInfo = view.GetViewInfo() as GridViewInfo;
            return viewInfo.RowsInfo.Last().RowHandle;
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

        private void button1_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg();
        }

        public void loadData()
        {
            gridControl1.Invoke(new Action(delegate ()
            {
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
            }));
            bool cFromDate = false, cToDate = false, cFromTime = false, cToTime = false;
            string sBranch = "?branch=", sFromDate = "&from_date=", sToDate = "&to_date=", sFromTime = "&from_time=", sToTime = "&to_time=", sWhse = "&whsecode=", sDocStatus = "&docstatus=" + docStatus;
            checkFromDate.Invoke(new Action(delegate ()
            {
                cFromDate = checkFromDate.Checked;
            }));
            checkToDate.Invoke(new Action(delegate ()
            {
                cToDate = checkToDate.Checked;
            }));
            checkFromTime.Invoke(new Action(delegate ()
            {
                cFromTime = checkFromTime.Checked;
            }));
            checkToTime.Invoke(new Action(delegate ()
            {
                cToTime = checkToTime.Checked;
            }));
            dtFromDate.Invoke(new Action(delegate ()
            {
                sFromDate += cFromDate ? dtFromDate.Text : "";
            }));
            dtToDate.Invoke(new Action(delegate ()
            {
                sToDate += cToDate ? dtToDate.Text : "";
            }));
            cmbFromTime.Invoke(new Action(delegate ()
            {
                sFromTime += cFromTime ? cmbFromTime.Text : "";
            }));
            cmbToTime.Invoke(new Action(delegate ()
            {
                sToTime += cToTime ? cmbToTime.Text : "";
            }));
            cmbWarehouse.Invoke(new Action(delegate ()
            {
                sWhse += apic.findValueInDataTable(dtWarehouse, cmbWarehouse.Text, "whsename", "whsecode");
            }));
            string sParams = sBranch + sFromDate + sToDate + sFromTime + sToTime + sWhse + sDocStatus;
            string sResult = apic.loadData("/api/production/issue_for_prod/get_all", sParams, "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
            {
                JObject joResponse = JObject.Parse(sResult);
                JArray jaData = (JArray)joResponse["data"];
                DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                AutoCompleteStringCollection auto = new AutoCompleteStringCollection();

                if (IsHandleCreated)
                {
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        if (dtData.Rows.Count > 0)
                        {
                            dtData.Columns.Add("view_comments");
                            string[] columnVisible = new string[]
                            {
                            "transdate", "reference","docstatus", "sap_number","remarks","confirm","confirmed_by","date_confirmed","prod_order_ref","view_comments"
                            };
                            dtData.SetColumnsOrder(columnVisible);
                        }

                        gridControl1.DataSource = null;


                        DataTable dtCloned = new DataTable(), dtFinal = new DataTable();
                        if (dtData.Rows.Count > 0)
                        {
                            dtCloned = dtData.Clone();
                            dtCloned.Columns["confirm"].DataType = typeof(string);
                            foreach (DataRow row in dtData.Rows)
                            {

                                dtCloned.ImportRow(row);
                            }
                            dtFinal = dtCloned.Clone();
                            foreach (DataRow row in dtCloned.Rows)
                            {

                                bool isConfirm = false, boolTemp = false;
                                isConfirm = bool.TryParse(row["confirm"].ToString(), out boolTemp) ? Convert.ToBoolean(row["confirm"].ToString()) : boolTemp;
                                row["confirm"] = isConfirm ? "✔" : "";

                                string encodeStatus = row["docstatus"].ToString() == "O" ? "Open" : row["docstatus"].ToString() == "C" ? "Closed" : row["docstatus"].ToString() == "N" ? "Cancelled" : "";
                                row["docstatus"] = encodeStatus;

                                dtFinal.ImportRow(row);
                            }
                        }

                        gridControl1.DataSource = dtFinal;


                        gridView1.OptionsView.ColumnAutoWidth = false;
                        gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;

                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            string v = col.GetCaption();
                            string s = col.GetCaption().Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.ColumnEdit = fieldName.Equals("remarks") ? repositoryItemMemoEdit1 : fieldName.Equals("view_comments") ? repositoryItemButtonEdit1 : fieldName.Equals("reference") || fieldName.Equals("prod_order_ref") ? repositoryItemTextEdit1 : repositoryItemTextEdit2;
                            col.DisplayFormat.FormatType = fieldName.Equals("transdate") || fieldName.Equals("date_confirmed") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;

                            col.DisplayFormat.FormatString = fieldName.Equals("transdate") || fieldName.Equals("date_confirmed") ? "yyyy-MM-dd HH:mm:ss" : "";

                            col.Visible = !(fieldName.Equals("id") || fieldName.Equals("objtype") || fieldName.Equals("seriescode") || fieldName.Equals("transnumber") || fieldName.Equals("prod_order_id") || fieldName.Equals("created_by") || fieldName.Equals("updated_by") || fieldName.Equals("date_created") || fieldName.Equals("date_updated") || fieldName.Equals("hashed_id") || fieldName.Equals("series") || fieldName.Equals("date_canceled") || fieldName.Equals("canceled_by") || fieldName.Equals("comment_count"));

                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }
                        gridView1.BestFitColumns();
                        var col2 = gridView1.Columns["remarks"];
                        if (col2 != null)
                        {
                            col2.Width = 200;
                        }
                        //auto complete
                        string[] suggestions = { "reference" };
                        string suggestConcat = string.Join(";", suggestions);
                        gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                        devc.loadSuggestion(gridView1, gridControl1, suggestions);
                        showBadge();
                    }));
                }
            }
        }

        public void loadBranch()
        {
            cmbBranch.Properties.Items.Clear();
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

        public void loadWarehouse()
        {
            string branchCode = apic.findValueInDataTable(dtBranch, cmbBranch.Text, "name", "code");
            cmbWarehouse.Properties.Items.Clear();
            cmbWarehouse.Properties.Items.Add("All");
            string sResult = apic.loadData("/api/whse/get_all", "?branch=" + branchCode, "", "", RestSharp.Method.GET, true);
            if (!string.IsNullOrEmpty(sResult.Trim()))
            {
                if (sResult.StartsWith("{"))
                {
                    JObject joResult = JObject.Parse(sResult);
                    JArray jaData = (JArray)joResult["data"];
                     dtWarehouse = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
                    foreach (DataRow row in dtWarehouse.Rows)
                    {
                        cmbWarehouse.Properties.Items.Add(row["whsename"].ToString());
                    }
                    string whseCode = Login.jsonResult["data"]["whse"].ToString();
                    cmbWarehouse.SelectedIndex = cmbWarehouse.Properties.Items.IndexOf(whseCode) <= 0 ? 0 : cmbWarehouse.Properties.Items.IndexOf(whseCode);
                }
            }
            else
            {
                cmbWarehouse.SelectedIndex = 0;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            int id = 0, baseID = 0, intTemp = 0;
            id = gridView1.GetFocusedRowCellValue("id") == null ? 0 : int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;

            int prodOrderID = gridView1.GetFocusedRowCellValue("prod_order_id")== null ? 0: int.TryParse(gridView1.GetFocusedRowCellValue("prod_order_id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("prod_order_id").ToString()) : intTemp;

            string selectedReference= gridView1.GetFocusedRowCellValue("reference").ToString();
            string selectedProdOrderReference = gridView1.GetFocusedRowCellValue("prod_order_ref").ToString();
            //baseID = int.TryParse(gridView1.GetFocusedRowCellValue("base_id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("base_id").ToString()) : intTemp;
            if (selectedColumnfieldName.Equals("reference"))
            {
                GoodsIssued_Details.isSubmit = false;
                GoodsIssued_Details frm = new GoodsIssued_Details(id,docStatus, selectedReference);
                frm.ShowDialog();
                if (GoodsIssued_Details.isSubmit)
                {
                    bg();
                }
            }
            else if (selectedColumnfieldName.Equals("prod_order_ref"))
            {
                ProductionOrder_Details frm = new ProductionOrder_Details(prodOrderID, selectedProdOrderReference);
                frm.ShowDialog();
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            if (selectedColumnfieldName.Equals("view_comments"))
            {
                int id = 0, baseID = 0, intTemp = 0;
                id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
                string reference = gridView1.GetFocusedRowCellValue("reference").ToString();
                GoodsIssued_AddComment.isSubmit = false;
                GoodsIssued_Comments frm = new GoodsIssued_Comments(id, reference);
                frm.ShowDialog();
                if (GoodsIssued_AddComment.isSubmit)
                {
                    bg();
                }
            }
        }

        private void gridView1_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            if (selectedColumnfieldName.Equals("reference") && docStatus.Equals("O"))
            {
                if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row && (docStatus.Equals("O")))
                {
                    DXMenuItem item = new DXMenuItem("Cancel this Transaction");
                    item.Click += (o, args) =>
                    {
                        int intTemp = 0;
                        int id = gridView1.GetFocusedRowCellValue("id") == null ? 0 : Int32.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id")) : intTemp;

                        string reference = gridView1.GetFocusedRowCellValue("reference").ToString();
                        Remarks.isSubmit = false;
                        Remarks.rem = "";
                        Remarks remarkss = new Remarks();
                        remarkss.Text = "Cancel - " + reference;
                        remarkss.ShowDialog();
                        if (Remarks.isSubmit)
                        {
                            string remarks = Remarks.rem;
                            try
                            {
                                JObject joBody = new JObject();
                                joBody.Add("remarks", remarks);
                                string sResult = apic.loadData("/api/production/issue_for_prod/cancel/", id.ToString(), "application/json", joBody.ToString(), Method.PUT, true);
                                if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
                                {
                                    JObject jObjectResponse = JObject.Parse(sResult);
                                    string msg = jObjectResponse["message"] == null ? "" : jObjectResponse["message"].ToString();
                                    bool boolTemp = false;
                                    bool isSubmit = jObjectResponse["success"] == null ? false : bool.TryParse(jObjectResponse["success"].ToString(), out boolTemp) ? Convert.ToBoolean(jObjectResponse["success"].ToString()) : boolTemp;
                                    MessageBox.Show(msg, isSubmit ? "Message" : "Validation", MessageBoxButtons.OK, isSubmit ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                                    if (isSubmit)
                                    {
                                        isSubmit = true;
                                        bg();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    };
                    e.Menu.Items.Add(item);
                }
            }
        }

        private void gridView1_TopRowChanged(object sender, EventArgs e)
        {
            showBadge();
        }

        private void gridView1_LeftCoordChanged(object sender, EventArgs e)
        {
            showBadge();
        }

        private void cmbBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadWarehouse();
        }
    }
}
