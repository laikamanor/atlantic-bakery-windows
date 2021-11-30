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
    public partial class CashVariance3 : Form
    {
        public CashVariance3(string tabName)
        {
            InitializeComponent();
            this.tabName = tabName;
        }
        string tabName = "";
        DataTable dtBranch = new DataTable();
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        private void CashVariance3_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            dtTransdate.EditValue = DateTime.Now;
            loadBranch();
            loadData();
            bg();
        }

        public void loadData()
        {
            gridControl1.Invoke(new Action(delegate ()
            {
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
            }));
            bool cTransdate = false;
            string sBranch = "?branch=", sTransdate = "&transdate=";
            string sNumber = tabName.Equals("For SAP") ? "&ar_number=&ip_number=" : "";
            string sDocStatus = tabName.Equals("Done") ? "&docstatus=C" : "";
            checkFromDate.Invoke(new Action(delegate ()
            {
                cTransdate = checkFromDate.Checked;
            }));
            cmbBranch.Invoke(new Action(delegate ()
            {
                string branchCode = apic.findValueInDataTable(dtBranch, cmbBranch.Text, "name", "code");
                sBranch += branchCode;
            }));
            dtTransdate.Invoke(new Action(delegate ()
            {
                sTransdate += cTransdate ? dtTransdate.Text : "";
            }));
            string sParams = sBranch + sNumber + sDocStatus;
            string sResult = apic.loadData("/api/cashvar/get_all", sParams, "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
            {
                JObject joResponse = JObject.Parse(sResult);
                JArray jaData = (JArray)joResponse["data"];
                DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                if(dtData.Rows.Count > 0 && tabName.Equals("For SAP"))
                {
                    dtData.Columns.Add("update_sap_number");
                }
                if (IsHandleCreated)
                {
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        if (dtData.Rows.Count > 0)
                        {
                            string[] columnVisible = new string[]
{
                            "transdate", "reference","branch", "system_cash", "actual_cash", "variance","ar_number","ip_number","update_sap_number"
};
                            dtData.SetColumnsOrder(columnVisible);
                        }
                        gridControl1.DataSource = null;
                        gridControl1.DataSource = dtData;

                        gridView1.OptionsView.ColumnAutoWidth = false;
                        gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            string v = col.GetCaption();
                            string s = col.GetCaption().Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.ColumnEdit = fieldName.Equals("update_sap_number") ? repositoryItemButtonEdit1 : repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("system_cash") || fieldName.Equals("actual_cash") || fieldName.Equals("variance") ? DevExpress.Utils.FormatType.Numeric : fieldName.Equals("transdate") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;
                            col.DisplayFormat.FormatString = fieldName.Equals("system_cash") || fieldName.Equals("actual_cash") || fieldName.Equals("variance") ? "n2" : fieldName.Equals("transdate") ? "yyyy-MM-dd HH:mm:ss" : "";
                            col.Visible = fieldName.Equals("transdate") || fieldName.Equals("reference") || fieldName.Equals("branch") || fieldName.Equals("system_cash") || fieldName.Equals("actual_cash") || fieldName.Equals("variance") || fieldName.Equals("ar_number") || fieldName.Equals("ip_number") || fieldName.Equals("update_sap_number");

                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }

                        //auto complete
                        string[] suggestions = { "reference" };
                        string suggestConcat = string.Join(";", suggestions);
                        gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                        devc.loadSuggestion(gridView1, gridControl1, suggestions);
                        gridView1.BestFitColumns();
                        var col2 = gridView1.Columns["remarks"];
                        if (col2 != null)
                        {
                            col2.Width = 200;
                        }
                    }));
                }
            }
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

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
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
            //GridView view = sender as GridView;
            //GridHitInfo info = view.CalcHitInfo(new Point(e.X, e.Y));

            //if (info.InRowCell)
            //    HotTrackRow = info.RowHandle;
            //else
            //    HotTrackRow = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        }

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            //if (e.RowHandle == HotTrackRow)
            //    e.Appearance.BackColor = gridView1.PaintAppearance.SelectedRow.BackColor;
            //else
            //    e.Appearance.BackColor = e.Appearance.BackColor;
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
            dtTransdate.Visible = checkFromDate.Checked;
        }

        private void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            int id = 0, baseID = 0, intTemp = 0;
            double doubleTemp = 0.00;
            id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
            if (selectedColumnfieldName.Equals("update_sap_number"))
            {
                double variance = gridView1.GetFocusedRowCellValue("variance") == null ? doubleTemp : double.TryParse(gridView1.GetFocusedRowCellValue("variance").ToString(), out doubleTemp) ? Convert.ToDouble(gridView1.GetFocusedRowCellValue("variance").ToString()) : doubleTemp;
                IPRemarks ipRemarks = new IPRemarks();
                ipRemarks.label2.Text = variance < 0 ? "*AR #: " : "*IP #: ";
                ipRemarks.ShowDialog();
                if (IPRemarks.isSubmit)
                {
                    JObject body = new JObject();
                    body.Add(variance < 0 ? "ar_number" : "ip_number", IPRemarks.sap_number);
                    body.Add("remarks", string.IsNullOrEmpty(IPRemarks.rem) ? null : IPRemarks.rem);
                    string URL = "/api/sap_num/cashvar/update/" + id.ToString();
                    apiPUT(body, URL);
                }
            }
        }

        public void apiPUT(JObject body, string URL)
        {
            utility_class utilityc = new utility_class();
            if (Login.jsonResult != null)
            {
                string token = "";
                foreach (var x in Login.jsonResult)
                {
                    if (x.Key.Equals("token"))
                    {
                        token = x.Value.ToString();
                    }
                }
                if (!token.Equals(""))
                {
                    var client = new RestClient(utilityc.URL);
                    client.Timeout = -1;
                    var request = new RestRequest(URL);
                    Console.WriteLine(URL);
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.Method = Method.PUT;
                    request.AddParameter("application/json", body, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    if (response.ErrorMessage == null)
                    {
                        bool boolTemp = false;
                        JObject jObjectResponse = JObject.Parse(response.Content);
                        bool isSubmit = jObjectResponse["success"].IsNullOrEmpty() ? false : bool.TryParse(jObjectResponse["success"].ToString(), out boolTemp) ? Convert.ToBoolean(jObjectResponse["success"].ToString()) : boolTemp;

                        string msg = "No message response found";
                        foreach (var x in jObjectResponse)
                        {
                            if (x.Key.Equals("message"))
                            {
                                msg = x.Value.ToString();
                            }
                        }
                        MessageBox.Show(msg, "", MessageBoxButtons.OK, isSubmit ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                        if (isSubmit)
                        {
                            bg();
                        }
                    }
                    else
                    {
                        MessageBox.Show(response.ErrorMessage, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }
            }
        }
    }
}
