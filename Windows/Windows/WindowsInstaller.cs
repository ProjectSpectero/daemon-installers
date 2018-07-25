using installer.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Windows;

namespace installer
{
    class WindowsInstaller
    {
        /// <summary>
        /// Check to see if it exists.
        /// </summary>
        /// <returns></returns>
        public bool Exists()
        {
            string uninstallRegKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(uninstallRegKeyPath, true))
            {
                try
                {
                    // Get the key from the registry.
                    RegistryKey key = parent.OpenSubKey(Resources.guid, true);
                    return true;
                } catch(Exception e)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Delete the windows installer entry.
        /// </summary>
        public void DeleteEntry()
        {
            Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + Resources.guid);
        }

        /// <summary>
        /// Create the entry in the add/remove programs page.
        /// </summary>
        public void CreateEntry(string installer)
        {
            string uninstallRegKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(uninstallRegKeyPath, true))
            {
                if (parent == null)
                    throw new Exception("Uninstall registry key not found.");

                try
                {
                    RegistryKey key = null;
                    try
                    {
                        // Get the key from the registry.
                        key = parent.OpenSubKey(Resources.guid, true) ?? parent.CreateSubKey(Resources.guid);

                        // If the key doesn't exist, throw a problem.
                        if (key == null)
                            throw new Exception(String.Format("Unable to create uninstaller '{0}\\{1}'", uninstallRegKeyPath, Resources.guid));

                        // Get information about the assembly and build a path.
                        Assembly asm = GetType().Assembly;
                        Version version = asm.GetName().Version;
                        string exe = "\"" + installer.Replace("/", "\\\\") + "\"";

                        // Set values
                        key.SetValue("DisplayName", "Spectero Daemon for Windows");
                        key.SetValue("ApplicationVersion", version.ToString());
                        key.SetValue("Publisher", "Spectero, Inc");
                        key.SetValue("DisplayIcon", exe);
                        key.SetValue("DisplayVersion", Program.Version);
                        key.SetValue("URLInfoAbout", "https://spectero.com");
                        key.SetValue("Contact", "support@spectero.com");
                        key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                        key.SetValue("UninstallString", exe + " --uninstall");
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
                    throw new Exception("An error occurred writing uninstall information to the registry.  The service is fully installed but can only be uninstalled manually through the command line.", ex);
                }
            }
        }
    }
}
