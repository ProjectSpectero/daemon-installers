using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Windows;

namespace installer
{
    class ServiceManager
    {
        private string _nssmPath;

        public ServiceManager(string nssmPath)
        {
            _nssmPath = nssmPath;
        }

        public string GetExecutablePath()
        {
            return Path.Combine(_nssmPath, "nssm-2.24/win64/nssm.exe");
        }

        public bool Exists()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = "interrogate spectero",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line.Contains("The specified service does not exist as an installed service")) return false;
            }

            return true;
        }

        public void Create()
        {
            Process.Start(
                GetExecutablePath(),
                string.Format("install \"spectero.daemon\" \"{0}\" \"{1}\"",
                    Program.DotnetPath,
                    GetBinaryExpectedPath()
                )
            );
        }

        public string GetBinaryExpectedPath()
        {
            string versionPath = Path.Combine(Program.InstallLocation, Program.Version);
            string daemonPath = Path.Combine(versionPath, "daemon");
            return Path.Combine(daemonPath, "daemon.dll");
        }

        public void Delete()
        {
            Process.Start(GetExecutablePath(), "remove spectero.daemon confirm");
        }
    }
}