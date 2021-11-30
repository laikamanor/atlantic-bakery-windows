using DevExpress.XtraGrid.Columns;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using AB.UI_Class;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
namespace AB
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            DataTable dtBranch = loadBranch();
            DataTable dtItem = loadItems();
            DataTable dtForeCast= loadData();


            DataTable dt = new DataTable();
            dt.Columns.Add("item_code");
            dt.Columns.Add("branch");
            dt.Columns.Add("base_qty", typeof(double));
            foreach (DataRow row in dtBranch.Rows)
            {
                foreach (DataRow row2 in dtItem.Rows)
                {
                    string sItemCode = row2["item_code"].ToString().Replace(@"'", "''");
                    string sBranch = row["code"].ToString().Replace(@"'", "''");
                    DataRow[] lists = dtForeCast.Select("item_code='" + sItemCode + "' AND branch='" + sBranch + "'");

                    double prodMin = 0.00, doubleTemp = 0.00;
                    bool isProdMin = false;
                    if (lists.Length > 0)
                    {
                        foreach (DataRow row3 in lists)
                        {
                            isProdMin = double.TryParse(row3["prod_min_qty"].ToString(), out doubleTemp);
                            prodMin = isProdMin ? Convert.ToDouble(row3["prod_min_qty"].ToString()) : doubleTemp;
                        }
                    }
                    if (isProdMin)
                    {
                        dt.Rows.Add(row["code"].ToString(), row2["item_code"].ToString(), prodMin);
                    }
                    else
                    {
                        dt.Rows.Add(row["code"].ToString(), row2["item_code"].ToString(), DBNull.Value);
                    }
                }
            }
            gridControl1.DataSource = dt;
        }

        public DataTable loadBranch()
        {
            api_class apic = new api_class();
            string sResult = apic.loadData("/api/branch/get_all", "", "", "", RestSharp.Method.GET, true);
            if (sResult.StartsWith("{"))
            {
                JObject joResult = string.IsNullOrEmpty(sResult.ToString().Trim()) || !sResult.ToString().Substring(0, 1).Equals("{") ? new JObject() : JObject.Parse(sResult);
                JArray jaData = (JArray)joResult["data"] == null || string.IsNullOrEmpty(joResult["data"].ToString().Trim()) || !joResult["data"].ToString().Substring(0, 1).Equals("[") ? new JArray() : (JArray)joResult["data"];
               return (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
            }
            return new DataTable();
        }


        public DataTable loadItems()
        {
            api_class apic = new api_class();
            string sResult = apic.loadData("/api/item/getall", "?is_active=1", "", "", RestSharp.Method.GET, true);
            if (sResult.StartsWith("{"))
            {
                JObject joResult = string.IsNullOrEmpty(sResult.ToString().Trim()) || !sResult.ToString().Substring(0, 1).Equals("{") ? new JObject() : JObject.Parse(sResult);
                JArray jaData = (JArray)joResult["data"] == null || string.IsNullOrEmpty(joResult["data"].ToString().Trim()) || !joResult["data"].ToString().Substring(0, 1).Equals("[") ? new JArray() : (JArray)joResult["data"];
                return (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
            }
            return new DataTable();
        }



        public DataTable loadData()
        {
            api_class apic = new api_class();
            string sResult = apic.loadData("/api/forecast/computed_forecast", "?from_date=2021-11-01&to_date=2021-11-01&from_time=00:00&to_time=14:00&ending_date=2021-11-01&ending_time=14:00&with_last_bal=1&multiplier=1&prod_whse=MTC-FG", "", "", RestSharp.Method.GET, true);
            if (sResult.StartsWith("{"))
            {
                JObject joResult = string.IsNullOrEmpty(sResult.ToString().Trim()) || !sResult.ToString().Substring(0, 1).Equals("{") ? new JObject() : JObject.Parse(sResult);
                JArray jaData = (JArray)joResult["data"] == null || string.IsNullOrEmpty(joResult["data"].ToString().Trim()) || !joResult["data"].ToString().Substring(0, 1).Equals("[") ? new JArray() : (JArray)joResult["data"];
                return (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), typeof(DataTable));
            }
            return new DataTable();
        }
    }
}
