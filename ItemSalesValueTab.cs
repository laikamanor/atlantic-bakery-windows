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
using AB.UI_Class;
using Newtonsoft.Json;
using RestSharp;

namespace AB
{
    public partial class ItemSalesValueTab : Form
    {
        public ItemSalesValueTab()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        branch_class branchc = new branch_class();
        utility_class utilityc = new utility_class();
        DataTable dtBranch = new DataTable(), dtWhse = new DataTable();
        public static DataTable dtSelectedBranches = new DataTable();
        DataTable dtGlobal = new DataTable();
        private async void ItemSalesReportTab_Load(object sender, EventArgs e)
        {
            dtSelectedBranches.Columns.Clear();
            dtSelectedBranches.Columns.Add("select", typeof(bool));
            dtSelectedBranches.Columns.Add("name", typeof(string));
            dtSelectedBranches.Columns.Add("type", typeof(string));
            dtSelectedBranches.Columns.Add("is_all", typeof(bool));
            dtSelectedBranches.Rows.Clear();
            await loadBranch();
            loadTenderType();
            cmbFromTime.SelectedIndex = 0;
            cmbToTime.SelectedIndex = cmbToTime.Items.Count - 1;
            bg();
        }


        public void loadTenderType()
        {
            if (Login.jsonResult != null)
            {
                Cursor.Current = Cursors.WaitCursor;
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
                    var client = new RestClient(utilityc.URL);
                    client.Timeout = -1;
                    //string branch = "A1-S";
                    var request = new RestRequest("/api/sales/type/get_all");
                    request.AddHeader("Authorization", "Bearer " + token);
                    var response = client.Execute(request);
                    JObject jObject = new JObject();
                    cmbTransType.Items.Clear();
                    cmbTransType.Items.Add("All");
                    jObject = JObject.Parse(response.Content.ToString());
                    bool isSuccess = false;
                    foreach (var x in jObject)
                    {
                        if (x.Key.Equals("success"))
                        {
                            isSuccess = Convert.ToBoolean(x.Value.ToString());
                        }
                    }
                    if (isSuccess)
                    {
                        foreach (var x in jObject)
                        {
                            if (x.Key.Equals("data"))
                            {
                                if (x.Value.ToString() != "[]")
                                {
                                    JArray jsonArray = JArray.Parse(x.Value.ToString());
                                    for (int i = 0; i < jsonArray.Count(); i++)
                                    {
                                        JObject data = JObject.Parse(jsonArray[i].ToString());
                                        string code = "";
                                        foreach (var q in data)
                                        {
                                            if (q.Key.Equals("code"))
                                            {
                                                code = q.Value.ToString();
                                            }
                                        }
                                        cmbTransType.Items.Add(code);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        string msg = "No message response found";
                        foreach (var x in jObject)
                        {
                            if (x.Key.Equals("message"))
                            {
                                msg = x.Value.ToString();
                            }
                        }
                        MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                cmbTransType.SelectedIndex = 0;
                Cursor.Current = Cursors.Default;
            }
        }

        public async Task loadBranch()
        {
            string currentBranch = "";
            bool isAdmin = false;
            AutoCompleteStringCollection auto = new AutoCompleteStringCollection();
            //get muna whse and check kung admin , superadmin or manager
            if (Login.jsonResult != null)
            {
                foreach (var x in Login.jsonResult)
                {
                    if (x.Key.Equals("data"))
                    {
                        JObject jObjectData = JObject.Parse(x.Value.ToString());
                        foreach (var y in jObjectData)
                        {
                            if (y.Key.Equals("branch"))
                            {
                                currentBranch = y.Value.ToString();
                            }
                            else if (y.Key.Equals("isAdmin") || y.Key.Equals("isSuperAdmin") || y.Key.Equals("isManager") || y.Key.Equals("isCashier") || y.Key.Equals("isAccounting") || y.Key.Equals("isSalesAgent"))
                            {
                                isAdmin = string.IsNullOrEmpty(y.Value.ToString()) ? false : Convert.ToBoolean(y.Value.ToString());
                                if (isAdmin)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            dtBranch = await branchc.returnBranches();
            if (isAdmin)
            {
                dtBranch = await branchc.returnBranches();
                //cmbBranch.Items.Add("All");
                foreach (DataRow row in dtBranch.Rows)
                {
                    auto.Add(row["name"].ToString());
                    //cmbBranch.Items.Add(row["name"].ToString());
                }
            }
            else
            {
                foreach (DataRow row in dtBranch.Rows)
                {
                    if (row["code"].ToString() == currentBranch)
                    {
                        auto.Add(row["name"].ToString());
                        //cmbBranch.Items.Add(row["name"].ToString());
                        break;
                    }
                }
            }
            //default text 
            //kapag admin or to whse all yung lalabas
            //kapag hindi kung ano yung current whse nya yun yung lalabas
            string branchName = "";
            foreach (DataRow row in dtBranch.Rows)
            {
                if (row["code"].ToString().Trim().ToLower() == currentBranch.Trim().ToLower())
                {
                    branchName = row["name"].ToString();
                    break;
                }
            }
            //cmbBranch.SelectedIndex = cmbBranch.Items.IndexOf(branchName);
            //cmbBranch.AutoCompleteCustomSource = auto;
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
            tabControl1.Invoke(new Action(delegate ()
            {
                tcIndex = tabControl1.SelectedIndex;
            }));
            toggleTabs(tcIndex, dtGlobal);
        }

        public void loadData()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                string sBranch = "";
                sBranch = "?branch=";
                string sWhse = "";
                sWhse = "&whsecode=";

                string sTransType = "";
                cmbTransType.Invoke(new Action(delegate ()
                {
                    sTransType = "&transtype=" + (cmbTransType.SelectedIndex == 0 || cmbTransType.Text == "All" ? "" : cmbTransType.Text);
                }));

                string sFromTime = "";
                cmbFromTime.Invoke(new Action(delegate ()
                {
                    sFromTime = "&from_time=" + cmbFromTime.Text;
                }));

                string sToTime = "";
                cmbToTime.Invoke(new Action(delegate ()
                {
                    sToTime = "&to_time=" + cmbToTime.Text;
                }));

                string sFromDate = "";
                dtFromDate.Invoke(new Action(delegate ()
                {
                    sFromDate = "&from_date=" + (checkDate.Checked ? dtFromDate.Value.ToString("yyyy-MM-dd") : "");
                }));
                string sToDate = "";
                dtFromDate.Invoke(new Action(delegate ()
                {
                    sToDate = "&to_date=" + (checkToDate.Checked ? dtToDate.Value.ToString("yyyy-MM-dd") : "");
                }));
                string sResult = apic.loadData("/api/report/item/sales/detailed", sBranch + sWhse + sTransType + sFromTime + sToTime + sFromDate + sToDate, "", "", Method.GET, true);
                if (!string.IsNullOrEmpty(sResult.Trim()))
                {
                    if (sResult.Substring(0, 1).Equals("{"))
                    {
                   
                        JObject jResult = JObject.Parse(sResult);
                        JArray jData = (JArray)jResult["data"];
                        DataTable dt = (DataTable)JsonConvert.DeserializeObject(jData.ToString(), typeof(DataTable));
                        dtGlobal = dt;
                        int tcIndex = 0;
                        tabControl1.Invoke(new Action(delegate ()
                        {
                            tcIndex = tabControl1.SelectedIndex;
                        }));
                        toggleTabs(tcIndex, dtGlobal);
                    }
                }
                Cursor.Current = Cursors.Default;
            }
            catch (Exception ex)
            {
                apic.showCustomMsgBox(ex.Message, ex.ToString());
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int tcIndex = 0;
            tabControl1.Invoke(new Action(delegate ()
            {
                tcIndex = tabControl1.SelectedIndex;
            }));
            toggleTabs(tcIndex, dtGlobal);
        }

        public void showForm(Form form, DataTable dt, Panel pn)
        {
            pn.Controls.Clear();
            form.TopLevel = false;
            pn.Controls.Add(form);
            form.BringToFront();
            form.Show();
        }

        public void toggleTabs(int tcIndex, DataTable dt)
        {
            if (tcIndex <= 0)
            {
                ItemSalesValue frm = new ItemSalesValue(dtGlobal, dt,dtSelectedBranches);
                panelGrid.Invoke(new Action(delegate ()
                {
                    showForm(frm, dt, panelGrid);
                }));
            }
            else
            {
                ItemSalesValueGraph frm = new ItemSalesValueGraph(dtGlobal, dt, dtSelectedBranches);
                panelGraph.Invoke(new Action(delegate ()
                {
                    showForm(frm, dt, panelGraph);
                }));
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

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void checkDate_CheckedChanged(object sender, EventArgs e)
        {
            dtFromDate.Visible = checkDate.Checked;
        }

        private void checkToDate_CheckedChanged(object sender, EventArgs e)
        {
            dtFromDate.Visible = checkToDate.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            showFilter("Branch", dtBranch);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            showFilter("Item", dtGlobal);
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
                dt2.Rows.Add(q.ItemCode, q.AllTotalQuantity, q.TotalQuantityAsPerSelected, q.TotalNetAmountAsPerSelected);
            }
            var j = (from row in dt2.AsEnumerable()
                     join row2 in dt.AsEnumerable() on row.Field<string>("item_code") equals row2.Field<string>("item_code")
                     select new
                     {
                         ItemCode = row.Field<string>("item_code"),
                         AllTotalQuantity = row.Field<double>("total"),
                         QuantityPerSelected = row.Field<double>("quantity_per_selected"),
                         NetAmountPerSelected = row.Field<double>("net_amount_per_selected"),
                         Quantity = row2.Field<double>("quantity"),
                         UnitPrice = row2.Field<double>("unit_price"),
                         //DiscPrcnt = row2.Field<double>("discprcnt"),
                         DiscAmount = row2.Field<double>("disc_amount"),
                         GrossAmount = row2.Field<double>("gross_amount"),
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
                dt3.Columns.Add("net_amount", typeof(double));
                dt3.Columns.Add("percentage", typeof(double));
            }
            string item = "";
            foreach (var q in j)
            {
                if (tagged.Equals("Open"))
                {
                    if (q.ItemCode == item_code)
                    {
                        if (!item.Equals(q.ItemCode))
                        {

                            dt3.Rows.Add(q.ItemCode, q.AllTotalQuantity == null ? 0.00 : q.AllTotalQuantity, q.QuantityPerSelected == null ? 0.00 : q.QuantityPerSelected, (double?)null, "", (double?)null, (double?)null, (double?)null, (double?)null, (double?)null);
                        }

                        //dt3.Rows.Add( (double?)null, (double?)null, (double?)null, q.Quantity, q.Branch, q.UnitPrice, q.DiscPrcnt == null ? 0.00 : q.DiscPrcnt, q.DiscAmount == null ? 0.00 : q.DiscAmount, q.GrossAmount == null ? 0.00 : q.GrossAmount, q.NetAmount == null ? 0.00 : q.NetAmount);
                        double percent = (q.NetAmount / q.NetAmountPerSelected) * 100;

                        dt3.Rows.Add((double?)null, (double?)null, (double?)null, q.Quantity, q.Branch, q.UnitPrice, q.DiscAmount == null ? 0.00 : q.DiscAmount, q.GrossAmount == null ? 0.00 : q.GrossAmount, q.NetAmount == null ? 0.00 : q.NetAmount, percent);
                    }
                }
                else
                {
                    if (!item.Equals(q.ItemCode))
                    {
                        dt3.Rows.Add(q.ItemCode, q.AllTotalQuantity == null ? 0.00 : q.AllTotalQuantity, q.QuantityPerSelected == null ? 0.00 : q.QuantityPerSelected);
                    }

                }
                item = q.ItemCode;
            }
            if (tagged.Equals("Open"))
            {
                DataView view = new DataView(dt3);
                dt3 = view.ToTable(true, "item_code", "all_total_quantity", "total_quantity_as_per_selected_branch", "quantity_per_branch", "branch", "unit_price", "disc_amount", "gross_amount", "net_amount", "percentage");
            }

            return dt3;
        }
    }
}
