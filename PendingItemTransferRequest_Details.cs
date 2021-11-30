using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
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
    public partial class PendingItemTransferRequest_Details : Form
    {
        public PendingItemTransferRequest_Details(int id, int baseObjtype, string reference)
        {
            InitializeComponent();
            this.id = id;
            this.reference = reference;
            this.baseObjtype = baseObjtype;
        }
        int id = 0, baseObjtype = 0;
        string reference = "";
        public static bool isSubmit = false;
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        private void PendingItemTransferRequest_Details_Load(object sender, EventArgs e)
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
            string sParams = id.ToString() + "?mode=delivery";
            string sResult = apic.loadData("/api/forecast/get_for_delivery/details/", sParams, "", "", Method.GET, true);
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
                            dtData.Columns.Add("actual_qty", typeof(double));
                            dtData.Columns.Add("variance", typeof(double));
                            dtData.Columns.Add("action");
                            string[] columnVisible = new string[]
                            {
                            "item_code", "final_qty","balance","actual_qty","actual_delivered","variance","uom","action","from_whse", "to_whse"
                            };
                            dtData.SetColumnsOrder(columnVisible);

                            string sFromWhse = "", sToWhse = "";
                            sFromWhse = dtData.Rows[0]["from_whse"].ToString();
                            sToWhse = dtData.Rows[0]["to_whse"].ToString();
                            lblFromWhse.Invoke(new Action(delegate ()
                            {
                                lblFromWhse.Text = sFromWhse;
                            }));
                            lblToWhse.Invoke(new Action(delegate ()
                            {
                                lblToWhse.Text = sToWhse;
                            }));
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
                            col.ColumnEdit = fieldName.Equals("remarks") ? repositoryItemMemoEdit1 : fieldName.Equals("actual_qty") ? repositoryItemTextEdit2 : fieldName.Equals("action") ? repositoryItemButtonEdit1 : repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("balance") || fieldName.Equals("final_qty") || fieldName.Equals("actual_delivered") || fieldName.Equals("actual_qty") || fieldName.Equals("variance") ? DevExpress.Utils.FormatType.Numeric : DevExpress.Utils.FormatType.None;

                            col.DisplayFormat.FormatString = fieldName.Equals("balance") || fieldName.Equals("final_qty") || fieldName.Equals("actual_delivered") || fieldName.Equals("actual_qty") || fieldName.Equals("variance") ? "n2" : "";

                            col.Visible = !(fieldName.Equals("id") || fieldName.Equals("objtype") || fieldName.Equals("id") || fieldName.Equals("doc_id") || fieldName.Equals("actual_delivered") || fieldName.Equals("linestatus") || fieldName.Equals("final_qty") || fieldName.Equals("from_whse") || fieldName.Equals("to_whse"));
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

        public bool haveZeroActualQty()
        {
            bool result = false;
            int iResult = 0;
            try
            {
                for (int i = 0; i < gridView1.RowCount; i++)
                {
                    double actualQty = 0.00, doubleTemp = 0.00;
                    actualQty = double.TryParse(gridView1.GetRowCellValue(i, "actual_qty").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(i, "actual_qty").ToString()) : doubleTemp;
                    Console.WriteLine("actual " + actualQty);
                    if (actualQty <= 0)
                    {
                        iResult++;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            result = iResult > 0;
            return result;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {

                string hashedID = RandomString(20);
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to submit?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    btnSubmit.Enabled = false;
                     if (haveZeroActualQty())
                    {
                        btnSubmit.Enabled = true;
                        MessageBox.Show("You have unfilled actual qty!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (string.IsNullOrEmpty(txtRemarks.Text.Trim()))
                    {
                        btnSubmit.Enabled = true;
                        MessageBox.Show("Remarks field is required", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        JObject joBody = new JObject();
                        JObject joHeader = new JObject();
                        joHeader.Add("transdate", DateTime.Now);
                        joHeader.Add("sap_number", string.IsNullOrEmpty(txtSAP.Text.Trim()) ? null : txtSAP.Text);
                        joHeader.Add("remarks", txtRemarks.Text.Trim());
                        joHeader.Add("base_id", id);
                        joHeader.Add("base_objtype", baseObjtype);
                        joHeader.Add("hashed_id", hashedID);

                        //rows
                        JArray jaRows = new JArray();
                        for (int i = 0; i < gridView1.RowCount; i++)
                        {
                            JObject joRows = new JObject();

                            string sItemCode = gridView1.GetRowCellValue(i, "item_code").ToString();
                            string sUom = gridView1.GetRowCellValue(i, "uom").ToString();
                            string sToWhse = gridView1.GetRowCellValue(i, "to_whse").ToString();

                            int rowId = 0, intTemp = 0;
                            rowId = int.TryParse(gridView1.GetRowCellValue(i, "id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(i, "id").ToString()) : intTemp;


                            double quantity = 0.00, doubleTemp = 0.00;
                            quantity = double.TryParse(gridView1.GetRowCellValue(i, "actual_qty").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(i, "actual_qty").ToString()) : doubleTemp;

                            joRows.Add("row_base_id", rowId);
                            joRows.Add("item_code", string.IsNullOrEmpty(sItemCode.Trim()) ? null : sItemCode);
                            joRows.Add("uom", string.IsNullOrEmpty(sUom.Trim()) ? null : sUom);
                            joRows.Add("to_whse", string.IsNullOrEmpty(sToWhse.Trim()) ? null : sToWhse);
                            joRows.Add("quantity", double.TryParse(gridView1.GetRowCellValue(i, "actual_qty").ToString(), out doubleTemp) ? quantity : (double?)null);

                            jaRows.Add(joRows);
                        }
                        joBody.Add("header", joHeader);
                        joBody.Add("details", jaRows);
                        Console.WriteLine(joBody);
                        apiPUT(joBody, "/api/inv/trfr/new");
                        if (isSubmit)
                        {
                            btnSubmit.Enabled = true;
                            this.Hide();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void gridView1_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            if (selectedColumnfieldName.Equals("actual_qty"))
            {
                double balance = 0.00, actualQty = 0.00, doubleTemp = 0.00;
                balance = double.TryParse(gridView1.GetFocusedRowCellValue("balance").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetFocusedRowCellValue("balance").ToString()) : doubleTemp;
                actualQty = double.TryParse(e.Value.ToString(), out doubleTemp) ? Convert.ToDouble(e.Value.ToString()) : doubleTemp;
                double variance = actualQty - balance;

                var varCol = gridView1.Columns["variance"];

                if (double.TryParse(e.Value.ToString(), out doubleTemp))
                {
                    gridView1.SetRowCellValue(e.RowHandle, varCol, variance.ToString("n2"));
                }
                else
                {
                    gridView1.SetRowCellValue(e.RowHandle, varCol, null);
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
          try
            {
                if (e.Column.FieldName.Equals("variance"))
                {
                    double doubleTemp = 0.00;
                    double variance = double.TryParse(gridView1.GetFocusedRowCellValue("variance").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetFocusedRowCellValue("variance").ToString()) : doubleTemp;
                    if (variance == 0)
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
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
