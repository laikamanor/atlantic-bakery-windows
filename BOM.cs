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
    public partial class BOM : Form
    {
        public BOM()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        private void BOM_Load(object sender, EventArgs e)
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
            string sParams = "";
            string sResult = apic.loadData("/api/bom/get_all", sParams, "", "", Method.GET, true);
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
                            col.ColumnEdit = fieldName.Equals("remarks") ? repositoryItemMemoEdit1 : repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("quantity") || fieldName.Equals("base_weight") || fieldName.Equals("tdw") || fieldName.Equals("dw_pc_unbake") || fieldName.Equals("dw_pc_baked") ? DevExpress.Utils.FormatType.Numeric : DevExpress.Utils.FormatType.None;

                            col.DisplayFormat.FormatString = fieldName.Equals("quantity") || fieldName.Equals("base_weight") || fieldName.Equals("tdw") || fieldName.Equals("dw_pc_unbake") || fieldName.Equals("dw_pc_baked") ? "n3" : "";


                            col.Visible = fieldName.Equals("item_code") || fieldName.Equals("quantity") || fieldName.Equals("uom") || fieldName.Equals("base_weight") || fieldName.Equals("base_uom") || fieldName.Equals("tdw") || fieldName.Equals("dw_pc_unbake") || fieldName.Equals("dw_pc_baked") || fieldName.Equals("remarks");

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
                        //showBadge();
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
    }
}
