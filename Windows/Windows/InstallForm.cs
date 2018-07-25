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
using System.Diagnostics;
using System.Reflection;

namespace installer
{
    public partial class InstallForm : Form
    {
        // Palceholder variables
        private string _zipFilename;
        private string _installerPath;
        private string _absoluteInstallationPath;
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
            // Fix the icon
            this.Icon = Resources.DefaultIcon;
            
            // Allow illegal thread access to the UI, doesn't mater here
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

            // Genereate an absolute installation path specifically so we can store the daemon here.
            _absoluteInstallationPath = Path.Combine(Program.InstallLocation, "Daemon");
            _installerPath = Path.Combine(Program.InstallLocation, "Installer.exe");

            // Create the base path and absolute path.
            if (!Directory.Exists(Program.InstallLocation)) Directory.CreateDirectory(Program.InstallLocation);
            if (!Directory.Exists(_absoluteInstallationPath)) Directory.CreateDirectory(_absoluteInstallationPath);

            // Check if we're the SxS installer - if not, then copy.
            if (Assembly.GetExecutingAssembly().Location != _installerPath) File.Copy(Assembly.GetExecutingAssembly().Location, _installerPath, true);

            // Create shorthand variables to use rather than redundant functions.
            _zipFilename = Program.Version + ".zip";
            _absoluteZipPath = Path.Combine(Program.InstallLocation, _zipFilename);

            // Download DNCRTs if it doesn't exist or if the old version is obsolete.
            if (!DotNetCore.Exists())
                DotNetCoreInstallSubroutine();
            else if (!DotNetCore.IsVersionCompatable())
                DotNetCoreInstallSubroutine();

            // Download the project files.
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
                    AddToPath(Path.Combine(_absoluteInstallationPath, "latest\\cli\\Tooling"));
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
                    Path.Combine(_absoluteInstallationPath, "latest"),
                    Path.Combine(_absoluteInstallationPath, Program.Version),
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

            // Add the path to the installer to add/remove page in control panel.
            var winins = new WindowsInstaller();
            if (winins.Exists()) winins.CreateEntry(_installerPath);

            // Set markers in registry
            CreateRegistryEntry();

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
                if (!Directory.Exists(nssmInstallPath))
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

                string workingDirectory = Path.Combine(_absoluteInstallationPath, Program.Version);
                if (!Directory.Exists(workingDirectory)) Directory.CreateDirectory(workingDirectory);

                // Iterate through each object in the archive
                foreach (ZipEntry zipEntry in versionZipFile)
                {
                    // Check if we should pause
                    while (_pauseActions)
                        Thread.Sleep(10);

                    // Get the current absolute path
                    string currentPath = Path.Combine(workingDirectory, zipEntry.Name);

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

        public void DotNetCoreInstallSubroutine()
        {
            // Thread signal.
            bool complete = false;

            // Webclient
            WebClient webClient = new WebClient();

            // Remember the directory
            string dotNetInstallerDownloadLink = Program.SourcesInformation["windows"]["dotnet"].ToString();
            string[] brokenUrlStrings = dotNetInstallerDownloadLink.Split('/');
            string dotNetInstallerFilename = brokenUrlStrings[brokenUrlStrings.Length - 1];
            var dotNetInstallerDownloadPath = Path.Combine(Program.InstallLocation, dotNetInstallerFilename);

            // Tell the user what's going to happen
            EasyLog(string.Format("Downloading {0} from {1}",
                dotNetInstallerFilename,
                dotNetInstallerDownloadLink
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
                EasyLog(string.Format("{0} was saved to {1}", dotNetInstallerFilename, dotNetInstallerDownloadPath));

                // TODO: INSTALL SILENTLY
                EasyLog("Installing Microsoft Dotnet Core Dependency...");
                var microsoftVisualCInstaller = Process.Start(dotNetInstallerDownloadPath, "/install /passive /norestart /q");
                microsoftVisualCInstaller.WaitForExit();

                // Change the thread signal.
                complete = true;
            };

            // Download the file asyncronously.
            webClient.DownloadFileAsync(new Uri(dotNetInstallerDownloadLink), dotNetInstallerDownloadPath);

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

        public void CreateRegistryEntry()
        {
            string root = @"SOFTWARE\";
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(root, true))
            {
                if (parent == null)
                    throw new Exception("Uninstall registry key not found.");

                try
                {
                    RegistryKey key = null;
                    try
                    {
                        // Get the key from the registry.
                        key = parent.OpenSubKey("Spectero", true) ?? parent.CreateSubKey("Spectero");

                        // If the key doesn't exist, throw a problem.
                        if (key == null)
                            throw new Exception(String.Format("Unable to create registry entry '{0}\\{1}'", root, "Spectero"));

                        // Set values
                        key.SetValue("InstallationDirectory", _absoluteInstallationPath);
                        key.SetValue("InstalledVersion", Program.Version);
                        key.SetValue("InstalledChannel", Program.Channel);
                    }
                    finally
                    {
                        // If data is written, close the stream.
                        if (key != null)
                            key.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred writing information to the registry.", ex);
                }
            }
        }
    }
}