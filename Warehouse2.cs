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
using RestSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;

namespace AB
{
    public partial class Warehouse2 : Form
    {
        public Warehouse2()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        ui_class uic = new ui_class();
        DataTable dtBranches = new DataTable();
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void Warehouse2_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            loadBranches();
            bg();
        }

        public void loadBranches()
        {
            try
            {
                cmbBranch.Invoke(new Action(delegate ()
                {
                    cmbBranch.Properties.Items.Clear();
                }));
                if (apic.haveAccess())
                {
                    string sResult = "";
                    sResult = apic.loadData("/api/branch/get_all", "", "", "", Method.GET, true);
                    if (sResult.Substring(0, 1).Equals("{"))
                    {
                        dtBranches = apic.getDtDownloadResources(sResult, "data");
                        if (IsHandleCreated)
                        {
                            cmbBranch.Invoke(new Action(delegate ()
                            {
                                cmbBranch.Properties.Items.Add("All");
                            }));
                        }
                        foreach (DataRow row in dtBranches.Rows)
                        {
                            if (IsHandleCreated)
                            {
                                cmbBranch.Invoke(new Action(delegate ()
                                {
                                    cmbBranch.Properties.Items.Add(row["name"].ToString());
                                }));
                            }

                        }
                        if (IsHandleCreated)
                        {
                            cmbBranch.Invoke(new Action(delegate ()
                            {
                                string branch = (string)Login.jsonResult["data"]["branch"];
                                string s = apic.findValueInDataTable(dtBranches, branch, "code", "name");
                                cmbBranch.SelectedIndex = cmbBranch.Properties.Items.IndexOf(s) <= 0 ? 0 : cmbBranch.Properties.Items.IndexOf(s);
                            }));
                        }
                    }
                    else
                    {
                        apic.showCustomMsgBox("Validation", sResult);
                    }
                }
                else
                {
                    if (IsHandleCreated)
                    {
                        cmbBranch.Invoke(new Action(delegate ()
                        {
                            cmbBranch.Properties.Items.Add(Login.jsonResult["data"]["branch"]);
                            cmbBranch.SelectedIndex = 0;
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void loadData()
        {
            string sBranch = "?branch=" + apic.findValueInDataTable(dtBranches, uic.delegateControl(cmbBranch), "name", "code");
            string sParams = sBranch;
            string sResult = apic.loadData("/api/whse/get_all",sParams, "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult.Trim()))
            {
                if (sResult.Substring(0, 1).Equals("{"))
                {
                    JObject joResponse = JObject.Parse(sResult);
                    JArray jaData = (JArray)joResponse["data"];
                    DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                    if (dtData.Rows.Count > 0)
                    {
                        dtData.Columns.Add("edit_pricelist");
                        dtData.Columns.Add("edit");
                    }
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        gridControl1.DataSource = null;
                        gridControl1.DataSource = dtData;
                        gridView1.OptionsView.ColumnAutoWidth = false;
                        gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            string v = col.GetCaption();
                            string s = v.Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.ColumnEdit = fieldName.Equals("edit_pricelist") ? repositoryItemButtonEdit1 : fieldName.Equals("edit") ? repositoryItemButtonEdit2 :  repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("allowed_discount") ? DevExpress.Utils.FormatType.Numeric :  DevExpress.Utils.FormatType.None;
                            col.DisplayFormat.FormatString = fieldName.Equals("allowed_discount") ? "n2" : "";
                            col.Visible = !(fieldName.Equals("pricelist_id") || fieldName.Equals("id") || fieldName.Equals("date_created") || fieldName.Equals("date_updated") || fieldName.Equals("created_by") || fieldName.Equals("updated_by") || fieldName.Equals("agent_account") || fieldName.Equals("production_whse") || fieldName.Equals("raw_wheat_whse") || fieldName.Equals("igoods_whse") || fieldName.Equals("pack_and_oth_whse") || fieldName.Equals("premix_whse") || fieldName.Equals("cutoff") || fieldName.Equals("is_active") || fieldName.Equals("is_fg") || fieldName.Equals("is_production") || fieldName.Equals("is_igoods") || fieldName.Equals("is_raw_mat") || fieldName.Equals("is_pack_oth") || fieldName.Equals("is_premix") || fieldName.Equals("is_main"));

                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }
                        gridView1.BestFitColumns();
                    }));
                }
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

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            int id=0, pricelistID = 0, intTemp = 0;
            pricelistID = int.TryParse(gridView1.GetFocusedRowCellValue("pricelist_id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("pricelist_id").ToString()) : intTemp;
            string priceList = gridView1.GetFocusedRowCellValue("pricelist").ToString() == null ? "" : gridView1.GetFocusedRowCellValue("pricelist").ToString().ToString();
            if (selectedColumnfieldName.Equals("edit_pricelist"))
            {
                Pricelist_Row2 row = new Pricelist_Row2(pricelistID,priceList);
                row.ShowDialog();
                bg();
            }
        }

        private void repositoryItemButtonEdit2_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            int id = 0, pricelistID = 0, intTemp = 0;
            id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
            if (selectedColumnfieldName.Equals("edit"))
            {
                EditWarehouse.isSubmit = false;
                EditWarehouse frm = new EditWarehouse();
                frm.selectedID = id;
                frm.ShowDialog();
                if (EditWarehouse.isSubmit)
                {
                    bg();
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
        private void gridView1_MouseMove(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(new Point(e.X, e.Y));

            if (info.InRowCell)
                HotTrackRow = info.RowHandle;
            else
                HotTrackRow = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        }

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.RowHandle == HotTrackRow)
                e.Appearance.BackColor = gridView1.PaintAppearance.SelectedRow.BackColor;
            else
                e.Appearance.BackColor = e.Appearance.BackColor;
        }
    }
}
