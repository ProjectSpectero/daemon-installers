using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Windows;

namespace installer
{
    public partial class InstallLocationForm : Form
    {
        public InstallLocationForm()
        {
            InitializeComponent();
        }

        private void InstallLocationForm_Load(object sender, EventArgs e)
        {
            // Set a default path to install
            if (Program.installLocation == null)
                Program.installLocation = Program.GetInstallationPath();

            // Restore the install location.
            InstallLocation.Text = Program.installLocation;
        }

        /// <summary>
        /// Allow the user to change the directory of where to install spectero.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeDirectory_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
            {
                folderBrowser.Description = "Select a folder";
                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    var newPath = Path.Combine(folderBrowser.SelectedPath, "Spectero");

                    Program.installLocation = newPath;
                    InstallLocation.Text = newPath;
                }
            }
        }

        /// <summary>
        /// Go back to the previous form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, EventArgs e)
        {
            new SelectChannelForm().Show();
            this.Close();
        }

        /// <summary>
        /// Go to the next form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextButton_Click(object sender, EventArgs e)
        {
            new InstallForm().Show();
            this.Close();
        }
    }
}