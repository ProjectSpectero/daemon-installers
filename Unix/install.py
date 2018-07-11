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
import shutil
import subprocess
import sys
import traceback
import urllib.request

config = {}
releases = {}


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
            print("Writing key '%s' with value '%s' into memory." % (key, value))
            config[key] = value

    print("Installation config successfully loaded.")


def get_install_directory_from_config():
    new = config["directory"]
    if not new.endswith('/'):
        new = new + "/"

    return new


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

    # Get the URL for the version.
    url = releases["versions"][config["version"]]["download"]

    # Build a easy to access path.
    path = ("%s%s.zip" % (get_install_directory_from_config(), config["version"]))

    # Download
    print("Invoking wget to download files...")
    os.system("wget %s -O %s" % (url, path))

    # Extract
    print("Invoking tar to extract files...")
    os.system("unzip %s -d %s" % (path, get_install_directory_from_config()))

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


def get_dotnet_core_path():
    return which("dotnet")


def create_systemd_service():
    if config["service"] == "true":
        try:
            systemd_script_location = "/etc/systemd/system/spectero.service"
            systemd_script_template = "%s%s/daemon/Tooling/Linux/spectero.service" % (get_install_directory_from_config(), config["version"])

            # Open a reader to the template
            with open(systemd_script_template, 'r') as file:
                filedata = file.read()

            # Replace template data here
            filedata = filedata.replace("ExecStart=/usr/bin/dotnet", "ExecStart=" + which("dotnet"))

            # Open a writer to the template location.
            with open(systemd_script_template, 'w') as file:
                file.write(filedata)

            # Copy to system directory
            shutil.copyfile(systemd_script_template, systemd_script_location)

            # Reload the systemd daemon
            os.system("systemctl daemon-reload")

            # Enable the process
            os.system("systemctl enable spectero")

            # Attempt to start the process
            print("Using systemctl to start spectero service.")
            os.system("systemctl start spectero")

            # Print the status
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
    # Define the propety of  what we need toc heck
    property = "net.ipv4.ip_forward"
    try:
        # Try to execute
        result = (subprocess.check_output(["sysctl", property])[:-1]).decode("utf-8")

        # Check if it is disabled
        if result == "%s = 0" % property:
            # Enable ip forwarding
            print("Enabling IPv4 Forwarding")
            os.system("""echo "%s = 1" >> /etc/sysctl.conf""" % property)
            print("Reloading System Configuration Kernel Properties...")
            os.system("sysctl --system")
    except:
        print("There was a problem attempting to check for kernel flag: ipv4_forward.")
        sys.exit(1)


def create_shell_script():
    try:
        cli_script = get_install_directory_from_config() + config["version"] + "/cli/Tooling/spectero"

        # String replacement.
        with open(cli_script, 'r') as file:
            filedata = file.read()
        print("Replacing variables in console management interface template...")
        filedata = filedata.replace("{dotnet path}", which("dotnet"))
        filedata = filedata.replace("{spectero working directory}", get_install_directory_from_config())
        filedata = filedata.replace("{version}", config["version"])

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


if __name__ == "__main__":
    read_config()
    get_download_channel_information()
    validate_user_requests_against_releases()
    download_and_extract()
    create_user()
    update_sudoers()
    create_latest_symlink()
    linux_enable_ipv4_forwarding()
    create_systemd_service()
    create_shell_script()
