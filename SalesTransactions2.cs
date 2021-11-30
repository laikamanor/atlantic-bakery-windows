﻿using System;
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
    public partial class SalesTransactions2 : Form
    {
        public SalesTransactions2()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        DataTable dtBranch = new DataTable(), dtWarehouse = new DataTable(), dtCustType = new DataTable();
        private void SalesTransactions2_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            checkToDate.Checked = true;
            dtFromDate.EditValue = DateTime.Now;
            dtToDate.EditValue = DateTime.Now;
            cmbDocStatus.SelectedIndex = 1;
            loadBranch();
            loadWarehouse(cmbWhse);
            loadCustType();
            bg();
        }

        public void loadBranch()
        {
            cmbBranch.Properties.Items.Clear();
            cmbBranch.Properties.Items.Add("All");
            string sResult = apic.loadData("/api/branch/get_all", "", "", "", RestSharp.Method.GET, true);
            if (!string.IsNullOrEmpty(sResult.Trim()))
            {
                if (sResult.StartsWith("{"))
                {
                    JObject joResult = JObject.Parse(sResult);
                    JArray jaData = (JArray)joResult["data"];
                    dtBranch = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));

                    foreach (DataRow row in dtBranch.Rows)
                    {
                        cmbBranch.Properties.Items.Add(row["name"].ToString());
                    }
                    string currentBranchCode = Login.jsonResult["data"]["branch"].ToString();
                    string currentBranchName = apic.findValueInDataTable(dtBranch, currentBranchCode, "code", "name");
                    cmbBranch.SelectedIndex = cmbBranch.Properties.Items.IndexOf(currentBranchName) <= 0 ? 0 : cmbBranch.Properties.Items.IndexOf(currentBranchName);
                }
            }
            else
            {
                cmbBranch.SelectedIndex = 0;
            }
        }

        public void loadCustType()
        {
            cmbCustType.Properties.Items.Clear();
            cmbCustType.Properties.Items.Add("All");
            string sResult = apic.loadData("/api/custtype/get_all", "", "", "", RestSharp.Method.GET, true);
            if (!string.IsNullOrEmpty(sResult.Trim()))
            {
                if (sResult.StartsWith("{"))
                {
                    JObject joResult = JObject.Parse(sResult);
                    JArray jaData = (JArray)joResult["data"];
                    dtCustType = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));

                    foreach (DataRow row in dtCustType.Rows)
                    {
                        cmbCustType.Properties.Items.Add(row["name"].ToString());
                    }
                    cmbCustType.SelectedIndex = 0;
                    //string currentBranchCode = Login.jsonResult["data"]["branch"].ToString();
                    //string currentBranchName = apic.findValueInDataTable(dtBranch, currentBranchCode, "code", "name");
                    //cmbBranch.SelectedIndex = cmbBranch.Properties.Items.IndexOf(currentBranchName) <= 0 ? 0 : cmbBranch.Properties.Items.IndexOf(currentBranchName);
                }
            }
            else
            {
                cmbCustType.SelectedIndex = 0;
            }
        }



        public void loadData()
        {
            gridControl1.Invoke(new Action(delegate ()
            {
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
            }));
            bool cFromDate = false, cToDate = false, cCheckSAP = false;
            string sBranch = "?branch=", sFromDate = "&from_date=", sToDate = "&to_date=", sWhse = "&whsecode=", sDocStatus = "&docstatus=", sHaveAR = "&sap_number=", sCustType = "&cust_type=";
            checkSAP.Invoke(new Action(delegate ()
            {
                cCheckSAP = checkSAP.Checked;
                sHaveAR += (cCheckSAP ? "1" : "");
            }));
            cmbDocStatus.Invoke(new Action(delegate ()
            {
                string decodeStatus = cmbDocStatus.Text.Equals("All") && cmbDocStatus.SelectedIndex == 0 ? "" : cmbDocStatus.Text.Equals("Open") && cmbDocStatus.SelectedIndex == 1 ? "O" : cmbDocStatus.Text.Equals("Closed") && cmbDocStatus.SelectedIndex == 2 ? "C" : cmbDocStatus.Text.Equals("Cancelled") && cmbDocStatus.SelectedIndex == 3 ? "N" : "";
                sDocStatus += decodeStatus;
            }));
            checkFromDate.Invoke(new Action(delegate ()
            {
                cFromDate = checkFromDate.Checked;
            }));
            checkToDate.Invoke(new Action(delegate ()
            {
                cToDate = checkToDate.Checked;
            }));
            cmbCustType.Invoke(new Action(delegate ()
            {
                string custTypeID = apic.findValueInDataTable(dtCustType, cmbCustType.Text, "name", "id");
                sCustType += custTypeID;
            }));
            cmbBranch.Invoke(new Action(delegate ()
            {
                string branchCode = apic.findValueInDataTable(dtBranch, cmbBranch.Text, "name", "code");
                sBranch += branchCode;
            }));
            dtFromDate.Invoke(new Action(delegate ()
            {
                sFromDate += cFromDate ? dtFromDate.Text : "";
            }));
            dtToDate.Invoke(new Action(delegate ()
            {
                sToDate += cToDate ? dtToDate.Text : "";
            }));
            cmbWhse.Invoke(new Action(delegate ()
            {
                sWhse += apic.findValueInDataTable(dtWarehouse, cmbWhse.Text, "whsename", "whsecode");
            }));
            string sParams = sBranch + sFromDate + sToDate + sWhse + sDocStatus + sHaveAR + sCustType;
            string sResult = apic.loadData("/api/sales/get_all", sParams, "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
            {
                JObject joResponse = JObject.Parse(sResult);
                JArray jaData = (JArray)joResponse["data"];
                DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                if (IsHandleCreated)
                {
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        if (dtData.Rows.Count > 0)
                        {
                            string[] columnVisible = new string[]
{
                            "transdate", "reference","cust_code","doctotal","transtype", "docstatus","sap_number","sap_date_updated"
};
                            dtData.SetColumnsOrder(columnVisible);
                        }
                        gridControl1.DataSource = null;

                        DataTable dtCloned = dtData.Clone();
                        if(dtCloned.Columns.Contains("transdate") || dtCloned.Columns.Contains("sap_date_updated"))
                        {
                            dtCloned.Columns["transdate"].DataType = dtCloned.Columns["sap_date_updated"].DataType = typeof(DateTime);
                        }else if (dtCloned.Columns.Contains("sap_number"))
                        {
                            dtCloned.Columns["sap_number"].DataType = typeof(string);
                        }
                        foreach (DataRow row in dtData.Rows)
                        {
                            dtCloned.ImportRow(row);
                        }

                        gridControl1.DataSource = dtCloned;

                        gridView1.OptionsView.ColumnAutoWidth = false;
                        gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            string v = col.GetCaption();
                            string s = col.GetCaption().Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.Caption = fieldName.Equals("whsecode") ? "From Whse" : col.Caption;
                            col.ColumnEdit = repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("doctotal") ? DevExpress.Utils.FormatType.Numeric : fieldName.Equals("transdate") || fieldName.Equals("sap_date_updated") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.Custom;
                            col.DisplayFormat.FormatString = fieldName.Equals("doctotal") ? "n2" : fieldName.Equals("transdate") || fieldName.Equals("sap_date_updated")  ? "yyyy-MM-dd HH:mm:ss" : "";
                            col.Visible = fieldName.Equals("transdate") || fieldName.Equals("reference") || fieldName.Equals("cust_code") || fieldName.Equals("doctotal") || fieldName.Equals("transtype") || fieldName.Equals("docstatus") || fieldName.Equals("sap_number") || fieldName.Equals("sap_date_updated");

                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }

                        //auto complete
                        string[] suggestions = {"cust_code", "reference" };
                        string suggestConcat = string.Join(";", suggestions);
                        gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                        devc.loadSuggestion(gridView1, gridControl1, suggestions);
                        gridView1.BestFitColumns();
                        //var col2 = gridView1.Columns["remarks"];
                        //if (col2 != null)
                        //{
                        //    col2.Width = 200;
                        //}
                    }));
                }
            }
        }

        public void loadWarehouse(DevExpress.XtraEditors.ComboBoxEdit cmb)
        {
            Console.WriteLine("dept count: " + dtBranch.Rows.Count + "/" + cmb.Name);
            string branchCode = cmb.Name.Equals("cmbToWhse") ? "" : apic.findValueInDataTable(dtBranch, cmbBranch.Text, "name", "code");
            cmb.Properties.Items.Clear();
            cmb.Properties.Items.Add("All");
            string sResult = apic.loadData("/api/whse/get_all", "?branch=" + branchCode, "", "", RestSharp.Method.GET, true);
            if (!string.IsNullOrEmpty(sResult.Trim()))
            {
                if (sResult.StartsWith("{"))
                {
                    JObject joResult = JObject.Parse(sResult);
                    JArray jaData = (JArray)joResult["data"];
                    DataTable dt = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
                    if (cmb.Name.Equals("cmbToWhse"))
                    {
                        dtWarehouse = dt;
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        cmb.Properties.Items.Add(row["whsename"].ToString());
                    }
                    string plantCode = Login.jsonResult["data"]["whse"].ToString();
                    string plantName = cmb.Name.Equals("cmbToWhse") ? "" : apic.findValueInDataTable(dt, plantCode, "whsecode", "whsename");
                    cmb.SelectedIndex = cmb.Properties.Items.IndexOf(plantName) <= 0 ? 0 : cmb.Properties.Items.IndexOf(plantName);
                }
            }
            else
            {
                cmb.SelectedIndex = 0;
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

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private int hotTrackRow = DevExpress.XtraGrid.GridControl.InvalidRowHandle;

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

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void checkFromDate_CheckedChanged(object sender, EventArgs e)
        {
            dtFromDate.Visible = checkFromDate.Checked;
        }

        private void repositoryItemTextEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            int id = 0, baseID = 0, intTemp = 0;
            id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
            if (selectedColumnfieldName.Equals("reference"))
            {
                SalesTransactions_Items2 frm = new SalesTransactions_Items2(id);
                frm.ShowDialog();
            }
        }

        private void checkToDate_CheckedChanged(object sender, EventArgs e)
        {
            dtToDate.Visible = checkToDate.Checked;
        }

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
    }
}
