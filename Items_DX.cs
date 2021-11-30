using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AB.API_Class.Items;
using DevExpress.XtraGrid.Views.Grid;
using System.Globalization;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using AB.UI_Class;
using Newtonsoft.Json.Linq;

namespace AB
{
    public partial class Items_DX : Form
    {
        public Items_DX()
        {
            InitializeComponent();
        }
        item_class itemc = new item_class();
        devexpress_class devc = new devexpress_class();
        private void Items_DX_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            loadData();
        }

        public void loadData()
        {
            gridControl1.DataSource = null;
            gridView1.Columns.Clear();

            int isActive = chckActiveItem.Checked ? 1 : 0;

            DataTable dt = itemc.loadData(isActive);
            if (dt.Rows.Count > 0)
            {
                dt.Columns.Add("edit");
                dt.Columns.Add("change_status");
            }
            gridControl1.DataSource = dt;
            gridView1.OptionsView.ColumnAutoWidth = false;
            foreach (GridColumn col in gridView1.Columns)
            {
                string fieldName = col.FieldName;
                string v = col.GetCaption();
                string s = col.GetCaption().Replace("_", " ");
                col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                col.ColumnEdit = fieldName.Equals("edit") ? repositoryItemButtonEdit1 : fieldName.Equals("change_status") ? repositoryItemButtonEdit2 : repositoryItemTextEdit1;
                col.DisplayFormat.FormatType = fieldName.Equals("quantity") ? DevExpress.Utils.FormatType.Numeric : DevExpress.Utils.FormatType.None;
                col.DisplayFormat.FormatString = fieldName.Equals("quantity") ? "n2" : "";
                col.Visible = (fieldName.Equals("item_code") || fieldName.Equals("item_name") || fieldName.Equals("item_group") || fieldName.Equals("uom") || fieldName.Equals("edit") || fieldName.Equals("change_status"));

                //fonts
                //FontFamily fontArial = new FontFamily("Arial");
                //col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                //col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                devc.changeFont(col);
            }
            gridView1.BestFitColumns();
            //auto complete
            string[] suggestions = { "item_code" };
            devc.loadSuggestion(gridView1, gridControl1, suggestions);
            gridView1.OptionsView.ColumnAutoWidth = false;
            gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
        }


        private void btnAddItem_Click(object sender, EventArgs e)
        {
            AddItem addItem = new AddItem();
            addItem.ShowDialog();
            if (AddItem.isSubmit)
            {
                loadData();
            }
        }

        private void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            int id = 0, intTemp = 0;
            id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
            if (selectedColumnfieldName.Equals("edit"))
            {
                EditItem.isSubmit = false;
                EditItem frm = new EditItem(id);
                frm.ShowDialog();
                if (EditItem.isSubmit)
                {
                    loadData();
                }
            }
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

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.RowHandle == HotTrackRow)
                e.Appearance.BackColor = gridView1.PaintAppearance.SelectedRow.BackColor;
            else
                e.Appearance.BackColor = e.Appearance.BackColor;
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            loadData();
        }

        private void repositoryItemButtonEdit2_Click(object sender, EventArgs e)
        {
            try
            {
                string sItemCode = gridView1.GetFocusedRowCellValue("item_code").ToString();

                string toggleInActive = chckActiveItem.Checked ? "in active" : "active";

                DialogResult dialogResult = MessageBox.Show("Are you sure you want to " + toggleInActive  +"'" + sItemCode + "'? ", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    api_class apic = new api_class();
                    string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
                    int id = 0, intTemp = 0;
                    id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
                    if (selectedColumnfieldName.Equals("change_status"))
                    {
                        JObject joBody = new JObject();
                        joBody.Add("is_active", !chckActiveItem.Checked);
                        Console.WriteLine(joBody);
                        string sResult = apic.loadData("/api/item/update/", id.ToString(), "application/json", joBody.ToString(), RestSharp.Method.PUT, true);
                        if (!string.IsNullOrEmpty(sResult.Trim()))
                        {
                            if (sResult.StartsWith("{"))
                            {
                                JObject joResult = JObject.Parse(sResult);
                                bool isSuccess = (bool)joResult["success"];
                                string msg = joResult["message"].ToString();
                                MessageBox.Show(msg, isSuccess ? "Message" : "Validation", MessageBoxButtons.OK, isSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                                if (isSuccess)
                                {
                                    loadData();
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void chckActiveItem_CheckedChanged(object sender, EventArgs e)
        {
            loadData();
        }
    }
}
