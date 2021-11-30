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
using System.Globalization;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

namespace AB
{
    public partial class SummaryDeposit_Details : Form
    {
        public SummaryDeposit_Details()
        {
            InitializeComponent();
        }
        utility_class utilityc = new utility_class();
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        private void SummaryDeposit_Details_Load(object sender, EventArgs e)
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
                string sFromDate = "?from_date=", sToDate = "&to_date=", sCustCode = "&cust_code=";
                dtFromDate.Invoke(new Action(delegate ()
                {
                    sFromDate += dtFromDate.Text;
                }));
                dtToDate.Invoke(new Action(delegate ()
                {
                    sToDate += dtToDate.Text;
                }));
                lblCustomerCode.Invoke(new Action(delegate ()
                {
                    sCustCode += lblCustomerCode.Text;
                }));
                string sParams = sFromDate + sToDate + sCustCode;
                string sResult = apic.loadData("/api/deposit/summary/details", sParams, "", "", Method.GET, true);
                if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
                {
                    double runningBalance = 0.00;
                    DateTime dtTemp = new DateTime();
                    JObject joResponse = JObject.Parse(sResult);
                    JObject joData = joResponse["data"] == null ? new JObject() : (JObject)joResponse["data"];
                    JArray joBalanceResult = joData["balance"].IsNullOrEmpty() ? new JArray() : JArray.Parse(joData["balance"].ToString());
                    double begBal = joBalanceResult[0]["balance"].IsNullOrEmpty() ? doubleTemp : double.TryParse(joBalanceResult[0]["balance"].ToString(), out doubleTemp) ? Convert.ToDouble(joBalanceResult[0]["balance"].ToString()) : doubleTemp;
                    lblBalance.Invoke(new Action(delegate ()
                    {
                        lblBalance.Text = begBal.ToString("n2");
                        runningBalance = begBal;
                    }));



                    JArray jaTransRow = joData["details"] == null ? new JArray() : (JArray)joData["details"];
                    //lblToWhse.Text = jaTransRow[0]["to_whse"].ToString();
                    DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaTransRow.ToString(), (typeof(DataTable)));


                    DataTable dtCloned = new DataTable();
                    if (dtData.Rows.Count > 0)
                    {
                        dtData.Columns.Add("running_balance", typeof(double));
                        dtCloned = dtData.Clone();
                        if (dtCloned.Columns.Contains("dep_in"))
                        {
                            dtCloned.Columns["dep_in"].DataType = typeof(double);
                        }
                        if (dtCloned.Columns.Contains("dep_out"))
                        {
                            dtCloned.Columns["dep_out"].DataType = typeof(double);
                        }
                        

                        foreach (DataRow row in dtData.Rows)
                        {
                            if (dtData.Columns.Contains("running_balance"))
                            {
                                double depIn = row["dep_in"] == null ? doubleTemp : double.TryParse(row["dep_in"].ToString(), out doubleTemp) ? Convert.ToDouble(row["dep_in"].ToString()) : doubleTemp;
                                double depOut = row["dep_out"] == null ? doubleTemp : double.TryParse(row["dep_out"].ToString(), out doubleTemp) ? Convert.ToDouble(row["dep_out"].ToString()) : doubleTemp;
                                runningBalance += depIn;
                                runningBalance -= depOut;
                                row["dep_in"] = depIn <= 0 ? (object)DBNull.Value : depIn;
                                row["dep_out"] = depOut <= 0 ? (object)DBNull.Value : depOut;
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
                            "transdate", "ref1", "ref2","transtype","dep_in","dep_out","running_balance"
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
                            col.Caption = fieldName.Equals("dep_in") ? "Deposit In" : fieldName.Equals("dep_out") ? "Deposit Out" : fieldName.Equals("ref1") ? "Reference" : fieldName.Equals("ref2") ? "Reference2" : col.Caption;
                            col.ColumnEdit = repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("dep_in") || fieldName.Equals("dep_out") || fieldName.Equals("running_balance") ? DevExpress.Utils.FormatType.Numeric : fieldName.Equals("transdate") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;
                            col.DisplayFormat.FormatString = fieldName.Equals("dep_in") || fieldName.Equals("dep_out") || fieldName.Equals("running_balance") ? "n2" : fieldName.Equals("transdate") ? "yyyy-MM-dd HH:mm:ss" : "";
                            col.Visible = !(fieldName.Equals("id"));
                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }
                        //auto complete
                        string[] suggestions = { "ref1" };
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


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bg();
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

        private void gridView1_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            if (e.RowHandle == HotTrackRow)
                e.Appearance.BackColor = gridView1.PaintAppearance.SelectedRow.BackColor;
            else
                e.Appearance.BackColor = e.Appearance.BackColor;
        }
    }
}
