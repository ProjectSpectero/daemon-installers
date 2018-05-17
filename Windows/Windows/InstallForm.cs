using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Windows;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

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

            string downloadLink = Program.ReleaseInformationJObject["versions"][Program.Version]["download"].ToString();

            // Create the installation directory if it doesn't exist.
            if (!Directory.Exists(Program.InstallLocation))
                Directory.CreateDirectory(Program.InstallLocation);
                EasyLog("Created Directory: " + Program.InstallLocation);

            // Tell the user what's going to happen
            EasyLog(string.Format("Downloading version {0} ({1} release) from {2}", 
                Program.Version,
                Program.Channel,
                downloadLink
                ));

            // Change the progress bar style to an unknown state.
            ProgressBar.CheckForIllegalCrossThreadCalls = false;
            OverallProgress.Style = ProgressBarStyle.Marquee;
            
            // Create shorthand variables to use rather than redundant functions.
            string zipFilename = Program.Version + ".zip";
            string absoluteZipPath = Path.Combine(Program.InstallLocation, zipFilename);

            // Download the archive from the download server to some place on the disk.
            webClient.DownloadFile(downloadLink, absoluteZipPath);
            EasyLog(string.Format("{0} was saved to {1}", zipFilename, absoluteZipPath));

            // Change the progress bar style to an known state.
            OverallProgress.Style = ProgressBarStyle.Continuous;

            // Extract the archive
            ZipFile versionZipFile = new ZipFile(File.OpenRead(absoluteZipPath));

            // Update the progress bar
            OverallProgress.Maximum = int.Parse(versionZipFile.Count.ToString());

            foreach (ZipEntry zipEntry in versionZipFile)
            {
                // Get the current absolute path
                string currentPath = Path.Combine(Program.InstallLocation, zipEntry.Name);

                // Create the directory if needed.
                if (zipEntry.IsDirectory)
                {
                    EasyLog("Creating Directory: " + currentPath);
                    Directory.CreateDirectory(currentPath);
                }
                else
                {
                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = versionZipFile.GetInputStream(zipEntry);

                    using (FileStream streamWriter = File.Create(currentPath))
                    {
                        EasyLog("Copying file: " + currentPath);
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }

                OverallProgress.Value += 1;
            }

            // Mark as complete and enable the progress bar
            EasyLog("Installation is complete.");
            NextButton.Enabled = true;
        }

        private void EasyLog(string appending)
        {
            Logger.Text += string.Format("[{0}] {1}\n", DateTime.Now, appending);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
