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

        public static JObject ReleaseInformationJObject;
        public static string TermsOfServices;
        public static string channel;
        public static string version;
        public static string installLocation;

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
