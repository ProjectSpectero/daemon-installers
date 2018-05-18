using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using installer.Properties;
using Newtonsoft.Json.Linq;

namespace Windows
{
    static class Program
    {
        /*
         * Installer Varaibles
         * Due to the nature of which the installer can go back and forth,
         * we assign these variables so in the event the user goes back
         * we remember where we left off.
         */
        public static JObject ReleaseInformation; // Get release information from the server.
        public static string TermsOfServices; // Placeholder to store the terms of services for the richtextbox.
        public static string Channel; // Stable, Alpha, Beta
        public static string Version; // The version the user has selected.
        public static string InstallLocation; // The path of where spectero should be installed.
        public static bool CreateService = true; // Where to install spectero
        public static bool InstallSliently = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Download information
            WebClient dataClient = new WebClient();
            Program.ReleaseInformation = JObject.Parse(dataClient.DownloadString(Resources.spectero_releases_url));
            Program.TermsOfServices = dataClient.DownloadString(Resources.terms_of_service_url);

            // Handle the arguments
            HandleArguments();

            new WelcomeForm().Show();
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
            if (CommandLineArgumentExists("--silent")) InstallSliently = true;

            // Channels - Most dangerous to safe.
            if (CommandLineArgumentExists("--alpha")) Channel = "alpha";
            if (CommandLineArgumentExists("--beta")) Channel = "beta";
            if (CommandLineArgumentExists("--stable")) Channel = "stable";

            // Check if the user provided a version
            if (InstallSliently == true && CommandLineArgumentExists("--version"))
            {
                int versionPosition = GetAfterArgument("--version");

            }
            else if (InstallSliently != true && CommandLineArgumentExists("--version"))
            {
                MessageBox.Show(
                    "You attempted to specify a specific version through the command line. This flag is unsupported in GUI mode.",
                    "Spectero Installer", MessageBoxButtons.OK, MessageBoxIcon.Stop
                );
                HarshExit(false);
            }
            else if (InstallSliently == true && !CommandLineArgumentExists("--version"))
            {
                // Determine if we should use the stable by default, or the provided channel.
                Version = (Channel == null)
                    ? ReleaseInformation["channels"]["stable"].ToString() // Use the stable, the user didn't provide.
                    : ReleaseInformation["channels"][Channel].ToString(); // Use the provided channel.
            }
        }

        private static int GetAfterArgument(string passedArg)
        {
            for (var index = 0; index < Environment.GetCommandLineArgs().Length; index++)
            {
                string arg = Environment.GetCommandLineArgs()[index];
                if (arg == passedArg) return index;
            }

            return -1;
        }
    }
}