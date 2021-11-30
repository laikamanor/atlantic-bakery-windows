using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using AB.UI_Class;
namespace AB
{
    public partial class GoodsReceipt_AddComment : Form
    {
        public GoodsReceipt_AddComment(int id, string reference)
        {
            InitializeComponent();
            this.id = id;
            this.reference = reference;
        }
        public static bool isSubmit = false;
        int id = 0;
        string reference = "";
        devexpress_class devc = new devexpress_class();
        utility_class utilityc = new utility_class();
        api_class apic = new api_class();
        ui_class uic = new ui_class();

        private void GoodsReceipt_AddComment_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            lblReference.Text = reference;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to submit?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                if (string.IsNullOrEmpty(txtComment.Text.Trim()))
                {
                    MessageBox.Show("Comment field is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    bg();
                }
            }
        }
        public void addComment()
        {
            try
            {
                JObject joBody = new JObject();
                joBody.Add("comments", txtComment.Text);
                string sResult = apic.loadData("/api/production/rec_from_prod/comments/new/", id.ToString(), "application/json", joBody.ToString(), Method.POST, true);
                if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
                {
                    JObject jObjectResponse = JObject.Parse(sResult);
                    string msg = jObjectResponse["message"] == null ? "" : jObjectResponse["message"].ToString();
                    bool boolTemp = false;
                    isSubmit = jObjectResponse["success"] == null ? false : bool.TryParse(jObjectResponse["success"].ToString(), out boolTemp) ? Convert.ToBoolean(jObjectResponse["success"].ToString()) : boolTemp;
                    apic.showCustomMsgBox(isSubmit ? "Message" : "Validation", msg);
                    if (isSubmit)
                    {
                        this.Invoke(new Action(delegate ()
                        {
                            this.Hide();
                        }));
                    }
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

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            addComment();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }
    }
}
