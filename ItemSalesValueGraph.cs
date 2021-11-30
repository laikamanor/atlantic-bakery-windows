using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace AB
{
    public partial class ItemSalesValueGraph : Form
    {
        public ItemSalesValueGraph(DataTable dt1, DataTable dt, DataTable dt2)
        {
            dtOriginal = dt1;
            dtGlobal = dt;
            dtSelectedBranches = dt2;
            InitializeComponent();
        }
        DataTable dtGlobal = new DataTable(), dtSelectedBranches = new DataTable(), dtOriginal = new DataTable();
        Dictionary<string, double> ExceptionMessages = new Dictionary<string, double>();
        string globalItemCode = "";
        double globalTotalNetAmount = 0.00;
        private void cmbTop_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadData();
        }

        public DataTable loadPopUpValues(string status, string itemCode)
        {
            DataTable dtResult = new DataTable();
            DataTable dt = dtGlobal;
            string sBranchSelected = "";
            string sItemSelected = "";
            bool isAllBranch = false, isAllItem = false, boolTemp = false;
            foreach(DataColumn col in dtSelectedBranches.Columns)
            {
                Console.WriteLine(col.Caption + "/" + col.DataType);
            }
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
            //Console.WriteLine("append: " + appendParams);
            if (dt.Rows.Count > 0)
            {
                if (dtSelectedBranches.Rows.Count > 0)
                {
                    DataRow[] results = dt.Select(appendParams);
                    string aaa = appendParams;
                    DataTable dtt = new DataTable();
                    if (results.Any())
                    {
                        dtt = results.CopyToDataTable();
                        dtt = loadMe(dtt, itemCode, status);
                        dtResult = dtt;
                    }
                    else
                    {
                        DataTable dt2 = new DataTable();
                        dtResult = dt2;
                    }
                }
                else
                {
                    dt = loadMe(dt, itemCode, status);
                    dtResult = dt;
                }
            }
            else
            {
                DataTable dt2 = new DataTable();
                dtResult = dt2;
            }
            return dtResult;
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
                     join row3 in dtOriginal.AsEnumerable() on row.Field<string>("item_code") equals row3.Field<string>("item_code")
                    into grp
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
                         OriginalNetAmount = grp.Sum(r => r.Field<double>("net_amount")),
                         NetAmount = row2.Field<double>("net_amount"),
                         Branch = row2.Field<string>("branch")
                     }).ToList();
            DataTable dt3 = new DataTable();
            dt3.Columns.Add("item_code", typeof(string));
            dt3.Columns.Add("total_net_amount_as_per_selected_branch", typeof(double));
            dt3.Columns.Add("net_amount", typeof(double));
            if (tagged.Equals("Open"))
            {
                dt3.Columns.Add("quantity_per_branch", typeof(double));
                dt3.Columns.Add("branch", typeof(string));
                dt3.Columns.Add("unit_price", typeof(double));
                dt3.Columns.Add("disc_amount", typeof(double));
                dt3.Columns.Add("gross_amount", typeof(double));
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

                            dt3.Rows.Add(q.ItemCode, (double?)null,(double?)null, (double?)null, "", (double?)null, (double?)null, (double?)null, (double?)null);
                        }

                        //dt3.Rows.Add( (double?)null, (double?)null, (double?)null, q.Quantity, q.Branch, q.UnitPrice, q.DiscPrcnt == null ? 0.00 : q.DiscPrcnt, q.DiscAmount == null ? 0.00 : q.DiscAmount, q.GrossAmount == null ? 0.00 : q.GrossAmount, q.NetAmount == null ? 0.00 : q.NetAmount);
                        double percent = (q.NetAmountPerSelected / q.OriginalNetAmount) * 100;
                        Console.WriteLine("tanginaa " + q.NetAmountPerSelected);
                        dt3.Rows.Add((double?)null, q.NetAmountPerSelected,q.NetAmount,  q.Quantity, q.Branch, q.UnitPrice, q.DiscAmount,q.GrossAmount, percent);
                    }
                }
                else
                {
                    if (!item.Equals(q.ItemCode))
                    {
                        dt3.Rows.Add(q.ItemCode, q.NetAmountPerSelected,q.OriginalNetAmount);
                    }

                }
                item = q.ItemCode;
            }
            if (tagged.Equals("Open"))
            {
                DataView view = new DataView(dt3);
                dt3 = view.ToTable(true, "item_code", "total_net_amount_as_per_selected_branch", "net_amount", "quantity_per_branch", "branch", "unit_price", "disc_amount", "gross_amount", "percentage");
            }
            return dt3;
        }
        public void loadData()
        {
            chart1.Legends.Clear();
            chart1.ChartAreas["ChartArea1"].AxisY.LabelStyle.Format = "{ 0.00} %";
            chart1.ChartAreas["ChartArea1"].AxisX.Interval = 1;
            chart1.Series["Series1"].Points.Clear();
            DataTable dtt = loadPopUpValues("Close", "");
            if (dtt.Rows.Count > 0)
            {
                double doubleTemp = 0.00;
                if (dtt.Rows.Count > 0)
                {
                    DataView dv = dtt.DefaultView;
                    dv.Sort = "total_net_amount_as_per_selected_branch DESC";
                    DataTable sortedDT = dv.ToTable();
                    double getSum = double.TryParse(dtt.Compute("SUM(total_net_amount_as_per_selected_branch)", "").ToString(), out doubleTemp) ? Convert.ToDouble(dtt.Compute("SUM(total_net_amount_as_per_selected_branch)", "")) : doubleTemp;
                    DataTable dt = new DataTable();
                    if(cmbTop.SelectedIndex > 0)
                    {
                        int topN = 0, intTemp = 0;
                        topN = Int32.TryParse(cmbTop.Text, out intTemp) ? Convert.ToInt32(cmbTop.Text) : intTemp;
                        dt = sortedDT.AsEnumerable().Take(topN).CopyToDataTable();
                    }
                    else
                    {
                        dt = sortedDT;
                    }

                    int count = 0;
                    ExceptionMessages.Clear();
                    foreach (DataRow row in dt.Rows)
                    {
                        Console.WriteLine(row["item_code"].ToString());
                        double allTotalQuantity = 0.00, percentage = 0.00;
                        allTotalQuantity = double.TryParse(row["total_net_amount_as_per_selected_branch"].ToString(), out doubleTemp) ? Convert.ToDouble(row["total_net_amount_as_per_selected_branch"].ToString()) : doubleTemp;
                        percentage = (allTotalQuantity / getSum) * 100;
                        ExceptionMessages.Add(row["item_code"].ToString(), percentage);
                        int p = chart1.Series["Series1"].Points.AddXY(row["item_code"].ToString(), percentage);
                        chart1.Series["Series1"].Points[p].ToolTip =  "Total Net Amount: " + getSum.ToString("n2") + Environment.NewLine + "Total Net Amount as Per Selected Branch: " + allTotalQuantity.ToString("n2");
                        count += 1;
                    }
                    chart1.ChartAreas["ChartArea1"].AxisX.LabelStyle.Angle = count >= 11 ? -65 : 0;
                    chart1.Titles["Title1"].Text = "Item" + Environment.NewLine +  (cmbTop.SelectedIndex <= 0 ? "All (" + count.ToString("N0") + ")" : "Top " + cmbTop.Text);
                    this.chart1.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.##} %";
                }
            }
        }
        Point? prevPosition = null;
        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {

            var pos = e.Location;
            if (prevPosition.HasValue && pos == prevPosition.Value)
            {
                return;
            }
            prevPosition = pos;
            var results = chart1.HitTest(pos.X, pos.Y, false,
                                            ChartElementType.DataPoint);
            string xValue = "", itemCode = "";
            foreach (var result in results)
            {
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    var prop = result.Object as DataPoint;
                    if (prop != null)
                    {
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        foreach (KeyValuePair<string, double> entry in ExceptionMessages)
                        {
                            if (!prop.YValues[0].Equals(entry.Value))
                            {
                                continue;
                            }
                            else
                            {
                                itemCode = entry.Key;
                            }
                        }

                    }
                }
            }
            globalItemCode = itemCode;
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            DataTable dt = loadPopUpValues("Open", globalItemCode);
            if (dt.Rows.Count > 0)
            {
                ItemSalesValueGraphDetails frm = new ItemSalesValueGraphDetails(dt);
                frm.Text = globalItemCode;
                frm.ShowDialog();
                return;
            }
        }

        private void ItemSalesReportGraph_Load(object sender, EventArgs e)
        {
            if(cmbTop.Properties.Items.Count > 0)
            {
                cmbTop.SelectedIndex = 1;
            }
        }
    }
}
