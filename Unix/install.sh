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
TOS_PROMPT="true";
INSTALL_PROMPT="true";

##### ==========================
##### EXCEPTIONS
##### ==========================
function EXCEPTION_USER_ABORT(){
    PRINT_SPACER;
    echo "You did not agree to the Terms of Service.";
    echo "";
    echo "Spectero did not install.";
    exit 1; # General software error.
}

function EXCEPTION_TERMS_OF_SERVICE_AGREEMENT() {
    if [ $TOS_PROMPT != "false" ]; then
        PRINT_SPACER;
        echo "You did not agree to the Terms of Service.";
        echo "";
        echo "Spectero did not install.";
        exit 1; # General software error.
    fi
}

function EXCEPTION_SYSTEMD_NOT_FOUND() {
    PRINT_SPACER;
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
    PRINT_SPACER;
    exit 126; # Brew cannot execute as root - "COMMAND INVOKED CANNOT EXECUTE"
}

function EXCEPTION_LINUX_USER_NEEDS_ROOT(){
    PRINT_SPACER;
    echo "This script must be ran as root.";
    exit 126;
}

function EXCEPTION_OSX_USER_CANNOT_BE_ROOT(){
    PRINT_SPACER;
    echo "The installation script requires the `brew` package manager, and cannot be run as root.";
    exit 126;
}

function EXCEPTION_INCOMPATIBLE_PACKAGE_MANAGER() {
    PRINT_SPACER;
    echo "The installer failed to find a compatible package manager.";
    echo "Supported Package Managers";
    echo "    ----> apt-get, apt, dnf (Debian / Ubuntu / Fedora / End user package managers)"
    echo "    ----> yum (CentOS / Red Hat Enterprise Linux / Server based package managers)"
    echo "";
    echo "Spectero did not install.";
    exit 127; # The command was not found
}

function EXCEPTION_MAN_PAGE() {
    echo "  -a=, --agree=   |   Agree to the Spectero Terms of Service.";
    echo "";
    echo "  -ai, --install  |   Automatically install without any prompt for confirmation.";
    echo "";
    echo "  -b=, --branch=  |   Specify the release channel that the installer will use.";
    echo "                  |   Potential possibilities:";
    echo "                  |   'stable' - The mature channel that is ready for production (DEFAULT)";
    echo "                  |   'beta'   - Get new features as they are implemented, may contain bugs.";
    echo "                  |   'alpha'  - Get the newest things that developers implement; (Will contain bugs, not suitable for production)";
    echo "";
    echo "  -d=, --dir=     |   The location of where Spectero should install.";
    ehco "";
    echo "  -h, --help      |   Displays this page.";
    echo "";
    exit 0;
}


##### ==========================
##### CONSOLE
##### ==========================
function PRINT_SPACER() {
    echo "===============================================================";
}

function PRINT_GREETINGS() {
    echo "Welcome to the Installation Wizard Script for Spectero Daemon";
}

function PRINT_TERMS_OF_SERVICE() {
    echo "Spectero's Daemon comes with a standard Terms of Service Agreement";
    echo "This document can be found at https://spectero.com/tos";
    PRINT_SPACER;
    echo "Do you agree to the Terms of Service? (no/yes)";

    # Read the response
    read TOS_AGREEMENT_INPUT;

    # Check if not yes
    if [ $TOS_AGREEMENT_INPUT != "yes" ]; then
        EXCEPTION_TERMS_OF_SERVICE_AGREEMENT
    fi
}

function PRINT_PROMPT_INSTALL_LOCATION () {
    if [ $INSTALL_PROMPT == "true" ]; then
        echo "By default, Spectero installs into the following directory:";
        echo $INSTALL_LOCATION;
        PRINT_SPACER;
        echo "Please press enter to accept this path as an installation directory, or provide a directory below:";

        # Read the response
        read USER_SPECIFIED_DIRECTORY;
     else
        USER_SPECIFIED_DIRECTORY="";
    fi

    # Check if not yes
    if [[ $USER_SPECIFIED_DIRECTORY != "" ]]; then
        INSTALL_LOCATION=$USER_SPECIFIED_DIRECTORY;
        mkdir -p $INSTALL_LOCATION
    fi
}

function PRINT_PROMPT_READY_TO_INSTALL() {
    echo "The installer has gathered enough information to start installation.";
    PRINT_SPACER;
    echo "Would you like to start the installation? (yes/no)";

    # Read the response
    read CONTINUE;

    # Check if not yes
    if [ $CONTINUE != "yes" ]; then
        EXCEPTION_USER_ABORT
    fi
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
        which -s brew;
        if [[ $? != 0 ]]; then
            sudo ruby -e "$(curl -FsSL https://raw.githubusercontent.com/Homebrew/install/master/install)"
        fi
    fi
}

function WORK_INSTALL_SUDO() {
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
    if [ "$(uname)" == "Darwin" ]; then # OS X
        brew install sudo;
    elif [ "$(uname)" == "Linux" ]; then
        echo "Dependency Check: python3 will be installed."

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
    if [ "$(uname)" == "Darwin" ]; then
        brew install wget;
    elif [ "$(uname)" == "Linux" ]; then
        if [[ ! -z $DNF_CMD ]]; then
            dnf install wget -y;
        elif [[ ! -z $YUM_CMD ]]; then
            yum install wget -y;
        elif [[ ! -z $APT_GET_CMD ]]; then
            apt-get install wget -y;
        fi
    fi
}

function WORK_INSTALL_DOTNET_CORE() {
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
    # IMPLEMENT HEREDOC HERE.
    # Python can do the intelegent bits that bash cannot.
    echo "Pending implementation. If you've made it here the installer works so far."
}

function WORK_PARSAE_ARGUMENTS() {
    for i in "$@"
        do
            case $i in
                -a|--agree)
                    TOS_PROMPT="false";
                    shift # past argument=value
                ;;
                -ai|--install)
                    INSTALL_PROMPT="false";
                    shift # past argument=value
                ;;
                -b=*|--branch=*)
                    BRANCH="${i#*=}";
                    shift # past argument=value
                ;;
                -d=*|--install-location=*)
                    INSTALL_LOCATION="${i#*=}"
                    shift # past argument=value
                ;;
                -h|--help)
                    EXCEPTION_MAN_PAGE;
                    shift # past argument=value
                ;;
                *)
                    echo "";
                ;;
            esac
        done
}


##### ==========================
##### DETECTORS
##### ==========================
function DETECT_PACKAGE_MANAGER() {
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
        apt-get update;
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

function DETECT_PROGRAM_BREW() {
    which -s brew;
    if [[ $? != 0 ]]; then
        WORK_INSTALL_BREW
    fi
}

function DETECT_PROGRAM_PYTHON3() {
    which -s python3;
    if [[ $? != 0 ]]; then
        WORK_INSTALL_PYTHON3
    fi
}

function DETECT_PROGRAM_OPENVPN() {
    which -s openvpn;
    if [[ $? != 0 ]]; then
        WORK_INSTALL_OPENVPN
    fi
}

function DETECT_PROGRAM_WGET() {
    which -s wget;
    if [[ $? != 0 ]]; then
        WORK_INSTALL_WGET
    fi
}

function DETECT_PROGRAM_DOTNET_CORE() {
    which -s dotnet;
    if [[ $? != 0 ]]; then
        WORK_INSTALL_DOTNET_CORE
    fi
}

function DETECT_OSX_USER_IS_ROOT() {
    if [ "$(uname)" == "Darwin" ]; then
        if [[ $EUID -ne 0 ]]; then
            EXCEPTION_LINUX_USER_NEEDS_ROOT
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
WORK_PARSAE_ARGUMENTS;

# Load environment variables if we're linux.
LOAD_SYSTEND_DISTRIBUTION_INFORMATION

# Print the welcome message and the agreement of the terms of service.
PRINT_GREETINGS
PRINT_TERMS_OF_SERVICE

# Prompt the users for the two options.
PRINT_PROMPT_INSTALL_LOCATION
PRINT_PROMPT_READY_TO_INSTALL

# Detect packages that either the installer or daemon needs, and install them.
DETECT_PROGRAM_BREW
DETECT_PACKAGE_MANAGER
DETECT_PROGRAM_WGET
DETECT_PROGRAM_OPENVPN
DETECT_PROGRAM_PYTHON3
DETECT_PROGRAM_DOTNET_CORE

# Install the daemon into the system.
WORK_INSTALL_SPECTERO

# Exit gracefully.
exit 0;
