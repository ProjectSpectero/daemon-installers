using installer.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Windows;

namespace installer
{
    public partial class InstallerOptionForm : Form
    {

        /// <summary>
        /// The function that loads the components.
        /// </summary>
        public InstallerOptionForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handle startup behavior.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstallerOptionForm_Load(object sender, EventArgs e)
        {
            // Fix the icon for the form in the taskbar.
            this.Icon = Resources.DefaultIcon;

            // Check if there's an active installation.
            if (Uninstaller.InstallationExists())
            {
                InstallArea.Enabled = false;
                UninstallArea.Enabled = true;
            }
            else
            {
                UninstallArea.Enabled = false;
                InstallArea.Enabled = true;
            }
        }

        /// <summary>
        /// The object that should perform the instllation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstallArea_Click(object sender, EventArgs e)
        {
            new TermsOfServiceForm().Show();
            this.Close();
        }

        /// <summary>
        /// The object that should unisntall.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UninstallArea_Click(object sender, EventArgs e)
        {
            new Uninstaller();
            UninstallArea.Enabled = false;
            InstallArea.Enabled = true; 
        }

        /// <summary>
        /// The exit button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitButton_Click(object sender, EventArgs e)
        {
            // Prompt the user and make sure they want to exit.
            DialogResult prompt = MessageBox.Show(
                "Are you sure you want to exit the installer?",
                "Spectero Install",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            // User said yes, stop.
            if (prompt == DialogResult.Yes)
            {
                Program.HarshExit(false);
            }
        }
    }
}
