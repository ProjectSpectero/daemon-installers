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
import sys
import urllib.request

releases = {}
config = {}
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


def get_sources_information():
    global sources
    request = urllib.request.Request('https://raw.githubusercontent.com/ProjectSpectero/daemon-installers/master/SOURCES.json')
    result = urllib.request.urlopen(request)
    sources = json.loads(result.read().decode('utf-8'))


def get_install_directory_from_config():
    global config
    new = config["directory"]
    if not new.endswith('/'):
        new = new + "/"
    return new


def get_dotnet_destination():
    return get_install_directory_from_config() + "/dotnet"


def get_download_channel_information():
    global releases
    request = urllib.request.Request('https://c.spectero.com/releases.json')
    result = urllib.request.urlopen(request)
    releases = json.loads(result.read().decode('utf-8'))


def get_dotnet_runtime_link():
    global releases
    return releases["linux"]["dotnet"]["x64"]


def resolve_version():
    global releases, config

    if config["branch"] not in releases["channels"]:
        exception_channel_not_found()

    # Convert latest to the latest branch string
    if config["version"] == "latest":
        config["version"] = releases["channels"][config["branch"]]


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


def install_dotnet():
    tmp_path = "/tmp/spectero-dotnet-install.tar.gz"
    link = get_dotnet_runtime_link

    # download, create the directory, extract.
    os.system("wget %s -O %s -q" % (link, tmp_path))
    os.system("mkdir -p %s" % get_dotnet_destination())
    os.system("tar -xf %s -C %s" % (tmp_path, get_dotnet_destination()))


if __name__ == "__main__":
    read_config()
    get_download_channel_information()
    get_sources_information()
    resolve_version()
    install_dotnet()
