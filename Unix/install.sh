#!/usr/bin/env bash

###############################################################################
##
##  Spectero, Inc
##  Copyright (c) 2018 - All rights reserved
##  https://spectero.com
## 
##  By using this script you are agreeing to the terms of services
##  provided to the end user at https://spectero.com/tos
##
##  For command line arguments, please refer to the readme in the GitHub Repo
##  https://github.com/ProjectSpectero/daemon-installers
##
###############################################################################

# VARIABLES
DOTNET_CORE_VERSION="2.1";


# Check if we have dependencies
if [ "$(uname)" == "Darwin" ]; then

    # Check if we are root.
    if (( EUID == 0 )); then
        echo "This installation requires brew, which cannot be ran as root.";
        echo "Please run this script as a normal user.";
        exit 1;
    fi

    # Check to see if python is installed.
    which -s python3;
    if [[ $? != 0 ]]; then

        # Check to see if brew is installed.
        which -s brew;
        if [[ $? != 0 ]]; then
            # Download and install
            sudo ruby -e "$(curl -FsSL https://raw.githubusercontent.com/Homebrew/install/master/install)"
        fi
        
        # Install Python3
        brew install python3;
    fi
    
    # Download wget if it doesn't exist.
    which -s wget;
    if [[ $? != 0 ]]; then
        brew install wget;
    fi
    
    # Check to see if dotnet core is installed
    which -s dotnet;
    if [[ $? != 0 ]]; then
        # Download dotnet installation script and execute it
        sudo wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh > /dev/null
        sudo bash /tmp/dotnet-install.sh --channel $DOTNET_CORE_VERSION --shared-runtime --install-dir /usr/local/bin/ 2> /dev/null
    fi

    which -s openvpn;
    if [[ $? != 0 ]]; then
        brew install openvpn;
    fi
    
elif [ "$(uname)" == "Linux" ]; then
    # Check if we are root.
    if [[ $EUID -ne 0 ]]; then
        echo "This script must be ran as root for the linux installation.";
        exit 1;
    fi


    # Check for Systemd
    if [ -e /etc/os-release ]; then

        # Load OS Variables
        source /etc/os-release;

        # Find the package manager
        YUM_CMD=$(which yum 2> /dev/null);
        APT_GET_CMD=$(which apt-get 2> /dev/null);

        # Update sources if apt-get based package manager exists.
        if [[ ! -z $APT_GET_CMD ]]; then
            # Debian / Ubuntu
            echo "Detected APT-GET as the operating system's package manager."
            apt-get update;

        elif [[ ! -z $YUM_CMD ]]; then
            # CentOS/RHEL.
            echo "Detected YUM as the operating system's package manager."
        fi

        # Check to see if we have the sudo tool, we will ned it later.
        if ! type "sudo" &> /dev/null; then
            echo "Dependency Check: sudo will be installed."
            if [[ ! -z $YUM_CMD ]]; then
                # Cent / RHEL / Fedora
                yum install sudo -y;

            elif [[ ! -z $APT_GET_CMD ]]; then
                # Debian / Ubuntu
                apt-get install sudo -y;

            else
                # ???
                echo "The package manager for your system was not found";
                echo "Please report your package manager to the developers so we can add support!";
                exit 1;
            fi
        else
            echo "Dependency Check: sudo is installed."
        fi

        # Check to see if we have Python3 installed.
        if ! type "python3" &> /dev/null; then
            echo "Dependency Check: python3 will be installed."
            if [[ ! -z $YUM_CMD ]]; then
                # Cent / RHEL / Fedora
                echo "Invoking package manager."
                yum install python34 -y;

            elif [[ ! -z $APT_GET_CMD ]]; then
                # Debian / Ubuntu
                echo "Invoking package manager."
                apt-get install python3 -y;

            else
                # ???
                echo "The package manager for your system was not found";
                echo "Please report your package manager to the developers so we can add support!";
                exit 1;
            fi

        else
            echo "Dependency Check: python3 is present in the system."
        fi

        # Check to see if we have openvpn installed.
        if ! type "openvpn" &> /dev/null; then
            echo "Dependency Check: OpenVPN will be installed."
            if [[ ! -z $YUM_CMD ]]; then
                # Cent / RHEL / Fedora
                yum install openvpn -y;

            elif [[ ! -z $APT_GET_CMD ]]; then
                # Debian / Ubuntu
                apt-get install openvpn -y;

            else
                # ???
                echo "The package manager for your system was not found";
                echo "Please report your package manager to the developers so we can add support!";
                exit 1;
            fi
        else
            echo "Dependency Check: OpenVPN is present in the system."
        fi

        # Check to see if we have dotnet core installed.
        if ! type "dotnet" &> /dev/null; then
            # Cent / RHEL
            if [[ ! -z $YUM_CMD ]]; then
                if [[ $ID == "fedora" ]]; then
                    # Fedora Workstation and Server
                    yum install libunwind-devel libcurl-devel libicu compat-openssl10 -y;

                else
                    # Generic Dependency Install
                    echo "The installer was unable to determine the variant of linux."
                    echo "Your package manager is yum, and we will try to install the required dependencies."
                    echo "If the installation fails, please report the issue to"
                    echo "https://github.com/ProjectSpectero/daemon-installers/issues"
                    ehco "So Spectero can implement support for your operating system."
                    yum install libunwind-devel libcurl-devel libicu -y;
                fi

            # Debian / Ubuntu
            elif [[ ! -z $APT_GET_CMD ]]; then
                # Ubuntu
                if [ $ID == "ubuntu" ]; then

                    # Some versions of ubuntu name the packages differently.
                    # Please add specifics for each version.

                    # Ubuntu 18.04
                    if [ $VERSION_ID == "18.04" ]; then
                        echo "Detected Operating System: Ubuntu 18.04 LTS";
                        echo "Installing dependencies for dotnet core framework."
                        apt-get install libunwind-dev libcurl4 -y;

                    # Undocumented Ubuntu Version
                    else
                        echo "Detected Operating System: Ubuntu (unknown version)";
                        echo "Installing dependencies for dotnet core framework."
                        apt-get install libunwind-dev libcurl4-openssl-dev -y;
                    fi

                # Debian
                elif [ $ID == "debian" ]; then

                    # Starch
                    if [ $VERSION_ID == "9" ]; then
                        echo "Detected Operating System: Debian 9";
                        echo "Installing dependencies for dotnet core framework."
                        apt-get install libicu-dev libunwind-dev libcurl4-openssl-dev -y;

                    # Undocumented Debian Version.
                    else
                        echo "Detected Operating System: Debian (unknown version)";
                        echo "Installing dependencies for dotnet core framework."
                        apt-get install libunwind-dev libcurl4-openssl-dev -y;
                    fi

                else
                    # Generic Dependency Install
                    echo "The installer was unable to determine the variant of linux."
                    echo "Your package manager is apt-get, and we will try to install the required dependencies."
                    echo "If the installation fails, please report the issue to"
                    echo "https://github.com/ProjectSpectero/daemon-installers/issues"
                    ehco "So Spectero can implement support for your operating system."
                    apt-get install libunwind-dev libcurl4-openssl-dev -y;
                fi

            # ???
            else
                echo "The package manager for your system was not found";
                echo "Please report your package manager to the developers so we can add support!";
                exit 1;
            fi

            # Download dotnet installation script and execute it
            echo "Downloading dotnet core installation script."
            wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh > /dev/null

            echo "Installing dotnet core version $DOTNET_CORE_VERSION...";
            bash /tmp/dotnet-install.sh --channel $DOTNET_CORE_VERSION --shared-runtime --install-dir /usr/local/bin/ 2> /dev/null
        fi

    else
        echo "Unsupported Operating System";
        echo "Spectero requires an operating system that uses systemd.";
        exit 1;
    fi
    
else
    echo "Unsupported Operating System";
    echo "This installation script supports Linux and MacOS";
fi

# Launch python script
cat << EOF > "/tmp/spectero-installer.sh"
#!/usr/bin/env python3
import json
import os
import shutil
import subprocess
import sys
import traceback
import zipfile

import urllib.request

INSTALLER_VERSION = 1


def execution_contains_cli_flag(flag):
    for current_flag in sys.argv:
        if flag == current_flag:
            return True
    return False


def is_root():
    return os.getuid() == 0


class SpecteroInstaller:
    def __init__(self):
        # Initialize Variables
        self.channel = None
        self.channel_version = None
        self.release_data = None
        self.spectero_releases_url = "https://c.spectero.com/releases.json"
        self.spectero_install_path = "/opt/spectero"
        self.suppress_bash_tag = " >/dev/null 2>&1"
        self.systemd_service_destination = "/etc/systemd/system/spectero.service"
        self.sudoers_template = "Cmnd_Alias SPECTERO_CMDS = {systemctl} start spectero, {systemctl} stop spectero, {systemctl} status spectero, " \
                                "{systemctl} restart spectero, {iptables}, {openvpn}"
        self.dotnet_framework_path = False

        # Determine which release channel to download from.
        self.determine_channel()

        # Check to see if the installer needs updated or if there's a release for the channel.
        self.check_release_available()

        # Show the welcome
        self.welcome()

        # Check if we should display the terms of service, if the user passes the --agree flag we can skip.
        if not execution_contains_cli_flag("--agree"):
            self.agree_to_terms_of_service()
        else:
            print("You have agreed to the Terms of Service.")

        # Prompt the user with the path that spectero should install.
        if not execution_contains_cli_flag("--no-prompt"):
            self.prompt_install_location()

        print("Spectero will be installed in: %s" % self.spectero_install_path)

        # Find dotnet framework, and download if it doesn't exist.
        if not self.find_dotnet_core():
            print("Failed to find dotnet core. Please make sure it is in your environment path.")
            sys.exit(13)

        else:
            print("The .NET Core Framework is already installed.")

        # Ask the user if they are ready to install.
        if not execution_contains_cli_flag("--no-prompt"):
            self.prompt_install_ready()

        if os.path.isfile(self.systemd_service_destination):
            print("Stopping the currently running spectero daemon to upgrade...")
            os.system('systemctl stop spectero')

        # Install.
        self.install()

    def welcome(self):
        print("Welcome to the installation script for Spectero.")

    def agree_to_terms_of_service(self):
        lines = [
            "",
            "Spectero releases all software with a Terms of Service Agreement located at:",
            "https://spectero.com/tos",
            "",
            "Do you agree to the Terms of Service? (yes/No)"
        ]
        for line in lines:
            print(line)

        if str(input("> ")).lower().strip() in ["yes", "y"]:
            return True
        else:
            print("You did not agree to the Terms of Service.")
            sys.exit(2)

    def determine_channel(self):
        if execution_contains_cli_flag("--stable"):
            self.channel = "stable"

        elif execution_contains_cli_flag("--beta"):
            self.channel = "beta"

        elif execution_contains_cli_flag("--alpha"):
            self.channel = "alpha"

        else:
            self.channel = "stable"

    def find_dotnet_core(self):
        # Check system installation
        try:
            which_path = self.which("dotnet");

            if 'dotnet' in which_path:
                self.dotnet_framework_path = which_path
                return True
        except:
            pass

        potential_paths = [
            "/usr/bin/dotnet",
            "/usr/local/bin/dotnet"
        ]

        for path in potential_paths:
            if os.path.exists(path):
                self.dotnet_framework_path = path
                return True

        # No installation
        return False

    def create_user_and_groups(self):
        # Create User, Group and assign.
        if sys.platform in ["linux", "linux2"]:
            os.system("useradd spectero" + self.suppress_bash_tag)
            os.system("groupadd spectero" + self.suppress_bash_tag)
            os.system("usermod -a -G spectero spectero" + self.suppress_bash_tag)

            print("Spectero User and Group have been created.")

    def prompt_install_location(self):
        lines = [
            "",
            "Where would you like to install Spectero?",
            "The default path is: %s" % self.spectero_install_path,
            "[Press enter for default path]"
        ]
        for line in lines:
            print(line)

        while True:
            # ask them for where they want to install the path.
            prompted_install_path = str(input('> '))

            # root of the filesystem was specified
            if prompted_install_path == "/":
                print("The installation path cannot be the root of the filesystem.")

            # if it is empty, they want the default installation path.
            elif prompted_install_path == "":
                return

            # the user specified a path.
            else:
                if prompted_install_path[0] == "/":
                    self.spectero_install_path = prompted_install_path

                    # Check for trailing slash
                    if self.spectero_install_path.endswith('/'):
                        self.spectero_install_path = self.spectero_install_path[:-1]  # Remove last character

                    try:
                        if not os.path.exists(self.spectero_install_path):
                            mkdir = subprocess.Popen(['mkdir', '-p', self.spectero_install_path])
                            mkdir.communicate()
                            print("Created directory: %s" % self.spectero_install_path)
                            return
                    except:
                        print(
                            "Failed to make the directory, please validate the path and ensure the parent directory exists.")
                        continue

    def prompt_install_ready(self):
        lines = [
            "",
            "Spectero is ready to install",
            "Continue? (Yes/no)"
        ]

        for line in lines:
            print(line)

        if str(input("> ")).lower().strip() not in ["yes", "y", ""]:
            print("Spectero will not be installed.")
            sys.exit(4)

    def check_release_available(self):
        releases = self.get_spectero_releases()

        # Check installer version
        if releases["installer.revision"] > INSTALLER_VERSION:
            print("Hey! there's a new installer available for Spectero.")
            print("Please go to https://spectero.com/ to download the new version.")
            sys.exit(8)

        # Check if null
        if releases["channels"][self.channel] is None:
            print("There are no release candidates for the %s download channel." % self.channel)
            print("Please use the stable channel instead.")
            sys.exit(7)

    def install(self):
        print("Getting releases from CI...")
        try:
            releases = self.get_spectero_releases()
            self.channel_version = releases["channels"][self.channel]

            print("Found %s release: %s" % (self.channel, self.channel_version))
            if not execution_contains_cli_flag("--overwrite") and os.path.exists(
                            "%s/%s" % (self.spectero_install_path, self.channel_version)):
                print("You are running the latest version of spectero.")
                sys.exit(0)

            channel_download_url = releases["versions"][self.channel_version]["download"]
            channel_download_url_alt = releases["versions"][self.channel_version]["altDownload"]

            projected_zip_path = ("/tmp/%s.zip" % self.channel_version)

            # Try to download from the primary mirror
            try:
                print("Downloading %s from %s..." % (self.channel_version, channel_download_url))

                with urllib.request.urlopen(channel_download_url) as response, open(projected_zip_path, 'wb') as zip:
                    shutil.copyfileobj(response, zip)

                print("Download complete.")
            except Exception as e:
                print("An exception occurred while downloading from the primary mirror.")

                # Try to download from the secondary
                try:
                    print("Downloading %s from %s..." % (self.channel_version, channel_download_url_alt))
                    with urllib.request.urlopen(channel_download_url_alt) as response, open(projected_zip_path,
                                                                                            'wb') as zip:
                        shutil.copyfileobj(response, zip)
                    print("Download complete.")
                except Exception as e2:
                    print("The installer failed to download from the primary and secondary mirrors.")
                    print("Please try again later.")
                    print(e2)
                    sys.exit(5)

            # Try to extract
            try:
                referenced_zip = zipfile.ZipFile(projected_zip_path, 'r')
                referenced_zip.extractall(self.spectero_install_path)
                referenced_zip.close()
                print("%s has been extracted to %s." % (projected_zip_path, self.spectero_install_path))
            except:
                print("The installer failed to extract the downloaded zipfile.")
                sys.exit(6)

            # Symlink
            if os.path.islink(self.spectero_install_path + "/latest"):
                print("Removing old symlink...")
                os.system("unlink " + self.spectero_install_path + "/latest")

            print("Symlinking latest to version %s" % self.channel_version)
            os.system("ln -s %s/%s %s/latest" % (
                self.spectero_install_path, self.channel_version, self.spectero_install_path))

            try:
                self.create_user_and_groups()
                print("Changing ownership for installation directory: %s" % self.spectero_install_path)
                if sys.platform in ["linux", "linux2"]:
                    os.system("chown -R spectero:spectero %s" % self.spectero_install_path)
                elif sys.platform in ["darwin"]:
                    os.system("chown -R daemon:daemon %s" % self.spectero_install_path)
            except Exception as e2:
                print("An error occurred while attempting to change directory ownership")
                print("Does the spectero user exist?")
                print(e2)
                sys.exit(9)

            # Chmod the auth.sh
            thirdPartyPath = "%s/latest/daemon/3rdParty/OpenVPN/auth.sh" % self.spectero_install_path
            if (os.path.exists(thirdPartyPath)):
                os.system("chmod +x %s" % thirdPartyPath)

            # Create the service if we're linux.
            if sys.platform in ["linux", "linux2"]:
                self.linux_enable_ipv4_forwarding()
                self.systemd_service()
                self.sudoers_automation()
            elif sys.platform in ["darwin"]:
                self.launchctl_service()

            # Create the command
            self.build_usr_local_bin_script()

            print("=" * 50)
            print("Spectero should be installed and running!")

        except Exception as e:
            print("The installer encountered a problem while retrieving release data from the download server.")
            print("Please try again later.")
            print(e)
            sys.exit(5)

    def get_spectero_releases(self):
        if self.release_data is None:
            self.release_data = json.loads(urllib.request.urlopen(self.spectero_releases_url).read().decode("utf-8"))
        return self.release_data

    def systemd_service(self):
        try:
            systemd_script = "%s/%s/daemon/Tooling/Linux/spectero.service" % (
                self.spectero_install_path, self.channel_version)

            # String replacement.
            with open(systemd_script, 'r') as file:
                filedata = file.read()

            filedata = filedata.replace("ExecStart=/usr/bin/dotnet", "ExecStart=" + self.dotnet_framework_path)

            with open(systemd_script, 'w') as file:
                file.write(filedata)

            if not os.path.isfile(self.systemd_service_destination):
                print("Installing systemd service")

            shutil.copyfile(systemd_script, self.systemd_service_destination)
            os.system("systemctl daemon-reload")
            os.system("systemctl enable spectero" + self.suppress_bash_tag)
            print("Using systemctl to start spectero service.")
            os.system("systemctl start spectero")
            os.system("systemctl status spectero")
            print("Systemd service installed successfully.")
        except Exception as e:
            traceback.print_exc()
            print("The installer encountered a problem while configuring the systemd service.")
            print("Please report this problem.")

    def launchctl_service(self):
        tooling_basepath = "%s/%s/daemon/Tooling/Mac" % (self.spectero_install_path, self.channel_version)
        plist_filename = "com.spectero.daemon.plist"
        plist_dest = "/Library/LaunchDaemons/" + plist_filename

        plist_template = "%s/%s" % (tooling_basepath, plist_filename)
        start_shell = "%s/%s" % (tooling_basepath, plist_filename)

        if os.path.exists("/Library/LaunchDaemons/com.spectero.daemon.plist"):
            os.system("launchctl unload -w " + plist_dest)

            # String replacement.
        with open(plist_template, 'r') as file:
            filedata = file.read()

        filedata = filedata.replace("{install_location}", self.spectero_install_path)
        filedata = filedata.replace("{install_version}", self.channel_version)

        with open(plist_template, 'w') as file:
            file.write(filedata)

        shutil.copy(plist_template, plist_dest)
        os.system("launchctl load -w " + plist_dest)

    def build_usr_local_bin_script(self):
        try:
            cli_script = self.spectero_install_path + "/latest/cli/Tooling/spectero"

            # String replacement.
            with open(cli_script, 'r') as file:
                filedata = file.read()
            print("Replacing variables in console management interface template...")
            filedata = filedata.replace("{dotnet path}", self.dotnet_framework_path)
            filedata = filedata.replace("{spectero working directory}", self.spectero_install_path)
            filedata = filedata.replace("{version}", self.channel_version)

            with open(cli_script, 'w') as file:
                file.write(filedata)

            print("Copying console management interface shell script to /usr/local/bin/spectero")
            shutil.copyfile(cli_script, "/usr/local/bin/spectero")
            os.system("chmod +x /usr/local/bin/spectero")
        except Exception as e:
            traceback.print_exc()
            print("The installer encountered a problem while copying the CLI script.")
            print("Please report this problem.")
            sys.exit(12)

    def which(self, command):
        with open(os.devnull, 'w') as devnull:
            return (subprocess.check_output(["which", command], stderr=devnull)[:-1]).decode("utf-8")

    def linux_enable_ipv4_forwarding(self):
        # Define the propety of  what we need toc heck
        property = "net.ipv4.ip_forward"
        try:
            # Try to execute
            result = (subprocess.check_output(["sysctl", "net.ipv4.ip_forward"], stderr=devnull)[:-1]).decode("utf-8")

            # Check if it is disabled
            if result == "%s = 0":
                # Enable ip forwarding
                print("Enabling IPv4 Forwarding")
                os.system("""echo "%s = 1" >> /etc/sysctl.conf""")
                print("Reloading System Configuration Kernel Properties...")
                os.system("sysctl --system")
        except:
            print("There was a problem attempting to check for kernel flag: ipv4_forward.")
            sys.exit(19)

    def sudoers_automation(self):
        # Replace the string templates.
        for command in ["systemctl", "iptables", "openvpn"]:
            self.sudoers_template = self.sudoers_template.replace("{%s}" % command, self.which(command))

        # Check if sudoers exists
        if os.path.exists('/etc/sudoers'):
            if self.sudoers_template not in open('/etc/sudoers').read():
                with open('/etc/sudoers', "a") as sudoers:
                    sudoers.write(self.sudoers_template + "\n" + "spectero ALL=(ALL) NOPASSWD:SPECTERO_CMDS\n")


class SpecteroUninstaller:
    def __init__(self):
        self.systemd_variables = {}
        self.systemd_service_path = "/etc/systemd/system/spectero.service"
        self.cli_path = "/usr/local/bin/spectero"
        self.spectero_install_location = "/opt/spectero"

        # Check to make sure the service is installed
        if os.path.exists(self.systemd_service_path):

            # Read the systemd script
            with open(self.systemd_service_path, 'r') as config_reader:
                for line in config_reader:
                    if "=" in line:
                        split_line = line.split("=")
                        self.systemd_variables[split_line[0]] = split_line[1]

            # Get Spectero's installation location.
            self.spectero_install_location = self.auto_abspath(
                self.auto_abspath(self.systemd_variables["WorkingDirectory"]))

            # Check to see if the CLI tool is installed.
            if os.path.isfile(self.cli_path):
                self.delete_cli_command()

            # Prompt the user to delete the systemd service
            self.delete_systemd_service()

            # Offer to delete the spectero folder.
            self.delete_spectero_folder()

        else:
            print("Failed to find a active installation of Spectero.")
            sys.exit(13)

    def delete_spectero_folder(self):
        print("Your spectero installation is located at:")
        print(self.spectero_install_location)
        print("If you choose to continue, doing so will erase the following information related to spectero:")
        print(" - All server configurations")
        print(" - All database information")
        response = input("Delete? (Yes/no)\n> ")

        if response.lower() in ["y", "ye", "yes", ""]:
            shutil.rmtree(self.spectero_install_location)
            print("Spectero's working directory has been uninstalled successfully.")
            sys.exit(0)
        else:
            sys.exit(0)

    def delete_systemd_service(self):
        response = input("Do you want to delete the Spectero Service? (Yes/no)\n> ")
        if response.lower() in ["y", "ye", "yes", ""]:
            os.system("systemctl stop spectero")
            os.system("systemctl disable spectero")
            os.unlink(self.systemd_service_path)
            print("%s has been deleted successfully." % self.systemd_service_path)

    def delete_cli_command(self):
        response = input("Do you want to delete the Spectero CLI Command? (Yes/no)\n> ")
        if response.lower() in ["y", "ye", "yes", ""]:
            os.unlink(self.cli_path)
            print("%s has been deleted successfully." % self.cli_path)

    def auto_abspath(self, path):
        return os.path.abspath(os.path.join(path, os.pardir))


if __name__ == "__main__":
    # Check if we're root.
    if not is_root():
        print("This script must be ran as root.")
        sys.exit(1)

    if execution_contains_cli_flag("--uninstall"):
        SpecteroUninstaller()
    else:
        SpecteroInstaller()

EOF

# Run the installer.
sudo python3 /tmp/spectero-installer.sh $@

# Exit gracefully.
exit 0;
