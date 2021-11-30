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
    public partial class ItemDiscount2 : Form
    {
        public ItemDiscount2(DataTable dt)
        {
            InitializeComponent();
            this.dt = dt;
        }
        DataTable dt = new DataTable();
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        private void ItemDiscount2_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            loadData();
        }

        public void loadData()
        {
            gridControl1.Invoke(new Action(delegate ()
            {
                dt.SetColumnsOrder("reference","item_code", "quantity", "unit_price", "gross", "disc_amount", "discprcnt", "linetotal", "username");
                gridControl1.DataSource = dt;
                gridView1.OptionsView.ColumnAutoWidth = false;
                gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;

                foreach (GridColumn col in gridView1.Columns)
                {
                    string fieldName = col.FieldName;
                    string v = col.GetCaption();
                    string s = col.GetCaption().Replace("_", " ");
                    col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                    col.ColumnEdit = fieldName.Equals("item_code") || fieldName.Equals("reference") || fieldName.Equals("disctype") ? repositoryItemMemoEdit2 : repositoryItemTextEdit1;

                    col.Caption = fieldName.Equals("linetotal") ? "Total Price" : fieldName.Equals("username") ? "Processed By" : col.Caption;

                    col.DisplayFormat.FormatType = fieldName.Equals("disc_amount") || fieldName.Equals("linetotal") || fieldName.Equals("gross") || fieldName.Equals("unit_price") || fieldName.Equals("quantity") || fieldName.Equals("discprcnt") || fieldName.Equals("gross") ? DevExpress.Utils.FormatType.Numeric : fieldName.Equals("transdate") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;

                    col.DisplayFormat.FormatString = fieldName.Equals("disc_amount") || fieldName.Equals("linetotal") || fieldName.Equals("gross") || fieldName.Equals("unit_price") || fieldName.Equals("quantity") || fieldName.Equals("discprcnt") || fieldName.Equals("gross") ? "n2" : fieldName.Equals("transdate") ? "yyyy-MM-dd HH:mm:ss" : "";

                    col.Visible= !(fieldName.Equals("fullname"));

                    //col.Visible = !(fieldName.Equals("id") || fieldName.Equals("transnumber") || fieldName.Equals("delfee"));

                    //fonts
                    FontFamily fontArial = new FontFamily("Arial");
                    col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                    col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                }

                //auto complete
                string[] suggestions = { "reference", "item_code" };
                string suggestConcat = string.Join(";", suggestions);
                gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                devc.loadSuggestion(gridView1, gridControl1, suggestions);
               
                var colRef = gridView1.Columns["reference"];
                if (colRef != null)
                {
                    gridView1.Columns["reference"].Summary.Clear();
                    gridView1.Columns["reference"].Summary.Add(DevExpress.Data.SummaryItemType.Count, "reference", "Total Item: {0:N0}");
                }

                gridView1.BestFitColumns();
                var colIemCode = gridView1.Columns["item_code"];
                var col2 = gridView1.Columns["remarks"];
                if (colIemCode != null)
                {
                    colIemCode.Width = 100;
                }
                if (colRef != null)
                {
                    colRef.Width = 120;
                }
            }));
        }
    }
}
