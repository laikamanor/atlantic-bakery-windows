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
    public partial class CustomerLedger_Details2 : Form
    {
        public CustomerLedger_Details2(string custCode)
        {
            InitializeComponent();
            this.custCode = custCode;
        }
        string custCode = "";
        devexpress_class devc = new devexpress_class();
        api_class apic = new api_class();
        ui_class uic = new ui_class();
        private void CustomerLedger_Details2_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            dtFromDate.EditValue = DateTime.Now;
            dtToDate.EditValue = DateTime.Now;
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
            try
            {
                double doubleTemp = 0.00;
                string sFromDate = "?from_date=", sToDate = "&to_date=";
                dtFromDate.Invoke(new Action(delegate ()
                {
                    sFromDate += dtFromDate.Text;
                }));
                dtToDate.Invoke(new Action(delegate ()
                {
                    sToDate += dtToDate.Text;
                }));
                string sParams = custCode + sFromDate + sToDate;
                string sResult = apic.loadData("/api/report/customer/sales_summary/details/", sParams, "", "", Method.GET, true);
                if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
                {
                    double runningBalance = 0.00;
                    DateTime dtTemp = new DateTime();
                    JObject joResponse = JObject.Parse(sResult);
                    JObject joData = joResponse["data"] == null ? new JObject() : (JObject)joResponse["data"];
                    JObject joBalanceResult = joData["bal_result"].IsNullOrEmpty() ? new JObject() : JObject.Parse(joData["bal_result"].ToString());
                    double begBal = joBalanceResult["balance"].IsNullOrEmpty() ? doubleTemp : double.TryParse(joBalanceResult["balance"].ToString(), out doubleTemp) ? Convert.ToDouble(joBalanceResult["balance"].ToString()) : doubleTemp;
                    lblBalance.Invoke(new Action(delegate ()
                    {
                        lblBalance.Text = begBal.ToString("n2");
                        runningBalance = begBal;
                    }));
                    lblCustomerCode.Invoke(new Action(delegate ()
                    {
                        lblCustomerCode.Text = this.custCode;
                    }));



                    JArray jaTransRow = joData["row_result"] == null ? new JArray() : (JArray)joData["row_result"];
                    //lblToWhse.Text = jaTransRow[0]["to_whse"].ToString();
                    DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaTransRow.ToString(), (typeof(DataTable)));


                    DataTable dtCloned = new DataTable();
                    if (dtData.Rows.Count > 0)
                    {
                        dtData.Columns.Add("running_balance", typeof(double));
                        dtCloned = dtData.Clone();

                        foreach (DataRow row in dtData.Rows)
                        {
                            if (dtData.Columns.Contains("running_balance"))
                            {
                                double amtIn = row["amount_in"] == null ? doubleTemp : double.TryParse(row["amount_in"].ToString(), out doubleTemp) ? Convert.ToDouble(row["amount_in"].ToString()) : doubleTemp;
                                double amtOut = row["amount_out"] == null ? doubleTemp : double.TryParse(row["amount_out"].ToString(), out doubleTemp) ? Convert.ToDouble(row["amount_out"].ToString()) : doubleTemp;
                                runningBalance += amtIn;
                                runningBalance -= amtOut;
                                row["amount_in"] = amtIn <= 0 ? (object)DBNull.Value : amtIn;
                                row["amount_out"] = amtOut <= 0 ? (object)DBNull.Value : amtOut;
                                row["running_balance"] = runningBalance <= 0 ? 0.00 : runningBalance;
                            }
                            dtCloned.ImportRow(row);
                        }
                    }

                    gridControl1.Invoke(new Action(delegate ()
                    {
                        gridControl1.DataSource = null;
                    }));

                    string[] columnVisible = new string[]
                    {
                            "transdate", "reference", "reference2","transtype","amount_in","amount_out","running_balance","ar_num","ip_num","remarks"
                    };
                    dtCloned.SetColumnsOrder(columnVisible);

                    gridControl1.Invoke(new Action(delegate ()
                    {
                        gridControl1.DataSource = dtCloned;
                        gridView1.OptionsView.ColumnAutoWidth = false;
                        gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            string v = col.GetCaption();
                            string s = col.GetCaption().Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.Caption = fieldName.Equals("amount_in") ? "Sales" : fieldName.Equals("amount_out") ? "Payment" : col.Caption;
                            col.ColumnEdit = repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("amount_in") || fieldName.Equals("amount_out") || fieldName.Equals("running_balance") ? DevExpress.Utils.FormatType.Numeric : fieldName.Equals("transdate") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;
                            col.DisplayFormat.FormatString = fieldName.Equals("amount_in") || fieldName.Equals("amount_out") || fieldName.Equals("running_balance") ? "n2" : fieldName.Equals("transdate") ? "yyyy-MM-dd HH:mm:ss" : "";
                            col.Visible = !(fieldName.Equals("id"));

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
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void button1_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dtResult = new DataTable();
                dtResult.Columns.Add("transdate");
                dtResult.Columns.Add("reference");
                dtResult.Columns.Add("reference2");
                dtResult.Columns.Add("transtype");
                dtResult.Columns.Add("sales");
                dtResult.Columns.Add("payment");
                dtResult.Columns.Add("running_balance");
                dtResult.Columns.Add("remarks");
                dtResult.Columns.Add("beginning_balance");
                dtResult.Columns.Add("ar_num");
                dtResult.Columns.Add("ip_num");
                if (gridView1.DataRowCount > 0)
                {
                    double doubleTemp = 0.00;
                    for (int i = 0; i < gridView1.DataRowCount; i++)
                    {
                        string transdate = gridView1.GetRowCellValue(i, "transdate").ToString(),
                            reference = gridView1.GetRowCellValue(i, "reference").ToString(),
                            reference2 = gridView1.GetRowCellValue(i, "reference2").ToString(),
                            transtype = gridView1.GetRowCellValue(i, "transtype").ToString(),
                            sales = gridView1.GetRowCellValue(i, "amount_in") == null ? "" : double.TryParse(gridView1.GetRowCellValue(i, "amount_in").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(i, "amount_in").ToString()).ToString("n2") : "",
                            payment = gridView1.GetRowCellValue(i, "amount_out").ToString() == null ? "" : double.TryParse(gridView1.GetRowCellValue(i, "amount_out").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(i, "amount_out").ToString()).ToString("n2") : "",
                            runningBalance = gridView1.GetRowCellValue(i, "running_balance") == null ? "" : Convert.ToDouble(gridView1.GetRowCellValue(i, "running_balance").ToString()).ToString("n2"),
                            remarks = gridView1.GetRowCellValue(i, "remarks").ToString(),
                            arNumber = gridView1.GetRowCellValue(i, "ar_num").ToString(),
                            IpNumber = gridView1.GetRowCellValue(i, "ip_num").ToString(),
                            begBal = lblBalance.Text;
                        dtResult.Rows.Add(transdate, reference, reference2, transtype, sales, payment, runningBalance, remarks, begBal, arNumber, IpNumber);
                    }
                    CustomerLedger_CR rpt = new CustomerLedger_CR(dtResult, lblCustomerCode.Text);
                    rpt.ShowDialog();
                }
                else
                {
                    apic.showCustomMsgBox("Validation", "No data to print!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            int id = 0, baseID = 0, intTemp = 0;
            id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
            string transtype = gridView1.GetFocusedRowCellValue("transtype").ToString();
            string reference = gridView1.GetFocusedRowCellValue("reference").ToString();
            string remarks = gridView1.GetFocusedRowCellValue("remarks").ToString();
            string transdate = gridView1.GetFocusedRowCellValue("transdate").ToString();
            DateTime dtTemp = new DateTime();
            if (selectedColumnfieldName.Equals("reference") && transtype.Equals("Sales"))
            {
                SalesReportItems salesReportItems = new SalesReportItems();
                salesReportItems.URLDetails = "/api/sales/details/" + id.ToString();
                salesReportItems.Text = "Customer Ledger Details";
                salesReportItems.ShowDialog();
            }
            else if (selectedColumnfieldName.Equals("reference") && transtype.Equals("Payment"))
            {
                SalesPerCustomer_PaidDetails frm = new SalesPerCustomer_PaidDetails();
                frm.selectedID = id;
                frm.selectedReference = reference;
                frm.remarks = remarks;
                frm.dtTransDate = DateTime.TryParse(transdate, out dtTemp) ? Convert.ToDateTime(transdate) : new DateTime();
                frm.selectedCustCode = lblCustomerCode.Text;
                frm.ShowDialog();
            }
        }
    }
}
