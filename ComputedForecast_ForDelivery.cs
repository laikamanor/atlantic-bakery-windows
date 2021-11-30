using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
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
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using Newtonsoft.Json.Linq;

namespace AB
{
    public partial class ComputedForecast_ForDelivery : Form
    {
        public ComputedForecast_ForDelivery()
        {
            InitializeComponent();
        }
        devexpress_class devc = new devexpress_class();
        public DataTable dtSelectedBranches = new DataTable();
        public DataTable dtGlobal = new DataTable();
        string[] sForDelivery_Item, sForDelivery_Branch;
        private void ComputedForecast_ForDelivery_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            loadData(gridControl1, gridView1, loadPopUpValues(true, gridControl1.Name));
            loadData(gridControl2, gridView2, loadPopUpValues(false, gridControl2.Name));
        }

        public void loadData(GridControl gridcontrol, GridView grid, DataTable dtFinal)
        {
            gridcontrol.DataSource = null;
            grid.Columns.Clear();
            gridcontrol.DataSource = dtFinal;

            grid.OptionsView.ColumnAutoWidth = false;
            grid.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            foreach (GridColumn col in grid.Columns)
            {
                string fieldName = col.FieldName;
                string fieldName2 = fieldName.Replace("_", " ");
                col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fieldName2);
                if (grid.Name.Equals("gridView1"))
                {
                    col.ColumnEdit = fieldName.Equals("item_code") || fieldName.Equals("branch") ? repositoryItemMemoEdit1 : null;
                }
                col.ColumnEdit = fieldName.Equals("final_for_delivery") || fieldName.Equals("final_base") ? repositoryItemTextEdit2 : repositoryItemTextEdit1;
                col.DisplayFormat.FormatType = col.GetCaption().Equals("branch") || col.GetCaption().Equals("item_code") ? DevExpress.Utils.FormatType.None : DevExpress.Utils.FormatType.Numeric;
                col.DisplayFormat.FormatString = col.GetCaption().Equals("branch") || col.GetCaption().Equals("item_code") ? "" : col.GetCaption().ToLower().Equals("days") || col.GetCaption().ToLower().Equals("average_per_day") ? "n2" : "n2";
                col.AppearanceCell.TextOptions.HAlignment = fieldName.Equals("branch") || fieldName.Equals("item_code") ? DevExpress.Utils.HorzAlignment.Near : DevExpress.Utils.HorzAlignment.Far;

                if (grid.Name.Equals("gridView1"))
                {
                    col.Visible = !(fieldName.Equals("id") || fieldName.Equals("uom"));
                }
                else
                {
                    col.Visible = !(fieldName.Equals("id") || fieldName.Equals("uom") || fieldName.Equals("branch"));
                }

                col.Fixed = fieldName.Equals("item_code") || fieldName.Equals("branch") ? FixedStyle.Left  : fieldName.Equals("final_for_delivery") ? FixedStyle.Right : FixedStyle.None;

                //fonts
                FontFamily fontArial = new FontFamily("Arial");
                col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
            }
            if (grid.Name.Equals("gridView1"))
            {
                var col1 = grid.Columns["target_for_delivery"];
                if (col1 != null)
                {
                    grid.Columns["target_for_delivery"].Summary.Clear();
                    grid.Columns["target_for_delivery"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "target_for_delivery", "Total Target For Delivery: {0:n2}");
                }
                var col2 = grid.Columns["final_for_delivery"];
                if (col2 != null)
                {
                    grid.Columns["final_for_delivery"].Summary.Clear();
                    grid.Columns["final_for_delivery"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "final_for_delivery", "Total Final For Delivery: {0:n2}");
                }
            }
            grid.BestFitColumns();
            //if (grid.Name.Equals("gridView1"))
            //{
            //    var colItemCode = grid.Columns["item_code"];
            //    var colBranch = grid.Columns["branch"];
            //    if (colItemCode != null)
            //    {
            //        colItemCode.Width = 150;
            //    }
            //    if (colBranch != null)
            //    {
            //        colBranch.Width = 150;
            //    }
            //}
        }

        public DataTable loadPopUpValues(bool isOpen, string controlName)
        {
            DataTable dtResult = new DataTable();
            try
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
                        string s = row["name"].ToString().Replace(@"'", "''");
                        if (sForDelivery_Branch != null && controlName.Equals("gridControl1"))
                        {
                            foreach (string item in sForDelivery_Branch)
                            {
                                if (s == item)
                                {
                                    sBranchSelected += "'" + s + "'" + ",";
                                    Console.WriteLine("one: " + sBranchSelected);
                                }
                            }
                        }
                        else
                        {
                            sBranchSelected += "'" + row["name"].ToString() + "'" + ",";
                            Console.WriteLine("two: " + sBranchSelected);
                            isAllBranch = bool.TryParse(row["is_all"].ToString(), out boolTemp) ? Convert.ToBoolean(row["is_all"].ToString()) : boolTemp;
                        }
                    }
                }
                foreach (DataRow row in dtSelectedBranches.Rows)
                {
                    //MessageBox.Show(row["type"].ToString());
                    if (row["type"].ToString().Equals("Item"))
                    {
                        string s = row["name"].ToString().Replace(@"'", "''");
                        if (sForDelivery_Item != null && controlName.Equals("gridControl1"))
                        {
                            foreach (string item in sForDelivery_Item)
                            {
                                if (s == item)
                                {
                                    sItemSelected += "'" + s + "'" + ",";
                                }
                            }
                        }
                        else
                        {
                            sItemSelected += "'" + s + "'" + ",";
                            isAllItem = bool.TryParse(row["is_all"].ToString(), out boolTemp) ? Convert.ToBoolean(row["is_all"].ToString()) : boolTemp;
                        }
                    }
                }
                if (dtSelectedBranches.Rows.Count <= 0)
                {
                    if (sForDelivery_Item != null && controlName.Equals("gridControl1"))
                    {
                        foreach (string item in sForDelivery_Item)
                        {
                            string s = item.Replace(@"'", "''");
                            sItemSelected += "'" + s + "'" + ",";
                        }
                    }
                    if (sForDelivery_Branch != null && controlName.Equals("gridControl1"))
                    {
                        foreach (string item in sForDelivery_Branch)
                        {
                            string s = item.Replace(@"'", "''");
                            sBranchSelected += "'" + s + "'" + ",";
                            Console.WriteLine("three: " + sBranchSelected);
                        }
                    }
                }
                else
                {
                    if (sForDelivery_Branch != null && controlName.Equals("gridControl1"))
                    {
                        foreach (string item in sForDelivery_Branch)
                        {
                            string s = item.Replace(@"'", "''");
                            sBranchSelected += "'" + s + "'" + ",";
                        }
                    }
                }
               
                string forDeliverItemText = customFiltering_ForDelivery.isAll && customFiltering_ForDelivery.forDeliveryName.Equals("sForDelivery_Item") ? "Item [All]" :  sForDelivery_Item != null ? "Item [" + string.Join(",", sForDelivery_Item) + "]" : "Item []";
                string forDeliverBranchText = customFiltering_ForDelivery.isAll && customFiltering_ForDelivery.forDeliveryName.Equals("sForDelivery_Branch") ? "Branch [All]" : sForDelivery_Branch != null ? "Branch [" + string.Join(",", sForDelivery_Branch) + "]" : "Branch []";
                labelForDeliveryItem.Text = forDeliverItemText;
                labelForDeliveryBranch.Text = forDeliverBranchText;
                sItemSelected = string.IsNullOrEmpty(sItemSelected.Trim()) ? "" : sItemSelected.Substring(0, sItemSelected.Length - 1);
                sBranchSelected = string.IsNullOrEmpty(sBranchSelected.Trim()) ? "" : sBranchSelected.Substring(0, sBranchSelected.Length - 1);
                sBranchSelected = string.IsNullOrEmpty(sBranchSelected.Trim()) ? "" : "branch IN (" + sBranchSelected + ")" + (string.IsNullOrEmpty(sItemSelected.Trim()) ? " " : " AND ");

                sItemSelected = string.IsNullOrEmpty(sItemSelected.Trim()) ? "" : "item_code IN (" + sItemSelected + ")";
                string appendParams = sBranchSelected + sItemSelected;
                if (dt.Rows.Count > 0)
                {
                    DataRow[] results = dt.Select(appendParams);
                    string aaa = appendParams;
                    DataTable dtt = new DataTable();
                    if (results.Any())
                    {
                        dtt = results.CopyToDataTable();
                        dtResult = loadFormat(isOpen, dtt);
                    }
                    else
                    {
                        //null
                        //DataTable dt2 = new DataTable();
                        //loadSettings(dt2);
                        dtResult = null;
                    }
                }
                else
                {
                    dtResult = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dtResult;
        }

        public DataTable loadFormat(bool isOpen, DataTable dtFiltered)
        {
            DataTable dtResult = new DataTable();
            try
            {
                double doubleTemp = 0.00;
                if (!isOpen)
                {
                    var query = (from row in dtFiltered.AsEnumerable()
                                 group row by new
                                 {
                                     ItemCode = row.Field<string>("item_code"),
                                     Uom = row.Field<string>("uom"),
                                 } into grp
                                 select new
                                 {
                                     ItemCode = grp.Key.ItemCode,
                                     TotalTargetDelivery = grp.Sum(r => r.Field<double?>("target_for_del") == null ? 0 : r.Field<double>("target_for_del")) - grp.Sum(r => r.Field<double>("prod_ending_bal")),
                                     ProdMinQty = grp.Sum(r => r.Field<double>("prod_min_qty")),
                                     LastBal = grp.Sum(r => r.Field<double>("prod_ending_bal")),
                                     Uom = grp.Key.Uom,
                                 }).ToList();

                    DataTable dt1 = new DataTable();
                    dt1.Columns.Add("item_code", typeof(string));
                    dt1.Columns.Add("uom", typeof(string));
                    dt1.Columns.Add("target_for_prod", typeof(double));
                    dt1.Columns.Add("prod_min_qty", typeof(double));
                    dt1.Columns.Add("target_base", typeof(double));
                    dt1.Columns.Add("final_base", typeof(double));
                    dt1.Columns.Add("prod_ending_bal", typeof(double));
                    dt1.Columns.Add("final_prod_qty", typeof(double));
                    dt1.Columns.Add("branch", typeof(string));

                    foreach (var j in query)
                    {
                        string currentItemCode = "";
                        foreach (DataRow row in dtFiltered.Rows)
                        {
                            if (dtFiltered.Columns.Contains("item_code"))
                            {
                                string rItemCode = row["item_code"].ToString();
                                if (rItemCode == j.ItemCode)
                                {
                                    if (currentItemCode != j.ItemCode)
                                    {
                                        string rBranchCode = dtFiltered.Columns.Contains("branch") ? row["branch"].ToString() : "";
                                        double targetBase = (j.TotalTargetDelivery / j.ProdMinQty);
                                        dt1.Rows.Add(j.ItemCode, j.Uom, j.TotalTargetDelivery, j.ProdMinQty <= 0 ? (double?)null : j.ProdMinQty, double.IsNaN(targetBase) || double.IsInfinity(targetBase) ? (double?)null : targetBase, null, j.LastBal, null, rBranchCode);
                                        currentItemCode = j.ItemCode;
                                    }
                                }
                            }
                        }
                    }

                    //DataTable dt2 = new DataTable();
                    //dt2.Columns.Add("item_code", typeof(string));
                    //dt2.Columns.Add("uom", typeof(string));
                    //dt2.Columns.Add("target_for_prod", typeof(double));
                    //dt2.Columns.Add("prod_min_qty", typeof(double));
                    //dt2.Columns.Add("target_base", typeof(double));
                    //dt2.Columns.Add("final_base", typeof(double));
                    //dt2.Columns.Add("final_prod_qty", typeof(double));
                    //dt2.Columns.Add("prod_ending_bal", typeof(double));
                    //dt2.Columns.Add("branch", typeof(string));

                    //var queryy = (from row in dt1.AsEnumerable()
                    //              join row2 in dtFiltered.AsEnumerable() on row.Field<string>("item_code") equals row2.Field<string>("item_code")
                    //              group row by new
                    //              {
                    //                  ItemCode = row.Field<string>("item_code"),
                    //                  Uom = row.Field<string>("uom"),
                    //                  Branch = row.Field<string>("branch"),
                    //              } into grp
                    //              select new
                    //              {
                    //                  ItemCode = r.Field<string>("item_code") == null ? "" : row.Field<string>("item_code"),
                    //                  Uom = r.Field<string>("uom") == null ? "" : row.Field<string>("uom"),
                    //                  TargetForProd = row.Field<double?>("target_for_prod") == null ? 0.00 : row.Field<double>("target_for_prod"),
                    //                  ProdMinQty = row.Field<double?>("prod_min_qty") == null ? 0.00 : row.Field<double>("prod_min_qty"),
                    //                  TargetBase = row.Field<double?>("target_base") == null ? 0 : row.Field<double>("target_base"),
                    //                  FinalBase = row.Field<double?>("final_base") == null ? 0 : row.Field<double>("final_base"),
                    //                  FinalProdQty = row.Field<double?>("final_prod_qty") == null ? 0 : row.Field<double>("final_prod_qty"),
                    //                  ProdEndBal = row.Field<double?>("prod_ending_bal") == null ? 0 : row.Field<double>("prod_ending_bal"),
                    //                  Branch = row.Field<string>("branch") == null ? "" : row.Field<string>("branch"),
                    //              }).ToList();

                    //foreach (var j in queryy)
                    //{
                    //    dt2.Rows.Add(j.ItemCode, j.Uom, j.TargetForProd, j.ProdMinQty, j.TargetBase, j.FinalBase, j.FinalProdQty, j.ProdEndBal, j.Branch);
                    //}

                    dtResult = dt1;
                }
                else
                {
                    var query = (from row in dtFiltered.AsEnumerable()
                                 group row by new
                                 {
                                     ItemCode = row.Field<string>("item_code"),
                                 } into grp
                                 select new
                                 {
                                     ItemCode = grp.Key.ItemCode,
                                     TotalSold = grp.Sum(r => r.Field<double?>("sold") == null ? 0 : r.Field<double>("sold")),
                                 }).ToList();

                    DataTable dt1 = new DataTable();
                    dt1.Columns.Add("item_code", typeof(string));
                    dt1.Columns.Add("total_sold_as_per_selected_branch", typeof(double));
                    foreach (var j in query)
                    {
                        //Console.WriteLine("query1: " + j.ItemCode + "/" + j.TotalSold);
                        dt1.Rows.Add(j.ItemCode, j.TotalSold);
                    }

                    var queryy = (from row in dtFiltered.AsEnumerable()
                                  join row2 in dt1.AsEnumerable() on row.Field<string>("item_code") equals row2.Field<string>("item_code")
                                  into grp
                                  select new
                                  {
                                      Id = row.Field<int>("id") == null ? 0 : row.Field<int>("id"),
                                      ItemCode = row.Field<string>("item_code") == null ? "" : row.Field<string>("item_code"),
                                      TotalSold = grp.Sum(r => r.Field<double?>("total_sold_as_per_selected_branch") == null ? 0 : r.Field<double>("total_sold_as_per_selected_branch")),
                                      Branch = row.Field<string>("branch") == null ? "" : row.Field<string>("branch"),
                                      Sold = row.Field<double?>("sold") == null ? 0 : row.Field<double>("sold"),
                                      DateDiff = row.Field<Int64?>("datediff") == null ? 0 : row.Field<Int64>("datediff"),
                                      Average = row.Field<double?>("average") == null ? 0.00 : row.Field<double>("average"),
                                      LastBal = row.Field<double?>("last_bal") == null ? 0 : row.Field<double>("last_bal"),
                                      TargetForDel = row.Field<double?>("target_for_del") == null ? 0 : row.Field<double>("target_for_del"),
                                      ProdMinQty = row.Field<dynamic>("prod_min_qty") == null ? 0.00 : row.Field<dynamic>("prod_min_qty"),
                                      FinalForDelivery = row.Field<dynamic>("final_for_delivery") == null ? (double?)null : row.Field<dynamic>("final_for_delivery")
                                  }).Distinct().ToList();


                    DataTable dt = new DataTable();
                    dt.Columns.Add("id", typeof(int));
                    dt.Columns.Add("item_code", typeof(string));
                    dt.Columns.Add("total_sold", typeof(double));
                    dt.Columns.Add("branch", typeof(string));
                    //dt.Columns.Add("branch", typeof(string));

                    dt.Columns.Add("sold_quantity", typeof(double));
                    dt.Columns.Add("days", typeof(double));
                    dt.Columns.Add("average_per_day", typeof(double));
                    dt.Columns.Add("last_bal_qty", typeof(double));
                    dt.Columns.Add("target_for_delivery", typeof(double));
                    dt.Columns.Add("final_for_delivery", typeof(double));
                    //foreach (var j in queryy)
                    //{
                    //    dt.Rows.Add(j.ItemCode, j.TotalSold, "", (double?)null, (Int64?)null, (Int64?)null, (double?)null, (double?)null, (double?)null);
                    //    dt.Rows.Add("", (double?)null, j.Branch, j.Sold, j.DateDiff, j.Average, j.LastBal, j.TargetForDel, (double?)null);
                    //}
                    string item = "";
                    foreach (var j in queryy)
                    {
                        if (!item.Equals(j.ItemCode))
                        {
                            dt.Rows.Add(j.Id, j.ItemCode, j.TotalSold, "", (double?)null, (Int64?)null, (Int64?)null, (double?)null, (double?)null, (double?)null);
                        }
                        dt.Rows.Add(j.Id, "", (double?)null, j.Branch, j.Sold, j.DateDiff, j.Average, j.LastBal, j.TargetForDel, j.FinalForDelivery != null ? j.FinalForDelivery : j.TargetForDel);
                        item = j.ItemCode;
                    }
                    DataView view = new DataView(dt);
                    dtResult = view.ToTable(true, "id", "item_code", "total_sold", "branch", "sold_quantity", "days", "average_per_day", "last_bal_qty", "target_for_delivery", "final_for_delivery");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dtResult;
        }

        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    string selectedColumnText = gridView1.FocusedColumn.FieldName;
            //    string itemCode = gridView1.GetFocusedRowCellValue("item_code").ToString();
            //    var mySoldColumn = gridView1.Columns.FirstOrDefault((col) => col.FieldName == "sold_quantity");
            //    if (selectedColumnText.Equals("total_sold"))
            //    {
            //        if (mySoldColumn == null)
            //        {
            //            loadForDelivery(loadPopUpValues(true, itemCode));
            //        }
            //        else
            //        {
            //            loadForDelivery(loadPopUpValues(false, ""));
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //}
        }

        private void gridView1_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
         
        }

        private void gridView1_ShowingEditor(object sender, CancelEventArgs e)
        {
            GridView currentView = sender as GridView;
            string branch = !Convert.IsDBNull(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch").ToString()) ? gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch").ToString() : "";
            string selectedColumnText = gridView1.FocusedColumn.FieldName;
            e.Cancel = selectedColumnText.Equals("final_for_delivery") && string.IsNullOrEmpty(branch.Trim()) ? true : false;
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
        }
        private void gridView1_MouseMove(object sender, MouseEventArgs e)
        {
            //GridView view = sender as GridView;
            //GridHitInfo info = view.CalcHitInfo(new Point(e.X, e.Y));

            //if (info.InRowCell)
            //    HotTrackRow = info.RowHandle;
            //else
            //    HotTrackRow = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        }

        private void gridView2_MouseMove(object sender, MouseEventArgs e)
        {
            //GridView view = sender as GridView;
            //GridHitInfo info = view.CalcHitInfo(new Point(e.X, e.Y));

            //if (info.InRowCell)
            //    HotTrackRow2 = info.RowHandle;
            //else
            //    HotTrackRow2 = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        }

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            //if (e.RowHandle == HotTrackRow)
            //    e.Appearance.BackColor = gridView1.PaintAppearance.SelectedRow.BackColor;
            //else
            //    e.Appearance.BackColor = e.Appearance.BackColor;
        }

        private void gridView2_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            //if (e.RowHandle == HotTrackRow2)
            //    e.Appearance.BackColor = gridView2.PaintAppearance.SelectedRow.BackColor;
            //else
            //    e.Appearance.BackColor = e.Appearance.BackColor;
        }

        private void gridView2_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {

        }

        private void gridView2_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            try
            {
                string selectedColumnfieldName = gridView2.FocusedColumn.FieldName;
                if (selectedColumnfieldName.Equals("final_base"))
                {
                    double doubleTemp = 0.00;
                    //double prodMinQty = Convert.ToDouble(gridView2.GetFocusedRowCellValue("prod_min_qty").ToString());
                    double prodMinQty = double.TryParse(gridView2.GetFocusedRowCellValue("prod_min_qty").ToString(), out doubleTemp) ? Convert.ToDouble(gridView2.GetFocusedRowCellValue("prod_min_qty").ToString()) : doubleTemp;
                    double finalBase = double.TryParse(e.Value.ToString(), out doubleTemp) ? Convert.ToDouble(e.Value.ToString()) : doubleTemp;

                    double finalProdQty = prodMinQty * finalBase;
                    var varCol = gridView2.Columns["final_prod_qty"];
                    if (e.Value.ToString().Trim() == "")
                    {
                        gridView2.SetRowCellValue(e.RowHandle, varCol, null);
                    }
                    else
                    {
                        gridView2.SetRowCellValue(e.RowHandle, varCol, finalProdQty.ToString("n3"));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                JArray jaArray = new JArray();

                string sBranchSelected = "";
                string sItemSelected = "";
                bool isAllBranch = false, isAllItem = false, boolTemp = false;
                foreach (DataRow row in dtSelectedBranches.Rows)
                {
                    if (row["type"].ToString().Equals("Branch"))
                    {
                        string s = row["name"].ToString().Replace(@"'", "''");
                        if (sForDelivery_Branch != null)
                        {
                            foreach (string item in sForDelivery_Branch)
                            {
                                if (s == item)
                                {
                                    sBranchSelected += "'" + s + "'" + ",";
                                }
                            }
                        }
                        else
                        {
                            sBranchSelected += "'" + row["name"].ToString() + "'" + ",";
                            isAllBranch = bool.TryParse(row["is_all"].ToString(), out boolTemp) ? Convert.ToBoolean(row["is_all"].ToString()) : boolTemp;
                        }
                    }
                }
                sBranchSelected = string.IsNullOrEmpty(sBranchSelected.Trim()) ? "" : sBranchSelected.Substring(0, sBranchSelected.Length - 1);
                foreach (DataRow row in dtSelectedBranches.Rows)
                {
                    if (row["type"].ToString().Equals("Item"))
                    {
                        string s = row["name"].ToString().Replace(@"'", "''");
                        if (sForDelivery_Item != null)
                        {
                            foreach (string item in sForDelivery_Item)
                            {
                                if (s == item)
                                {
                                    sItemSelected += "'" + s + "'" + ",";
                                }
                            }
                        }
                        else
                        {
                            sItemSelected += "'" + s + "'" + ",";
                            isAllItem = bool.TryParse(row["is_all"].ToString(), out boolTemp) ? Convert.ToBoolean(row["is_all"].ToString()) : boolTemp;
                        }
                        //string s = row["name"].ToString().Replace(@"'", "''");
                        //sItemSelected += "'" + s + "'" + ",";
                        //isAllItem = bool.TryParse(row["is_all"].ToString(), out boolTemp) ? Convert.ToBoolean(row["is_all"].ToString()) : boolTemp;
                    }
                }
                if (dtSelectedBranches.Rows.Count <= 0)
                {
                    if (sForDelivery_Item != null)
                    {
                        foreach (string item in sForDelivery_Item)
                        {
                            string s = item.Replace(@"'", "''");
                            sItemSelected += "'" + s + "'" + ",";
                        }
                    }
                    if (sForDelivery_Branch != null)
                    {
                        foreach (string item in sForDelivery_Branch)
                        {
                            string s = item.Replace(@"'", "''");
                            sBranchSelected += "'" + s + "'" + ",";
                        }
                    }
                }
                sItemSelected = string.IsNullOrEmpty(sItemSelected.Trim()) ? "" : sItemSelected.Substring(0, sItemSelected.Length - 1);
                sBranchSelected = string.IsNullOrEmpty(sBranchSelected.Trim()) ? "" : "branch IN (" + sBranchSelected + ")" + (string.IsNullOrEmpty(sItemSelected.Trim()) ? " " : " AND ");
                sItemSelected = string.IsNullOrEmpty(sItemSelected.Trim()) ? "" : "item_code IN (" + sItemSelected + ")";
                string appendParams = sBranchSelected + sItemSelected;

                DataRow[] results = dtGlobal.Select(appendParams);
                string aaa = appendParams;
                DataTable dtt = new DataTable();
                if (results.Any())
                {
                    dtt = results.CopyToDataTable();
                    DataView dv = dtt.DefaultView;
                    dv.Sort = "branch ASC";
                    dtt = dv.ToTable();
                    string branch = "";
                    JObject jo = new JObject();
                    JArray jaBody = new JArray();
                    
                    JArray jaRows = new JArray();
                    int checkFinalDelivery = 0;
                    foreach (DataRow row in dtt.Rows)
                    {
                        if (branch.Trim() != row["branch"].ToString().Trim())
                        {
                            //if (jo.ContainsKey("header") && jo.ContainsKey("rows"))
                            //{
                            //    jaArray.Add(jo);
                            //}
                            //jo = new JObject();
                            //if(jaRows.Count > 0)
                            //{
                            //    jo.Add("rows", jaRows);
                            //}
                            if(jaRows.Count > 0)
                            {
                                JObject joHeader = new JObject();
                                joHeader.Add("transdate", null);
                                joHeader.Add("delivery_date", null);
                                joHeader.Add("branch", null);
                                joHeader.Add("remarks", null);
                                joHeader.Add("shift", null);
                                joHeader.Add("hashed_id", null);
                                jo.Add("header", joHeader);
                                jo.Add("rows", jaRows);
                                jaBody.Add(jo);
                                jaRows = new JArray();
                                jo = new JObject();
                            }
                            branch = row["branch"].ToString();

                        }
                        if (branch.Trim() == row["branch"].ToString().Trim())
                        {
                            double targetForDel = 0.00, finalForDel = 0.00, doubleTemp = 0.00;
                            bool isTargetForDel = false, isFinalForDel = false;
                            if (double.TryParse(row["target_for_del"].ToString(), out doubleTemp))
                            {
                                targetForDel = Convert.ToDouble(row["target_for_del"].ToString());
                                isTargetForDel = true;
                            }
                            if (double.TryParse(row["final_for_delivery"].ToString(), out doubleTemp))
                            {
                                finalForDel = Convert.ToDouble(row["final_for_delivery"].ToString());
                                isFinalForDel = true;
                            }
                            else
                            {
                                checkFinalDelivery++;
                            }
                            JObject joRows = new JObject();
                            joRows.Add("item_code", row["item_code"].ToString());
                            joRows.Add("to_branch", null);
                            joRows.Add("quantity", isTargetForDel ? targetForDel : (double?)null);
                            joRows.Add("final_qty", isFinalForDel ? finalForDel : (double?)null);
                            jaRows.Add(joRows);
                            //jaRows.Add(joRows);
                            //JArray jaTemp = JArray.Parse(jo["rows"].ToString());
                            //jaTemp.Add(joRows);
                            //jo["rows"] = jaTemp;
                        }
                        bool isLast = dtt.Rows.IndexOf(row) + 1 == dtt.Rows.Count;
                        if (isLast)
                        {
                            JObject joHeader = new JObject();
                            joHeader.Add("transdate", null);
                            joHeader.Add("delivery_date", null);
                            joHeader.Add("branch", null);
                            joHeader.Add("remarks", null);
                            joHeader.Add("shift", null);
                            joHeader.Add("hashed_id", null);
                            jo.Add("header", joHeader);
                            jo.Add("rows", jaRows);
                            jaBody.Add(jo);
                        }
                    }
                    if (checkFinalDelivery > 0)
                    {
                        MessageBox.Show("You have unfilled Final for Delivery Quantity! " + checkFinalDelivery, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }else
                    {
                        this.Cursor = Cursors.Default;
                        //MessageBox.Show(jaArray.ToString());
                        ComputedForecast_ForDelivery_Dialog.isSubmit = false;
                        ComputedForecast_ForDelivery_Dialog frm = new ComputedForecast_ForDelivery_Dialog(jaBody,branch);
                        frm.ShowDialog();
                        if (ComputedForecast_ForDelivery_Dialog.isSubmit)
                        {
                            foreach (DataRow row in dtGlobal.Rows)
                            {
                                row["final_for_delivery"] = DBNull.Value;
                            }
                            loadData(gridControl1, gridView1, loadPopUpValues(true, gridControl1.Name));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.ToString());
            }


            //apic.loadData("/api/forecast/target_for_delivery/new", "", "application/json", "", RestSharp.Method.POST, true);
        }

        private void gridView1_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
           try
            {
                string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
                if (selectedColumnfieldName.Equals("final_for_delivery"))
                {
                    double doubleTemp = 0.00;
                    int intTemp = 0;
                    bool isFinalBase = false;
                    double finalBase = 0.00;
                    if (double.TryParse(e.Value.ToString(), out doubleTemp))
                    {
                        finalBase = Convert.ToDouble(e.Value.ToString());
                        isFinalBase = true;
                    }
                    int id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
                    //dtGlobal.Rows[id]["final_for_delivery"] = finalBase;
                    //dtGlobal.AcceptChanges();
                    foreach (DataRow row in dtGlobal.Rows)
                    {
                        if (id.ToString() == row["id"].ToString())
                        {
                            row["final_for_delivery"] = isFinalBase ? finalBase : (object) DBNull.Value;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnCreateForProduction_Click(object sender, EventArgs e)
        {
            string branch = "";
            int checkFinalProdQty = 0;
            JArray jaRows = new JArray();
            for (int i =0; i< gridView2.RowCount; i++)
            {
                double finalProdQty = 0.00,targetForProd=0.00, doubleTemp = 0.00;
                bool isFinalProdQty = false, isTargetForProd = false;
                if (double.TryParse(gridView2.GetRowCellValue(i, "final_prod_qty").ToString(), out doubleTemp))
                {
                    finalProdQty = Convert.ToDouble(gridView2.GetRowCellValue(i, "final_prod_qty").ToString());
                    isFinalProdQty = true;
                }
                else
                {
                    checkFinalProdQty++;
                }
                if (double.TryParse(gridView2.GetRowCellValue(i, "target_for_prod").ToString(), out doubleTemp))
                {
                    targetForProd = Convert.ToDouble(gridView2.GetRowCellValue(i, "target_for_prod").ToString());
                    isTargetForProd = true;
                }

                branch = gridView2.GetRowCellValue(i, "branch").ToString();
                string itemCode = gridView2.GetRowCellValue(i, "item_code").ToString();
                string uom = gridView2.GetRowCellValue(i, "uom").ToString();


                JObject joRows = new JObject();
                joRows.Add("item_code", itemCode);
                joRows.Add("targeted_qty", isTargetForProd ?  targetForProd : (double?) null);
                joRows.Add("planned_qty", isFinalProdQty ? finalProdQty : (double?)null);
                joRows.Add("uom", uom);
                jaRows.Add(joRows);
            }
            if(checkFinalProdQty > 0)
            {
                MessageBox.Show("You have unfilled Final Prod Quantity!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                ComputedForecast_ForProduction_Dialog.isSubmit = false;
                ComputedForecast_ForProduction_Dialog frm = new ComputedForecast_ForProduction_Dialog(jaRows, branch);
                frm.ShowDialog();
                if (ComputedForecast_ForProduction_Dialog.isSubmit)
                {
                    loadData(gridControl2, gridView2, loadPopUpValues(false, gridControl2.Name));
                }
            }
        }

        private void gridView1_ColumnFilterChanged(object sender, EventArgs e)
        {
            try
            {
                var val = gridView1.Columns["item_code"].FilterInfo.FilterString;
                if(val.Length > 15)
                {
                    string val15 = val.Substring(15);
                    char v = val15[0];
                    if (char.ToString(v) == "(")
                    {
                        string s1 = val15.Remove(0, 1);
                        string s2 = s1.Remove(s1.Length-1,1);
                        string[] s = s2.Split(',');
                        //gridView1.Columns["item_code"].FilterInfo = new ColumnFilterInfo(new DevExpress.Data.Filtering.InOperator("item_code", s));
                    }
                    else
                    {
                        Console.WriteLine("false: " + "'" + val15);
                    }
                    //if (val.Substring(15, 15).Equals("("))
                    //{
                    //    Console.WriteLine(val.Substring(15));
                    //}
                    //else
                    //{
                    //    Console.WriteLine("false: " + val.Substring(15));
                    //}
                }else
                {
                    Console.WriteLine("no value found ");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public DataTable createNewDt(string columnName, GridView view)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("name", typeof(string));
            string item = "";
            foreach (DataRow row in dtGlobal.Rows)
            {
                string s = row[columnName].ToString();
                if (!string.IsNullOrEmpty(s.Trim()))
                {
                    if (item != s)
                    {
                        dt.Rows.Add(s);
                    }
                }
                item = s;
            }
            DataView dview = new DataView(dt);
            dt = dview.ToTable(true, "name");
            return dt;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                customFiltering_ForDelivery.selectedValues = null;
                customFiltering_ForDelivery.isAll = false;
                customFiltering_ForDelivery.forDeliveryName = "sForDelivery_Item";
                DataTable dt = createNewDt("item_code", gridView1);
                customFiltering_ForDelivery frm = new customFiltering_ForDelivery(sForDelivery_Item, "Item", dt);
                frm.ShowDialog();
                this.Focus();
                if (customFiltering_ForDelivery.selectedValues != null)
                {
                    sForDelivery_Item = customFiltering_ForDelivery.selectedValues;
                }
                //sItemSelected = string.IsNullOrEmpty(sItemSelected.Trim()) ? "" : sItemSelected.Substring(0, sItemSelected.Length - 1);
                //string[] s = sItemSelected.Split(',');
                loadData(gridControl1, gridView1, loadPopUpValues(true, gridControl1.Name));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                customFiltering_ForDelivery.selectedValues = null;
                customFiltering_ForDelivery.isAll = false;
                customFiltering_ForDelivery.forDeliveryName = "sForDelivery_Branch";
                DataTable dt = createNewDt("branch", gridView1);
                customFiltering_ForDelivery frm = new customFiltering_ForDelivery(sForDelivery_Branch, "Branch", dt);
                frm.ShowDialog();
                this.Focus();
                if (customFiltering_ForDelivery.selectedValues != null)
                {
                    sForDelivery_Branch = customFiltering_ForDelivery.selectedValues;
                }
                //sItemSelected = string.IsNullOrEmpty(sItemSelected.Trim()) ? "" : sItemSelected.Substring(0, sItemSelected.Length - 1);
                //string[] s = sItemSelected.Split(',');
                loadData(gridControl1, gridView1, loadPopUpValues(true, gridControl1.Name));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void gridView1_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
        }
    }
}
