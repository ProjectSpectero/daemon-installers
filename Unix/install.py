import os
import json
import sys
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
    # Check if install instructions exist
    if not os.path.isfile("/temp/spectero.installconfig"):
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
            print("Writing key '%s' with value '%s' into memory.", key, value)
            config[key] = value

    print("Installation config successfully loaded.")


def get_download_channel_information():
    request = urllib.request.Request('https://c.spectero.com/releases.json')
    result = urllib.request.urlopen(request)
    releases = json.loads(result.read())


def validate_user_requests_against_releases():
    # Check to make sure we are a valid branch.
    if config["branch"] not in releases["channels"]:
        exception_channel_not_found()

    # Convert latest to the latest branch string
    if config["version"] == "latest":
        config["version"] = releases["channels"][config["branch"]]

    # Check if we can download this version.
    if config["version"] not in releases["versions"]:
        exception_version_not_available_for_download()

    if releases["versions"][config["version"]] is None:
        exception_no_download_link()


read_config()
get_download_channel_information()
validate_user_requests_against_releases()
