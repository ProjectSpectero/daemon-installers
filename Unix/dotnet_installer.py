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
import sys
import os
import urllib.request

config = {}
sources = {}


def exception_config_missing():
    print("The installer requires a proper configuration file.")
    print("Please make sure you ran the install.sh")
    print("If this issue still exists, please report it to the developers.")
    sys.exit(1)


def download_sources():
    global sources
    request = urllib.request.Request('https://raw.githubusercontent.com/ProjectSpectero/daemon-installers/master/SOURCES.json')
    result = urllib.request.urlopen(request)
    sources = json.loads(result.read().decode('utf-8'))


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


if __name__ == "__main__":
    read_config()
    download_sources()
