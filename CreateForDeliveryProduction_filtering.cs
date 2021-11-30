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
    public partial class CreateForDeliveryProduction_filtering : Form
    {
        public CreateForDeliveryProduction_filtering(DataTable dt, string title, string[] currentSelectedNames, DataTable dtItemGroup)
        {
            InitializeComponent();
            this.dt = dt;
            this.title = title;
            this.currentSelectedNames= currentSelectedNames;
            this.dtItemGroup = dtItemGroup;
        }
        DataTable dt = new DataTable(), dtItemGroup = new DataTable();
        string title = "";
        public static string[] selectedNames;
        string[] currentSelectedNames;
        public static int count = 0;
        bool draggable = false;
        int mouseX = 0, mouseY = 0;
        api_class apic = new api_class();
        bool isSubmit = false;
        private void CreateForDeliveryProduction_filtering_Load(object sender, EventArgs e)
        {
            lblTitle.Text = title;
            label8.Visible = cmbItemGroup.Visible = title.Equals("Item");
            if (title.Equals("Item"))
            {
                loadItemGroup();
            }
            loadData();
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
                if(dtItemGroup.Rows.Count > 0)
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

        public void selectNames()
        {
            //int counter = 0;
            //foreach (DataRow row in dt.Rows)
            //{
            //    string rName = "";
            //    if (dt.Columns.Contains("name"))
            //    {
            //        rName = row["name"].ToString();
            //        if (currentSelectedNames != null)
            //        {
            //            if (currentSelectedNames.Count() > 0)
            //            {
            //                foreach (string name in currentSelectedNames)
            //                {
            //                    if (name.Replace("'","").Equals(rName))
            //                    {
            //                        gridView1.SelectRow(counter);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    counter++;
            //}
            for (int i = 0; i < gridView1.RowCount; i++)
            {
                string rName = gridView1.GetRowCellValue(i, "name").ToString();
                if (currentSelectedNames != null)
                {
                    if (currentSelectedNames.Count() > 0)
                    {
                        foreach (string name in currentSelectedNames)
                        {
                            if (name.Replace("'", "").Equals(rName))
                            {
                                gridView1.SelectRow(i);
                            }
                        }
                    }
                }
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
                    int counter = 0;
                    if(dt.Rows.Count > 0)
                    {
                        if (!dt.Columns.Contains("row_index"))
                        {
                            dt.Columns.Add("row_index", typeof(int));
                        }
                    }
                  
                    foreach(DataRow row in dt.Rows)
                    {
                        row["row_index"] = counter;
                        counter++;
                    }

                    if (!dt.Columns.Contains("isSelected"))
                    {
                        dt.Columns.Add("isSelected", typeof(Boolean));
                    }
                    DataTable dtTemp = dt;
                    if (title.Equals("Item"))
                    {
                        
                        if (cmbItemGroup.SelectedIndex > 0)
                        {
                            string s = cmbItemGroup.Text.ToString().Replace(@"'", "''");
                            DataRow[] rows = dtTemp.Select("item_group='" + s + "'");
                            if (rows.Length > 0)
                            {
                                dtTemp = rows.CopyToDataTable();
                            }
                            else
                            {
                                dtTemp = new DataTable();
                            }
                        }
                    }
                    gridControl1.DataSource = dtTemp;
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
                        col.Visible = !(fieldName.Equals("isSelected")|| fieldName.Equals("item_group") || fieldName.Equals("row_index"));
                        col.Width = 200;
                    }
                    gridView1.ClearSelection();
                    selectNames();
                }));
            }
            catch(Exception ex)
            {
               
                MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CreateForDeliveryProduction_filtering_FormClosing(object sender, FormClosingEventArgs e)
        {
            //get all selected names
            try
            {
                DataRow[] rows = dt.Select("isSelected=true");
                if (rows.Any())
                {
                    selectedNames = new string[rows.Count()];
                    int counter = 0;
                    foreach (DataRow row in rows)
                    {
                        string name = row["name"].ToString();
                        Console.WriteLine("item " + name);
                        string finalName = "'" + name.Replace(@"'", "''") + "'";
                        selectedNames[counter] = finalName;
                        counter++;
                    }
                }
                else
                {
                    selectedNames = new string[0];
                    selectedNames = null;

                }
                //int[] ids = gridView1.GetSelectedRows();
                //if (ids != null)
                //{
                //    int intTemp = 0;
                //    selectedNames = new string[ids.Count()];
                //    int counter = 0;
                //    foreach (int id in ids)
                //    {
                //        string name =  gridView1.GetRowCellValue(id, "name").ToString();
                //        string finalName = "'" + name.Replace(@"'", "''") + "'";
                //        selectedNames[counter] = finalName;
                //        counter++;
                //    }
                //}
                apic.showCustomMsgBox("Message", title + " filtering succeeded!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CreateForDeliveryProduction_filtering_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                draggable = true;
                mouseX = Cursor.Position.X - this.Left;
                mouseY = Cursor.Position.Y - this.Top;
            }
            catch (Exception ex)
            {
                apic.showCustomMsgBox(ex.Message, ex.ToString());
            }
        }

        private void CreateForDeliveryProduction_filtering_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (draggable)
                {
                    this.Top = Cursor.Position.Y - mouseY;
                    this.Left = Cursor.Position.X - mouseX;
                }
            }
            catch (Exception ex)
            {
                apic.showCustomMsgBox(ex.Message, ex.ToString());
            }
        }

        private void CreateForDeliveryProduction_filtering_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                draggable = false;
            }
            catch (Exception ex)
            {
                apic.showCustomMsgBox(ex.Message, ex.ToString());
            }
        }

        private void gridView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            try
            {
                int i = gridView1.FocusedRowHandle;
                int rowIndex = 0, intTemp = 0;
                rowIndex = int.TryParse(gridView1.GetRowCellValue(i, "row_index").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetRowCellValue(i, "row_index").ToString()) : intTemp;
                dt.Rows[rowIndex]["isSelected"] = gridView1.IsRowSelected(i);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cmbItemGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadData();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
