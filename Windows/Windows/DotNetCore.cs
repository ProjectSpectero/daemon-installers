using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Windows;

namespace installer
{
    internal class DotNetCore
    {
        // List of potential paths where dotnet core can be installed in the system.
        private static readonly string[] DotnetPotentialPaths =
        {
            "C:\\Program Files (x86)\\dotnet\\dotnet.exe", // 32 bit path
            "C:\\Program Files\\dotnet\\dotnet.exe" // 64 bit path
        };

        /// <summary>
        /// Oterate pver each of the potential paths to see if any exist.
        /// </summary>
        /// <returns></returns>
        public static bool Exists() => DotnetPotentialPaths.Any(File.Exists);

        /// <summary>
        /// Iterate over each of the potential paths to find the valid one.
        /// </summary>
        /// <returns></returns>
        public static string GetDotnetPath()
        {
            foreach (var dotnetPotentialPath in DotnetPotentialPaths)
                if (File.Exists(dotnetPotentialPath))
                    return dotnetPotentialPath;
            return null;
        }

        /// <summary>
        /// Get the download link dynamically based on the architecture of the system.
        /// </summary>
        /// <returns></returns>
        public static string GetDownloadLinkFromArch() => 
            Program.SourcesInformation["dependencies"]["dotnet"][
                Program.ReleaseInformation["versions"][Program.Version]["requiredDotnetCoreVersion"].ToString()
                ]
            ["Windows"]["default"].ToString();

        public static bool IsVersionCompatable()
        {
            // Consult the local installation.
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GetDotnetPath(),
                    Arguments = "--list-runtimes",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            
            // Execute and determine the version.
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();

                // Get the line that contains a version.
                if (line.Contains("Microsoft.AspNetCore.All"))
                {
                    // Single out the data from the installed version.
                    line = line.Remove(0, 25); // Remove the "Microsoft.AspNetCore.All"
                    line = line.Substring(0, 5); // Single out the version                

                    // Split 
                    string[] installed = line.Split('.');
                    string[] requirement = Program.ReleaseInformation["versions"][Program.Version]["requiredDotnetCoreVersion"].ToString().Split('.');

                    // Compare.
                    for (var i = 0; i != installed.Length; i++)
                        if (int.Parse(installed[i]) < int.Parse(requirement[i]))
                            // The installed version cannot run the required version.
                            return false;
                    // The installed version can run the required version
                    return true;
                }
            }

            // Generic failure.
            return false;
        }
    }
}