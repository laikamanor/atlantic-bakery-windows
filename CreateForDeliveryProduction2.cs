using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AB
{
    public partial class CreateForDeliveryProduction2 : Form
    {
        public CreateForDeliveryProduction2(DataTable dtGlobal)
        {
            InitializeComponent();
            this.dtGlobal = dtGlobal;
        }
        DataTable dtGlobal = new DataTable();
        private void CreateForDeliveryProduction2_Load(object sender, EventArgs e)
        {
            this.MinimumSize = new Size(643, 480);
            this.Icon = Properties.Resources.abc_logo;
            gridControl1.DataSource = loadForDeliveryQuery();
        }

        public DataTable loadForDeliveryQuery()
        {
            var query = (from row in dtGlobal.AsEnumerable()
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

            var queryy = (from row in dtGlobal.AsEnumerable()
                          join row2 in dt1.AsEnumerable() on row.Field<string>("item_code") equals row2.Field<string>("item_code")
                          into grp
                          select new
                          {
                              //Id = row.Field<int>("id") == null ? 0 : row.Field<int>("id"),
                              ItemCode = row.Field<string>("item_code") == null ? "" : row.Field<string>("item_code"),
                              TotalSold = grp.Sum(r => r.Field<double?>("total_sold_as_per_selected_branch") == null ? 0 : r.Field<double>("total_sold_as_per_selected_branch")),
                              Branch = row.Field<string>("branch") == null ? "" : row.Field<string>("branch"),
                              Sold = row.Field<double?>("sold") == null ? 0 : row.Field<double>("sold"),
                              DateDiff = row.Field<Int64?>("datediff") == null ? 0 : row.Field<Int64>("datediff"),
                              Average = row.Field<double?>("average") == null ? 0.00 : row.Field<double>("average"),
                              LastBal = row.Field<double?>("last_bal") == null ? 0 : row.Field<double>("last_bal"),
                              TargetForDel = row.Field<double?>("target_for_del") == null ? 0 : row.Field<double>("target_for_del"),
                              ProdMinQty = row.Field<dynamic>("prod_min_qty") == null ? 0.00 : row.Field<dynamic>("prod_min_qty")
                              //FinalForDelivery = row.Field<dynamic>("final_for_delivery") == null ? (double?)null : row.Field<dynamic>("final_for_delivery")
                          }).Distinct().ToList();

            DataTable dt = new DataTable();
            //dt.Columns.Add("id", typeof(int));
            dt.Columns.Add("item_code", typeof(string));
            dt.Columns.Add("total_sold", typeof(double));
            dt.Columns.Add("branch", typeof(string));
            //dt.Columns.Add("branch", typeof(string));

            dt.Columns.Add("sold_quantity", typeof(double));
            dt.Columns.Add("days", typeof(double));
            dt.Columns.Add("average_per_day", typeof(double));
            dt.Columns.Add("last_bal_qty", typeof(double));
            dt.Columns.Add("target_for_delivery", typeof(double));
            //dt.Columns.Add("final_for_delivery", typeof(double));

            string item = "";
            foreach (var j in queryy)
            {
                if (!item.Equals(j.ItemCode))
                {
                    dt.Rows.Add( j.ItemCode, j.TotalSold, "", (double?)null, (Int64?)null, (Int64?)null, (double?)null, (double?)null);
                }
                dt.Rows.Add( "", (double?)null, j.Branch, j.Sold, j.DateDiff, j.Average, j.LastBal, j.TargetForDel);
                item = j.ItemCode;
            }
            DataView view = new DataView(dt);
            return view.ToTable(true, "item_code", "total_sold", "branch", "sold_quantity", "days", "average_per_day", "last_bal_qty", "target_for_delivery");
        }
    }
}
