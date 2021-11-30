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
using DevExpress.XtraGrid;
using System;
using System.ComponentModel;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid;
using System.Globalization;
using AB.UI_Class;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace AB
{
    public partial class Customers2 : Form
    {
        public Customers2()
        {
            InitializeComponent();
        }
        utility_class utilityc = new utility_class();
        devexpress_class devc = new devexpress_class();
        api_class apic = new api_class();
        private void Customers2_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            bg();
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


        public void loadData()
        {
            string sResult = apic.loadData("/api/customer/get_all", "", "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
            {
                JObject joResponse = JObject.Parse(sResult);
                JArray jaData = (JArray)joResponse["data"];
                DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                if (dtData.Rows.Count > 0)
                {
                    dtData.Columns.Add("edit");
                }
                gridControl1.Invoke(new MethodInvoker(delegate
                {
                    gridControl1.DataSource = dtData;
                    gridView1.OptionsView.ColumnAutoWidth = false;
                    foreach (GridColumn col in gridView1.Columns)
                    {
                        string fieldName = col.FieldName;
                        string v = col.GetCaption();
                        string s = col.GetCaption().Replace("_", " ");
                        col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                        col.ColumnEdit = fieldName.Equals("edit") ? repositoryItemButtonEdit1 : fieldName.Equals("remove") ? repositoryItemButtonEdit2 : repositoryItemTextEdit1;
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.None;
                        col.DisplayFormat.FormatString = "";
                        col.Visible = !(fieldName.Equals("id") || fieldName.Equals("created_by") || fieldName.Equals("updated_by") || fieldName.Equals("is_active") || fieldName.Equals("date_created") || fieldName.Equals("date_updated") || fieldName.Equals("balance") || fieldName.Equals("dep_balance") || fieldName.Equals("is_confidential") || fieldName.Equals("cust_type"));

                        //fonts
                        //FontFamily fontArial = new FontFamily("Arial");
                        //col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                        //col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        devc.changeFont(col);
                    }
                    gridView1.BestFitColumns();
                    var col2 = gridView1.Columns["address"];
                    if (col2 != null)
                    {
                        col2.Width = 200;
                    }
                    //auto complete
                    string[] suggestions = { "code" };
                    devc.loadSuggestion(gridView1, gridControl1, suggestions);
                    gridView1.OptionsView.ColumnAutoWidth = false;
                    gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                }));
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            int id = 0, custType = 0, intTemp = 0;
            id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
            custType = int.TryParse(gridView1.GetFocusedRowCellValue("cust_type").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("cust_type").ToString()) : intTemp;
            string custCode = gridView1.GetFocusedRowCellValue("code").ToString() == null ? "" : gridView1.GetFocusedRowCellValue("code").ToString().ToString();
            string custName = gridView1.GetFocusedRowCellValue("name").ToString() == null ? "" : gridView1.GetFocusedRowCellValue("name").ToString().ToString();
            if (selectedColumnfieldName.Equals("edit"))
            {
                editCustomer.isSubmit = false;
                editCustomer add = new editCustomer();
                add.lblID.Text = id.ToString();
                add.custType = custType;
                add.txtCustCode.Text = custCode;
                add.txtCustName.Text = custName;
                add.ShowDialog();
                if (editCustomer.isSubmit)
                {
                    bg();
                }
            }
        }

        private void btnAddCustomer_Click(object sender, EventArgs e)
        {
            AddCustomer addCustomer = new AddCustomer("Add");
            addCustomer.ShowDialog();
            if (AddCustomer.isSubmit)
            {
                bg();
            }
        }
    }
}
