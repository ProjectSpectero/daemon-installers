using System;
using System.IO;
using System.Linq;
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
            (Program.Is64BitOperatingSystem)
                ? Program.SourcesInformation["dotnet"]["x64"].ToString()
                : Program.SourcesInformation["dotnet"]["x86"].ToString();
    }
}