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
using DevExpress.Utils.Menu;
namespace AB
{
    public partial class PendingItemTransferRequest : Form
    {
        public PendingItemTransferRequest()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        DataTable dtWarehouse = new DataTable();
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void PendingItemTransferRequest_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            loadWarehouse();
            bg();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
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
            try
            {
                gridControl1.Invoke(new Action(delegate ()
                {
                    gridControl1.DataSource = null;
                    gridView1.Columns.Clear();
                }));
                string sToWhse = "?to_whse=";
                cmbWhse.Invoke(new Action(delegate ()
                {
                    sToWhse += apic.findValueInDataTable(dtWarehouse, cmbWhse.Text, "whsename", "whsecode");
                }));


                string sParams = sToWhse;
                string sResult = apic.loadData("/api/forecast/get_for_delivery", sParams, "", "", Method.GET, true);
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
                            "transdate", "reference","delivery_date","prod_whse","to_whse","remarks"
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
                                col.DisplayFormat.FormatType = fieldName.Equals("transdate") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;

                                col.DisplayFormat.FormatString = fieldName.Equals("transdate") ? "yyyy-MM-dd HH:mm:ss" : "";

                                col.Visible = !(fieldName.Equals("id") || fieldName.Equals("series") || fieldName.Equals("seriescode") || fieldName.Equals("objtype") || fieldName.Equals("transnumber") || fieldName.Equals("docstatus") || fieldName.Equals("remarks2") || fieldName.Equals("created_by") || fieldName.Equals("updated_by") || fieldName.Equals("canceled_by") || fieldName.Equals("date_canceled") || fieldName.Equals("date_updated") || fieldName.Equals("confirmed_by") || fieldName.Equals("date_confirmed") || fieldName.Equals("confirm") || fieldName.Equals("hashed_id") || fieldName.Equals("date_created") || fieldName.Equals("shift") || fieldName.Equals("delivery_status") || fieldName.Equals("sap_number") || fieldName.Equals("date_closed") || fieldName.Equals("closed_by"));

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
                            string[] suggestions = { "reference" };
                            string suggestConcat = string.Join(";", suggestions);
                            gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                            devc.loadSuggestion(gridView1, gridControl1, suggestions);
                        }));
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
                int id = 0, baseID = 0,objtype=0, intTemp = 0;
                id = gridView1.GetFocusedRowCellValue("id") == null ? 0 : int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;

                objtype = gridView1.GetFocusedRowCellValue("objtype") == null ? 0 : int.TryParse(gridView1.GetFocusedRowCellValue("objtype").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("objtype").ToString()) : intTemp;

                string currentRef = gridView1.GetFocusedRowCellValue("reference").ToString();
                if (selectedColumnfieldName.Equals("reference"))
                {
                    PendingItemTransferRequest_Details.isSubmit = false;
                    PendingItemTransferRequest_Details frm = new PendingItemTransferRequest_Details(id,objtype, currentRef);
                    frm.ShowDialog();
                    if (PendingItemTransferRequest_Details.isSubmit)
                    {
                        bg();
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void loadWarehouse()
        {
            try
            {
                cmbWhse.Invoke(new Action(delegate ()
                {
                    cmbWhse.Properties.Items.Clear();
                }));
                string sResult = "";
                sResult = apic.loadData("/api/whse/get_all", "", "", "", Method.GET, true);
                if (sResult.Substring(0, 1).Equals("{"))
                {
                    //DataTable dtData = apic.getDtDownloadResources(sResult, "data");
                    //string sBranch = apic.getFirstRowDownloadResources(dtData, "data");

                    dtWarehouse = apic.getDtDownloadResources(sResult, "data");
                    if (IsHandleCreated)
                    {
                        cmbWhse.Invoke(new Action(delegate ()
                        {
                            cmbWhse.Properties.Items.Add("All");
                        }));
                    }


                    foreach (DataRow row in dtWarehouse.Rows)
                    {
                        if (IsHandleCreated)
                        {
                            cmbWhse.Invoke(new Action(delegate ()
                            {
                                cmbWhse.Properties.Items.Add(row["whsename"].ToString());
                            }));
                        }

                    }
                    if (IsHandleCreated)
                    {
                        cmbWhse.Invoke(new Action(delegate ()
                        {
                            string branch = (string)Login.jsonResult["data"]["whse"];
                            string s = apic.findValueInDataTable(dtWarehouse, branch, "whsecode", "whsename");
                            cmbWhse.SelectedIndex = cmbWhse.Properties.Items.IndexOf(s);
                        }));
                    }
                }
                else
                {
                    apic.showCustomMsgBox("Validation", sResult);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg();
        }
    }
}
