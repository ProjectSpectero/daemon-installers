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
import platform
import subprocess
import sys
import traceback
import urllib.request

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


def exception_unsupported_os():
    print("The python installer script is currently incompatible with this OS.")
    print("Please file an issue with the following information:")
    print("=" * 40)
    print("sys.platform\t\t=\t%s" % sys.platform)
    print("platform.architecture\t\t=>\t%s" % platform.architecture())
    print("os.uname\t\t=>\t%s" % os.uname())
    sys.exit(1)


def exception_sysctl_add_fail():
    print("There was an error adding a value to sysctl.conf")
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


def sysctl_tweaks():
    tweaks = {
        "net.ipv4.tcp_fin_timeout": 10,
        "net.ipv4.tcp_tw_reuse": 1,
        "fs.file-max": 2097152,
        "net.ipv4.ip_forward": 1
    }

    for tweak in tweaks:
        try:
            sysctl_safe_add(tweak, tweaks[tweak])
            print("sysctl value '%s' was added successfully." % tweak)
        except:
            exception_sysctl_add_fail()


def sysctl_safe_add(property, value):
    # Try to execute
    result = (subprocess.check_output(["sysctl", property])[:-1]).decode("utf-8")

    # Check if it is disabled, if so add.
    if result == "%s = %s" % (property, value):
        print("Appending '%s' to /etc/sysctl.conf with value '%s'..." % (property, value))
        os.system("""echo "%s = %s" >> /etc/sysctl.conf""" % (property, value))


def sysctl_reload():
    print("Reloading sysctl configuration...")
    os.system("sysctl --system > /dev/null 2>&1")  # Needs more aggressive suppression.


def set_ulimit_spectero_user():
    filepath = "/etc/security/limits.conf"

    # String replacement.
    with open(filepath, 'r') as file:
        filedata = file.read()

    # Strings
    soft_limit = "spectero soft nofile 500000"
    hard_limit = "spectero hard nofile 500000"

    # Append Spectero specific limit.
    if soft_limit not in filedata:
        print("Setting soft file descriptor ulimit for specctero user...")
        filedata += ("\n" + soft_limit)
    if hard_limit not in filedata:
        print("Setting hard file descriptor ulimit for specctero user...")
        filedata += ("\n" + hard_limit)

    with open(filepath, 'w') as file:
        file.write(filedata)


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
    # use the machine unique string to find the download needed for dotnet core.
    get_repository_dotnet_version()
    return sources["dependencies"]["dotnet"][get_repository_dotnet_version()]["Linux"][os.uname().machine]


def determine_valid_architecture():
    if os.uname().machine not in ["armv7l", "x86_64"]:
        exception_unsupported_os()


if __name__ == "__main__":
    determine_valid_architecture()
    read_config()
    get_download_channel_information()
    get_sources_information()
    validate_user_requests_against_releases()
    download_and_extract()

    # If the script is attempted to run in reverse compatibility, linux2 may be a valid value.
    if sys.platform in ["linux"]:
        create_user()
        set_ulimit_spectero_user()

    else:
        exception_unsupported_os()

    fix_permissions()

    if sys.platform in ["linux"]:
        update_sudoers()

    create_latest_symlink()

    if sys.platform in ["linux"]:
        sysctl_tweaks()
        sysctl_reload()
        create_systemd_service()

    create_shell_script()
