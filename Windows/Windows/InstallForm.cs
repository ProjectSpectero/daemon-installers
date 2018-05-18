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
using installer.Properties;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace installer
{
    public partial class InstallForm : Form
    {
        // Palceholder variables
        private string _zipFilename;
        private string _absoluteZipPath;
        private string _downloadLink;
        private bool _downloaded = false;
        private DateTime _timeStarted;
        private bool _pauseActions = false;
        private long _lastBytesDownlaoded;

        /// <summary>
        /// Class constructor
        /// </summary>
        public InstallForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Form constructor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstallForm_Load(object sender, EventArgs e)
        {
            // allow illegal thread access to the UI, doesn't mater here
            CheckForIllegalCrossThreadCalls = false;
            
            // Show the form and get to work.
            this.Show();

            // Do our work in a thread, so the user has some form of interactivity.
            Thread workerThread = new Thread(Worker);
            workerThread.Start();
        }

        /// <summary>
        /// The thread that will do most of the logic for the form.
        /// </summary>
        private void Worker()
        {
            // Store the download link in an easy to access variable
            _downloadLink = Program.ReleaseInformation["versions"][Program.Version]["download"].ToString();

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
                _downloadLink
            ));

            // Update the progres sbar to an unknown state.
            //OverallProgress.Style = ProgressBarStyle.Marquee;

            // Create shorthand variables to use rather than redundant functions.
            _zipFilename = Program.Version + ".zip";
            _absoluteZipPath = Path.Combine(Program.InstallLocation, _zipFilename);

            DownloadBackgroundWorker.RunWorkerAsync();

            // Waiting for the download to complete.
            while (!_downloaded)
            {
                Thread.Sleep(10);
            }

            // Extract the archive
            ZipFile versionZipFile = new ZipFile(File.OpenRead(_absoluteZipPath));

            // Reset the progress bar.
            OverallProgress.Maximum = int.Parse(versionZipFile.Count.ToString());
            OverallProgress.Value = 0;

            // Iterate through each object in the archive.
            foreach (ZipEntry zipEntry in versionZipFile)
            {
                // Check if we should pause
                while (_pauseActions)
                    Thread.Sleep(10);

                // Get the current absolute path
                string currentPath = Path.Combine(Program.InstallLocation, zipEntry.Name);

                // Create the directory if needed.
                if (zipEntry.IsDirectory)
                {
                    Directory.CreateDirectory(currentPath);
                    EasyLog("Created Directory: " + currentPath);
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
                ProgressText.Text = string.Format("Extracting file {0}/{1}", OverallProgress.Value,
                    OverallProgress.Maximum);

                // Perform all needed UI events.
                Application.DoEvents();
            }

            // Mark as complete and enable the progress bar
            EasyLog("Installation is complete.");
            NextButton.Enabled = true;
        }

        /// <summary>
        /// Helper function to enable easy logging to the output window on the form
        /// </summary>
        /// <param name="appending"></param>
        private void EasyLog(string appending)
        {
            Logger.Text += string.Format("[{0}] {1}\n", DateTime.Now, appending);
        }

        /// <summary>
        /// The worker that will handle the downloading of the daemon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Webclient for file donwloading.
            WebClient webClient = new WebClient();

            _timeStarted = DateTime.Now;

            // Update the progress bar.
            webClient.DownloadProgressChanged += (senderChild, eChild) =>
            {
                OverallProgress.Maximum = int.Parse(eChild.TotalBytesToReceive.ToString());
                OverallProgress.Value = int.Parse(eChild.BytesReceived.ToString());
                ProgressText.Text = string.Format("Downloaded {0}/{1} MiB @ {2} KiB/s",
                    Math.Round(eChild.BytesReceived / Math.Pow(1024, 2), 2),
                    Math.Round(eChild.TotalBytesToReceive / Math.Pow(1024, 2), 2),
                    Math.Round(eChild.BytesReceived / (DateTime.Now - _timeStarted).TotalSeconds / Math.Pow(1024, 1), 2
                    )
                );
            };

            // Define a rule to the webclient to change a boolean when the download is done.
            webClient.DownloadFileCompleted += (senderChild, eChild) =>
            {
                // Set the continuation boolean.
                _downloaded = true;

                // Tell the user where the file was saved.
                EasyLog(string.Format("{0} was saved to {1}", _zipFilename, _absoluteZipPath));
            };

            // Download the file asyncronously.
            webClient.DownloadFileAsync(new Uri(_downloadLink), _absoluteZipPath);
        }

        /// <summary>
        /// Exit/Cancel the installation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitButton_Click(object sender, EventArgs e)
        {
            // Pause the for-each loop.
            _pauseActions = true;

            // Update the progress text to waiting on user interaction
            ProgressText.Text = Resources.wait_user;
            // Prompt the user if they really want to exit
            DialogResult prompt = MessageBox.Show(
                Resources.cancel_installation_prompt,
                Resources.messagebox_title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            // User said yes, stop.
            if (prompt == DialogResult.Yes)
            {
                Program.HarshExit(false);
            }

            // If we didn't get the result we needed, continue.
            _pauseActions = false;
        }

        /// <summary>
        /// The next button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextButton_Click(object sender, EventArgs e)
        {

        }

        private void Logger_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            Logger.SelectionStart = Logger.Text.Length;

            // scroll it automatically
            Logger.ScrollToCaret();
        }
    }
}