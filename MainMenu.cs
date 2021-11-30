using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AB.UI_Class;
using Newtonsoft.Json.Linq;
using Tulpep.NotificationWindow;
using System.Data.SqlClient;
using AB.API_Class.Notification;
using System.Threading.Tasks;
using RestSharp;
using System.Threading;
using AB.UI_Class;
using System.IO;
using Newtonsoft.Json;

namespace AB
{
    public partial class MainMenu : Form
    {
        public MainMenu()
        {
            InitializeComponent();
        }
        utility_class utilityc = new utility_class();
        api_class apic = new api_class();
        bool isProductionAddress = false;
        private async void MainMenu_Load(object sender, EventArgs e)
        {
            //badge1.Properties.Text = "1";
            //badge1.Properties.Location = ContentAlignment.TopRight;
            //badge1.TargetElement = menuStrip1;
            this.Text = utilityc.appName +  " - " + Login.fullName + " - v" + utilityc.versionName +" - " + utilityc.URL.Replace("http://", "");
            isProductionAddress = utilityc.URL.Contains(utilityc.getTextfromGithub(utilityc.abWindowsProdURLFile));
            //await NotifcationAsync();
        }



        public void showForm(Form form)
        {
            form.TopLevel = false;
            panelchildform.Controls.Add(form);
            form.BringToFront();
            form.Show();
        }

        private void pendingOrdersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //PendingOrder pendingOrder = new PendingOrder();
            //showForm(pendingOrder);
            SalesTab frm = new SalesTab();
            showForm(frm);
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to logout?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                Login.jsonResult = null;
                this.Dispose();
                Login login = new Login();
                login.ShowDialog();
            }
        }

        private void usersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (apic.haveAccessAdmin())
            {
                Users_DX users = new Users_DX();
                showForm(users);
            }
            else
            {
                MessageBox.Show("Access Denied", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void MainMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to logout?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                Login.jsonResult = null;
                this.Dispose();
                Login login = new Login();
                login.ShowDialog();
            }
        }

        private void advancePaymentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AdvancePayment advancePayment = new AdvancePayment();
            showForm(advancePayment);
        }

        private void inventoryToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Inventory2 inventory = new Inventory2();
            showForm(inventory);
        }

        private void itemRequestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ItemRequest itemRequest = new ItemRequest();
            showForm(itemRequest);
        }

        private void transferTransactionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Transfer transfer = new Transfer();
            //transfer.Text = "Transfer Transactions";
            //showForm(transfer);
            TransferItem_Tab frm = new TransferItem_Tab();
            showForm(frm);
        }

        //private void cashTransactionReportToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    CashTransactionReport cashTransactionReport = new CashTransactionReport();
        //    showForm(cashTransactionReport);
        //}

        //private void salesReportToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    SalesReport salesReport = new SalesReport();
        //    showForm(salesReport);
        //}

        private void receivedTransactionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Transfer transfer = new Transfer();
            //transfer.Text = "Received Transactions";
            ReceiveItem_Tab frm = new ReceiveItem_Tab();
           
            showForm(frm);
        }

        private void branchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (apic.haveAccessAdmin())
            {
                Branches2 branch = new Branches2();
                showForm(branch);
            }
            else
            {
                MessageBox.Show("Access Denied", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void customersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (apic.haveAccessAdmin())
            {
                Customers2 customers = new Customers2();
                showForm(customers);
            }
            else
            {
                MessageBox.Show("Access Denied", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void inventoryCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printableReports("Final Count Report");
        }

        public void printableReports(string reportType)
        {
            EnterDate enterDate = new EnterDate(reportType);
            enterDate.ShowDialog();
        }

        private void inventorySummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printableReports("Final Report");
        }

        private void salesTransactionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SalesTransactions2 salesTransactions = new SalesTransactions2();
            showForm(salesTransactions);
        }

        private void pulloutTransactionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Transfer transfer = new Transfer();
            //transfer.Text = "Pullout Transactions";
            PullOut_Tab frm = new PullOut_Tab();
            showForm(frm);
        }

        private void adjustmentInToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AdjustmentIn adjustmentIn = new AdjustmentIn("in");
            showForm(adjustmentIn);
        }

        private void adjustmentOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AdjustmentIn adjustmentIn = new AdjustmentIn("out");
            showForm(adjustmentIn);
        }

        private void objectTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (apic.haveAccessAdmin())
            {
                ObjectType2 adjustmentIn = new ObjectType2();
                showForm(adjustmentIn);
            }
            else
            {
                MessageBox.Show("Access Denied", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void warehouseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (apic.haveAccessAdmin())
            {
                Warehouse2 warehouse = new Warehouse2();
                showForm(warehouse);
            }
            else
            {
                MessageBox.Show("Access Denied", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void itemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (apic.haveAccessAdmin())
            {
                Items_DX items = new Items_DX();
                showForm(items);
            }
            else
            {
                MessageBox.Show("Access Denied", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cashVarianceTransactionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CashVariance items = new CashVariance();
            showForm(items);
        }

        private void itemSalesReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ItemSalesValueTab items = new ItemSalesValueTab();
            showForm(items);
        }

        private void seriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (apic.haveAccessAdmin())
            {
                Series2 items = new Series2();
                showForm(items);
            }
            else
            {
                MessageBox.Show("Access Denied", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void priceListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (apic.haveAccessAdmin())
            {
                Pricelist2 items = new Pricelist2();
                showForm(items);
            }
            else
            {
                MessageBox.Show("Access Denied", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void salesReportsWoInventoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printableReports("Final Sales Report");
        }

        private void notifications0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //NotificationBar items = new NotificationBar();
            //items.Location = new Point(Cursor.Position.X - 50, Cursor.Position.Y + 20);
            //items.ShowDialog();
            //int notifBarSelectedID = NotificationBar.selectedID;
            //if (notifBarSelectedID > 0)
            //{
            //    Notification nf = new Notification();
            //    nf.selectedID = notifBarSelectedID;
            //    showForm(nf);
            //}
            NotificationTab nf = new NotificationTab();
            showForm(nf);
        }

        private void addActualCashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddActualCash add = new AddActualCash();
            add.ShowDialog();
        }

        private void pOSToolStripMenuItem_Click(object sender, EventArgs e)
        {
                POS items = new POS();
                showForm(items);
        }

        private void uomGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (apic.haveAccessAdmin())
            {
                UOMGroup2 items = new UOMGroup2();
                showForm(items);
            }
            else
            {
                MessageBox.Show("Access Denied", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void uomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (apic.haveAccessAdmin())
            {
                UOM2 items = new UOM2();
                showForm(items);
            }
            else
            {
                MessageBox.Show("Access Denied", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void productionOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProductionOrder_Tab items = new ProductionOrder_Tab();
            showForm(items);
        }

        private void issueForProductionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoodsIssued_Tab items = new GoodsIssued_Tab();
            showForm(items);
        }

        private void receiptFromProductionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoodsReceipt_Tab items = new GoodsReceipt_Tab();
            showForm(items);
        }

        private void gLAccountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (apic.haveAccessAdmin())
            {
                GLAccounts2 items = new GLAccounts2();
                showForm(items);
            }
            else
            {
                MessageBox.Show("Accedd Denied", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            //await NotifcationAsync();
        }

        public async Task NotifcationAsync()
        {
            notification_class notifc = new notification_class();
            DataTable dt = await Task.Run(() => notifc.getUnreadNotif("","","","",0));
            loadNotification(dt);
        }

        public async void loadNotification(DataTable dtUnread)
        {
            notification_class notifc = new notification_class();
            //dtUnread = await notifc.getUnreadNotif();
            int count = 0;
            if (dtUnread.Rows.Count > 0)
            {
                DataRow row = dtUnread.Rows[0];
                count = string.IsNullOrEmpty(row["count"].ToString()) ? 0 : Convert.ToInt32(row["count"].ToString());
            }
            notifications0ToolStripMenuItem.Text = "Notification (" + count.ToString("N0") + ")";

        }

        private void productionToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void salesAmountSummaryReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SASR0 frm = new SASR0();
            showForm(frm);
        }

        private void reportABugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Under Development", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void updatesOverviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Under Development", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void itemSalesSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ItemSalesQuantityTab frm = new ItemSalesQuantityTab();
            showForm(frm);
        }

        private void changePasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changePassword frm = new changePassword();
            frm.ShowDialog();
        }

        private void MainMenu_Shown(object sender, EventArgs e)
        {

        }


        public void downloadResources()
        {
            try
            {
                if (!File.Exists(utilityc.resourcesFile))
                {
                    File.Create(utilityc.resourcesFile).Close();
                }
                else
                {
                    File.WriteAllText(utilityc.resourcesFile, String.Empty);
                }
                DataTable dt = new DataTable();
                dt.Columns.Add("name");
                dt.Columns.Add("path");
                List<string> sResourcesName = apic.downloadResourcesName();
                List<string> sResourcesPath = apic.downloadResourcesPath();
                for (int i = 0; i < sResourcesName.Count; i++)
                {
                    dt.Rows.Add(sResourcesName[i], sResourcesPath[i]);
                }
                JObject joResult = new JObject();
                int haveErrorinDownload = 0;
                foreach (DataRow row in dt.Rows)
                {
                    string sResult = apic.loadData(row["path"].ToString(), "", "", "", Method.GET, true);
                    if (!string.IsNullOrEmpty(sResult.Trim()) && sResult.Substring(0,1).Equals("{"))
                    {
                        JObject joData = new JObject();
                        joData.Add("result", sResult);
                        JArray jaData = new JArray();
                        jaData.Add(joData);
                        joResult.Add(row["name"].ToString(), jaData);
                    }
                    else
                    {
                        haveErrorinDownload += 1;
                    }
                }
                if (haveErrorinDownload <=0)
                {
                    File.AppendAllText("resources.txt", apic.encodeValue(joResult.ToString()));
                    MessageBox.Show("Resources Downloaded", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Download failed try again!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void downloadResourcesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            downloadResources();
        }

        private void customerLedgerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CustomerLedger2 frm = new CustomerLedger2();
            showForm(frm);
        }

        private void sOAToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SOATab2 frm = new SOATab2();
            showForm(frm);
        }

        private void computedForecastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateForDeliveryProduction frm = new CreateForDeliveryProduction();
            showForm(frm);
            //ComputedForecast frm = new ComputedForecast();
            //showForm(frm);
        }

        private void adjustmentsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void salesTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SalesTab frm = new SalesTab();
            showForm(frm);
        }

        private void targetForDeliveryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ItemRequestTransfer_Tab frm = new ItemRequestTransfer_Tab();
            showForm(frm);
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
         
        }

        private void salesDetailedReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SalesDetailedReport frm = new SalesDetailedReport();
            showForm(frm);
        }

        private void paymentTransactionReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PaymentTransactionReport frm = new PaymentTransactionReport();
            showForm(frm);
        }

        private void shortSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form5 frm = new Form5();
            showForm(frm);
        }

        private void salesDetailedSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SalesReport frm = new SalesReport();
            showForm(frm);
        }

        private void pulloutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Transfer transfer = new Transfer();
            transfer.Text = "Pullout Transactions";
            showForm(transfer);
        }

        private void salesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PendingOrder pendingOrder = new PendingOrder();
            showForm(pendingOrder);
        }

        private void salesNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SalesTab frm = new SalesTab();
            showForm(frm);
        }

        private void salesTestToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            PendingOrder frm = new PendingOrder();
            showForm(frm);
        }

        private void bOMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BOM frm = new BOM();
            showForm(frm);
        }

        private void tEST2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form4 frm = new Form4();
            frm.ShowDialog();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void salesTesttToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PendingOrder frm = new PendingOrder();
            showForm(frm);
        }
    }
    public static class JsonExtensions
    {
        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
        }
    }

    public static class DataTableExtensions
    {
        public static void SetColumnsOrder(this DataTable table, params String[] columnNames)
        {
            int columnIndex = 0;
            try
            {
                foreach (var columnName in columnNames)
                {
                    if (table.Columns.Contains(columnName))
                    {
                        table.Columns[columnName].SetOrdinal(columnIndex);
                        columnIndex++;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message);
            }
        }
    }
}
