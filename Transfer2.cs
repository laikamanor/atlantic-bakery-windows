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
using AB.API_Class.Warehouse;
using AB.API_Class.Transfer;
using AB.API_Class.User;
using Newtonsoft.Json.Linq;
namespace AB
{
    public partial class Transfer2 : Form
    {

        DataTable dtBranch = new DataTable(), dtWarehouse = new DataTable();
        branch_class branchc = new branch_class();
        warehouse_class warehousec = new warehouse_class();
        transfer_class transferc = new transfer_class();
        user_clas userc = new user_clas();
        string gForType = "";

        int cBranch = 1, cWarehouse = 1, cStatus = 1, cToDate = 1, cToWarehouse = 1, cFromDate = 1, cCheckFromDate = 1, cCheckToDate = 1, cCheckBranchToBranch = 1;
        public Transfer2(string forType)
        {
            gForType = forType;
            InitializeComponent();
        }

        private async void Transfer2_Load(object sender, EventArgs e)
        {
            await loadBranch();
            loadWarehouse(cmbWhse, this.Text.Equals("Received Transactions") ? true : false);
            loadWarehouse(cmbToWhse, this.Text.Equals("Received Transactions") ? false : true);

            
            dtFromDate.Visible = checkFromDate.Checked = gForType.Equals("Open") ? false : true;
            checkToDate.Checked = true;
            checkBranchToBranch.Visible = checkBranchToBranch.Checked = this.Text == "Transfer Transactions" ? true : false;
            cmbFromTime.SelectedIndex = 0;
            cmbToTime.SelectedIndex = cmbToTime.Items.Count - 1;
            label2.Visible = label12.Visible = cmbFromTime.Visible = cmbToTime.Visible = this.Text.Equals("Pullout Transactions") ? false : true;

            bg();
            dtFromDate.Value = DateTime.Now;
            dtToDate.Value = DateTime.Now;
            cCheckFromDate = 0;
            cCheckToDate = 0;
            cBranch = 0;
            cCheckBranchToBranch = 0;
            cWarehouse = 0;
            cStatus = 0;
            cToDate = 0;
            cFromDate = 0;
            cToWarehouse = 0;
            dgvTransactions.Columns["rec_reference"].Visible = dgvTransactions.Columns["rec_trandate"].Visible = this.Text.Equals("Transfer Transactions") && (gForType.Equals("Closed") || gForType.Equals("Cancelled")) ? true : false;
            dgvTransactions.Columns["from_whse"].Visible = dgvTransactions.Columns["to_whse"].Visible = this.Text.Equals("Transfer Transactions") || this.Text.Equals("Pullout Transactions") ? true : false;
            dgvTransactions.Columns["date_confirmed"].Visible = gForType.Equals("Closed") && this.Text.Equals("Pullout Transactions") ? true : false;
            dgvTransactions.Columns["date_close"].Visible = gForType.Equals("Closed") && this.Text.Equals("Received Transactions") ? true : false;
        }

        public void checkVariance()
        {
            for (int i = 0; i < dgvTransactions.Rows.Count; i++)
            {
                bool isBranchBranch = false, boolTemp = false, interWhse = false;
                isBranchBranch = bool.TryParse(dgvTransactions.Rows[i].Cells["is_branch_to_branch"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvTransactions.Rows[i].Cells["is_branch_to_branch"].Value.ToString()) : false;
                interWhse = bool.TryParse(dgvTransactions.Rows[i].Cells["inter_whse"].Value.ToString(), out boolTemp) ? Convert.ToBoolean(dgvTransactions.Rows[i].Cells["inter_whse"].Value.ToString()) : false;
                if (Convert.ToDouble(dgvTransactions.Rows[i].Cells["variance_count"].Value.ToString()) != 0)
                {
                    dgvTransactions.Rows[i].Cells["reference"].Style.BackColor = Color.FromArgb(255, 110, 110);
                }
                else if (isBranchBranch)
                {
                    dgvTransactions.Rows[i].Cells["from_whse"].Style.BackColor = Color.FromArgb(115, 255, 110);
                    dgvTransactions.Rows[i].Cells["to_whse"].Style.BackColor = Color.FromArgb(115, 255, 110);
                }
                else if (!interWhse && !isBranchBranch)
                {
                    dgvTransactions.Rows[i].Cells["from_whse"].Style.BackColor = Color.FromArgb(255, 173, 110);
                    dgvTransactions.Rows[i].Cells["to_whse"].Style.BackColor = Color.FromArgb(255, 173, 110);
                }
            }
        }

        public async Task loadBranch()
        {
            string currentBranch = "";
            bool isAdmin = false;
            cmbBranch.Items.Clear();

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
                                currentBranch = y.Value.ToString();
                            }
                            else if (y.Key.Equals("isAdmin") || y.Key.Equals("isSuperAdmin") || y.Key.Equals("isManager") || y.Key.Equals("isCashier") || y.Key.Equals("isAccounting") || y.Key.Equals("isSalesAgent"))
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
            }
            dtBranch = await branchc.returnBranches();
            if (isAdmin)
            {
                dtBranch = await branchc.returnBranches();
                cmbBranch.Items.Add("All");
                foreach (DataRow row in dtBranch.Rows)
                {
                    cmbBranch.Items.Add(row["name"]);
                }
            }
            else
            {
                foreach (DataRow row in dtBranch.Rows)
                {
                    if (row["code"].ToString() == currentBranch)
                    {
                        cmbBranch.Items.Add(row["name"]);
                        break;
                    }
                }
            }
            //default text 
            //kapag admin or to whse all yung lalabas
            //kapag hindi kung ano yung current whse nya yun yung lalabas
            string branchName = "";
            foreach (DataRow row in dtBranch.Rows)
            {
                if (row["code"].ToString().Trim().ToLower() == currentBranch.Trim().ToLower())
                {
                    branchName = row["name"].ToString();
                    break;
                }
            }
            cmbBranch.SelectedIndex = cmbBranch.Items.IndexOf(branchName);
        }

        public async void loadWarehouse(ComboBox cmb, bool isTo)
        {
            try
            {
                string warehouse = "";
                bool isAdmin = false;
                string whse = "", branch = "";
                cmb.Items.Clear();

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
                                if (y.Key.Equals("whse"))
                                {
                                    whse = y.Value.ToString();
                                }
                                else if (y.Key.Equals("isAdmin") || y.Key.Equals("isSuperAdmin") || y.Key.Equals("isManager") || y.Key.Equals("isCashier") || y.Key.Equals("isAccounting") || y.Key.Equals("isSalesAgent"))
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
                }

                //kunin yung branch code ng combobox branch text
                foreach (DataRow row in dtBranch.Rows)
                {
                    if (row["name"].ToString() == cmbBranch.Text)
                    {
                        branch = row["code"].ToString();
                        break;
                    }
                }
                // kunin warehouse base kung to or from whse
                dtWarehouse = await Task.Run(() => warehousec.returnWarehouse(isTo ? "" : branch, this.Text == "Transfer Transactions" && checkBranchToBranch.Checked ? "&is_main=True" : "&is_main="));
                //dtWarehouse = await Task.Run(() => warehousec.returnWarehouse(isTo ? "" : branch, this.Text == "Transfer Transactions" && checkBranchToBranch.Checked ? "" : ""));
                //kapag admin kunin lahat ng warehouse 
                // kapag di admin kukunin lang yung current wareheouse nya
                if (isAdmin)
                {
                    cmb.Items.Add("All");
                    foreach (DataRow row in dtWarehouse.Rows)
                    {
                        cmb.Items.Add(row["whsename"]);
                    }
                }
                else
                {
                    string currentWhse = "";
                    foreach (DataRow row in dtWarehouse.Rows)
                    {
                        if (row["whsecode"].ToString() == whse)
                        {
                            currentWhse = row["whsename"].ToString();
                        }
                    }
                    cmb.Items.Add(currentWhse);
                }
                //default text 
                //kapag admin or to whse all yung lalabas
                //kapag hindi kung ano yung current whse nya yun yung lalabas
                if (isAdmin || isTo)
                {
                    cmb.SelectedIndex = 0;
                }
                else
                {
                    string whseName = "";
                    foreach (DataRow row in dtWarehouse.Rows)
                    {
                        if (row["whsecode"].ToString() == whse)
                        {
                            whseName = row["whsename"].ToString();
                            break;
                        }
                    }
                    cmb.SelectedIndex = cmb.Items.IndexOf(whseName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void loadData()
        {
            dgvTransactions.Invoke(new Action(delegate ()
            {
                dgvTransactions.Rows.Clear();
            }));
            string statusCode = gForType.Equals("Open") || gForType.Equals("Closed") ? gForType.Substring(0, 1) : "N";
            DataTable dtTransfers = new DataTable();

            string url = "trfr";
            if (this.Text == "Transfer Transactions")
            {
                url = "trfr";
            }
            else if (this.Text == "Pullout Transactions")
            {
                url = "pullout";
            }
            else
            {
                url = "recv";
            }
            //MessageBox.Show("TRANSFER 2: "  +url);

            string warehouseCode = "", branchCode = "", toWarehouseCode = "";
            //WAREHOUSE
            string fromWhse = "", toWhse = "", branch = "";
            cmbWhse.Invoke(new Action(delegate ()
            {
                fromWhse = cmbWhse.Text;
            }));
            foreach (DataRow row in dtWarehouse.Rows)
            {
                if (fromWhse.Equals(row["whsename"].ToString()))
                {
                    warehouseCode = row["whsecode"].ToString();
                    break;
                }
            }
            //TO WAREHOUSE
            cmbToWhse.Invoke(new Action(delegate ()
            {
                toWhse = cmbToWhse.Text;
            }));
            foreach (DataRow row in dtWarehouse.Rows)
            {
                if (toWhse.Equals(row["whsename"].ToString()))
                {
                    toWarehouseCode = row["whsecode"].ToString();
                    break;
                }
            }
            //BRANCH
            cmbBranch.Invoke(new Action(delegate ()
            {
                branch = cmbBranch.Text;
            }));
            foreach (DataRow row in dtBranch.Rows)
            {
                if (branch.Equals(row["name"].ToString()))
                {
                    branchCode = row["code"].ToString();
                    break;
                }
            }

            string sWarehouse = string.IsNullOrEmpty(warehouseCode) ? "" : "&from_whse=" + warehouseCode;
            string sToWarehouse = string.IsNullOrEmpty(toWarehouseCode) ? "" : "&to_whse=" + toWarehouseCode;
            string sBranch = string.IsNullOrEmpty(branchCode) ? "" : "&branch=" + branchCode;
            string sUnderScore = "", sURL = "";
            if (url.Equals("recv") || url.Equals("pullout"))
            {
                sUnderScore = "_";
            }
            if (url.Equals("pullout"))
            {
                sURL = "/api/";
            }
            else
            {
                sURL = "/api/inv/";
            }
            //MessageBox.Show("class: " + URL);

            string sFromDate = checkFromDate.Checked ? dtFromDate.Value.ToString("yyyy-MM-dd") : "",
                sToDate = checkToDate.Checked ? dtToDate.Value.ToString("yyyy-MM-dd") : "";

            string isBranchToBranch = "";
            checkBranchToBranch.Invoke(new Action(delegate ()
            {
                isBranchToBranch = checkBranchToBranch.Checked ? "1" : "";
            }));
            string sFromTime = "", sToTime = "";
           cmbFromTime.Invoke(new Action(delegate ()
            {
                sFromTime = "&from_time=" + cmbFromTime.Text;
            }));
            cmbToTime.Invoke(new Action(delegate ()
            {
                sToTime = "&to_time=" + cmbToTime.Text;
            }));

            dtTransfers = transferc.loadData(sURL + url + "/get" + sUnderScore + "all", statusCode, txtsearchTransactions.Text.Trim(), sToDate, gForType, sBranch, sWarehouse, sToWarehouse, sFromDate, isBranchToBranch, this.Text.Equals("Pullout Transactions") ? "" : sFromTime + sToTime);
            if (dtTransfers.Rows.Count > 0)
            {
                if(dtTransfers.Rows.Count > 0)
                {
                    DataRow dtRow1 = dtTransfers.Rows[0];
                    if (Convert.ToBoolean(dtRow1[0].ToString()))
                    {
                        AutoCompleteStringCollection auto = new AutoCompleteStringCollection();
                        foreach (DataRow row in dtTransfers.Rows)
                        {
                            string decodeDocStatus = row["docstatus"].ToString() == "O" ? "Open" : row["docstatus"].ToString() == "C" ? "Closed" : "Cancelled";
                            auto.Add(row["reference"].ToString());

                            DateTime dtTransDate = Convert.ToDateTime(row["transdate"].ToString());
                            DateTime dtRecTransDate = Convert.ToDateTime(row["rec_transdate"].ToString());
                            DateTime dtDateConfirmed = Convert.ToDateTime(row["date_confirmed"].ToString());
                            DateTime dtDateClose = Convert.ToDateTime(row["date_close"].ToString());
                            if (!txtsearchTransactions.Text.ToLower().Equals("search reference..."))
                            {
                                if (txtsearchTransactions.Text.ToLower().Equals(row["reference"].ToString().ToLower()))
                                {
                                    dgvTransactions.Invoke(new Action(delegate ()
                                    {
                                        dgvTransactions.Rows.Add(row["id"], row["transnumber"], row["reference"], row["from_whse"], row["to_whse"], dtTransDate.Equals(default(DateTime)) ? "" : dtTransDate.ToString("yyyy-MM-dd HH:mm"), row["remarks"], decodeDocStatus, row["variance_count"].ToString(), row["rec_reference"], dtRecTransDate.Equals(default(DateTime)) ? "" : dtRecTransDate.ToString("yyyy-MM-dd HH:mm"), row["is_branch_to_branch"], dtDateConfirmed.ToString("yyyy-MM-dd HH:mm"), row["inter_whse"], dtDateClose == DateTime.MinValue ? "" : dtDateClose.ToString("yyyy-MM-dd HH:mm:ss"),row["sap_number"].ToString());
                                    }));
                                }

                            }
                            else
                            {
                                dgvTransactions.Invoke(new Action(delegate ()
                                {
                                    dgvTransactions.Rows.Add(row["id"], row["transnumber"], row["reference"], row["from_whse"], row["to_whse"], dtTransDate.Equals(default(DateTime)) ? "" : dtTransDate.ToString("yyyy-MM-dd HH:mm"), row["remarks"], decodeDocStatus, row["variance_count"].ToString(), row["rec_reference"], dtRecTransDate.Equals(default(DateTime)) ? "" : dtRecTransDate.ToString("yyyy-MM-dd HH:mm"), row["is_branch_to_branch"], dtDateConfirmed.ToString("yyyy-MM-dd HH:mm"), row["inter_whse"], dtDateClose == DateTime.MinValue ? "" : dtDateClose.ToString("yyyy-MM-dd HH:mm:ss"));
                                }));
                            }
                        }
                        txtsearchTransactions.Invoke(new Action(delegate ()
                        {
                            txtsearchTransactions.AutoCompleteCustomSource = auto;
                        }));
                    }
                    else
                    {
                        MessageBox.Show(dtRow1[1].ToString(), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

            }
            lblNoDataFound.Invoke(new Action(delegate ()
            {
                lblNoDataFound.Visible = (dgvTransactions.Rows.Count <= 0 ? true : false);
            }));

            if (this.Text == "Transfer Transactions")
            {
                checkVariance();
            }
        }


        private void checkFromDate_CheckedChanged(object sender, EventArgs e)
        {
            if(cCheckFromDate <= 0)
            {
                dtFromDate.Visible = checkFromDate.Checked;
            }
        }

        private void checkToDate_CheckedChanged(object sender, EventArgs e)
        {
            if(cCheckToDate <= 0)
            {
                dtToDate.Visible = checkToDate.Checked;
            }
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

        private void txtsearchTransactions_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtsearchTransactions.Text.Trim()))
            {
                txtsearchTransactions.Text = "Search Reference...";
                txtsearchTransactions.ForeColor = Color.DimGray;
            }
        }

        private void btnSearchQuery_Click(object sender, EventArgs e)
        {
            bg();
        }

        private void btnSearchQuery2_Click(object sender, EventArgs e)
        {
            bg();
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

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadData();
        }


        private void txtsearchTransactions_Enter(object sender, EventArgs e)
        {
            if (txtsearchTransactions.Text.ToLower().Equals("search reference..."))
            {
                txtsearchTransactions.Text = string.Empty;
                txtsearchTransactions.ForeColor = Color.Black;
            }
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }

        private void checkBranchToBranch_CheckedChanged(object sender, EventArgs e)
        {
            if(this.Text== "Transfer Transactions" && cCheckBranchToBranch <=0)
            {
                cmbWhse.Items.Clear();
                cmbToWhse.Items.Clear();
                loadWarehouse(cmbWhse, this.Text.Equals("Received Transactions") ? true : false);
                loadWarehouse(cmbToWhse, this.Text.Equals("Received Transactions") ? false : true);
            }
        }

        private void dgvTransactions_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvTransactions.Rows.Count > 0)
            {
                if (e.ColumnIndex == 2)
                {
                    string sText = "";
                    if (this.Text.Equals("Transfer Transactions"))
                    {
                        sText = "Transfer Items";
                    }
                    else if (this.Text.Equals("Received Transactions"))
                    {
                        sText = "Received Items";
                    }
                    else
                    {
                        sText = "Pullout Items";

                    }
                    TransferItems transferItems = new TransferItems(gForType);
                    transferItems.selectedID = Convert.ToInt32(dgvTransactions.CurrentRow.Cells["id"].Value.ToString());

                    transferItems.Text = sText;
                    transferItems.ShowDialog();
                    if (TransferItems.isSubmit)
                    {
                        bg();
                    }
                }
            }
        }




        private  void cmbBranch_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cBranch <= 0)
            {
                cmbWhse.Items.Clear();
                cmbToWhse.Items.Clear();
                loadWarehouse(cmbWhse, this.Text.Equals("Received Transactions") ? true : false);
                loadWarehouse(cmbToWhse, this.Text.Equals("Received Transactions") ? false : true);
            }
        }
    }
}
