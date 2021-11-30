using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AB.UI_Class;
using RestSharp;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace AB.API_Class.Item_Sales_Summary
{
    class itemsalessummary_class
    {
        utility_class utilityc = new utility_class();

        public void closeForm()
        {
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.Name == "customMessageBox")
                {
                    frm.Hide();
                }
            }
        }

        public DataTable loadData(string appendURL)
        {
            DataTable dt = new DataTable();
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
                    //string branch = (cmbBranch.Text.Equals("") || cmbBranch.Text == "All" ? "" : cmbBranch.Text);
                    var request = new RestRequest("/api/report/item/sales/summary" + appendURL);
                    Console.WriteLine("/api/report/item/sales/summary" + appendURL);
                    request.AddHeader("Authorization", "Bearer " + token);
                    var response = client.Execute(request);
                    if (response.ErrorMessage == null)
                    {
                        if (response.Content.Substring(0, 1).Equals("{"))
                        {
                            //Console.WriteLine(response.Content);
                            JObject jObject = new JObject();
                            jObject = JObject.Parse(response.Content.ToString());
                            bool isSuccess = false;
                            foreach (var x in jObject)
                            {
                                if (x.Key.Equals("success"))
                                {
                                    isSuccess = Convert.ToBoolean(x.Value.ToString());
                                }
                            }
                            if (isSuccess)
                            {
                                foreach (var x in jObject)
                                {
                                    if (x.Key.Equals("data"))
                                    {
                                        
                                        dt = (DataTable)JsonConvert.DeserializeObject(x.Value.ToString(), (typeof(DataTable)));
                                    }
                                }
                            }
                            else
                            {
                                string msg = "No message response found";
                                foreach (var x in jObject)
                                {
                                    if (x.Key.Equals("message"))
                                    {
                                        msg = x.Value.ToString();
                                    }
                                }
                                if (msg.Equals("Token is invalid"))
                                {
                                    closeForm();
                                    customMessageBox frm = new customMessageBox();
                                    frm.lblTitle.Text = "Validation";
                                    frm.lblBody.Text = "Your login session is expired. Please login again";
                                    frm.ShowDialog();
                                }
                                else
                                {
                                    closeForm();
                                    customMessageBox frm = new customMessageBox();
                                    frm.lblTitle.Text = "Validation";
                                    frm.lblBody.Text = msg;
                                    frm.ShowDialog();
                                }
                            }
                        }
                        else
                        {
                            closeForm();
                            customMessageBox frm = new customMessageBox();
                            frm.lblTitle.Text = "Validation";
                            frm.lblBody.Text = response.Content;
                            frm.ShowDialog();
                        }
                    }
                    else
                    {
                        closeForm();
                        customMessageBox frm = new customMessageBox();
                        frm.lblTitle.Text = "Validation";
                        frm.lblBody.Text = response.ErrorMessage;
                        frm.ShowDialog();
                    }
                }
            }
            return dt;
        }
    }
}
