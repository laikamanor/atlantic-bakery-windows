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
using AB.API_Class.Item_Sales_Summary;
using Newtonsoft.Json.Linq;
using AB.UI_Class;
namespace AB
{
    public partial class ItemSalesQuantityTab : Form
    {
        public ItemSalesQuantityTab()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        DataTable dtBranches = new DataTable();
        branch_class branchc = new branch_class();
        DataTable dtGlobal = new DataTable();
        public static DataTable dtSelectedBranches = new DataTable();
        itemsalessummary_class itemssc = new itemsalessummary_class();
        private void ItemSalesSummaryTab_Load(object sender, EventArgs e)
        {
            dtSelectedBranches.Columns.Clear();
            dtSelectedBranches.Columns.Add("select", typeof(bool));
            dtSelectedBranches.Columns.Add("name", typeof(string));
            dtSelectedBranches.Columns.Add("type", typeof(string));
            dtSelectedBranches.Columns.Add("is_all", typeof(bool));
            dtSelectedBranches.Rows.Clear();
            dtFromDate.EditValue = DateTime.Now;
            dtToDate.EditValue = DateTime.Now;
            cmbFromTime.SelectedIndex = 0;
            cmbToTime.SelectedIndex = cmbToTime.Properties.Items.Count - 1;
            loadBranch();
            button1.Visible = cmbBranch.Text.Equals("All") ? true : false;
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

        public async void loadBranch()
        {
            try
            {
                string currentBranch = "";
                bool isAdmin = false;
                cmbBranch.Invoke(new Action(delegate ()
                {
                    cmbBranch.Properties.Items.Clear();
                }));
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
                dtBranches = await branchc.returnBranches();
                if (isAdmin)
                {
                    dtBranches = await branchc.returnBranches();

                    cmbBranch.Invoke(new Action(delegate ()
                    {
                        cmbBranch.Properties.Items.Add("All");
                        foreach (DataRow row in dtBranches.Rows)
                        {
                            cmbBranch.Properties.Items.Add(row["name"]);
                        }
                    }));


                }
                else
                {
                    foreach (DataRow row in dtBranches.Rows)
                    {
                        if (row["code"].ToString() == currentBranch)
                        {
                            cmbBranch.Invoke(new Action(delegate ()
                            {
                                cmbBranch.Properties.Items.Add(row["name"]);
                            }));
                            break;
                        }
                    }
                }
                //default text 
                //kapag admin or to whse all yung lalabas
                //kapag hindi kung ano yung current whse nya yun yung lalabas
                string branchName = "";
                foreach (DataRow row in dtBranches.Rows)
                {
                    if (row["code"].ToString().Trim().ToLower() == currentBranch.Trim().ToLower())
                    {
                        branchName = row["name"].ToString();
                        break;
                    }
                }
                cmbBranch.Invoke(new Action(delegate ()
                {
                    //cmbBranch.SelectedIndex = cmbBranch.Properties.Items.IndexOf(branchName);
                    cmbBranch.SelectedIndex = 0;
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        public void loadData()
        {
            string sBranch = "", sFromDate = "", sToDate = "", sParams = "", sFromTime = "", sToTime = "";
            bool isCheckFromDate = false, isCheckToDate = false;
            checkDate.Invoke(new Action(delegate ()
            {
                isCheckFromDate = checkDate.Checked;
            }));
            checkToDate.Invoke(new Action(delegate ()
            {
                isCheckToDate = checkToDate.Checked;
            }));
            cmbBranch.Invoke(new Action(delegate ()
            {
                sBranch = "?branch=" + apic.findValueInDataTable(dtBranches, cmbBranch.Text, "name", "code");
            }));
            dtFromDate.Invoke(new Action(delegate ()
            {
                sFromDate = "&from_date=" + (isCheckFromDate ? dtFromDate.DateTime.ToString("yyyy-MM-dd") : "");
            }));
            dtToDate.Invoke(new Action(delegate ()
            {
                sToDate = "&to_date=" + (isCheckToDate ? dtToDate.DateTime.ToString("yyyy-MM-dd") : "");
            }));
            cmbFromTime.Invoke(new Action(delegate ()
            {
                sFromTime = "&from_time=" + cmbFromTime.Text;
            }));
            cmbToTime.Invoke(new Action(delegate ()
            {
                sToTime = "&to_time=" + cmbToTime.Text;
            }));
            sParams = sBranch + sFromDate + sToDate + sFromTime + sToTime;
            DataTable dt = itemssc.loadData(sParams);
            dtGlobal = dt;
            int tcIndex = 0;
            tabControl1.Invoke(new Action(delegate ()
            {
                tcIndex = tabControl1.SelectedIndex;
            }));
            toggleTabs(tcIndex, dtGlobal);
        }

        public void toggleTabs(int tcIndex, DataTable dt)
        {
            if (tcIndex <= 0)
            {
                ItemSalesQuantity frm = new ItemSalesQuantity(dt, dtSelectedBranches);
                panelGrid.Invoke(new Action(delegate ()
                {
                    showForm(frm, dt, panelGrid);
                }));
            }
            else
            {
                ItemSalesQuantityGraph frm = new ItemSalesQuantityGraph(dt, dtSelectedBranches);
                panelGraph.Invoke(new Action(delegate ()
                {
                    showForm(frm, dt, panelGraph);
                }));
            }
        }


        public void showForm(Form form,DataTable dt,Panel pn)
        {
            pn.Controls.Clear();
            form.TopLevel = false;
            pn.Controls.Add(form);
            form.BringToFront();
            form.Show();
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

                            dt3.Rows.Add(q.ItemCode, q.AllTotalQuantity == null ? 0.00 : q.AllTotalQuantity, q.QuantityPerSelected == null ? 0.00 : q.QuantityPerSelected, "", (double?)null, (double?)null, (double?)null, (double?)null);
                        }

                        dt3.Rows.Add("", (double?)null, (double?)null, q.Branch, q.QuantityPerBranch == null ? 0.00 : q.QuantityPerBranch, q.Average == null ? 0.00 : q.Average, q.DateDiff == null ? 0 : q.DateDiff);
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
            return dt3;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            showFilter("Branch", dtBranches);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            showFilter("Item", dtGlobal);
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg();
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

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void checkDate_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkToDate_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }

}
