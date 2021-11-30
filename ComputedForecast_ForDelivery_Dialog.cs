using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms;
using AB.UI_Class;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace AB
{
    public partial class ComputedForecast_ForDelivery_Dialog : Form
    {
        public ComputedForecast_ForDelivery_Dialog(JArray jaArray, string branch)
        {
            InitializeComponent();
            this.jaArray = jaArray;
            this.branch = branch;
        }
        string branch = "";
        JArray jaArray = new JArray();
        api_class apic = new api_class();
        ui_class uic = new ui_class();
        DataTable dtShift = new DataTable(), dtBranches = new DataTable();
        public static bool isSubmit = false;
        private void ComputedForecast_ForDelivery_Dialog_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            dtDeliveryDate.EditValue = DateTime.Now;
            loadShift();
        }

        public string RandomString(int length)
        {
            string result = "";
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ0123456789abcdefghijklmnñopqrstuvxyz";
            result= new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
            System.Threading.Thread.Sleep(5);
            return result;
        }

        public void loadShift()
        {
            try
            {
                cmbShift.Invoke(new Action(delegate ()
                {
                    cmbShift.Properties.Items.Clear();
                }));
                string sResult = "";
                sResult = apic.loadData("/api/production/shift/get_all", "", "", "", Method.GET, true);
                if (sResult.Substring(0, 1).Equals("{"))
                {
                    dtShift = apic.getDtDownloadResources(sResult, "data");
                    foreach (DataRow row in dtShift.Rows)
                    {
                        if (IsHandleCreated)
                        {
                            cmbShift.Invoke(new Action(delegate ()
                            {
                                cmbShift.Properties.Items.Add(row["description"].ToString());
                            }));
                        }

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

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to submit?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                     if (cmbShift.SelectedIndex == -1 || string.IsNullOrEmpty(cmbShift.Text.Trim()))
                    {
                        MessageBox.Show("Shift field is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        cmbShift.Focus();
                    }
                    else if (string.IsNullOrEmpty(txtRemarks.Text.Trim()))
                    {
                        MessageBox.Show("Remarks field is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtRemarks.Focus();
                    }
                    else
                    {
                        btnSubmit.Enabled = false;
                        if(jaArray.Count > 0)
                        {
                            for (int i = 0; i < jaArray.Count; i++)
                            {
                               
                                JObject jo = jaArray[i].IsNullOrEmpty() ? new JObject() : jaArray[i].Type == JTokenType.Object ? (JObject)jaArray[i] : new JObject();
                                jo["header"]["transdate"] = DateTime.Now;

                                jo["header"]["delivery_date"] = dtDeliveryDate.Text;
                                jo["header"]["remarks"] = txtRemarks.Text.Trim();
                                int shiftID = 0, intTemp = 0;
                                string sID = apic.findValueInDataTable(dtShift, cmbShift.Text, "description", "id");
                                shiftID = int.TryParse(sID, out intTemp) ? Convert.ToInt32(sID) : intTemp;
                                jo["header"]["shift"] = shiftID <= 0 ? (int?)null : shiftID;
                            }
                            Console.WriteLine(jaArray);
                            string response = apic.loadData("/api/forecast/target_for_delivery/new", "", "application/json", jaArray.ToString(), Method.POST, true);
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
                                    btnSubmit.Enabled = true;
                                    if (isSuccess)
                                    {
                                        this.Hide();
                                    }
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
                        else
                        {
                            MessageBox.Show("No rows found!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
