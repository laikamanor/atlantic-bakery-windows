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
    public partial class GoodsReceipt_FinishGoodsReceive : Form
    {
        public GoodsReceipt_FinishGoodsReceive()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        private void GoodsReceipt_FinishGoodsReceive_Load(object sender, EventArgs e)
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

        public void loadData()
        {
            gridControl1.Invoke(new Action(delegate ()
            {
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
            }));
            string sParams = "?mode=receive";
            string sResult = apic.loadData("/api/production/order/get_all", sParams, "", "", Method.GET, true);
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
                            string[] columnVisible = new string[]
                            {
                            "transdate", "reference","sap_number","production_date","remarks"
                            };
                            dtData.SetColumnsOrder(columnVisible);
                        }

                        gridControl1.DataSource = null;


                        //DataTable dtCloned = new DataTable(), dtFinal = new DataTable();
                        //if (dtData.Rows.Count > 0)
                        //{
                        //    dtCloned = dtData.Clone();
                        //    dtCloned.Columns["confirm"].DataType = typeof(string);
                        //    foreach (DataRow row in dtData.Rows)
                        //    {

                        //        dtCloned.ImportRow(row);
                        //    }
                        //    dtFinal = dtCloned.Clone();
                        //    foreach (DataRow row in dtCloned.Rows)
                        //    {

                        //        bool isConfirm = false, boolTemp = false;
                        //        isConfirm = bool.TryParse(row["confirm"].ToString(), out boolTemp) ? Convert.ToBoolean(row["confirm"].ToString()) : boolTemp;
                        //        row["confirm"] = isConfirm ? "✔" : "";

                        //        string encodeStatus = row["docstatus"].ToString() == "O" ? "Open" : row["docstatus"].ToString() == "C" ? "Closed" : row["docstatus"].ToString() == "N" ? "Cancelled" : "";
                        //        row["docstatus"] = encodeStatus;

                        //        dtFinal.ImportRow(row);
                        //    }
                        //}

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
                            col.DisplayFormat.FormatType = fieldName.Equals("transdate") || fieldName.Equals("production_date") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;

                            col.DisplayFormat.FormatString = fieldName.Equals("transdate") || fieldName.Equals("production_date") ? "yyyy-MM-dd HH:mm:ss" : "";

                            col.Visible = !(fieldName.Equals("id") || fieldName.Equals("docstatus") || fieldName.Equals("issued") || fieldName.Equals("production_status"));

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
                    }));
                }
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            int id = 0, baseID = 0, intTemp = 0;
            id = gridView1.GetFocusedRowCellValue("id") == null ? 0 : int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;

            string selectedReference = gridView1.GetFocusedRowCellValue("reference").ToString();
            //baseID = int.TryParse(gridView1.GetFocusedRowCellValue("base_id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("base_id").ToString()) : intTemp;
            if (selectedColumnfieldName.Equals("reference"))
            {
                GoodsReceipt_FinishGoodsReceive_Details.isSubmit = false;
                GoodsReceipt_FinishGoodsReceive_Details frm = new GoodsReceipt_FinishGoodsReceive_Details(id, selectedReference);
                frm.ShowDialog();
                if (GoodsReceipt_FinishGoodsReceive_Details.isSubmit)
                {
                    bg();
                }
            }
        }
    }
}
