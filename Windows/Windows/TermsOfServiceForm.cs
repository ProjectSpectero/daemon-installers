using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using installer;
using installer.Properties;
using Newtonsoft.Json.Linq;
using Environment = System.Environment;

namespace Windows
{
    public partial class TermsOfServiceForm : Form
    {
        public TermsOfServiceForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Disalble the next button by default.
            NextButton.Enabled = false;

            // Enable back button
            BackButton.Enabled = true;

            // Change rich text box license
            EULABox.Text = Program.TermsOfServices;
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Resources.exit_message, Resources.messagebox_title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            new SelectChannelForm().Show();
            this.Close();
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            new WelcomeForm().Show();
            this.Close();
        }

        private void DisagreeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            NextButton.Enabled = false;
        }

        private void AgreeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            NextButton.Enabled = true;
        }

        private void FormClosing_event(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}