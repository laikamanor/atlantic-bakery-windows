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
    public partial class SOA2 : Form
    {
        public SOA2(string docStatus)
        {
            InitializeComponent();
            this.docStatus = docStatus;
        }
        string docStatus = "";
        api_class apic = new api_class();
        ui_class uic = new ui_class();
        devexpress_class devc = new devexpress_class();
        DataTable dtCustType = new DataTable();
        private void SOA2_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            dtFromDate.EditValue = DateTime.Now;
            dtToDate.EditValue = DateTime.Now;
            loadCustType();
            bg();
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
                sResult = apic.loadData("/api/custtype/get_all", "?plant=" + plantCode, "", "", Method.GET, true);
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
            string sCustCode = "?cust_type=", sFromDate = "&from_date=", sToDate = "&to_date=",  sDocStatus = "&docstatus=" + docStatus;
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
            dtFromDate.Invoke(new Action(delegate ()
            {
                sFromDate += cFromDate ? dtFromDate.Text : "";
            }));
            dtToDate.Invoke(new Action(delegate ()
            {
                sToDate += cToDate ? dtToDate.Text : "";
            }));
            string sParams = sCustCode + sFromDate + sToDate + sDocStatus;
            string sResult = apic.loadData("/soa/get_all", sParams, "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
            {
                JObject joResponse = JObject.Parse(sResult);
                JArray jaData = (JArray)joResponse["data"];
                DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                if (IsHandleCreated)
                {
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        if (dtData.Rows.Count > 0)
                        {
                            string[] columnVisible = new string[]
{
                            "transdate", "reference", "cust_code","balance", "total_amount","age","docstatus"
};
                            dtData.SetColumnsOrder(columnVisible);
                        }
                        gridControl1.DataSource = null;

                        if (dtData.Rows.Count > 0)
                        {
                            foreach (DataRow row in dtData.Rows)
                            {
                                string decodeStatus = row["docstatus"].ToString() == "O" ? "Open" : row["docstatus"].ToString() == "C" ? "Closed" : row["docstatus"].ToString() == "N" ? "Cancelled" : "";
                                row["docstatus"] = decodeStatus;
                            }
                        }

                        gridControl1.DataSource = dtData;

                        gridView1.OptionsView.ColumnAutoWidth = false;
                        gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            string v = col.GetCaption();
                            string s = col.GetCaption().Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.ColumnEdit =repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("total_amount") || fieldName.Equals("balance") || fieldName.Equals("age") ? DevExpress.Utils.FormatType.Numeric : fieldName.Equals("transdate") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;
                            col.DisplayFormat.FormatString = fieldName.Equals("total_amount") || fieldName.Equals("balance") || fieldName.Equals("age") ? "n2" : fieldName.Equals("transdate") ? "yyyy-MM-dd HH:mm:ss" : "";
                            col.Visible = fieldName.Equals("transdate") || fieldName.Equals("reference") || fieldName.Equals("cust_code") || fieldName.Equals("total_amount") || fieldName.Equals("balance") || fieldName.Equals("docstatus") || fieldName.Equals("age");

                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }

                        //auto complete
                        string[] suggestions = { "reference","cust_code" };
                        string suggestConcat = string.Join(";", suggestions);
                        gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                        devc.loadSuggestion(gridView1, gridControl1, suggestions);

                        var refCol = gridView1.Columns["reference"];
                        if (refCol != null)
                        {
                            gridView1.Columns["reference"].Summary.Clear();
                            gridView1.Columns["reference"].Summary.Add(DevExpress.Data.SummaryItemType.Count, "reference", "Count: {0:N0}");
                        }
                        var balCol = gridView1.Columns["balance"];
                        if(balCol != null)
                        {
                            gridView1.Columns["balance"].Summary.Clear();
                            gridView1.Columns["balance"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "balance", "Total Balance: {0:n2}");
                        }

                        gridView1.BestFitColumns();
                        //var col2 = gridView1.Columns["remarks"];
                        //if (col2 != null)
                        //{
                        //    col2.Width = 200;
                        //}
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

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
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

        private void gridView1_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            if (e.RowHandle == HotTrackRow)
                e.Appearance.BackColor = gridView1.PaintAppearance.SelectedRow.BackColor;
            else
                e.Appearance.BackColor = e.Appearance.BackColor;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void checkFromDate_CheckedChanged(object sender, EventArgs e)
        {
            dtFromDate.Visible = checkFromDate.Checked;
        }

        private void checkToDate_CheckedChanged(object sender, EventArgs e)
        {
            dtToDate.Visible = checkToDate.Checked;
        }

        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            int id = 0, baseID = 0, intTemp = 0;
            id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
            if (selectedColumnfieldName.Equals("reference"))
            {
                SOA_Details frm = new SOA_Details();
                frm.selectedID = id;
                frm.ShowDialog();
            }
        }
    }
}
