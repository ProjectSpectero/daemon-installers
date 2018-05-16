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

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void InstallLocationForm_Load(object sender, EventArgs e)
        {
            InstallLocation.Text = Program.GetInstallationPath();
        }

        private void ChangeDirectory_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
            {
                folderBrowser.Description = "Select a folder";
                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    InstallLocation.Text = Path.Combine(folderBrowser.SelectedPath, "Spectero");
                }
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            new SelectChannelForm().Show();
            this.Close();
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(InstallLocation.Text))
                Directory.CreateDirectory(InstallLocation.Text);
        }
    }
}
