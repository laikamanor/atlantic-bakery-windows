using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using RestSharp;
using AB.UI_Class;
using AB.API_Class.Branch;
using System.Threading;

namespace AB
{
    public partial class salesAmountSummaryReport : Form
    {
        public salesAmountSummaryReport()
        {
            InitializeComponent();
        }
        JObject joResponse = new JObject();
        utility_class utilityc = new utility_class();
        branch_class branchc = new branch_class();
        public static DataTable dtSelectedBranches = new DataTable();
        DataTable dtBranch = new DataTable();
        int cTopBy = 1;
        public static bool isLoaded=false;
        private void salesAmountSummaryReport_Load(object sender, EventArgs e)
        {
            dtSelectedBranches.Columns.Clear();
            dtSelectedBranches.Columns.Add("code");
            dtSelectedBranches.Columns.Add("branch");

            cmbTransType.SelectedIndex = 0;
            label5.Visible = tabControl1.SelectedIndex.Equals(0) ? false : true;
            cmbTransType.Visible = tabControl1.SelectedIndex.Equals(0) ? false : true;
            cmbFromTime.SelectedIndex = 0;
            cmbToTime.SelectedIndex = cmbToTime.Items.Count - 1;

            if (!backgroundWorker1.IsBusy)
            {
                closeForm();
                Loading frm = new Loading();
                frm.Show();
                backgroundWorker1.RunWorkerAsync();
            }

            cmbByTop.SelectedIndex = 0;
            cTopBy = 0;
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

        public JArray getArray(string arrayName)
        {
            bool isSummary = false;
            tabControl1.Invoke(new Action(delegate ()
            {
                isSummary = tabControl1.SelectedIndex <= 0;
            }));
            JArray result = new JArray();
            foreach (var q in joResponse)
            {
                if (isSummary)
                {
                    if (q.Key.Equals("data"))
                    {
                        if (q.Value.ToString().Substring(0, 1).Equals("{"))
                        {
                            JObject joData = JObject.Parse(q.Value.ToString());
                            foreach (var w in joData)
                            {
                                if (w.Key.Equals(arrayName))
                                {
                                    if (!string.IsNullOrEmpty(w.Value.ToString()))
                                    {
                                        if (w.Value.ToString().Substring(0, 1).Equals("["))
                                        {
                                            result = JArray.Parse(w.Value.ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (q.Key.Equals("data"))
                    {
                        if (q.Value.ToString().Substring(0, 1).Equals("["))
                        {
                            result = JArray.Parse(q.Value.ToString());
                        }
                    }
                }
            }
            return result;
        }

        public void loadData()
        {
            joResponse = new JObject();
            try
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
                        var client = new RestClient(utilityc.URL);
                        client.Timeout = -1;


                        string branches = "";
                        for (int i = 0; i < dgvSelectedBranch.Rows.Count; i++)
                        {
                            branches = branches + "," + dgvSelectedBranch.Rows[i].Cells["code"].Value.ToString();
                        }
                        branches = (string.IsNullOrEmpty(branches) ? "" : branches.Substring(1));

                        string sFromDate = checkDate.Checked ? dtFromDate.Value.ToString("yyyy-MM-dd") : "";
                        string sToDate = checkToDate.Checked ? dtToDate.Value.ToString("yyyy-MM-dd") : "";
                        string sFromTime = "";
                        cmbFromTime.Invoke(new Action(delegate ()
                        {
                            sFromTime = cmbFromTime.Text == "" ? "&from_time=" : "&from_time=" + cmbFromTime.Text;
                        }));
                        string sToTime = "";
                        cmbToTime.Invoke(new Action(delegate ()
                        {
                            sToTime = cmbToTime.Text == "" ? "&to_time=" : "&to_time=" + cmbToTime.Text;
                        }));
                        int topIndex = 0;
                        cmbByTop.Invoke(new Action(delegate ()
                        {
                            topIndex = cmbByTop.SelectedIndex == 1 ? 5 : cmbByTop.SelectedIndex == 2 ? 10 : 0;
                        }));
                        string sTopBy = cTopBy <= 0 && topIndex > 0 ? "&top=" + topIndex : "";
                        string sTransType = "";
                        cmbTransType.Invoke(new Action(delegate ()
                        {
                            sTransType = cmbTransType.SelectedIndex == 0 || cmbTransType.Text == "All" ? "" : cmbTransType.Text;
                        }));
                        string appendParam = "?from_date=" + sFromDate + "&to_date=" + sToDate + "&branch=%5B" + branches + "%5D" + sFromTime + sToTime + sTopBy + "&transtype=" + sTransType;
                        string surl = "";
                        tabControl1.Invoke(new Action(delegate ()
                        {
                            surl = tabControl1.SelectedIndex.Equals(0) ? "/api/report/branch/sales/amount/summary" : "/api/report/branch/top/sales/amount";
                        }));

                        var request = new RestRequest(surl + appendParam);
                        request.AddHeader("Authorization", "Bearer " + token);
                        var response = client.Execute(request);
                        if (response.ErrorMessage == null)
                        {
                            if (response.Content.Substring(0, 1).Equals("{"))
                            {
                                JObject joTemp = JObject.Parse(response.Content.ToString());
                                bool isSuccess = false, boolTemp = false;
                                string msg = "";
                                foreach(var q in joTemp)
                                {
                                    if (q.Key.Equals("success"))
                                    {
                                        isSuccess = bool.TryParse(q.Value.ToString(), out boolTemp) ? Convert.ToBoolean(q.Value.ToString()) : boolTemp;
                                    }else if (q.Key.Equals("message"))
                                    {
                                        msg = q.Value.ToString();
                                        break;
                                    }
                                }
                                if (isSuccess)
                                {
                                    joResponse = JObject.Parse(response.Content.ToString());
                                }
                                else
                                {
                                    MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                            else
                            {
                                MessageBox.Show(response.Content, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show(response.ErrorMessage, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Thread.Sleep(1000);
            bool isSomeInvisible = dtFromDate.Visible || dtToDate.Visible ? false : true;
            double diff = isSomeInvisible ? 1 : (dtToDate.Value - dtFromDate.Value).TotalDays;
            string transType = "";
            cmbTransType.Invoke(new Action(delegate ()
            {
                transType = cmbTransType.Text;
            }));
            int count = dgvSelectedBranch.Rows.Count == dtBranch.Rows.Count || dgvSelectedBranch.Rows.Count <= 0 ? 0 : dgvSelectedBranch.Rows.Count;
            SASR1 s1 = new SASR1(getArray("branch_summary_report"), "branch_summary_report", diff,count);

            SASR2 s2 = new SASR2(getArray("data"), transType, true, "Branch",count,diff);

            bool isSummary = false;
            tabControl1.Invoke(new Action(delegate ()
            {
                isSummary = tabControl1.SelectedIndex.Equals(0);
            }));

            if (isSummary)
            {
                panel1.Invoke(new Action(delegate ()
                {
                    showForm(s1, panel1);
                }));
            }
            else
            {
                panel2.Invoke(new Action(delegate ()
                {
                    showForm(s2, panel2);
                }));
            }
        }

  

        private void checkToDate_CheckedChanged(object sender, EventArgs e)
        {
            dtToDate.Visible = checkToDate.Checked;
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void checkDate_CheckedChanged(object sender, EventArgs e)
        {
            dtFromDate.Visible = checkDate.Checked;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void dtFromDate_ValueChanged(object sender, EventArgs e)
        {

        }

        private async void btnSelectBranch_Click(object sender, EventArgs e)
        {
            if(dtBranch.Rows.Count <= 0)
            { 
                dtBranch = await branchc.returnBranches();
            }
            SelectBranch frm = new SelectBranch("Branch");
            frm.dt = dtBranch;
            frm.dtSelected = dtSelectedBranches;
            frm.ShowDialog();

            dtSelectedBranches.DefaultView.Sort = "branch ASC";
            dtSelectedBranches = dtSelectedBranches.DefaultView.ToTable();

            dgvSelectedBranch.DataSource = dtSelectedBranches;
            dgvSelectedBranch.Columns["branch"].HeaderText = "Branch";
            dgvSelectedBranch.Columns["branch"].ReadOnly = true;
            dgvSelectedBranch.Columns["code"].Visible = false;

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                closeForm();
                Loading frm = new Loading();
                frm.Show();
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void cmbByTop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cTopBy <= 0)
            {
                if (!backgroundWorker1.IsBusy)
                {
                    closeForm();
                    Loading frm = new Loading();
                    frm.Show();
                    backgroundWorker1.RunWorkerAsync();
                }
            }
        }

        public void showForm(Form form, Panel pn)
        {
            form.TopLevel = false;

            pn.Controls.Add(form);


            form.BringToFront();
            form.Show();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                isLoaded = false;
                loadData();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isLoaded = true;
            closeForm();
        }

        private void dgvSelectedBranch_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel5_Click(object sender, EventArgs e)
        {
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label5.Visible = tabControl1.SelectedIndex.Equals(0) ? false : true;
            cmbTransType.Visible = tabControl1.SelectedIndex.Equals(0) ? false : true;

            if (!backgroundWorker1.IsBusy)
            {
                closeForm();
                Loading frm = new Loading();
                frm.Show();
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void cmbTransType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex.Equals(1))
            {
                if (!backgroundWorker1.IsBusy)
                {
                    closeForm();
                    Loading frm = new Loading();
                    frm.Show();
                    backgroundWorker1.RunWorkerAsync();
                }
            }
        }
    }
}
