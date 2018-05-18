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
        // Palceholder variables
        private string zipFilename;
        private string absoluteZipPath;
        private string downloadLink;
        private bool downloaded = false;

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
            // Store the download link in an easy to access variable
             downloadLink = Program.ReleaseInformationJObject["versions"][Program.Version]["download"].ToString();

            // Create the installation directory if it doesn't exist.
            if (!Directory.Exists(Program.InstallLocation))
            {
                Directory.CreateDirectory(Program.InstallLocation);
                EasyLog("Created Directory: " + Program.InstallLocation);
            }

            // Tell the user what's going to happen
            EasyLog(string.Format("Downloading version {0} ({1} release) from {2}", 
                Program.Version,
                Program.Channel,
                downloadLink
                ));

            // Update the progres sbar to an unknown state.
            //OverallProgress.Style = ProgressBarStyle.Marquee;
            
            // Create shorthand variables to use rather than redundant functions.
            zipFilename = Program.Version + ".zip";
            absoluteZipPath = Path.Combine(Program.InstallLocation, zipFilename);

            DownloadBackgroundWorker.RunWorkerAsync();

            // Waiting for the download to complete.
            while (!downloaded)
            {
                Thread.Sleep(10);
            }
            EasyLog(string.Format("{0} was saved to {1}", zipFilename, absoluteZipPath));

            // Extract the archive
            ZipFile versionZipFile = new ZipFile(File.OpenRead(absoluteZipPath));

            // Reset the progress bar.
            OverallProgress.Maximum = int.Parse(versionZipFile.Count.ToString());
            OverallProgress.Value = 0;
            
            // Iterate through each object in the archive.
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
                // Copy the file to the directory.
                else
                {
                    // Use a buffer, 4096 bytes seems to be pretty optimal.
                    byte[] buffer = new byte[4096];    
                    Stream zipStream = versionZipFile.GetInputStream(zipEntry);

                    // Copy to and from the buffer, and then to the disk.
                    using (FileStream streamWriter = File.Create(currentPath))
                    {
                        EasyLog("Copying file: " + currentPath);
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }

                // Update the progress bar.
                OverallProgress.Value += 1;

                // Update the progress text.
                ProgressText.Text = string.Format("files Extracted: {0}/{1}", OverallProgress.Value, OverallProgress.Maximum);

                Application.DoEvents();
            }

            // Mark as complete and enable the progress bar
            EasyLog("Installation is complete.");
            NextButton.Enabled = true;
        }

        private void EasyLog(string appending)
        {
            Logger.Text += string.Format("[{0}] {1}\n", DateTime.Now, appending);
        }

        private void DownloadBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Webclient for file donwloading.
            WebClient webClient = new WebClient();

            // Update the progress bar.
            webClient.DownloadProgressChanged += (senderChild, eChild) =>
            {
                OverallProgress.Maximum = int.Parse(eChild.TotalBytesToReceive.ToString());
                OverallProgress.Value = int.Parse(eChild.BytesReceived.ToString());
                ProgressText.Text = string.Format("Downloaded {0}/{1} kB", 
                    eChild.BytesReceived / 1024,
                    eChild.TotalBytesToReceive / 1024);
            };

            // Define a rule to the webclient to change a boolean when the download is done.
            webClient.DownloadFileCompleted += (senderChild, eChild) =>
            {
                downloaded = true;
            };

            // Download the file asyncronously.
            webClient.DownloadFileAsync(new Uri(downloadLink), absoluteZipPath);
        }
    }
}
