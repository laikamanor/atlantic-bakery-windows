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
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        private void Form5_Load(object sender, EventArgs e)
        {
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

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        public void loadData()
        {
            gridControl1.Invoke(new Action(delegate ()
            {
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
            }));
            string sParams = "";
            string sResult = apic.loadData("/api/inv/count/confirm?transdate=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sParams, "", "", Method.GET, true);
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
                            "item_code", "ending_final_count","quantity", "po_final_count","variance","total_amount"
                            };
                            dtData.SetColumnsOrder(columnVisible);
                        }

                        gridControl1.DataSource = null;


                        //DataTable dtCloned = new DataTable();
                        //if (dtData.Rows.Count > 0)
                        //{
                        //    dtCloned = dtData.Clone();
                        //    dtCloned.Columns["date_closed"].DataType = dtCloned.Columns["transdate"].DataType = typeof(DateTime);
                        //    foreach (DataRow row in dtData.Rows)
                        //    {
                        //        dtCloned.ImportRow(row);
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
                            col.ColumnEdit = repositoryItemTextEdit1;
                            //col.DisplayFormat.FormatType = fieldName.Equals("quantity") ? DevExpress.Utils.FormatType.Numeric : fieldName.Equals("transdate") || fieldName.Equals("date_closed") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;
                            //col.DisplayFormat.FormatString = fieldName.Equals("quantity") ? "n2" : fieldName.Equals("transdate") || fieldName.Equals("date_closed") ? "yyyy-MM-dd HH:mm:ss" : "";
                            //col.Visible = !(fieldName.Equals("id") || fieldName.Equals("series") || fieldName.Equals("seriescode") || fieldName.Equals("transnumber") || fieldName.Equals("docstatus") || fieldName.Equals("transtype"));

                            col.Visible = fieldName.Equals("item_code") || fieldName.Equals("ending_final_count") || fieldName.Equals("quantity") || fieldName.Equals("po_final_count") || fieldName.Equals("variance") || fieldName.Equals("total_amount") || fieldName.Equals("po_final_count");

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
                        string[] suggestions = { "item_code" };
                        string suggestConcat = string.Join(";", suggestions);
                        gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                        devc.loadSuggestion(gridView1, gridControl1, suggestions);
                    }));
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }
    }
}
