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
using installer.Properties;
using Newtonsoft.Json.Linq;
using Environment = System.Environment;

namespace Windows
{
    public partial class WelcomeForm : Form
    {
        WebClient dataClient = new WebClient();

        public WelcomeForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Disable next movement
            NextButton.Enabled = false;

            // Change status text to gathering data
            StatusLabel.Text = Resources.welcome_gathering_information;

            // Download data
            Program.ReleaseInformationJObject = JObject.Parse(dataClient.DownloadString(Resources.spectero_releases_url));
            Program.TermsOfServices = dataClient.DownloadString(Resources.terms_of_service_url);

            // Change status text to ready
            StatusLabel.Text = Resources.welcome_next_text;

            // Enable the button
            NextButton.Enabled = true;
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Resources.exit_message, Resources.messagebox_title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            new TermsOfServiceForm().Show();
            this.Close();
        }
    }
}