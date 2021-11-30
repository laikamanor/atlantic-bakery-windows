﻿using System;
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
namespace AB
{
    public partial class changePassword : Form
    {
        public changePassword()
        {
            InitializeComponent();
        }
        api_class apic = new api_class();
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNewPassword.Text.Trim()))
            {
                apic.showCustomMsgBox("Validation", "New Password field is required!");
                txtNewPassword.Focus();
            }
            else if (string.IsNullOrEmpty(txtReTypePassword.Text.Trim()))
            {
                apic.showCustomMsgBox("Validation", "Re-Type Password field is required!");
                txtReTypePassword.Focus();
            }
            else if (txtNewPassword.Text.Length <= 5)
            {
                apic.showCustomMsgBox("Validation", "New Password field must be atlest 6 characters!");
                txtNewPassword.Focus();
            }
            else if (!txtNewPassword.Text.Trim().Equals(txtReTypePassword.Text.Trim()))
            {
                apic.showCustomMsgBox("Validation", "Password did not match!");
                txtReTypePassword.Focus();
            }
            else
            {
                bg(backgroundWorker1);
            }
        }


        public void bg(BackgroundWorker bgw)
        {
            if (!bgw.IsBusy)
            {
                closeForm();
                Loading frm = new Loading();
                frm.Show();
                bgw.RunWorkerAsync();
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

        public string delegateControl(Control c, string s)
        {
            string result = "";
            c.Invoke(new Action(delegate ()
            {
                result = s;
            }));
            return result;
        }

        public void submit()
        {
            JObject joBody = new JObject();
            joBody.Add("password", delegateControl(txtReTypePassword, txtReTypePassword.Text.Trim()));
            string sResult = apic.loadData("/api/user/change_pass", "", "application/json", joBody.ToString(), RestSharp.Method.PUT, true);
            if (!string.IsNullOrEmpty(sResult.Trim()) && sResult.Substring(0, 1).Equals("{"))
            {
                JObject joResult = JObject.Parse(sResult);
                string msg = (string)joResult["message"];
                bool isSuccess = false, boolTemp = false;
                isSuccess = bool.TryParse((string)joResult["success"], out boolTemp) ? Convert.ToBoolean((string)joResult["success"]) : boolTemp;
                apic.showCustomMsgBox(isSuccess ? "Message" : "Validation", msg);
                //MessageBox.Show(msg, isSuccess ? "Message" : "Validation", MessageBoxButtons.OK, isSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                if (isSuccess)
                {
                    this.Invoke(new Action(delegate ()
                    {
                        this.Close();
                    }));
                }
            }
        }

        private void checkBoxNewPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtNewPassword.PasswordChar = checkBoxNewPassword.Checked ? '\0' : '*';
        }

        private void checkBoxReTypePassword_CheckedChanged(object sender, EventArgs e)
        {
            txtReTypePassword.PasswordChar = checkBoxReTypePassword.Checked ? '\0' : '*';
        }

        private void txtNewPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Enter))
            {
                btnSubmit.PerformClick();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            submit();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closeForm();
        }
    }
}
