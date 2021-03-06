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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DevExpress.XtraGrid.Columns;
using System.Globalization;
using RestSharp;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
namespace AB
{
    public partial class GoodsReceipt_Details : Form
    {
        public GoodsReceipt_Details(int id, string docStatus, string selectedReference)
        {
            InitializeComponent();
            this.id = id;
            this.docStatus = docStatus;
            this.selectedReference = selectedReference;
        }
        string docStatus = "", selectedReference = "";
        int id = 0;
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        public static bool isSubmit = false;
        private void ReceiptFromProduction_Details_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            btnUpdateSAP.Visible = docStatus.Equals("O"); 
            lblReference.Text = "Reference #: " + selectedReference;
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
            gridControl1.Invoke(new Action(delegate ()
            {
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
            }));
            string sParams = id.ToString() + "?mode=receive";
            string sResult = apic.loadData("/api/production/rec_from_prod/details/", sParams, "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
            {
                JObject joResponse = JObject.Parse(sResult);
                JArray jaData = (JArray)joResponse["data"];
                DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                AutoCompleteStringCollection auto = new AutoCompleteStringCollection();

                if (IsHandleCreated)
                {
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        if (dtData.Rows.Count > 0)
                        {
                            string[] columnVisible = new string[]
                            {
                            "item_code", "quantity","uom","inv_qty", "inv_uom","whsecode",""
                            };
                            dtData.SetColumnsOrder(columnVisible);
                        }

                        gridControl1.DataSource = null;


                        //DataTable dtCloned = new DataTable(), dtFinal = new DataTable();
                        //if (dtData.Rows.Count > 0)
                        //{
                        //    dtCloned = dtData.Clone();
                        //    dtCloned.Columns["confirm"].DataType = typeof(string);
                        //    foreach (DataRow row in dtData.Rows)
                        //    {

                        //        dtCloned.ImportRow(row);
                        //    }
                        //    dtFinal = dtCloned.Clone();
                        //    foreach (DataRow row in dtCloned.Rows)
                        //    {

                        //        bool isConfirm = false, boolTemp = false;
                        //        isConfirm = bool.TryParse(row["confirm"].ToString(), out boolTemp) ? Convert.ToBoolean(row["confirm"].ToString()) : boolTemp;
                        //        row["confirm"] = isConfirm ? "✔" : "";

                        //        string encodeStatus = row["docstatus"].ToString() == "O" ? "Open" : row["docstatus"].ToString() == "C" ? "Closed" : row["docstatus"].ToString() == "N" ? "Cancelled" : "";
                        //        row["docstatus"] = encodeStatus;

                        //        dtFinal.ImportRow(row);
                        //    }
                        //}

                        gridControl1.DataSource = dtData;


                        gridView1.OptionsView.ColumnAutoWidth = false;
                        gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;

                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            string v = col.GetCaption();
                            string s = col.GetCaption().Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.ColumnEdit = fieldName.Equals("remarks") ? repositoryItemMemoEdit1 : repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("quantity") || fieldName.Equals("inv_qty") ? DevExpress.Utils.FormatType.Numeric : DevExpress.Utils.FormatType.None;

                            col.DisplayFormat.FormatString = fieldName.Equals("quantity") || fieldName.Equals("inv_qty") ? "n3" : "";

                            col.Visible = !(fieldName.Equals("id") || fieldName.Equals("objtype") || fieldName.Equals("doc_id") || fieldName.Equals("created_by") || fieldName.Equals("updated_by") || fieldName.Equals("date_created") || fieldName.Equals("date_updated") || fieldName.Equals("prod_order_row_id") || fieldName.Equals("base_row_id"));

                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }
                        gridView1.BestFitColumns();
                        var col2 = gridView1.Columns["remarks"];
                        if (col2 != null)
                        {
                            col2.Width = 200;
                        }
                        //auto complete
                        string[] suggestions = { "item_code" };
                        string suggestConcat = string.Join(";", suggestions);
                        gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                        devc.loadSuggestion(gridView1, gridControl1, suggestions);
                    }));
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void btnUpdateSAP_Click(object sender, EventArgs e)
        {
            if (docStatus.Equals("O"))
            {
                SAP_Remarks.isSubmit = false;
                SAP_Remarks frm = new SAP_Remarks();
                frm.ShowDialog();
                if (SAP_Remarks.isSubmit)
                {
                    JObject joBody = new JObject();
                    joBody.Add("sap_number", SAP_Remarks.sap_number);
                    joBody.Add("remarks", SAP_Remarks.rem);
                    string sResult = apic.loadData("/api/sap_num/receive_from_prod/update/", id.ToString(), "application/body", joBody.ToString(), Method.PUT, true);
                    if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
                    {
                        JObject jObjectResponse = JObject.Parse(sResult);
                        bool isSuccess = false, boolTemp = false;
                        isSuccess = isSubmit = jObjectResponse["success"].Type == JTokenType.Boolean ? Convert.ToBoolean(jObjectResponse["success"].ToString()) : boolTemp;
                        string msg = jObjectResponse["message"].ToString();
                        MessageBox.Show(msg, "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (isSuccess)
                        {
                            this.Hide();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Access Denied", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }
    }
}
