using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AB.API_Class.Branch;
using RestSharp;
using AB.UI_Class;
using Newtonsoft.Json;
using DevExpress.XtraGrid.Columns;
using System.Globalization;
using AB.API_Class.Reports;
using DevExpress.XtraGrid.Views.Grid;

namespace AB
{
    public partial class salesAmountSummaryPrintedReport : Form
    {
        public salesAmountSummaryPrintedReport()
        {
            InitializeComponent();
        }
        utility_class utilityc = new utility_class();
        branch_class branchc = new branch_class();
        public static DataTable dtSelectedBranches = new DataTable();
        DataTable dtBranch = new DataTable();
        private async void salesAmountSummaryPrintedReport_Load(object sender, EventArgs e)
        {
            dtSelectedBranches.Columns.Clear();
            dtSelectedBranches.Columns.Add("code");
            dtSelectedBranches.Columns.Add("branch");
            if (!backgroundWorker1.IsBusy)
            {
                closeForm();
                Loading frm = new Loading();
                frm.Show();
                backgroundWorker1.RunWorkerAsync();
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

                    string branches = "";
                    int count = 0;
                    for (int i = 0; i < dgvSelectedBranch.Rows.Count; i++)
                    {
                        branches = branches + "," + dgvSelectedBranch.Rows[i].Cells["code"].Value.ToString();
                        count += 1;
                    }

                    lblSelectedBranches.Invoke(new Action(delegate ()
                {
                    lblSelectedBranches.Text = "Selected Branches: " + (dgvSelectedBranch.Rows.Count == dtBranch.Rows.Count || dgvSelectedBranch.Rows.Count <= 0 ? "All" : count.ToString("N0"));
                }));
                    branches = (string.IsNullOrEmpty(branches) ? "" : branches.Substring(1));

                    string sBranch = "", sBranchText = "";
                    var client = new RestClient(utilityc.URL);
                    client.Timeout = -1;
                    var request = new RestRequest("/api/report/daily_printed?branch=%5B" + branches + "%5D&transdate=" + (checkTransDate.Checked ? dtTransDate.Text : ""));
                    //Console.WriteLine("/api/report/daily_printed?branch=" + sBranch + "&transdate=" + (checkDate.Checked ? dtTransDate.Text : ""));
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.Method = Method.GET;
                    var response = client.Execute(request);
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
                            gridControl1.Invoke(new Action(delegate ()
                            {
                                gridView1.Columns.Clear();
                                gridControl1.DataSource = null;
                                DataTable dt = (DataTable)JsonConvert.DeserializeObject(data, (typeof(DataTable)));
                                gridControl1.DataSource = dt;
                                if (gridControl1.DataSource != null)
                                {
                                    gridView1.OptionsView.ShowFooter = true;
                                    gridView1.Columns["actual_cash"].Summary.Clear();
                                    gridView1.Columns["total_cash_on_hand"].Summary.Clear();
                                    gridView1.Columns["cash_variance"].Summary.Clear();
                                    gridView1.Columns["total_gross"].Summary.Clear();
                                    gridView1.Columns["total_discount_amount"].Summary.Clear();
                                    gridView1.Columns["total_net_sales"].Summary.Clear();
                                    gridView1.Columns["inv_shortage_value"].Summary.Clear();
                                    gridView1.Columns["inv_overage_value"].Summary.Clear();
                                    gridView1.Columns["inv_total_overage_value"].Summary.Clear();
                                    gridView1.Columns["total_charge"].Summary.Clear();
                                    gridView1.Columns["actual_cash"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "actual_cash", "Total Actual Cash: {0:n2}");
                                    gridView1.Columns["total_cash_on_hand"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "total_cash_on_hand", "Total Cash On Hand: {0:n2}");
                                    gridView1.Columns["cash_variance"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "cash_variance", "Total Cash Variance: {0:n2}");
                                    gridView1.Columns["total_gross"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "total_gross", "Total Gross: {0:n2}");
                                    gridView1.Columns["total_discount_amount"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "total_discount_amount", "Total Discount Amount: {0:n2}");
                                    gridView1.Columns["total_net_sales"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "total_net_sales", "Total Net Sales: {0:n2}");
                                    gridView1.Columns["inv_shortage_value"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "inv_shortage_value", "Inv. Net Shortage Value: {0:n2}");
                                    gridView1.Columns["inv_overage_value"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "inv_overage_value", "Inv. Net Overage Value: {0:n2}");
                                    gridView1.Columns["inv_total_overage_value"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "inv_total_overage_value", "Inv. Net Total Overage Value: {0:n2}");
                                    gridView1.Columns["total_charge"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "total_charge", "Total Charge: {0:n2}");


                                    foreach (GridColumn col in gridView1.Columns)
                                    {
                                        col.Width = 100;
                                        string v = col.GetCaption();
                                        string s = col.GetCaption().Replace("_", " ");
                                        col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());

                                        col.Visible = v.Equals("final_count_reference") ? false : true;

                                        col.DisplayFormat.FormatType = v.Equals("transdate") || v.Equals("branch") || v.Equals("finalcount_reference") || v.Equals("id") ? DevExpress.Utils.FormatType.None : DevExpress.Utils.FormatType.Numeric;

                                        col.DisplayFormat.FormatString = v.Equals("branch") || v.Equals("final_count_reference") || v.Equals("id") ? ""  : v.Equals("transdate") || v.Equals("date_printed") ? "yyyy-MM-dd HH:mm" : "n2";
                                        //if (col.AbsoluteIndex >= 9)
                                        //{
                                        //    col.Visible = false;
                                        //}
                                        //else if(col.AbsoluteIndex >= 17)
                                        //{
                                        //    col.Visible = false;
                                        //}

                                        if (col.AbsoluteIndex == 4 || col.AbsoluteIndex == 5 || col.AbsoluteIndex == 9 || col.AbsoluteIndex == 10 || col.AbsoluteIndex == 11 || col.AbsoluteIndex == 12 || col.AbsoluteIndex == 13 || col.AbsoluteIndex == 14 || col.AbsoluteIndex == 15 || col.AbsoluteIndex == 16 || col.AbsoluteIndex == 17)
                                        {
                                            col.Visible = false;
                                        }

                                        if (v.Equals("actual_cash"))
                                        {
                                            col.VisibleIndex = gridView1.Columns["branch"].VisibleIndex + 1;
                                        }
                                        else if (v.Equals("cash_variance"))
                                        {
                                            col.VisibleIndex = gridView1.Columns["total_cash_on_hand"].VisibleIndex + 1;
                                        }
                                        else if (v.Equals("total_gross"))
                                        {
                                            col.VisibleIndex = gridView1.Columns["cash_variance"].VisibleIndex + 1;
                                        }
                                        else if (v.Equals("total_discount_amount"))
                                        {
                                            col.VisibleIndex = gridView1.Columns["total_gross"].VisibleIndex + 1;
                                        }
                                        else if (v.Equals("total_net_sales"))
                                        {
                                            col.VisibleIndex = gridView1.Columns["total_discount_amount"].VisibleIndex + 1;
                                        }
                                        col.Width = v.Equals("transdate") || v.Equals("branch") || v.Equals("final_count_reference") || v.Equals("date_printed") ? 180 : 100;
                                        col.ColumnEdit = repositoryItemTextEdit1;

                                        FontFamily fontArial = new FontFamily("Arial");
                                        col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                                        col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                                    }
                                    if (gridView1.RowCount > 0)
                                    {
                                        //GridColumn myCol = new GridColumn() { Caption = "View Payment Method", Visible = true, FieldName = "Action" };
                                        //gridView1.Columns.Add(myCol);
                                        //(gridControl1.MainView as GridView).Columns["Action"].Width = 120;
                                        //(gridControl1.MainView as GridView).Columns["Action"].ColumnEdit = repositoryItemButtonEdit1;

                                        GridColumn myCol2 = new GridColumn() { Caption = "View Sales and Inventory Report", Visible = true, FieldName = "Action2" };
                                        gridView1.Columns.Add(myCol2);
                                        (gridControl1.MainView as GridView).Columns["Action2"].Width = 180;
                                        (gridControl1.MainView as GridView).Columns["Action2"].ColumnEdit = repositoryItemButtonEdit2;

                                        GridColumn myCol4 = new GridColumn() { Caption = "View Inventory Count Summary", Visible = true, FieldName = "Action4" };
                                        gridView1.Columns.Add(myCol4);
                                        (gridControl1.MainView as GridView).Columns["Action4"].Width = 200;
                                        (gridControl1.MainView as GridView).Columns["Action4"].ColumnEdit = repositoryItemButtonEdit4;


                                        //GridColumn myCol3 = new GridColumn() { Caption = "View Sales Report w/o Inventory", Visible = true, FieldName = "Action3" };
                                        //gridView1.Columns.Add(myCol3);
                                        //(gridControl1.MainView as GridView).Columns["Action3"].Width = 200;
                                        //(gridControl1.MainView as GridView).Columns["Action3"].ColumnEdit = repositoryItemButtonEdit3;

                                        int haveId = 0, haveNoId = 0, branchCount = 0;
                                        for (int i = 0; i < gridView1.DataRowCount; i++)
                                        {
                                            branchCount += 1;
                                            if(gridView1.GetRowCellValue(i, "id") == DBNull.Value)
                                            {
                                                haveNoId += 1;
                                            }
                                            else
                                            {
                                                if (Convert.ToInt32(gridView1.GetRowCellValue(i, "id")) > 0)
                                                {
                                                    haveId += 1;
                                                }
                                                else
                                                {
                                                    haveNoId += 1;
                                                }
                                            }
                                        }
                                        gridView1.BestFitColumns();
                                        lblBranchPrinted.Invoke(new Action(delegate ()
                                        {
                                            lblBranchPrinted.Text = "Branches that have already printed: " + haveId.ToString("N0");
                                        }));
                                        lblBranchNotPrinted.Invoke(new Action(delegate ()
                                        {
                                            lblBranchNotPrinted.Text = "Branches that have not yet been printed: " + haveNoId.ToString("N0");
                                        }));
                                        lblBranches.Invoke(new Action(delegate ()
                                        {
                                            lblBranches.Text = "Branches: " + branchCount.ToString("N0");
                                        }));
                                    }

                                }
                            }));
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
        private void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            int id = 0, intTemp = 0;
            string vID = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "id").ToString();
            printedDetails frm = new printedDetails();
            frm.url = "/api/report/printed/payment_method/";
            frm.selectedID = Int32.TryParse(vID, out intTemp) ? Convert.ToInt32(vID) : intTemp;
            frm.Text = "Payment Method - " + gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch").ToString() + " - " + gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "transdate").ToString();
            frm.ShowDialog();
        }

        private void gridView1_RowCellClick(object sender, RowCellClickEventArgs e)
        {
            if(e.Column.GetCaption().Equals("Total Discount Amount") || e.Column.GetCaption().Equals("Total Charge"))
            {
                string vID = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "id").ToString();
                int id = 0, intTemp = 0;

                printedDetails frm = new printedDetails();
                frm.url = findURL(e.Column.GetCaption());
                frm.selectedID = Int32.TryParse(vID, out intTemp) ? Convert.ToInt32(vID) : intTemp;
                frm.Text = e.Column.GetCaption() + " - " + gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch").ToString() + " - " + gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "transdate").ToString();
                frm.ShowDialog();
            }
        }

        public string findURL(string columnName)
        {
            if (columnName.Contains("Discount"))
            {
                return "/api/report/printed/discount/";
            }
            else if (columnName.Contains("Charge"))
            {
                return "/api/report/printed/charged/";
            }
            return "";
        }

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView currentView = sender as GridView;
            if (e.Column.FieldName == "cash_variance")
            {
                double value = !Convert.IsDBNull(currentView.GetRowCellValue(e.RowHandle, "cash_variance")) ? Convert.ToDouble(currentView.GetRowCellValue(e.RowHandle, "cash_variance")) : 0.00;
                e.Appearance.BackColor = value > 0 ? Color.FromArgb(110, 119, 255) : value == 0 ? Color.White : Color.FromArgb(255, 110, 110);
            }
            else if (e.Column.FieldName == "total_discount_amount")
            {
                double value = !Convert.IsDBNull(currentView.GetRowCellValue(e.RowHandle, "total_discount_amount")) ? Convert.ToDouble(currentView.GetRowCellValue(e.RowHandle, "total_discount_amount")) : 0.00;

                if (value > 0)
                {
                    e.Appearance.BackColor = Color.FromArgb(226, 255, 110);
                }
            }
            else if (e.Column.FieldName == "inv_shortage_value")
            {
                double value = !Convert.IsDBNull(currentView.GetRowCellValue(e.RowHandle, "inv_shortage_value")) ? Convert.ToDouble(currentView.GetRowCellValue(e.RowHandle, "inv_shortage_value")) : 0.00;

                if (value != 0)
                {
                    e.Appearance.BackColor = Color.FromArgb(110, 255, 112);
                }
            }
            else if (e.Column.FieldName == "total_charge")
            {
                double value = !Convert.IsDBNull(currentView.GetRowCellValue(e.RowHandle, "total_charge")) ? Convert.ToDouble(currentView.GetRowCellValue(e.RowHandle, "total_charge")) : 0.00;

                e.Appearance.BackColor = value > 0 ? Color.FromArgb(255, 173, 110) : Color.White;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                loadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //closeForm();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void checkTransDate_CheckedChanged(object sender, EventArgs e)
        {
            label1.Visible = dtTransDate.Visible = checkTransDate.Checked;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                closeForm();
                Loading frm = new Loading();
                frm.Show();
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private async void btnSelectBranch_Click(object sender, EventArgs e)
        {
            if (dtBranch.Rows.Count <= 0)
            {
                dtBranch = await branchc.returnBranches();
            }
            SelectBranch frm = new SelectBranch("Printed");
            frm.dt = dtBranch;
            frm.dtSelected = dtSelectedBranches;
            frm.ShowDialog();

            dtSelectedBranches.DefaultView.Sort = "branch ASC";
            dtSelectedBranches = dtSelectedBranches.DefaultView.ToTable();

            dgvSelectedBranch.DataSource = dtSelectedBranches;
            dgvSelectedBranch.Columns["branch"].HeaderText = "Branch";
            dgvSelectedBranch.Columns["branch"].ReadOnly = true;
            dgvSelectedBranch.Columns["code"].Visible = false;
        }

        private async void repositoryItemButtonEdit2_Click(object sender, EventArgs e)
        {

            string transdate = !Convert.IsDBNull(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "transdate").ToString()) ? gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "transdate").ToString() : "";
            string branch = !Convert.IsDBNull(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch").ToString()) ? gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch").ToString() : "";
            string branchName = "";

            if (dtBranch.Rows.Count <= 0)
            {
                dtBranch = await branchc.returnBranches();
            }
            foreach (DataRow row in dtBranch.Rows)
            {
                if(row["code"].ToString() == branch)
                {
                    branchName = row["name"].ToString();
                    break;
                }
            }
            DateTime dtTemp = new DateTime();
            if (!DateTime.TryParse(transdate.Trim(), out dtTemp))
            {
                MessageBox.Show("No data found!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                reportsDialog frm = new reportsDialog("Final Report", Convert.ToDateTime(transdate).ToString("MM/dd/yyyy"), branch, branchName, ".");
                frm.ShowDialog();
            }
        }

        private async void repositoryItemButtonEdit3_Click(object sender, EventArgs e)
        {
            string transdate = !Convert.IsDBNull(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "transdate").ToString()) ? gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "transdate").ToString() : "";
            string branch = !Convert.IsDBNull(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch").ToString()) ? gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch").ToString() : "";
            string branchName = "";

            if (dtBranch.Rows.Count <= 0)
            {
                dtBranch = await branchc.returnBranches();
            }
            foreach (DataRow row in dtBranch.Rows)
            {
                if (row["code"].ToString() == branch)
                {
                    branchName = row["name"].ToString();
                    break;
                }
            }
            DateTime dtTemp = new DateTime();
            if (!DateTime.TryParse(transdate.Trim(), out dtTemp))
            {
                MessageBox.Show("No data found!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                reportsDialog frm = new reportsDialog("Final Sales Report", Convert.ToDateTime(transdate).ToString("MM/dd/yyyy"), branch, branchName, ".");
                frm.ShowDialog();
            }
        }

        private async void repositoryItemButtonEdit4_Click(object sender, EventArgs e)
        {
            string transdate = !Convert.IsDBNull(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "transdate").ToString()) ? gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "transdate").ToString() : "";
            string branch = !Convert.IsDBNull(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch").ToString()) ? gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch").ToString() : "";
            string branchName = "";

            if (dtBranch.Rows.Count <= 0)
            {
                dtBranch = await branchc.returnBranches();
            }
            foreach (DataRow row in dtBranch.Rows)
            {
                if (row["code"].ToString() == branch)
                {
                    branchName = row["name"].ToString();
                    break;
                }
            }
            DateTime dtTemp = new DateTime();
            if (!DateTime.TryParse(transdate.Trim(), out dtTemp))
            {
                MessageBox.Show("No data found!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                reportsDialog frm = new reportsDialog("Final Count Report", Convert.ToDateTime(transdate).ToString("MM/dd/yyyy"), branch, branchName, ".");
                frm.ShowDialog();
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
