using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Windows;

namespace installer
{
    public partial class InstallForm : Form
    {
        public InstallForm()
        {
            InitializeComponent();
        }

        private void InstallForm_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            this.Show();

            // Do our work in a thread, so the user has some form of interactivity.
            Thread workerThread = new Thread(Worker);
            workerThread.Start();
        }

        private void Worker()
        {
            // Webclient for file donwloading.
            WebClient webClient = new WebClient();

            // Store the download link in an easy to access variable

            string downloadLink = Program.ReleaseInformationJObject["versions"][Program.version]["download"].ToString();

            // Create the installation directory if it doesn't exist.
            if (!Directory.Exists(Program.installLocation))
                Directory.CreateDirectory(Program.installLocation);
                EasyLog("Created Directory: " + Program.installLocation);

            // Tell the user what's going to happen
            EasyLog(string.Format("Downloading version {0} ({1} release) from {2}", 
                Program.version,
                Program.channel,
                downloadLink
                ));

            // Change the progress bar style to an unknown state.
            progressBar1.Style = ProgressBarStyle.Marquee;
            
            // Create shorthand variables to use rather than redundant functions.
            string zipFilename = Program.version + ".zip";
            string absoluteZipPath = Path.Combine(Program.installLocation, zipFilename);
            string versionDirectory = Path.Combine(Program.installLocation, Program.version);

            // Download the archive from the download server to some place on the disk.
            webClient.DownloadFile(downloadLink, absoluteZipPath);
            EasyLog(string.Format("{0} was saved to {1}", zipFilename, absoluteZipPath));

            // Change the progress bar style to an known state.
            progressBar1.Style = ProgressBarStyle.Continuous;

            // Create Version Directory
            if (!Directory.Exists(versionDirectory))
            {
                EasyLog("Created version directory: " + versionDirectory);
                Directory.CreateDirectory(versionDirectory);
            }

            // TODO: continue
        }

        private void EasyLog(string appending)
        {
            Logger.Text = $"[{DateTime.Now}] {appending}\n{Logger.Text}";
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
