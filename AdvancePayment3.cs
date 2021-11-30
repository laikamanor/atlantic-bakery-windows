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
    public partial class AdvancePayment3 : Form
    {
        public AdvancePayment3(string tabName)
        {
            InitializeComponent();
            this.tabName = tabName;
        }
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        utility_class utilityc = new utility_class();
        string tabName = "";
        private void AdvancePayment3_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            cmbDocStatus.SelectedIndex = tabName.Equals("In Deposit") ? 1 : 0;
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
            ///api/deposit/get_all?&status=O&used=
            gridControl1.Invoke(new Action(delegate ()
            {
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
            }));
            string sStatus = "?status=";
            string sUsedDeposit = tabName.Equals("In Deposit") ? "&used=" : "&used=1";
            cmbDocStatus.Invoke(new Action(delegate ()
            {
                string encodeStatus = cmbDocStatus.Text.Equals("Open") ? "O" : cmbDocStatus.Text.Equals("Closed") ? "C" : cmbDocStatus.Text.Equals("Cancelled") ? "N" : "";
                sStatus += encodeStatus;
            }));
            string sParams = sStatus + sUsedDeposit;
            string sResult = apic.loadData("api/deposit/get_all", sParams, "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
            {
                JObject joResponse = JObject.Parse(sResult);
                JArray jaData = (JArray)joResponse["data"];
                DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                if (dtData.Rows.Count > 0 && tabName.Equals("In Deposit"))
                {
                    dtData.Columns.Add("edit");
                    dtData.Columns.Add("cashout");
                }
                if (IsHandleCreated)
                {
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        if (dtData.Rows.Count > 0)
                        {
                            string[] columnVisible = new string[]
    {
                            "transdate", "reference","cust_code","branch","amount", "balance","remarks","sap_number","status","reference2","remarks","edit","cashout"
    };
                            dtData.SetColumnsOrder(columnVisible);
                            foreach (DataRow row in dtData.Rows)
                            {
                                if (dtData.Columns.Contains("status"))
                                {
                                    string decodeStatus = row["status"].ToString() == "O" ? "Open" : row["status"].ToString() == "C" ? "Closed" : row["status"].ToString() == "N" ? "Cancelled" : "";
                                    row["status"] = decodeStatus;
                                }
                            }
                        }
                        gridControl1.DataSource = null;

                        //DataTable dtCloned = dtData.Clone();
                        //if (dtCloned.Columns.Contains("transdate") || dtCloned.Columns.Contains("sap_date_updated"))
                        //{
                        //    dtCloned.Columns["transdate"].DataType = dtCloned.Columns["sap_date_updated"].DataType = typeof(DateTime);
                        //}
                        //else if (dtCloned.Columns.Contains("sap_number"))
                        //{
                        //    dtCloned.Columns["sap_number"].DataType = typeof(string);
                        //}
                        //foreach (DataRow row in dtData.Rows)
                        //{
                        //    dtCloned.ImportRow(row);
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
                            col.Caption = fieldName.Equals("whsecode") ? "From Whse" : col.Caption;
                            col.ColumnEdit = fieldName.Equals("edit") ? repositoryItemButtonEdit1 : fieldName.Equals("cashout") ? repositoryItemButtonEdit2 : fieldName.Equals("remarks") ? repositoryItemMemoEdit1 : repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("amount") || fieldName.Equals("balance") ? DevExpress.Utils.FormatType.Numeric : fieldName.Equals("transdate") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.Custom;
                            col.DisplayFormat.FormatString = fieldName.Equals("amount") || fieldName.Equals("balance") ? "n2" : fieldName.Equals("transdate") ? "yyyy-MM-dd HH:mm:ss" : "";
                            if (tabName.Equals("Summary Deposit"))
                            {
                                col.Visible = fieldName.Equals("cust_code") || fieldName.Equals("balance") || fieldName.Equals("reference2") || fieldName.Equals("remarks");
                            }
                            else
                            {
                                col.Visible = fieldName.Equals("transdate") || fieldName.Equals("reference") || fieldName.Equals("cust_code") || fieldName.Equals("branch") || fieldName.Equals("amount") || fieldName.Equals("balance") || fieldName.Equals("sap_number") || fieldName.Equals("status") || fieldName.Equals("reference2") || fieldName.Equals("remarks") || fieldName.Equals("edit") || fieldName.Equals("cashout");
                            }


                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }

                        //auto complete
                        string[] suggestions = tabName.Equals("Summary Deposit") ? new string[1] { "cust_code" } : new string[2] { "cust_code", "reference" };

                        string suggestConcat = string.Join(";", suggestions);
                        gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                        devc.loadSuggestion(gridView1, gridControl1, suggestions);
                        gridView1.BestFitColumns();
                        var col2 = gridView1.Columns["remarks"];
                        if (col2 != null)
                        {
                            col2.Width = 200;
                        }
                        gridView1.OptionsView.RowAutoHeight = true;
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

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            int id = 0, intTemp = 0;
            id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
            if (selectedColumnfieldName.Equals("edit"))
            {
                EditAdvancePayment.isSubmit = false;
                EditAdvancePayment editAdvancePayment = new EditAdvancePayment();
                editAdvancePayment.id = id;
                editAdvancePayment.ShowDialog();
                if (EditAdvancePayment.isSubmit)
                {
                    bg();
                }
            }
        }

        private void repositoryItemButtonEdit2_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            int id = 0, intTemp = 0;
            id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
            if (selectedColumnfieldName.Equals("cashout"))
            {
                AmountRemaks amountRemarks = new AmountRemaks();
                amountRemarks.ShowDialog();
                if (AmountRemaks.isSubmit)
                {
                    string remarks = "";
                    double amount = 0.00;
                    remarks = AmountRemaks.remarks;
                    amount = AmountRemaks.amount;
                    DialogResult dialogResult = MessageBox.Show("Are you sure you want to cashout?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        JObject body = new JObject();
                        body.Add("amount", amount);
                        body.Add("remarks", remarks);
                        apiPUT(body, "/api/deposit/cancel/" + id);
                    }
                }
            }
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
                    bool isSubmit = false;
                    var client = new RestClient(utilityc.URL);
                    client.Timeout = -1;
                    var request = new RestRequest(URL);
                    Console.WriteLine(URL);
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.Method = Method.PUT;

                    Console.WriteLine(body);
                    request.AddParameter("application/json", body, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    if (response.ErrorMessage == null)
                    {
                        JObject jObjectResponse = JObject.Parse(response.Content);

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

                        if (isSubmit)
                        {
                            bg();
                        }
                    }
                    else
                    {
                        MessageBox.Show(response.ErrorMessage, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;

            string custCode = gridView1.GetFocusedRowCellValue("cust_code").ToString();
            if (selectedColumnfieldName.Equals("cust_code") && tabName.Equals("Summary Deposit"))
            {
                SummaryDeposit_Details summaryDeposit_Details = new SummaryDeposit_Details();
                summaryDeposit_Details.lblCustomerCode.Text = custCode;
                summaryDeposit_Details.ShowDialog();
            }
            else if (selectedColumnfieldName.Equals("reference") && tabName.Equals("Used Deposit"))
            {
                int id = 0, intTemp = 0;
                id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
                ItemDeposit itemDeposit = new ItemDeposit(id);
                itemDeposit.ShowDialog();
            }
        }
        private int hotTrackRow = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        private int HotTrackRow
        {
            get
            {
                return hotTrackRow;
            }
            set
            {
                if (hotTrackRow != value)
                {
                    int prevHotTrackRow = hotTrackRow;
                    hotTrackRow = value;
                    gridView1.RefreshRow(prevHotTrackRow);
                    gridView1.RefreshRow(hotTrackRow);

                    if (hotTrackRow >= 0)
                        gridControl1.Cursor = Cursors.Hand;
                    else
                        gridControl1.Cursor = Cursors.Default;
                }
            }
        }
        private void gridView1_MouseMove(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(new Point(e.X, e.Y));

            if (info.InRowCell)
                HotTrackRow = info.RowHandle;
            else
                HotTrackRow = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        }

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.RowHandle == HotTrackRow)
                e.Appearance.BackColor = gridView1.PaintAppearance.SelectedRow.BackColor;
            else
                e.Appearance.BackColor = e.Appearance.BackColor;
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            AddAdvancePayment.isSubmit = false;
            AddAdvancePayment frm = new AddAdvancePayment();
            frm.ShowDialog();
            if (AddAdvancePayment.isSubmit)
            {
                bg();
            }
        }
    }


}
