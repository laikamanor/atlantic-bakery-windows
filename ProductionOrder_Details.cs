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
using RestSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraEditors.Controls;
using System.Reflection;

namespace AB
{
    public partial class ProductionOrder_Details : Form
    {
        public ProductionOrder_Details(int id, string reference)
        {
            InitializeComponent();
            this.id = id;
            this.reference = reference;
        }
        int id = 0;
        string reference = "";
        devexpress_class devc = new devexpress_class();
        utility_class utilityc = new utility_class();
        api_class apic = new api_class();
        ui_class uic = new ui_class();
        private void ProductionOrder_Details_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.abc_logo;
            lblReference.Text = reference;
            bg();
        }

        public void loadData()
        {
            try
            {
                string sParams = id.ToString();
                string sResult = apic.loadData("/api/production/order/details/", sParams, "", "", Method.GET, true);
                if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
                {
                    double runningBalance = 0.00;
                    DateTime dtTemp = new DateTime();
                    JObject joResponse = JObject.Parse(sResult);
                    JArray jaData = joResponse["data"] == null ? new JArray() : (JArray)joResponse["data"];
                    //lblToWhse.Text = jaTransRow[0]["to_whse"].ToString();
                    DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));

                    DataTable dtCloned = new DataTable();
                    if (dtData.Rows.Count > 0)
                    {
                        dtCloned = dtData.Clone();
                        dtCloned.Columns.Add("closee");
                        if (dtCloned.Columns.Contains("close"))
                        {
                            dtCloned.Columns["close"].DataType = typeof(string);
                        }

                        foreach (DataRow row in dtData.Rows)
                        {
                            //if (dtData.Columns.Contains("close"))
                            //{
                            //    string sClosed = row["close"].ToString();
                            //    row["close"] = sClosed.Equals("True") ? "✔" : "";
                            //}
                            dtCloned.ImportRow(row);
                        }
                        if (dtCloned.Rows.Count > 0)
                        {
                            foreach (DataRow row in dtCloned.Rows)
                            {
                                if (dtCloned.Columns.Contains("close"))
                                {
                                    string sClosed = row["close"].ToString();
                                    row["close"] = sClosed.Equals("True") ? "✔" : "";
                                }
                            }
                        }
                    }

                    gridControl1.Invoke(new Action(delegate ()
                    {
                        gridControl1.DataSource = null;
                    }));

                    dtCloned.SetColumnsOrder("item_code", "targeted_qty", "planned_qty", "received_qty","variance", "uom", "whsecode", "close", "date_closed", "remarks", "closee");

                    gridControl1.Invoke(new Action(delegate ()
                    {
                        gridControl1.DataSource = dtCloned;

                        gridView1.FocusedRowHandle = 0;
                        gridView1.FocusedColumn = gridView1.Columns["closee"];

                        gridView1.OptionsView.ColumnAutoWidth = false;
                        gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                        foreach (GridColumn col in gridView1.Columns)
                        {
                            string fieldName = col.FieldName;
                            string v = col.GetCaption();
                            string s = col.GetCaption().Replace("_", " ");
                            col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                            col.Caption = fieldName.Equals("closee") ? "Close" : col.Caption;
                            //col.Caption = fieldName.Equals("amount_in") ? "Sales" : fieldName.Equals("amount_out") ? "Payment" : col.Caption;
                            col.ColumnEdit = fieldName.Equals("remarks") ? repositoryItemMemoEdit1 : fieldName.Equals("closee") ? repositoryItemButtonEdit1 : repositoryItemTextEdit1;
                            col.DisplayFormat.FormatType = fieldName.Equals("targeted_qty") || fieldName.Equals("planned_qty") || fieldName.Equals("received_qty") || fieldName.Equals("variance") ? DevExpress.Utils.FormatType.Numeric : fieldName.Equals("date_closed") ? DevExpress.Utils.FormatType.DateTime : DevExpress.Utils.FormatType.None;
                            col.DisplayFormat.FormatString = fieldName.Equals("targeted_qty") || fieldName.Equals("planned_qty") || fieldName.Equals("received_qty") || fieldName.Equals("variance") ? "n2" : fieldName.Equals("date_closed") ? "yyyy-MM-dd HH:mm:ss" : "";
                            col.Visible = !(fieldName.Equals("id") || fieldName.Equals("doc_id") || fieldName.Equals("created_by") || fieldName.Equals("date_created") || fieldName.Equals("date_updated") || fieldName.Equals("closed_by") || fieldName.Equals("objtype") || fieldName.Equals("updated_by") || fieldName.Equals("targeted_qty"));

                            //fonts
                            FontFamily fontArial = new FontFamily("Arial");
                            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        }
                        //auto complete
                        string[] suggestions = { "item_code" };
                        string suggestConcat = string.Join(";", suggestions);
                        gridView1.OptionsFind.FindFilterColumns = suggestConcat;
                        devc.loadSuggestion(gridView1, gridControl1, suggestions);
                        gridView1.BestFitColumns();
                        var col2 = gridView1.Columns["remarks"];
                        if (col2 != null)
                        {
                            col2.Width = 200;
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }
        private int hotTrackRow = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        private int HotTrackRow
        {
            get
            {
                return hotTrackRow;
            }
            set
            {
                if (hotTrackRow != value)
                {
                    int prevHotTrackRow = hotTrackRow;
                    hotTrackRow = value;
                    gridView1.RefreshRow(prevHotTrackRow);
                    gridView1.RefreshRow(hotTrackRow);

                    if (hotTrackRow >= 0)
                        gridControl1.Cursor = Cursors.Hand;
                    else
                        gridControl1.Cursor = Cursors.Default;
                }
            }
        }
        private void gridView1_MouseMove(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(new Point(e.X, e.Y));

            if (info.InRowCell)
                HotTrackRow = info.RowHandle;
            else
                HotTrackRow = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        }

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            //if (e.RowHandle == HotTrackRow)
            //    e.Appearance.BackColor = gridView1.PaintAppearance.SelectedRow.BackColor;
            //else
            //    e.Appearance.BackColor = e.Appearance.BackColor;
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
                        if (response.Content.Substring(0, 1).Equals("{"))
                        {
                            JObject jObjectResponse = JObject.Parse(response.Content);
                            bool isSubmit = false;
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
                        }
                        else
                        {
                            MessageBox.Show(response.Content.ToString(), "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show(response.ErrorMessage, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }
            }
        }

        private void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("yes i");

        }

        public void executeClose()
        {
            string selectedColumnfieldName = gridView1.FocusedColumn.FieldName;
            if (selectedColumnfieldName.Equals("closee"))
            {
                int id = 0, baseID = 0, intTemp = 0;
                id = int.TryParse(gridView1.GetFocusedRowCellValue("id").ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedRowCellValue("id").ToString()) : intTemp;
                bool isClosed = gridView1.GetFocusedRowCellValue("close") == null ? false : !string.IsNullOrEmpty(gridView1.GetFocusedRowCellValue("close").ToString());
                if (isClosed)
                {
                    string itemCode = gridView1.GetFocusedRowCellValue("item_code").ToString();
                    MessageBox.Show(itemCode + " is already closed!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    Remarks remarks = new Remarks();
                    remarks.ShowDialog();
                    if (Remarks.isSubmit)
                    {
                        string rem = Remarks.rem;
                        JObject joBody = new JObject();
                        joBody.Add("remarks", rem);
                        string URL = "/api/production/order/details/close/" + id;
                        apiPUT(joBody, URL);
                        bg();
                    }
                }
            }
        }

        private void gridView1_RowStyle(object sender, RowStyleEventArgs e)
        {
            if(e.RowHandle >= 0)
            {
                var colVar = gridView1.Columns["variance"];

                if (gridView1.Columns.Contains(colVar))
                {
                    double variance = 0.00, doubleTemp = 0.00;
                    string sVariance = gridView1.GetRowCellValue(e.RowHandle, "variance").ToString();

                    variance = double.TryParse(sVariance, out doubleTemp) ? Convert.ToDouble(sVariance) : doubleTemp;
                    if (variance == 0)
                    {
                        e.Appearance.ForeColor = e.Appearance.ForeColor;
                    }
                    else if (variance < 0)
                    {
                        e.Appearance.ForeColor = Color.Blue;
                    }
                    else
                    {
                        e.Appearance.ForeColor = Color.Red;
                    }
                }
            }
        }

        private void gridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) != Keys.Control)
            {
                GridView view = sender as GridView;
                GridHitInfo hi = view.CalcHitInfo(e.Location);
                if (hi.InRowCell)
                {
                    if (hi.Column.RealColumnEdit.GetType() == typeof(RepositoryItemButtonEdit))
                    {
                        view.FocusedRowHandle = hi.RowHandle;
                        view.FocusedColumn = hi.Column;
                        view.ShowEditor();
                        //force button click 
                        ButtonEdit edit = (view.ActiveEditor as ButtonEdit);
                        Point p = view.GridControl.PointToScreen(e.Location);
                        p = edit.PointToClient(p);
                        EditHitInfo ehi = (edit.GetViewInfo() as ButtonEditViewInfo).CalcHitInfo(p);
                        if (ehi.HitTest == EditHitTest.Button)
                        {
                            executeClose();
                        }
                    }
                }
            }
        }
        //void PerformClick(ButtonEdit editor, ButtonPressedEventArgs e)
        //{
        //    if (editor == null || e == null) return;
        //    MethodInfo mi = typeof(RepositoryItemButtonEdit).GetMethod("RaiseButtonClick",
        //        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        //    if (mi != null)
        //        mi.Invoke(editor.Properties, new object[] { e });
        //}

    }
}
