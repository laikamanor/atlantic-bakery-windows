using DevExpress.XtraGrid.Columns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraGrid.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System.Globalization;

namespace AB.UI_Class
{
    class devexpress_class
    {
        public void changeFont(GridColumn col)
        {
            //fonts
            FontFamily fontArial = new FontFamily("Arial");
            col.AppearanceHeader.Font = new Font(fontArial, 11, FontStyle.Regular);
            col.AppearanceCell.Font = new Font(fontArial, 10, FontStyle.Regular);
        }

       


        public void loadSuggestion(GridView gView, GridControl gControl, string[] suggests)
        {
            if (gView.IsFindPanelVisible)
            {
                string suggestConcat = string.Join(";", suggests);
                gView.OptionsFind.AlwaysVisible = true;
                gView.OptionsFind.FindNullPrompt = "Search " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(suggestConcat.Replace(";", ", ").Replace("_", " ") + "...");
                gView.OptionsFind.FindFilterColumns = suggestConcat;

                List<Control> controls = gControl.Controls.OfType<FindControl>().ToList<Control>();
                if (controls.Count > 0)
                {
                    DataTable dtGrid = (DataTable)gControl.DataSource;
                    AutoCompleteStringCollection auto = loadAutoComplete(dtGrid, suggests);
                    FindControl fControl = controls[0] as FindControl;
                    fControl.FindEdit.MaskBox.AutoCompleteCustomSource = null;
                    fControl.FindEdit.MaskBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    fControl.FindEdit.MaskBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    fControl.FindEdit.MaskBox.AutoCompleteCustomSource = auto;
                }
                else
                {
                    Console.WriteLine("no count");
                }
            }
            else
            {
                Console.WriteLine("not visible");
            }
        }

        public AutoCompleteStringCollection loadAutoComplete(DataTable dt, string[] suggests)
        {
            api_class apic = new api_class();
            string msg = "";
            AutoCompleteStringCollection auto = new AutoCompleteStringCollection();
            try
            {
                if (dt != null)
                {
                    foreach (string suggest in suggests)
                    {
                        Console.WriteLine(suggest);
                        foreach (DataRow row in dt.Rows)
                        {
                            if (!msg.ToLower().Trim().Contains(suggest))
                            {
                                msg += !dt.Columns.Contains(suggest) ? suggest + " column not found!" + Environment.NewLine : "";
                                string val = !dt.Columns.Contains(suggest) ? "" : row[suggest].ToString();
                                if (!string.IsNullOrEmpty(val.Trim()))
                                {
                                    //Console.WriteLine(val);
                                    auto.Add(val);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("dt null");
                }

                if (!string.IsNullOrEmpty(msg.Trim()))
                {
                    apic.showCustomMsgBox("Autocomplete Validation", msg);
                }
            }
            catch (Exception ex)
            {
                apic.showCustomMsgBox(ex.Message, ex.ToString());
            }
            return auto;
        }
    }
}
