using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
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
        public static JObject ReleaseInformationJObject; // Get release information from the server.
        public static string TermsOfServices; // Placeholder to store the terms of services for the richtextbox.
        public static string Channel; // Stable, Alpha, Beta
        public static string Version; // The version the user has selected.
        public static string InstallLocation; // The path of where spectero should be installed.
        public static bool CreateService = true; // Where to install spectero

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
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
    }
}
