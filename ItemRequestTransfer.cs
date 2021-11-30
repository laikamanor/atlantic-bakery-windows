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
using DevExpress.Utils.Menu;

namespace AB
{
    public partial class ItemRequestTransfer : Form
    {
        public ItemRequestTransfer(string docStatus, string forSap)
        {
            InitializeComponent();
            this.docStatus = docStatus;
            this.forSap = forSap;
        }
        public static DevExpress.Utils.VisualEffects.AdornerUIManager adornerUIManager1 = new DevExpress.Utils.VisualEffects.AdornerUIManager();
        devexpress_class devc = new devexpress_class();
        utility_class utilityc = new utility_class();
        api_class apic = new api_class();
        ui_class uic = new ui_class();
        string docStatus = "", forSap = "";
        DataTable dtBranch = new DataTable(), dtWhse = new DataTable();
        private void TargetForDelivery_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            dtFromDate.EditValue = dtToDate.EditValue = DateTime.Now;
            adornerUIManager1.Owner = this;
            loadWarehouse(cmbFromWhse);
            loadWarehouse(cmbToWhse);
            bg();
        }

        private int GetLastVisibleRowHandle(GridView view)
        {
            GridViewInfo viewInfo = view.GetViewInfo() as GridViewInfo;
            return viewInfo.RowsInfo.Last().RowHandle;
        }

        public void loadData()
        {
            try
            {
                bool cFromDate = false, cToDate = false;
                string sFromDate = "?from_date=", sToDate = "&to_date=", sDocStatus = string.IsNullOrEmpty(docStatus.Trim()) ? "" : "&docstatus=" + docStatus, sFromWhse = "&from_whse=", sToWhse = "&to_whse=";

                cmbFromWhse.Invoke(new Action(delegate ()
                {
                    sFromWhse += apic.findValueInDataTable(dtWhse, cmbFromWhse.Text, "whsename", "whsecode");
                }));

                cmbToWhse.Invoke(new Action(delegate ()
                {
                    sToWhse += apic.findValueInDataTable(dtWhse, cmbToWhse.Text, "whsename", "whsecode");
                }));

                checkFromDate.Invoke(new Action(delegate ()
                {
                    cFromDate = checkFromDate.Checked;
                }));
                checkToDate.Invoke(new Action(delegate ()
                {
                    cToDate = checkToDate.Checked;
                }));
                dtFromDate.Invoke(new Action(delegate ()
                {
                    sFromDate += cFromDate ? dtFromDate.Text : "";
                }));
                dtToDate.Invoke(new Action(delegate ()
                {
                    sToDate += cToDate ? dtToDate.Text : "";
                }));
                string sForSap = !string.IsNullOrEmpty(forSap.Trim()) ? "&sap_num=" + forSap : "";
                string sParams = sFromDate + sToDate + sForSap + sDocStatus + sFromWhse + sToWhse;
                string sResult = apic.loadData("/api/forecast/target_for_delivery/get_all", sParams, "", "", Method.GET, true);
                if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
                {
                    double runningBalance = 0.00;
                    DateTime dtTemp = new DateTime();
                    JObject joResponse = JObject.Parse(sResult);
                    JArray jaData = joResponse["data"] == null ? new JArray() : (JArray)joResponse["data"];
                    //lblToWhse.Text = jaTransRow[0]["to_whse"].ToString();
                    DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));



                    gridControl1.Invoke(new Action(delegate ()
                    {



                        if (dtData.Rows.Count > 0)
                        {
                            dtData.Columns.Add("view_comments");
                        }

                        dtData.SetColumnsOrder("transdate", "reference", "prod_whse", "to_whse", "sap_number", "shift", "docstatus", "delivery_date", "delivery_status", "remarks", "date_closed", "closed_by", "date_canceled", "canceled_by", "remarks2", "view_comments");

                        gridControl1.Invoke(new Action(delegate ()
                        {
                            gridControl1.DataSource = null;
                            gridControl1.DataSource = dtData;

                        //badge1.Properties.Text = "1asdd";
                        //badge1.TargetElement = gridControl1;
                        //badge1.Visible = true;
                    }));

                        gridView1.OptionsView.ColumnAutoWidth = false;
                        gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            string v = col.GetCaption();
                            string s = col.GetCaption().Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.Caption = fieldName.Equals("remarks2") && docStatus.Equals("C") ? "Closed Remarks" : fieldName.Equals("remarks2") && docStatus.Equals("N") ? "Canceled Remarks" : col.Caption;
                            //col.Caption = fieldName.Equals("amount_in") ? "Sales" : fieldName.Equals("amount_out") ? "Payment" : col.Caption;
                            col.ColumnEdit = fieldName.Equals("remarks") || fieldName.Equals("remarks2") ? repositoryItemMemoEdit1 : fieldName.Equals("view_comments") ? repositoryItemButtonEdit1 : fieldName.Equals("reference") ? repositoryItemTextEdit1 : repositoryItemTextEdit2;
                            col.DisplayFormat.FormatType = fieldName.Equals("transdate") || fieldName.Equals("delivery_date") || fieldName.Equals("date_closed") || fieldName.Equals("date_canceled") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;
                            col.DisplayFormat.FormatString = fieldName.Equals("transdate") || fieldName.Equals("delivery_date") || fieldName.Equals("date_closed") || fieldName.Equals("date_canceled") ? "yyyy-MM-dd HH:mm:ss" : "";


                            if (docStatus.Equals("O") || forSap.Equals("0"))
                            {
                                col.Visible = fieldName.Equals("transdate") || fieldName.Equals("reference") || fieldName.Equals("branch") || fieldName.Equals("shift") || fieldName.Equals("docstatus") || fieldName.Equals("delivery_date") || fieldName.Equals("remarks") || fieldName.Equals("prod_whse") || fieldName.Equals("to_whse") || fieldName.Equals("delivery_status") || fieldName.Equals("view_comments");
                            }
                            else if (forSap.Equals("1"))
                            {
                                col.Visible = fieldName.Equals("transdate") || fieldName.Equals("reference") || fieldName.Equals("branch") || fieldName.Equals("shift") || fieldName.Equals("docstatus") || fieldName.Equals("delivery_date") || fieldName.Equals("remarks") || fieldName.Equals("prod_whse") || fieldName.Equals("to_whse") || fieldName.Equals("delivery_status") || fieldName.Equals("view_comments") || fieldName.Equals("sap_number");
                            }
                            else if (docStatus.Equals("C"))
                            {
                                col.Visible = fieldName.Equals("transdate") || fieldName.Equals("reference") || fieldName.Equals("branch") || fieldName.Equals("shift") || fieldName.Equals("docstatus") || fieldName.Equals("delivery_date") || fieldName.Equals("remarks") || fieldName.Equals("closed_by") || fieldName.Equals("date_closed") || fieldName.Equals("remarks2") || fieldName.Equals("prod_whse") || fieldName.Equals("to_whse") || fieldName.Equals("delivery_status") || fieldName.Equals("view_comments");
                            }
                            else if (docStatus.Equals("N"))
                            {
                                col.Visible = fieldName.Equals("transdate") || fieldName.Equals("reference") || fieldName.Equals("branch") || fieldName.Equals("shift") || fieldName.Equals("docstatus") || fieldName.Equals("delivery_date") || fieldName.Equals("remarks") || fieldName.Equals("canceled_by") || fieldName.Equals("date_canceled") || fieldName.Equals("remarks2") || fieldName.Equals("prod_whse") || fieldName.Equals("to_whse") || fieldName.Equals("delivery_status") || fieldName.Equals("view_comments");
                            }

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
                        var col3 = gridView1.Columns["remarks2"];
                        if (col2 != null)
                        {
                            col2.Width = 200;
                        }
                        if (col3 != null)
                        {
                            col3.Width = 200;
                        }
                        showBadge();
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void loadWarehouse(DevExpress.XtraEditors.ComboBoxEdit cmb)
        {
            //string branchCode = cmb.Name.Equals("cmbToWhse") ? "" : Login.jsonResult["data"]["branch"].ToString();
            cmb.Properties.Items.Clear();
            cmb.Properties.Items.Add("All");
            string sResult = apic.loadData("/api/whse/get_all", "", "", "", RestSharp.Method.GET, true);
            if (!string.IsNullOrEmpty(sResult.Trim()))
            {
                if (sResult.StartsWith("{"))
                {
                    JObject joResult = JObject.Parse(sResult);
                    JArray jaData = (JArray)joResult["data"];
                    DataTable dt = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
                    if (cmb.Name.Equals("cmbToWhse"))
                    {
                        dtWhse = dt;
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        cmb.Properties.Items.Add(row["whsename"].ToString());
                    }
                    string plantCode = Login.jsonResult["data"]["whse"].ToString();
                    string plantName = cmb.Name.Equals("cmbToWhse") ? "" : apic.findValueInDataTable(dt, plantCode, "whsecode", "whsename");
                    cmb.SelectedIndex = cmb.Properties.Items.IndexOf(plantName) <= 0 ? 0 : cmb.Properties.Items.IndexOf(plantName);
                }
            }
            else
            {
                cmb.SelectedIndex = 0;
            }
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

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            bg();
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
            if (e.Column.FieldName.Equals("delivery_status"))
            {
                string deliveryStatus = gridView1.GetRowCellValue(e.RowHandle, "delivery_status") == null ? "" : gridView1.GetRowCellValue(e.RowHandle, "delivery_status").ToString();
                if (string.IsNullOrEmpty(deliveryStatus.Trim()) && deliveryStatus.ToLower() == "Unserved")
                {
                    e.Appearance.BackColor = Color.Transparent;
                }
                if (deliveryStatus.ToLower() == "Partially Served".ToLower())
                {
                    e.Appearance.BackColor = Color.FromArgb(252, 101, 101);
                }
                if (deliveryStatus.ToLower() == "Fully Served".ToLower())
                {
                    e.Appearance.BackColor = Color.FromArgb(101, 252, 109);
                }
                if (deliveryStatus.ToLower() == "Manual Closed".ToLower())
                {
                    e.Appearance.BackColor = Color.FromArgb(252, 249, 101);
                }
            }
        }

        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            if (selectedColumnfieldName.Equals("reference"))
            {
                int id = 0, baseID = 0, intTemp = 0;
                id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
                string reference = gridView1.GetFocusedRowCellValue("reference").ToString();
                ItemRequestTransfer_Details.isSubmit = false;
                ItemRequestTransfer_Details frm = new ItemRequestTransfer_Details(id, reference, docStatus, forSap);
                frm.ShowDialog();
                if (ItemRequestTransfer_Details.isSubmit)
                {
                    bg();
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

        private void button1_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void gridView1_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            try
            {
                string selectedColumnfieldName = gridView1.FocusedColumn != null ? gridView1.FocusedColumn.FieldName : "";
                if (selectedColumnfieldName.Equals("reference") && docStatus.Equals("O"))
                {
                    if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row && (docStatus.Equals("O")))
                    {
                        DXMenuItem item = new DXMenuItem("Close this Transaction");
                        item.Click += (o, args) =>
                        {
                            int intTemp = 0;
                            int id = gridView1.GetFocusedRowCellValue("id") == null ? 0 : Int32.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id")) : intTemp;

                            string reference = gridView1.GetFocusedRowCellValue("reference").ToString();

                            Remarks.isSubmit = false;
                            Remarks.rem = "";
                            Remarks remarkss = new Remarks();
                            remarkss.Text = "Close - " + reference;
                            remarkss.ShowDialog();
                            if (Remarks.isSubmit)
                            {
                                string remarks = Remarks.rem;
                                try
                                {
                                    JObject joBody = new JObject();
                                    joBody.Add("remarks", remarks);
                                    string sResult = apic.loadData("/api/forecast/target_for_delivery/close/", id.ToString(), "application/json", joBody.ToString(), Method.PUT, true);
                                    if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
                                    {
                                        JObject jObjectResponse = JObject.Parse(sResult);
                                        string msg = jObjectResponse["message"] == null ? "" : jObjectResponse["message"].ToString();
                                        bool boolTemp = false;
                                        bool isSubmit = jObjectResponse["success"] == null ? false : bool.TryParse(jObjectResponse["success"].ToString(), out boolTemp) ? Convert.ToBoolean(jObjectResponse["success"].ToString()) : boolTemp;
                                        MessageBox.Show(msg, isSubmit ? "Message" : "Validation", MessageBoxButtons.OK, isSubmit ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                                        if (isSubmit)
                                        {
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
                        DXMenuItem item2 = new DXMenuItem("Cancel this Transaction");
                        item2.Click += (o, args) =>
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
                                    string sResult = apic.loadData("/api/forecast/target_for_delivery/cancel/", id.ToString(), "application/json", joBody.ToString(), Method.PUT, true);
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
                        e.Menu.Items.Add(item2);
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
      
        }

        private void gridView1_TopRowChanged(object sender, EventArgs e)
        {
            showBadge();
        }

        private void gridView1_LeftCoordChanged(object sender, EventArgs e)
        {
            showBadge();
        }


        protected override void OnActivated(EventArgs e)
        {
            Console.WriteLine("Form activated");
        }

        protected override void OnDeactivate(EventArgs e)
        {
            Console.WriteLine("Form deactivated");
        }

        private void gridControl1_SizeChanged(object sender, EventArgs e)
        {
            showBadge();
        }

        private void ItemRequestTransfer_SizeChanged(object sender, EventArgs e)
        {
            foreach (Form form in MdiChildren)
                form.MaximumSize = new System.Drawing.Size(ClientSize.Width - form.Left - 1, ClientSize.Height - form.Top - 1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            if (selectedColumnfieldName.Equals("view_comments"))
            {
                int id = 0, baseID = 0, intTemp = 0;
                id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
                string reference = gridView1.GetFocusedRowCellValue("reference").ToString();
                ItemRequestTransfer_AddComment.isSubmit = false;
                ItemRequestTransfer_Comments frm = new ItemRequestTransfer_Comments(id,reference);
                frm.ShowDialog();
               if (ItemRequestTransfer_AddComment.isSubmit)
                {
                    bg();
                }
            }
        }
    }
}
