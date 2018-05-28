using System;
using System.IO;
using System.Linq;
using Windows;

namespace installer
{
    internal class DotNetCore
    {
        // 32 bit download link for dotnet core
        public static string X86DownloadLink =
            "https://download.microsoft.com/download/1/1/0/11046135-4207-40D3-A795-13ECEA741B32/dotnet-runtime-2.0.5-win-x64.zip";

        // 64 bit download link for dotnet core
        public static string X64DownloadLink =
            "https://download.microsoft.com/download/1/1/0/11046135-4207-40D3-A795-13ECEA741B32/dotnet-runtime-2.0.5-win-x64.zip";

        // List of potential paths where dotnet core can be installed in the system.
        private static readonly string[] DotnetPotentialPaths =
        {
            "C:\\Program Files (x86)\\dotnet\\dotnet.exe", // 32 bit path
            "C:\\Program Files\\dotnet\\dotnet.exe" // 64 bit path
        };

        public static bool Exists() => DotnetPotentialPaths.Any(File.Exists);

        public static string GetDotnetPath()
        {
            foreach (var dotnetPotentialPath in DotnetPotentialPaths)
                if (File.Exists(dotnetPotentialPath))
                    return dotnetPotentialPath;

            return null;
        }

        public static string GetDownloadLinkFromArch() =>
            (Program.InternalCheckIsWow64()) ? X64DownloadLink : X86DownloadLink;
    }
}