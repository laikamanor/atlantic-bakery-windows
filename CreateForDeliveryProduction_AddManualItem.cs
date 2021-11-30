using DevExpress.XtraGrid.Columns;
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
using AB.UI_Class;
namespace AB
{
    public partial class CreateForDeliveryProduction_AddManualItem : Form
    {
        public CreateForDeliveryProduction_AddManualItem(DataTable dt, DataTable dtItemGroup)
        {
            InitializeComponent();
            this.dt = dt;
            dt.Columns.Add("isSelected", typeof(Boolean));
            this.dtItemGroup = dtItemGroup;
        }
        devexpress_class devc = new devexpress_class();
        DataTable dt = new DataTable(), dtItemGroup = new DataTable();
        public static string[] selectedItems;
        public static int count = 0;
        string[] currentSelectedNames;
        private void CreateForDeliveryProduction_AddManualItem_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            loadItemGroup();
            loadData();
            
        }

        public void selectItems()
        {
            int counter = 0;
            foreach (DataRow row in dt.Rows)
            {
                string rName = "";
                if (dt.Columns.Contains("item_code"))
                {
                    rName = row["item_code"].ToString();
                    if (currentSelectedNames != null)
                    {
                        if (currentSelectedNames.Count() > 0)
                        {
                            foreach (string name in currentSelectedNames)
                            {
                                if (name.Replace("'", "").Equals(rName))
                                {
                                    gridView1.SelectRow(counter);
                                }
                            }
                        }
                    }
                }
                counter++;
            }
        }

        public void loadData()
        {
            try
            {
                gridControl1.Invoke(new Action(delegate ()
                {
                    gridControl1.DataSource = null;
                    count = dt.Rows.Count;
                    gridControl1.DataSource = dt;
                    gridView1.OptionsView.ColumnAutoWidth = false;
                    gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                    gridView1.OptionsSelection.CheckBoxSelectorField = "isSelected";
                    foreach (GridColumn col in gridView1.Columns)
                    {
                        string fieldName = col.FieldName;
                        string v = col.GetCaption();
                        string s = v.Replace("_", " ");
                        col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                        col.ColumnEdit = repositoryItemMemoEdit1;
                        //fonts
                        FontFamily fontArial = new FontFamily("Arial");
                        col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                        col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        col.Visible = !(fieldName.Equals("isSelected"));
                        col.Width = 200;
                    }
                    //auto complete
                    string[] suggestions = { "item_code" };
                    string suggestConcat = string.Join(";", suggestions);
                    gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                    devc.loadSuggestion(gridView1, gridControl1, suggestions);
                    selectItems();
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void gridView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            int i = gridView1.FocusedRowHandle;
            dt.Rows[i]["isSelected"] = gridView1.IsRowSelected(i);

        }

        private void CreateForDeliveryProduction_AddManualItem_FormClosing(object sender, FormClosingEventArgs e)
        {
            //get all selected names
            try
            {
                //DataRow[] rows = dt.Select("isSelected=true");
                //if (rows.Any())
                //{
                //    selectedItems = new string[rows.Count()];
                //    int counter = 0;
                //    foreach (DataRow row in rows)
                //    {
                //        Console.WriteLine("what it is " + row["item_code"].ToString());
                //        string name = row["item_code"].ToString();
                //        string finalName = name.Replace(@"'", "''");
                //        selectedItems[counter] = finalName;
                //        counter++;
                //    }
                //}
                int[] ids = gridView1.GetSelectedRows();
                if (ids != null)
                {
                    int intTemp = 0;
                    selectedItems = new string[ids.Count()];
                    int counter = 0;
                    foreach (int id in ids)
                    {
                        string name = gridView1.GetRowCellValue(id, "item_code").ToString();
                        //string finalName = name.Replace(@"'", "''");
                        selectedItems[counter] = name;
                        counter++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void loadItemGroup()
        {
            try
            {
                cmbItemGroup.Invoke(new Action(delegate ()
                {
                    cmbItemGroup.Properties.Items.Clear();
                    cmbItemGroup.Properties.Items.Add("All");
                }));
                if (dtItemGroup.Rows.Count > 0)
                {
                    foreach (DataRow row in dtItemGroup.Rows)
                    {
                        if (IsHandleCreated)
                        {
                            cmbItemGroup.Invoke(new Action(delegate ()
                            {
                                cmbItemGroup.Properties.Items.Add(row["code"].ToString());
                            }));
                        }
                    }
                    if (IsHandleCreated)
                    {
                        cmbItemGroup.Invoke(new Action(delegate ()
                        {
                            cmbItemGroup.SelectedIndex = 0;
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
