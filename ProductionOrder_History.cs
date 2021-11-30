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
    public partial class ProductionOrder_History : Form
    {
        public ProductionOrder_History(int id)
        {
            InitializeComponent();
            this.id = id;
        }
        int id = 0;
        devexpress_class devc = new devexpress_class();
        utility_class utilityc = new utility_class();
        api_class apic = new api_class();
        ui_class uic = new ui_class();
        private void ProductionOrder_History_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            bg();
        }
        public void loadData()
        {
            try
            {
                string sParams = id.ToString();
                string sResult = apic.loadData("/api/production/order/status/history/", sParams, "", "", Method.GET, true);
                if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
                {
                    double runningBalance = 0.00;
                    DateTime dtTemp = new DateTime();
                    JObject joResponse = JObject.Parse(sResult);
                    JArray jaData = joResponse["data"] == null ? new JArray() : (JArray)joResponse["data"];

                    if(jaData[0].Count() > 0)
                    {
                        lblReference.Invoke(new Action(delegate ()
                        {
                            lblReference.Text = jaData[0]["prod_order_ref"].ToString();
                        }));
                        lblProdDate.Invoke(new Action(delegate ()
                        {
                            DateTime dtProdDate = new DateTime(), dtProdTemp = new DateTime();

                            dtProdDate = DateTime.TryParse(jaData[0]["prod_order_date"].ToString(), out dtProdTemp) ? Convert.ToDateTime(jaData[0]["prod_order_date"].ToString()) : dtProdTemp;

                            lblProdDate.Text = dtProdDate.Equals(DateTime.MinValue) ? "" : dtProdDate.ToString("yyyy-MM-dd HH:mm:ss");
                        }));
                    }
                    
                    //lblToWhse.Text = jaTransRow[0]["to_whse"].ToString();
                    DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));

                    gridControl1.Invoke(new Action(delegate ()
                    {
                        gridControl1.DataSource = null;
                    }));

                    gridControl1.Invoke(new Action(delegate ()
                    {
                        dtData.SetColumnsOrder("gi_ref", "gi_date", "gi_date_confirmed", "gr_ref", "gr_date");
                        gridControl1.DataSource = dtData;
                        gridView1.OptionsView.ColumnAutoWidth = false;
                        gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            string v = col.GetCaption();
                            string s = col.GetCaption().Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.ColumnEdit =  repositoryItemTextEdit1;

                            col.DisplayFormat.FormatType = fieldName.Equals("prod_order_date") || fieldName.Equals("gi_date") || fieldName.Equals("gi_date_confirmed") || fieldName.Equals("gr_date") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;

                            col.DisplayFormat.FormatString = fieldName.Equals("prod_order_date") || fieldName.Equals("gi_date") || fieldName.Equals("gi_date_confirmed") || fieldName.Equals("gr_date") ? "yyyy-MM-dd HH:mm:ss" : "";

                            col.Visible = !(fieldName.Equals("prod_id") || fieldName.Equals("gi_id") || fieldName.Equals("gr_id") || fieldName.Equals("prod_order_date") || fieldName.Equals("prod_order_ref"));

                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }
                        //auto complete
                        string[] suggestions = { "prod_order_ref" };
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            int GIid = 0, GRid = 0, intTemp = 0;

            GIid = int.TryParse(gridView1.GetFocusedRowCellValue("gi_id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("gi_id").ToString()) : intTemp;

            GRid = int.TryParse(gridView1.GetFocusedRowCellValue("gr_id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("gr_id").ToString()) : intTemp;

            if (selectedColumnfieldName.Equals("gi_ref"))
            {
                string giRef = gridView1.GetFocusedRowCellValue("gi_ref").ToString();

                GoodsIssued_Details frm = new GoodsIssued_Details(GIid, "", giRef);
                frm.ShowDialog();
            }
            else if (selectedColumnfieldName.Equals("gr_ref"))
            {
                string grRef = gridView1.GetFocusedRowCellValue("gr_ref").ToString();

                GoodsReceipt_Details frm = new GoodsReceipt_Details(GRid, "", grRef);
                frm.ShowDialog();
            }
        }
    }
}
