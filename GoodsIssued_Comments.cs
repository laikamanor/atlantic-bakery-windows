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
    public partial class GoodsIssued_Comments : Form
    {
        public GoodsIssued_Comments(int id, string reference)
        {
            InitializeComponent();
            this.id = id;
            this.reference = reference;
        }
        int id = 0;
        string reference = "";
        devexpress_class devc = new devexpress_class();
        utility_class utilityc = new utility_class();
        api_class apic = new api_class();
        ui_class uic = new ui_class();
        private void GoodsIssued_Comments_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            lblReference.Text = reference;
            bg();
        }
        public void loadData()
        {
            try
            {
                string sParams = id.ToString();
                string sResult = apic.loadData("/api/production/issue_for_prod/comments/get_all/", sParams, "", "", Method.GET, true);
                if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
                {
                    double runningBalance = 0.00;
                    DateTime dtTemp = new DateTime();
                    JObject joResponse = JObject.Parse(sResult);
                    JArray jaData = joResponse["data"] == null ? new JArray() : (JArray)joResponse["data"];
                    //lblToWhse.Text = jaTransRow[0]["to_whse"].ToString();
                    DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                    if (dtData.Rows.Count > 0)
                    {
                        DataRow row = dtData.Rows[0];
                        //btnClose.Invoke(new Action(delegate ()
                        //{
                        //    btnClose.Visible =  docStatus.Trim().ToLower().Equals("o");
                        //}));
                        //btnCancel.Invoke(new Action(delegate ()
                        //{
                        //    btnCancel.Visible = !(docStatus.Trim().ToLower().Equals("n"));
                        //}));
                    }
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        gridControl1.DataSource = null;
                    }));

                    dtData.SetColumnsOrder("date_created", "comments", "created_by", "id");

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
                            col.ColumnEdit = fieldName.Equals("comments") ? repositoryItemMemoEdit1 : repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("date_created") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;
                            col.DisplayFormat.FormatString = fieldName.Equals("date_created") ? "yyyy-MM-dd HH:mm:ss" : "";
                            col.Visible = !(fieldName.Equals("id"));

                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }
                        //auto complete
                        string[] suggestions = { "comments" };
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

        private void btnAddComment_Click(object sender, EventArgs e)
        {
            GoodsIssued_AddComment.isSubmit = false;
            GoodsIssued_AddComment frm = new GoodsIssued_AddComment(id, reference);
            frm.ShowDialog();
            if (GoodsIssued_AddComment.isSubmit)
            {
                bg();
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
    }
}
