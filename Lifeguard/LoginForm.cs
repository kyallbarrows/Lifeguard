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
        public LoginForm()
        {
            InitializeComponent();
            labelErrorMessage.Visible = false;
            ConfigureFormState();
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Lifeguard.TrayIcon.ico");
            this.Icon = new Icon(stream);
        }

        private void OnShowForm(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void SetLoggedInAsText() {
            var username = ConfigRepo.GetUsername();
            labelLoggedInAs.Text = "Logged in as " + username;
        }

        private void ConfigureFormState()
        {
            labelLoading.Visible = false;
            SetLoggedInAsText();

            if (HaveValidToken())
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

        private String GetToken(String username, String password)
        {
            var client = new HttpClient();

            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            // Get the response.
            var response = client.GetAsync("http://lifeguard.pixelheavyindustries.com/wp-json/lifeguard/v1/token").Result;

            // Get the response content.
            HttpContent responseContent = response.Content;

            // Get the stream of the content.
            using (var reader = new StreamReader(responseContent.ReadAsStreamAsync().Result))
            {
                // Write the output.
                //strip the " that seem to come along with the token
                var output = reader.ReadToEndAsync().Result;
                output = output.Replace("\"", "");
                return output;
            }

        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private bool HaveValidToken() {
            var currToken = ConfigRepo.GetToken();
            return (currToken != null && currToken.Length == 36);
        }

        private void buttonSignIn_Click(object sender, EventArgs e)
        {

            labelErrorMessage.Visible = false;
            labelLoading.Visible = true;

            var username = textBoxUsername.Text.Trim();
            var password = textBoxPassword.Text.Trim();

            if (String.IsNullOrEmpty(textBoxUsername.Text))
            {
                labelErrorMessage.Text = "Username is required";
                return;
            }
            if (String.IsNullOrEmpty(textBoxPassword.Text))
            {
                labelErrorMessage.Text = "Password is required";
                return;
            }


            //throw new System.Exception("Got val back from configRepo: " + val);

            var token = GetToken(username, password);

            //invalid tokens come back with length 37
            if (token.Length > 36)
            {
                labelErrorMessage.Visible = true;
                labelErrorMessage.Text = "Invalid username or password";
            }
            else {
                ConfigRepo.StoreConfig(username, token, "");
                SetLoggedInAsText();
                ConfigureFormState();
            }
        }

        private void buttonSignOut_Click(object sender, EventArgs e)
        {
            ConfigRepo.StoreConfig("", "", "");

            ConfigureFormState();
        }

        public void ShowForm() {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }
    }
}
