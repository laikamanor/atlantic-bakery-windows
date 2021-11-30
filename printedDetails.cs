using Newtonsoft.Json.Linq;
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
using Newtonsoft.Json;
using System.Globalization;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;

namespace AB
{
    public partial class printedDetails : Form
    {
        public printedDetails()
        {
            InitializeComponent();
        }
        utility_class utilityc = new utility_class();
        public string url = "";
        public int selectedID = 0;
        private void printedDetails_Load(object sender, EventArgs e)
        {
            loadData();
            foreach (GridColumn col in gridView1.Columns)
            {
                col.Width = 100;
                string v = col.GetCaption();
                string s = col.GetCaption().Replace("_", " ");
                col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                col.Visible = v.Equals("id") || v.Equals("doc_id") ? false : true;
                col.DisplayFormat.FormatType = v.Equals("cash_sales") || v.Equals("ar_sales") || v.Equals("agent_sales") || v.Equals("total") || v.Equals("doctotal") ? DevExpress.Utils.FormatType.Numeric : v.Equals("transdate") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;
                col.DisplayFormat.FormatString = v.Equals("cash_sales") || v.Equals("ar_sales") || v.Equals("agent_sales") || v.Equals("total") || v.Equals("doctotal") ? "n2" : v.Equals("transdate") ? "yyyy-MM-dd HH:mm" : "";
                col.ColumnEdit = repositoryItemTextEdit1;
            }
        }

        public void loadData()
        {
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
                    var client = new RestClient(utilityc.URL);
                    client.Timeout = -1;
                    var request = new RestRequest(url + selectedID);
                    Console.WriteLine(url + selectedID);
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.Method = Method.GET;
                    var response = client.Execute(request);
                    Console.WriteLine(response.Content);
                    if (response.ErrorMessage == null)
                    {
                        JObject jObjectResponse = JObject.Parse(response.Content);
                        bool isSubmit = false, boolTemp = false;
                        string msg = "No message response found", data = "";
                        foreach (var x in jObjectResponse)
                        {
                            if (x.Key.Equals("success"))
                            {
                                isSubmit = bool.TryParse(x.Value.ToString(), out boolTemp) ? Convert.ToBoolean(x.Value.ToString()) : false;
                            }
                            else if (x.Key.Equals("message"))
                            {
                                msg = x.Value.ToString();
                            }
                            else if (x.Key.Equals("data"))
                            {
                                data = x.Value.ToString();
                            }
                        }
                        if (isSubmit)
                        {
                            DataTable dt = (DataTable)JsonConvert.DeserializeObject(data, (typeof(DataTable)));
                            gridControl1.DataSource = dt;
                        }
                        else
                        {
                            MessageBox.Show(msg, "", MessageBoxButtons.OK, isSubmit ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show(response.ErrorMessage, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void gridView1_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            //Console.WriteLine(gridView1.GetRowCellValue(e.RowHandle, "total_discount_amount").ToString());
            //e.HighPriority = true;
            //if (e.RowHandle >= 0)
            //{
            //    double doubleTemp = 0.00;
            //    double cashVariance = double.TryParse(gridView1.GetRowCellValue(e.RowHandle, "cash_variance").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(e.RowHandle, "cash_variance").ToString()) : doubleTemp;
            //    double discountAmount = double.TryParse(gridView1.GetRowCellValue(e.RowHandle, "total_discount_amount").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(e.RowHandle, "total_discount_amount").ToString()) : doubleTemp;
            //    e.Appearance.BackColor = cashVariance < 0 ? Color.Red : Color.Blue;
            //    Console.WriteLine(gridView1.GetRowCellValue(e.RowHandle, "total_discount_amount").ToString());
            //    e.Appearance.BackColor = discountAmount > 0 ? Color.Yellow : Color.White;
            //}else
            //{
            //    Console.WriteLine("wa : " + gridView1.GetRowCellValue(e.RowHandle, "total_discount_amount").ToString());
            //}
        }

        private void gridView1_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
     
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            finalSummaryReport frm = new finalSummaryReport();
            frm.ShowDialog();
        }
    }
}
