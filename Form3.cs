using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AB
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }
        HttpClient _httpClient = new HttpClient();

        private async void Form3_Load(object sender, EventArgs e)
        {
       

        }

        public async 
        Task
getDataFromWebService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://122.54.198.84:82");

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            _httpClient.DefaultRequestHeaders.Authorization =
new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzUxMiIsImlhdCI6MTYzNTMxNDgwMiwiZXhwIjoxNjM1NDg3NjAyfQ.eyJ1c2VyX2lkIjoyM30.fM9gvyEOoOFkgDDSv1tS_Zhz0YYQzDw-SQF8LzqFjtuCTj89nUDg6rwo_BA53A_aqX5LNdkUgwJ70okxV23Auw");

            JObject joBody = new JObject();
            JObject joHeader = new JObject();
            joHeader.Add("code", "C_Test");
            joHeader.Add("name", "C_Test");
            joHeader.Add("cust_type", 1);
            joBody.Add("header", joHeader);
            joBody.Add("details", new JArray());

            var content = new StringContent(joBody.ToString(), Encoding.UTF8, "application/json");


            HttpResponseMessage response = _httpClient.PostAsync("/api/customer/new", content).Result;
            var products = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(products);

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await getDataFromWebService();
        }
    }
}
