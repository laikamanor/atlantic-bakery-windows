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
using Newtonsoft.Json.Linq;
using AB.API_Class.Item_Sales_Summary;
using DevExpress.XtraGrid.Columns;
using System.Globalization;
using DevExpress.XtraGrid.Views.Grid;
using Newtonsoft.Json;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid;

namespace AB
{
    public partial class ItemSalesQuantity : Form
    {
        public ItemSalesQuantity(DataTable dt,DataTable dt2)
        {
            dtGlobal = dt;
            dtSelectedBranches = dt2;
            InitializeComponent();
        }
        DataTable dtGlobal = new DataTable(), dtSelectedBranches = new DataTable();
        int firstIndex = 0;
        double globalTotalSold = 0.00;
       
        private async void ItemSalesSummary_Load(object sender, EventArgs e)
        {
            gridControl1.Tag = "Close";
            bg();
            firstIndex = gridView1.DataRowCount - 1;
        }

        private void ShowGridPreview()
        {
            // Check whether the GridControl can be previewed.
            if (!gridControl1.IsPrintingAvailable)
            {
                MessageBox.Show("The 'DevExpress.XtraPrinting' library is not found", "Error");
                return;
            }

            // Open the Preview window.
            gridControl1.ShowPrintPreview();
        }

        private void PrintGrid()
        {
            // Check whether the GridControl can be printed.
            if (!gridControl1.IsPrintingAvailable)
            {
                MessageBox.Show("The 'DevExpress.XtraPrinting' library is not found", "Error");
                return;
            }

            // Print.
            gridControl1.Print();
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

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        public void loadData()
        {
            gridControl1.Invoke(new Action(delegate ()
            {
                loadPopUpValues("Close", "");
            }));
        }

        public DataTable loadMe(DataTable dt, string item_code, string tagged)
        {
            double doubleTemp = 0.00;
            var query = (from row in dt.AsEnumerable()
                         group row by new
                         {
                             ItemCode = row.Field<string>("item_code"),
                             AllTotalQuantity = row.Field<double?>("total") == null ? 0 : row.Field<double>("total"),
                         } into grp
                         select new
                         {
                             ItemCode = grp.Key.ItemCode,
                             AllTotalQuantity = grp.Key.AllTotalQuantity,
                             DateDiff = grp.Sum(r => r.Field<Int64?>("datediff") == null ? 0 : r.Field<Int64>("datediff")),
                             TotalQuantityAsPerSelected = grp.Sum(r => r.Field<double?>("quantity") == null ? 0 : r.Field<double>("quantity")),
                             Average = grp.Sum(r => r.Field<double?>("average") == null ? 0 : r.Field<double>("average")),
                         }).ToList();
            DataTable dt2 = new DataTable();
            dt2.Columns.Add("item_code", typeof(string));
            dt2.Columns.Add("total", typeof(double));
            dt2.Columns.Add("quantity_per_selected", typeof(double));
            dt2.Columns.Add("average", typeof(double));
            dt2.Columns.Add("datediff", typeof(Int64));
            foreach (var q in query)
            {
                dt2.Rows.Add(q.ItemCode, q.AllTotalQuantity, q.TotalQuantityAsPerSelected, q.Average, q.DateDiff);
            }


            var j = (from row in dt2.AsEnumerable()
                     join row2 in dt.AsEnumerable() on row.Field<string>("item_code") equals row2.Field<string>("item_code")
                     select new
                     {
                         ItemCode = row.Field<string>("item_code"),
                         AllTotalQuantity = row.Field<double>("total"),
                         QuantityPerSelected = row.Field<double>("quantity_per_selected"),
                         Branch = row2.Field<string>("branch"),
                         QuantityPerBranch = row2.Field<double>("quantity"),
                         Average = row2.Field<double>("average"),
                         DateDiff = row2.Field<Int64>("datediff")
                     }).ToList();
            DataTable dt3 = new DataTable();
            dt3.Columns.Add("item_code", typeof(string));
            dt3.Columns.Add("all_total_quantity", typeof(double));
            dt3.Columns.Add("total_quantity_as_per_selected_branch", typeof(double));
            if (tagged.Equals("Open"))
            {
                dt3.Columns.Add("branch", typeof(string));
                dt3.Columns.Add("quantity_per_branch", typeof(double));
                dt3.Columns.Add("average", typeof(double));
                dt3.Columns.Add("datediff", typeof(Int64));
                dt3.Columns.Add("percentage", typeof(double));
            }
            else
            {
                dt3.Columns.Add("percentage", typeof(double));
            }

            string item = "", itemCode2 = "";
            globalTotalSold = !tagged.Equals("Open") ? 0.00 : globalTotalSold;
            foreach (var q in j)
            {
                if (!tagged.Equals("Open"))
                {
                    globalTotalSold += q.QuantityPerBranch;
                }
                itemCode2 = q.ItemCode;
            }
            
            //Console.WriteLine("sold " + totalSold);
            foreach (var q in j)
            {

                if (tagged.Equals("Open"))
                {
                    if (q.ItemCode == item_code)
                    {
                        if (!item.Equals(q.ItemCode))
                        {

                            dt3.Rows.Add(q.ItemCode, q.AllTotalQuantity == null ? 0.00 : q.AllTotalQuantity, q.QuantityPerSelected == null ? 0.00 : q.QuantityPerSelected,"", (double?)null, (double?)null, (double?)null,(double?)null);
                        }
                        double percentage = (q.QuantityPerSelected / globalTotalSold) * 100;
                        Console.WriteLine("% " + q.QuantityPerSelected + "/" + globalTotalSold + "/" + tagged);
                        dt3.Rows.Add("", (double?)null, (double?)null, q.Branch, q.QuantityPerBranch == null ? 0.00 : q.QuantityPerBranch, q.Average == null ? 0.00 : q.Average, q.DateDiff == null ? 0 : q.DateDiff, percentage);
                    }
                }
                else
                {
                    if (!item.Equals(q.ItemCode))
                    {
                        Console.WriteLine("global " + globalTotalSold);
                        double percentage = (q.QuantityPerSelected / globalTotalSold) * 100;
                        dt3.Rows.Add(q.ItemCode, q.AllTotalQuantity == null ? 0.00 : q.AllTotalQuantity, q.QuantityPerSelected == null ? 0.00 : q.QuantityPerSelected, percentage);
                    }

                }
                item = q.ItemCode;
            }
            return dt3;
        }

        private void btnCopyRows_Click(object sender, EventArgs e)
        {
            gridView1.SelectAll();
            gridView1.CopyToClipboard();
            MessageBox.Show("Copied to clipboard","Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void loadPopUpValues(string status,string itemCode)
        {
            DataTable dt = dtGlobal;
            string sBranchSelected = "";
            string sItemSelected = "";
            bool isAllBranch = false, isAllItem=false, boolTemp = false;
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
                    sItemSelected += "'" + s+ "'" + ",";
                    isAllItem = bool.TryParse(row["is_all"].ToString(), out boolTemp) ? Convert.ToBoolean(row["is_all"].ToString()) : boolTemp;
                }
            }
            sItemSelected = string.IsNullOrEmpty(sItemSelected.Trim()) ? "" : sItemSelected.Substring(0, sItemSelected.Length - 1);
            labelSelectedBranch.Text = "Branch [" + (string.IsNullOrEmpty(sBranchSelected.Trim()) || isAllBranch ? "All" : sBranchSelected) + "]";
            labelItem.Text = "Item [" + (string.IsNullOrEmpty(sItemSelected.Trim()) || isAllItem ? "All" : sItemSelected) + "]";
            toolTip1.SetToolTip(labelSelectedBranch, labelSelectedBranch.Text);
            toolTip1.SetToolTip(labelItem, labelItem.Text);
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
                sItemSelected = "item_code IN (" + sItemSelected + ")";
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
                if(dtSelectedBranches.Rows.Count > 0)
                {
                    DataRow[] results = dt.Select(appendParams);
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

        public void loadSettings(DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
                gridControl1.DataSource = dt;
                foreach (GridColumn col in gridView1.Columns)
                {
                    string v = col.GetCaption();
                    string s = col.GetCaption().Replace("_", " ");
                    col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                    col.ColumnEdit = v.Equals("total_quantity_as_per_selected_branch") ? repositoryItemTextEdit2 : repositoryItemTextEdit1;
                    //col.ColumnEdit = repositoryItemTextEdit2;

                    col.OptionsColumn.AllowSort = v.Equals("item_code") ? DevExpress.Utils.DefaultBoolean.True : DevExpress.Utils.DefaultBoolean.Default;
                    col.SortMode = v.Equals("item_code") ? DevExpress.XtraGrid.ColumnSortMode.Custom : DevExpress.XtraGrid.ColumnSortMode.Default;
                    col.DisplayFormat.FormatType = v.Equals("branch") || v.Equals("item_code") ? DevExpress.Utils.FormatType.None : DevExpress.Utils.FormatType.Numeric;
                    col.DisplayFormat.FormatString = v.Equals("branch") || v.Equals("item_code") ? "" : "n2";
                    //col.Visible = v.Equals("datediff") ? false : true;
                    col.Caption = v.Equals("datediff") ? "Days" : col.GetCaption();

                    //fonts
                    FontFamily fontArial = new FontFamily("Arial");
                    col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                    col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                }
                gridView1.BestFitColumns();
                if (dt.Rows.Count > 0)
                {
                    gridView1.ClearSorting();
                    gridView1.Columns["item_code"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;

                    gridView1.OptionsView.ShowFooter = true;
                    var myColumn = gridView1.Columns.FirstOrDefault((col) => col.GetCaption() == "Branch");
                    if (myColumn == null)
                    {
                        gridView1.Columns["item_code"].Summary.Clear();
                        gridView1.Columns["all_total_quantity"].Summary.Clear();
                        gridView1.Columns["total_quantity_as_per_selected_branch"].Summary.Clear();
                        gridView1.Columns["item_code"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Count;
                        gridView1.Columns["item_code"].SummaryItem.FieldName = "Total";
                        gridView1.Columns["item_code"].SummaryItem.DisplayFormat = "Total: {0:N0}";
                        gridView1.Columns["all_total_quantity"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "all_total_quantity", "Total Quantity: {0:n2}");
                        gridView1.Columns["total_quantity_as_per_selected_branch"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "total_quantity_as_per_selected_branch", "Total Quantity As Per Selected Branch: {0:n2}");
                        gridView1.BestFitColumns();
                    }
                    else
                    {
                        gridView1.Columns["branch"].Summary.Clear();
                        gridView1.Columns["branch"].Summary.Add(DevExpress.Data.SummaryItemType.Custom, "branch", "Total: " + (gridView1.RowCount -1 ).ToString("N0"));
                        gridView1.Columns["quantity_per_branch"].Summary.Clear();
                        gridView1.Columns["quantity_per_branch"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "quantity_per_branch", "Total Quantity Per Branch: {0:n2}");
                        gridView1.BestFitColumns();
                    }
                    //    gridView1.Columns["branch"].OptionsFilter.FilterPopupMode = FilterPopupMode.Excel;

                    //    gridView1.Columns["item_code"].GroupIndex = 1;
                    //    gridView1.Columns["quantity"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
                }
            }
            else
            {
                gridControl1.DataSource = null;
            }
        }


        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnText = gridView1.FocusedColumn.GetCaption();
            string code = gridView1.GetFocusedRowCellValue("item_code").ToString();
            string quantityPerSelected = gridView1.GetFocusedRowCellValue("total_quantity_as_per_selected_branch").ToString();
            if (selectedColumnText.Equals("Total Quantity As Per Selected Branch") && !string.IsNullOrEmpty(quantityPerSelected))
            {
                var myColumn = gridView1.Columns.FirstOrDefault((col) => col.GetCaption() == "Branch");
                string status = myColumn == null ? "Open" : "Close";
                string sBranch = "";
                string qwe = "";
                loadPopUpValues(status, code);
                this.Focus();
            }
        }
private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView currentView = sender as GridView;
            if (e.Column.FieldName == "computed_forecast")
            {
                double value = !Convert.IsDBNull(currentView.GetRowCellValue(e.RowHandle, "computed_forecast")) ? Convert.ToDouble(currentView.GetRowCellValue(e.RowHandle, "computed_forecast")) : 0.00;
                e.Appearance.BackColor = value < 0 ? Color.FromArgb(255, 110, 110) : Color.White;
            }
        }

        private void gridView1_CustomColumnSort(object sender, CustomColumnSortEventArgs e)
        {
            e.Handled = true;
            if (e.ListSourceRowIndex1 == 1)
            {
                e.Result = (e.SortOrder == DevExpress.Data.ColumnSortOrder.Ascending ? 1 : -1);
            }
            else if (e.ListSourceRowIndex2 == 1)
            {
                e.Result = (e.SortOrder == DevExpress.Data.ColumnSortOrder.Ascending ? -1 : 1);
            }
            else
            {
                e.Handled = false;
            }
        }

        private void repositoryItemTextEdit1_Click_1(object sender, EventArgs e)
        {
          
        }

        private void repositoryItemTextEdit2_Click(object sender, EventArgs e)
        {
            string selectedColumnText = gridView1.FocusedColumn.GetCaption();
            string code = gridView1.GetFocusedRowCellValue("item_code").ToString();
            string quantityPerSelected = gridView1.GetFocusedRowCellValue("total_quantity_as_per_selected_branch").ToString();
            if (selectedColumnText.Equals("Total Quantity As Per Selected Branch") && !string.IsNullOrEmpty(quantityPerSelected))
            {
                var myColumn = gridView1.Columns.FirstOrDefault((col) => col.GetCaption() == "Branch");
                string status = myColumn == null ? "Open" : "Close";
                string sBranch = "";
                string qwe = "";
                loadPopUpValues(status, code);
                this.Focus();
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
    }
}
