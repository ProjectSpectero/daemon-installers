using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using installer.Properties;

namespace installer
{
    class Uninstaller
    {
        public Uninstaller()
        {
            // Store the installation path.
            string specteroInstallPath = GetSpecteroInstallationLocation();

            // Delete the service
            UninstallNssm();

            // Remove from path
            RemoveSpecteroFromPath();

            // Delete Spectero
            Directory.Delete(specteroInstallPath, true);

            // Let the user know
            MessageBox.Show(Resources.removal_success, Resources.messagebox_title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Check to see if there's an installation of spectero in the environment path.
        /// </summary>
        /// <returns></returns>
        public static bool InstallationExists()
        {
            return Environment.GetEnvironmentVariable("PATH").Contains("Spectero") && Directory.Exists(GetSpecteroInstallationLocation());
        }

        /// <summary>
        /// Remove Spectero paths from the environment variables.
        /// </summary>
        public static void RemoveSpecteroFromPath()
        {
            // Get the current environment state
            string env = Environment.GetEnvironmentVariable("PATH");

            // Explode the string to get the objects in an iterable format.
            string[] envSplit = env.Split(';');

            // Use LINQ to remove the objects that contain spectero.
            envSplit = envSplit.Where(path => !path.Contains("Spectero")).ToArray();

            // Recompile the string using concatment with the delimiter.
            string recompiledEnv = string.Join(";", envSplit);

            // Update the environment.
            Environment.SetEnvironmentVariable("PATH", recompiledEnv, EnvironmentVariableTarget.User);
        }

        /// <summary>
        /// Try to determine the installation location from the path.
        /// </summary>
        /// <returns></returns>
        public static string GetSpecteroInstallationLocation()
        {
            // Get the current environment state
            string env = Environment.GetEnvironmentVariable("PATH");

            // Explode the string to get the objects in an iterable format.
            string[] envSplit = env.Split(';');

            // Return the single path
            foreach (var currentPath in envSplit)
                if (currentPath.Contains("Spectero"))
                {
                    string newpath = currentPath;
                    while (!newpath.EndsWith("Spectero"))
                    {
                        newpath = Directory.GetParent(newpath).FullName;
                    }
                    return newpath;
                }
                    

            SpecteroPathNotFoundException();
            return "";
        }

        public static Exception SpecteroPathNotFoundException()
        {
            return new Exception("The installation path of spectero could not be found.");
        }

        public static void UninstallNssm()
        {
            // Placeholder variable to store the NSSM path.
            string nssmDir = null;

            // Iterate through each subfolder and find the one for NSSM.
            foreach (var currentPath in Directory.GetDirectories(GetSpecteroInstallationLocation()))
                if (currentPath.Contains("nssm"))
                    nssmDir = Path.Combine(GetSpecteroInstallationLocation(), currentPath);


            // Check to see if NSSM has been found.
            if (nssmDir != null)
            {
                // Initialize a new service manager to delete the class.
                var service = new ServiceManager(nssmDir);
                service.Stop();
                service.Delete();

                Thread.Sleep(1000);

                // Delete the NSSM directory.
                Directory.Delete(nssmDir, true);
            }
        }
    }
}