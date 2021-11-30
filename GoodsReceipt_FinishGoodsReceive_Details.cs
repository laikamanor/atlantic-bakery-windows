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
    public partial class GoodsReceipt_FinishGoodsReceive_Details : Form
    {
        public GoodsReceipt_FinishGoodsReceive_Details(int id, string reference)
        {
            InitializeComponent();
            this.id = id;
            this.reference = reference;
        }
        int id = 0;
        string reference = "";
        public static bool isSubmit = false;
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        DataTable dtData = new DataTable();
        private void GoodsReceipt_FinishGoodsReceive_Details_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            lblReference.Text = reference;
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
            string sParams = id.ToString() + "?mode=receive";
            string sResult = apic.loadData("/api/production/order/details/", sParams, "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
            {
                JObject joResponse = JObject.Parse(sResult);
                JArray jaData = (JArray)joResponse["data"];
                dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                AutoCompleteStringCollection auto = new AutoCompleteStringCollection();

                if (IsHandleCreated)
                {
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        if (dtData.Rows.Count > 0)
                        {
                            dtData.Columns.Add("var", typeof(double));
                            dtData.Columns.Add("actual_qty", typeof(double));
                            dtData.Columns.Add("action");
                            string[] columnVisible = new string[]
                            {
                            "item_code", "variance","actual_qty","var","action"
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
                            col.Caption = fieldName.Equals("variance") ? "For Receive" : fieldName.Equals("var") ? "Variance" : col.Caption;
                            col.ColumnEdit = fieldName.Equals("remarks") ? repositoryItemMemoEdit1 : fieldName.Equals("action") ? repositoryItemButtonEdit1 : fieldName.Equals("actual_qty") ? repositoryItemTextEdit2 : repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("variance") || fieldName.Equals("var") || fieldName.Equals("actual_qty") ? DevExpress.Utils.FormatType.Numeric : DevExpress.Utils.FormatType.None;

                            col.DisplayFormat.FormatString = fieldName.Equals("variance") || fieldName.Equals("var") || fieldName.Equals("actual_qty") ? "n2" : "";

                            col.Visible = !(fieldName.Equals("id") || fieldName.Equals("doc_id") || fieldName.Equals("targeted_qty") || fieldName.Equals("planned_qty") || fieldName.Equals("received_qty") || fieldName.Equals("whsecode") || fieldName.Equals("uom") || fieldName.Equals("close"));

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

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void gridView1_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            if (selectedColumnfieldName.Equals("actual_qty"))
            {
                double forReceive = 0.00, actualQty = 0.00, doubleTemp = 0.00;
                 forReceive = double.TryParse(gridView1.GetFocusedRowCellValue("variance").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetFocusedRowCellValue("variance").ToString()) : doubleTemp;
                actualQty = double.TryParse(e.Value.ToString(), out doubleTemp) ? Convert.ToDouble(e.Value.ToString()) : doubleTemp;
                double variance = actualQty - forReceive;

                var varCol = gridView1.Columns["var"];

                if(double.TryParse(e.Value.ToString(), out doubleTemp))
                {
                    gridView1.SetRowCellValue(e.RowHandle, varCol, variance.ToString("n2"));
                }else
                {
                    gridView1.SetRowCellValue(e.RowHandle, varCol, null);
                }
            }

        }

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName.Equals("var"))
            {
                double doubleTemp = 0.00;
                double variance = double.TryParse(gridView1.GetFocusedRowCellValue("var").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetFocusedRowCellValue("var").ToString()) : doubleTemp;
                if(variance == 0)
                {
                    e.Appearance.ForeColor = Color.Black;
                }
                if (variance < 0)
                {
                    e.Appearance.ForeColor = Color.Red;
                }
                else if (variance > 0)
                {
                    e.Appearance.ForeColor = Color.Blue;
                }
            }
        }

        private void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            try
            {
                var colItem = gridView1.Columns["item_code"];
                string currentItemCode = gridView1.GetFocusedRowCellValue(colItem).ToString();
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to remove " + currentItemCode + "?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    gridView1.DeleteRow(gridView1.FocusedRowHandle);
                    MessageBox.Show(currentItemCode + " removed!", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string hashedID = RandomString(20);
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to submit?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                if (string.IsNullOrEmpty(txtRemarks.Text.Trim()))
                {
                    MessageBox.Show("Remarks field is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    btnSubmit.Enabled = false;
                    JObject joBody = new JObject();
                    JArray jaDetails = new JArray();
                    string whseCode = "";
                    for (int i = 0; i < gridView1.RowCount; i++)
                    {
                        string sItemCode = gridView1.GetRowCellValue(i, "item_code").ToString();
                        string sWhseCode = whseCode = gridView1.GetRowCellValue(i, "whsecode").ToString();
                        string sUom = gridView1.GetRowCellValue(i, "uom").ToString();

                        double quantity = 0.00, doubleTemp = 0.00;
                        quantity = double.TryParse(gridView1.GetRowCellValue(i, "actual_qty").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(i, "actual_qty").ToString()) : doubleTemp;

                        int id = 0, intTemp = 0;
                        id = int.TryParse(gridView1.GetRowCellValue(i, "id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(i, "id").ToString()) : intTemp;

                        JObject joDetails = new JObject();
                        joDetails.Add("item_code", sItemCode);
                        joDetails.Add("quantity", quantity);
                        joDetails.Add("whsecode", sWhseCode);
                        joDetails.Add("uom", sUom);
                        joDetails.Add("prod_order_row_id", id);
                        joDetails.Add("base_row_id", id);
                        jaDetails.Add(joDetails);
                    }
                    JObject joHeader = new JObject();
                    joHeader.Add("transdate", DateTime.Now);
                    joHeader.Add("prod_order_id", id);
                    if (string.IsNullOrEmpty(txtSAP.Text.Trim()))
                    {
                        joHeader.Add("sap_number", null);
                    }
                    else
                    {
                        joHeader.Add("sap_number", txtSAP.Text);
                    }
                    joHeader.Add("whsecode", whseCode);
                    joHeader.Add("hashed_id", hashedID);
                    joBody.Add("header", joHeader);
                    joBody.Add("rows", jaDetails);
                    Console.WriteLine(joBody);
                    apiPUT(joBody, "/api/production/rec_from_prod/new");
                    btnSubmit.Enabled = true;
                    if (isSubmit)
                    {

                        this.Hide();
                    }
                }
            }
        }

        public string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ0123456789abcdefghijklmnñopqrstuvxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public void apiPUT(JObject body, string URL)
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
                    utility_class utilityc = new utility_class();
                    var client = new RestClient(utilityc.URL);
                    client.Timeout = -1;
                    var request = new RestRequest(URL);
                    Console.WriteLine(URL);
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.Method = Method.POST;

                    Console.WriteLine(body);
                    request.AddParameter("application/json", body, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    if (response.ErrorMessage == null)
                    {
                        if (response.Content.Substring(0, 1).Equals("{"))
                        {
                            JObject jObjectResponse = JObject.Parse(response.Content);
                            isSubmit = false;
                            foreach (var x in jObjectResponse)
                            {
                                if (x.Key.Equals("success"))
                                {
                                    isSubmit = string.IsNullOrEmpty(x.Value.ToString()) ? false : Convert.ToBoolean(x.Value.ToString());
                                    break;
                                }
                            }

                            string msg = "No message response found";
                            foreach (var x in jObjectResponse)
                            {
                                if (x.Key.Equals("message"))
                                {
                                    msg = x.Value.ToString();
                                }
                            }
                            MessageBox.Show(msg, "", MessageBoxButtons.OK, isSubmit ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show(response.Content.ToString(), "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show(response.ErrorMessage, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }
            }
        }
    }
}
