using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AB.API_Class.Transfer;
using Newtonsoft.Json.Linq;
using AB.UI_Class;
using RestSharp;

namespace AB
{
    public partial class TransferItems : Form
    {
        transfer_class transferc = new transfer_class();
        utility_class utilityc = new utility_class();
        public int selectedID = 0;
        public static bool isSubmit = false;
        string gForType = "";
        public TransferItems(string forType)
        {
            gForType = forType;
            InitializeComponent();
        }

        private void TransferItems_Load(object sender, EventArgs e)
        {
            dgvitems.Columns["itemname"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //btnPrint.Visible = this.Text == "Transfer Items" ? true : false;
            btnPrint.Visible = false;
             loadData();
        }

        public void checkVariance()
        {
            for (int i = 0; i < dgvitems.Rows.Count; i++)
            {
                if (Convert.ToDouble(dgvitems.Rows[i].Cells["variance"].Value.ToString()) == 0)
                {
                    //dgvitems.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                }
                else if(Convert.ToDouble(dgvitems.Rows[i].Cells["variance"].Value.ToString()) < 0){
                    dgvitems.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(248, 255, 43);
                }
                else if (Convert.ToDouble(dgvitems.Rows[i].Cells["variance"].Value.ToString()) > 0){
                    dgvitems.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(0, 227, 76);
                }
            }
        }

        public void loadData()
        {
            DataTable dtItems = new DataTable();
            string URL = "";
            if (this.Text.Equals("Transfer Items"))
            {
                URL = "inv/trfr";
            }
            else if (this.Text.Equals("Received Items"))
            {
                URL = "inv/recv";
            }
            else
            {
                URL = "pullout";

            }
            dtItems = transferc.loadItems(URL, selectedID);
            if(dtItems.Rows.Count > 0)
            {
                foreach(DataRow row in dtItems.Rows)
                {
                    string decodeDocStatus = row["docstatus"].ToString() == "O" ? "Open" : row["docstatus"].ToString() == "C" ? "Closed" : "Cancelled";
                    double doubleTemp = 0.00;
                    double quantity = double.TryParse(row["quantity"].ToString(), out doubleTemp) ? Convert.ToDouble(row["quantity"].ToString()) : doubleTemp;


                    double price = double.TryParse(row["price"].ToString(), out doubleTemp) ? Convert.ToDouble(row["price"].ToString()) : doubleTemp;
                    double gross = double.TryParse(row["gross"].ToString(), out doubleTemp) ? Convert.ToDouble(row["gross"].ToString()) : doubleTemp;
                    if (URL.Equals("pullout"))
                    {

                        double receiveQty = double.TryParse(row["receive_qty"].ToString(), out doubleTemp) ? Convert.ToDouble(row["receive_qty"].ToString()) : doubleTemp;

                        double variance = (receiveQty - quantity);
                        dgvitems.Rows.Add(row["id"], row["transfer_id"], row["item_code"], Convert.ToDecimal(string.Format("{0:0.00}", quantity)), Convert.ToDecimal(string.Format("{0:0.00}", receiveQty)), Convert.ToDecimal(string.Format("{0:0.00}", variance)), price,gross);
                    }
                    else
                    {
                        double actualRec = Convert.ToDouble(row["actualrec"].ToString());
                        double variance = (actualRec - quantity);
                        dgvitems.Rows.Add(row["id"], row["transfer_id"], row["item_code"], Convert.ToDecimal(string.Format("{0:0.00}", quantity)), Convert.ToDecimal(string.Format("{0:0.00}", actualRec)), Convert.ToDecimal(string.Format("{0:0.00}", variance)),price,gross);
                    }
                    lblDocumentStatus.Text =decodeDocStatus;
                    lblReference.Text = row["reference"].ToString();
                    lblTransDate.Text = row["transdate"].ToString();
                    lblToWhse.Text = row[URL.Equals("inv/recv") ? "from_whse" : "to_whse"].ToString();
                    lblFromWhse.Text = row["from_whse"].ToString();
                    label5.Text= (URL.Equals("inv/recv") ? "From Warehouse:" : "To Warehouse");
                }
            }
            if(this.Text=="Received Items")
            {
                for (int i = 0; i < dgvitems.Rows.Count; i++)
                {
                    //MessageBox.Show(Convert.ToDouble(dgvitems.Rows[i].Cells["variance"].Value.ToString()).ToString("n2"));
                    if (Convert.ToDouble(dgvitems.Rows[i].Cells["variance"].Value.ToString()) == 0.00)
                    {
                        dgvitems.Rows[i].Cells["variance"].Style.ForeColor = Color.Black;
                    }
                    else if (Convert.ToDouble(dgvitems.Rows[i].Cells["variance"].Value.ToString()) > 0.00)
                    {
                        dgvitems.Rows[i].Cells["variance"].Style.ForeColor = Color.Blue;
                    }
                    else if (Convert.ToDouble(dgvitems.Rows[i].Cells["variance"].Value.ToString()) < 0.00)
                    {
                        dgvitems.Rows[i].Cells["variance"].Style.ForeColor = Color.Red;
                    }
                }
            }
            if (gForType.Equals("Open") && this.Text.Equals("Pullout Items") && lblDocumentStatus.Text != "Cancelled")
            {
                btnCancel.Visible = false;
                //btnCancel.Text = "Confirm";
                //btnCancel.BackColor = Color.DodgerBlue;
            }
            else if (gForType.Equals("Closed") && this.Text.Equals("Received Items"))
            {
                btnCancel.Visible = true;
                btnCancel.Text = "Cancel";
                btnCancel.BackColor = Color.Firebrick;
            }
            else if(gForType.Equals("Open") && this.Text.Equals("Received Items") && lblDocumentStatus.Text != "Cancelled")
            {
                btnCancel.Visible = true;
                btnCancel.Text = "Cancel";
                btnCancel.BackColor = Color.Firebrick;
            }
            else if (gForType.Equals("Open") && this.Text.Equals("Transfer Items") && lblDocumentStatus.Text != "Cancelled")
            {
                btnCancel.Visible = true;
                btnCancel.Text = "Cancel";
                btnCancel.BackColor = Color.Firebrick;
            }
            else if (gForType.Equals("Closed") && this.Text.Equals("Received Items") && lblDocumentStatus.Text != "Cancelled")
            {
                btnCancel.Visible = true;
                btnCancel.Text = "Cancel";
                btnCancel.BackColor = Color.Firebrick;
            }
            else if (gForType.Equals("Transfer Transactions SAP IT Open") && this.Text.Equals("Transfer Items"))
            {
                btnCancel.Visible = true;
                btnCancel.Text = "Update SAP #";
                btnCancel.BackColor = Color.DodgerBlue;
            }
            else
            {
                btnCancel.Visible = false;
            }
            dgvitems.Columns["actualrec"].HeaderText = (this.Text.Equals("Pullout Items") ? "Receive Quantity." : "Actual Receive");
            //dgvitems.Columns["actualrec"].Visible= (this.Text.Equals("Pullout Items") ? false : true);
            //dgvitems.Columns["variance"].Visible = ( this.Text.Equals("Pullout Items") ? false : true);
            if (this.Text == "Transfer Items")
            {
                checkVariance();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if(gForType.Equals("Open") || gForType.Equals("Closed"))
            {
                forCancel();
            }
            else
            {
                forUpdatingSAP();
            }
        }

        public void forUpdatingSAP()
        {
            JObject jObjectBody = new JObject();
            if (this.Text.Equals("Pullout Items"))
            {
                SAPWarehouse sAPWarehouse = new SAPWarehouse();
                sAPWarehouse.ShowDialog();
                if (SAPWarehouse.isSubmit)
                {
                    int sapNumber = SAPWarehouse.sapNumber;
                    string warehouseCode = SAPWarehouse.warehouseCode;
                    jObjectBody.Add("sap_number", sapNumber);
                    jObjectBody.Add("to_whse", warehouseCode);
                    string URL = "/api/pullout/transfer/" + selectedID;
                    apiPUT(jObjectBody, URL);
                }
            }
            else
            {
                SAP_Remarks frm = new SAP_Remarks();
                frm.ShowDialog();
                if (SAP_Remarks.isSubmit)
                {
                    int sap_number = SAP_Remarks.sap_number;
                    string remarks = SAP_Remarks.rem;
                    jObjectBody.Add("sap_number", sap_number > 0 ? sap_number : 0);
                    jObjectBody.Add("remarks", remarks);
                }
                string URL = (this.Text.Equals("Pullout Items") ? "/api/sap_num/pullout/update?ids=" + "%5B" + selectedID + "%5D" : gForType.Equals("Transfer Transactions SAP IT Open") ? "api/inv/trfr/update_sap/" + selectedID : "/api/inv/recv/update/" + selectedID);
                apiPUT(jObjectBody, URL);
            }
        }

        public void apiPUT(JObject body, string URL)
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
                    var request = new RestRequest(URL);
                    Console.WriteLine(URL);
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.Method = Method.PUT;

                    Console.WriteLine(body);
                    request.AddParameter("application/json", body, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    if (response.ErrorMessage == null)
                    {
                        JObject jObjectResponse = JObject.Parse(response.Content);

                        foreach (var x in jObjectResponse)
                        {
                            if (x.Key.Equals("success"))
                            {
                                isSubmit = string.IsNullOrEmpty(x.Value.ToString()) ? false : Convert.ToBoolean(x.Value.ToString());
                                break;
                            }
                        }

                        string msg = "No message response found";
                        foreach (var x in jObjectResponse)
                        {
                            if (x.Key.Equals("message"))
                            {
                                msg = x.Value.ToString();
                            }
                        }
                        MessageBox.Show(msg, "", MessageBoxButtons.OK, isSubmit ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                        if (isSubmit)
                        {
                            this.Dispose();
                        }
                    }
                    else
                    {
                        MessageBox.Show(response.ErrorMessage, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        public void forCancel()
        {
            if (this.Text.Equals("Received Items") || this.Text.Equals("Transfer Items"))
            {
                    Remarks remarkss = new Remarks();
                    remarkss.ShowDialog();
                    string remarks = Remarks.rem;
                if (Remarks.isSubmit)
                {

                    DialogResult dialogResult = MessageBox.Show("Are you sure you want to cancel?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        string type = this.Text.Equals("Received Items") ? "recv" : "trfr";
                        string sResponse = transferc.cancelTransfer(selectedID, remarks, type);
                        JObject jObjectResponse = JObject.Parse(sResponse);
                        string msg = "";
                        foreach (var x in jObjectResponse)
                        {
                            if (x.Key.Equals("message"))
                            {
                                msg = x.Value.ToString();
                            }
                        }
                        if (!string.IsNullOrEmpty(msg))
                        {
                            MessageBox.Show(msg, "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            isSubmit = true;
                            this.Dispose();
                        }
                    }
                }
            }
            else if (this.Text == "Pullout Items")
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to confirm?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    JObject jObjectBody = new JObject();
                    string URL = "/api/pullout/confirm/" + selectedID;
                    apiPUT(jObjectBody, URL);
                }
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("reference");
            dt.Columns.Add("document_status");
            dt.Columns.Add("transdate");
            dt.Columns.Add("from_whse");
            dt.Columns.Add("to_whse");
            dt.Columns.Add("item_code");
            dt.Columns.Add("quantity");
            dt.Columns.Add("actual_receive");
            dt.Columns.Add("variance");
            dt.Columns.Add("price");
            dt.Columns.Add("gross");
            for (int i = 0; i < dgvitems.Rows.Count; i++)
            {
                double quantity = 0.00, actualReceive = 0.00, variance = 0.00, doubleTemp = 0.00, price = 0.00, gross = 0.00;
                string itemCode = dgvitems.Rows[i].Cells["itemname"].Value.ToString(),
                    fromWhse = lblFromWhse.Text;

                quantity = double.TryParse(dgvitems.Rows[i].Cells["quantity"].Value.ToString(), out doubleTemp) ? Convert.ToDouble(dgvitems.Rows[i].Cells["quantity"].Value.ToString()) : doubleTemp;

                actualReceive = double.TryParse(dgvitems.Rows[i].Cells["actualrec"].Value.ToString(), out doubleTemp) ? Convert.ToDouble(dgvitems.Rows[i].Cells["actualrec"].Value.ToString()) : doubleTemp;

                variance = double.TryParse(dgvitems.Rows[i].Cells["variance"].Value.ToString(), out doubleTemp) ? Convert.ToDouble(dgvitems.Rows[i].Cells["variance"].Value.ToString()) : doubleTemp;

                price = double.TryParse(dgvitems.Rows[i].Cells["price"].Value.ToString(), out doubleTemp) ? Convert.ToDouble(dgvitems.Rows[i].Cells["price"].Value.ToString()) : doubleTemp;

                gross = double.TryParse(dgvitems.Rows[i].Cells["gross"].Value.ToString(), out doubleTemp) ? Convert.ToDouble(dgvitems.Rows[i].Cells["gross"].Value.ToString()) : doubleTemp;

                dt.Rows.Add(lblReference.Text, lblDocumentStatus.Text, lblTransDate.Text, fromWhse, lblToWhse.Text, itemCode, quantity, actualReceive, variance,price,gross);
            }

            printTransfer frm = new printTransfer(dt);
            frm.ShowDialog();
        }
    }
}
