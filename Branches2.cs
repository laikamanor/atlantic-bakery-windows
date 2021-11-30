using Newtonsoft.Json.Linq;
using RestSharp;
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
using DevExpress.XtraGrid;
using System;
using System.ComponentModel;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid;
using System.Globalization;
using AB.UI_Class;
using Newtonsoft.Json;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

namespace AB
{
    public partial class Branches2 : Form
    {
        public Branches2()
        {
            InitializeComponent();
            //gridView1.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
        }
        utility_class utilityc = new utility_class();
        devexpress_class devc = new devexpress_class();
        api_class apic = new api_class();
        private async void Branches2_Load(object sender, EventArgs e)
        {
            refresh();
        }

        public void refresh()
        {
            string sResult = apic.loadData("/api/branch/get_all", "", "", "", Method.GET, true);
            if (!string.IsNullOrEmpty(sResult) && sResult.Substring(0, 1).Equals("{"))
            {
                JObject joResponse = JObject.Parse(sResult);
                JArray jaData = (JArray)joResponse["data"];


                DataTable dtData = (DataTable)JsonConvert.DeserializeObject(jaData.ToString(), (typeof(DataTable)));
                if (dtData.Rows.Count > 0)
                {
                    dtData.Columns.Add("edit");
                    dtData.Columns.Add("remove");
                }
                gridControl1.Invoke(new MethodInvoker(delegate
                {
                    gridControl1.DataSource = dtData;
                    foreach (GridColumn col in gridView1.Columns)
                    {
                        string fieldName = col.FieldName;
                        string v = col.GetCaption();
                        string s = col.GetCaption().Replace("_", " ");
                        col.Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                        col.ColumnEdit = fieldName.Equals("edit") ? repositoryItemButtonEdit1 : fieldName.Equals("remove") ? repositoryItemButtonEdit2 : repositoryItemTextEdit1;
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.None;
                        col.DisplayFormat.FormatString = "";
                        col.Visible = !(fieldName.Equals("id") || fieldName.Equals("created_by") || fieldName.Equals("updated_by") || fieldName.Equals("is_active") || fieldName.Equals("date_created") || fieldName.Equals("date_updated"));

                        //fonts
                        //FontFamily fontArial = new FontFamily("Arial");
                        //col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
                        //col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
                        devc.changeFont(col);
                    }
                    gridView1.BestFitColumns();
                    //auto complete
                    string[] suggestions = { "code" };
                    devc.loadSuggestion(gridView1, gridControl1, suggestions);
                    gridView1.OptionsView.ColumnAutoWidth = false;
                    gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
                }));
            }
        }

        private void gridView1_InitNewRow(object sender, DevExpress.XtraGrid.Views.Grid.InitNewRowEventArgs e)
        {
       
        }

        private async void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            string code = gridView1.GetFocusedDataRow()["code"].ToString(), name = gridView1.GetFocusedDataRow()["name"].ToString();
            int id = 0, intTemp = 0;
            id = Int32.TryParse(gridView1.GetFocusedDataRow()["id"].ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedDataRow()["id"].ToString()) : intTemp;
            EditBranch editBranch = new EditBranch();
            editBranch.selectedID = id;
            editBranch.selectedCode = code;
            editBranch.selectedName = name;
            editBranch.ShowDialog();
            if (EditBranch.isSubmit)
            {
                refresh();
            }
        }

        public async void deleteBranch(int id)
        {
            if (Login.jsonResult != null)
            {
                Cursor.Current = Cursors.WaitCursor;
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
                    var request = new RestRequest("/api/branch/delete/" + id);
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.Method = Method.DELETE;
                    Task<IRestResponse> t = client.ExecuteAsync(request);
                    t.Wait();
                    var response = await t;
                    JObject jObject = JObject.Parse(response.Content.ToString());
                    bool isSuccess = false;

                    string msg = "No message response found";
                    foreach (var x in jObject)
                    {
                        if (x.Key.Equals("message"))
                        {
                            msg = x.Value.ToString();
                        }
                    }

                    foreach (var x in jObject)
                    {
                        if (x.Key.Equals("success"))
                        {
                            isSuccess = Convert.ToBoolean(x.Value.ToString());
                            MessageBox.Show(msg, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            refresh();
                        }
                    }

                    if (!isSuccess)
                    {
                        if (msg.Equals("Token is invalid"))
                        {
                            MessageBox.Show("Your login session is expired. Please login again", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private async void btnAddUser_Click(object sender, EventArgs e)
        {
            AddBranch frm = new AddBranch();
            frm.ShowDialog();
            if (AddBranch.isSubmit)
            {
                refresh();
            }
        }

        private async void repositoryItemButtonEdit2_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to remove?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                int id = 0, intTemp = 0;
                id = Int32.TryParse(gridView1.GetFocusedDataRow()["id"].ToString(), out intTemp) ? Convert.ToInt32(gridView1.GetFocusedDataRow()["id"].ToString()) : intTemp;
                await Task.Run(() => deleteBranch(id));
            }
        }

        //private void btnRefresh_Click(object sender, EventArgs e)
        //{
        //    refresh();
        //}

        private void btnRefresh_Click_1(object sender, EventArgs e)
        {
            refresh();
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
            if (e.RowHandle == HotTrackRow)
                e.Appearance.BackColor = gridView1.PaintAppearance.SelectedRow.BackColor;
            else
                e.Appearance.BackColor = e.Appearance.BackColor;
        }
    }
}
