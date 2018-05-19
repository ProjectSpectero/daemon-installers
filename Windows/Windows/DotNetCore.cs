﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace installer
{
    class DotNetCore
    {

        public static string PortableRuntimeDownloadLink = "https://download.microsoft.com/download/1/1/0/11046135-4207-40D3-A795-13ECEA741B32/dotnet-runtime-2.0.5-win-x64.zip";
        private static readonly string[] DotnetPotentialPaths =
        {
            "C:\\Program Files\\dotnet\\dotnet.exe",
        };

        public static bool Exists() => DotnetPotentialPaths.Any(File.Exists);

        public static string GetDotnetPath() => DotnetPotentialPaths.FirstOrDefault(dotnetPath => Directory.Exists(dotnetPath));
    }
}
