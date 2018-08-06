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

        /// <summary>
        /// Get the executable path for NSSN.
        /// </summary>
        /// <returns></returns>
        public string GetExecutablePath()
        {
            return Path.Combine(_nssmPath,
                string.Format("{0}\\nssm.exe", (Program.Is64BitOperatingSystem) ? "win64" : "win32"));
        }

        /// <summary>
        /// Check to see if the service exists.
        /// </summary>
        /// <returns></returns>
        public bool Exists()
        {
            return ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals("spectero.daemon"));
        }

        /// <summary>
        /// Create the service
        /// </summary>
        public void Create()
        {
            Process.Start(
                GetExecutablePath(),
                string.Format("install \"spectero.daemon\" \"{0}\" \"{1}\"",
                    DotNetCore.GetDotnetPath(),
                    GetBinaryExpectedPath()
                )
            );
        }


        /// <summary>
        /// Get the expected binary path to the daemon.
        /// </summary>
        /// <returns></returns>        
        public string GetBinaryExpectedPath()
        {
            string absoluteRoot = Path.Combine(Program.InstallLocation, "Daemon");
            string versionPath = Path.Combine(absoluteRoot, Program.Version);
            string daemonPath = Path.Combine(versionPath, "daemon");
            return Path.Combine(daemonPath, "daemon.dll");
        }

        /// <summary>
        /// Delete the service.
        /// </summary>
        public void Delete()
        {
            Process.Start(GetExecutablePath(), "remove spectero.daemon confirm");
        }

        /// <summary>
        /// Stop the service.
        /// </summary>
        public void Stop()
        {
            Process.Start(GetExecutablePath(), "stop spectero.daemon");
        }

        /// <summary>
        /// Start the service.
        /// </summary>
        public void Start()
        {
            Process.Start(GetExecutablePath(), "start spectero.daemon");
        }
    }
}