using System;
using System.Windows.Forms;
using RestSharp;
using Newtonsoft.Json.Linq;
using AB.UI_Class;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using AB.UI_Class;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AB
{
    public partial class Login : Form
    {
        public static JObject jsonResult = new JObject();
        UI_Class.utility_class utilityc = new utility_class();
        api_class apic = new api_class();
        public static string fullName = "";
        public Login()
        {
            InitializeComponent();
        }
        bool is32Bit = false;

        private async void btnLogin_Click(object sender, EventArgs e)
        {
           try
            {
                var strContent = utilityc.getTextfromGithub(utilityc.abWindowsVersionFile);
                Console.WriteLine(strContent + "//" + utilityc.versionName);
                if (strContent.Trim() != utilityc.versionName)
                {
                    txtUsername.Enabled = txtPassword.Enabled = btnLogin.Enabled = false;
                    progressBarsss.Visible  = true;
                    linkLabel1.Visible = false;
                   DialogResult dialogResult = MessageBox.Show("There is a version (v" + strContent.Trim() + ") available do you want to update now?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.Yes)
                    {
                        string URL = "";
                        DialogResult dialogResult2 = MessageBox.Show("Do you want to install 32 bit (Click Yes) or 64 bit (Click No)?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        is32Bit = dialogResult2.Equals(DialogResult.Yes) ? true : false;
                        URL = dialogResult2.Equals(DialogResult.Yes) ? utilityc.githubDownload32FileLink : utilityc.githubDownload64FileLink;
                        using (var client = new WebClient())
                        {
                            string path = utilityc.localDirectoryFolder;
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            client.DownloadProgressChanged += wc_DownloadProgressChanged;
                            client.DownloadFileAsync(new Uri(URL), is32Bit ? utilityc.localDirectory32Exe : utilityc.localDirectory64Exe);
                        }
                    }
                    else
                    {
                        txtUsername.Enabled = txtPassword.Enabled = btnLogin.Enabled = true;
                        progressBarsss.Visible = false;
                        linkLabel1.Visible = true;
                    }
                }
                else
                {
                    //string allLines = System.IO.File.ReadAllText("resources.txt").Trim();
                    login();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void login()
        {
            string sURL = "/api/auth/login",
                sURLParams= "?username=" + txtUsername.Text.ToString().Trim() + "&password=" + txtPassword.Text.ToString().Trim();
            string result= apic.loadData(sURL, sURLParams, "", "", Method.GET,false);
            if (!string.IsNullOrEmpty(result.Trim()))
            {
                jsonResult = JObject.Parse(result);
                string s = (string)jsonResult["message"];
                fullName = (string)jsonResult["data"]["fullname"];

                MessageBox.Show(s, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Action action = () => this.Hide();
                this.BeginInvoke(action);
                MainMenu mainMenu = new MainMenu();
                mainMenu.ShowDialog();
            }
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 100)
            {
                DialogResult dialogResult = MessageBox.Show("Downloaded", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (dialogResult == DialogResult.OK)
                {
                    try
                    {
                        Process.Start(is32Bit ? utilityc.localDirectory32Exe : utilityc.localDirectory64Exe);
                        this.Dispose();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message, " Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            else
            {
                progressBarsss.Value = e.ProgressPercentage;
            }
        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin.PerformClick();
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin.PerformClick();
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {
            linkLabel1.Text = utilityc.URL;
            this.Text = "Login - v" + utilityc.versionName;
        }

        private void Login_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void checkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = checkShowPassword.Checked ? '\0' : '*';
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Read_URL frm = new Read_URL();
            frm.ShowDialog();
            linkLabel1.Text= System.IO.File.ReadAllText("URL.txt");
            txtUsername.Focus();
        }
    }
}
