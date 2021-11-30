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
    public partial class ItemSalesValueGraphDetails : Form
    {
        public ItemSalesValueGraphDetails(DataTable dt)
        {
            dtGlobal = dt;
            InitializeComponent();
        }
        DataTable dtGlobal = new DataTable();
        private void ItemSalesReportGraphDetails_Load(object sender, EventArgs e)
        {
            cmbTop.SelectedIndex = 1;


            loadData();
        }

        public void loadData()
        {

            chart1.Legends.Clear();
            chart1.ChartAreas["ChartArea1"].AxisY.LabelStyle.Format = "{ 0.00} %";
            chart1.ChartAreas["ChartArea1"].AxisX.Interval = 1;

            chart1.Series["Series1"].Points.Clear();
            chart1.ChartAreas[0].RecalculateAxesScale();

            DataView dv = dtGlobal.DefaultView;
            dv.Sort = "quantity_per_branch DESC";
            DataTable sortedDT = dv.ToTable();

            DataTable dt = new DataTable();
            if (cmbTop.SelectedIndex > 0)
            {
                int topN = 0, intTemp = 0;
                topN = Int32.TryParse(cmbTop.Text, out intTemp) ? Convert.ToInt32(cmbTop.Text) : intTemp;
                dt = sortedDT.AsEnumerable().Take(topN).CopyToDataTable();
            }
            else
            {
                dt = sortedDT;
            }

            DataRow row1 = dtGlobal.Rows[0];
            double quantityPerSelectedBranch = 0.00, doubleTemp = 0.00;
            quantityPerSelectedBranch = double.TryParse(dtGlobal.Compute("SUM(net_amount)","").ToString(), out doubleTemp) ? Convert.ToDouble(dtGlobal.Compute("SUM(net_amount)", "").ToString()) : doubleTemp;
            int counter = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (row["branch"].ToString().Trim() != "")
                {
                    double quantityPerBranch = 0.00, result = 0.00;
                    quantityPerBranch = double.TryParse(row["net_amount"].ToString(), out doubleTemp) ? Convert.ToDouble(row["net_amount"].ToString()) : doubleTemp;
                    result = (quantityPerBranch / quantityPerSelectedBranch) * 100;
                    //double percent = (q.NetAmount / q.NetAmountPerSelected) * 100;
                    int p = chart1.Series["Series1"].Points.AddXY(row["branch"].ToString(), result);
                    chart1.Series["Series1"].Points[p].ToolTip = "Total Net Amount as Per Selected Branch: " + quantityPerSelectedBranch.ToString("n2") + Environment.NewLine + "Net Amount Per Branch: " + quantityPerBranch.ToString("n2");
                    counter += 1;
                }
            }
            this.chart1.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.##} %";
            chart1.ChartAreas["ChartArea1"].AxisX.LabelStyle.Angle = counter >= 11 ? -65 : 0;
            chart1.Titles["Title1"].Text = "Branch" + Environment.NewLine + (cmbTop.SelectedIndex <= 0 ? "All (" + counter.ToString("N0") + ")" : "Top " + cmbTop.Text);
        }

        private void cmbTop_SelectedIndexChanged(object sender, EventArgs e)
        {

            loadData();
        }
    }
}
