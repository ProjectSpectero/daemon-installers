#!/usr/bin/env bash

###############################################################################
##
##  Spectero, Inc
##  Copyright (c) 2018 - All rights reserved 
## 
##  By using this script you are agreeing to the terms of services
##  provided to the end user at https://spectero.com/tos
##
###############################################################################

# Check if we are root
if [[ $EUID -ne 0 ]]; then
    echo "This script must be ran as root.";
    exit 1;
fi

# Check if we have dependencies
if [ "$(uname)" == "Darwin" ]; then
    # Check to see if python is installed.
    which -s python3
    if [[ $? != 0 ]]; then

        # Check to see if brew is installed.
        which -s brew
        if [[ $? != 0 ]]; then
            # Download and install
            ruby -e "$(curl -FsSL https://raw.githubusercontent.com/Homebrew/install/master/install)"
            
            # chown
            chown root /usr/local/bin/brew
        fi
        
        # Install Python3
        brew install python3
    fi
    
    # Check to see if dotnet core is installed
    which -s dotnet
    if [[ $? != 0 ]]; then
        # Install dotnet core
        brew install dotnet
    fi
    
elif [ "$(uname)" == "Linux" ]; then
    
    # Find the package manager
    YUM_CMD=$(which yum)
    APT_GET_CMD=$(which apt-get)

    # Check to see if we have Python3 installed.
    if ! type "python3" > /dev/null; then
    
        # Debian/Ubuntu
        if [[ ! -z $YUM_CMD ]]; then
            yum install python34 -y
    
        # Cent/RHEL
        elif [[ ! -z $APT_GET_CMD ]]; then
            apt-get install python3 -y
    
        # ???
        else
            echo "The package manager for your system was not found"
            echo "Please report your package manager to the developers so we can add support!"
            exit 1;
        fi
    fi
    
    # Check to see if we have dotnet core installed.
    if ! type "dotnet" > /dev/null; then
    
        # Debian/Ubuntu
        if [[ ! -z $YUM_CMD ]]; then
            yum install libunwind-devel libcurl-devel libicu -y
    
        # Cent/RHEL
        elif [[ ! -z $APT_GET_CMD ]]; then
            apt-get install libunwind-dev libcurl4 -y
    
        # ???
        else
            echo "The package manager for your system was not found"
            echo "Please report your package manager to the developers so we can add support!"
            exit 1;
        fi
        
        # Download dotnet installation script and execute it
        wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
        bash /tmp/dotnet-install.sh --channel 2.0 --shared-runtime --install-dir /usr/local/bin/
    fi
    
    
else
    echo "Unsupported Operating System"
fi

# Launch python script
cat << EOF > "/tmp/spectero-installer.sh"
#!/usr/bin/env python3
import json
import os
import shutil
import subprocess
import sys
import zipfile
import traceback
import urllib.request

"""
Spectero Installer
Copyright (c) 2018 Spectero - All Rights Reserved
https://spectero.com/

Description:
    This utility serves the purpose to install the daemon and command line tools into your operating system
    It supports Mac OS X and Linux variants.

Warranty:
    This software is provided without warranty.

Liability:
    The user runs this application at their own risk, and takes responsibility for any system changes.
    You will be promoted to read the terms service upon execution and installation.

Command Line Arguments:

    Release Channels:
        Release channels are categorized based on how stable the builds are.

        --alpha
            Use the alpha channel and get bleeding edge builds of the daemon.
            It is ill-advised to use this channel as bugs may be present.

        --beta
            Use the beta channel and get semi-stable builds that contain new features.
            This channel also may contain bugs.

        --stable
            Use the stable channel and get release candidates that are deemed suitable for production
            (this flag is used by default)

    Other:
        --agree
            Agree to the Spectero Terms of Service located at https://spectero.com/tos.

        --install-dotnet
            Automatically install the .NET Core Framework if it does not exist.

        --no-prompt
            Disable all Spectero related prompts and automatically install into /opt/spectero

        --overwrite
            Force an overwrite of the current version installed.
            If this is not provided, the installer will assume it should check for an update.

"""

INSTALLER_VERSION = 1


def execution_contains_cli_flag(flag):
    for current_flag in sys.argv:
        if flag == current_flag:
            return True
    return False


class SpecteroInstaller:
    def __init__(self):
        # Initialize Variables
        self.channel = None
        self.channel_version = None
        self.release_data = None
        self.spectero_releases_url = "https://spectero.com/releases.json"
        self.spectero_install_path = "/opt/spectero"
        self.suppress_bash_tag = " >/dev/null 2>&1"
        self.systemd_service_destination = "/etc/systemd/system/spectero.service"
        self.use_local_dotnet = False

        # Determine which release channel to download from.
        self.determine_channel()

        # Check to see if the installer needs updated or if there's a release for the channel.
        self.check_release_available()

        # Check if we're root.
        if not self.is_root():
            print("This script must be ran as root.")
            sys.exit(1)

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

    def is_root(self):
        return os.getuid() == 0

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
            with open(os.devnull, 'w') as devnull:
                which_path = (subprocess.check_output(["which", "dotnet"], stderr=devnull)[:-1]).decode("utf-8")

            if 'dotnet' in which_path:
                self.dotnet_framework_path = which_path
                return True
        except:
            pass

        # Check local installation
        if os.path.isfile(os.path.join(self.spectero_install_path, "/dotnet/dotnet")):
            self.dotnet_framework_path = self.spectero_install_path + "/dotnet/dotnet"
            return True

        # No installation
        return False

    def create_user_and_groups(self):
        # Create User, Group and assign.
        if sys.platform in ["linux", "linux2"]:
            os.system("useradd spectero" + self.suppress_bash_tag)
            os.system("groupadd spectero" + self.suppress_bash_tag)
            os.system("usermod -a -G spectero spectero" + self.suppress_bash_tag)

        elif sys.platform in ["darwin"]:
            os.system("dscl . -create /Users/spectero")
            os.system("dscl . -create /Users/spectero UniqueID GROUP_ID")
            os.system("dscl . -create /Users/spectero UserShell /bin/bash")
            os.system("dscl . -create /Users/spectero RealName \"Spectero User\"")
            os.system("dseditgroup -o edit -a spectero -t user spectero")

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
                    try:
                        if not os.path.exists(self.spectero_install_path):
                            os.mkdir(self.spectero_install_path)
                            print("Created directory: %s" % self.spectero_install_path)
                            return
                    except:
                        print("Failed to make the directory, please validate the path and ensure the parent directory exists.")
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
            if not execution_contains_cli_flag("--overwrite") and os.path.exists(os.path.join(self.spectero_install_path, self.channel_version)):
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
                    with urllib.request.urlopen(channel_download_url_alt) as response, open(projected_zip_path, 'wb') as zip:
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
            os.system("ln -s %s/%s %s/latest" % (self.spectero_install_path, self.channel_version, self.spectero_install_path))

            try:
                self.create_user_and_groups()
                print("Changing ownership for installation directory: %s" % self.spectero_install_path)
                os.system("chown -R spectero:spectero %s" % self.spectero_install_path)
            except Exception as e2:
                print("An error occurred while attempting to change directory ownership")
                print("Does the spectero user exist?")
                print(e2)
                sys.exit(9)

            # Create the service.
            self.systemd_service(self.spectero_install_path + "/" + self.channel_version + "/daemon")

            # Create the command
            self.build_usr_sbin_script()

            print("=" * 50)
            print("Spectero should be installed and running!")

        except Exception as e:
            print("The installer encountered a problem while retrieving release data from the download server.")
            print("Please try again later.")
            print(e)
            sys.exit(5)

    def get_spectero_releases(self):
        if self.release_data is None:
            self.release_data = json.loads(urllib.request.urlopen(self.spectero_releases_url).read())
        return self.release_data

    def systemd_service(self, daemon_path):
        try:
            systemd_script = daemon_path + "/Tooling/Linux/spectero.service"

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

    def build_usr_sbin_script(self):
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

            print("Copying console management interface shell script to /usr/sbin/spectero")
            shutil.copyfile(cli_script, "/usr/sbin/spectero")
            os.system("chmod +x /usr/sbin/spectero")
        except Exception as e:
            traceback.print_exc()
            print("The installer encountered a problem while copying the CLI script.")
            print("Please report this problem.")
            sys.exit(12)


if __name__ == "__main__":
    SpecteroInstaller()

EOF

# Run the installer.
python3 /tmp/spectero-installer.sh $@

# Exit gracefully.
exit 0;