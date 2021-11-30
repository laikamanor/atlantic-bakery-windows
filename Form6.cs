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
    public partial class Form6 : Form
    {
        public Form6()
        {
            InitializeComponent();
        }

        private void Form6_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("item_code", typeof(string));
            dt.Columns.Add("uom", typeof(string));
            dt.Columns.Add("prod_min_qty", typeof(double));
            dt.Columns.Add("target_for_del", typeof(double));
            dt.Rows.Add("MAALAT @ 10", 0,5);
            dt.Rows.Add("MAALAT @ 10", 0,10);

            var query = (from row in dt.AsEnumerable()
                         group row by new
                         {
                             ItemCode = row.Field<string>("item_code"),
                             Uom = row.Field<string>("uom"),
                         } into grp
                         select new
                         {
                             ItemCode = grp.Key.ItemCode,
                             Uom = grp.Key.Uom,
                             ProdMinQty = grp.AsEnumerable().Where(r =>  r.Field<dynamic>("prod_min_qty") > 0).First().Field<dynamic>("prod_min_qty"),
                             TargetForDel = grp.Sum(r=> r.Field<double>("target_for_del"))
                         }).Distinct().ToList();

            foreach (var q in query)
            {
                MessageBox.Show(q.ItemCode + "/" + q.Uom + "/" + q.ProdMinQty + "/" + q.TargetForDel);
            }
        }
    }
}

