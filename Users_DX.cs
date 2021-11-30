using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AB.API_Class.Branch;
using RestSharp;
using AB.UI_Class;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using System.Globalization;
using AB.UI_Class;
namespace AB
{
    public partial class Users_DX : Form
    {
        public Users_DX()
        {
            InitializeComponent();
        }
        utility_class utilityc = new utility_class();
        branch_class branchc = new branch_class();
        DataTable dtBranch = new DataTable();
        devexpress_class devc = new devexpress_class();
        int cBranch = 1;
        private async void Users_DX_Load(object sender, EventArgs e)
        {
            await loadBranch();
            refresh();
            cBranch = 0;
        }

        public async Task loadBranch()
        {
            int isAdmin = 0;
            string branch = "";
            dtBranch = await Task.Run(() => branchc.returnBranches());
            cmbBranch.Invoke(new Action(delegate ()
            {
                cmbBranch.Items.Clear();
            }));
            if (Login.jsonResult != null)
            {
                foreach (var x in Login.jsonResult)
                {
                    if (x.Key.Equals("data"))
                    {
                        JObject jObjectData = JObject.Parse(x.Value.ToString());
                        foreach (var y in jObjectData)
                        {
                            if (y.Key.Equals("branch"))
                            {
                                branch = y.Value.ToString();
                            }
                            else if (y.Key.Equals("isAdmin"))
                            {

                                if (y.Value.ToString().ToLower() == "false" || y.Value.ToString() == "")
                                {
                                    foreach (DataRow row in dtBranch.Rows)
                                    {
                                        if (row["code"].ToString() == branch)
                                        {
                                            cmbBranch.Invoke(new Action(delegate ()
                                            {
                                                cmbBranch.Items.Add(row["name"].ToString());
                                                if (cmbBranch.Items.Count > 0)
                                                {
                                                    cmbBranch.SelectedIndex = 0;
                                                }
                                            }));
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    isAdmin += 1;
                                    break;
                                }
                            }
                            else if (y.Key.Equals("isAccounting"))
                            {
                                if (y.Value.ToString().ToLower() == "false" || y.Value.ToString() == "")
                                {
                                    foreach (DataRow row in dtBranch.Rows)
                                    {
                                        if (row["code"].ToString() == branch && isAdmin <= 0)
                                        {
                                            cmbBranch.Invoke(new Action(delegate ()
                                            {
                                                cmbBranch.Items.Add(row["name"].ToString());
                                            }));
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                cmbBranch.Invoke(new Action(delegate ()
                {
                    if (cmbBranch.Items.Count <= 0)
                    {
                        foreach (DataRow row in dtBranch.Rows)
                        {
                            cmbBranch.Items.Add(row["name"]);
                        }
                    }
                }));
            }
            cmbBranch.Invoke(new Action(delegate ()
            {
                if (cmbBranch.Items.Count > 0)
                {
                    string branchName = "";
                    foreach (DataRow row in dtBranch.Rows)
                    {
                        if (row["code"].ToString() == branch)
                        {
                            branchName = row["name"].ToString();
                            break;
                        }
                        else
                        {
                            cmbBranch.SelectedIndex = 0;
                        }
                    }
                    cmbBranch.SelectedIndex = cmbBranch.Items.IndexOf(branchName);
                }
            }));
        }

        public async Task <DataTable> loadData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("id");
            dt.Columns.Add("username");
            dt.Columns.Add("fullname");
            dt.Columns.Add("warehouse");
            try
            {
                if (Login.jsonResult != null)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    AutoCompleteStringCollection auto = new AutoCompleteStringCollection();
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
                        string branchValue = "", search = "";
                        cmbBranch.Invoke(new Action(delegate ()
                        {
                            branchValue = cmbBranch.Text;
                        }));
                        string branch = (branchValue.Equals("") || branchValue == "All" ? "" : findBranchCode(branchValue));
                        //string branch = "A1-S";
                        var request = new RestRequest("/api/auth/user/get_all?branch=" + branch + "&search=" + search);
                        Console.WriteLine("/api/auth/user/get_all?branch=" + branch + "&search=" + search);
                        request.AddHeader("Authorization", "Bearer " + token);
                        Task<IRestResponse> t = client.ExecuteAsync(request);
                        t.Wait();
                        var response = await t;
                        Console.WriteLine(response.Content.ToString());
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
                                    if (x.Value.ToString() != "[]")
                                    {
                                        JArray jsonArray = JArray.Parse(x.Value.ToString());
                                        for (int i = 0; i < jsonArray.Count(); i++)
                                        {
                                            JObject data = JObject.Parse(jsonArray[i].ToString());
                                            int id = 0;
                                            string userName = "",
                fullName = "", warehouse = "";
                                            foreach (var q in data)
                                            {
                                                if (q.Key.Equals("username"))
                                                {
                                                    userName = q.Value.ToString();
                                                    auto.Add(q.Value.ToString());
                                                }
                                                else if (q.Key.Equals("fullname"))
                                                {
                                                    fullName = q.Value.ToString();
                                                }
                                                else if (q.Key.Equals("id"))
                                                {
                                                    id = Convert.ToInt32(q.Value.ToString());
                                                }
                                                else if (q.Key.Equals("whse"))
                                                {
                                                    warehouse = q.Value.ToString();
                                                }
                                            }
                                            dt.Rows.Add(id, userName, fullName, warehouse);
                                        }
                                    }
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
                            MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    Cursor.Current = Cursors.Default;
                }
            }
            catch
            {

            }
            return dt;
        }

        public string findBranchCode(string value)
        {
            string result = "";
            foreach (DataRow row in dtBranch.Rows)
            {
                if (row["name"].ToString() == value)
                {
                    result = row["code"].ToString();
                    break;
                }
            }
            return result;
        }

        public async void refresh()
        {
            gridView1.Columns.Clear();
            gridControl1.DataSource = null;
            DataTable dt = await loadData();
            if (dt.Rows.Count > 0)
            {
                dt.Columns.Add("edit");
            }
            gridControl1.DataSource = dt;
            gridView1.OptionsView.ColumnAutoWidth = false;
            foreach (GridColumn col in gridView1.Columns)
            {
                string fieldName = col.FieldName;
                string v = col.GetCaption();
                string s = col.GetCaption().Replace("_", " ");
                col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                col.ColumnEdit = fieldName.Equals("edit") ? repositoryItemButtonEdit1 : repositoryItemTextEdit1;
                col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.None;
                col.DisplayFormat.FormatString = "";
                col.Visible = !(fieldName.Equals("id"));

                devc.changeFont(col);
            }
            gridView1.BestFitColumns();
            //auto complete
            string[] suggestions = { "username" };
            devc.loadSuggestion(gridView1, gridControl1, suggestions);
            gridView1.OptionsView.ColumnAutoWidth = false;
            gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
        }

        private async void cmbBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cBranch <= 0)
            {
                refresh();
            }
        }

        private async void btnAddUser_Click(object sender, EventArgs e)
        {
            AddUser addUser = new AddUser();
            addUser.Text = "Add User";
            addUser.ShowDialog();
            if (AddUser.isSubmit)
            {
                refresh();
            }
        }

        private void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            int id = 0, intTemp = 0;
            id = gridView1.GetFocusedDataRow()["id"] == null ? intTemp : Int32.TryParse(gridView1.GetFocusedDataRow()["id"].ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedDataRow()["id"].ToString()) : intTemp;
            AddUser addUser = new AddUser();
            addUser.userID = id;
            addUser.Text = "Edit User";
            addUser.ShowDialog();
            if (AddUser.isSubmit)
            {
                refresh();
            }
        }
    }
}
