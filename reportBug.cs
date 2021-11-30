using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AB
{
    public partial class reportBug : Form
    {
        public reportBug()
        {
            InitializeComponent();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            CreateBug();
        }

        public void CreateBug()
        {
            //WebRequest request = WebRequest.Create("");
            //request.Method = "POST";
            //string postData = "{'title':'exception occured!', 'body':'{0}','assignee': 'laikamanor'}";

            //byte[] byteArray = Encoding.UTF8.GetBytes(string.Format(postData));
            //request.ContentLength = byteArray.Length;
            //Stream dataStream = request.GetRequestStream();
            //dataStream.Write(byteArray, 0, byteArray.Length);
            //dataStream.Close();
            //WebResponse response = request.GetResponse();
            //using (Stream stream = response.GetResponseStream())
            //{
            //    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            //    String responseString = reader.ReadToEnd();
            //    MessageBox.Show(responseString);
            //}

            var client = new RestClient("https://api.github.com");
            client.Timeout = -1;
            var request = new RestRequest("/repos/laikamanor/files/issues");
            request.Method = Method.POST;

            JObject body = new JObject();
            body.Add("title", "found a bug!");
            body.Add("body", "asd");
            body.Add("owner", "laikamanor");
            body.Add("repo", "pos");

            request.AddParameter("application/json", body, ParameterType.RequestBody);
            var response = client.Execute(request);
            if (response.ErrorMessage == null)
            {
                Console.WriteLine(response.Content);
            }
            else
            {
                Console.WriteLine(response.ErrorMessage);
            }
        }
    }
}
