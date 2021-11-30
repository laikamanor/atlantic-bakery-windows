using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AB.API_Class.Notification;
using AB.API_Class.Branch;
using Newtonsoft.Json.Linq;
using AB.API_Class.Warehouse;
using System.Globalization;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;

namespace AB
{
    public partial class Notification2 : Form
    {
        public Notification2(int isDone)
        {
            InitializeComponent();
            gIsDone = isDone;
        }
        int gIsDone = 0;
        DataTable dtBranches = new DataTable(), dtWarehouse = new DataTable();
        branch_class branchc = new branch_class();
        warehouse_class warehousec = new warehouse_class();
        int cBranch = 1, cWhse = 1, cDate = 1, cToDate = 1, cCheckDate = 1, cCheckToDate = 1;
        private async void Notification2_Load(object sender, EventArgs e)
        {
            checkDate.Checked = gIsDone <= 0 ? false : true;
            dtFromDate.Visible = gIsDone <= 0 ? false : true;
            //dgv.Columns["btnAction"].Visible = gIsDone > 0 ? false : true;
            label1.Visible = gIsDone <= 0 ? false : true;
            dtFromDate.Value = DateTime.Now;
            dtToDate.Value = DateTime.Now;
            await loadBranches();
            bg();
            cBranch = 0;
            cWhse = 0;
            cDate = 0;
            cToDate = 0;
            cCheckDate = 0;
            cCheckToDate = 0;
        }


        public async Task loadBranches()
        {
            bool isAdmin = false;
            string branch = "";
            string currentBranch = "";
            dtBranches = await Task.Run(() => branchc.returnBranches());
            cmbBranches.Items.Clear();
            cmbBranches.Items.Add("All");
            AutoCompleteStringCollection auto = new AutoCompleteStringCollection();
            //get muna whse and check kung admin , superadmin or manager
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
                            else if (y.Key.Equals("isAdmin") || y.Key.Equals("isSuperAdmin") || y.Key.Equals("isAccounting") || y.Key.Equals("isManager"))
                            {
                                isAdmin = string.IsNullOrEmpty(y.Value.ToString()) ? false : Convert.ToBoolean(y.Value.ToString());
                                if (isAdmin)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                foreach (DataRow row in dtBranches.Rows)
                {
                    if (row["code"].ToString() == branch)
                    {
                        currentBranch = row["name"].ToString();
                    }
                }
                if (isAdmin)
                {
                    foreach (DataRow row in dtBranches.Rows)
                    {
                        auto.Add(row["name"].ToString());
                        cmbBranches.Items.Add(row["name"].ToString());
                    }
                }
                else
                {
                    auto.Add(currentBranch);
                    cmbBranches.Items.Add(currentBranch);
                }
                cmbBranches.SelectedIndex = cmbBranches.Items.IndexOf(currentBranch);
                cmbBranches.AutoCompleteCustomSource = auto;
            }
        }


        public async void loadData()
        {
            try
            {

                gridControl1.Invoke(new Action(delegate ()
                {
                    gridControl1.DataSource = null;
                    gridView1.Columns.Clear();
                }));

                string sBranch = "", sWarehouse = "", branch = "";
                cmbBranches.Invoke(new Action(delegate ()
                {
                    branch = cmbBranches.Text;
                }));
                foreach (DataRow row in dtBranches.Rows)
                {
                    if (row["name"].ToString() == branch)
                    {
                        sBranch = row["code"].ToString();
                    }
                }

                notification_class notifc = new notification_class();

                AutoCompleteStringCollection auto = new AutoCompleteStringCollection();
                string fromDate = checkDate.Checked ? "&from_date=" + dtFromDate.Value.ToString("MM/dd/yyyy") : "&from_date=";
                string toDate = checkToDate.Checked ? "&to_date=" + dtToDate.Value.ToString("MM/dd/yyyy") : "&to_date=";

                DataTable dt = await notifc.getUnreadNotif(sBranch, fromDate, toDate, "&whsecode=" + sWarehouse, gIsDone);
                if (dt.Rows.Count > 0 && dt.Columns.Count > 0)
                {
                    gridControl1.Invoke(new Action(delegate ()
                    {
                        gridControl1.DataSource = dt;

                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string v = col.GetCaption();
                            string s = col.GetCaption().Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.Visible = v.Equals("count") || v.Equals("id") || v.Equals("whsecode") || v.Equals("remarks_count") || v.Equals("auto_done") ? false : true;
                            col.ColumnEdit = repositoryItemTextEdit1;
                            if (v.Equals("date_done") && gIsDone <= 0)
                            {
                                col.Visible = false;
                            }
                            else if (v.Equals("date_updated") && gIsDone > 0)
                            {
                                col.Visible = false;
                            }
                            if (v.Equals("quantity"))
                            {
                                col.VisibleIndex = gridView1.Columns["item_code"].VisibleIndex + 1;
                            }
                            if (v.Equals("branch"))
                            {
                                col.VisibleIndex = gridView1.Columns["item_code"].VisibleIndex + 1;
                            }
                        }

                        GridColumn myCol1 = new GridColumn() { Caption = "Action", Visible = true, FieldName = "view_remarks" };
                        gridView1.Columns.Add(myCol1);
                        (gridControl1.MainView as GridView).Columns["view_remarks"].Width = 70;
                        (gridControl1.MainView as GridView).Columns["view_remarks"].ColumnEdit = repositoryItemButtonEdit1;

                        if(gIsDone <= 0)
                        {
                            GridColumn myCol2 = new GridColumn() { Caption = "Action", Visible = true, FieldName = "btnAction" };
                            gridView1.Columns.Add(myCol2);
                            (gridControl1.MainView as GridView).Columns["btnAction"].Width = 100;
                            (gridControl1.MainView as GridView).Columns["btnAction"].ColumnEdit = repositoryItemButtonEdit2;
                        }

                        gridView1.OptionsView.ShowFooter = true;

                        //Set up a summary on the "yourfieldname" column  
                        gridView1.Columns["item_code"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Count;

                        gridView1.Columns["item_code"].SummaryItem.FieldName = "Count";

                        gridView1.Columns["item_code"].SummaryItem.DisplayFormat = "Count: {0:n2}";
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //public void loadUI(DataTable dt)
        //{
        //    dgv.Invoke(new Action(delegate ()
        //    {

        //    }));

        //    foreach (DataRow row in dt.Rows)
        //    {
        //        dgv.Invoke(new Action(delegate ()
        //        {

        //        }));
        //    }

        //}

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void checkDate_CheckedChanged(object sender, EventArgs e)
        {
            dtFromDate.Visible = checkDate.Checked;
        }

        private void checkToDate_CheckedChanged(object sender, EventArgs e)
        {
            dtToDate.Visible = checkToDate.Checked;
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

        private void button1_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }

        private void gridView1_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            bool autoDone = false, boolTemp = false;
            GridView View = sender as GridView;
            //if (e.RowHandle >= 0)
            //{
           if(e.RowHandle >= 0)
            {
                autoDone = bool.TryParse(View.GetRowCellValue(e.RowHandle, View.Columns["auto_done"]).ToString(), out boolTemp) ? Convert.ToBoolean(View.GetRowCellValue(e.RowHandle, View.Columns["auto_done"]).ToString()) : boolTemp;
                e.Appearance.BackColor = !autoDone ? Color.White : Color.FromArgb(255, 249, 161);
            }
        }

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView currentView = sender as GridView;
            if (e.Column.FieldName == "view_remarks")
            {
                double value = !Convert.IsDBNull(currentView.GetRowCellValue(e.RowHandle, "remarks_count")) ? Convert.ToDouble(currentView.GetRowCellValue(e.RowHandle, "remarks_count")) : 0.00;
                e.Appearance.BackColor = value > 0 ? Color.FromArgb(40, 82, 122) : Color.FromArgb(192, 0, 192);
            }
        }

        private void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            RemarksDetails frm = new RemarksDetails(gIsDone);
            int temp = 0;
            frm.selectedID = int.TryParse(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "id").ToString(), out temp) ? Convert.ToInt32(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "id").ToString()) : 1;
            frm.ShowDialog();
            if (gIsDone <= 0)
            {
                bg();
            }
        }

        private async void repositoryItemButtonEdit2_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to Mark as Done?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                int temp = 0;
                int selectedID = int.TryParse(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "id").ToString(), out temp) ? Convert.ToInt32(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "id").ToString()) : 1;
                notification_class notifc = new notification_class();
                string msgDone = "";
                msgDone = await notifc.markAsRead(selectedID);
                MessageBox.Show(msgDone, "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                bg();
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

    }
}
