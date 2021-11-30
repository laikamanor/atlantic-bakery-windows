using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AB.API_Class.SOA;
using DevExpress.XtraGrid.Columns;
using System.Globalization;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;

namespace AB
{
    public partial class SOA_Details : Form
    {
        public SOA_Details()
        {
            InitializeComponent();
        }
        soa_class soac = new soa_class();
        DataTable dtForSOA = new DataTable();
        public int selectedID = 0;
        private void SOA_Details_Load(object sender, EventArgs e)
        {
            loadData();
        }

        public async void loadData()
        {
            dtForSOA = await Task.Run(() => soac.getSOADetails(selectedID));
            foreach (DataRow row in dtForSOA.Rows)
            {
                //dgv.Rows.Add(row["base_transdate"].ToString(), row["base_reference"].ToString(), row["sales_remarks"].ToString(), row["amount"].ToString());
                lblReference.Text = row["reference"].ToString();
                lblCustomerCode.Text = row["cust_code"].ToString();
                lblDateTransaction.Text = row["transdate"].ToString();
                lblTotalAmount.Text = row["total_amount"].ToString();
            }
            //dgv.Columns["amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridControl1.DataSource = null;
            gridControl1.DataSource = dtForSOA;
            foreach (GridColumn col in gridView1.Columns)
            {

                string fieldName = col.FieldName;
                string v = col.GetCaption();
                string s = col.GetCaption().Replace("_", " ");
                col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                col.ColumnEdit = repositoryItemTextEdit1;
                col.DisplayFormat.FormatType = fieldName.Equals("amount") ? DevExpress.Utils.FormatType.Numeric : fieldName.Equals("transdate") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;
                col.DisplayFormat.FormatString = fieldName.Equals("amount")  ? "n2" : fieldName.Equals("transdate") ? "yyyy-MM-dd HH:mm:ss" : "";
                col.Visible = fieldName.Equals("transdate") || fieldName.Equals("base_reference") || fieldName.Equals("amount") || fieldName.Equals("sales_remarks");

                //fonts
                FontFamily fontArial = new FontFamily("Arial");
                col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
            }
            gridView1.BestFitColumns();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            printSOA frm = new printSOA();
            frm.dtResult = dtForSOA;
            frm.ShowDialog();
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

        private void gridView1_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            if (e.RowHandle == HotTrackRow)
                e.Appearance.BackColor = gridView1.PaintAppearance.SelectedRow.BackColor;
            else
                e.Appearance.BackColor = e.Appearance.BackColor;
        }
    }
}
