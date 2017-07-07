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
using System.Threading;

namespace Lifeguard
{
    public partial class LoginForm : Form
    {
        public LoginForm(String currentUsername, Boolean loggedIn, String currentToken, LoginComplete onLogin, LogoutComplete onLogout, ShutdownApplication onShutdown)
        {
            InitializeComponent();

            CurrentUsername = currentUsername;
            LoggedIn = loggedIn;
            CurrentToken = currentToken;

            OnLoginComplete = onLogin;
            OnLogoutComplete = onLogout;
            OnShutdownApplication = onShutdown;

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
        public delegate void ShutdownApplication();
        private ShutdownApplication OnShutdownApplication;

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
            buttonSignIn.Enabled = false;
            SetSigningInState();
            Thread.Sleep(50);
            DoSignIn();
            buttonSignIn.Enabled = true;
            SetReadyToSigningInState();
        }

        private void DoSignIn() { 
            labelErrorMessage.Visible = false;
            labelLoading.Visible = true;

            CurrentUsername = textBoxUsername.Text.Trim();
            var password = textBoxPassword.Text.Trim();

            if (string.IsNullOrEmpty(CurrentUsername))
            {
                labelErrorMessage.Text = "Username is required";
                return;
            }
            if (string.IsNullOrEmpty(password))
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
            LoggedIn = !string.IsNullOrEmpty(CurrentToken);
            if (LoggedIn)
            {
                ConfigureFormState();
                OnLoginComplete(CurrentUsername, CurrentToken);
                ConfigRepo.SaveConfig(CurrentUsername, CurrentToken);
                //wipe out the password, so that when they logout, it will be empty
                textBoxPassword.Text = "";
            }
            else {
                labelErrorMessage.Visible = true;
                //TODO srsly need to get everything into a string table
                labelErrorMessage.Text = "Invalid username or password";
            }
        }

        private void sendLogoutNotification(string token, string machineId, string message) {
            try
            {
                var client = new HttpClient();

                var requestContent = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("machineid", machineId),
                    new KeyValuePair<string, string>("imagedata", message)
                });

                client.DefaultRequestHeaders.Add("Authorization", "Token " + token);
                var response = client.PostAsync(ConfigRepo.GetScreenshotUri(), requestContent).Result;
                Logger.LogError(response.IsSuccessStatusCode.ToString());
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void buttonSignOut_Click(object sender, EventArgs e)
        {
            var config = ConfigRepo.GetConfig();
            sendLogoutNotification(config.Token, config.MachineID, ConfigRepo.LOGOUT_STRING);
            ConfigRepo.SaveConfig("", "");
            LoggedIn = false;
            ConfigureFormState();
            OnLogoutComplete();
        }

        private void menuItemShutDownClick(object sender, EventArgs e)
        {
            var config = ConfigRepo.GetConfig();
            sendLogoutNotification(config.Token, config.MachineID, ConfigRepo.SHUTDOWN_STRING);
            OnShutdownApplication();
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

        private void SetSigningInState() {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.labelLoading.Visible = true;
            }));
        }
        private void SetReadyToSigningInState()
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.labelLoading.Visible = false;
            }));
        }
    }
}
