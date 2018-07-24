using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using installer.Properties;
using Microsoft.Win32;

namespace installer
{
    class Uninstaller
    {
        public Uninstaller()
        {
            // Delete the service
            UninstallNssm();

            // Remove from path
            RemoveSpecteroFromPath();

            // Delete Spectero
            Directory.Delete(GetInstallationDirectory(), true);

            // Delete the windows installer entry.
            new WindowsInstaller().DeleteEntry();

            // Remove from the registry.
            DeleteRegistryEntry();

            // Let the user know
            MessageBox.Show(Resources.removal_success, Resources.messagebox_title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Check to see if there's an installation of spectero in the environment path.
        /// </summary>
        /// <returns></returns>
        public static bool InstallationExists()
        {
            try
            {
                return Directory.Exists(GetInstallationDirectory());
            } catch (Exception e)
            {
                return false;
            }
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

        public static Exception SpecteroPathNotFoundException()
        {
            return new Exception("The installation path of spectero could not be found.");
        }

        public static void UninstallNssm()
        {
            // Placeholder variable to store the NSSM path.
            string nssmDir = null;

            // Iterate through each subfolder and find the one for NSSM.
            foreach (var currentPath in Directory.GetDirectories(GetInstallationDirectory()))
                if (currentPath.Contains("nssm"))
                    nssmDir = Path.Combine(GetInstallationDirectory(), currentPath);


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

        public static string GetInstallationDirectory()
        {
            string uninstallRegKeyPath = @"SOFTWARE\";
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(uninstallRegKeyPath, true))
            {
                // Get the key from the registry.
                RegistryKey key = parent.OpenSubKey("Spectero", true);

                // Get the value we need
                return key.GetValue("InstallationDirectory").ToString();
            }
        }

        public static void DeleteRegistryEntry()
        {
            string uninstallRegKeyPath = @"SOFTWARE\";
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(uninstallRegKeyPath, true))
            {
                parent.DeleteSubKey("Spectero");
            }
        }
    }
}