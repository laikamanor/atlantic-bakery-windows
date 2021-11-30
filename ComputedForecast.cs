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
using AB.API_Class.Branch;
using DevExpress.XtraGrid.Columns;
using System.Globalization;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using RestSharp;

namespace AB
{
    public partial class ComputedForecast : Form
    {
        public ComputedForecast()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        DataTable dtBranches = new DataTable();
        branch_class branchc = new branch_class();
        devexpress_class devc = new devexpress_class();
        DataTable dtGlobal = new DataTable(), dtProdWhse = new DataTable();
        public static DataTable dtSelectedBranches = new DataTable();
        private void ComputedForecast_Load(object sender, EventArgs e)
        {
            dtSelectedBranches.Columns.Clear();
            dtSelectedBranches.Columns.Add("select", typeof(bool));
            dtSelectedBranches.Columns.Add("name", typeof(string));
            dtSelectedBranches.Columns.Add("type", typeof(string));
            dtSelectedBranches.Columns.Add("is_all", typeof(bool));
            dtSelectedBranches.Rows.Clear();
            dtFromDate.EditValue = dtToDate.EditValue = dtEndingDate.EditValue = DateTime.Now;
            cmbFromTime.SelectedIndex = cmbEndingTime.SelectedIndex = 0;
            cmbToTime.SelectedIndex = cmbToTime.Properties.Items.Count - 1;
            loadBranch();
            loadProdWhse();
            bg();
        }

        public void loadProdWhse()
        {
            try
            {
                cmbProdWhse.Invoke(new Action(delegate ()
                {
                    cmbProdWhse.Properties.Items.Clear();
                }));
                string sResult = "";
                sResult = apic.loadData("/api/whse/get_all?is_production=True", "", "", "", Method.GET, true);
                if (sResult.Substring(0, 1).Equals("{"))
                {
                    dtProdWhse = apic.getDtDownloadResources(sResult, "data");
                    if (IsHandleCreated)
                    {
                        cmbProdWhse.Invoke(new Action(delegate ()
                        {
                            cmbProdWhse.Properties.Items.Add("All");
                        }));
                    }
                    foreach (DataRow row in dtProdWhse.Rows)
                    {
                        if (IsHandleCreated)
                        {
                            cmbProdWhse.Invoke(new Action(delegate ()
                            {
                                cmbProdWhse.Properties.Items.Add(row["whsename"].ToString());
                            }));
                        }

                    }
                    if (IsHandleCreated)
                    {
                        cmbProdWhse.Invoke(new Action(delegate ()
                        {
                            cmbProdWhse.SelectedIndex = 0;
                        }));
                    }
                }
                else
                {
                    apic.showCustomMsgBox("Validation", sResult);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public async void loadBranch()
        {
            try
            {
                dtBranches = await branchc.returnBranches();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        public void loadData()
        {
            try
            {
                string sFromDate = "?from_date=", sToDate = "&to_date=", sFromTime = "&from_time=", sToTime = "&to_time=", sEndingDate = "&ending_date=", sEndingTime = "&ending_time=", sWithLastBal = "&with_last_bal=", sMultipler = "&multiplier=", sProdWhse= "&prod_whse=", appendParams = "";
                dtFromDate.Invoke(new Action(delegate ()
                {
                    sFromDate += (checkFromDate.Checked ? dtFromDate.Text : "");
                }));
                dtToDate.Invoke(new Action(delegate ()
                {
                    sToDate += (checkToDate.Checked ? dtToDate.Text : "");
                }));
                cmbFromTime.Invoke(new Action(delegate ()
                {
                    sFromTime += (checkFromTime.Checked ? cmbFromTime.Text : "");
                }));
                cmbToTime.Invoke(new Action(delegate ()
                {
                    sToTime += (checkToTime.Checked ? cmbToTime.Text : "");
                }));
                cmbEndingTime.Invoke(new Action(delegate ()
                {
                    sEndingTime += (checkEndingTime.Checked ? cmbEndingTime.Text : "");
                }));
                dtEndingDate.Invoke(new Action(delegate ()
                {
                    sEndingDate += (checkEndingDate.Checked ? dtEndingDate.Text : "");
                }));
                checkWithLastBal.Invoke(new Action(delegate ()
                {
                    sWithLastBal += (checkWithLastBal.Checked ? "1" : "0");
                }));
                cmbProdWhse.Invoke(new Action(delegate ()
                {
                    sProdWhse += apic.findValueInDataTable(dtProdWhse, cmbProdWhse.Text, "whsename", "whsecode");
                }));
                txtMultiplier.Invoke(new Action(delegate ()
                {
                    long intTemp = 0;
                    sMultipler += long.TryParse(txtMultiplier.Text, out intTemp) ? Convert.ToInt64(txtMultiplier.Text) : intTemp;
                }));
                appendParams = sFromDate + sToDate + sFromTime + sToTime + sEndingDate + sEndingTime + sWithLastBal + sMultipler + sProdWhse;
                string sResult = apic.loadData("/api/forecast/computed_forecast", appendParams, "", "", RestSharp.Method.GET, true);
                //Console.WriteLine(sResult);
                JObject joResult = JObject.Parse(sResult) == null || string.IsNullOrEmpty(sResult.ToString().Trim()) || !sResult.ToString().Substring(0, 1).Equals("{") ? new JObject() : JObject.Parse(sResult);
                JArray jaData = (JArray)joResult["data"] == null || string.IsNullOrEmpty(joResult["data"].ToString().Trim()) || !joResult["data"].ToString().Substring(0, 1).Equals("[") ? new JArray() : (JArray)joResult["data"];
                DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
                gridControl1.Invoke(new Action(delegate ()
                {
                    dtGlobal = dtData;
                    loadUI(loadPopUpValues(false, ""));
                }));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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

            int tcIndex = 0;
            loadUI(loadPopUpValues(false, ""));
        }

        public DataTable loadPopUpValues(bool isOpen, string itemCode)
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
                toolTip1.SetToolTip(labelSelectedBranch, labelSelectedBranch.Text);
                toolTip1.SetToolTip(labelItem, labelItem.Text);
                sBranchSelected = string.IsNullOrEmpty(sBranchSelected.Trim()) ? "" : "branch IN (" + sBranchSelected + ")" + (string.IsNullOrEmpty(sItemSelected.Trim()) ? " " : " AND ");
                sItemSelected = string.IsNullOrEmpty(sItemSelected.Trim()) ? "" : "item_code IN (" + sItemSelected + ")";
                string appendParams = sBranchSelected + sItemSelected;
                if (dt.Rows.Count > 0)
                {
                    if (dtSelectedBranches.Rows.Count > 0)
                    {
                        //Console.WriteLine("append " + appendParams);
                        DataRow[] results = dt.Select(appendParams);
                        //Console.WriteLine("append: " + appendParams);
                        string aaa = appendParams;
                        DataTable dtt = new DataTable();
                        if (results.Any())
                        {
                            dtt = results.CopyToDataTable();
                            dtResult = loadFormat(dtt, isOpen, itemCode);
                        }
                        else
                        {
                            //null
                            //DataTable dt2 = new DataTable();
                            //loadSettings(dt2);
                            dtResult= null;
                        }
                    }
                    else
                    {
                        //same pa rin pero di naka filter
                        //dt = loadMe(dt, itemCode, status);
                        //loadSettings(dt);
                        dtResult = loadFormat(dt, isOpen, itemCode);
                    }
                }
                else
                {
                    dtResult = null;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dtResult;
        }

        public DataTable loadFormat(DataTable dtFiltered, bool isOpen, string itemCode)
        {
            DataTable dtResult = new DataTable();
            try
            {
                double doubleTemp = 0.00;
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
                    dt1.Rows.Add(j.ItemCode, j.TotalSold);
                }

                var queryy = (from row in dtFiltered.AsEnumerable()
                              join row2 in dt1.AsEnumerable() on row.Field<string>("item_code") equals row2.Field<string>("item_code")
                              into grp
                              select new
                              {
                                  ItemCode = row.Field<string>("item_code") == null ? "" : row.Field<string>("item_code"),
                                  TotalSold = grp.Sum(r => r.Field<double?>("total_sold_as_per_selected_branch") == null ? 0 : r.Field<double>("total_sold_as_per_selected_branch")),
                                  Branch = row.Field<string>("branch") == null ? "" : row.Field<string>("branch"),
                                  Sold = row.Field<double?>("sold") == null ? 0 : row.Field<double>("sold"),
                                  DateDiff = row.Field<Int64?>("datediff") == null ? 0 : row.Field<Int64>("datediff"),
                                  Average = row.Field<double?>("average") == null ? 0 : row.Field<double>("average"),
                                  LastBal = row.Field<double?>("last_bal") == null ? 0 : row.Field<double>("last_bal"),
                                  TargetForDel = row.Field<double?>("target_for_del") == null ? 0 : row.Field<double>("target_for_del"),
                                  ProdMinQty = row.Field<dynamic>("prod_min_qty") == null ? 0.00 : row.Field<dynamic>("prod_min_qty")
                              }).ToList();


                DataTable dt = new DataTable();
                dt.Columns.Add("item_code", typeof(string));
                dt.Columns.Add("total_sold_as_per_selected_branch", typeof(double));
                if (isOpen)
                {
                    dt.Columns.Add("branch", typeof(string));
                    dt.Columns.Add("total_sold_as_per_branch", typeof(double));
                    dt.Columns.Add("average", typeof(double));
                    dt.Columns.Add("multiplier", typeof(long));
                    dt.Columns.Add("last_bal", typeof(double));
                    dt.Columns.Add("target_for_del", typeof(double));
                }
                //dt.Columns.Add("prod_min_qty", typeof(double));

                long multi = 0, intTemp = 0;
                multi = long.TryParse(txtMultiplier.Text, out intTemp) ? Convert.ToInt64(txtMultiplier.Text) : intTemp;
                string item = "";
                foreach (var j in queryy)
                {
                    if (isOpen)
                    {
                        if (j.ItemCode == itemCode)
                        {
                            if (!item.Equals(j.ItemCode))
                            {
                                dt.Rows.Add(j.ItemCode, j.TotalSold, "", (double?)null, (double?)null, (Int64?)null, (double?)null, (double?)null);
                            }
                            dt.Rows.Add("", (double?)null, j.Branch, j.Sold, j.Average, multi, j.LastBal, j.TargetForDel);
                        }
                    }
                    else
                    {
                        if (!item.Equals(j.ItemCode))
                        {
                            dt.Rows.Add(j.ItemCode, j.TotalSold);
                        }
                    }
                    item = j.ItemCode;
                }

                if (isOpen)
                {
                    DataView view = new DataView(dt);
                    dtResult = view.ToTable(true, "item_code", "total_sold_as_per_selected_branch", "branch", "total_sold_as_per_branch", "average", "multiplier", "last_bal", "target_for_del");
                }
                else
                {
                    dtResult = dt;
                    //foreach(DataRow row in dtResult.Rows)
                    //{
                    //    //Console.WriteLine("row " + row[0]);
                    //}
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dtResult;
        }

        public void loadUI(DataTable dtFinal)
        {
            try
            {
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
                gridControl1.DataSource = dtFinal;

                foreach (GridColumn col in gridView1.Columns)
                {
                    string fieldName = col.FieldName;
                    string fieldName2 = fieldName.Replace("_", " ");
                    col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fieldName2);
                    col.ColumnEdit = repositoryItemTextEdit1;
                    col.DisplayFormat.FormatType = col.GetCaption().Equals("branch") || col.GetCaption().Equals("item_code") ? DevExpress.Utils.FormatType.None : DevExpress.Utils.FormatType.Numeric;
                    col.DisplayFormat.FormatString = col.GetCaption().Equals("branch") || col.GetCaption().Equals("item_code") ? "" : col.GetCaption().ToLower().Equals("multiplier") ? "N0" : "n2";

                    //fonts
                    FontFamily fontArial = new FontFamily("Arial");
                    col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                    col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                }
                var mySoldColumn = gridView1.Columns.FirstOrDefault((col) => col.FieldName == "total_sold_as_per_branch");
                var myItemColumn = gridView1.Columns.FirstOrDefault((col) => col.FieldName == "item_code");
                if (mySoldColumn == null && myItemColumn != null)
                {
                    gridView1.Columns["item_code"].Summary.Clear();
                    gridView1.Columns["total_sold_as_per_selected_branch"].Summary.Clear();
                    gridView1.Columns["item_code"].Summary.Add(DevExpress.Data.SummaryItemType.Count, "item_code", "Total: {0:N0}");
                    gridView1.Columns["total_sold_as_per_selected_branch"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "total_sold_as_per_selected_branch", "Total Sold As Per Selected Branch: {0:n2}");
                }
                //else if(myItemColumn==null && mySoldColumn == null)
                //{

                //}
                else if(mySoldColumn!= null && mySoldColumn==null)
                {
                    gridView1.Columns["branch"].Summary.Clear();
                    gridView1.Columns["total_sold_as_per_branch"].Summary.Clear();
                    gridView1.Columns["item_code"].Summary.Clear();
                    gridView1.Columns["total_sold_as_per_selected_branch"].Summary.Clear();

                    gridView1.Columns["branch"].Summary.Add(DevExpress.Data.SummaryItemType.Custom, "branch", "Total: " + (gridView1.RowCount - 1).ToString("N0"));
                    gridView1.Columns["total_sold_as_per_branch"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "total_sold_as_per_branch", "Total Sold As Per Branch: {0:n2}");
                }
                gridView1.OptionsView.ColumnAutoWidth = false;
                //auto complete
                string[] suggestions = { "reference" };
                string suggestConcat = string.Join(";", suggestions);
                gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                devc.loadSuggestion(gridView1, gridControl1, suggestions);
                gridView1.BestFitColumns();
            }
            catch(Exception ex)
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

        private void checkFromDate_CheckedChanged(object sender, EventArgs e)
        {
            dtFromDate.Visible = checkFromDate.Checked;
        }

        private void checkToDate_CheckedChanged(object sender, EventArgs e)
        {
            dtToDate.Visible = checkToDate.Checked;
        }

        private void checkFromTime_CheckedChanged(object sender, EventArgs e)
        {
            cmbFromTime.Visible = checkFromTime.Checked;
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            long intTemp = 0;
            if (string.IsNullOrEmpty(txtMultiplier.Text.Trim()))
            {
                MessageBox.Show("Multiplier field is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!long.TryParse(txtMultiplier.Text.Trim(), out intTemp))
            {
                MessageBox.Show("Multiplier field must be a number!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                bg();
            }
        }

        private void checkToTime_CheckedChanged(object sender, EventArgs e)
        {
            cmbToTime.Visible = checkToTime.Checked;
        }

        private void checkEndingDate_CheckedChanged(object sender, EventArgs e)
        {
            dtEndingDate.Visible = checkEndingDate.Checked;
        }

        private void checkEndingTime_CheckedChanged(object sender, EventArgs e)
        {
            cmbEndingTime.Visible = checkEndingTime.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            showFilter("Branch", dtBranches);
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

        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
           try
            {
                string selectedColumnText = gridView1.FocusedColumn.FieldName;
                string itemCode = gridView1.GetFocusedRowCellValue("item_code").ToString();
                var mySoldColumn = gridView1.Columns.FirstOrDefault((col) => col.FieldName == "total_sold_as_per_branch");
                if (selectedColumnText.Equals("total_sold_as_per_selected_branch"))
                {
                    if (mySoldColumn == null)
                    {
                        loadUI(loadPopUpValues(true, itemCode));
                    }
                    else
                    {
                        loadUI(loadPopUpValues(false, ""));
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearchQuery1_Click(object sender, EventArgs e)
        {
            long intTemp = 0;
            if (string.IsNullOrEmpty(txtMultiplier.Text.Trim()))
            {
                MessageBox.Show("Multiplier field is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!long.TryParse(txtMultiplier.Text.Trim(), out intTemp))
            {
                MessageBox.Show("Multiplier field must be a number!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                bg();
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            DataTable dt = dtGlobal;
            string sBranchSelected = "";
            string sItemSelected = "";
            bool isAllBranch = false, isAllItem = false, boolTemp = false;
            foreach (DataRow row in dtSelectedBranches.Rows)
            {
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
            toolTip1.SetToolTip(labelSelectedBranch, labelSelectedBranch.Text);
            toolTip1.SetToolTip(labelItem, labelItem.Text);
            sBranchSelected = string.IsNullOrEmpty(sBranchSelected.Trim()) ? "" : "branch IN (" + sBranchSelected + ")" + (string.IsNullOrEmpty(sItemSelected.Trim()) ? " " : " AND ");
            sItemSelected = string.IsNullOrEmpty(sItemSelected.Trim()) ? "" : "item_code IN (" + sItemSelected + ")";
            string appendParams = sBranchSelected + sItemSelected;
            if (dt.Rows.Count > 0)
            {
                if (dtSelectedBranches.Rows.Count > 0)
                {
                    //Console.WriteLine("append " + appendParams);
                    DataRow[] results = dt.Select(appendParams);
                    //Console.WriteLine("append: " + appendParams);
                    string aaa = appendParams;
                    if (results.Any())
                    {
                        dt = results.CopyToDataTable();
                    }
                }
            }
            if(dt.Rows.Count > 0)
            {
                DataTable dtFinal = new DataTable();
                dtFinal.Columns.Add("id", typeof(int));
                dtFinal.Columns.Add("branch", typeof(string));
                dtFinal.Columns.Add("item_code", typeof(string));
                dtFinal.Columns.Add("uom", typeof(string));
                dtFinal.Columns.Add("sold", typeof(double));
                dtFinal.Columns.Add("datediff", typeof(Int64));
                dtFinal.Columns.Add("average", typeof(double));
                dtFinal.Columns.Add("last_bal", typeof(double));
                dtFinal.Columns.Add("target_for_del", typeof(double));
                dtFinal.Columns.Add("prod_min_qty", typeof(double));
                dtFinal.Columns.Add("final_for_delivery", typeof(double));
                dtFinal.Columns.Add("prod_ending_bal", typeof(double));
                double doubleTemp = 0.00;
                int intTemp = 0, id = 0;
                foreach (DataRow row in dt.Rows)
                {
                    id++;
                    string itemCode = row["item_code"].ToString(),
                        branch = row["branch"].ToString(),
                        uom = row["uom"].ToString();
                    double sold = double.TryParse(row["sold"].ToString(), out doubleTemp) ? Convert.ToDouble(row["sold"].ToString()) : doubleTemp,
                        dateDiff = double.TryParse(row["datediff"].ToString(), out doubleTemp) ? Convert.ToDouble(row["datediff"].ToString()) : doubleTemp,
                        avg = double.TryParse(row["average"].ToString(), out doubleTemp) ? Convert.ToDouble(row["average"].ToString()) : doubleTemp,
                        lastBal = double.TryParse(row["last_bal"].ToString(), out doubleTemp) ? Convert.ToDouble(row["last_bal"].ToString()) : doubleTemp,
                        targetForDel = double.TryParse(row["target_for_del"].ToString(), out doubleTemp) ? Convert.ToDouble(row["target_for_del"].ToString()) : doubleTemp,
                        prodMinQty = double.TryParse(row["prod_min_qty"].ToString(), out doubleTemp) ? Convert.ToDouble(row["prod_min_qty"].ToString()) : doubleTemp,
                         ProdEndingBal = double.TryParse(row["prod_ending_bal"].ToString(), out doubleTemp) ? Convert.ToDouble(row["prod_ending_bal"].ToString()) : doubleTemp;
                    dtFinal.Rows.Add(id, branch, itemCode, uom, sold, dateDiff, avg, lastBal, targetForDel, prodMinQty, targetForDel, ProdEndingBal);
                }
                ComputedForecast_ForDelivery frm = new ComputedForecast_ForDelivery();
                frm.dtSelectedBranches = dtSelectedBranches;
                frm.dtGlobal = dtFinal;
                frm.ShowDialog();
            }else
            {
                MessageBox.Show("No rows found!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtMultiplier_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
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
    }
}