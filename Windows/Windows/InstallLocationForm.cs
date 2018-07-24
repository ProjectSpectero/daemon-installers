using installer.Properties;
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
        public bool formLoaded = false;

        public InstallLocationForm()
        {
            InitializeComponent();
        }

        private void InstallLocationForm_Load(object sender, EventArgs e)
        {
            // Fix the icon.
            this.Icon = Resources.DefaultIcon;
            
            // Set a default path to install
            if (Program.InstallLocation == null)
                Program.InstallLocation = Program.GetInstallationPath();

            // Restore the install location.
            InstallLocation.Text = Program.InstallLocation;

            // Restore the checkboxes
            InstallAsService.Checked = Program.CreateService;
            addToEnv.Checked = Program.AddToPath;

            formLoaded = true;
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

                    Program.InstallLocation = newPath;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (formLoaded) Program.CreateService = InstallAsService.Checked;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addToEnv_CheckedChanged(object sender, EventArgs e)
        {
            if (formLoaded) Program.AddToPath = addToEnv.Checked;
        }

        /// <summary>
        /// Exit the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitButton_Click(object sender, EventArgs e)
        {
            Program.HarshExit();
        }
    }
}