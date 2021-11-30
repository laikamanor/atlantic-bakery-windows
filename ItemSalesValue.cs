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
using AB.API_Class.Warehouse;
using Newtonsoft.Json.Linq;
using RestSharp;
using AB.UI_Class;
using DevExpress.XtraGrid.Columns;
using System.Globalization;
using DevExpress.XtraGrid.Views.Grid;
using Newtonsoft.Json;
using System.Web;
using DevExpress.XtraGrid;

namespace AB
{
    public partial class ItemSalesValue : Form
    {
        public ItemSalesValue(DataTable dt1, DataTable dt, DataTable dt2)
        {
            dtOriginal = dt1;
            dtGlobal = dt;
            dtSelectedBranches = dt2;
            InitializeComponent();
        }
        warehouse_class warehousec = new warehouse_class();
        api_class apic = new api_class();
        branch_class branchc = new branch_class();
        utility_class utilityc = new utility_class();
        DataTable dtBranch = new DataTable(), dtWhse = new DataTable();

        DataTable dtOriginal = new DataTable();
        DataTable dtGlobal = new DataTable();
        DataTable dtSelectedBranches = new DataTable();
        double globalTotalNetAmount = 0.00;
        private void gridView1_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (e.RowHandle >= 0)
            {
                var myColumn = gridView1.Columns.FirstOrDefault((col) => col.FieldName == "disc_amount");
                if(myColumn != null)
                {
                    double doubleTemp = 0.00;
                    double discAmt = double.TryParse(gridView1.GetRowCellValue(e.RowHandle, "disc_amount").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(e.RowHandle, "disc_amount").ToString()) : doubleTemp;
                    if (discAmt > 0)
                    {
                        e.Appearance.BackColor = Color.Yellow;
                    }
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


        public DataTable loadSelected(DataTable dtSelected, string type)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("select", typeof(bool));
            dt.Columns.Add("name", typeof(string));
            foreach (DataRow row in dtSelected.Rows)
            {
                dt.Rows.Add(false, row[type.Equals("Branch") ? "code" : "item_code"].ToString());
            }
            return dt;
        }

        public string getSelected(string type)
        {
            string sResult = "";
            foreach (DataRow row in dtSelectedBranches.Rows)
            {
                if (row["type"].ToString().Equals(type))
                {
                    sResult += "'" + row["name"].ToString() + "'" + ",";
                }
            }
            sResult = string.IsNullOrEmpty(sResult.Trim()) ? "" : sResult.Substring(0, sResult.Length - 1);
            return sResult;
        }

        public void showFilter(string type, DataTable dt)
        {
            customFiltering frm = new customFiltering();
            frm.Tag = this.Name;
            frm.Text = type;
            frm.dt = loadSelected(dt, type);
            frm.ShowDialog();
            string sResult = getSelected(type);
            loadPopUpValues("Close", "");
        }

        public void loadPopUpValues(string status, string itemCode)
        {
            DataTable dt = dtGlobal;
            string sBranchSelected = "";
            string sItemSelected = "";
            bool isAllBranch = false, isAllItem = false, boolTemp = false;
            foreach (DataRow row in dtSelectedBranches.Rows)
            {
                //MessageBox.Show(row["type"].ToString());
                if (row["type"].ToString().Equals("Branch"))
                {
                    sBranchSelected += "'" + row["name"].ToString() + "'" + ",";
                    isAllBranch = bool.TryParse(row["is_all"].ToString(), out boolTemp) ? Convert.ToBoolean(row["is_all"].ToString()) : boolTemp;
                }
            }
            sBranchSelected = string.IsNullOrEmpty(sBranchSelected.Trim()) ? "" : sBranchSelected.Substring(0, sBranchSelected.Length - 1);
            foreach (DataRow row in dtSelectedBranches.Rows)
            {
                //MessageBox.Show(row["type"].ToString());
                if (row["type"].ToString().Equals("Item"))
                {
                    string s = row["name"].ToString().Replace(@"'", "''");
                    sItemSelected += "'" + s + "'" + ",";
                    isAllItem = bool.TryParse(row["is_all"].ToString(), out boolTemp) ? Convert.ToBoolean(row["is_all"].ToString()) : boolTemp;
                }
            }
            sItemSelected = string.IsNullOrEmpty(sItemSelected.Trim()) ? "" : sItemSelected.Substring(0, sItemSelected.Length - 1);
            labelSelectedBranch.Text = "Branch [" + (string.IsNullOrEmpty(sBranchSelected.Trim()) || isAllBranch ? "All" : sBranchSelected) + "]";
            labelItem.Text = "Item [" + (string.IsNullOrEmpty(sItemSelected.Trim()) || isAllItem ? "All" : sItemSelected) + "]";
            //DataView view = new DataView(dt);
            //DataTable distinctValues = view.ToTable(true, "select", "name");

            //int selectedBranch = dtSelectedBranches.Select("type = 'Branch' AND select=true").Length;
            //int Branch = distinctValues.Rows.Count;

            if (sBranchSelected == "")
            {
                sBranchSelected = "";
            }
            else
            {
                sBranchSelected = "branch IN (" + sBranchSelected + ")";
            }

            if (sBranchSelected == "")
            {
                if (sItemSelected != "")
                {
                    sItemSelected = "item_code IN (" + sItemSelected + ")";
                }

            }
            else
            {
                if (sItemSelected != "")
                {
                    sItemSelected = "AND item_code IN (" + sItemSelected + ")";
                }
            }
            string appendParams = sBranchSelected + sItemSelected;
            Console.WriteLine("append: " + appendParams);
            if (dt.Rows.Count > 0)
            {
                if (dtSelectedBranches.Rows.Count > 0)
                {
                    DataRow[] results = dt.Select(appendParams);
                    Console.WriteLine("appendsssssssss: " + appendParams);
                    string aaa = appendParams;
                    DataTable dtt = new DataTable();
                    if (results.Any())
                    {
                        dtt = results.CopyToDataTable();
                        dtt = loadMe(dtt, itemCode, status);
                        loadSettings(dtt);
                    }
                    else
                    {
                        DataTable dt2 = new DataTable();
                        loadSettings(dt2);
                    }
                }
                else
                {
                    dt = loadMe(dt, itemCode, status);
                    loadSettings(dt);
                }
            }
            else
            {
                DataTable dt2 = new DataTable();
                loadSettings(dt2);
            }
        }

        public DataTable loadMe(DataTable dt, string item_code, string tagged)
        {
            double doubleTemp = 0.00;
            var query = (from row in dt.AsEnumerable()
                         group row by new
                         {
                             ItemCode = row.Field<string>("item_code"),
                         } into grp
                         select new
                         {
                             ItemCode = grp.Key.ItemCode,
                             AllTotalQuantity = grp.Sum(r => r.Field<double?>("total_qty") == null ? 0 : r.Field<double>("total_qty")),
                             TotalQuantityAsPerSelected = grp.Sum(r => r.Field<double?>("quantity") == null ? 0 : r.Field<double>("quantity")),
                             TotalNetAmountAsPerSelected = grp.Sum(r => r.Field<double?>("net_amount") == null ? 0 : r.Field<double>("net_amount")),
                         }).ToList();
            DataTable dt2 = new DataTable();
            dt2.Columns.Add("item_code", typeof(string));
            dt2.Columns.Add("total", typeof(double));
            dt2.Columns.Add("quantity_per_selected", typeof(double));
            dt2.Columns.Add("net_amount_per_selected", typeof(double));
            foreach (var q in query)
            {
                dt2.Rows.Add(q.ItemCode, q.AllTotalQuantity, q.TotalQuantityAsPerSelected,q.TotalNetAmountAsPerSelected);
            }
            var j = (from row in dt2.AsEnumerable()
                     join row2 in dt.AsEnumerable() on row.Field<string>("item_code") equals row2.Field<string>("item_code")
                     join row3 in dtOriginal.AsEnumerable() on row.Field<string>("item_code") equals row3.Field<string>("item_code")
                     into grp
                     select new
                     {
                         ItemCode = row.Field<string>("item_code"),
                         AllTotalQuantity = grp.Sum(r => r.Field<double>("total_qty")),
                         QuantityPerSelected = row.Field<double>("quantity_per_selected"),
                         NetAmountPerSelected = row.Field<double>("net_amount_per_selected"),
                         Quantity = row2.Field<double>("quantity"),
                         UnitPrice = row2.Field<double>("unit_price"),
                         //DiscPrcnt = row2.Field<double>("discprcnt"),
                         DiscAmount = row2.Field<double>("disc_amount"),
                         GrossAmount = row2.Field<double>("gross_amount"),
                         OriginalNetAmount = grp.Sum(r=> r.Field<double>("net_amount")),
                         NetAmount = row2.Field<double>("net_amount"),
                         Branch = row2.Field<string>("branch")
                     }).ToList();
            DataTable dt3 = new DataTable();
            dt3.Columns.Add("item_code", typeof(string));
            dt3.Columns.Add("all_total_quantity", typeof(double));
            dt3.Columns.Add("total_quantity_as_per_selected_branch", typeof(double));
            if (tagged.Equals("Open"))
            {
                dt3.Columns.Add("quantity_per_branch", typeof(double));
                dt3.Columns.Add("branch", typeof(string));
                dt3.Columns.Add("unit_price", typeof(double));
                dt3.Columns.Add("disc_amount", typeof(double));
                dt3.Columns.Add("gross_amount", typeof(double));
                dt3.Columns.Add("net_amount_per_branch", typeof(double));
                dt3.Columns.Add("total_net_amount_as_per_selected_branch", typeof(double));
                dt3.Columns.Add("percentage", typeof(double));
            }
            else
            {
                dt3.Columns.Add("total_net_amount", typeof(double));
                dt3.Columns.Add("total_net_amount_as_per_selected_branch", typeof(double));
                dt3.Columns.Add("percentage", typeof(double));
            }
            //else
            //{
            //    dt3.Columns.Add("percentage", typeof(double));
            //}
            string item = "", itemm = "";
            if (tagged.Equals("Close"))
            {
                foreach (var q in j)
                {
                    if (!tagged.Equals("Open"))
                    {
                        globalTotalNetAmount += q.NetAmount;
                    }
                    item = q.ItemCode;
                }
            }

            foreach (var q in j)
            {
                if (tagged.Equals("Open"))
                {
                    if (q.ItemCode == item_code)
                    {
                        if (!item.Equals(q.ItemCode))
                        {
                            dt3.Rows.Add(q.ItemCode,  q.AllTotalQuantity, q.QuantityPerSelected, (double?)null, "", (double?)null, (double?)null, (double?)null, (double?)null,(double?)null, (double?)null);
                        }

                        //dt3.Rows.Add( (double?)null, (double?)null, (double?)null, q.Quantity, q.Branch, q.UnitPrice, q.DiscPrcnt == null ? 0.00 : q.DiscPrcnt, q.DiscAmount == null ? 0.00 : q.DiscAmount, q.GrossAmount == null ? 0.00 : q.GrossAmount, q.NetAmount == null ? 0.00 : q.NetAmount);

                        double percent = (q.NetAmount / q.NetAmountPerSelected) * 100;

                        dt3.Rows.Add((double?)null, (double?)null, (double?)null, q.Quantity, q.Branch, q.UnitPrice, q.DiscAmount == null ? 0.00 : q.DiscAmount, q.GrossAmount == null ? 0.00 : q.GrossAmount,q.NetAmount,q.NetAmountPerSelected, percent);
                    }
                }
                else
                {
                    if (!item.Equals(q.ItemCode))
                    {
                        double percent = (q.NetAmountPerSelected / globalTotalNetAmount) * 100;
                        dt3.Rows.Add(q.ItemCode, q.AllTotalQuantity == null ? 0.00 : q.AllTotalQuantity, q.QuantityPerSelected == null ? 0.00 : q.QuantityPerSelected, q.OriginalNetAmount, q.NetAmountPerSelected, percent);
                    }
                }
                item = q.ItemCode;
            }
            if (tagged.Equals("Open"))
            {
                DataView view = new DataView(dt3);
                dt3 = view.ToTable(true, "item_code", "all_total_quantity", "total_quantity_as_per_selected_branch", "quantity_per_branch", "branch", "unit_price", "disc_amount", "gross_amount", "net_amount_per_branch", "total_net_amount_as_per_selected_branch", "percentage");
                //            Console.WriteLine("sum " + sum);
                //            DataTable dt4 = new DataTable();
                //            dt4.Columns.Add("item_code", typeof(string));
                //            dt4.Columns.Add("all_total_quantity", typeof(double));
                //            dt4.Columns.Add("total_quantity_as_per_selected_branch", typeof(double));
                //            dt4.Columns.Add("quantity_per_branch", typeof(double));
                //            dt4.Columns.Add("branch", typeof(string));
                //            dt4.Columns.Add("unit_price", typeof(double));
                //            dt4.Columns.Add("disc_amount", typeof(double));
                //            dt4.Columns.Add("gross_amount", typeof(double));
                //            dt4.Columns.Add("net_amount", typeof(double));
                //            dt4.Columns.Add("total_net_amount", typeof(double));
                //            dt4.Columns.Add("net_amount_per_selected", typeof(double));

                //            foreach (DataRow row in dt3.Rows)
                //            {
                //                dt4.Rows.Add(row["item_code"], row["all_total_quantity"],
                //row["total_quantity_as_per_selected_branch"],
                //row["quantity_per_branch"], row["branch"],
                //row["unit_price"],
                //row["disc_amount"],
                //row["gross_amount"],
                //row["net_amount"],
                //sum,
                // row["net_amount_per_selected"]);
                //            }
                //            dt3 = dt4;
            }

            return dt3;
        }

        public void loadSettings(DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
                gridControl1.DataSource = dt;
                long avg = 0, longTemp = 0;
                foreach (GridColumn col in gridView1.Columns)
                {
                    string v = col.GetCaption();
                    string s = col.GetCaption().Replace("_", " ");
                    col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                    col.ColumnEdit = col.FieldName.Equals("total_quantity_as_per_selected_branch") ? repositoryItemTextEdit2 : repositoryItemTextEdit1;
                    //col.OptionsColumn.AllowSort = v.Equals("item_code") ? DevExpress.Utils.DefaultBoolean.True : DevExpress.Utils.DefaultBoolean.Default;
                    //col.SortMode = v.Equals("item_code") ? DevExpress.XtraGrid.ColumnSortMode.Custom : DevExpress.XtraGrid.ColumnSortMode.Default;
                    col.DisplayFormat.FormatType =v.Equals("item_code") ? DevExpress.Utils.FormatType.None : DevExpress.Utils.FormatType.Numeric;
                    col.DisplayFormat.FormatString =  v.Equals("item_code") ? "" : "n2";

                    //fonts
                    FontFamily fontArial = new FontFamily("Arial");
                    col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                    col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                }
                gridView1.BestFitColumns();
                if (dt.Rows.Count > 0)
                {

                    //gridView1.ClearSorting();
                    //gridView1.Columns["item_code"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;

                    gridView1.OptionsView.ShowFooter = true;


                    var myColumn = gridView1.Columns.FirstOrDefault((col) => col.GetCaption() == "Unit Price");
                    if(myColumn != null)
                    {

                        gridView1.Columns["branch"].Summary.Clear();
                        gridView1.Columns["branch"].Summary.Add(DevExpress.Data.SummaryItemType.Custom, "branch", "Total: " + (gridView1.RowCount-1).ToString("N0"));

                        gridView1.Columns["quantity_per_branch"].Summary.Clear();
                        gridView1.Columns["quantity_per_branch"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "quantity_per_branch", "Total Quantity Per Branch: {0:n2}");

                        gridView1.Columns["disc_amount"].Summary.Clear();
                        gridView1.Columns["gross_amount"].Summary.Clear();
                        gridView1.Columns["net_amount_per_branch"].Summary.Clear();
                        gridView1.Columns["disc_amount"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "disc_amount", "Total Disc. Amount: {0:N0}");
                        gridView1.Columns["gross_amount"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "gross_amount", "Total Gross Amount: {0:N0}");
                        gridView1.Columns["net_amount_per_branch"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "net_amount_per_branch", "Total Net Amount Per Branch: {0:N0}");
                        gridView1.BestFitColumns();
                    }
                    else
                    {
                        gridView1.Columns["item_code"].Summary.Clear();
                        gridView1.Columns["item_code"].Summary.Add(DevExpress.Data.SummaryItemType.Count, "item_code", "Total: {0:N0}");

                        gridView1.Columns["all_total_quantity"].Summary.Clear();
                        gridView1.Columns["all_total_quantity"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "all_total_quantity", "Total Quantity: {0:n2}");

                        gridView1.Columns["total_quantity_as_per_selected_branch"].Summary.Clear();
                        gridView1.Columns["total_quantity_as_per_selected_branch"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "total_quantity_as_per_selected_branch", "Total Quantity As Per Selected Branch: {0:n2}");

                        gridView1.Columns["total_net_amount"].Summary.Clear();
                        gridView1.Columns["total_net_amount"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "total_net_amount", "Total Net Amount: {0:n2}");

                        gridView1.Columns["total_net_amount"].Summary.Clear();
                        gridView1.Columns["total_net_amount"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "total_net_amount", "Total Net Amount: {0:n2}");

                        gridView1.Columns["total_net_amount_as_per_selected_branch"].Summary.Clear();
                        gridView1.Columns["total_net_amount_as_per_selected_branch"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "total_net_amount_as_per_selected_branch", "Total Net Amount As Per Selected Branch: {0:n2}");
                        gridView1.BestFitColumns();
                    }
                }
            }
            else
            {
                gridControl1.DataSource = null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            showFilter("Branch", dtBranch);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            showFilter("Item", dtGlobal);
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            gridView1.SelectAll();
            gridView1.CopyToClipboard();
            MessageBox.Show("Copied to clipboard", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void repositoryItemTextEdit1_Click_1(object sender, EventArgs e)
        {

        }

        private async void ItemSalesReport_Load(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                closeForm();
                Loading frm = new Loading();
                frm.Show();
                backgroundWorker1.RunWorkerAsync();
            }
        }



        private void repositoryItemTextEdit2_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedColumnText = gridView1.FocusedColumn.GetCaption();
                string code = gridView1.GetFocusedRowCellValue("item_code").ToString();
                string quantityPerSelected = gridView1.GetFocusedRowCellValue("total_quantity_as_per_selected_branch").ToString();
                var myColumn = gridView1.Columns.FirstOrDefault((col) => col.GetCaption() == "Unit Price");
                if (selectedColumnText.Equals("Item Code") && myColumn != null)
                {
                    string selectedItemCode = "", selectedDiscountAmount = "";
                    int[] ids = gridView1.GetSelectedRows();
                    foreach (int i in ids)
                    {
                        selectedDiscountAmount = gridView1.GetRowCellValue(i, "discprcnt").ToString();
                        selectedItemCode = gridView1.GetRowCellValue(i, "item_code").ToString();

                    }
                    if (gridView1.RowCount > 0)
                    {
                        //string sBranch = "?branch=" + "";
                        //string sWhse = "&whsecode=" + "";
                        //string sTransType = "&transtype=" + (cmbTransType.SelectedIndex == 0 || cmbTransType.Text == "All" ? "" : cmbTransType.Text);
                        //string sFromTime = "&from_time=" + cmbFromTime.Text;
                        //string sToTime = "&to_time=" + cmbToTime.Text;
                        //string sFromDate = "&from_date=" + dtFromDate.Value.ToString("yyyy-MM-dd");
                        //string sToDate = "&to_date=" + dtToDate.Value.ToString("yyyy-MM-dd");
                        //string sItemCode = "&item_code=" + HttpUtility.UrlEncode(selectedItemCode);
                        //string sDiscount = "&discount=" + selectedDiscountAmount;
                        //string URL = "/api/report/item/summary/details" + sBranch + sWhse + sTransType + sFromTime + sToTime + sFromDate + sToDate + sItemCode + sDiscount;
                        //ItemSalesReport_Details details = new ItemSalesReport_Details();
                        //details.URL = URL;
                        //details.ShowDialog();
                    }
                }
                else if (selectedColumnText.Equals("Total Quantity As Per Selected Branch") && !string.IsNullOrEmpty(quantityPerSelected))
                {
                    string status = myColumn == null ? "Open" : "Close";
                    string sBranch = "";
                    string qwe = "";
                    loadPopUpValues(status, code);
                    this.Focus();
                }
            }
            catch (Exception ex)
            {
                apic.showCustomMsgBox(ex.Message, ex.ToString());
            }
        }

        private void gridView1_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            if (e.RowHandle != GridControl.NewItemRowHandle && e.Column.FieldName == "total_quantity_as_per_selected_branch")
            {
                e.RepositoryItem = e.CellValue == null || string.IsNullOrEmpty(e.CellValue.ToString().Trim()) ? repositoryItemTextEdit1 : repositoryItemTextEdit2;
            }
        }

        private void gridControl1_Click(object sender, EventArgs e)
        {

        }

        public void loadData()
        {
            gridControl1.Invoke(new Action(delegate ()
            {
                loadPopUpValues("Close", "");
            }));
        }
    }
}
