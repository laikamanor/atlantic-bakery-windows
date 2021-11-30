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
using Newtonsoft.Json.Linq;
using RestSharp;
using Newtonsoft.Json;
using DevExpress.XtraGrid.Columns;
using System.Globalization;
using System.Web;
using System.Threading;
using System.IO;
using AB.UI_Class;
using DevExpress.XtraGrid.Views.Grid;

namespace AB
{
    public partial class Inventory2 : Form
    {
        public Inventory2()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        utility_class utilityc = new utility_class();
        DataTable dtBranches = new DataTable(), dtWarehouses = new DataTable();
        AutoResetEvent doneEvent = new AutoResetEvent(false);
        private void btnSelectMultipleItem_Click(object sender, EventArgs e)
        {
            if (gridView1.Columns["item_code"] != null)
            {
                gridView1.ShowFilterPopup(gridView1.Columns["item_code"]);
            }
        }

        private void Inventory2_Load(object sender, EventArgs e)
        {
            dtFromDate.EditValue = DateTime.Now;
            dtToDate.EditValue = DateTime.Now;
            loadBranches();
            loadWarehouse(cmbBranch.Text);
            loadItemGroup();
            bg(backgroundWorkerLoadData);
        }


        public void loadData(string branchCode, string whseCode, string itemGroup, string fromDate, string toDate)
        {
            string sURL = "/api/inv/warehouse/report?branch=" + branchCode + "&from_date=" + fromDate + "&to_date=" + toDate + "&whse=" + whseCode + "&item_group=" + itemGroup;
            string sResult = apic.loadData(sURL, "", "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
            {
                JObject joResponse = JObject.Parse(sResult);
                JArray jaData = (JArray)joResponse["data"];
                DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                if (IsHandleCreated)
                {
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        gridControl1.DataSource = null;
                        gridControl1.DataSource = dtData;
                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            Console.WriteLine(fieldName);
                            string v = col.GetCaption();
                            string s = col.GetCaption().Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.Caption = v.Equals("Adj In") ? "Adjustment In" : v.Equals("Adj Out") ? "Adjustment Out" : v.Equals("Transferred") ? "Transfer Out" : v.Equals("In Transit") ? v + " (For Received)" : v.Equals("Out Transit") ? v + " (Out For Delivery)" : col.GetCaption();
                            col.ColumnEdit = repositoryItemTextEdit1;

                            col.DisplayFormat.FormatType = fieldName.Equals("Item Code") ? DevExpress.Utils.FormatType.None : DevExpress.Utils.FormatType.Numeric;
                            col.DisplayFormat.FormatString = v.Equals("Item Code") ? "" : "n2";

                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                            //col.AppearanceHeader.BackColor = Color.FromArgb(0, 102, 153);
                            //col.AppearanceHeader.ForeColor = Color.White;
                            col.MinWidth = 150;
                            
                            col.Fixed = fieldName.Equals("item_code") ? FixedStyle.Left : FixedStyle.None;
                            //col.AppearanceCell.BackColor = v.Equals("Beginning") || v.Equals("Total In") || v.Equals("Total Out") || v.Equals("Available") ? Color.FromArgb(230, 225, 90) : col.GetCaption().Equals("In Transit (For Received)") || v.Equals("Receipt From Prod") || v.Equals("Received") || v.Equals("Transfer In") || col.GetCaption().Equals("Adjustment In") ? Color.FromArgb(255, 255, 128) : col.GetCaption().Equals("Out Transit (Out For Delivery)") || col.GetCaption().Equals("Adjustment Out") || col.GetCaption().Equals("Transfer Out") || v.Equals("Pull Out") || v.Equals("Issue For Prod") || v.Equals("Sold") ? Color.FromArgb(192, 255, 192) : col.AppearanceCell.BackColor;
                            gridView1.Columns.ColumnByFieldName("TotalIn").VisibleIndex = gridView1.Columns.ColumnByFieldName("AdjIn").VisibleIndex + 1;
                            gridView1.Columns.ColumnByFieldName("OutTransit").VisibleIndex = gridView1.Columns.ColumnByFieldName("AdjOut").VisibleIndex - 1;
                            gridView1.Columns.ColumnByFieldName("IssueForProd").VisibleIndex = gridView1.Columns.ColumnByFieldName("Transferred").VisibleIndex + 1;

                        }

  

                        //for (int i = 0; i < gridView1.DataRowCount; i++)
                        //{
                        //    double value = !Convert.IsDBNull(gridView1.GetRowCellValue(e.RowHandle, "Available")) ? Convert.ToDouble(gridView1.GetRowCellValue(e.RowHandle, "Available")) : 0.00;
                        //    if (gridView1.GetRowCellValue(i, "Available").ToString() == "A")
                        //    {
                        //        gridView1. = value <= 0 ? Color.FromArgb(230, 225, 90) : value == 0 ? Color.FromArgb(230, 90, 102) : Color.FromArgb(230, 225, 90);
                        //    }
                        //}
                        var colItemCode = gridView1.Columns["item_code"];
                        if (colItemCode != null)
                        {
                            gridView1.Columns["item_code"].Summary.Clear();
                            gridView1.Columns["item_code"].Summary.Add(DevExpress.Data.SummaryItemType.Count, "item_code", "Count: {0:N0}");
                        }

                        gridView1.BestFitColumns();
                    }));
                }
            }
        }

        public void bg(BackgroundWorker bgw)
        {
            if (!bgw.IsBusy)
            {
                closeForm();
                Loading frm = new Loading();
                frm.Show();
                bgw.RunWorkerAsync();
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
                        //DataTable dtData = apic.getDtDownloadResources(sResult, "data");
                        //string sBranch = apic.getFirstRowDownloadResources(dtData, "data");

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
                                cmbBranch.SelectedIndex = cmbBranch.Properties.Items.IndexOf(s);
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
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void loadWarehouse(string branch)
        {
            try
            {
                cmbWarehouse.Invoke(new Action(delegate ()
                {
                    cmbWarehouse.Properties.Items.Clear();
                }));
                string sBranchCode = apic.findValueInDataTable(dtBranches, branch, "name", "code");
                string sResult = "";
                sResult = apic.loadData("/api/whse/get_all", "?branch=" + branch, "", "", Method.GET, true);
                if (sResult.Substring(0, 1).Equals("{"))
                {
                    dtWarehouses =  apic.getDtDownloadResources(sResult, "data");
                }
                if (dtWarehouses.Rows.Count > 1)
                {
                    cmbWarehouse.Invoke(new Action(delegate ()
                    {
                        cmbWarehouse.Properties.Items.Add("All");
                    }));
                }
                foreach (DataRow row in dtWarehouses.Rows)
                {
                    cmbWarehouse.Invoke(new Action(delegate ()
                    {
                        cmbWarehouse.Properties.Items.Add(row["whsename"].ToString());
                    }));
                }
                cmbWarehouse.Invoke(new Action(delegate ()
                {
                    string whse = (string)Login.jsonResult["data"]["whse"];
                    string s = apic.findValueInDataTable(dtWarehouses, whse, "whsecode", "whsename");

                    cmbWarehouse.SelectedIndex = cmbBranch.Text.Equals("All") || string.IsNullOrEmpty(s.Trim()) ? 0 : cmbWarehouse.Properties.Items.IndexOf(s);
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void loadItemGroup()
        {
            try
            {
                cmbItemGroup.Invoke(new Action(delegate ()
                {
                    cmbItemGroup.Properties.Items.Clear();
                }));
                string sResult = apic.loadData("/api/item/item_grp/getall", "", "", "", Method.GET, true);
                if (sResult.Substring(0, 1).Equals("{"))
                {
                    JObject joResponse = JObject.Parse(sResult);
                    JArray jaData = (JArray)joResponse["data"];
                    DataTable dtItemGroup = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                    if (dtItemGroup.Rows.Count > 0)
                    {
                        cmbItemGroup.Invoke(new Action(delegate ()
                        {
                            cmbItemGroup.Properties.Items.Add("All");
                        }));
                    }
                    foreach (DataRow row in dtItemGroup.Rows)
                    {
                        cmbItemGroup.Invoke(new Action(delegate ()
                        {
                            cmbItemGroup.Properties.Items.Add(row["code"].ToString());
                        }));
                    }
                    cmbItemGroup.Invoke(new Action(delegate ()
                    {
                        cmbItemGroup.SelectedIndex = 0;
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void backgroundWorkerCmbBranch_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
             
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                doneEvent.Set();
            }
        }

        private void backgroundWorkerCmbWarehouse_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                cmbBranch.Invoke(new Action(delegate ()
                {
                    string branchCode = apic.findValueInDataTable(dtBranches, cmbBranch.Text, "name", "code");
                    loadWarehouse(branchCode);
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                doneEvent.Set();
            }
        }

        private void backgroundWorkerCmbWarehouse_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void cmbBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            bg(backgroundWorkerCmbWarehouse);
        }

        private void backgroundWorkerCmbItemGroup_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //loadItemGroup();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                doneEvent.Set();
            }
        }

        private void backgroundWorkerCmbItemGroup_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void backgroundWorkerLoadData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        public string delegateControl(Control c,string s)
        {
            string result = "";
            c.Invoke(new Action(delegate ()
            {
                result = s;
            }));
            return result;
        }

        private void backgroundWorkerLoadData_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string sBranch = delegateControl(cmbBranch, cmbBranch.Text), sWhse = delegateControl(cmbWarehouse, cmbWarehouse.Text), sFromDate = delegateControl(dtFromDate, dtFromDate.Text), sToDate = delegateControl(dtToDate, dtToDate.Text), sItemGroup = delegateControl(cmbItemGroup, cmbItemGroup.Text);
                string branchCode = apic.findValueInDataTable(dtBranches, sBranch, "name", "code");
                string whseCode = apic.findValueInDataTable(dtWarehouses, sWhse, "whsename", "whsecode");
                sItemGroup = sItemGroup.Equals("All") ? "" : sItemGroup;

                loadData(branchCode, whseCode, sItemGroup, sFromDate, sToDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                doneEvent.Set();
            }
        }



        private void btnrefresh_Click(object sender, EventArgs e)
        {
            bg(backgroundWorkerLoadData);
        }

        public List<string> listCodes()
        {
            string[] input = {
          "in_transit",
                "receive_from_prod",
                "receive",
                "transfer_in",
                "adjustment_in",
                "out_transit",
                "transfer_out",
                "adjustment_out",
                "issue_for_prod",
                "pullout",
                "sales",
            };

            List<string> result = new List<string>(input);
            return result;
        }

        public List<string> listSelectedColumns()
        {
            string[] input = {
                "In Transit (For Received)",
                "Receipt From Prod",
                "Received",
                "Transfer In",
                "Adjustment In",
                "Out Transit (Out For Delivery)",
                "Transfer Out",
                "Adjustment Out",
                "Issue For Prod",
                "Pull Out",
                "Sold"
            };

            List<string> result = new List<string>(input);
            return result;
        }

        public DataTable listDetails()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("code", typeof(string));
            dt.Columns.Add("caption", typeof(string));
            try
            {
                for (int i = 0; i < listCodes().Count; i++)
                {
                    dt.Rows.Add(listCodes()[i], listSelectedColumns()[i]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dt;
        }

        private async void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnText = gridView1.FocusedColumn.GetCaption();
            string code = gridView1.GetFocusedRowCellValue("item_code").ToString();
            foreach (DataRow row in listDetails().Rows)
            {
                if (selectedColumnText == row["caption"].ToString())
                {
                    loadDetails(row["code"].ToString(), code,selectedColumnText);
                    break;
                }
            }
        }

        public void loadDetails(string objType, string itemCode, string columnCaption)
        {

            Console.WriteLine("objtype " + objType);
            string sBranch = apic.findValueInDataTable(dtBranches, cmbBranch.Text, "name", "code"),
                sWhse = apic.findValueInDataTable(dtWarehouses, cmbWarehouse.Text, "whsename", "whsecode");
            string sURL = "?branch=" + sBranch + "&from_date=" + dtFromDate.Text + "&to_date=" + dtToDate.Text + "&whsecode=" + sWhse + "&transaction=" + objType + "&item_code=" + (itemCode.Contains("+") ? HttpUtility.UrlEncode(itemCode) : itemCode);

            string sResult = apic.loadData("/api/inv/whse/detailed/transaction", sURL, "", "", Method.GET, true);
            //Console.WriteLine(sResult);
            if (sResult.Trim().StartsWith("{"))
            {
                JObject joResult = JObject.Parse(sResult);
                DataTable dtInvDetails = (DataTable)JsonConvert.DeserializeObject((string)joResult["data"].ToString(), (typeof(DataTable)));

                //"Receive From Prod",
                //    "Receive",
                //    "Transfer In",
                //    "Adj In",

                int id = columnCaption.Equals("In Transit (For Received)") || columnCaption.Equals("Out Transit (Out For Delivery)") ? 2 : columnCaption.Equals("Receive From Prod") || columnCaption.Equals("Receive") || columnCaption.Equals("Transfer In") || columnCaption.Equals("Adjustment In") ? 5 : 13;

                InventoryDetails frm = new InventoryDetails(dtInvDetails, id);
                frm.Text = columnCaption;
                frm.ShowDialog();
            }
            else
            {
                MessageBox.Show(sResult, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg(backgroundWorkerLoadData);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bg(backgroundWorkerLoadData);
        }

        private void gridView1_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            //col.AppearanceCell.BackColor = v.Equals("Beginning") || v.Equals("Total In") || v.Equals("Total Out") || v.Equals("Available") ? Color.FromArgb(230, 225, 90) : col.GetCaption().Equals("In Transit (For Received)") || v.Equals("Receipt From Prod") || v.Equals("Received") || v.Equals("Transfer In") || col.GetCaption().Equals("Adjustment In") ? Color.FromArgb(255, 255, 128) : col.GetCaption().Equals("Out Transit (Out For Delivery)") || col.GetCaption().Equals("Adjustment Out") || col.GetCaption().Equals("Transfer Out") || v.Equals("Pull Out") || v.Equals("Issue For Prod") || v.Equals("Sold") ? Color.FromArgb(192, 255, 192) : col.AppearanceCell.BackColor;
            string fieldName = e.Column.FieldName;
            if (fieldName.Equals("Beginning") || fieldName.Equals("TotalIn") || fieldName.Equals("TotalOut") || fieldName.Equals("Available"))
            {
                e.Appearance.BackColor = Color.FromArgb(230, 225, 90);
            }else if(fieldName.Equals("InTransit") || fieldName.Equals("ReceiptFromProd") || fieldName.Equals("Received") || fieldName.Equals("TransferIn") || fieldName.Equals("AdjIn"))
            {
                e.Appearance.BackColor = Color.FromArgb(255, 255, 128);
            }else if(fieldName.Equals("OutTransit") || fieldName.Equals("AdjOut") || fieldName.Equals("Transferred") || fieldName.Equals("PullOut") || fieldName.Equals("IssueForProd") || fieldName.Equals("Sold"))
            {
                e.Appearance.BackColor = Color.FromArgb(192, 255, 192);
            }
            var rows = gridView1.GetSelectedCells();
            if (rows.Length > 0)
            {
                foreach (var row in rows)
                {
                   if(row.Column == e.Column && row.RowHandle == e.RowHandle)
                    {
                        e.Appearance.BackColor = Color.FromArgb(226, 234, 253);
                    }
                }
            }

            //if(e.Column.Equals(""))
            //GridView currentView = sender as GridView;
            //if (e.Column.FieldName == "Available")
            //{
            //    double value = !Convert.IsDBNull(currentView.GetRowCellValue(e.RowHandle, "Available")) ? Convert.ToDouble(currentView.GetRowCellValue(e.RowHandle, "Available")) : 0.00;
            //    Console.WriteLine("value " + value);
            //    e.Appearance.BackColor = value <=0 ? Color.FromArgb(230, 225, 90) : value == 0 ? Color.FromArgb(230, 90, 102) : Color.FromArgb(230, 225, 90);
            //}
        }

        private void gridView1_RowStyle(object sender, RowStyleEventArgs e)
        {
            //GridView currentView = sender as GridView;
            //if (!gridView1.IsValidRowHandle(e.RowHandle))
            //{
            //    return;
            //}
            //else
            //{
            //    if (Convert.ToInt32(view.GetRowCellValue(e.RowHandle, currentView.Columns["Number"])) > 100)
            //    {
            //        e.Appearance.BackColor = Color.Red;
            //    }
            //    if(e.col)
            //}
        }

        private void gridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
          
        }

        private void backgroundWorkerCmbBranch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }
    }
}
