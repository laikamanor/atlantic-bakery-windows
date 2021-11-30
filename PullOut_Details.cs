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
    public partial class PullOut_Details : Form
    {
        public PullOut_Details(int id)
        {
            InitializeComponent();
            this.id = id;
        }
        public static bool isSubmit=false;
        int id = 0;
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        private void PullOut_Details_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            loadData();
        }

        public void loadData()
        {
            try
            {
                string sParams = id.ToString();
                string sResult = apic.loadData("/api/pullout/details/", sParams, "", "", Method.GET, true);
                if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
                {
                    DateTime dtTemp = new DateTime();
                    JObject joResponse = JObject.Parse(sResult);
                    JObject joData = joResponse["data"] == null ? new JObject() : (JObject)joResponse["data"];

                    lblRemarks.Text = joData["remarks"].IsNullOrEmpty() ? "" : joData["remarks"].ToString();
                    lblReference.Text = joData["reference"] == null ? "" : joData["reference"].ToString();
                    string docStatus = joData["docstatus"] == null ? "" : joData["docstatus"].ToString();
                    docStatus = docStatus.Equals("O") ? "Open" : docStatus.Equals("C") ? "Closed" : docStatus.Equals("N") ? "Cancelled" : "";
                    lblDocStatus.Text = docStatus;
                    lblTransDate.Text = joData["transdate"] == null ? "" : DateTime.TryParse(joData["transdate"].ToString().Replace("T", " "), out dtTemp) ? Convert.ToDateTime(joData["transdate"].ToString().Replace("T", " ")).ToString("yyyy-MM-dd HH:mm:ss") : "";
                    JArray jaTransRow = joData["row"] == null ? new JArray() : (JArray)joData["row"];
                    DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaTransRow.ToString(), (typeof(DataTable)));

                    gridControl1.DataSource = null;
                    string[] columnVisible = new string[]
                    {
                            "item_code", "quantity", "receive_qty", "uom","whsecode","to_whse"
                    };
                    dtData.SetColumnsOrder(columnVisible);

                    gridControl1.DataSource = dtData;
                    gridView1.OptionsView.ColumnAutoWidth = false;
                    gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                    foreach (GridColumn col in gridView1.Columns)
                    {
                        string fieldName = col.FieldName;
                        string v = col.GetCaption();
                        string s = col.GetCaption().Replace("_", " ");
                        col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                        col.Caption = fieldName.Equals("whsecode") ? "From Whse" : col.Caption;
                        col.ColumnEdit = repositoryItemTextEdit1;
                        col.DisplayFormat.FormatType = fieldName.Equals("quantity") || fieldName.Equals("receive_qty") ? DevExpress.Utils.FormatType.Numeric : DevExpress.Utils.FormatType.None;
                        col.DisplayFormat.FormatString = fieldName.Equals("quantity") || fieldName.Equals("receive_qty") ? "n2" : "";
                        col.Visible = fieldName.Equals("item_code") || fieldName.Equals("quantity") || fieldName.Equals("receive_qty") || fieldName.Equals("uom") || fieldName.Equals("whsecode") || fieldName.Equals("to_whse");

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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
