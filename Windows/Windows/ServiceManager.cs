using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
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
            return Path.Combine(_nssmPath,
                string.Format("{0}\\nssm.exe", (Program.Is64BitOperatingSystem) ? "win64" : "win32"));
        }

        public bool Exists()
        {
            return ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals("spectero.daemon"));
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

        public void Stop()
        {
            Process.Start(GetExecutablePath(), "stop spectero.daemon");
        }

        public void Start()
        {
            Process.Start(GetExecutablePath(), "start spectero.daemon");
        }
    }
}