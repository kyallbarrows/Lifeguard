namespace Lifeguard
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.linkLabelForgotPassword = new System.Windows.Forms.LinkLabel();
            this.labelLoggedInAs = new System.Windows.Forms.Label();
            this.buttonSignIn = new System.Windows.Forms.Button();
            this.labelErrorMessage = new System.Windows.Forms.Label();
            this.panelLoggedIn = new System.Windows.Forms.Panel();
            this.panelLoggedOut = new System.Windows.Forms.Panel();
            this.labelLoading = new System.Windows.Forms.Label();
            this.buttonSignOut = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shutDownLifeguardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelLoggedIn.SuspendLayout();
            this.panelLoggedOut.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.25F);
            this.textBoxUsername.Location = new System.Drawing.Point(111, 31);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(179, 27);
            this.textBoxUsername.TabIndex = 0;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.25F);
            this.textBoxPassword.Location = new System.Drawing.Point(111, 64);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '●';
            this.textBoxPassword.Size = new System.Drawing.Size(179, 27);
            this.textBoxPassword.TabIndex = 1;
            this.textBoxPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxPassword_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.25F);
            this.label1.Location = new System.Drawing.Point(13, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 22);
            this.label1.TabIndex = 2;
            this.label1.Text = "Username";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.25F);
            this.label2.Location = new System.Drawing.Point(13, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 22);
            this.label2.TabIndex = 3;
            this.label2.Text = "Password";
            // 
            // linkLabelForgotPassword
            // 
            this.linkLabelForgotPassword.AutoSize = true;
            this.linkLabelForgotPassword.Location = new System.Drawing.Point(108, 94);
            this.linkLabelForgotPassword.Name = "linkLabelForgotPassword";
            this.linkLabelForgotPassword.Size = new System.Drawing.Size(91, 13);
            this.linkLabelForgotPassword.TabIndex = 4;
            this.linkLabelForgotPassword.TabStop = true;
            this.linkLabelForgotPassword.Text = "forgot password...";
            // 
            // labelLoggedInAs
            // 
            this.labelLoggedInAs.AutoSize = true;
            this.labelLoggedInAs.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.25F);
            this.labelLoggedInAs.Location = new System.Drawing.Point(14, 12);
            this.labelLoggedInAs.Name = "labelLoggedInAs";
            this.labelLoggedInAs.Size = new System.Drawing.Size(113, 22);
            this.labelLoggedInAs.TabIndex = 5;
            this.labelLoggedInAs.Text = "Logged in as";
            this.labelLoggedInAs.Click += new System.EventHandler(this.label3_Click);
            // 
            // buttonSignIn
            // 
            this.buttonSignIn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.buttonSignIn.Location = new System.Drawing.Point(111, 131);
            this.buttonSignIn.Name = "buttonSignIn";
            this.buttonSignIn.Size = new System.Drawing.Size(115, 32);
            this.buttonSignIn.TabIndex = 6;
            this.buttonSignIn.Text = "Sign In";
            this.buttonSignIn.UseVisualStyleBackColor = true;
            this.buttonSignIn.Click += new System.EventHandler(this.buttonSignIn_Click);
            // 
            // labelErrorMessage
            // 
            this.labelErrorMessage.AutoSize = true;
            this.labelErrorMessage.ForeColor = System.Drawing.Color.DarkRed;
            this.labelErrorMessage.Location = new System.Drawing.Point(14, 10);
            this.labelErrorMessage.Name = "labelErrorMessage";
            this.labelErrorMessage.Size = new System.Drawing.Size(576, 13);
            this.labelErrorMessage.TabIndex = 7;
            this.labelErrorMessage.Text = "Username is required and stuff should go here fdafdafdsafdsafdsafdsafdsafdsafdsaf" +
    "dsafdsafdsafdsafdsafdsafdsafdsafdsa";
            this.labelErrorMessage.Click += new System.EventHandler(this.labelErrorMessage_Click);
            // 
            // panelLoggedIn
            // 
            this.panelLoggedIn.Controls.Add(this.labelLoggedInAs);
            this.panelLoggedIn.Location = new System.Drawing.Point(12, 35);
            this.panelLoggedIn.Name = "panelLoggedIn";
            this.panelLoggedIn.Size = new System.Drawing.Size(308, 185);
            this.panelLoggedIn.TabIndex = 8;
            // 
            // panelLoggedOut
            // 
            this.panelLoggedOut.Controls.Add(this.labelLoading);
            this.panelLoggedOut.Controls.Add(this.labelErrorMessage);
            this.panelLoggedOut.Controls.Add(this.textBoxUsername);
            this.panelLoggedOut.Controls.Add(this.textBoxPassword);
            this.panelLoggedOut.Controls.Add(this.buttonSignIn);
            this.panelLoggedOut.Controls.Add(this.label1);
            this.panelLoggedOut.Controls.Add(this.linkLabelForgotPassword);
            this.panelLoggedOut.Controls.Add(this.label2);
            this.panelLoggedOut.Location = new System.Drawing.Point(12, 35);
            this.panelLoggedOut.Name = "panelLoggedOut";
            this.panelLoggedOut.Size = new System.Drawing.Size(309, 190);
            this.panelLoggedOut.TabIndex = 9;
            // 
            // labelLoading
            // 
            this.labelLoading.AutoSize = true;
            this.labelLoading.ForeColor = System.Drawing.Color.SteelBlue;
            this.labelLoading.Location = new System.Drawing.Point(108, 166);
            this.labelLoading.Name = "labelLoading";
            this.labelLoading.Size = new System.Drawing.Size(54, 13);
            this.labelLoading.TabIndex = 10;
            this.labelLoading.Text = "Loading...";
            // 
            // buttonSignOut
            // 
            this.buttonSignOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.buttonSignOut.Location = new System.Drawing.Point(18, 59);
            this.buttonSignOut.Name = "buttonSignOut";
            this.buttonSignOut.Size = new System.Drawing.Size(115, 32);
            this.buttonSignOut.TabIndex = 9;
            this.buttonSignOut.Text = "Sign Out";
            this.buttonSignOut.UseVisualStyleBackColor = true;
            this.buttonSignOut.Click += new System.EventHandler(this.buttonSignOut_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(359, 24);
            this.menuStrip1.TabIndex = 10;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.shutDownLifeguardToolStripMenuItem});
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // shutDownLifeguardToolStripMenuItem
            // 
            this.shutDownLifeguardToolStripMenuItem.Name = "shutDownLifeguardToolStripMenuItem";
            this.shutDownLifeguardToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.shutDownLifeguardToolStripMenuItem.Text = "Shut Down Lifeguard";
            this.shutDownLifeguardToolStripMenuItem.Click += new System.EventHandler(this.menuItemShutDownClick);
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 263);
            this.Controls.Add(this.panelLoggedIn);
            this.Controls.Add(this.panelLoggedOut);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "LoginForm";
            this.Text = "Login to Lifeguard";
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.panelLoggedIn.ResumeLayout(false);
            this.panelLoggedIn.PerformLayout();
            this.panelLoggedOut.ResumeLayout(false);
            this.panelLoggedOut.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel linkLabelForgotPassword;
        private System.Windows.Forms.Label labelLoggedInAs;
        private System.Windows.Forms.Button buttonSignIn;
        private System.Windows.Forms.Label labelErrorMessage;
        private System.Windows.Forms.Panel panelLoggedIn;
        private System.Windows.Forms.Button buttonSignOut;
        private System.Windows.Forms.Panel panelLoggedOut;
        private System.Windows.Forms.Label labelLoading;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shutDownLifeguardToolStripMenuItem;
    }
}