using installer;
using installer.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Windows
{
    internal static class Program
    {
        // Kernel Functions
        [DllImport("kernel32.dll")]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        extern static bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool isWow64);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern static IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        extern static IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        extern static IntPtr GetProcAddress(IntPtr hModule, string methodName);

        /*
         * Symlink Enumerator
         * Used to define which type resolves to an integer.
         */
        public enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        /*
         * Installer Varaibles
         * Due to the nature of which the installer can go back and forth,
         * we assign these variables so in the event the user goes back
         * we remember where we left off.
         */
        public static JObject ReleaseInformation; // Get release information from the server.
        public static JObject SourcesInformation;

        public static string TermsOfServices; // Placeholder to store the terms of services for the richtextbox.
        public static string Channel; // Stable, Alpha, Beta
        public static string Version; // The version the user has selected.
        public static string InstallLocation; // The path of where spectero should be installed.
        public static bool CreateService = true; // Should we install spectero as a service?
        public static bool AddToPath = false;
        public static bool InstallSliently = false; // Should the installer run silently?
        public static string DotnetPath;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Try to get the release data
            WebClient dataClient = new WebClient();
            try
            {
                Program.ReleaseInformation = JObject.Parse(dataClient.DownloadString(Resources.spectero_releases_url));
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    string.Format("{0}\n{1}", Resources.release_data_error, e),
                    Resources.messagebox_title,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );
                HarshExit();
            }

            // Try to get data from the sources url
            try
            {
                Program.SourcesInformation = JObject.Parse(dataClient.DownloadString(Resources.sources_url));
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    string.Format("{0}\n{1}", Resources.release_data_error, e),
                    Resources.messagebox_title,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );
                HarshExit();
            }

            // Try to get the terms of services.
            try
            {
                Program.TermsOfServices = dataClient.DownloadString(SourcesInformation["terms-of-service"].ToString());
            }
            catch (Exception ex2)
            {
                Program.TermsOfServices += "Failed to download Terms of Service\n" + ex2;
            }

            // Handle the arguments
            HandleArguments();

            // Should we install silently?
            if (!InstallSliently)
            {
                ValidateAllChannelAvailability();
                new WelcomeForm().Show();
            }
            else
            {
                // Make sure we can use the current channel.
                ValidateChhannelAvailability();
                new InstallForm().Worker();
            }

            Application.Run();
        }

        /// <summary>
        /// Check for a specified command line argument
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static bool CommandLineArgumentExists(string argument)
        {
            return Environment.GetCommandLineArgs().Any(currentArgument => currentArgument.Contains(argument));
        }

        /// <summary>
        /// Determine the folder that we should install in by deafult, it depends on the bits of the system.
        /// </summary>
        /// <returns></returns>
        public static string GetInstallationPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Spectero");
        }

        /// <summary>
        /// Agressive workaround since Application.exit doesn't work.
        /// </summary>
        public static void HarshExit(bool prompt = true)
        {
            if (prompt)
            {
                if (MessageBox.Show(Resources.exit_message, Resources.messagebox_title, MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.Yes)
                    Process.GetCurrentProcess().Kill();
            }
            else
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        private static void HandleArguments()
        {
            // Check if we should install silently.
            if (CommandLineArgumentExists("--silent"))
                InstallSliently = true;

            // Channels - Most dangerous to safe.
            if (CommandLineArgumentExists("--alpha"))
                Channel = "alpha";

            if (CommandLineArgumentExists("--beta"))
                Channel = "beta";

            if (CommandLineArgumentExists("--stable"))
                Channel = "stable";

            // If a channel wasn't provided, use stable by deafult.
            if (InstallSliently && Channel == null)
                Channel = "stable";

            // Check if the user provided a version
            if (InstallSliently && CommandLineArgumentExists("--version"))
            {
                Version = Environment.GetCommandLineArgs()[GetAfterArgument("--version")];
            }
            else if (!InstallSliently && CommandLineArgumentExists("--version"))
            {
                MessageBox.Show(
                    Resources.version_not_silent,
                    Resources.messagebox_title, MessageBoxButtons.OK, MessageBoxIcon.Stop
                );
                HarshExit(false);
            }
            else if (InstallSliently && !CommandLineArgumentExists("--version"))
            {
                // Determine if we should use the stable by default, or the provided channel.
                Version = (Channel == null)
                    ? ReleaseInformation["channels"]["stable"].ToString() // Use the stable, the user didn't provide.
                    : ReleaseInformation["channels"][Channel].ToString(); // Use the provided channel.
            }

            // Check for install location
            if (InstallSliently == true && CommandLineArgumentExists("--install-path"))
            {
                InstallLocation = Environment.GetCommandLineArgs()[GetAfterArgument("--install-path")];
            }
            else if (InstallSliently == true && !CommandLineArgumentExists("--install-path"))
            {
                InstallLocation = GetInstallationPath();
            }

            // Check if we should install the service
            if (CommandLineArgumentExists("--service"))
                CreateService = true;
        }

        /// <summary>
        /// Get the next argument after the passed.
        /// This is a quick way to determine the --version and --install-path
        /// </summary>
        /// <param name="passedArg"></param>
        /// <returns></returns>
        private static int GetAfterArgument(string passedArg)
        {
            for (var index = 0; index < Environment.GetCommandLineArgs().Length; index++)
                if (Environment.GetCommandLineArgs()[index + 1] == passedArg)
                    return index;

            HarshExit(false);
            return -1;
        }

        /// <summary>
        /// Make sure the release channel is available.
        /// </summary>
        private static void ValidateChhannelAvailability()
        {
            if (ReleaseInformation["channels"][Channel].Type == JTokenType.Null)
            {
                MessageBox.Show("Spectero has not released a version for this release channel.", "Spectero Installer",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                HarshExit(false);
            }
        }

        private static void ValidateAllChannelAvailability()
        {
            if (ReleaseInformation["channels"]["stable"].Type == JTokenType.Null &&
                ReleaseInformation["channels"]["beta"].Type == JTokenType.Null &&
                ReleaseInformation["channels"]["alpha"].Type == JTokenType.Null)
            {
                MessageBox.Show("There are no available releases of Spectero.", "Spectero Installer",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static bool Is64BitProcess
        {
            get { return IntPtr.Size == 8; }
        }

        public static bool Is64BitOperatingSystem
        {
            get
            {
                // Clearly if this is a 64-bit process we must be on a 64-bit OS.
                if (Is64BitProcess)
                    return true;
                // Ok, so we are a 32-bit process, but is the OS 64-bit?
                // If we are running under Wow64 than the OS is 64-bit.
                bool isWow64;
                return ModuleContainsFunction("kernel32.dll", "IsWow64Process") &&
                       IsWow64Process(GetCurrentProcess(), out isWow64) && isWow64;
            }
        }

        static bool ModuleContainsFunction(string moduleName, string methodName)
        {
            IntPtr hModule = GetModuleHandle(moduleName);
            if (hModule != IntPtr.Zero)
                return GetProcAddress(hModule, methodName) != IntPtr.Zero;
            return false;
        }

    }
}