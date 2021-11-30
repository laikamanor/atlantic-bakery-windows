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
    public partial class ItemRequestTransfer_Details : Form
    {
        public ItemRequestTransfer_Details(int id,string reference,string docStatus, string forSap)
        {
            InitializeComponent();
            this.id = id;
            this.reference = reference;
            this.docStatus = docStatus;
            this.forSap = forSap;
        }
        int id = 0;
        string reference = "", docStatus = "", forSap = "";
        devexpress_class devc = new devexpress_class();
        utility_class utilityc = new utility_class();
        api_class apic = new api_class();
        ui_class uic = new ui_class();
        public static bool isSubmit = false;
        private void TargetForDelivery_Details_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            lblReference.Text = reference;
            btnUpdateSAP.Visible = forSap.Equals("0");
            bg();
        }

        public void loadData()
        {
            try
            {
                string sParams = id.ToString();
                string sResult = apic.loadData("/api/forecast/get_for_delivery/details/", sParams, "", "", Method.GET, true);
                if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
                {
                    double runningBalance = 0.00;
                    DateTime dtTemp = new DateTime();
                    JObject joResponse = JObject.Parse(sResult);
                    JArray jaData = joResponse["data"] == null ? new JArray() : (JArray)joResponse["data"];
                    //lblToWhse.Text = jaTransRow[0]["to_whse"].ToString();
                    DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        gridControl1.DataSource = null;
                    }));

                    dtData.SetColumnsOrder("item_code", "quantity", "final_qty", "actual_delivered","balance", "from_whse", "to_whse", "linestatus");

                    gridControl1.Invoke(new Action(delegate ()
                    {
                        gridControl1.DataSource = dtData;
                        gridView1.OptionsView.ColumnAutoWidth = false;
                        gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            string v = col.GetCaption();
                            string s = col.GetCaption().Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.Caption = fieldName.Equals("closee") ? "Close" : col.Caption;
                            //col.Caption = fieldName.Equals("amount_in") ? "Sales" : fieldName.Equals("amount_out") ? "Payment" : col.Caption;
                            col.ColumnEdit = repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("quantity") || fieldName.Equals("final_qty") || fieldName.Equals("actual_delivered") || fieldName.Equals("balance") ? DevExpress.Utils.FormatType.Numeric : DevExpress.Utils.FormatType.None;
                            col.DisplayFormat.FormatString = fieldName.Equals("quantity") || fieldName.Equals("final_qty") || fieldName.Equals("actual_delivered") || fieldName.Equals("balance") ? "n2" : "";
                            col.Visible = !(fieldName.Equals("id") || fieldName.Equals("doc_id") || fieldName.Equals("linestatus") || fieldName.Equals("objtype"));

                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }
                        //auto complete
                        string[] suggestions = { "item_code" };
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        public void apiPUT(JObject body, string URL)
        {
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
                    Console.WriteLine("received trans " + URL);
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.Method = Method.PUT;

                    Console.WriteLine(body);
                    request.AddParameter("application/json", body, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    bool boolTemp = false;
                    if (response.ErrorMessage == null)
                    {
                        if (response.Content.StartsWith("{"))
                        {
                            JObject jObjectResponse = JObject.Parse(response.Content);
                            isSubmit = bool.TryParse(jObjectResponse["success"].ToString(), out boolTemp) ? Convert.ToBoolean(jObjectResponse["success"].ToString()) : boolTemp;
                            string msg = jObjectResponse["message"].ToString();
                            MessageBox.Show(msg, "", MessageBoxButtons.OK, isSubmit ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                            if (isSubmit)
                            {
                                this.Dispose();
                            }
                        }
                        else
                        {
                            MessageBox.Show(response.Content, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show(response.ErrorMessage, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void btnUpdateSAP_Click(object sender, EventArgs e)
        {
            if (docStatus.Equals("") && forSap.Equals("0"))
            {
                SAP_Remarks.isSubmit = false;
                SAP_Remarks frm = new SAP_Remarks();
                frm.Text = "Update SAP# - " + reference;
                frm.ShowDialog();
                if (SAP_Remarks.isSubmit)
                {
                    JObject joBody = new JObject();
                    joBody.Add("sap_number", SAP_Remarks.sap_number);
                    joBody.Add("remarks", SAP_Remarks.rem.Trim());
                    apiPUT(joBody, "/api/sap_num/target_for_delivery/" + id.ToString());
                }
            }
            else
            {
                MessageBox.Show("Access Denied", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
