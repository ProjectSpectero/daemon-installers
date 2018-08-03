#!/usr/bin/env bash

###############################################################################
##
##  Spectero, Inc
##  Copyright (c) 2018 - All rights reserved
##  https://spectero.com
##
##  For command line arguments, please refer to our confluence documentation
##  https://spectero.atlassian.net/wiki/spaces/docs/pages/3244075/Linux+macOS+Installer+Arguments
##
###############################################################################


##### ==========================
##### VARIABLES
##### ==========================
DOTNET_CORE_VERSION="2.1.2";
INSTALL_LOCATION="/opt/spectero";
BRANCH="stable";
BRANCH_VERSION="latest"
TOS_PROMPT="true";
INSTALL_PROMPT="true";
OVERWRITE="true";
SYMLINK="true"
SERVICE="true";

##### ==========================
##### EXCEPTIONS
##### ==========================
function EXCEPTION_MAN_PAGE() {
    PRINT_MAN_PAGE
    exit 0;
}

function EXCEPTION_USER_ABORT(){
    clear
    echo "The user has aborted the installation.";
    echo "Spectero did not install.";
    exit 0; # General software error.
}

function EXCEPTION_TERMS_OF_SERVICE_AGREEMENT() {
    if [[ $TOS_PROMPT != "false" ]]; then
        clear
        echo "You did not agree to the Terms of Service.";
        echo "Spectero did not install.";
        exit 1; # General software error.
    fi
}

function EXCEPTION_SYSTEMD_NOT_FOUND() {
    clear
    echo "Spectero Daemon Installer has encountered a problem.";
    echo "The Operating System this script is running on doesn't appear to utilize the systemd init system.";
    echo "Spectero's daemon specifically works for systemd.";
    echo "You can find what operating systems Spectero is compatible on the website:";
    echo "https://spectero.com/downloads";
    echo "";
    echo "Spectero did not install.";
    exit 1;
}

function EXCEPTION_OSX_USER_CANNOT_BE_ROOT() {
    echo "This installation requires brew, which cannot be ran as root.";
    echo "Please run this script as a normal user.";
    clear;
    exit 126; # Brew cannot execute as root - "COMMAND INVOKED CANNOT EXECUTE"
}

function EXCEPTION_LINUX_USER_NEEDS_ROOT(){
    echo "This script must be ran as root.";
    exit 126;
}

function EXCEPTION_INCOMPATIBLE_PACKAGE_MANAGER() {
    clear;
    echo "The installer failed to find a compatible package manager.";
    echo "Supported Package Managers";
    echo "    ----> apt-get, apt, dnf (Debian / Ubuntu / Fedora / End user package managers)";
    echo "    ----> yum (CentOS / Red Hat Enterprise Linux / Server based package managers)";
    echo "    ----> brew (MacOS / OS X)";
    echo "";
    echo "Spectero did not install.";
    exit 127; # The command was not found
}

function EXCEPTION_PYTHON_INSTALLER() {
    echo "The python installer has encountered a problem.";
    echo "The Spectero installer was unable to install properly."
    exit $?;
}


##### ==========================
##### CONSOLE
##### ==========================
function PRINT_GREETINGS() {
    clear;
    echo "Welcome to the Installation Wizard Script for Spectero Daemon";
}

function PRINT_MAN_PAGE() {
    echo "Spectero Daemon Installer Manual";
    echo "===========================";
    echo "  -a, --agree             |   Agree to the Spectero Terms of Service.";
    echo "                          |";

    echo "  -ai, --install          |   Automatically install without any prompt for confirmation.";
    echo "                          |";

    echo "  -b, --branch            |   Specify the release channel that the installer will use.";
    echo "                          |   Potential possibilities:";
    echo "                          |   'stable' - The mature channel that is ready for production";
    echo "                          |       (This option is enabled by default)";
    echo "                          |   'beta'   - Get new features as they are implemented";
    echo "                          |       (Software may contain bugs at the user's own risk)";
    echo "                          |   'alpha'  - Get the newest things that developers implement";
    echo "                          |       (Bleeding edge, will contain bugs, designed for testing)";
    echo "                          |";

    echo "  -c, --channel           |   This argument does the same things as --branch";
    echo "                          |";

    echo "  -d, --dir               |   The location of where Spectero should install.";
    echo "                          |";

    echo "  -h, --help              |   Displays this page.";
    echo "                          |";

    echo "  -nsds, --no-systemd     |   Disables symlinking for the 'latest' folder.";
    echo "                          |";

    echo "  -nsl, --no-sl           |   Disables symlinking for the 'latest' folder.";
    echo "                          |";

    echo "  -u, --uninstall         |   Uninstalls the Spectero CLI and Service.";
    echo "                          |";

    echo "  -v, --version           |   Install a specific version.";
    echo "                          |";
}

function PRINT_TERMS_OF_SERVICE() {
    echo "";
    echo "Spectero's Daemon comes with a standard Terms of Service Agreement";
    echo "This document can be found at https://spectero.com/tos";
    echo "";
    echo "Do you agree to the Terms of Service? (no/yes)";

    # check if the argument exists
    if [[ $TOS_PROMPT == "true" ]]; then
        TOS_AGREEMENT_INPUT="no";

        # Read the response
        read TOS_AGREEMENT_INPUT;

        # Check if not yes
        if [[ $TOS_AGREEMENT_INPUT != "yes" ]]; then
            EXCEPTION_TERMS_OF_SERVICE_AGREEMENT
        fi
    fi
}

function PRINT_PROMPT_INSTALL_LOCATION () {
    clear;

    # Check if the user should be propmpted.
    if [ $INSTALL_PROMPT == "true" ]; then
        echo "By default if you press enter, Spectero will install into the following directory: $INSTALL_LOCATION";
        echo "Please press enter to accept this path as an installation directory, or provide a directory below:";

        # Prompt the user
        read INSTALL_LOCATION_ENTERED;

        # Reassign the installation location.
        if [[ $INSTALL_LOCATION_ENTERED != "" ]]; then
            INSTALL_LOCATION=$INSTALL_LOCATION_ENTERED;
        fi
    fi

    # Create the directory if it doesn't exist
    if [[ ! -d $INSTALL_LOCATION ]]; then
        echo "Created directory: $INSTALL_LOCATION";
        mkdir -p $INSTALL_LOCATION
    fi
}

function PRINT_PROMPT_READY_TO_INSTALL() {
    clear;

    echo "The installer has gathered enough information to start installation.";
    echo "Would you like to start the installation? (yes/no)";
    echo "Anything other than 'yes' will result in the installer exiting.";

    # check if the argument exists
    if [[ $INSTALL_PROMPT == "true" ]]; then
        # Read the response
        CONTINUE="no";
        read CONTINUE;

        # Check if not yes
        if [[ $CONTINUE != "yes" ]]; then
            EXCEPTION_USER_ABORT
        fi
    fi
}

function PRINT_INSTALL_COMPLETE() {
    echo "";
    echo "Installation of the Spectero Daemon has completed successfully.";
    exit 0;
}


##### ==========================
##### SYSTEM READING
##### ==========================
function LOAD_SYSTEND_DISTRIBUTION_INFORMATION() {
    if [ "$(uname)" == "Linux" ]; then
        source "/etc/os-release";
    fi
}


##### ==========================
##### WORKERS
##### ==========================
function WORK_INSTALL_BREW() {
    if [ "$(uname)" == "Darwin" ]; then
        which brew &> /dev/null;
        if [[ $? != 0 ]]; then
        echo "The brew package manager will now be installed.";
            sudo ruby -e "$(curl -FsSL https://raw.githubusercontent.com/Homebrew/install/master/install)"
        fi
    fi
}

function WORK_UNINSTALL() {
    clear;
    echo "Uninstalling Spectero Daemon...";

    if [[ -e "/usr/local/bin/spectero" ]]; then
        sudo rm -f "/usr/local/bin/spectero";
    fi

    if [[ -e "/etc/systemd/system/spectero.service" ]]; then
        systemctl disable spectero.service
        systemctl stop spectero.service
        sudo rm -f "/etc/systemd/system/spectero.service";
    fi

    echo "Spectero was successfully removed from your computer.";
    echo "The files still exist in the installation directory in case you wish to reinstall, but can be manually deleted if you wish.";
    exit 0;
}

function WORK_INSTALL_SUDO() {
    echo "The 'sudo' utility will now be installed.";
    if [ "$(uname)" == "Darwin" ]; then # OS X
        brew install sudo;
    elif [ "$(uname)" == "Linux" ]; then
        if [[ ! -z $DNF_CMD ]]; then
            dnf install sudo -y &> /dev/null;
        elif [[ ! -z $YUM_CMD ]]; then
            yum install sudo -y &> /dev/null;
        elif [[ ! -z $APT_GET_CMD ]]; then
            apt-get install sudo -y &> /dev/null;
        fi
    fi
}

function WORK_INSTALL_PYTHON3() {
    echo "The 'python3' executable will now be installed.";
    if [ "$(uname)" == "Darwin" ]; then # OS X
        brew install sudo;
    elif [ "$(uname)" == "Linux" ]; then
        if [[ ! -z $DNF_CMD ]]; then
            dnf install python34 -y &> /dev/null;
        elif [[ ! -z $YUM_CMD ]]; then
            yum install python34 -y &> /dev/null;
        elif [[ ! -z $APT_GET_CMD ]]; then
            apt-get install python3 -y &> /dev/null;
        fi
    fi
}

function WORK_INSTALL_OPENVPN() {
    echo "The 'openvpn' utility will now be installed.";
    if [ "$(uname)" == "Darwin" ]; then
        brew install openvpn;
    elif [ "$(uname)" == "Linux" ]; then
        if [[ ! -z $DNF_CMD ]]; then
            dnf install openvpn -y &> /dev/null;
        elif [[ ! -z $YUM_CMD ]]; then
            yum install openvpn -y &> /dev/null;
        elif [[ ! -z $APT_GET_CMD ]]; then
            apt-get install openvpn -y &> /dev/null;
        fi
    fi
}

function WORK_INSTALL_WGET() {
    echo "The 'wget' utility will now be installed.";
    if [ "$(uname)" == "darwin" ]; then
        brew install wget;
    elif [ "$(uname)" == "Linux" ]; then
        if [[ ! -z $DNF_CMD ]]; then
            dnf install wget -y &> /dev/null;
        elif [[ ! -z $YUM_CMD ]]; then
            yum install wget -y &> /dev/null;
        elif [[ ! -z $APT_GET_CMD ]]; then
            apt-get install wget -y &> /dev/null;
        fi
    fi
}

function WORK_INSTALL_UNZIP() {
    echo "The 'unzip' utility will now be installed.";
    if [ "$(uname)" == "Linux" ]; then
        if [[ ! -z $DNF_CMD ]]; then
            dnf install unzip -y &> /dev/null;
        elif [[ ! -z $YUM_CMD ]]; then
            yum install unzip -y &> /dev/null;
        elif [[ ! -z $APT_GET_CMD ]]; then
            apt-get install unzip -y &> /dev/null;
        fi
    fi
}

function WORK_INSTALL_IPTABLES() {
    echo "The 'iptables' utility will now be installed.";
    if [ "$(uname)" == "Linux" ]; then
        if [[ ! -z $DNF_CMD ]]; then
            dnf install unzip -y &> /dev/null;
        elif [[ ! -z $YUM_CMD ]]; then
            yum install unzip -y &> /dev/null;
        elif [[ ! -z $APT_GET_CMD ]]; then
            apt-get install unzip -y &> /dev/null;
        fi
    fi
}

function WORK_INSTALL_DOTNET_CORE() {
    echo "The 'dotnet' framework will now be installed.";
    if [[ ! -z $DNF_CMD ]]; then
        if [[ $ID == "fedora" ]]; then
           dnf install libunwind-devel libcurl-devel libicu compat-openssl10 -y &> /dev/null;
        fi
    elif [[ ! -z $YUM_CMD ]]; then
        if [[ $ID == "centos" ]]; then
           yum install libunwind-devel libcurl-devel libicu -y &> /dev/null;
        else
            # Generic Dependency Install
            echo "The installer was unable to determine the variant of linux."
            echo "Your package manager is yum, and we will try to install the required dependencies."
            echo "If the installation fails, please report the issue to"
            echo "https://github.com/ProjectSpectero/daemon-installers/issues"
            echo "So Spectero can implement support for your operating system."
            yum install libunwind-devel libcurl-devel libicu -y &> /dev/null;
        fi
    elif [[ ! -z $APT_GET_CMD ]]; then
        # Ubuntu
        if [ $ID == "ubuntu" ]; then
            if [ $VERSION_ID == "18.04" ]; then
                echo "Detected Operating System: Ubuntu 18.04 LTS";
                echo "Installing dependencies for dotnet core framework."
                apt-get install libunwind-dev libcurl4 -y &> /dev/null;
            else
                echo "Detected Operating System: Ubuntu (unknown version)";
                echo "Installing dependencies for dotnet core framework."
                apt-get install libunwind-dev libcurl4-openssl-dev -y &> /dev/null;
            fi
        elif [ $ID == "debian" ]; then
            if [ $VERSION_ID == "9" ]; then
                echo "Detected Operating System: Debian 9";
                echo "Installing dependencies for dotnet core framework."
                apt-get install libicu-dev libunwind-dev libcurl4-openssl-dev -y &> /dev/null;
            else
                echo "Detected Operating System: Debian (unknown version)";
                echo "Installing dependencies for dotnet core framework."
                apt-get install libunwind-dev libcurl4-openssl-dev -y &> /dev/null;
            fi
        else
            echo "The installer was unable to determine the variant of linux."
            echo "Your package manager is apt-get, and we will try to install the required dependencies."
            echo "If the installation fails, please report the issue to"
            echo "https://github.com/ProjectSpectero/daemon-installers/issues"
            echo "so Spectero can implement support for your operating system."
            apt-get install libunwind-dev libcurl4-openssl-dev -y &> /dev/null;
        fi
    fi

    echo "Dependencies for dotnet core have been installed.";
    echo "The second stage of the installer will create a local installation of dotnet.";
}

function WORK_INSTALL_EPEL_REPO() {
    if [[ $ID == "centos" ]]; then
        if [[ $VERSION_ID == "7" ]]; then
            yum install https://dl.fedoraproject.org/pub/epel/epel-release-latest-7.noarch.rpm -y;
        elif [[ $VERSION_ID == "6" ]]; then
            yum install https://dl.fedoraproject.org/pub/epel/epel-release-latest-6.noarch.rpm -y;
        fi
    fi
}

function WORK_WRITE_CONFIG() {
    # Write all data to a config
    echo "directory=$INSTALL_LOCATION" > /tmp/spectero.installconfig;
    echo "overwrite=$OVERWRITE" >> /tmp/spectero.installconfig;
    echo "branch=$BRANCH" >> /tmp/spectero.installconfig;
    echo "version=$BRANCH_VERSION" >> /tmp/spectero.installconfig;
    echo "service=$SERVICE" >> /tmp/spectero.installconfig;
    echo "symlink=$SYMLINK" >> /tmp/spectero.installconfig;
}

function WORK_INSTALL_SPECTERO() {
cat << EOF > "/tmp/spectero-installer.py"
#!/usr/bin/env python3

###############################################################################
##
##  Spectero, Inc
##  Copyright (c) 2018 - All rights reserved
##  https://spectero.com
##
##  This python script is a special case, and should not be used without
##  the proper 'install.sh'.
##
##  The bash script produces a configuration that this script is designed
##  to read. Although you can write your own configuration if you
##  study this script, it is not advised.
##
##  This script is also subject to Spectero's Terms of Service.
##  You may find this document at https://spectero.com/tos
##
###############################################################################

import json
import os
import subprocess
import sys
import traceback
import urllib.request
import platform

config = {}
releases = {}
sources = {}


def exception_config_missing():
    print("The installer requires a proper configuration file.")
    print("Please make sure you ran the install.sh")
    print("If this issue still exists, please report it to the developers.")
    sys.exit(1)


def exception_version_not_found():
    print("The version you provided does not exist, please re-run the installer with a valid version.")
    sys.exit(1)


def exception_channel_not_found():
    print("The release channel you provided does not exist.")
    print("Please run the installer with the --help flag for a list of valid channels.")
    sys.exit(1)


def exception_version_not_available_for_download():
    print("The version '%s' is not available for downloading." % config["version"])
    sys.exit(1)


def exception_no_download_link():
    print("There is no download link available for this version: %s" % config["version"])
    print("Please report this to Spectero Developers.")
    sys.exit(1)


def exception_cannnot_read_popen(popen):
    print("Failed to read dotnet output")
    print(popen)
    sys.exit(1)


def read_config():
    global config

    # Check if install instructions exist
    if not os.path.isfile("/tmp/spectero.installconfig"):
        exception_config_missing()

    # Read the install instructions
    with open("/tmp/spectero.installconfig", 'rU') as f:
        for line_terminated in f:
            line = line_terminated.rstrip('\n')
            splitline = line.split('=')
            key = splitline[0]
            value = None
            if len(splitline) > 2:
                splitline = splitline.pop(0)
                value = splitline.join("=")

            elif len(splitline) == 2:
                value = splitline[1]

            # Update the dictionary
            config[key] = value

    print("Installation config successfully loaded.")


def get_install_directory_from_config():
    new = config["directory"]
    if not new.endswith('/'):
        new = new + "/"
    return new


def get_dotnet_destination():
    return get_install_directory_from_config() + "latest/dotnet"


def local_dotnet_core_installer():
    # Local dotnet core was not installed.
    if not os.path.isdir(get_dotnet_destination()):
        tmp_path = "/tmp/spectero-dotnet-install.tar.gz"
        link = get_dotnet_runtime_link()

        # download, create the directory, extract.
        os.system("wget %s -O %s -q" % (link, tmp_path))
        os.system("mkdir -p %s" % get_dotnet_destination())
        os.system("tar -xf %s -C %s" % (tmp_path, get_dotnet_destination()))

    # Get the downloaded dotnet executable path.
    return get_dotnet_destination() + "/dotnet"


def get_repository_dotnet_version():
    return releases["versions"][config["version"]]["requiredDotnetCoreVersion"]


def is_dotnet_version_compatible(installed_version):
    requirement = get_repository_dotnet_version()
    split_requirement = requirement.split('.')
    installed_version_split = installed_version.split('.')

    # Iterate through each.
    for i in range(0, len(split_requirement)):
        # Check to see if the requirement is higher.
        if int(split_requirement[i]) > int(installed_version_split[i]):
            # We need a local install.
            return False

    # We can use the currently installed version of dotnet core.
    return True


def get_dotnet_core_path():
    try:
        # Attempt to get the system install, will determine if it exists.
        result = which("dotnet")

        # Find the version
        info_output = subprocess.check_output([result, "--info"]).decode('utf-8').split('\n')

        # Check if the version is compatible.
        for line in info_output:
            if "Version: " in line:
                current_line = line
                current_line = current_line.strip()
                version_numbers = current_line.split("Version:")[1].strip()
                if is_dotnet_version_compatible(version_numbers):
                    # The version is compatable.
                    return result
                else:
                    # The version wasn't compatible.
                    return local_dotnet_core_installer()

        # Throw exception and print lines for debugging.
        exception_cannnot_read_popen(info_output)
    except:
        # Perform a local install
        return local_dotnet_core_installer()


def get_download_channel_information():
    global releases
    request = urllib.request.Request('https://c.spectero.com/releases.json')
    result = urllib.request.urlopen(request)
    releases = json.loads(result.read().decode('utf-8'))


def validate_user_requests_against_releases():
    global releases, config

    # Check to make sure we are a valid branch.
    print("Validating that %s is a valid release channel..." % config["branch"])
    if config["branch"] not in releases["channels"]:
        exception_channel_not_found()

    # Convert latest to the latest branch string
    if config["version"] == "latest":
        print("Resolving 'latest' version...")
        config["version"] = releases["channels"][config["branch"]]
        print("The latest version for branch '%s' is %s" % (config["branch"], config["version"]))

    # Check if we can download this version.
    print("Making sure that %s is a valid release..." % config["version"])
    if config["version"] not in releases["versions"]:
        exception_version_not_available_for_download()

    # Check to make sure there's a download link
    print("Getting download links for %s..." % config["version"])
    if releases["versions"][config["version"]] is None:
        exception_no_download_link()
    else:
        print("Link found: %s" % releases["versions"][config["version"]]["download"])


def download_and_extract():
    global releases, config

    # Create the working directory.
    os.system("mkdir -p %s%s" % (get_install_directory_from_config(), config["version"]))

    # Get the URL for the version.
    url = releases["versions"][config["version"]]["download"]

    # Build a easy to access path.
    path = ("%s%s.zip" % (get_install_directory_from_config(), config["version"]))

    # Download
    print("Invoking wget to download files...")
    os.system("wget %s -O %s -q" % (url, path))

    # Extract
    print("Invoking unzip to extract files...")
    os.system("unzip -qq -u %s -d %s%s" % (path, get_install_directory_from_config(), config["version"]))

    # Cleanup
    os.system("rm %s" % path)
    print("Extract and download procedure complete.")


def create_latest_symlink():
    if config["symlink"] == "true":
        path = ("%s%s" % (get_install_directory_from_config(), config["version"]))
        symlink_path = get_install_directory_from_config() + "latest"

        if os.path.islink(symlink_path):
            print("Old symlink exists, removing...")
            os.system("unlink %s" % symlink_path)

        print("Creating symlink %s to %s" % (symlink_path, path))
        os.system("ln -s %s %s" % (path, symlink_path))
    else:
        print("User has specified that they do not want a symlink - skipping this step.")


def which(command):
    with open(os.devnull, 'w') as devnull:
        return (subprocess.check_output(["which", command], stderr=devnull)[:-1]).decode("utf-8")


def fix_permissions():
    print("Fixing directory ownership...")
    os.system("chown -R spectero:spectero %s" % get_install_directory_from_config())
    os.system("chmod -R 744 %s" % get_install_directory_from_config())
    os.system("usermod -m -d %s spectero" % get_install_directory_from_config()[:-1])


def create_systemd_service():
    if config["service"] == "true":
        try:
            systemd_script_destination = "/etc/systemd/system/spectero.service"
            systemd_script_template = "%s%s/daemon/Tooling/Linux/spectero.service" % (
            get_install_directory_from_config(), config["version"])

            # Open a reader to the template
            with open(systemd_script_template, 'r') as file:
                filedata = file.read()

            # Replace template data here
            filedata = filedata.replace("ExecStart=/usr/bin/dotnet", "ExecStart=" + get_dotnet_core_path())

            # Open a writer to the template location.
            with open(systemd_script_template, 'w') as file:
                file.write(filedata)

            # Copy to system directory
            os.system("cp %s %s" % (systemd_script_template, systemd_script_destination))

            # Reload the systemd daemon
            os.system("systemctl daemon-reload &> /dev/null")

            # Enable the process
            os.system("systemctl enable spectero > /dev/null 2>&1")

            # Attempt to start the process
            print("Using systemctl to start spectero service.")
            os.system("systemctl start spectero")
            print("Getting service status... (You may have to press CTRL+C)")
            os.system("systemctl status spectero")
        except Exception as e:
            traceback.print_exc()
            print("The installer encountered a problem while configuring the systemd service.")
            print("Please report this problem.")
    else:
        print("User has specified that they do not want a service for the Spectero Daemon - skipping this step.")


def create_user():
    # Create User, Group and assign.
    if sys.platform in ["linux", "linux2"]:
        os.system("useradd spectero")
        os.system("groupadd spectero")
        os.system("usermod -a -G spectero spectero")


def update_sudoers():
    template = "Cmnd_Alias SPECTERO_CMDS = {systemctl} start spectero, {systemctl} stop spectero, {systemctl} status spectero, {systemctl} restart spectero, {iptables}, {openvpn}, {which}"

    # Replace the string templates.
    for command in ["systemctl", "iptables", "openvpn", "which"]:
        template = template.replace("{%s}" % command, which(command))

    # Check if sudoers exists
    if os.path.exists('/etc/sudoers'):
        if template not in open('/etc/sudoers').read():
            with open('/etc/sudoers', "a") as sudoers:
                sudoers.write(template + "\n" + "spectero ALL=(ALL) NOPASSWD:SPECTERO_CMDS\n")


def linux_enable_ipv4_forwarding():
    # Defined property of what needs to be checked and assigned
    property = "net.ipv4.ip_forward"
    try:
        # Try to execute
        result = (subprocess.check_output(["sysctl", property])[:-1]).decode("utf-8")

        # Check if it is disabled
        if result == "%s = 0" % property:
            # Enable ip forwarding
            print("Enabling IPv4 Forwarding")
            os.system("""echo "%s = 1" >> /etc/sysctl.conf""" % property)
    except:
        print("There was a problem attempting to check for kernel flag: %s." % property)
        sys.exit(1)


def linux_enable_fs_max():
    # Defined property of what needs to be checked and assigned
    property = "fs.file-max"
    val = 2097152
    try:
        # Try to execute
        result = (subprocess.check_output(["sysctl", property])[:-1]).decode("utf-8")

        # Check if it is disabled
        if result == "%s = %s" % (property, val):
            # Enable ip forwarding
            print("Adjusting %s value" % property)
            os.system("""echo "%s = 1" >> /etc/sysctl.conf""" % property)
    except:
        print("There was a problem attempting to check for kernel flag: %s." % property)
        sys.exit(1)


def linux_enable_ipv4_reuse():
    # Defined property of what needs to be checked and assigned
    property = "net.ipv4.tcp_tw_reuse"
    val = 1
    try:
        # Try to execute
        result = (subprocess.check_output(["sysctl", property])[:-1]).decode("utf-8")

        # Check if it is disabled
        if result == "%s = %s" % (property, val):
            # Enable ip forwarding
            print("Adjusting %s value" % property)
            os.system("""echo "%s = 1" >> /etc/sysctl.conf""" % property)
    except:
        print("There was a problem attempting to check for kernel flag: %s." % property)
        sys.exit(1)


def linux_enable_ipv4_timeout():
    # Defined property of what needs to be checked and assigned
    property = "net.ipv4.tcp_fin_timeout"
    val = 10
    try:
        # Try to execute
        result = (subprocess.check_output(["sysctl", property])[:-1]).decode("utf-8")

        # Check if it is disabled
        if result == "%s = %s" % (property, val):
            # Enable ip forwarding
            print("Adjusting %s value" % property)
            os.system("""echo "%s = 1" >> /etc/sysctl.conf""" % property)
    except:
        print("There was a problem attempting to check for kernel flag: %s." % property)
        sys.exit(1)

def set_ulimit_spectero_user():
    filepath = "/etc/security/limits.conf"

    # String replacement.
    with open(filepath, 'r') as file:
        filedata = file.read()

    # Strings
    soft_limit = "spectero soft nofile 500000"
    hard_limit = "spectero hard nofile 500000"

    # Append Spectero specific limit.
    if soft_limit  not in filedata:
        print("Setting soft file descriptor ulimit for specctero user...")
        filedata += ("\n" + soft_limit)
    if hard_limit not in filedata:
        print("Setting hard file descriptor ulimit for specctero user...")
        filedata += ("\n" + hard_limit)

    with open(filepath, 'w') as file:
        file.write(filedata)


def reload_sysctl():
    print("Reloading sysctl configuration...")
    os.system("sysctl --system > /dev/null 2>&1")  # Needs more aggressive suppression.


def create_shell_script():
    try:
        cli_script = get_install_directory_from_config() + config["version"] + "/cli/Tooling/spectero"
        cli_script_destination = "/usr/local/bin/spectero"

        # String replacement.
        with open(cli_script, 'r') as file:
            filedata = file.read()
        print("Replacing variables in console management interface template...")
        filedata = filedata.replace("{dotnet path}", get_dotnet_core_path())
        filedata = filedata.replace("{spectero working directory}", get_install_directory_from_config())
        filedata = filedata.replace("{version}", config["version"])

        with open(cli_script, 'w') as file:
            file.write(filedata)

        print("Copying console management interface shell script to /usr/local/bin/spectero")
        os.system("cp %s %s" % (cli_script, cli_script_destination))
        os.system("chmod +x %s" % cli_script_destination)
    except Exception as e:
        traceback.print_exc()
        print("The installer encountered a problem while copying the CLI script.")
        print("Please report this problem.")
        sys.exit(12)


def get_sources_information():
    global sources
    try:
        request = urllib.request.Request(
            'https://raw.githubusercontent.com/ProjectSpectero/daemon-installers/master/SOURCES.json')
        result = urllib.request.urlopen(request)
        sources = json.loads(result.read().decode('utf-8'))
    except:
        request = urllib.request.Request(
            'https://raw.githubusercontent.com/ProjectSpectero/daemon-installers/development/SOURCES.json')
        result = urllib.request.urlopen(request)
        sources = json.loads(result.read().decode('utf-8'))


def get_dotnet_runtime_link():
        return sources["linux"]["dotnet"]["x64"]


def exception_unsupported_os():
    print("The python installer script is currently incompatible with this OS.")
    print("Please file an issue with the following information:")
    print("="*40)
    print("sys.platform\t\t=\t%s" % sys.platform)
    print("platform.architecture\t\t=>\t%s" % platform.architecture())
    sys.exit(1)


if __name__ == "__main__":
    read_config()
    get_download_channel_information()
    get_sources_information()
    validate_user_requests_against_releases()
    download_and_extract()

    if sys.platform in ["linux", "linux2"]:
        create_user()
        set_ulimit_spectero_user()

    else:
        exception_unsupported_os()

    fix_permissions()

    if sys.platform in ["linux", "linux2"]:
        update_sudoers()

    create_latest_symlink()

    if sys.platform in ["linux", "linux2"]:
        linux_enable_ipv4_forwarding()
        linux_enable_fs_max()
        linux_enable_ipv4_reuse()
        linux_enable_ipv4_timeout()
        reload_sysctl()
        create_systemd_service()

    create_shell_script()

EOF
python3 /tmp/spectero-installer.py

if [[ $? != "0" ]]; then
    EXCEPTION_PYTHON_INSTALLER
fi
}

##### ==========================
##### DETECTORS
##### ==========================
function DETECT_PACKAGE_MANAGER() {
    clear;

    # Find the package manager
    DNF_CMD=$(which dnf 2> /dev/null);
    YUM_CMD=$(which yum 2> /dev/null);
    APT_GET_CMD=$(which apt-get 2> /dev/null);

    # Fedora (dandified)
    if [[ ! -z $DNF_CMD ]]; then
        echo "Detected DNF as the operating system's package manager.";

    # CentOS/RHEL (Server based)
    elif [[ ! -z $YUM_CMD ]]; then
        echo "Detected YUM as the operating system's package manager."

    # Debian/Ubuntu (Desktop and Server Distros).
    elif [[ ! -z $APT_GET_CMD ]]; then
        echo "Detected APT-GET as the operating system's package manager."

        # apt-get specifically needs an update to the repositories.
        echo "Updating the package manager's sources...";
        echo "(This can take a minute)";
        apt-get update &> /dev/null;
    else
        EXCEPTION_INCOMPATIBLE_PACKAGE_MANAGER
    fi
}

function DETECT_SYSTEMD() {
    if [ "$(uname)" == "Linux" ]; then
        if [ ! -e /etc/os-release ]; then
            EXCEPTION_SYSTEMD_NOT_FOUND
        fi
    fi
}

function DETECT_PROGRAM_SUDO() {
    which sudo &> /dev/null;
    if [[ $? != 0 ]]; then
        WORK_INSTALL_SUDO
    else
        echo "Dependency exists: sudo - skipping installation.";
    fi
}

function DETECT_PROGRAM_BREW() {
    which brew &> /dev/null;
    if [[ $? != 0 ]]; then
        WORK_INSTALL_BREW
    else
        echo "Dependency exists: brew - skipping installation.";
    fi
}

function DETECT_PROGRAM_PYTHON3() {
    which python3 &> /dev/null;
    if [[ $? != 0 ]]; then
        WORK_INSTALL_PYTHON3
    else
        echo "Dependency exists: python3 - skipping installation.";
    fi
}

function DETECT_PROGRAM_OPENVPN() {
    which openvpn &> /dev/null;
    if [[ $? != 0 ]]; then
        WORK_INSTALL_OPENVPN
    else
        echo "Dependency exists: openvpn - skipping installation.";
    fi
}

function DETECT_PROGRAM_WGET() {
    which wget &> /dev/null;
    if [[ $? != 0 ]]; then
        WORK_INSTALL_WGET
    else
        echo "Dependency exists: wget - skipping installation.";
    fi
}

function DETECT_EPEL_REPO() {
    if [[ $ID == "centos" ]]; then
        rpm -qa | grep epel-release &> /dev/null;
        if [[ $? == "1" ]]; then
            WORK_INSTALL_EPEL_REPO;
        fi
    fi
}

function DETECT_PROGRAM_UNZIP() {
    which unzip &> /dev/null;
    if [[ $? != 0 ]]; then
        WORK_INSTALL_UNZIP
    else
        echo "Dependency exists: unzip - skipping installation.";
    fi
}

function DETECT_PROGRAM_IPTABLES() {
    which iptables &> /dev/null;
    if [[ $? != 0 ]]; then
        WORK_INSTALL_IPTABLES
    else
        echo "Dependency exists: iptables - skipping installation.";
    fi
}

function DETECT_PROGRAM_DOTNET_CORE() {
    which dotnet &> /dev/null;
    if [[ $? != 0 ]]; then
        WORK_INSTALL_DOTNET_CORE
    else
        echo "Dependency exists: dotnet - skipping installation.";
    fi
}

function DETECT_OSX_USER_IS_ROOT() {
    if [ "$(uname)" == "Darwin" ]; then
        if [[ $EUID -ne 0 ]]; then
            EXCEPTION_OSX_USER_CANNOT_BE_ROOT
        fi
    fi
}

function DETECT_LINUX_USER_IS_ROOT() {
    if [ "$(uname)" == "Linux" ]; then
        if [[ $EUID -ne 0  ]]; then
            EXCEPTION_LINUX_USER_NEEDS_ROOT
        fi
    fi
}


##### =====================================
##### MAIN ROUTINE
##### =====================================
# Validate we are the right user for the operating system.
DETECT_OSX_USER_IS_ROOT
DETECT_LINUX_USER_IS_ROOT

# Make sure we are a supported distribution if we're linux.
DETECT_SYSTEMD

# Parse the passed arguments.
while [ $# -ne 0 ]
do
    name="$1"
    case "$name" in
        -a|--agree|-[Aa]gree)
            TOS_PROMPT="false";
            ;;
        -ai|--install|-[Ii]nstall)
            INSTALL_PROMPT="false";
            ;;
        -b|--branch|--[Br]ranch|-c|--channel|-[Cc]hannel)
            shift
            BRANCH="$1";
            ;;
        -v|--version|--[Vv]ersion)
            shift
            BRANCH_VERSION="$1";
            ;;
        -loc|--location|-[Ll]ocation|-d|--dir|--directory|-[Dd]irectory)
            shift
            INSTALL_LOCATION="$1"
            ;;
        -nsl|--no-sl|-[Nn]o[Ss]ymlink)
            SYMLINK="false";
            ;;
        -nsds|--no-systemd|-[Nn]o[Ss]ystemd)
            SERVICE="false";
            ;;
        -u|--uninstall|-[Uu]ninstall)
            WORK_UNINSTALL;
            ;;
        -?|--?|-h|--help|-[Hh]elp)
            EXCEPTION_MAN_PAGE;
            ;;
        *)
            echo "Unknown argument \`$name\`"
            exit 1
            ;;
    esac
    shift
done

# Load environment variables if we're linux.
LOAD_SYSTEND_DISTRIBUTION_INFORMATION;

# Print the welcome message and the agreement of the terms of service.
PRINT_GREETINGS;
PRINT_TERMS_OF_SERVICE;

# Prompt the users for the two options.
PRINT_PROMPT_INSTALL_LOCATION;
PRINT_PROMPT_READY_TO_INSTALL;

# Write the configuration to the disk.
WORK_WRITE_CONFIG

# Detect packages that either the installer or daemon needs, and install them.
DETECT_EPEL_REPO;
DETECT_PROGRAM_SUDO;
DETECT_PROGRAM_BREW;
DETECT_PACKAGE_MANAGER;
DETECT_PROGRAM_WGET;
DETECT_PROGRAM_UNZIP
DETECT_PROGRAM_OPENVPN;
DETECT_PROGRAM_PYTHON3;
DETECT_PROGRAM_DOTNET_CORE;
DETECT_PROGRAM_IPTABLES

# Install the daemon into the system.
WORK_INSTALL_SPECTERO;

# Let the user know the install is complete.
PRINT_INSTALL_COMPLETE;

# Exit gracefully.
exit 0;
