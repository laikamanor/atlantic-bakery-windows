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
using DevExpress.XtraGrid;
using DevExpress.Utils.Menu;

namespace AB
{
    public partial class CreateForDeliveryProduction : Form
    {
        public CreateForDeliveryProduction()
        {
            InitializeComponent();
        }
        DataTable dtProdWhse = new DataTable();
        api_class apic = new api_class();
        DataTable dtBranches = new DataTable(), dtGlobal = new DataTable(), dtCurrentGlobal = new DataTable();
        Dictionary<int, double> dicFinalForDelivery = new Dictionary<int, double>();
        devexpress_class devc = new devexpress_class();
        string[] selectedItems, selectedBranches, selectedPremix;
        bool isAllItem = false, isAllBranch = false, isAllPremix = false;
        int lastTopIndex = 0;
        DataTable dtForProd = new DataTable(), dtBranch = new DataTable(), dtItem = new DataTable(), dtBom = new DataTable(), dtItemGroup = new DataTable();
        BackgroundWorker bgSubmit = new BackgroundWorker();
        BackgroundWorker bgFiltering = new BackgroundWorker();
        DataTable dtManualItem = new DataTable();
        private void CreateForDeliveryProduction_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            dtFromDate.EditValue = dtToDate.EditValue = dtEndingDate.EditValue = DateTime.Now;
            cmbFromTime.SelectedIndex = cmbEndingTime.SelectedIndex = 0;
            cmbToTime.SelectedIndex = cmbToTime.Properties.Items.Count - 1;

            txtMultiplier.Text = "1";
            loadProdWhse();
            loadItemGroup();
            loadBranch();
            loadItem();
            loadBOM();
            bg(backgroundWorker1);
        }


        public void loadBranch()
        {
            string sResult = apic.loadData("/api/branch/get_all", "", "", "", RestSharp.Method.GET, true);
            if (sResult.StartsWith("{"))
            {
                JObject joResult = string.IsNullOrEmpty(sResult.ToString().Trim()) || !sResult.ToString().Substring(0, 1).Equals("{") ? new JObject() : JObject.Parse(sResult);
                JArray jaData = (JArray)joResult["data"] == null || string.IsNullOrEmpty(joResult["data"].ToString().Trim()) || !joResult["data"].ToString().Substring(0, 1).Equals("[") ? new JArray() : (JArray)joResult["data"];
                dtBranch = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
            }
        }

        public void loadItemGroup()
        {
            string sResult = apic.loadData("/api/item/item_grp/getall", "", "", "", RestSharp.Method.GET, true);
            if (sResult.StartsWith("{"))
            {
                JObject joResult = string.IsNullOrEmpty(sResult.ToString().Trim()) || !sResult.ToString().Substring(0, 1).Equals("{") ? new JObject() : JObject.Parse(sResult);
                JArray jaData = (JArray)joResult["data"] == null || string.IsNullOrEmpty(joResult["data"].ToString().Trim()) || !joResult["data"].ToString().Substring(0, 1).Equals("[") ? new JArray() : (JArray)joResult["data"];
                dtItemGroup = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
            }
        }

        public void loadBOM()
        {
            string sResult = apic.loadData("/api/bom/get_all", "", "", "", RestSharp.Method.GET, true);
            if (sResult.StartsWith("{"))
            {
                JObject joResult = string.IsNullOrEmpty(sResult.ToString().Trim()) || !sResult.ToString().Substring(0, 1).Equals("{") ? new JObject() : JObject.Parse(sResult);
                JArray jaData = (JArray)joResult["data"] == null || string.IsNullOrEmpty(joResult["data"].ToString().Trim()) || !joResult["data"].ToString().Substring(0, 1).Equals("[") ? new JArray() : (JArray)joResult["data"];
                dtBom = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
            }
        }

        public void loadItem()
        {
            string sResult = apic.loadData("/api/item/getall", "?is_active=1", "", "", RestSharp.Method.GET, true);
            if (sResult.StartsWith("{"))
            {
                JObject joResult = string.IsNullOrEmpty(sResult.ToString().Trim()) || !sResult.ToString().Substring(0, 1).Equals("{") ? new JObject() : JObject.Parse(sResult);
                JArray jaData = (JArray)joResult["data"] == null || string.IsNullOrEmpty(joResult["data"].ToString().Trim()) || !joResult["data"].ToString().Substring(0, 1).Equals("[") ? new JArray() : (JArray)joResult["data"];
                dtItem = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
            }
        }

        public void bg(BackgroundWorker bgw1)
        {
            if (!bgw1.IsBusy)
            {
                closeForm();
                Loading frm = new Loading();
                frm.Show();
                bgw1.RunWorkerAsync();
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

        public void loadProdWhse()
        {
            try
            {
                cmbProdWhse.Invoke(new Action(delegate ()
                {
                    cmbProdWhse.Properties.Items.Clear();
                    cmbProdWhse.Properties.Items.Add("");
                }));
                string sResult = "";
                sResult = apic.loadData("/api/whse/get_all?is_production=True", "", "", "", Method.GET, true);
                if (sResult.Substring(0, 1).Equals("{"))
                {
                    dtProdWhse = apic.getDtDownloadResources(sResult, "data");
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


        public void loadQuery(bool isOpen)
        {
            gridControl1.Invoke(new Action(delegate ()
            {
                gridControl1.DataSource = null;
            }));
            try
            {
                bool isProdWhse = false;

                cmbProdWhse.Invoke(new Action(delegate ()
                {
                    isProdWhse = string.IsNullOrEmpty(cmbProdWhse.Text.Trim()) || cmbProdWhse.SelectedIndex == -1;
                }));
                if (isProdWhse)
                {   
                    panelProdWhse.Invoke(new Action(delegate ()
                    {
                        panelProdWhse.BackColor = Color.Red;
                    }));

                    apic.showCustomMsgBox("Validation", "Please select Production Warehouse first!");
                }
                else
                {
                    string sFromDate = "?from_date=", sToDate = "&to_date=", sFromTime = "&from_time=", sToTime = "&to_time=", sEndingDate = "&ending_date=", sEndingTime = "&ending_time=", sWithLastBal = "&with_last_bal=", sMultipler = "&multiplier=", sProdWhse = "&prod_whse=", appendParams = "";
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
                        sEndingTime += checkEndingTime.Checked ? cmbEndingTime.Text : "";
                    }));
                    dtEndingDate.Invoke(new Action(delegate ()
                    {
                        sEndingDate += checkEndingDate.Checked ? dtEndingDate.Text : "";
                    }));
                    checkWithLastBal.Invoke(new Action(delegate ()
                    {
                        sWithLastBal += (checkWithLastBal.Checked ? "1" : "0");
                        if (!checkWithLastBal.Checked)
                        {
                            sEndingDate = sEndingTime = "";
                        }
                    }));
                    cmbProdWhse.Invoke(new Action(delegate ()
                    {
                        sProdWhse += apic.findValueInDataTable(dtProdWhse, cmbProdWhse.Text, "whsename", "whsecode");
                    }));
                    txtMultiplier.Invoke(new Action(delegate ()
                    {
                        double doubleTemp = 0.00;
                        sMultipler += double.TryParse(txtMultiplier.Text, out doubleTemp) ? Convert.ToDouble(txtMultiplier.Text) : doubleTemp;
                    }));
                    appendParams = sFromDate + sToDate + sFromTime + sToTime + sEndingDate + sEndingTime + sWithLastBal + sMultipler + sProdWhse;
                    string sResult = apic.loadData("/api/forecast/computed_forecast", appendParams, "", "", RestSharp.Method.GET, true);
                    if (sResult.StartsWith("{"))
                    {
                        JObject joResult = string.IsNullOrEmpty(sResult.ToString().Trim()) || !sResult.ToString().Substring(0, 1).Equals("{") ? new JObject() : JObject.Parse(sResult);
                        JArray jaData = (JArray)joResult["data"] == null || string.IsNullOrEmpty(joResult["data"].ToString().Trim()) || !joResult["data"].ToString().Substring(0, 1).Equals("[") ? new JArray() : (JArray)joResult["data"];
                        DataTable dtForeCast = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));

                        btnAddTransaction.Invoke(new Action(delegate ()
                        {
                            btnAddTransaction.Enabled  = true;
                        }));
                        btnAddTransaction2.Invoke(new Action(delegate ()
                        {
                            btnAddTransaction2.Enabled = true;
                        }));

                        dtGlobal = new DataTable();
                        dtGlobal.Columns.Add("branch", typeof(string));
                        dtGlobal.Columns.Add("item_code", typeof(string));
                        dtGlobal.Columns.Add("sold", typeof(double));
                        dtGlobal.Columns.Add("datediff", typeof(Int64));

                        dtGlobal.Columns.Add("average", typeof(double));
                        dtGlobal.Columns.Add("last_bal", typeof(double));
                        dtGlobal.Columns.Add("target_for_del", typeof(double));

                        dtGlobal.Columns.Add("prod_min_qty", typeof(double));
                        dtGlobal.Columns.Add("prod_ending_bal", typeof(double));
                        dtGlobal.Columns.Add("uom", typeof(string));
                        dtGlobal.Columns.Add("premix_code", typeof(string));


                        foreach (DataRow row in dtBranch.Rows)
                        {
                            foreach (DataRow row2 in dtItem.Rows)
                            {
                                string sItemCode = row2["item_code"].ToString().Replace(@"'", "''");
                                string sBranch = row["code"].ToString().Replace(@"'", "''");
                                if(dtForeCast.Rows.Count > 0)
                                {
                                    DataRow[] lists = dtForeCast.Select("item_code='" + sItemCode + "' AND branch='" + sBranch + "'");

                                    if (lists.Length > 0)
                                    {
                                        foreach (DataRow row3 in lists)
                                        {

                                            dtGlobal.Rows.Add(row["code"].ToString(), row2["item_code"].ToString(), row3["sold"], row3["datediff"], row3["average"], row3["last_bal"], row3["target_for_del"], row3["prod_min_qty"], row3["prod_ending_bal"], row2["uom"].ToString(), row2["premix_code"].ToString());
                                        }
                                    }
                                    else
                                    {
                                        double baseQty = 0.00, doubleTemp = 0.00;
                                        DataRow[] listsBom = dtBom.Select("item_code='" + sItemCode + "'");
                                        if (listsBom.Length > 0)
                                        {
                                            baseQty = double.TryParse(listsBom[0]["quantity"].ToString(), out doubleTemp) ? Convert.ToDouble(listsBom[0]["quantity"].ToString()) : doubleTemp;
                                        }
                                        dtGlobal.Rows.Add(row["code"].ToString(), row2["item_code"].ToString(), null, null, null, 0.00, 0.00, baseQty, 0.00, row2["uom"].ToString(), row2["premix_code"].ToString());
                                    }
                                }

                            }
                        }

                        //DataView view = new DataView(dtGlobal);
                        //DataTable distinctItems = view.ToTable(true, "item_code");

                        //foreach (DataRow row in dtItem.Rows)
                        //{
                        //    string s = row["item_code"].ToString().Replace(@"'", "''");
                        //    DataRow[] contacts = distinctItems.Select("item_code='" + s + "'");
                        //    if (contacts.Length == 0)
                        //    {
                        //        dtGlobal.Rows.Add(null, s, 0.00, DBNull.Value, 0.00, 0.00, 0.00, DBNull.Value, 0.00, row["uom"].ToString(), row["premix_code"].ToString());
                        //    }
                        //}

                        //DataTable dtt = dtGlobal.Copy();
                        ////group
                        //DataTable dtNew = dtt.AsEnumerable()
                        //.GroupBy(r => new { Col1 = r["item_code"] })
                        //.Select(g =>
                        //{
                        //    var row = dtt.NewRow();

                        //    row["item_code"] = g.Key.Col1;
                        //    row["branch"] = g.FirstOrDefault().Field<string>("branch");
                        //    row["sold"] = g.FirstOrDefault().Field<dynamic>("sold");
                        //    row["datediff"] = g.FirstOrDefault().Field<dynamic>("datediff") == null ? 0 : g.FirstOrDefault().Field<dynamic>("datediff");
                        //    row["average"] = g.FirstOrDefault().Field<dynamic>("average");
                        //    row["last_bal"] = g.FirstOrDefault().Field<dynamic>("last_bal");

                        //    row["target_for_del"] = g.FirstOrDefault().Field<dynamic>("target_for_del");
                        //    row["prod_min_qty"] = g.FirstOrDefault().Field<dynamic>("prod_min_qty");
                        //    row["prod_ending_bal"] = g.FirstOrDefault().Field<dynamic>("prod_ending_bal");
                        //    row["uom"] = g.FirstOrDefault().Field<string>("uom");
                        //    row["premix_code"] = g.FirstOrDefault().Field<string>("premix_code");
                        //    return row;

                        //})
                        //.CopyToDataTable();

                        //DataView dv = dtNew.DefaultView;
                        //dv.Sort = "item_code,branch ASC";
                        //DataTable dtTemp = dv.ToTable();

                        //foreach (DataRow row1 in dtNew.Rows)
                        //{
                        //    //Console.WriteLine(row1["item_code"].ToString());
                        //    string s1 = row1["item_code"].ToString().Replace(@"'", "''");
                        //    //DataRow[] lists = dtGlobal.Select("item_code='" + s1 + "'");
                        //    //foreach (DataRow row3 in lists)
                        //    //{
                        //    foreach (DataRow row in dtBranch.Rows)
                        //    {
                        //        string s = row1["item_code"].ToString().Replace(@"'", "''");
                        //        string s2 = row["code"].ToString().Replace(@"'", "''");
                        //        DataRow[] lists2 = dtGlobal.Select("item_code='" + s + "' AND branch='" + s2 + "'");
                        //        if (lists2.Length == 0)
                        //        {
                        //            dtGlobal.Rows.Add(row["code"].ToString(), row1["item_code"].ToString(), 0, row1["datediff"], row1["average"], 0, 0, 0, 0, row1["uom"], row1["premix_code"]);
                        //        }
                        //    }
                        //}

                        dtGlobal.Columns.Add("final_for_delivery", typeof(double));
                        dtGlobal.Columns.Add("row_index", typeof(int));
                        dtGlobal.Columns.Add("final_base", typeof(double));
                        dtGlobal.Columns.Add("final_prod_qty", typeof(double));
                        dtGlobal.Columns.Add("buffer_stock", typeof(double));
                        dtGlobal.Columns.Add("isManualItem", typeof(Boolean));
                        foreach (DataRow row in dtGlobal.Rows)
                        {
                            row["row_index"] = dtGlobal.Rows.IndexOf(row);
                            row["final_for_delivery"] = row["target_for_del"];
                            row["final_prod_qty"] = row["final_base"] = DBNull.Value;
                            row["buffer_stock"] = 0.00;
                            row["isManualItem"] = false;
                        }
                        dtManualItem = dtGlobal.Clone();
                        loadForDelivery(dtGlobal, isOpen, "");
                   
                        loadForProduction("");
                    }
                }
            }
            catch (Exception ex)
            {
                apic.showCustomMsgBox(ex.Message, ex.ToString());
            }
        }

        public void loadForProduction(string selectedItemCode)
        {
            gridControl2.Invoke(new Action(delegate ()
            {
                gridControl2.DataSource = null;
                gridView2.Columns.Clear();
            }));
            gridControl2.Invoke(new Action(delegate ()
            {
                DataTable dtCloned = dtGlobal.Clone();
                if (dtCloned.Columns.Contains("prod_min_qty"))
                {
                    double doubleTemp = 0.00;
                    dtCloned.Columns["prod_min_qty"].DataType = typeof(double);
                    foreach (DataRow row in dtCurrentGlobal.Rows)
                    {
                        row["prod_min_qty"] = double.TryParse(row["prod_min_qty"].ToString(), out doubleTemp) ? Convert.ToDouble(row["prod_min_qty"].ToString()) : doubleTemp;
                        dtCloned.ImportRow(row);
                    }
                    gridControl2.DataSource = null;
                    gridControl2.DataSource = loadForProductionQuery(dtCloned, selectedItemCode);

                    gridView2.OptionsView.ColumnAutoWidth = false;
                    gridView2.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                    foreach (GridColumn col in gridView2.Columns)
                    {
                        string fieldName = col.FieldName;
                        string v = col.GetCaption();
                        string s = v.Replace("_", " ");
                        col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                        col.Caption = fieldName.Equals("for_del_for_prod") ? "Target For Prod / Final For Prod Variance" : fieldName.Equals("prod_min_qty") ? "Base Qty" : fieldName.Equals("target_base") ? "Computed Base" :  fieldName.Equals("target_for_prod") ? "Total Target For Prod" : col.Caption;
                        col.ColumnEdit = fieldName.Equals("final_base") || fieldName.Equals("buffer_stock") ? null : repositoryItemTextEdit3;
                        col.ToolTip = fieldName.Equals("target_for_prod") ? "Target For Prod = Final For Delivery - Prod Ending Balance" : null;
                        col.DisplayFormat.FormatType = fieldName.Equals("branch") || fieldName.Equals("item_code") || fieldName.Equals("uom") ? DevExpress.Utils.FormatType.None : DevExpress.Utils.FormatType.Numeric;
                        col.DisplayFormat.FormatString = fieldName.Equals("branch") || fieldName.Equals("item_code") || fieldName.Equals("uom") ? "" : "n2";
                        //col.DisplayFormat.FormatType = fieldName.Equals("allowed_discount") ? DevExpress.Utils.FormatType.Numeric : DevExpress.Utils.FormatType.None;
                        //col.DisplayFormat.FormatString = fieldName.Equals("allowed_discount") ? "n2" : "";
                        //col.Visible = !(fieldName.Equals("pricelist_id") || fieldName.Equals("id") || fieldName.Equals("date_created") || fieldName.Equals("date_updated") || fieldName.Equals("created_by") || fieldName.Equals("updated_by") || fieldName.Equals("agent_account") || fieldName.Equals("production_whse") || fieldName.Equals("raw_wheat_whse") || fieldName.Equals("igoods_whse") || fieldName.Equals("pack_and_oth_whse") || fieldName.Equals("premix_whse") || fieldName.Equals("cutoff") || fieldName.Equals("is_active") || fieldName.Equals("is_fg") || fieldName.Equals("is_production") || fieldName.Equals("is_igoods") || fieldName.Equals("is_raw_mat") || fieldName.Equals("is_pack_oth") || fieldName.Equals("is_premix") || fieldName.Equals("is_main"));
                        col.Visible = !(fieldName.Equals("branch") || fieldName.Equals("uom") || fieldName.Equals("row_index") || fieldName.Equals("isManualItem"));
                        col.Fixed = fieldName.Equals("item_code") ? FixedStyle.Left : FixedStyle.None;
                        //fonts
                        FontFamily fontArial = new FontFamily("Arial");
                        col.AppearanceHeader.Font = new Font(fontArial, 9, FontStyle.Regular);
                        col.AppearanceCell.Font = new Font(fontArial, 8, FontStyle.Regular);
                    }
                    gridView2.BestFitColumns();
                    foreach (GridColumn col in gridView2.Columns)
                    {
                        string fieldName = col.FieldName;
                        col.Width = !(fieldName.Equals("item_code")) ? 70 : col.Width;
                    }
                }
            }));
        }

        public DataTable loadForDeliveryFormat(DataTable dtFiltered, bool isOpen, string currentItemCode,bool forSubmit)
        {
            DataTable dtResult = new DataTable();
            double doubleTemp = 0.00;
            var query = (from row in dtFiltered.AsEnumerable()
                         group row by new
                         {
                             ItemCode = row.Field<string>("item_code"),
                         } into grp
                         select new
                         {
                             ItemCode = grp.Key.ItemCode,
                             TotalSold = grp.Sum(r => r.Field<double?>("sold") == null ? 0.00 :  r.Field<double>("sold")),
                             TotalFinalForDeivery = grp.Sum(r => r.Field<double?>("final_for_delivery") == null ? 0 : r.Field<double>("final_for_delivery")),
                             TotalTargetForDel = grp.Sum(r => r.Field<double?>("target_for_del") == null ? 0 : r.Field<double>("target_for_del"))
                         }).ToList();
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("item_code", typeof(string));
            dt1.Columns.Add("total_sold_as_per_selected_branch", typeof(double));
            dt1.Columns.Add("total_final_for_delivery", typeof(double));
            dt1.Columns.Add("total_og_target_for_delivery", typeof(double));
            foreach (var j in query)
            {
               
                dt1.Rows.Add(j.ItemCode, j.TotalSold,j.TotalFinalForDeivery, j.TotalTargetForDel);
            }
            if (!isOpen)
            {
                return dt1;
            }
            else
            {
                var queryy = (from row in dtFiltered.AsEnumerable()
                              join row2 in dt1.AsEnumerable() on row.Field<string>("item_code") equals row2.Field<string>("item_code")
                              into grp
                              select new
                              {
                                  //Id = row.Field<int>("id") == null ? 0 : row.Field<int>("id"),
                                  ItemCode = row.Field<string>("item_code") == null ? "" : row.Field<string>("item_code"),
                                  Uom = row.Field<string>("uom") == null ? "" : row.Field<string>("uom"),
                                  TotalSold = grp.Sum(r => r.Field<double>("total_sold_as_per_selected_branch") == null ? 0 : r.Field<double>("total_sold_as_per_selected_branch")),
                                  Branch = row.Field<string>("branch") == null ? "" : row.Field<string>("branch"),
                                  Sold = row["sold"] != DBNull.Value ? row.Field<double>("sold") : 0.00,
                                  //Sold = 0,
                                  //Sold = row.Field<double?>("sold") == null ? 0 : row.Field<double>("sold"),
                                  DateDiff = row.Field<Int64?>("datediff") == null ? 0 : row.Field<Int64>("datediff"),
                                  //Average = row.Field<double?>("average") == null ? 0.00 : row.Field<double>("average"),
                                  Average = row["average"] != DBNull.Value ? row.Field<double>("average") : 0.00,
                                  //LastBal = row.Field<double?>("last_bal") == null ? 0 : row.Field<double>("last_bal"),
                                  LastBal = row["last_bal"] != DBNull.Value ? Convert.ToDouble(row["last_bal"].ToString()) : 0.00,
                                  //TargetForDel = row.Field<double?>("target_for_del") == null ? 0 : row.Field<double>("target_for_del"),
                                  TargetForDel = row["target_for_del"] != DBNull.Value ? Convert.ToDouble(row["target_for_del"].ToString()) : 0.00,
                                  ProdMinQty = row["prod_min_qty"] != DBNull.Value ? Convert.ToDouble(row["prod_min_qty"].ToString()) : 0.00,
                                  TotalTargetForDel = grp.Sum(r => r.Field<double?>("total_final_for_delivery") == null ? 0 : r.Field<double>("total_final_for_delivery")),
                                  FinalForDelivery = row["final_for_delivery"] != DBNull.Value ? row.Field<double>("final_for_delivery") : 0.00,
                                  RowIndex = row.Field<int?>("row_index") == null ? 0 : row.Field<int>("row_index")
                                  //FinalForDelivery = row.Field<dynamic>("final_for_delivery") == null ? (double?)null : row.Field<dynamic>("final_for_delivery")
                              }).Distinct().OrderBy(r => r.ItemCode).ToList();

                DataTable dt = new DataTable();
                dt.Columns.Add("parent_id", typeof(int));
                dt.Columns.Add("item_code", typeof(string));
                dt.Columns.Add("total_sold", typeof(double));
                dt.Columns.Add("total_final_for_delivery", typeof(double));
                dt.Columns.Add("branch", typeof(string));
                //dt.Columns.Add("branch", typeof(string));

                dt.Columns.Add("sold_quantity", typeof(double));
                dt.Columns.Add("days", typeof(double));
                dt.Columns.Add("average_per_day", typeof(double));
                dt.Columns.Add("last_bal_qty", typeof(double));
                dt.Columns.Add("target_for_delivery", typeof(double));
                dt.Columns.Add("final_for_delivery", typeof(double));
                dt.Columns.Add("item_code2", typeof(string));
                dt.Columns.Add("total_sold2", typeof(double));
                dt.Columns.Add("total_final_for_delivery2", typeof(double));
                dt.Columns.Add("row_index", typeof(int));
                dt.Columns.Add("uom", typeof(string));
                string item = "";
                int id = 0;
                int id2 = 0;

                foreach (var j in queryy)
                {
                    if (forSubmit)
                    {
                        dt.Rows.Add(0, j.ItemCode, j.TotalSold, j.TotalTargetForDel, j.Branch, j.Sold, j.DateDiff, j.Average, j.LastBal, j.TargetForDel, j.FinalForDelivery, j.ItemCode, j.TotalSold, j.TotalTargetForDel, j.RowIndex,j.Uom);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(currentItemCode.Trim()))
                        {
                            if (j.ItemCode == currentItemCode)
                            {
                                if (!item.Trim().Equals(j.ItemCode.ToString()))
                                {
                                    id++;
                                    id2 = id - 1;
                                    dt.Rows.Add((int?)null, j.ItemCode, j.TotalSold, j.TotalTargetForDel, "", (double?)null, (Int64?)null, (Int64?)null, (double?)null, (double?)null, (double?)null, "", (double?)null, (double?)null, j.RowIndex);
                                }
                                id++;
                                dt.Rows.Add(id2, "", (double?)null, (double?)null, j.Branch, j.Sold, j.DateDiff, j.Average, j.LastBal, j.TargetForDel, j.FinalForDelivery, j.ItemCode, j.TotalSold, j.TotalTargetForDel, j.RowIndex,j.Uom);
                                item = j.ItemCode;
                            }
                        }
                        else
                        {
                            if (!item.Trim().Equals(j.ItemCode.ToString()))
                            {
                                id++;
                                id2 = id - 1;
                                dt.Rows.Add((int?)null, j.ItemCode, j.TotalSold, j.TotalTargetForDel, "", (double?)null, (Int64?)null, (Int64?)null, (double?)null, (double?)null, (double?)null, "", (double?)null, (double?)null, j.RowIndex);
                            }
                            id++;
                            dt.Rows.Add(id2, "", (double?)null, (double?)null, j.Branch, j.Sold, j.DateDiff, j.Average, j.LastBal, j.TargetForDel, j.FinalForDelivery, j.ItemCode, j.TotalSold, j.TotalTargetForDel, j.RowIndex,j.Uom);
                            item = j.ItemCode;
                        }
                    }
                }
                return dt;
            }
        }

        public DataTable loadForProductionQuery(DataTable dtFiltered, string selectedItemCode)
        {
            DataTable dtResult = new DataTable();
            try
            {
                foreach (DataRow row in dtFiltered.Rows)
                {
                    string s = row["item_code"].ToString().Replace(@"'", "''");
                    DataRow[] rows = dtManualItem.Select("item_code='" + s + "'");
                    if (rows.Count() > 0)
                    {
                        foreach (DataRow r in rows)
                        {
                            dtManualItem.Rows.Remove(r);
                        }
                    }
                }
                //dtGlobal.Columns.Add("final_for_delivery", typeof(double));
                //dtGlobal.Columns.Add("row_index", typeof(int));
                //dtGlobal.Columns.Add("final_base", typeof(double));
                //dtGlobal.Columns.Add("final_prod_qty", typeof(double));
                //dtGlobal.Columns.Add("buffer_stock", typeof(double));
                //dtGlobal.Columns.Add("isManualItem", typeof(Boolean));
                foreach (DataRow row in dtManualItem.Rows)
                {
                    dtFiltered.Rows.Add(row["branch"], row["item_code"], row["sold"], row["datediff"], row["average"], row["last_bal"], row["target_for_del"], row["prod_min_qty"], row["prod_ending_bal"], row["uom"], row["premix_code"], row["final_for_delivery"], row["row_index"], row["final_base"], row["final_prod_qty"], row["buffer_stock"], true);
                }
                //foreach (DataRow row in dtFiltered.Rows)
                //{
                //foreach (DataColumn column in dtFiltered.Columns)
                //{
                //    string ColumnName = column.ColumnName;
                //    //string ColumnData = row[column].ToString();
                //    Console.WriteLine(ColumnName);
                //}
                //}
                double doubleTemp = 0.00;
                var query = (from row in dtFiltered.AsEnumerable()
                             group row by new
                             {
                                 ItemCode = row.Field<string>("item_code"),

                             } into grp
                             select new
                             {
                                 ItemCode = grp.Key.ItemCode,
                                 TotalFinalDelivery = grp.Sum(r => r.Field<double?>("final_for_delivery") == null ? (double?)null : r.Field<double>("final_for_delivery")),
                                 //TotalFinalDelivery = grp.Sum(r => r.Field<double?>("final_for_delivery") == null ? (double?)null : r.Field<double>("final_for_delivery")),
                                 TotalTargetDelivery = grp.Sum(r => r.Field<double?>("target_for_del") == null ? 0.00 : r.Field<double>("target_for_del")),
                                 //ProdMinQty = grp.First().Field<double>("prod_min_qty"),
                                 ProdMinQty =  grp.AsEnumerable().Where(r => r.Field<dynamic>("prod_min_qty") >0).Any() ? grp.AsEnumerable().Where(r => r.Field<dynamic>("prod_min_qty") > 0).First().Field<dynamic>("prod_min_qty") : 0.00,
                                 //ProdMinQty = grp.LastOrDefault().Field<dynamic>("prod_min_qty"),
                                 LastBal = grp.AsEnumerable().Where(r => r.Field<dynamic>("prod_ending_bal") > 0).Any() ? grp.AsEnumerable().Where(r => r.Field<dynamic>("prod_ending_bal") > 0).First().Field<dynamic>("prod_ending_bal") : 0.00,
                                 FinalBase = grp.Sum(r => r.Field<double?>("final_base") == null ? (double?)null : r.Field<double>("final_base")),
                                 FinalProdQty = grp.Sum(r => r.Field<double?>("final_prod_qty") == null ? (double?)null : r.Field<double>("final_prod_qty")),
                                 BufferStock = grp.Sum(r => r.Field<double?>("buffer_stock") == null ? (double?)null : r.Field<double>("buffer_stock")),
                                 Uom = grp.FirstOrDefault().Field<string>("uom"),
                                 IsManual = grp.FirstOrDefault().Field<dynamic>("isManualItem") == null ? false : grp.FirstOrDefault().Field<bool>("isManualItem"),
                             }).Distinct().ToList();

                DataTable dt1 = new DataTable();
                dt1.Columns.Add("item_code", typeof(string));
                dt1.Columns.Add("prod_ending_bal", typeof(double));
                dt1.Columns.Add("final_for_delivery", typeof(double));
                dt1.Columns.Add("buffer_stock", typeof(double));
                dt1.Columns.Add("uom", typeof(string));
                dt1.Columns.Add("target_for_prod", typeof(double));
                dt1.Columns.Add("prod_min_qty", typeof(double));
                dt1.Columns.Add("target_base", typeof(double));
                dt1.Columns.Add("final_base", typeof(double));

                dt1.Columns.Add("final_prod_qty", typeof(double));
                dt1.Columns.Add("branch", typeof(string));
                dt1.Columns.Add("row_index", typeof(int));
                dt1.Columns.Add("for_del_for_prod", typeof(double));
                dt1.Columns.Add("isManualItem", typeof(Boolean));
                int intTemp = 0;
                foreach (var j in query)
                {
                    string currentItemCode = "";
                    foreach (DataRow row in dtFiltered.Rows)
                    {
                        if (!string.IsNullOrEmpty(selectedItemCode.Trim()))
                        {
                            if (selectedItemCode == j.ItemCode)
                            {
                                if (dtFiltered.Columns.Contains("item_code"))
                                {
                                    string rItemCode = row["item_code"].ToString();
                                    if (rItemCode == j.ItemCode)
                                    {
                                        if (currentItemCode != j.ItemCode)
                                        {
                                            string rBranchCode = dtFiltered.Columns.Contains("branch") ? row["branch"].ToString() : "";

                                  

                                            int rowIndex = int.TryParse(row["row_index"].ToString(), out intTemp) ? Convert.ToInt32(row["row_index"].ToString()) : intTemp;

                        

                                            double buffStock = j.BufferStock.HasValue ? j.BufferStock.Value : 0.00;

                                            double finalDel = ((j.TotalFinalDelivery.HasValue ? j.TotalFinalDelivery.Value : 0.00) + buffStock) - j.LastBal;

                                            double targetBase = finalDel / (j.ProdMinQty);

                                            double variance = (j.FinalProdQty.HasValue ? j.FinalProdQty.Value : 0.00) - finalDel;

                                            dt1.Rows.Add(j.ItemCode, j.LastBal, j.TotalFinalDelivery, buffStock, j.Uom, finalDel, row["prod_min_qty"] == DBNull.Value ? (double?)null : j.ProdMinQty, double.IsNaN(targetBase) || double.IsInfinity(targetBase) ? (double?)null : targetBase, row["final_base"] == DBNull.Value ? (double?)null : j.FinalBase, row["final_prod_qty"] == DBNull.Value ? (double?)null : j.FinalProdQty, rBranchCode, rowIndex, variance, j.IsManual);
                                            currentItemCode = j.ItemCode;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (dtFiltered.Columns.Contains("item_code"))
                            {
                                string rItemCode = row["item_code"].ToString();
                                if (rItemCode == j.ItemCode)
                                {
                                    if (currentItemCode != j.ItemCode)
                                    {
                                        string rBranchCode = dtFiltered.Columns.Contains("branch") ? row["branch"].ToString() : "";

                                      

                                        int rowIndex = int.TryParse(row["row_index"].ToString(), out intTemp) ? Convert.ToInt32(row["row_index"].ToString()) : intTemp;


                                        double buffStock = j.BufferStock.HasValue ? j.BufferStock.Value : 0.00;



                                        double finalDel = ((j.TotalFinalDelivery.HasValue ? j.TotalFinalDelivery.Value : 0.00) + buffStock) - j.LastBal;

                                        double targetBase = finalDel / (j.ProdMinQty);

                                        double variance = (j.FinalProdQty.HasValue ? j.FinalProdQty.Value : 0.00) - finalDel;

                                        dt1.Rows.Add(j.ItemCode, j.LastBal, j.TotalFinalDelivery, buffStock, j.Uom, finalDel, row["prod_min_qty"] == DBNull.Value ? (double?)null : j.ProdMinQty, double.IsNaN(targetBase) || double.IsInfinity(targetBase) ? (double?)null : targetBase, row["final_base"] == DBNull.Value ? (double?)null : j.FinalBase, row["final_prod_qty"] == DBNull.Value ? (double?)null : j.FinalProdQty, rBranchCode, rowIndex, variance,j.IsManual);
                                        currentItemCode = j.ItemCode;
                                    }
                                }
                            }
                        }
                    }
                }
                dtResult = dt1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dtResult;
        }


        public void loadForDelivery(DataTable dtData, bool isOpen, string selectedItemCode)
        {
            try
            {
                gridControl1.Invoke(new Action(delegate ()
                {
                    //gridView1.OptionsView.RowAutoHeight = true;
                    gridControl1.DataSource = null;
                    gridView1.Columns.Clear();
                }));
                gridControl2.Invoke(new Action(delegate ()
                {
                    gridControl2.DataSource = null;
                    gridView2.Columns.Clear();
                }));
                if (dtData.Columns.Contains("item_code") && dtData.Columns.Contains("branch"))
                {
                    DataTable dtResult = new DataTable();
                    DataRow[] results = null;
                    string sSelectedItems = "", sSelectedBranches = "", sSelectedPremix = "", sParams = "";
                    gridControl1.DataSource = null;
                    if (selectedItems != null)
                    {
                        if (selectedItems.Count() > 0)
                        {
                            sSelectedItems = string.Join(",", selectedItems);
                            lblSelectedItem.Invoke(new Action(delegate ()
                            {
                                lblSelectedItem.Text = "Item [" + (isAllItem ? "All" : sSelectedItems) + "]";
                            }));

                            sParams = "item_code IN (" + sSelectedItems + ")";
                        }
                        else
                        {
                            lblSelectedItem.Invoke(new Action(delegate ()
                            {
                                lblSelectedItem.Text = "Item [All]";
                            }));

                        }
                    }
                    else
                    {
                        lblSelectedItem.Invoke(new Action(delegate ()
                        {
                            lblSelectedItem.Text = "Item [All]";
                        }));
                    }
                    if (selectedBranches != null)
                    {
                        if (selectedBranches.Count() > 0)
                        {
                            sSelectedBranches = string.Join(",", selectedBranches);
                            lblSelectedBranch.Invoke(new Action(delegate ()
                            {
                                lblSelectedBranch.Text = "Branch [" + (isAllBranch ? "All" : sSelectedBranches) + "]";
                            }));

                            sParams += string.IsNullOrEmpty(sSelectedBranches.Trim()) ? "" : (string.IsNullOrEmpty(sSelectedItems.Trim()) ? "" : " AND") + " branch IN (" + sSelectedBranches + ")";
                        }
                        else
                        {
                            lblSelectedBranch.Invoke(new Action(delegate ()
                            {
                                lblSelectedBranch.Text = "Branch [All]";
                            }));
                        }
                    }
                    else
                    {
                        lblSelectedBranch.Invoke(new Action(delegate ()
                        {
                            lblSelectedBranch.Text = "Branch [All]";
                        }));
                    }
                    if (selectedPremix != null)
                    {
                        if (selectedPremix.Count() > 0)
                        {
                            sSelectedPremix = string.Join(",", selectedPremix);
                            lblSelectedPremix.Invoke(new Action(delegate ()
                            {
                                lblSelectedPremix.Text = "Premix [" + (isAllPremix ? "All" : sSelectedPremix) + "]";
                            }));
                            sParams += string.IsNullOrEmpty(sSelectedPremix.Trim()) ? "" : (string.IsNullOrEmpty(sSelectedItems.Trim()) ? string.IsNullOrEmpty(sSelectedBranches.Trim()) ? "" : " AND" : " AND") + " premix_code IN (" + sSelectedPremix + ")";
                        }
                        else
                        {
                            lblSelectedPremix.Invoke(new Action(delegate ()
                            {
                                lblSelectedPremix.Text = "Premix [All]";
                            }));
                        }
                    }
                    else
                    {
                        lblSelectedPremix.Invoke(new Action(delegate ()
                        {
                            lblSelectedPremix.Text = "Premix [All]";
                        }));
                    }
                    if (!string.IsNullOrEmpty(sParams.Trim()))
                    {
                        Console.WriteLine(sParams);
                        dtData.CaseSensitive = true;
                        results = dtData.Select(sParams);
                   
                    }
                    DataTable dtCopyTable = results == null ? dtData : results.Any() ? results.CopyToDataTable() : new DataTable();
                    DataTable dtFormat = loadForDeliveryFormat(dtCopyTable, isOpen, selectedItemCode, false);
                    if (dtFormat.Rows.Count > 0)
                    {
                        dtCurrentGlobal = dtCopyTable;

                        gridControl1.Invoke(new Action(delegate ()
                        {
                            gridControl1.DataSource = dtFormat;
                            gridView1.OptionsView.ColumnAutoWidth = false;
                            gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                            foreach (GridColumn col in gridView1.Columns)
                            {
                                string fieldName = col.FieldName;
                                string v = col.GetCaption();
                                string s = v.Replace("_", " ");
                                col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());

                                col.ColumnEdit = fieldName.Equals("final_for_delivery") ? repositoryItemTextEdit1 : fieldName.Equals("total_final_for_delivery") ? repositoryItemTextEdit3 : fieldName.Equals("item_code") ? repositoryItemMemoEdit1 : fieldName.Equals("branch") ? repositoryItemMemoEdit3 : repositoryItemTextEdit3;
                                col.DisplayFormat.FormatType = fieldName.Equals("branch") || fieldName.Equals("item_code") || fieldName.Equals("uom") ? DevExpress.Utils.FormatType.None : DevExpress.Utils.FormatType.Numeric;
                                col.Caption = fieldName.Equals("sold_quantity") ? "Sold" : fieldName.Equals("average_per_day") ? "Avg. Per Day" : fieldName.Equals("target_for_delivery") ? "Target For Del." : fieldName.Equals("final_for_delivery") ? "Final For Del." : fieldName.Equals("total_final_for_delivery") ? "Total Final For Del." : col.Caption;
                                col.DisplayFormat.FormatString = fieldName.Equals("branch") || fieldName.Equals("item_code") || fieldName.Equals("uom") ? "" : "n2";
                                //col.DisplayFormat.FormatType = fieldName.Equals("allowed_discount") ? DevExpress.Utils.FormatType.Numeric : DevExpress.Utils.FormatType.None;
                                //col.DisplayFormat.FormatString = fieldName.Equals("allowed_discount") ? "n2" : "";
                                //col.Visible = !(fieldName.Equals("pricelist_id") || fieldName.Equals("id") || fieldName.Equals("date_created") || fieldName.Equals("date_updated") || fieldName.Equals("created_by") || fieldName.Equals("updated_by") || fieldName.Equals("agent_account") || fieldName.Equals("production_whse") || fieldName.Equals("raw_wheat_whse") || fieldName.Equals("igoods_whse") || fieldName.Equals("pack_and_oth_whse") || fieldName.Equals("premix_whse") || fieldName.Equals("cutoff") || fieldName.Equals("is_active") || fieldName.Equals("is_fg") || fieldName.Equals("is_production") || fieldName.Equals("is_igoods") || fieldName.Equals("is_raw_mat") || fieldName.Equals("is_pack_oth") || fieldName.Equals("is_premix") || fieldName.Equals("is_main"));
                                col.Visible = !(fieldName.Equals("item_code2") || fieldName.Equals("total_sold2") || fieldName.Equals("final_for_delivery2") || fieldName.Equals("parent_id") || fieldName.Equals("total_final_for_delivery2") || fieldName.Equals("row_index") || fieldName.Equals("uom") || fieldName.Equals("total_og_target_for_delivery"));

                                col.Fixed = fieldName.Equals("item_code") || fieldName.Equals("total_sold") || fieldName.Equals("total_final_for_delivery") || fieldName.Equals("branch") || fieldName.Equals("total_sold_as_per_selected_branch") ? FixedStyle.Left : fieldName.Equals("final_for_delivery") ? FixedStyle.Right : FixedStyle.None;
                                //fonts
                                FontFamily fontArial = new FontFamily("Arial");
                                col.AppearanceHeader.Font = new Font(fontArial, 9, FontStyle.Regular);
                                col.AppearanceCell.Font = new Font(fontArial, 8, FontStyle.Regular);
                                col.Width = 95;

                            }
                            gridView1.BestFitColumns();
                            if (isOpen)
                            {
                                foreach (GridColumn col in gridView1.Columns)
                                {
                                    string fieldName = col.FieldName;
                                    col.Width = !(fieldName.Equals("item_code") || fieldName.Equals("branch")) ? 70 : col.Width;
                                }
                            }
                        }));
                    }
                }
                gridControl2.Invoke(new Action(delegate ()
                {
                    gridControl2.DataSource = null;
                    gridView2.Columns.Clear();
                }));
                loadForProduction(selectedItemCode);
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(delegate ()
                {
                    MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }));

            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadQuery(false);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            if (cmbProdWhse.SelectedIndex == 0)
            {
                panelProdWhse.BackColor = Color.Red;
                apic.showCustomMsgBox("Validation", "Please select Production Warehouse first!");
            }
            else
            {
                bg(backgroundWorker1);
            }
        }

        private void btnSearchQuery1_Click(object sender, EventArgs e)
        {
            if (cmbProdWhse.SelectedIndex == 0)
            {
                panelProdWhse.BackColor = Color.Red;
                apic.showCustomMsgBox("Validation", "Please select Production Warehouse first!");
            }
            else
            {
                btnAddTransaction.Enabled = btnAddTransaction2.Enabled = true;
                bg(backgroundWorker1);
            }

        }

        private void gridView1_ShowingEditor(object sender, CancelEventArgs e)
        {
            //GridView currentView = sender as GridView;
            //string branch = !Convert.IsDBNull(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch").ToString()) ? gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch").ToString() : "";
            //string selectedColumnText = gridView1.FocusedColumn.FieldName;
            //e.Cancel = selectedColumnText.Equals("final_for_delivery") && string.IsNullOrEmpty(branch.Trim()) ? true : false;
        }

        private void gridView1_TopRowChanged(object sender, EventArgs e)
        {

        }

        private void gridView1_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            try
            {
                //double finalForDel = 0.00, doubleTemp = 0.00;
                //for (int i = 0; i < gridView1.DataRowCount; i++)
                //{
                //    if (e.RowHandle != i)
                //    {
                //        finalForDel += double.TryParse(gridView1.GetRowCellValue(i, "final_for_delivery").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(i, "final_for_delivery").ToString()) : doubleTemp;
                //    }
                //}
                //double currentValue = double.TryParse(e.Value.ToString(), out doubleTemp) ? Convert.ToDouble(e.Value.ToString()) : doubleTemp;
                //finalForDel += currentValue;
                //int parentID = 0, intTemp = 0;
                //parentID = int.TryParse(gridView1.GetRowCellValue(e.RowHandle, "parent_id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(e.RowHandle, "parent_id").ToString()) : intTemp;
                ////Console.WriteLine(finalForDel);
                //var varCol = gridView1.Columns["total_final_for_delivery"];
                //var varCol2 = gridView1.Columns["total_final_for_delivery2"];
                ////Console.WriteLine(parentID-1);

                //int lastTopParentID = int.TryParse(gridView1.GetRowCellValue(lastTopIndex, "parent_id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(lastTopIndex, "parent_id").ToString()) : intTemp;
                //bool isLastRow = lastTopParentID == parentID;
                //if (varCol != null)
                //{
                //    gridView1.SetRowCellValue(parentID, varCol, finalForDel);

                //    if (isLastRow)
                //    {
                //        gridView1.SetRowCellValue(lastTopIndex, varCol, finalForDel);
                //    }

                //}
                //if (varCol2 != null)
                //{
                //    gridView1.SetRowCellValue(parentID, varCol2, finalForDel);
                //    if (isLastRow)
                //    {
                //        gridView1.SetRowCellValue(lastTopIndex, varCol2, finalForDel);
                //    }
                //    //gridView1.SetRowCellValue(e.RowHandle, "final_for_delivery", currentValue);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void gridView1_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
           
        }

        private void gridView1_CellValueChanging_1(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            try
            {
                double finalForDel = 0.00, doubleTemp = 0.00, targetForDel = 0.00;
                int currentParentID = 0, intTemp = 0;
                currentParentID = int.TryParse(gridView1.GetRowCellValue(e.RowHandle, "parent_id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(e.RowHandle, "parent_id").ToString()) : intTemp;

                string currentItemCode = gridView1.GetRowCellValue(e.RowHandle, "item_code2").ToString();
                string currentBranch = gridView1.GetRowCellValue(e.RowHandle, "branch").ToString();
                int lastTopParentID = gridView1.GetRowCellValue(lastTopIndex, "parent_id") == null ? intTemp : int.TryParse(gridView1.GetRowCellValue(lastTopIndex, "parent_id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(lastTopIndex, "parent_id").ToString()) : intTemp;
                bool isLastRow = lastTopParentID == currentParentID;

                for (int i = 0; i < gridView1.DataRowCount; i++)
                {
                    if (e.RowHandle != i)
                    {
                        int iParentID = int.TryParse(gridView1.GetRowCellValue(i, "parent_id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(i, "parent_id").ToString()) : intTemp;
                        if (currentParentID == iParentID)
                        {
                            finalForDel += double.TryParse(gridView1.GetRowCellValue(i, "final_for_delivery").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(i, "final_for_delivery").ToString()) : doubleTemp;
                        }
                    }
                }
                double currentValue = double.TryParse(e.Value.ToString(), out doubleTemp) ? Convert.ToDouble(e.Value.ToString()) : doubleTemp;
                finalForDel += currentValue;
                gridView1.SetRowCellValue(currentParentID, "total_final_for_delivery", finalForDel.ToString("n2"));
                if (isLastRow)
                {
                    gridView1.SetRowCellValue(lastTopIndex, "total_final_for_delivery", finalForDel.ToString("n2"));
                }
                for (int i = 0; i < gridView1.DataRowCount; i++)
                {
                    int iParentID = int.TryParse(gridView1.GetRowCellValue(i, "parent_id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(i, "parent_id").ToString()) : intTemp;
                    if (currentParentID == iParentID)
                    {
                        gridView1.SetRowCellValue(i, "total_final_for_delivery2", finalForDel.ToString("n2"));
                        if (isLastRow)
                        {
                            gridView1.SetRowCellValue(lastTopIndex, "total_final_for_delivery2", finalForDel.ToString("n2"));
                        }
                    }
                }

                int currentRowIndex = int.TryParse(gridView1.GetRowCellValue(e.RowHandle, "row_index").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(e.RowHandle, "row_index").ToString()) : intTemp;
                dtGlobal.Rows[currentRowIndex]["final_for_delivery"] = currentValue;

                        for (int i = 0; i < gridView2.DataRowCount; i++)
                {
                    string g2ItemCode = gridView2.GetRowCellValue(i, "item_code").ToString();
                    if(g2ItemCode == currentItemCode)
                    {
                        double prodEndBal = gridView2.GetRowCellValue(i, "prod_ending_bal") == null ? 0.00 : double.TryParse(gridView2.GetRowCellValue(i, "prod_ending_bal").ToString(), out doubleTemp) ? Convert.ToDouble(gridView2.GetRowCellValue(i, "prod_ending_bal").ToString()) : doubleTemp;

                        double prodMinQty = gridView2.GetRowCellValue(i, "prod_min_qty") == null  ? 0.00 : double.TryParse(gridView2.GetRowCellValue(i, "prod_min_qty").ToString(), out doubleTemp) ? Convert.ToDouble(gridView2.GetRowCellValue(i, "prod_min_qty").ToString()) : doubleTemp;


                        double bufferStock = gridView2.GetRowCellValue(i, "buffer_stock") == null ? 0.00 : double.TryParse(gridView2.GetRowCellValue(i, "buffer_stock").ToString(), out doubleTemp) ? Convert.ToDouble(gridView2.GetRowCellValue(i, "buffer_stock").ToString()) : doubleTemp;

                        double targetForProd = (finalForDel + bufferStock) - prodEndBal;

                        double targetBase = targetForProd / prodMinQty;


          

                        gridView2.SetRowCellValue(i, "target_for_prod", targetForProd);
                        gridView2.SetRowCellValue(i, "final_for_delivery", finalForDel);

                        double finalBase = gridView2.GetRowCellValue(i, "final_base") == null ? 0.00 : double.TryParse(gridView2.GetRowCellValue(i, "final_base").ToString(), out doubleTemp) ? Convert.ToDouble(gridView2.GetRowCellValue(i, "final_base").ToString()) : doubleTemp;

                        bool isFinalBaseNull = gridView2.GetRowCellValue(i, "final_base") == null;
                        isFinalBaseNull = !isFinalBaseNull ? !double.TryParse(gridView2.GetRowCellValue(i, "final_base").ToString(), out doubleTemp) : false;


                        double finalProdQty = prodMinQty * finalBase;

                        gridView2.SetRowCellValue(i, "target_base", double.IsNaN(targetBase) || double.IsInfinity(targetBase) ? (double?)null : targetBase);

                        gridView2.SetRowCellValue(i, "final_base", isFinalBaseNull ? (double?)null : finalBase);

                        gridView2.SetRowCellValue(i, "final_prod_qty", isFinalBaseNull ? (double?)null : finalProdQty);


                        foreach (DataRow row in dtGlobal.Rows)
                        {
                            string s = row["item_code"].ToString().Replace(@"'", "''");
                            DataRow[] rows = dtManualItem.Select("item_code='" + s + "'");
                            if (rows.Count() > 0)
                            {
                                int asddd = dtManualItem.Rows.IndexOf(rows[0]);
                                dtManualItem.Rows[asddd]["final_base"] = finalBase;
                                dtManualItem.Rows[asddd]["final_prod_qty"] = finalProdQty;
                            }
                        }

                        double variance = finalProdQty-targetForProd;

                        gridView2.SetRowCellValue(i, "for_del_for_prod", variance);

                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void gridView1_TopRowChanged_1(object sender, EventArgs e)
        {
            try
            {
                var myColumn = gridView1.Columns["branch"];
                if (myColumn != null)
                {
                    if (gridView1.GetRowCellValue(lastTopIndex, "item_code2") != null)
                    {
                        if (gridView1.GetRowCellValue(lastTopIndex, "item_code2").ToString().Trim() != "")
                        {
                            gridView1.SetRowCellValue(lastTopIndex, "item_code", "");

                        }
                    }
                    if (gridView1.GetRowCellValue(lastTopIndex, "total_sold2") != null)
                    {
                        if (gridView1.GetRowCellValue(lastTopIndex, "total_sold2").ToString().Trim() != "")
                        {
                            gridView1.SetRowCellValue(lastTopIndex, "total_sold", (double?)null);
                        }
                    }
                    if (gridView1.GetRowCellValue(lastTopIndex, "total_final_for_delivery2") != null)
                    {
                        if (gridView1.GetRowCellValue(lastTopIndex, "total_final_for_delivery2").ToString().Trim() != "")
                        {
                            gridView1.SetRowCellValue(lastTopIndex, "total_final_for_delivery", (double?)null);
                        }
                    }
                    int topIndex = gridView1.TopRowIndex;
                    lastTopIndex = topIndex;
                    double doubleTemp = 0.00;
                    string itemCode = gridView1.GetRowCellValue(topIndex, "item_code2").ToString();
                    if (!string.IsNullOrEmpty(itemCode.Trim()))
                    {
                        gridView1.SetRowCellValue(topIndex, "item_code", itemCode);
                    }
                    if (double.TryParse(gridView1.GetRowCellValue(topIndex, "total_sold2").ToString(), out doubleTemp))
                    {
                        double totalSold = Convert.ToDouble(gridView1.GetRowCellValue(topIndex, "total_sold2").ToString());
                        gridView1.SetRowCellValue(topIndex, "total_sold", totalSold);
                    }
                    if (double.TryParse(gridView1.GetRowCellValue(topIndex, "total_final_for_delivery2").ToString(), out doubleTemp))
                    {
                        double totalSold = Convert.ToDouble(gridView1.GetRowCellValue(topIndex, "total_final_for_delivery2").ToString());
                        gridView1.SetRowCellValue(topIndex, "total_final_for_delivery", totalSold);
                    }
                    gridView1.LayoutChanged();
                }
                //else
                //{
                //    for (int i = 0; i < gridView1.DataRowCount; i++)
                //    {
                //        //string fBranch = gridView1.GetRowCellValue(i, "branch").ToString();
                //        if (topIndex != i)
                //        {
                //            gridView1.SetRowCellValue(i, "item_code", "");
                //        }
                //    }
                //}

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void gridView2_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            try
            {
                double doubleTemp = 0.00;
                bool boolTemp = false;
                string selectedColumnfieldName = gridView2.FocusedColumn.FieldName;
                if (selectedColumnfieldName.Equals("final_base"))
                {     
                    //double prodMinQty = Convert.ToDouble(gridView2.GetFocusedRowCellValue("prod_min_qty").ToString());
                    double prodMinQty = double.TryParse(gridView2.GetFocusedRowCellValue("prod_min_qty").ToString(), out doubleTemp) ? Convert.ToDouble(gridView2.GetFocusedRowCellValue("prod_min_qty").ToString()) : doubleTemp;
                    double finalBase = double.TryParse(e.Value.ToString(), out doubleTemp) ? Convert.ToDouble(e.Value.ToString()) : doubleTemp;

                    double finalProdQty = prodMinQty * finalBase;
                    var varCol = gridView2.Columns["final_prod_qty"];
                    var varCol2 = gridView2.Columns["for_del_for_prod"];

                    int intTemp = 0;
                    int rowIndex = Convert.ToInt32(gridView2.GetRowCellValue(e.RowHandle, "row_index").ToString());

                    double targetForProd = double.TryParse(gridView2.GetFocusedRowCellValue("target_for_prod").ToString(), out doubleTemp) ? Convert.ToDouble(gridView2.GetFocusedRowCellValue("target_for_prod").ToString()) : doubleTemp;


                    if (e.Value.ToString().Trim() == "")
                    {
                        gridView2.SetRowCellValue(e.RowHandle, varCol, null);
                        gridView2.SetRowCellValue(e.RowHandle, varCol2, (targetForProd - 0));
                        dtGlobal.Rows[rowIndex]["final_base"] = DBNull.Value;
                        dtGlobal.Rows[rowIndex]["final_prod_qty"] = DBNull.Value;

                        string sItem = gridView2.GetRowCellValue(e.RowHandle, "item_code").ToString();
                        string s = sItem.Replace(@"'", "''");
                        DataRow[] rows = dtManualItem.Select("item_code='" + s + "'");
                        if (rows.Count() > 0)
                        {

                            foreach (DataRow a in rows)
                            {
                                int asddd = dtManualItem.Rows.IndexOf(a);
                                //dtManualItem.Rows[asddd]["target_for_prod"] = 0;
                                dtManualItem.Rows[asddd]["final_base"] = DBNull.Value;
                                dtManualItem.Rows[asddd]["final_prod_qty"] = DBNull.Value;
                            }
                        }
                    }
                    else
                    {
                        gridView2.SetRowCellValue(e.RowHandle, varCol, finalProdQty.ToString("n3"));
                        gridView2.SetRowCellValue(e.RowHandle, varCol2, (targetForProd - finalProdQty).ToString("n3"));
                        dtGlobal.Rows[rowIndex]["final_base"] = finalBase;


                        string sItem = gridView2.GetRowCellValue(e.RowHandle, "item_code").ToString();
                        string s =sItem.Replace(@"'", "''");
                        DataRow[] rows = dtManualItem.Select("item_code='" + s + "'");
                        if (rows.Count() > 0)
                        {
                            foreach (DataRow a in rows)
                            {
                                int asddd = dtManualItem.Rows.IndexOf(a);
                                //dtManualItem.Rows[asddd]["target_for_prod"] = 0;
                                dtManualItem.Rows[asddd]["final_base"] = finalBase;
                                dtManualItem.Rows[asddd]["final_prod_qty"] = finalProdQty;
                            }
                        }
                        

                        dtGlobal.Rows[rowIndex]["final_prod_qty"] = finalProdQty;
                        double variance = finalProdQty - targetForProd;

                        gridView2.SetRowCellValue(e.RowHandle, "for_del_for_prod", variance);
                    }
                }
                else if (e.Column.FieldName.Equals("buffer_stock"))
                {
                    var bufferCol = gridView2.Columns["buffer_stock"];
                    var targetForProdCol = gridView2.Columns["target_for_prod"];
                    var computedBaseCol = gridView2.Columns["target_base"];
                    var finalForDelCol = gridView2.Columns["final_for_delivery"];

                    int intTemp = 0;

                    bool isRowIndex = int.TryParse(gridView2.GetRowCellValue(e.RowHandle, "row_index").ToString(), out intTemp);

                    int rowIndex = int.TryParse(gridView2.GetRowCellValue(e.RowHandle, "row_index").ToString(), out intTemp) ? Convert.ToInt32(gridView2.GetRowCellValue(e.RowHandle, "row_index").ToString()) : intTemp;

                    double finalForDel = gridView2.GetRowCellValue(e.RowHandle, "final_for_delivery") == null ? 0.00 : double.TryParse(gridView2.GetRowCellValue(e.RowHandle, "final_for_delivery").ToString(), out doubleTemp) ? Convert.ToDouble(gridView2.GetRowCellValue(e.RowHandle, "final_for_delivery").ToString()) : doubleTemp;
                    string sItemCode1 = gridView2.GetRowCellValue(e.RowHandle, "item_code").ToString();

                    //instance buffer stock
                    double bufferStock = double.TryParse(e.Value.ToString(), out doubleTemp) ? Convert.ToDouble(e.Value.ToString()) : doubleTemp;

                    //instance computed base
                    double baseQty = double.TryParse(gridView2.GetRowCellValue(e.RowHandle, "prod_min_qty").ToString(), out doubleTemp) ? Convert.ToDouble(gridView2.GetRowCellValue(e.RowHandle, "prod_min_qty").ToString()) : doubleTemp;

                    bool isFinalBase = double.TryParse(gridView2.GetRowCellValue(e.RowHandle, "final_base").ToString(), out doubleTemp);

                    //instance final base
                    double finalBase = double.TryParse(gridView2.GetRowCellValue(e.RowHandle, "final_base").ToString(), out doubleTemp) ? Convert.ToDouble(gridView2.GetRowCellValue(e.RowHandle, "final_base").ToString()) : doubleTemp;


                    //instance prod end bal
                    double prodEndBal = double.TryParse(gridView2.GetRowCellValue(e.RowHandle, "prod_ending_bal").ToString(), out doubleTemp) ? Convert.ToDouble(gridView2.GetRowCellValue(e.RowHandle, "prod_ending_bal").ToString()) : doubleTemp;


                    //computation of target for prod
                    double targetForProd = (finalForDel + bufferStock) - prodEndBal;

                    double computedBase = targetForProd / baseQty;

                    if (e.Value.ToString().Trim() == "")
                    {
                        gridView2.SetRowCellValue(e.RowHandle, bufferCol, null);
                        dtGlobal.Rows[rowIndex]["final_base"] = DBNull.Value;
                        dtGlobal.Rows[rowIndex]["final_prod_qty"] = DBNull.Value;
                    }
                    gridView2.SetRowCellValue(e.RowHandle, targetForProdCol, targetForProd);
                    gridView2.SetRowCellValue(e.RowHandle, computedBaseCol, computedBase);
                    gridView2.SetRowCellValue(e.RowHandle, finalForDelCol, finalForDel);
                    dtGlobal.Rows[rowIndex]["buffer_stock"] = bufferStock;


                    double finalProdQty = baseQty * finalBase;

                    //gridView2.SetRowCellValue(i, "final_base", isFinalBase ? (double?)null : finalBase);

                    //gridView2.SetRowCellValue(i, "final_prod_qty", isFinalBaseNull ? (double?)null : finalProdQty);

                    double variance = finalProdQty - targetForProd;

                    gridView2.SetRowCellValue(e.RowHandle, "for_del_for_prod", variance);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void gridView1_ShowingEditor_1(object sender, CancelEventArgs e)
        {
            GridView currentView = sender as GridView;
            string branch = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch") == null ? "" : !Convert.IsDBNull(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch").ToString()) ? gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "branch").ToString() : "";
            string selectedColumnText = gridView1.FocusedColumn.FieldName;
            e.Cancel = selectedColumnText.Equals("final_for_delivery") && string.IsNullOrEmpty(branch.Trim()) ? true : false;
        }

        private void repositoryItemTextEdit4_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedColumnText = gridView1.FocusedColumn.FieldName;
                string code = gridView1.GetFocusedRowCellValue("item_code").ToString();
                if (selectedColumnText.Equals("item_code") && !string.IsNullOrEmpty(code))
                {
                    var myColumn = gridView1.Columns["branch"];
                    bool isOpen = myColumn == null;
                    bgSubmit = new BackgroundWorker();
                    bgSubmit.DoWork += delegate
                    {
                        loadForDelivery(dtGlobal, isOpen, code);
                        loadForProduction(isOpen ? code : "");
                        //this.Focus();
                    };
                    bgSubmit.RunWorkerCompleted += delegate
                    {
                        closeForm();
                        gridControl1.Invoke(new Action(delegate ()
                        {
                            if (isOpen)
                            {
                                int parentID = 0, intTemp = 0;
                                string sParentID = gridView1.GetFocusedRowCellValue("parent_id").ToString();
                                parentID = int.TryParse(sParentID, out intTemp) ? Convert.ToInt32(sParentID) : intTemp;
                                gridView1.ClearSelection();
                                gridView1.SelectRow(parentID);
                                gridView1.FocusedRowHandle = parentID;
                                gridView1.MakeRowVisible(parentID);
                            }
                        }));
                    };
                    bg(bgSubmit);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void gridView1_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {

            if (e.RowHandle != GridControl.NewItemRowHandle && e.Column.FieldName == "item_code")
            {
                if (gridView1.Columns["branch"] != null )
                {
                    e.RepositoryItem = e.CellValue== null || string.IsNullOrEmpty(e.CellValue.ToString().Trim())  ? repositoryItemMemoEdit3 : repositoryItemMemoEdit1;
                }
                else
                {
                    e.RepositoryItem = repositoryItemMemoEdit1;
                }
                //if (s != "" && e.RowHandle == lastTopIndex)
                //{
                //    Console.WriteLine("hindi null " + e.CellValue.ToString());
                //    e.RepositoryItem = repositoryItemTextEdit4;
                //}
                //else
                //{
                //    Console.WriteLine("null");
                //    e.RepositoryItem = repositoryItemTextEdit5;
                //}
            }
        }
        public string RandomString(int length)
        {
            string result = "";
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ0123456789abcdefghijklmnñopqrstuvxyz";
            result = new string(Enumerable.Repeat(chars, length)
             .Select(s => s[random.Next(s.Length)]).ToArray());
            System.Threading.Thread.Sleep(10);
            return result;
        }
        private void btnAddTransaction_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dtResult = new DataTable();
                DataRow[] results = null;
                string sSelectedItems = "", sSelectedBranches = "", sSelectedPremix = "", sParams = "", branch = "";
                if (selectedItems != null)
                {
                    if (selectedItems.Count() > 0)
                    {
                        sSelectedItems = string.Join(",", selectedItems);
                        lblSelectedItem.Text = "Item [" + (isAllItem ? "All" : sSelectedItems) + "]";
                        sParams = "item_code IN (" + sSelectedItems + ")";
                    }
                }

                if (selectedBranches != null)
                {
                    if (selectedBranches.Count() > 0)
                    {
                        sSelectedBranches = string.Join(",", selectedBranches);
                        lblSelectedBranch.Text = "Branch [" + (isAllBranch ? "All" : sSelectedBranches) + "]";
                        sParams += string.IsNullOrEmpty(sSelectedBranches.Trim()) ? "" : (string.IsNullOrEmpty(sSelectedItems.Trim()) ? "" : " AND") + " branch IN (" + sSelectedBranches + ")";
                    }
                }
                if (selectedPremix != null)
                {
                    if (selectedPremix.Count() > 0)
                    {
                        sSelectedPremix = string.Join(",", selectedPremix);
                        lblSelectedPremix.Text = "Premix [" + (isAllPremix ? "All" : sSelectedPremix) + "]";
                        sParams += string.IsNullOrEmpty(sSelectedPremix.Trim()) ? "" : (string.IsNullOrEmpty(sSelectedItems.Trim()) ? string.IsNullOrEmpty(sSelectedBranches.Trim()) ? "" : " AND" : " AND") + " premix_code IN (" + sSelectedPremix + ")";
                    }
                }
                if (!string.IsNullOrEmpty(sParams.Trim()))
                {
                    Console.WriteLine(sParams);
                    results = dtGlobal.Select(sParams);
                }
                DataTable dtCopyTable = results == null ? dtGlobal : results.Any() ? results.CopyToDataTable() : new DataTable();
                DataTable dtFormat = loadForDeliveryFormat(dtCopyTable, true, "", true);

                JObject jo = new JObject();
                JArray jaBody = new JArray();

                JArray jaRows = new JArray();
                DataView dv = dtFormat.DefaultView;
                dv.Sort = "item_code,branch ASC";
                dtFormat = dv.ToTable();
                string hashedID = "";
                string listBranchHaveNegativeFinalForDelivery = "";
                string item = "";
                foreach (DataRow row in dtFormat.Rows)
                {
                    if (branch.Trim() != row["branch"].ToString().Trim())
                    {

                        JObject joHeader = new JObject();
                        joHeader.Add("transdate", null);
                        joHeader.Add("delivery_date", null);
                        joHeader.Add("prod_whse", apic.findValueInDataTable(dtProdWhse, cmbProdWhse.Text, "whsename", "whsecode"));
                        joHeader.Add("remarks", null);
                        joHeader.Add("shift", null);
                        joHeader.Add("hashed_id", RandomString(100));
                        jo.Add("header", joHeader);
                        jo.Add("rows", jaRows);
                        if (jaRows.Count > 0)
                        {
                            jaBody.Add(jo);
                        }
                        jaRows = new JArray();
                        jo = new JObject();
                        branch = row["branch"].ToString();
                    }
                    if (branch.Trim() == row["branch"].ToString().Trim())
                    {
                        double targetForDel = 0.00, finalForDel = 0.00, doubleTemp = 0.00;
                        bool isTargetForDel = false, isFinalForDel = false;
                        if (double.TryParse(row["target_for_delivery"].ToString(), out doubleTemp))
                        {
                            targetForDel = Convert.ToDouble(row["target_for_delivery"].ToString());
                            isTargetForDel = true;
                        }
                        if (double.TryParse(row["final_for_delivery"].ToString(), out doubleTemp))
                        {
                            finalForDel = Convert.ToDouble(row["final_for_delivery"].ToString());
                            isFinalForDel = true;
                        }
                        if (finalForDel < 0)
                        {
                            if(item != row["item_code"].ToString())
                            {
                                listBranchHaveNegativeFinalForDelivery += row["item_code"].ToString() + Environment.NewLine;
                            }
                        }
                        JObject joRows = new JObject();
                        item = row["item_code"].ToString();
                        joRows.Add("item_code", row["item_code"].ToString());
                        joRows.Add("to_branch", row["branch"].ToString());
                        joRows.Add("quantity", isTargetForDel ? targetForDel : (double?)null);
                        joRows.Add("final_qty", isFinalForDel ? finalForDel : (double?)null);
                        joRows.Add("uom", row["uom"].ToString());
                        jaRows.Add(joRows);
                        //jaRows.Add(joRows);
                        //JArray jaTemp = JArray.Parse(jo["rows"].ToString());
                        //jaTemp.Add(joRows);
                        //jo["rows"] = jaTemp;
                    }
                    bool isLast = dtFormat.Rows.IndexOf(row) + 1 == dtFormat.Rows.Count;
                    if (isLast)
                    {
                        JObject joHeader = new JObject();
                        joHeader.Add("transdate", null);
                        joHeader.Add("delivery_date", null);
                        joHeader.Add("prod_whse", apic.findValueInDataTable(dtProdWhse, cmbProdWhse.Text, "whsename", "whsecode"));
                        joHeader.Add("remarks", null);
                        joHeader.Add("shift", null);
                        joHeader.Add("hashed_id", RandomString(100));
                        jo.Add("header", joHeader);
                        jo.Add("rows", jaRows);
                        jaBody.Add(jo);
                    }
                }
                this.Cursor = Cursors.Default;
                if (!string.IsNullOrEmpty(listBranchHaveNegativeFinalForDelivery.Trim()))
                {
                    apic.showCustomMsgBox("Validation", "You have negative final for delivery on following items. Please check." + Environment.NewLine + Environment.NewLine + "-" + listBranchHaveNegativeFinalForDelivery);
                }
                else
                {
                    ComputedForecast_ForDelivery_Dialog.isSubmit = false;
                    ComputedForecast_ForDelivery_Dialog frm = new ComputedForecast_ForDelivery_Dialog(jaBody, branch);
                    frm.ShowDialog();
                    if (ComputedForecast_ForDelivery_Dialog.isSubmit)
                    {
                        //loadForDelivery(dtGlobal, false, "");
                        btnAddTransaction.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnExpand_Click(object sender, EventArgs e)
        {
            loadForDelivery(dtGlobal, true, "");
        }

        public string haveNegativeFinalForDelivery()
        {
            string sresult = "";
            DataTable dtResult = new DataTable();
            DataRow[] results = null;
            string sSelectedItems = "", sSelectedBranches = "", sSelectedPremix = "", sParams = "", branch = "";
            if (selectedItems != null)
            {
                if (selectedItems.Count() > 0)
                {
                    sSelectedItems = string.Join(",", selectedItems);
                    lblSelectedItem.Invoke(new Action(delegate ()
                    {
                        lblSelectedItem.Text = "Item [" + (isAllItem ? "All" : sSelectedItems) + "]";
                    }));

                    sParams = "item_code IN (" + sSelectedItems + ")";
                }
                else
                {
                    lblSelectedItem.Invoke(new Action(delegate ()
                    {
                        lblSelectedItem.Text = "Item [All]";
                    }));

                }
            }
            else
            {
                lblSelectedItem.Invoke(new Action(delegate ()
                {
                    lblSelectedItem.Text = "Item [All]";
                }));
            }
            if (selectedBranches != null)
            {
                if (selectedBranches.Count() > 0)
                {
                    sSelectedBranches = string.Join(",", selectedBranches);
                    lblSelectedBranch.Invoke(new Action(delegate ()
                    {
                        lblSelectedBranch.Text = "Branch [" + (isAllBranch ? "All" : sSelectedBranches) + "]";
                    }));

                    sParams += string.IsNullOrEmpty(sSelectedBranches.Trim()) ? "" : (string.IsNullOrEmpty(sSelectedItems.Trim()) ? "" : " AND") + " branch IN (" + sSelectedBranches + ")";
                }
                else
                {
                    lblSelectedBranch.Invoke(new Action(delegate ()
                    {
                        lblSelectedBranch.Text = "Branch [All]";
                    }));
                }
            }
            else
            {
                lblSelectedBranch.Invoke(new Action(delegate ()
                {
                    lblSelectedBranch.Text = "Branch [All]";
                }));
            }
            if (selectedPremix != null)
            {
                if (selectedPremix.Count() > 0)
                {
                    sSelectedPremix = string.Join(",", selectedPremix);
                    lblSelectedPremix.Invoke(new Action(delegate ()
                    {
                        lblSelectedPremix.Text = "Premix [" + (isAllPremix ? "All" : sSelectedPremix) + "]";
                    }));
                    sParams += string.IsNullOrEmpty(sSelectedPremix.Trim()) ? "" : (string.IsNullOrEmpty(sSelectedItems.Trim()) ? string.IsNullOrEmpty(sSelectedBranches.Trim()) ? "" : " AND" : " AND") + " premix_code IN (" + sSelectedPremix + ")";
                }
                else
                {
                    lblSelectedPremix.Invoke(new Action(delegate ()
                    {
                        lblSelectedPremix.Text = "Premix [All]";
                    }));
                }
            }
            else
            {
                lblSelectedPremix.Invoke(new Action(delegate ()
                {
                    lblSelectedPremix.Text = "Premix [All]";
                }));
            }
            if (!string.IsNullOrEmpty(sParams.Trim()))
            {
                Console.WriteLine(sParams);
                dtGlobal.CaseSensitive = true;
                results = dtGlobal.Select(sParams);

            }
            DataTable dtCopyTable = results == null ? dtGlobal : results.Any() ? results.CopyToDataTable() : new DataTable();
            DataTable dtFormat = loadForDeliveryFormat(dtCopyTable, true, "", true);
            if (dtFormat.Rows.Count > 0)
            {
                string listBranchHaveNegativeFinalForDelivery = "";
                string item = "";
                foreach(DataRow row in dtFormat.Rows)
                {
                    //Console.WriteLine("rowww " + row["item_code"].ToString());
                    double doubleTemp = 0.00;
                    double finalForDelivery = double.TryParse(row["final_for_delivery"].ToString(), out doubleTemp) ? Convert.ToDouble(row["final_for_delivery"].ToString()) : doubleTemp;
                    if(finalForDelivery < 0)
                    {
                        if (item != row["item_code"].ToString() && !string.IsNullOrEmpty(row["item_code"].ToString()))
                        {
                            //Console.WriteLine("meow " + row["item_code"].ToString());
                            listBranchHaveNegativeFinalForDelivery += "-" + row["item_code"].ToString() + Environment.NewLine;
                        }
                    }
                    item = row["item_code"].ToString();
                }
                if (!string.IsNullOrEmpty(listBranchHaveNegativeFinalForDelivery.Trim()))
                {
                    sresult= "You have negative final for delivery on following items. Please check." + Environment.NewLine + Environment.NewLine + listBranchHaveNegativeFinalForDelivery;
                }
            }
            return sresult;
        }

        private void btnAddTransaction2_Click(object sender, EventArgs e)
        {
            try
            {
                string branch = apic.findValueInDataTable(dtProdWhse, cmbProdWhse.Text, "whsename", "whsecode");
                int checkFinalProdQty = 0;
                JArray jaRows = new JArray();
                DataRow[] results = null;
                string sSelectedItems = "", sSelectedBranches = "", sSelectedPremix = "", sParams = "";
                double doubleTemp = 0.00;
                DataTable dtCloned = new DataTable();
                if (selectedItems != null)
                {
                    if (selectedItems.Count() > 0)
                    {
                        sSelectedItems = string.Join(",", selectedItems);
                        lblSelectedItem.Text = "Item [" + (isAllItem ? "All" : sSelectedItems) + "]";
                        sParams = "item_code IN (" + sSelectedItems + ")";
                    }
                    else
                    {
                        lblSelectedItem.Text = "Item [All]";
                    }
                }
                else
                {
                    lblSelectedItem.Text = "Item [All]";
                }
                if (selectedBranches != null)
                {
                    if (selectedBranches.Count() > 0)
                    {
                        sSelectedBranches = string.Join(",", selectedBranches);
                        lblSelectedBranch.Text = "Branch [" + (isAllBranch ? "All" : sSelectedBranches) + "]";
                        sParams += string.IsNullOrEmpty(sSelectedBranches.Trim()) ? "" : (string.IsNullOrEmpty(sSelectedItems.Trim()) ? "" : " AND") + " branch IN (" + sSelectedBranches + ")";
                    }
                    else
                    {
                        lblSelectedBranch.Text = "Branch [All]";
                    }
                }
                else
                {
                    lblSelectedBranch.Text = "Branch [All]";
                }
                if (selectedPremix != null)
                {
                    if (selectedPremix.Count() > 0)
                    {
                        sSelectedPremix = string.Join(",", selectedPremix);
                        lblSelectedPremix.Text = "Premix [" + (isAllPremix ? "All" : sSelectedPremix) + "]";
                        sParams += string.IsNullOrEmpty(sSelectedPremix.Trim()) ? "" : (string.IsNullOrEmpty(sSelectedItems.Trim()) ? string.IsNullOrEmpty(sSelectedBranches.Trim()) ? "" : " AND" : " AND") + " premix_code IN (" + sSelectedPremix + ")";
                    }
                    else
                    {
                        lblSelectedPremix.Text = "Premix [All]";
                    }
                }
                if (!string.IsNullOrEmpty(sParams.Trim()))
                {
                    results = dtGlobal.Select(sParams);
                }
                DataTable dtCopyTable = results == null ? dtGlobal : results.Any() ? results.CopyToDataTable() : new DataTable();

                dtCloned = dtCopyTable.Clone();
                if (dtCloned.Columns.Contains("prod_min_qty"))
                {
                    dtCloned.Columns["prod_min_qty"].DataType = typeof(double);
                    foreach (DataRow row in dtCopyTable.Rows)
                    {
                        row["prod_min_qty"] = double.TryParse(row["prod_min_qty"].ToString(), out doubleTemp) ? Convert.ToDouble(row["prod_min_qty"].ToString()) : doubleTemp;
                        dtCloned.ImportRow(row);
                    }

                    DataTable dt = loadForProductionQuery(dtCloned, "");
                    //foreach(DataColumn col in dt.Columns){
                    //    Console.WriteLine("dt " + col.ColumnName);
                    //}
                    //foreach (DataRow row in dtManualItem.Rows)
                    //{
                    //    dt.ImportRow(row);
                    //}
                    //foreach (DataRow row in dtManualItem.Rows)
                    //{
                    //    dt.Rows.Add(row["branch"], row["item_code"], row["sold"], row["datediff"], row["average"], row["last_bal"], row["target_for_del"], row["prod_min_qty"], row["prod_ending_bal"], row["uom"], row["premix_code"], 0.00, 0, 0.00, 0.00, 0.00,);
                    //}

                    foreach (DataRow row in dt.Rows)
                    {
                        double finalProdQty = 0.00, targetForProd = 0.00;
                        bool isFinalProdQty = false, isTargetForProd = false;
                        if (double.TryParse(row["final_prod_qty"].ToString(), out doubleTemp))
                        {
                            finalProdQty = Convert.ToDouble(row["final_prod_qty"].ToString());
                            isFinalProdQty = true;
                        }
                        if (!double.TryParse(row["final_base"].ToString(), out doubleTemp))
                        {
                            checkFinalProdQty++;
                        }else if (Convert.ToDouble(row["final_base"].ToString()) <= 0)
                        {
                            checkFinalProdQty++;
                        }
                        if (double.TryParse(row["target_for_prod"].ToString(), out doubleTemp))
                        {
                            targetForProd = Convert.ToDouble(row["target_for_prod"].ToString());
                            isTargetForProd = true;
                        }
                        string itemCode = row["item_code"].ToString();
                        string uom = row["uom"].ToString();
                        JObject joRows = new JObject();
                        joRows.Add("item_code", itemCode);
                        if (isTargetForProd)
                        {
                            joRows.Add("targeted_qty", targetForProd);
                        }
                        else
                        {
                            joRows.Add("targeted_qty", null);
                        }
                        joRows.Add("planned_qty", isFinalProdQty ? finalProdQty : (double?)null);
                        joRows.Add("uom", uom);
                        jaRows.Add(joRows);
                    }
                    string shaveNegativeFinalForDelivery = haveNegativeFinalForDelivery();
                    if (!string.IsNullOrEmpty(haveNegativeFinalForDelivery()))
                    {
                        apic.showCustomMsgBox("Validation", shaveNegativeFinalForDelivery);
                    }
                    else if (checkFinalProdQty > 0)
                    {
                        MessageBox.Show("You have unfilled Final Base!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        Console.WriteLine(jaRows);
                        ComputedForecast_ForProduction_Dialog.isSubmit = false;
                        ComputedForecast_ForProduction_Dialog frm = new ComputedForecast_ForProduction_Dialog(jaRows, branch);
                        frm.ShowDialog();
                        if (ComputedForecast_ForProduction_Dialog.isSubmit)
                        {
                            btnAddTransaction2.Enabled = false;
                            //loadForProduction("");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void checkFromDate_CheckedChanged(object sender, EventArgs e)
        {
            dtFromDate.Visible = checkFromDate.Checked;
        }

        private void checkToDate_CheckedChanged(object sender, EventArgs e)
        {
            dtToDate.Visible = checkToDate.Checked;
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            //Console.WriteLine("two");
            ////gridView1.OptionsView.RowAutoHeight = false;
        }

        private void gridView1_CellValueChanged_1(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
        }

        private void gridView1_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            e.RowHeight = 60;
        }

        private void checkFromTime_CheckedChanged(object sender, EventArgs e)
        {
            cmbFromTime.Visible = checkFromTime.Checked;
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

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            string selectedColumnfieldName = e.Column.FieldName;
            if(e.RowHandle >= 0)
            {
                if (selectedColumnfieldName.Equals("final_for_delivery"))
                {
                    if (string.IsNullOrEmpty(gridView1.GetRowCellValue(e.RowHandle, "item_code").ToString()))
                    {
                        e.Appearance.BackColor = Color.FromArgb(248, 252, 154);
                    }
                    if (!string.IsNullOrEmpty(gridView1.GetRowCellValue(e.RowHandle, "branch").ToString()) && e.RowHandle == lastTopIndex)
                    {
                        e.Appearance.BackColor = Color.FromArgb(248, 252, 154);
                    }
                    double finalForDel = 0.00, doubleTemp = 0.00;
                    finalForDel = double.TryParse(gridView1.GetRowCellValue(e.RowHandle, "final_for_delivery").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(e.RowHandle, "final_for_delivery").ToString()) : doubleTemp;
                    if(finalForDel < 0)
                    {
                        e.Appearance.ForeColor = Color.Red;
                    }
                }
            }
        }

        private void gridView2_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            string selectedColumnfieldName = e.Column.FieldName;
            if(e.RowHandle >= 0)
            {
                if (selectedColumnfieldName.Equals("final_base") || selectedColumnfieldName.Equals("buffer_stock"))
                {
                    e.Appearance.BackColor = Color.FromArgb(248, 252, 154);
                }
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {

            try
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable dtCloned = dtGlobal.Clone();
                DataTable dtFiltered = new DataTable();
                if (dtCloned.Columns.Contains("prod_min_qty"))
                {
                    double doubleTemp = 0.00;
                    dtCloned.Columns["prod_min_qty"].DataType = typeof(double);
                    foreach (DataRow row in dtCurrentGlobal.Rows)
                    {
                        row["prod_min_qty"] = double.TryParse(row["prod_min_qty"].ToString(), out doubleTemp) ? Convert.ToDouble(row["prod_min_qty"].ToString()) : doubleTemp;
                        dtCloned.ImportRow(row);
                    }
                    dtFiltered = loadForProductionQuery(dtCloned, "");
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("item_code", typeof(string));

                if (!dt.Columns.Contains("item_group"))
                {
                    dt.Columns.Add("item_group");
                }

                int i_noItemFound = 0;
                foreach (DataRow row in dtBom.Rows)
                {
                    string s = row["item_code"].ToString().Replace(@"'", "''");
                    if (dtFiltered.Rows.Count > 0)
                    {
                        DataRow[] rows = dtFiltered.Select("item_code='" + s + "'");
                        if (rows.Count() <= 0)
                        {
                            DataRow[] items = dtItem != null ? dtItem.Select("item_code='" + s + "'") : null;
                            dt.Rows.Add(row["item_code"].ToString(), items.Length > 0 ? items[0]["item_group"].ToString() :"123");
                        }
                    }
                    else
                    {
                        i_noItemFound++;
                    }
                }
                if (i_noItemFound > 0)
                {
                    this.Cursor = Cursors.Default;
                    apic.showCustomMsgBox("Validation", "No Item found! Please click Search Query button first");
                }
                else
                {
                    DataView dv = dt.DefaultView;
                    dv.Sort = "item_code,item_group ASC";
                    dt = dv.ToTable();
                    this.Cursor = Cursors.Default;
                    CreateForDeliveryProduction_AddManualItem.selectedItems = null;
                    CreateForDeliveryProduction_AddManualItem.count = dt.Rows.Count;
                    CreateForDeliveryProduction_AddManualItem frm = new CreateForDeliveryProduction_AddManualItem(dt, dtItemGroup);
                    frm.ShowDialog();
                    string[] selectedItemCode = CreateForDeliveryProduction_AddManualItem.selectedItems;
                    this.Cursor = Cursors.WaitCursor;
                    if (dtGlobal.Rows.Count > 0)
                    {
                        if (selectedItemCode != null)
                        {
                            //dt1.Columns.Add("item_code", typeof(string));
                            //dt1.Columns.Add("prod_ending_bal", typeof(double));
                            //dt1.Columns.Add("final_for_delivery", typeof(double));
                            //dt1.Columns.Add("buffer_stock", typeof(double));
                            //dt1.Columns.Add("uom", typeof(string));
                            //dt1.Columns.Add("target_for_prod", typeof(double));
                            //dt1.Columns.Add("prod_min_qty", typeof(double));
                            //dt1.Columns.Add("target_base", typeof(double));
                            //dt1.Columns.Add("final_base", typeof(double));

                            //dt1.Columns.Add("final_prod_qty", typeof(double));
                            //dt1.Columns.Add("branch", typeof(string));
                            //dt1.Columns.Add("row_index", typeof(int));
                            //dt1.Columns.Add("for_del_for_prod", typeof(double));


                            var col1 = gridView2.Columns["item_code"];
                            var col2 = gridView2.Columns["prod_ending_bal"];
                            var col3 = gridView2.Columns["final_for_delivery"];
                            var col4 = gridView2.Columns["uom"];
                            var col5 = gridView2.Columns["target_for_prod"];
                            var col6 = gridView2.Columns["prod_min_qty"];
                            var col7 = gridView2.Columns["target_base"];
                            var col8 = gridView2.Columns["final_base"];
                            var col9 = gridView2.Columns["final_prod_qty"];
                            var col10 = gridView2.Columns["final_prod_qty"];
                            var col11 = gridView2.Columns["branch"];
                            var col12 = gridView2.Columns["row_index"];
                            var col13 = gridView2.Columns["for_del_for_prod"];
                            var col14 = gridView2.Columns["isManualItem"];
                            foreach (string item in selectedItemCode)
                            {
                                string s = item.Replace(@"'", "''");
            
                                DataRow[] row = dtBom.Select("item_code='" + s + "'");
                                if (row.Count() > 0)
                                {
                                    gridView2.AddNewRow();

                                    int rowHandleNew = gridView2.FocusedRowHandle;
                                    gridView2.SetRowCellValue(rowHandleNew, col1, item);
                                    gridView2.SetRowCellValue(rowHandleNew, col2, 0.00);
                                    gridView2.SetRowCellValue(rowHandleNew, col3, 0.00);
                                    gridView2.SetRowCellValue(rowHandleNew, col4, row[0]["uom"]);
                                    gridView2.SetRowCellValue(rowHandleNew, col5, 0.00);
                                    gridView2.SetRowCellValue(rowHandleNew, col6, row[0]["quantity"]);
                                    gridView2.SetRowCellValue(rowHandleNew, col7, 0.00);
                                    gridView2.SetRowCellValue(rowHandleNew, col8, 0.00);
                                    gridView2.SetRowCellValue(rowHandleNew, col9, 0.00);
                                    gridView2.SetRowCellValue(rowHandleNew, col10, 0.00);
                                    gridView2.SetRowCellValue(rowHandleNew, col11, "A1 Main");
                                    gridView2.SetRowCellValue(rowHandleNew, col12, 0);
                                    gridView2.SetRowCellValue(rowHandleNew, col13, 0);
                                    gridView2.SetRowCellValue(rowHandleNew, col14, true);
                                    dtManualItem.Rows.Add("A1 Main", item, 0.00, 0, 0.00, 0.00, 0.00, row[0]["quantity"], 0.00, row[0]["uom"], row[0]["premix_code"], 0.00, 0, 0.00, 0.00, 0.00, true);
                                    this.Cursor = Cursors.Default;
                                }
                            }
                        }
                    }
                    this.Cursor = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void gridView2_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            try
            {
                if (gridView2 != null)
                {
                    string selectedColumnfieldName = gridView2.FocusedColumn.FieldName;

                    bool isManualItem = false, boolTemp = false;
                    isManualItem = bool.TryParse(gridView2.GetFocusedRowCellValue("isManualItem").ToString(), out boolTemp) ? Convert.ToBoolean(gridView2.GetFocusedRowCellValue("isManualItem").ToString()) : boolTemp;

                    if (selectedColumnfieldName.Equals("item_code") && isManualItem && btnAddTransaction2.Enabled)
                    {
                        if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
                        {
                            string itemCode = gridView2.GetFocusedRowCellValue("item_code").ToString();
                            DXMenuItem item = new DXMenuItem("Remove " + itemCode);
                            item.Click += (o, args) =>
                            {
                                DialogResult dialogResult = MessageBox.Show("Are you sure you want to remove " + itemCode + "?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (dialogResult == DialogResult.Yes)
                                {
                                 try
                                    {
                                        string s = itemCode.Replace(@"'", "''");
                                        DataRow[] rows = dtManualItem.Select("item_code='" + s + "'");
                                        if (rows.Count() > 0)
                                        {
                                            foreach (DataRow r in rows)
                                            {
                                                dtManualItem.Rows.Remove(r);
                                            }
                                            gridView2.DeleteRow(gridView2.FocusedRowHandle);
                                            MessageBox.Show("Removed " + itemCode + "!", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                }
                            };
                            e.Menu.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void gridView2_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (e.RowHandle >= 0)
            {
                var colVar = gridView2.Columns["isManualItem"];
                //252, 101, 101
                if (gridView2.Columns.Contains(colVar))
                {
                    bool isManual = false, boolTemp = false;
                    isManual = gridView2.GetRowCellValue(e.RowHandle, "isManualItem") == null ? boolTemp : bool.TryParse(gridView2.GetRowCellValue(e.RowHandle, "isManualItem").ToString(), out boolTemp) ? Convert.ToBoolean(gridView2.GetRowCellValue(e.RowHandle, "isManualItem").ToString()) : boolTemp;
                    if (isManual)
                    {
                        e.Appearance.BackColor = Color.FromArgb(255, 230, 138);
                    }
                }
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
          
        }

        private void checkWithLastBal_CheckedChanged(object sender, EventArgs e)
        {
            dtEndingDate.Enabled = checkEndingDate.Enabled = cmbEndingTime.Enabled = checkEndingTime.Enabled = checkWithLastBal.Checked;
            //dtEndingDate.Enabled = checkEndingTime.Enabled = checkEndingTime.Checked = cmbEndingTime.Enabled = dtEndingDate.Visible = cmbEndingTime.Visible = checkEndingDate.Visible = checkEndingTime.Visible = checkWithLastBal.Checked;
        }

        private void cmbProdWhse_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelProdWhse.Invoke(new Action(delegate ()
            {
                panelProdWhse.BackColor = cmbProdWhse.SelectedIndex == 0 ? Color.Red : Color.Transparent;
            }));
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            bgFiltering = new BackgroundWorker();
            bgFiltering.DoWork += delegate
            {
                loadFiltering("branch", "Branch");
            };
            bgFiltering.RunWorkerCompleted += delegate
            {
                closeForm();
            };
            bg(bgFiltering);
        }

        //private void btnCreate_Click(object sender, EventArgs e)
        //{
        //    CreateForDeliveryProduction2 frm = new CreateForDeliveryProduction2(dtCurrentGlobal);
        //    frm.ShowDialog();
        //}

        private void txtMultiplier_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void txtMultiplier_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
       && !char.IsDigit(e.KeyChar)
       && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void btnSelectPremix_Click(object sender, EventArgs e)
        {
            bgFiltering = new BackgroundWorker();
            bgFiltering.DoWork += delegate
            {
                loadFiltering("premix_code", "Premix");
            };
            bgFiltering.RunWorkerCompleted += delegate
            {
                closeForm();
            };
            bg(bgFiltering);
        }

        public void loadFiltering(string columnName, string title)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("name");
                if (title.Equals("Item") && !dt.Columns.Contains("item_group"))
                {
                    dt.Columns.Add("item_group");
                }
                DataTable dtTempGlobal = dtGlobal;
                if (title.Equals("Item"))
                {
                    string sParams = "", sSelectedPremix = "";
                    if (selectedPremix != null)
                    {
                        if (selectedPremix.Count() > 0)
                        {
                            sSelectedPremix = string.Join(",", selectedPremix);
                            sParams = "premix_code IN (" + sSelectedPremix + ") AND premix_code IS NOT NULL";
                        }
                    }

                    if (!string.IsNullOrEmpty(sParams.Trim()))
                    {
                        dtTempGlobal.CaseSensitive = true;
                        DataRow[] results = dtTempGlobal.Select(sParams);
                        dtTempGlobal = results == null ? dtTempGlobal : results.Any() ? results.CopyToDataTable() : new DataTable();
                    }
                }
       
                foreach (DataRow row in dtTempGlobal.Rows)
                {
                    var colItemCode = row[columnName].ToString();
                    if (colItemCode != null)
                    {
                        if (title.Equals("Item"))
                        {
           
                            string s = row[columnName].ToString().Replace(@"'", "''");
                            DataRow[] items = dtItem != null ? dtItem.Select("item_code='" + s + "'") : null;
                            dt.Rows.Add(row[columnName].ToString(), items.Length > 0 ? items[0]["item_group"].ToString() : null);
                        }
                        else
                        {
                            dt.Rows.Add(row[columnName].ToString());
                        }
                    }
                }
                if (dt.Rows.Count > 0)
                {
                    string[] currentSelectedNames = title.Equals("Item") ? selectedItems : title.Equals("Branch") ? selectedBranches : selectedPremix;
                    DataView view = new DataView(dt);
                    DataTable distinctValues = new DataTable();
                    if (title.Equals("Item"))
                    {
                        distinctValues = view.ToTable(true, "name", "item_group");
                    }
                    else
                    {
                        distinctValues = view.ToTable(true, "name");
                    }
                    CreateForDeliveryProduction_filtering.count = 0;
                    CreateForDeliveryProduction_filtering.selectedNames = null;
                    DataTable dtTempItemGroup = title.Equals("Item") ? dtItemGroup : new DataTable();
                    CreateForDeliveryProduction_filtering frm = new CreateForDeliveryProduction_filtering(distinctValues, title, currentSelectedNames, dtTempItemGroup);
                    frm.ShowDialog();
                    if (CreateForDeliveryProduction_filtering.selectedNames != null)
                    {
                        if (title.Equals("Item"))
                        {
                            selectedItems = CreateForDeliveryProduction_filtering.selectedNames;
                            isAllItem = CreateForDeliveryProduction_filtering.count == selectedItems.Count();
                        }
                        else if (title.Equals("Branch"))
                        {
                            selectedBranches = CreateForDeliveryProduction_filtering.selectedNames;
                            isAllBranch = CreateForDeliveryProduction_filtering.count == selectedBranches.Count();
                        }
                        else
                        {
                            selectedPremix = CreateForDeliveryProduction_filtering.selectedNames;
                            isAllPremix = CreateForDeliveryProduction_filtering.count == selectedPremix.Count();
                        }
                        loadForDelivery(dtGlobal, false, "");
                    }
                    else
                    {
                        selectedItems = null;
                        loadForDelivery(dtGlobal, false, "");
                    }
                }
                else
                {
                    MessageBox.Show("No " + title + " to filtered!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {

            bgFiltering = new BackgroundWorker();
            bgFiltering.DoWork += delegate
            {
                loadFiltering("item_code", "Item");
            };
            bgFiltering.RunWorkerCompleted += delegate
            {
                closeForm();
            };
            bg(bgFiltering);
        }
    }
}
