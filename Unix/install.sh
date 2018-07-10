#!/usr/bin/env bash

###############################################################################
##
##  Spectero, Inc
##  Copyright (c) 2018 - All rights reserved
##  https://spectero.com
##
##  For command line arguments, please refer to our confluence documentation
##  <LINK TO CONFLUENCE>
##
###############################################################################


##### ==========================
##### VARIABLES
##### ==========================
DOTNET_CORE_VERSION="2.1.1";
INSTALL_LOCATION="/opt/spectero";
BRANCH="stable";
VERSION="latest"
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
    echo "                          |      (This option is enabled by default)";
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
    if [[ $TOS_PROMPT == "yes" ]]; then
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

    # Placeholder.
    USER_SPECIFIED_DIRECTORY="";

    if [ $INSTALL_PROMPT == "true" ]; then
        echo "By default if you press enter, Spectero will install into the following directory: $INSTALL_LOCATION";
        echo "Please press enter to accept this path as an installation directory, or provide a directory below:";

        # Read the response
        read USER_SPECIFIED_DIRECTORY;
    fi

    # Check if not yes
    if [[ $USER_SPECIFIED_DIRECTORY != "" ]]; then
        INSTALL_LOCATION=$USER_SPECIFIED_DIRECTORY;
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
    clear;

    echo "Installation of the Spectero Daemon has completed successfully."
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
    echo "The `sudo` utility will now be installed.";
    if [ "$(uname)" == "Darwin" ]; then # OS X
        brew install sudo;
    elif [ "$(uname)" == "Linux" ]; then
        if [[ ! -z $DNF_CMD ]]; then
            dnf install sudo -y;
        elif [[ ! -z $YUM_CMD ]]; then
            yum install sudo -y;
        elif [[ ! -z $APT_GET_CMD ]]; then
            apt-get install sudo -y;
        fi
    fi
}

function WORK_INSTALL_PYTHON3() {
    echo "The `python3` executable will now be installed.";
    if [ "$(uname)" == "Darwin" ]; then # OS X
        brew install sudo;
    elif [ "$(uname)" == "Linux" ]; then
        if [[ ! -z $DNF_CMD ]]; then
            dnf install python34 -y;
        elif [[ ! -z $YUM_CMD ]]; then
            yum install python34 -y;
        elif [[ ! -z $APT_GET_CMD ]]; then
            apt-get install python3 -y;
        fi
    fi
}

function WORK_INSTALL_OPENVPN() {
    echo "The `openvpn` utility will now be installed.";
    if [ "$(uname)" == "Darwin" ]; then
        brew install openvpn;
    elif [ "$(uname)" == "Linux" ]; then
        if [[ ! -z $DNF_CMD ]]; then
            dnf install openvpn -y;
        elif [[ ! -z $YUM_CMD ]]; then
            yum install openvpn -y;
        elif [[ ! -z $APT_GET_CMD ]]; then
            apt-get install openvpn -y;
        fi
    fi
}

function WORK_INSTALL_WGET() {
    echo "The `sudo` utility will now be installed.";
    if [ "$(uname)" == "Linux" ]; then
        if [[ ! -z $DNF_CMD ]]; then
            dnf install sudo -y;
        elif [[ ! -z $YUM_CMD ]]; then
            yum install sudo -y;
        elif [[ ! -z $APT_GET_CMD ]]; then
            apt-get install sudo -y;
        fi
    fi
}

function WORK_INSTALL_DOTNET_CORE() {
    echo "The `dotnet` framework will now be installed.";
    if [[ ! -z $DNF_CMD ]]; then
        if [[ $ID == "fedora" ]]; then
           dnf install libunwind-devel libcurl-devel libicu compat-openssl10 -y;
        fi
    elif [[ ! -z $YUM_CMD ]]; then
        # Generic Dependency Install
        echo "The installer was unable to determine the variant of linux."
        echo "Your package manager is yum, and we will try to install the required dependencies."
        echo "If the installation fails, please report the issue to"
        echo "https://github.com/ProjectSpectero/daemon-installers/issues"
        ehco "So Spectero can implement support for your operating system."
        yum install libunwind-devel libcurl-devel libicu -y;
    elif [[ ! -z $APT_GET_CMD ]]; then
        # Ubuntu
        if [ $ID == "ubuntu" ]; then
            if [ $VERSION_ID == "18.04" ]; then
                echo "Detected Operating System: Ubuntu 18.04 LTS";
                echo "Installing dependencies for dotnet core framework."
                apt-get install libunwind-dev libcurl4 -y;
            else
                echo "Detected Operating System: Ubuntu (unknown version)";
                echo "Installing dependencies for dotnet core framework."
                apt-get install libunwind-dev libcurl4-openssl-dev -y;
            fi
        elif [ $ID == "debian" ]; then
            if [ $VERSION_ID == "9" ]; then
                echo "Detected Operating System: Debian 9";
                echo "Installing dependencies for dotnet core framework."
                apt-get install libicu-dev libunwind-dev libcurl4-openssl-dev -y;
            else
                echo "Detected Operating System: Debian (unknown version)";
                echo "Installing dependencies for dotnet core framework."
                apt-get install libunwind-dev libcurl4-openssl-dev -y;
            fi
        else
            echo "The installer was unable to determine the variant of linux."
            echo "Your package manager is apt-get, and we will try to install the required dependencies."
            echo "If the installation fails, please report the issue to"
            echo "https://github.com/ProjectSpectero/daemon-installers/issues"
            ehco "so Spectero can implement support for your operating system."
            apt-get install libunwind-dev libcurl4-openssl-dev -y;
        fi
    fi

    # Download .NET Core Installation script
    echo "Downloading dotnet core installation script."
    wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh > /dev/null

    # Install
    echo "Installing Microsoft .NET Core Framework...";
    bash /tmp/dotnet-install.sh \
        --version $DOTNET_CORE_VERSION \
        --runtime aspnetcore \
        --install-dir /usr/local/bin/ \
        2> /dev/null
}

function WORK_INSTALL_SPECTERO() {
    # Write all data to a config
    echo "directory=$INSTALL_LOCATION" > /tmp/spectero.installconfig
    echo "overwrite=$OVERWRITE" >> /tmp/spectero.installconfig
    echo "branch=$BRANCH" >> /tmp/spectero.installconfig
    echo "version=$VERSION" >> /tmp/spectero.installconfig
    echo "service=$SERVICE" >> /tmp/spectero.installconfig
    echo "symlink=$SYMLINK" >> /tmp/spectero.installconfig

    # Download the python side of things, python will read the above file and handle the intricate parts.
    wget -O - https://raw.githubusercontent.com/ProjectSpectero/daemon-installers/master/Unix/install.py &> /dev/null | sudo python3
}

function WORK_KERNEL_CHANGE_FORWARDING() {

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
            VERSION="$1";
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

# Change kernel settings
WORK_KERNEL_CHANGE_FORWARDING

# Detect packages that either the installer or daemon needs, and install them.
DETECT_PROGRAM_SUDO;
DETECT_PROGRAM_BREW;
DETECT_PACKAGE_MANAGER;
DETECT_PROGRAM_WGET;
DETECT_PROGRAM_OPENVPN;
DETECT_PROGRAM_PYTHON3;
DETECT_PROGRAM_DOTNET_CORE;

# Install the daemon into the system.
WORK_INSTALL_SPECTERO;

# Let the user know the install is complete.
PRINT_INSTALL_COMPLETE;

# Exit gracefully.
exit 0;
