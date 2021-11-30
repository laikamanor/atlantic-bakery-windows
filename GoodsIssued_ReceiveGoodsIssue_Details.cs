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
    public partial class GoodsIssued_ReceiveGoodsIssue_Details : Form
    {
        public GoodsIssued_ReceiveGoodsIssue_Details(int id, string reference)
        {
            InitializeComponent();
            this.id = id;
            this.reference = reference;
        }
        int id = 0;
        public static bool isSubmit = false;
        string reference = "";
        api_class apic = new api_class();
        devexpress_class devc = new devexpress_class();
        DataTable dtForIssue = new DataTable(), dtBase = new DataTable();
        private void GoodsIssued_ReceiveGoodsIssue_Details_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            lblReference.Text = reference;
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

            string sParams = id.ToString() + "?mode=confirm";
            string sResult = apic.loadData("/api/production/issue_for_prod/details/", sParams, "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
            {
                JObject joResponse = JObject.Parse(sResult);
                JObject joData = (JObject)joResponse["data"];
                JArray jaBase = (JArray)joData["base"];
                JArray jaForIssue = (JArray)joData["for_issue"];
                dtBase = (DataTable)JsonConvert.DeserializeObject(jaBase.ToString(), (typeof(DataTable)));
                dtForIssue = (DataTable)JsonConvert.DeserializeObject(jaForIssue.ToString(), (typeof(DataTable)));

                //whsecode
                lblWhse.Invoke(new Action(delegate ()
                {
                    if (dtBase.Rows.Count > 0)
                    {
                        if (dtBase.Columns.Contains("whsecode"))
                        {
                            lblWhse.Text = dtBase.Rows[0]["whsecode"].ToString();
                        }
                    }
                }));

                loadBaseFg(dtBase);
                loadForIssue(dtForIssue);
            }
        }

        public void loadForIssue(DataTable dt)
        {
            gridControl1.Invoke(new Action(delegate ()
            {
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
            }));
            gridControl1.Invoke(new Action(delegate ()
            {
                gridControl1.DataSource = dt;

                foreach (GridColumn col in gridView1.Columns)
                {
                    string fieldName = col.FieldName;
                    string v = col.GetCaption();
                    string s = col.GetCaption().Replace("_", " ");
                    col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                    col.ColumnEdit = repositoryItemTextEdit1;
                    col.DisplayFormat.FormatType = fieldName.Equals("quantity") ? DevExpress.Utils.FormatType.Numeric : DevExpress.Utils.FormatType.None;

                    col.DisplayFormat.FormatString = fieldName.Equals("quantity") ? "n3" : "";

                    col.Visible = !(fieldName.Equals("id") || fieldName.Equals("doc_id") || fieldName.Equals("objtype") || fieldName.Equals("inv_qty") || fieldName.Equals("inv_uom") || fieldName.Equals("created_by") || fieldName.Equals("updated_by") || fieldName.Equals("date_created") || fieldName.Equals("date_updated"));

                    //fonts
                    FontFamily fontArial = new FontFamily("Arial");
                    col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                    col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                }
                gridView1.BestFitColumns();
            }));
        }

        public void loadBaseFg(DataTable dt)
        {
            gridControl2.Invoke(new Action(delegate ()
            {
                gridControl2.DataSource = null;
                gridView2.Columns.Clear();
            }));
            gridControl2.Invoke(new Action(delegate ()
            {
                gridControl2.DataSource = dt;
                gridView2.OptionsView.ColumnAutoWidth = false;
                gridView2.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                foreach (GridColumn col in gridView2.Columns)
                {
                    string fieldName = col.FieldName;
                    string v = col.GetCaption();
                    string s = col.GetCaption().Replace("_", " ");
                    col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                    col.ColumnEdit = fieldName.Equals("item_code") ? repositoryItemMemoEdit1 : repositoryItemTextEdit1;
                    col.DisplayFormat.FormatType = fieldName.Equals("planned_qty") ? DevExpress.Utils.FormatType.Numeric : DevExpress.Utils.FormatType.None;

                    col.DisplayFormat.FormatString = fieldName.Equals("planned_qty") ? "n2" : "";

                    col.Visible = !(fieldName.Equals("id"));

                    //fonts
                    FontFamily fontArial = new FontFamily("Arial");
                    col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                    col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);

                }
                gridView2.BestFitColumns();
                var colItemCode = gridView2.Columns["item_code"];
                if (colItemCode != null)
                {
                    colItemCode.Width = 150;
                }
            }));
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void btnConfirmIssue_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to confirm issue?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    JObject joBody = new JObject();
                    apiPUT(joBody, "/api/production/issue_for_prod/confirm/"  + id.ToString());
                    if (isSubmit)
                    {
                        this.Hide();
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
                    utility_class utilityc = new utility_class();
                    var client = new RestClient(utilityc.URL);
                    client.Timeout = -1;
                    var request = new RestRequest(URL);
                    Console.WriteLine(URL);
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.Method = Method.PUT;

                    Console.WriteLine(body);
                    request.AddParameter("application/json", body, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    if (response.ErrorMessage == null)
                    {
                        if (response.Content.Substring(0, 1).Equals("{"))
                        {
                            JObject jObjectResponse = JObject.Parse(response.Content);
                            isSubmit = false;
                            foreach (var x in jObjectResponse)
                            {
                                if (x.Key.Equals("success"))
                                {
                                    isSubmit = string.IsNullOrEmpty(x.Value.ToString()) ? false : Convert.ToBoolean(x.Value.ToString());
                                    break;
                                }
                            }

                            string msg = "No message response found";
                            foreach (var x in jObjectResponse)
                            {
                                if (x.Key.Equals("message"))
                                {
                                    msg = x.Value.ToString();
                                }
                            }
                            MessageBox.Show(msg, "", MessageBoxButtons.OK, isSubmit ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show(response.Content.ToString(), "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show(response.ErrorMessage, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }
            }
        }

        private void c(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }
    }
}
