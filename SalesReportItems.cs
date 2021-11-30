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

namespace AB
{
    public partial class SalesReportItems : Form
    {
        utility_class utilityc = new utility_class();
        devexpress_class devc = new devexpress_class();
        public string URLDetails = "";
        public SalesReportItems()
        {
            InitializeComponent();
        }

        private void SalesReport_Items_Load(object sender, EventArgs e)
        {
            loadData();
        }

        public void loadData()
        {
            Cursor.Current = Cursors.WaitCursor;
            if (Login.jsonResult != null)
            {
                string token = "";
                foreach (var x in Login.jsonResult)
                {
                    if (x.Key.Equals("token"))
                    {
                        token = x.Value.ToString();
                    }
                }
                if (!token.Equals(""))
                {
                    gridControl1.DataSource = null;
                    var client = new RestClient(utilityc.URL);
                    client.Timeout = -1;

                    var request = new RestRequest(URLDetails);
                    request.AddHeader("Authorization", "Bearer " + token);
                    var response = client.Execute(request);
                    JObject jObject = JObject.Parse(response.Content);
                    bool isSuccess = false;
                    foreach(var x in jObject)
                    {
                        if (x.Key.Equals("success"))
                        {
                            isSuccess = Convert.ToBoolean(x.Value.ToString());
                        }
                    }
                    if (isSuccess)
                    {
                        foreach (var x in jObject)
                        {
                            if (x.Key.Equals("data"))
                            {
                                JObject jObjectData = JObject.Parse(x.Value.ToString());
                                foreach (var w in jObjectData)
                                {
                                    if (w.Key.Equals("salesrow"))
                                    {
                                        JArray jsonArraySalesRow = JArray.Parse(w.Value.ToString());
                                        DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jsonArraySalesRow.ToString(), (typeof(DataTable)));
                                        dtData.SetColumnsOrder("item_code", "quantity", "price", "discprcnt", "disc_amount","linetotal");
                                        gridControl1.DataSource = dtData;

                                        gridControl1.Invoke(new Action(delegate ()
                                        {
                                            gridControl1.DataSource = dtData;
                                            gridView1.OptionsView.ColumnAutoWidth = false;
                                            gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                                            foreach (GridColumn col in gridView1.Columns)
                                            {
                                                string fieldName = col.FieldName;
                                                string v = col.GetCaption();
                                                string s = col.GetCaption().Replace("_", " ");
                                                col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                                                col.Caption = fieldName.Equals("linetotal") ? "Total Price" : col.Caption;
                                                col.ColumnEdit = repositoryItemTextEdit1;
                                                col.DisplayFormat.FormatType = fieldName.Equals("item_code") ? DevExpress.Utils.FormatType.None : DevExpress.Utils.FormatType.Numeric;
                                                col.DisplayFormat.FormatString = fieldName.Equals("item_code") ? "" : "n2";
                                                col.Visible = fieldName.Equals("item_code") || fieldName.Equals("quantity") || fieldName.Equals("price") || fieldName.Equals("discprcnt") || fieldName.Equals("disc_amount") || fieldName.Equals("linetotal");

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
                                        }));

                                    }
                                    else if (w.Key.Equals("gross"))
                                    {
                                        txtGrossPrice.Text = Convert.ToDouble(w.Value.ToString()).ToString("n2");
                                    }
                                    else if (w.Key.Equals("disc_amount"))
                                    {
                                        txtDiscountAmount.Text = Convert.ToDouble(w.Value.ToString()).ToString("n2");
                                    }
                                    else if (w.Key.Equals("amount_due"))
                                    {
                                        txtlAmountPayable.Text = Convert.ToDouble(w.Value.ToString()).ToString("n2");
                                    }
                                    else if (w.Key.Equals("tenderamt"))
                                    {
                                        txtTenderAmount.Text = Convert.ToDouble(w.Value.ToString()).ToString("n2");
                                    }
                                    else if (w.Key.Equals("change"))
                                    {
                                        txtChange.Text = Convert.ToDouble(w.Value.ToString()).ToString("n2");
                                    }
                                    else if (w.Key.Equals("reference"))
                                    {
                                        txtReference.Text = w.Value.ToString();
                                    }
                                    else if (w.Key.Equals("transtype"))
                                    {
                                        txtTenderType.Text = w.Value.ToString();
                                    }
                                    else if (w.Key.Equals("cust_code"))
                                    {
                                        txtCustomerCode.Text = w.Value.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
