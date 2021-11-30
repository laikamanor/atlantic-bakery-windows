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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using DevExpress.XtraGrid.Columns;

namespace AB
{
    public partial class updateOverview : Form
    {
        public updateOverview()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        string sUpdates = "";
        private void updateOverview_Load(object sender, EventArgs e)
        {
            sUpdates = apic.loadTextFile("updates");
            if (!string.IsNullOrEmpty(sUpdates.Trim()) && sUpdates.Substring(0, 1).Equals("{"))
            {
                JObject joUpdates = JObject.Parse(sUpdates);
                foreach (var q in joUpdates)
                {
                    if (!q.Key.Equals("current_version"))
                        cmbVersion.Properties.Items.Add(q.Key);
                }
                string currentVersion = (string)joUpdates["current_version"];
                cmbVersion.SelectedIndex = cmbVersion.Properties.Items.IndexOf(currentVersion);
            }
           
        }

        private void cmbVersion_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(sUpdates.Trim()) && sUpdates.Substring(0, 1).Equals("{"))
            {
                JObject joUpdates = JObject.Parse(sUpdates);
                JArray jaSelectedVersion = (JArray) joUpdates[cmbVersion.Text];
                DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaSelectedVersion.ToString(), (typeof(DataTable)));
                gridControl1.DataSource = dtData;
            }
        }
    }
}
