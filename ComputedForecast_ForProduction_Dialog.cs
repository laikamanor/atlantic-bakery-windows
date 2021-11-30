using Newtonsoft.Json.Linq;
using RestSharp;
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
namespace AB
{
    public partial class ComputedForecast_ForProduction_Dialog : Form
    {
        public ComputedForecast_ForProduction_Dialog(JArray ja,string branch)
        {
            InitializeComponent();
            this.ja = ja;
            this.branch = branch;
        }
        JArray ja = new JArray();
        api_class apic = new api_class();
        string branch = "";
        public static bool isSubmit = false;
        private void ComputedForecast_ForProduction_Dialog_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            dtProductionDate.EditValue = DateTime.Now;
        }

        public string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ0123456789abcdefghijklmnñopqrstuvxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string hashedID = RandomString(20);
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to submit?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                if (string.IsNullOrEmpty(txtRemarks.Text.Trim()))
                {
                    txtRemarks.Focus();
                    MessageBox.Show("Remarks field is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }else
                {
                    JObject jo = new JObject();
                    JObject joHeader = new JObject();
                    joHeader.Add("transdate", DateTime.Now);
                    joHeader.Add("production_date", dtProductionDate.Text);
                    joHeader.Add("sap_number", null);
                    joHeader.Add("remarks", string.IsNullOrEmpty(txtRemarks.Text.Trim()) ? null : txtRemarks.Text.Trim());
                    joHeader.Add("prod_whse", branch);
                    joHeader.Add("hashed_id", hashedID);
                    jo.Add("header", joHeader);
                    jo.Add("rows", ja);
                    Console.WriteLine(jo);
                    string response = apic.loadData("/api/production/order/new", "", "application/json", jo.ToString(), Method.POST, true);
                    Console.WriteLine("response: " + response);
                    if (!string.IsNullOrEmpty(response.Trim()))
                    {
                        if (response.StartsWith("{"))
                        {
                            JObject joResponse = JObject.Parse(response);
                            string msg = joResponse["message"].ToString();
                            bool isSuccess = false, boolTemp = false;
                            isSuccess = bool.TryParse(joResponse["success"].ToString(), out boolTemp) ? Convert.ToBoolean(joResponse["success"].ToString()) : boolTemp;
                            MessageBox.Show(msg, isSuccess ? "Message" : "Validation", MessageBoxButtons.OK, isSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                            isSubmit = isSuccess;
                            if (isSubmit)
                            {
                                this.Hide();
                            }
                            btnSubmit.Enabled = true;
                        }
                        else
                        {
                            btnSubmit.Enabled = true;
                        }
                    }
                    else
                    {
                        btnSubmit.Enabled = true;
                    }
                }
            }
        }
    }
}
