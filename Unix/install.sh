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

DOTNET_CORE_VERSION = "2.1.1";


##### ==========================
##### CONSOLE ADDITIONS
##### ==========================
function PRINT_SPACER() {
    echo "===============================================================";
}

function PRINT_GREETINGS() {
    echo "Welcome Installation Wizard Script for Spectero Daemon";
}

function PRINT_TERMS_OF_SERVICE() {
    echo "Spectero\'s Daemon comes with a standard Terms of Service Agreement";
    echo "This document can be found at https://spectero.com/tos";
    echo "";
    echo "Do you agree to the Terms of Service? (no/yes)";

    # Read the response
    read tos_agreement;

    # Check if not yes
    if [ $tos_agreement != "yes" ]; then
        EXCEPTION_TERMS_OF_SERVICE_AGREEMENT
    fi
}


##### ==========================
##### EXCEPTIONS
##### ==========================
function EXCEPTION_TERMS_OF_SERVICE_AGREEMENT() {
    PRINT_SPACER
    echo "You did not agree to the Terms of Service.";
    echo "";
    echo "Spectero did not install.";
    exit 1; # General software error.
}

function EXCEPTION_SYSTEMD_NOT_FOUND() {
    PRINT_SPACER
    echo "Spectero Daemon Installer has encountered a problem.";
    echo "The Operating System this script is running on doesn't appear to utilize the systemd init system.";
    echo "Spectero\s daemon specifically works for systemd.";
    echo "You can find what operating systems Spectero is compatible on the website:";
    echo "https://spectero.com/downloads";
    PRINT_SPACER
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
    PRINT_SPACER
    echo "This script must be ran as root.";
    exit 126;
}

function EXCEPTION_OSX_USER_CANNOT_BE_ROOT(){
    PRINT_SPACER
    echo "The installation script requires the \`brew\` package manager, and cannot be run as root.";
    exit 126;
}

function EXCEPTION_INCOMPATIBLE_PACKAGE_MANAGER() {
    PRINT_SPACER
    echo "The installer failed to find a compatible package manager.";
    echo "Supported Package Managers";
    echo "    ----> apt-get, apt, dnf (Debian / Ubuntu / Fedora / End user package managers)"
    echo "    ----> yum (CentOS / Red Hat Enterprise Linux / Server based package managers)"
    PRINT_SPACER
    echo "Spectero did not install.";
    exit 127; # The command was not found
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
        EXCEPTION_INCOMPATABLE_PACKAGE_MANAGER
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

# Find the package manager.
DETECT_PACKAGE_MANAGER

# Load environment variables if we're linux.
LOAD_SYSTEND_DISTRIBUTION_INFORMATION

# Print the welcome message and the agreement of the terms of service.
PRINT_GREETINGS
PRINT_TERMS_OF_SERVICE

# Prompt the users for the two options.
PROMPT_INSTALL_LOCATION
PROMPT_READY_TO_INSTALL



# Detect packages that either the installer or daemon needs, and install them.
DETECT_PROGRAM_BREW
DETECT_PACKAGE_MANAGER
DETECT_PROGRAM_OPENVPN
DETECT_PROGRAM_PYTHON3

# Install .NET Core Runtime after dependencies are met.
WORK_INSTALL_DOTNET_CORE

WORK_INSTALL_SPECTERO

# Exit gracefully.
exit 0;
