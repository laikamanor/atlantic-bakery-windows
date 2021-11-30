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
    public partial class SalesTransactions_Items2 : Form
    {
        public SalesTransactions_Items2(int id)
        {
            InitializeComponent();
            this.id = id;
        }
        int id = 0;
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        private void SalesTransactions_Items2_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
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

        public void delegateControl(Control c, string value)
        {
            string result = "";
            c.Invoke(new Action(delegate ()
            {
                c.Text = value;
            }));
        }

        public string checkDouble(string value)
        {
            double doubleTemp = 0.00;
            double v = double.TryParse(value.ToString(), out doubleTemp) ? Convert.ToDouble(value.ToString()) : doubleTemp;
            return v.ToString("n2");
        }

        public string checkDateTime(string value)
        {
            DateTime dtTemp = new DateTime();
            DateTime dt  = DateTime.TryParse(value.ToString(), out dtTemp) ? Convert.ToDateTime(value.ToString()) : dtTemp;
            string s = dt.Equals(DateTime.MinValue) ? "" : dt.ToString("yyyy-MM-dd HH:mm:ss");
            return s;
        }

        public string checkDocStatus(string value)
        {
            string s = value.Equals("O") ? "Open" : value.Equals("C") ? "Closed" : value.Equals("N") ? "Cancelled" : "";
            return s;
        }

        public void loadData()
        {
            gridControl1.Invoke(new Action(delegate ()
            {
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
            }));
            string sParams = id.ToString();
            string sResult = apic.loadData("/api/sales/details/", sParams, "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
            {
                JObject joResponse = JObject.Parse(sResult);
                JObject joData = (JObject)joResponse["data"];

                double doubleTemp = 0.00;
                DateTime dtTransdate = new DateTime(), dtTemp = new DateTime();
                //header
                delegateControl(lblReference, joData["reference"].IsNullOrEmpty() ? doubleTemp.ToString("n2") : joData["reference"].ToString());
                delegateControl(lblSAPNumber, joData["sap_number"].IsNullOrEmpty() ?"" : joData["sap_number"].ToString());
                delegateControl(lblDocStatus, joData["docstatus"].IsNullOrEmpty() ? "" : checkDocStatus(joData["docstatus"].ToString()));

                delegateControl(lblTransDate, joData["transdate"].IsNullOrEmpty() ? "" : checkDateTime(joData["transdate"].ToString()));

                //delegateControl(lblTransDate, joData["transdate"].IsNullOrEmpty() ? dtTemp : DateTime.TryParse(joData["transdate"].ToString(), out dtTemp) ? Convert.ToDateTime(joData["transdate"].ToString()) : dtTemp);

                delegateControl(lblGross, joData["gross"].IsNullOrEmpty() ? doubleTemp.ToString("n2") : checkDouble(joData["gross"].ToString()));
                delegateControl(lblDiscAmount, joData["disc_amount"].IsNullOrEmpty() ? doubleTemp.ToString("n2") : checkDouble(joData["disc_amount"].ToString()));
                delegateControl(lblDocTotal, joData["doctotal"].IsNullOrEmpty() ? doubleTemp.ToString("n2") : checkDouble(joData["doctotal"].ToString()));
                delegateControl(lblAppliedAmount, joData["appliedamt"].IsNullOrEmpty() ? doubleTemp.ToString("n2") : checkDouble(joData["appliedamt"].ToString()));
                delegateControl(lblTenderAmount, joData["tenderamt"].IsNullOrEmpty() ? doubleTemp.ToString("n2") : checkDouble(joData["tenderamt"].ToString()));
                delegateControl(lblAmountDue, joData["amount_due"].IsNullOrEmpty() ? doubleTemp.ToString("n2") : checkDouble(joData["amount_due"].ToString()));

                JArray jaSalesRow = (JArray)joData["salesrow"];
                DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaSalesRow.ToString(), (typeof(DataTable)));
                if (IsHandleCreated)
                {
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        if (dtData.Rows.Count > 0)
                        {
                            string[] columnVisible = new string[]
{
                            "item_code", "quantity", "unit_price", "gross", "disc_amount","discprcnt","linetotal"
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
                            //col.Caption = fieldName.Equals("whsecode") ? "From Whse" : col.Caption;
                            col.ColumnEdit =  repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("quantity") || fieldName.Equals("unit_price") || fieldName.Equals("gross") || fieldName.Equals("disc_amount") || fieldName.Equals("discprcnt") || fieldName.Equals("linetotal") ? DevExpress.Utils.FormatType.Numeric : DevExpress.Utils.FormatType.None;
                            col.DisplayFormat.FormatString = fieldName.Equals("quantity") || fieldName.Equals("unit_price") || fieldName.Equals("gross") || fieldName.Equals("disc_amount") || fieldName.Equals("discprcnt") || fieldName.Equals("linetotal") ? "n2" : "";
                            col.Visible = fieldName.Equals("item_code") || fieldName.Equals("quantity") || fieldName.Equals("unit_price") || fieldName.Equals("gross") || fieldName.Equals("disc_amount") || fieldName.Equals("discprcnt") || fieldName.Equals("linetotal");

                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }

                        //auto complete
                        string[] suggestions = { "item_code" };
                        string suggestConcat = string.Join(";", suggestions);
                        gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                        devc.loadSuggestion(gridView1, gridControl1, suggestions);
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

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.RowHandle == HotTrackRow)
                e.Appearance.BackColor = gridView1.PaintAppearance.SelectedRow.BackColor;
            else
                e.Appearance.BackColor = e.Appearance.BackColor;
        }
    }
}
