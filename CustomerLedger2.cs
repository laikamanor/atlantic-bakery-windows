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
    public partial class CustomerLedger2 : Form
    {
        public CustomerLedger2()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        ui_class uic = new ui_class();
        devexpress_class devc = new devexpress_class();
        DataTable dtCustType = new DataTable();
        private void CustomerLedger2_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            loadCustType();
            bg();
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


        public void loadData()
        {
            string sCustType = "?cust_type=" + apic.findValueInDataTable(dtCustType, uic.delegateControl(cmbCustomerType), "name", "id");
            string sParams = sCustType;
            string sResult = apic.loadData("/api/report/customer/sales_summary", sParams, "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult.Trim()))
            {
                if (sResult.Substring(0, 1).Equals("{"))
                {
                    JObject joResponse = JObject.Parse(sResult);
                    JArray jaData = (JArray)joResponse["data"];
                    DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        gridControl1.DataSource = null;
                        gridControl1.DataSource = dtData;
                        gridView1.OptionsView.ColumnAutoWidth = false;
                        gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            string v = col.GetCaption();
                            string s = v.Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.ColumnEdit =  repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("balance") ? DevExpress.Utils.FormatType.Numeric : DevExpress.Utils.FormatType.None;
                            col.DisplayFormat.FormatString = fieldName.Equals("balance") ? "n2" : "";

                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }
                        //auto complete
                        string[] suggestions = { "cust_code" };
                        string suggestConcat = string.Join(";", suggestions);
                        gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                        devc.loadSuggestion(gridView1, gridControl1, suggestions);

                        gridView1.Columns["balance"].Summary.Clear();
                        gridView1.Columns["balance"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "balance", "Total: {0:n2}");

                        gridView1.BestFitColumns();
                    }));
                }
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
            bg();
        }

        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            string custCode = gridView1.GetFocusedRowCellValue("cust_code") == null ? "" : gridView1.GetFocusedRowCellValue("cust_code").ToString();
            if (selectedColumnfieldName.Equals("cust_code"))
            {
                CustomerLedger_Details2 frm = new CustomerLedger_Details2(custCode);
                frm.ShowDialog();
            }
        }
    }
}
