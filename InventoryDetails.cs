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

namespace AB
{
    public partial class InventoryDetails : Form
    {
        public InventoryDetails(DataTable dt, int id)
        {
            gDt = dt;
            selectedID = id;
            InitializeComponent();
        }
        DataTable gDt = new DataTable();
        int selectedID = 0;
        private void InventoryDetails_Load(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        public void loadData()
        {
            try
            {
                gridControl1.Invoke(new Action(delegate ()
                {
                    gridControl1.DataSource = gDt;
                    //gridView1.OptionsSelection.MultiSelectMode = gDt.Rows.Count > 0 ? GridMultiSelectMode.CheckBoxRowSelect : GridMultiSelectMode.RowSelect;
                    gridView1.OptionsSelection.MultiSelect = gDt.Rows.Count > 0 ? true : false;

                    foreach (GridColumn col in gridView1.Columns)
                    {
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        col.DisplayFormat.FormatString = "n2";
                        string s = col.GetCaption().Replace("_", " ");
                        col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());

                        col.Caption = (selectedID == 2 || selectedID == 8) && col.GetCaption() == "Warehouse" ? "From Warehouse" : selectedID <= 5 && col.GetCaption() == "Warehouse" ? "To Warehouse" : selectedID <= 13 && col.GetCaption() == "Warehouse" ? "From Warehouse" : col.Caption;

                        col.Caption = (selectedID == 2 || selectedID == 8) && col.GetCaption() == "Warehouse2" ? "To Warehouse" : selectedID <= 5 && col.GetCaption() == "Warehouse2" ? "From Warehouse" : selectedID <= 13 && col.GetCaption() == "Warehouse2" ? "To Warehouse" : col.Caption;


                        switch (col.Caption)
                        {
                            case "Cust Code":
                                col.Caption = "Cust. Code";
                                col.Visible = this.Text.Equals("Sold") ? true : false;
                                break;
                            case "Discprcnt":
                                col.Caption = "Disc. %";
                                col.Visible = this.Text.Equals("Sold") ? true : false;
                                break;
                            case "Unit Price":
                                col.Visible = this.Text.Equals("Sold") ? true : false;
                                break;
                            case "Disc Amount":
                                col.Caption = "Disc. Amount";
                                col.Visible = this.Text.Equals("Sold") ? true : false;
                                break;
                            case "Net Amount":
                                col.Visible = this.Text.Equals("Sold") ? true : false;
                                break;
                            case "Trans Id":
                                col.Visible = false;
                                break;
                            case "From Warehouse":
                                //col.Visible = this.Text.Equals("Sold") ? true : false;
                                col.Caption = this.Text.Equals("Sold") ? "Warehouse" : col.Caption;
                                break;
                            case "Username":
                                col.Caption = "Processed By";
                                break;
                            case "To Warehouse":
                                col.Visible = this.Text.Equals("Sold") ? false : true;
                                break;
                            case "Sap Number":
                                col.DisplayFormat.FormatString = "";
                                break;
                            default:
                                col.Visible = true;
                                break;
                        }
                        if (col.Caption.Equals("Is Branch To Branch") || col.Caption.Equals("Inter Whse")) col.Visible = false;
                        col.Width = col.Caption == "Reference" || col.Caption == "Transdate" || col.Caption == "Cust. Code" || col.Caption.Equals("Base Ref") ? 150 : 100;

                        (gridControl1.MainView as GridView).Columns[col.AbsoluteIndex].ColumnEdit = repositoryItemTextEdit1;

                        //fonts
                        FontFamily fontArial = new FontFamily("Arial");
                        col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                        col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);

                    }
                    if (this.Text == "Sold" && gridView1.Columns.Count > 0)
                    {
                        gridView1.Columns.ColumnByFieldName("cust_code").VisibleIndex = gridView1.Columns.ColumnByFieldName("item_code").VisibleIndex;

                        gridView1.Columns.ColumnByFieldName("item_code").VisibleIndex = gridView1.Columns.ColumnByFieldName("quantity").VisibleIndex;

                        gridView1.Columns.ColumnByFieldName("unit_price").VisibleIndex = gridView1.Columns.ColumnByFieldName("warehouse").VisibleIndex;

                        gridView1.Columns.ColumnByFieldName("discprcnt").VisibleIndex = gridView1.Columns.ColumnByFieldName("warehouse").VisibleIndex;

                        gridView1.Columns.ColumnByFieldName("disc_amount").VisibleIndex = gridView1.Columns.ColumnByFieldName("warehouse").VisibleIndex;

                        gridView1.Columns.ColumnByFieldName("net_amount").VisibleIndex = gridView1.Columns.ColumnByFieldName("warehouse").VisibleIndex;
                        lblDiscAmount.Visible = true;
                        lblNetAmount.Visible = true;
                    }
                    else
                    {

                        if (gridView1.Columns.Count > 0)
                        {
                            gridView1.Columns.ColumnByFieldName("warehouse2").VisibleIndex = selectedID == 2 ? gridView1.Columns.ColumnByFieldName("warehouse2").VisibleIndex : selectedID <= 5 ? gridView1.Columns.ColumnByFieldName("quantity").VisibleIndex + 1 : gridView1.Columns.ColumnByFieldName("warehouse2").VisibleIndex;
                        }

                        lblDiscAmount.Visible = false;
                        lblNetAmount.Visible = false;
                    }
                    gridView1.BestFitColumns();
                }));
            }
            catch (Exception ex)
            {

            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void gridView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            int[] array1;
            double quantity = 0.00, discAmount = 0.00, netAmount = 0.00, doubleTemp = 0.00;
            array1 = gridView1.GetSelectedRows();
            foreach (int a in array1)
            {
                quantity += double.TryParse(gridView1.GetRowCellValue(a, "quantity").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(a, "quantity").ToString()) : doubleTemp;
                discAmount += double.TryParse(gridView1.GetRowCellValue(a, "disc_amount").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(a, "disc_amount").ToString()) : doubleTemp;
                netAmount += double.TryParse(gridView1.GetRowCellValue(a, "net_amount").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(a, "net_amount").ToString()) : doubleTemp;
            }
            lblTotalQuantity.Text = "Total Quantity: " + quantity.ToString("n2");
            lblDiscAmount.Text = "Total Disc. Amount: " + discAmount.ToString("n2");
            lblNetAmount.Text = "Total Net Amount: " + netAmount.ToString("n2");
        }

        private void gridView1_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (e.RowHandle >= 0)
            {
                double doubleTemp = 0.00;
                double discAmt = double.TryParse(gridView1.GetRowCellValue(e.RowHandle, "disc_amount").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetRowCellValue(e.RowHandle, "disc_amount").ToString()) : doubleTemp;
                if (discAmt > 0)
                {
                    e.Appearance.BackColor = Color.Yellow;
                }
            }
        }

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView currentView = sender as GridView;
            if (e.Column.FieldName == "warehouse" || e.Column.FieldName == "warehouse2")
            {
                bool boolTemp = false;
                bool isBranchToBranch = bool.TryParse(gridView1.GetRowCellValue(e.RowHandle, "is_branch_to_branch").ToString(), out boolTemp) ? Convert.ToBoolean(gridView1.GetRowCellValue(e.RowHandle, "is_branch_to_branch").ToString()) : boolTemp;
                bool interWhse = bool.TryParse(gridView1.GetRowCellValue(e.RowHandle, "inter_whse").ToString(), out boolTemp) ? Convert.ToBoolean(gridView1.GetRowCellValue(e.RowHandle, "inter_whse").ToString()) : boolTemp;
                if (isBranchToBranch)
                {
                    e.Appearance.BackColor = Color.FromArgb(115, 255, 110);
                }
                else if (interWhse)
                {
                    e.Appearance.BackColor = Color.FromArgb(255, 173, 110);
                }
            }
        }
    }
}
