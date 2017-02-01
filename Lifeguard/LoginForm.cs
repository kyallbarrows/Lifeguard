using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Net.Http;
using System.IO;
using System.Reflection;

namespace Lifeguard
{
    public partial class LoginForm : Form
    {
        public LoginForm(String currentUsername, Boolean loggedIn, String currentToken, LoginComplete onLogin, LogoutComplete onLogout)
        {
            InitializeComponent();

            CurrentUsername = currentUsername;
            LoggedIn = loggedIn;
            CurrentToken = currentToken;

            OnLoginComplete = onLogin;
            OnLogoutComplete = onLogout;

            labelErrorMessage.Visible = false;
            ConfigureFormState();
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Lifeguard.TrayIcon.ico");
            this.Icon = new Icon(stream);
        }

        private String CurrentUsername = "";
        private Boolean LoggedIn = false;
        private String CurrentToken = "";

        public delegate void LoginComplete(String username, String token);
        private LoginComplete OnLoginComplete;
        public delegate void LogoutComplete();
        private LogoutComplete OnLogoutComplete;

        private void OnShowForm(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void SetLoggedInAsText(String username) {
            labelLoggedInAs.Text = "Logged in as " + username;
        }

        private void ConfigureFormState()
        {
            labelLoading.Visible = false;
            SetLoggedInAsText(CurrentUsername);

            if (LoggedIn)
            {
                panelLoggedOut.Visible = false;
                panelLoggedIn.Visible = true;
            }
            else
            {
                panelLoggedOut.Visible = true;
                panelLoggedIn.Visible = false;
            }
        }



        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void buttonSignIn_Click(object sender, EventArgs e)
        {
            DoSignIn();
        }

        private void DoSignIn() { 
            labelErrorMessage.Visible = false;
            labelLoading.Visible = true;

            CurrentUsername = textBoxUsername.Text.Trim();
            var password = textBoxPassword.Text.Trim();

            if (String.IsNullOrEmpty(CurrentUsername))
            {
                labelErrorMessage.Text = "Username is required";
                return;
            }
            if (String.IsNullOrEmpty(password))
            {
                labelErrorMessage.Text = "Password is required";
                return;
            }

            try
            {
                var token = ApiInteractions.GetToken(CurrentUsername, password);
                CurrentToken = token;
            }
            catch (Exception ex) {
                labelErrorMessage.Visible = true;
                //TODO srsly need to get everything into a string table
                labelErrorMessage.Text = ex.Message;
                Logger.LogException(ex);
                return;
            }

            //invalid tokens come back with length 37
            LoggedIn = CurrentToken.Length == 36;
            if (LoggedIn)
            {
                ConfigureFormState();
                OnLoginComplete(CurrentUsername, CurrentToken);
                //wipe out the password, so that when they logout, it will be empty
                textBoxPassword.Text = "";
            }
            else {
                labelErrorMessage.Visible = true;
                //TODO srsly need to get everything into a string table
                labelErrorMessage.Text = "Invalid username or password";
            }
        }

        private void buttonSignOut_Click(object sender, EventArgs e)
        {
            try
            {
                var client = new HttpClient();

                // Create the HttpContent for the form to be posted.
                var requestContent = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("token", CurrentToken),
                    new KeyValuePair<string, string>("image", ConfigRepo.LOGOUT_STRING)
                });

                var response = client.PostAsync(ConfigRepo.GetScreenshotUri(), requestContent).Result;
            }
            catch (Exception ex) {
                Logger.LogException(ex);
            }

            ConfigRepo.SaveConfig("", "", "");
            LoggedIn = false;
            ConfigureFormState();
            OnLogoutComplete();
        }

        public void ShowForm() {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }

        private void textBoxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                DoSignIn();
            }
        }

        private void labelErrorMessage_Click(object sender, EventArgs e)
        {
           
        }
    }
}
