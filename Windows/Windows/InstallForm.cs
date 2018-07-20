using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using installer.Properties;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Windows;
using Microsoft.Win32;

namespace installer
{
    public partial class InstallForm : Form
    {
        // Palceholder variables
        private string _zipFilename;

        private string _absoluteZipPath;
        private string _downloadLink;
        private DateTime _timeStarted;
        private bool _pauseActions = false;
        private long _lastBytesDownlaoded;
        private string nssmInstallPath;

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
        public void Worker()
        {
            // Store the download link in an easy to access variable
            _downloadLink = Program.ReleaseInformation["versions"][Program.Version]["download"].ToString();

            // Create shorthand variables to use rather than redundant functions.
            _zipFilename = Program.Version + ".zip";
            _absoluteZipPath = Path.Combine(Program.InstallLocation, _zipFilename);

            // Download .NET core if it doesn't exist.
            DotNetCoreDownloaderSubworker();

            // Download the files.
            SpecteroDownloaderSubworker();

            // Check service things.
            try
            {
                if (Program.CreateService)
                {
                    NonSuckingServiceManagerSubworker();
                    ServiceManager sm = new ServiceManager(nssmInstallPath);
                    if (sm.Exists())
                    {
                        EasyLog("Service already exists - will be updated.");
                        sm.Stop();
                        EasyLog("Stopped old service...");
                        // Update the service
                        sm.Delete();
                        EasyLog("The old spectero.daemon service has been deleted.");
                    }
                    // Create the service.
                    sm.Create();
                    EasyLog("Created widnows service: spectero.daemon.");
                    sm.Start();
                }
            }
            catch (Exception exception)
            {
                EasyLog("An exception occured while trying to create the service.\n" + exception);
            }

            // Try to add to path
            try
            {
                // Check to see if we should add to the path of the system.
                if (Program.AddToPath)
                {
                    AddToPath(Path.Combine(Program.InstallLocation, "latest\\cli\\Tooling"));
                }
            }
            catch (Exception exception)
            {
                EasyLog("An exception occured while adding to PATH.\n" + exception);
            }

            // Try to create a symbolic link.
            try
            {
                Program.CreateSymbolicLink(
                    Path.Combine(Program.InstallLocation, "latest"),
                    Path.Combine(Program.InstallLocation, Program.Version),
                    Program.SymbolicLink.Directory
                );
            }
            catch (Exception exception)
            {
                EasyLog("An exception occured while creating a symbolic link.\n" + exception);
            }

            // Disable the cancel button.
            ExitButton.Enabled = false;
            
            // Modify registry
            EnableFirewallFormwarding();

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
            new FinishForm().Show();
            this.Close();
        }

        private void Logger_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            Logger.SelectionStart = Logger.Text.Length;

            // scroll it automatically
            Logger.ScrollToCaret();
        }

        public void DotNetCoreDownloaderSubworker()
        {
            // Thread signal.
            bool complete = false;

            // Webclient
            WebClient webClient = new WebClient();

            // Remember the directory
            const string zipName = "dotnet-binary.zip";
            var dotnetInstallationPath = Path.Combine(Program.InstallLocation, "dotnet");
            var dotnetZipPath = Path.Combine(Program.InstallLocation, zipName);
            bool forceInstall = false;
            
            dotnet_force:

            if (!DotNetCore.Exists() || forceInstall)
            {
                // Make the directory if it doesn't exist.
                if (!Directory.Exists(dotnetInstallationPath))
                    Directory.CreateDirectory(dotnetInstallationPath);

                // Tell the user what's going to happen
                EasyLog(string.Format("Downloading {0} from {1}",
                    zipName,
                    DotNetCore.GetDownloadLinkFromArch()
                ));

                // Start the download stopwatch.
                _timeStarted = DateTime.Now;

                // Update the progress bar.
                webClient.DownloadProgressChanged += (senderChild, eChild) =>
                {
                    OverallProgress.Maximum = int.Parse(eChild.TotalBytesToReceive.ToString());
                    OverallProgress.Value = int.Parse(eChild.BytesReceived.ToString());
                    ProgressText.Text = string.Format("Downloaded {0}/{1} MiB @ {2} KiB/s",
                        Math.Round(eChild.BytesReceived / Math.Pow(1024, 2), 2),
                        Math.Round(eChild.TotalBytesToReceive / Math.Pow(1024, 2), 2),
                        Math.Round(
                            eChild.BytesReceived / (DateTime.Now - _timeStarted).TotalSeconds / Math.Pow(1024, 1), 2
                        )
                    );
                };

                // Define a rule to the webclient to change a boolean when the download is done.
                webClient.DownloadFileCompleted += (senderChild, eChild) =>
                {
                    // Tell the user where the file was saved.
                    EasyLog(string.Format("{1} runtime was saved to {0}", dotnetZipPath, zipName));

                    // Extract the archive
                    ZipFile versionZipFile = new ZipFile(File.OpenRead(dotnetZipPath));

                    // Reset the progress bar.
                    OverallProgress.Maximum = int.Parse(versionZipFile.Count.ToString());
                    OverallProgress.Value = 0;

                    // Iterate through each object in the archive
                    foreach (ZipEntry zipEntry in versionZipFile)
                    {
                        // Check if we should pause
                        while (_pauseActions)
                            Thread.Sleep(10);

                        // Get the current absolute path
                        string currentPath = Path.Combine(dotnetInstallationPath, zipEntry.Name);

                        // Create the directory if needed.
                        if (zipEntry.IsDirectory)

                        {
                            Directory.CreateDirectory(currentPath);
                            EasyLog("Created Directory: " + currentPath);
                        }
                        // Copy the file to the directory.
                        else
                        {
                            // Redundant path checking
                            string basepath = new FileInfo(currentPath).Directory.FullName;
                            if (!Directory.Exists(basepath))
                            {
                                Directory.CreateDirectory(basepath);
                            }

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
                    }

                    // Assign where dotnet is.
                    Program.DotnetPath = Path.Combine(dotnetInstallationPath, "dotnet.exe");

                    // ADd the installation path to the PATH varaible.
                    AddToPath(dotnetInstallationPath);

                    // Mark the process as complete.
                    complete = true;
                };

                // Download the file asyncronously.
                webClient.DownloadFileAsync(new Uri(DotNetCore.GetDownloadLinkFromArch()), dotnetZipPath);

                while (webClient.IsBusy || !complete)
                {
                    Thread.Sleep(1);
                }
            }
            else
            {
                if (DotNetCore.IsVersionCompatable())
                {
                    Program.DotnetPath = DotNetCore.GetDotnetPath();
                } else
                {
                    forceInstall = true;
                    goto dotnet_force;
                }
            }
        }

        public void NonSuckingServiceManagerSubworker()
        {
            // Thread signal.
            bool complete = false;

            // Webclient
            WebClient webClient = new WebClient();

            // Remember the directory
            string nssmDownloadLink = Program.SourcesInformation["windows"]["nssm"].ToString();
            string[] brokenUrlStrings = nssmDownloadLink.Split('/');
            string zipName = brokenUrlStrings[brokenUrlStrings.Length - 1];
            nssmInstallPath = Path.Combine(Program.InstallLocation, zipName.Substring(0, zipName.Length - 4));
            var nssmZipPath = Path.Combine(Program.InstallLocation, zipName);

            // Tell the user what's going to happen
            EasyLog(string.Format("Downloading {0} from {1}",
                zipName,
                nssmDownloadLink
            ));

            // Start the stopwatch.
            _timeStarted = DateTime.Now;

            // Download Percent Changed Event - Update the progress bar
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

            // Download Complete Event
            webClient.DownloadFileCompleted += (senderChild, eChild) =>
            {
                // Tell the user where the file was saved.
                EasyLog(string.Format("{0} was saved to {1}", zipName, nssmZipPath));

                // Create the installation directory if it doesn't exist.
                if (!Directory.Exists(Program.InstallLocation))
                {
                    Directory.CreateDirectory(nssmInstallPath);
                    EasyLog("Created Directory: " + nssmInstallPath);
                }

                // Extract the archive
                ZipFile versionZipFile = new ZipFile(File.OpenRead(nssmZipPath));

                // Reset the progress bar.
                OverallProgress.Maximum = int.Parse(versionZipFile.Count.ToString());
                OverallProgress.Value = 0;

                // Iterate through each object in the archive
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
                }

                complete = true;
            };

            // Download the file asyncronously.
            webClient.DownloadFileAsync(new Uri(nssmDownloadLink), nssmZipPath);

            while (webClient.IsBusy || !complete)
            {
                Thread.Sleep(1);
            }
        }

        public void SpecteroDownloaderSubworker()
        {
            // Thread signal.
            bool complete = false;

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

            // Webclient for file donwloading.
            WebClient webClient = new WebClient();

            // Start the stopwatch.
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
                // Tell the user where the file was saved.
                EasyLog(string.Format("{0} was saved to {1}", _zipFilename, _absoluteZipPath));

                // Extract the archive
                ZipFile versionZipFile = new ZipFile(File.OpenRead(_absoluteZipPath));

                // Reset the progress bar.
                OverallProgress.Maximum = int.Parse(versionZipFile.Count.ToString());
                OverallProgress.Value = 0;

                string workingDirecotry = Path.Combine(Program.InstallLocation, Program.Version);
                if (!Directory.Exists(workingDirecotry)) Directory.CreateDirectory(workingDirecotry);

                // Iterate through each object in the archive
                foreach (ZipEntry zipEntry in versionZipFile)
                {
                    // Check if we should pause
                    while (_pauseActions)
                        Thread.Sleep(10);

                    // Get the current absolute path
                    string currentPath = Path.Combine(workingDirecotry, zipEntry.Name);

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
                }

                complete = true;
            };

            // Download the file asyncronously.
            webClient.DownloadFileAsync(new Uri(_downloadLink), _absoluteZipPath);

            while (webClient.IsBusy || !complete)
            {
                Thread.Sleep(1);
            }
        }

        public void AddToPath(string pathToAdd)
        {
            // PATH Variable
            const string name = "PATH";

            // Get the previous value for the PATH.
            string currentEnvironmentVariableValue = Environment.GetEnvironmentVariable(name);

            // If it doesn't already exist, add.
            if (!currentEnvironmentVariableValue.Contains(pathToAdd))
            {
                EasyLog("Adding Spectero CLI to PATH...");
                string newValue = currentEnvironmentVariableValue + @";" + pathToAdd;
                Environment.SetEnvironmentVariable(name, newValue, EnvironmentVariableTarget.User);
            }
            else
            {
                EasyLog("Spectero CLI already exists in the PATH.");
            }
        }

        /// <summary>
        /// Enable firewall forwarding for routing, this is crucial for the daemon's network with OpenVPN
        /// </summary>
        public void EnableFirewallFormwarding()
        {
            Registry.SetValue(
                "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\services\\Tcpip\\Parameters",
                "IpEnableRouter",
                1
            );
        }
    }
}