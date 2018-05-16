using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Windows;
using installer.Properties;
using Newtonsoft.Json.Linq;
using MessageBox = System.Windows.Forms.MessageBox;

namespace installer
{
    public partial class SelectChannelForm : Form
    {
        public SelectChannelForm()
        {
            InitializeComponent();
        }

        private void SelectChannelForm_Load(object sender, EventArgs e)
        {
            // Disable channels that are null.
            if (Program.ReleaseInformationJObject["channels"]["stable"].Type == JTokenType.Null)
                StableRadio.Enabled = false;

            if (Program.ReleaseInformationJObject["channels"]["beta"].Type == JTokenType.Null)
                BetaRadio.Enabled = false;

            if (Program.ReleaseInformationJObject["channels"]["alpha"].Type == JTokenType.Null)
                AlphaRadio.Enabled = false;

            // Check if we should just exit
            if (!StableRadio.Enabled && !BetaRadio.Enabled && !AlphaRadio.Enabled)
            {
                MessageBox.Show("There are no available releases of spectero. Please try again later.");
                Application.Exit();
            }
            else if (StableRadio.Enabled)
            {
                StableRadio.Select();
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            // Delete all objects since we're a new choice.
            ChannelVersion.Items.Clear();

            // Iterate over each version to provide choice for this channel.
            foreach (var jToken in Program.ReleaseInformationJObject["versions"])
            {
                var currentVersion = (JProperty) jToken;
                ChannelVersion.Items.Add(currentVersion.Name);
            }

            // Select the combobox to the latest value.
            ChannelVersion.SelectedText = Program.ReleaseInformationJObject["channels"]["stable"].ToString();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            new NotImplementedException();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            new NotImplementedException();
        }

        private void ChannelVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.version = ChannelVersion.SelectedText;
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Resources.exit_message, Resources.messagebox_title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            new TermsOfServiceForm().Show();
            this.Close();
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (ChannelVersion.SelectedIndex > -1)
            {
                new InstallLocationForm().Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a version to install.", "Spectero Installer", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}