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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using DevExpress.XtraGrid.Columns;
using System.Globalization;
using RestSharp;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;

namespace AB
{
    public partial class ItemDeposit : Form
    {
        public ItemDeposit(int id)
        {
            InitializeComponent();
            selectedID = id;
        }
        int selectedID = 0;
        utility_class utilityc = new utility_class();
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        private void ItemDeposit_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            loadData();
        }

        public void loadData()
        {
            try
            {
                string sParams = selectedID.ToString();
                string sResult = apic.loadData("/api/deposit/applied_trans/", sParams, "", "", Method.GET, true);
                if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
                {
                    DateTime dtTemp = new DateTime();
                    JObject joResponse = JObject.Parse(sResult);
                    JArray jaData = joResponse["data"] == null ? new JArray() : (JArray)joResponse["data"];

                    DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));

                    gridControl1.DataSource = null;
                    string[] columnVisible = new string[]
                    {
                            "transdate", "ref1", "ref2", "transtype","dep_in","dep_out","running_balance","remarks"
                    };
                    double doubleTemp = 0.00;
                    DataTable dtCloned = new DataTable();
                    if (dtData.Rows.Count > 0)
                    {
                        dtData.Columns.Add("running_balance", typeof(double));
                        dtCloned = dtData.Clone();
                        if (dtCloned.Columns.Contains("dep_in"))
                        {
                            dtCloned.Columns["dep_in"].DataType = typeof(double);
                        }
                        if (dtCloned.Columns.Contains("dep_out"))
                        {
                            dtCloned.Columns["dep_out"].DataType = typeof(double);
                        }

                        double runningBalance = 0.00;
                        foreach (DataRow row in dtData.Rows)
                        {
                            if (dtData.Columns.Contains("running_balance"))
                            {
                                double depIn = row["dep_in"] == null ? doubleTemp : double.TryParse(row["dep_in"].ToString(), out doubleTemp) ? Convert.ToDouble(row["dep_in"].ToString()) : doubleTemp;
                                double depOut = row["dep_out"] == null ? doubleTemp : double.TryParse(row["dep_out"].ToString(), out doubleTemp) ? Convert.ToDouble(row["dep_out"].ToString()) : doubleTemp;
                                //runningBalance += depIn;
                                runningBalance += depIn - depOut ;
                                row["dep_in"] = depIn <= 0 ? (object)DBNull.Value : depIn;
                                row["dep_out"] = depOut <= 0 ? (object)DBNull.Value : depOut;
                                row["running_balance"] = runningBalance <= 0 ? 0.00 : runningBalance;
                                dtCloned.ImportRow(row);
                            }
                        }
                        dtCloned.SetColumnsOrder(columnVisible);
                    }



                    gridControl1.DataSource = dtCloned;
                    gridView1.OptionsView.ColumnAutoWidth = false;
                    gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                    foreach (GridColumn col in gridView1.Columns)
                    {
                        string fieldName = col.FieldName;
                        string v = col.GetCaption();
                        string s = col.GetCaption().Replace("_", " ");
                        col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                        col.Caption = fieldName.Equals("dep_in") ? "Deposit In" : fieldName.Equals("dep_out") ? "Deposit Out" : fieldName.Equals("ref1") ? "Reference" : fieldName.Equals("ref2") ? "Reference2" : col.Caption;
                        col.ColumnEdit = fieldName.Equals("remarks") ? repositoryItemMemoEdit1 : repositoryItemTextEdit1;
                        col.DisplayFormat.FormatType = fieldName.Equals("dep_in") || fieldName.Equals("dep_out") || fieldName.Equals("running_balance") ? DevExpress.Utils.FormatType.Numeric : DevExpress.Utils.FormatType.None;
                        col.DisplayFormat.FormatString = fieldName.Equals("dep_in") || fieldName.Equals("dep_out") || fieldName.Equals("running_balance") ? "n2" : "";
                        col.Visible = fieldName.Equals("transdate") || fieldName.Equals("dep_in") || fieldName.Equals("dep_out") || fieldName.Equals("ref1") || fieldName.Equals("ref2") || fieldName.Equals("remarks") || fieldName.Equals("running_balance");

                        //fonts
                        FontFamily fontArial = new FontFamily("Arial");
                        col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                        col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                    }
                    //auto complete
                    string[] suggestions = { "ref1" };
                    string suggestConcat = string.Join(";", suggestions);
                    gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                    devc.loadSuggestion(gridView1, gridControl1, suggestions);
                    gridView1.BestFitColumns();
                    var col2 = gridView1.Columns["remarks"];
                    if (col2 != null)
                    {
                        col2.Width = 200;
                    }
                    gridView1.OptionsView.RowAutoHeight = true;

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
    }
}
