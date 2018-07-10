import os
import sys

config = {}


def exception_config_missing():
    print("The installer requires a proper configuration file.")
    print("Please make sure you ran the install.sh")
    print("If this issue still exists, please report it to the developers.")
    sys.exit(1)


def read_config():

    if not os.path.isfile("/temp/spectero.installconfig"):
        exception_config_missing()

    with open("/tmp/spectero.installconfig", 'rU') as f:
        for line_terminated in f:
            line = line_terminated.rstrip('\n')
            splitline = line.split('=')
            key = splitline[0]
            if len(splitline) > 2:
                splitline = splitline.pop(0)
                value = splitline.join("=")

            elif len(splitline) == 2:
                value = splitline[1]

            # Update the dictionary
            print("Writing key '%s' with value '%s' into memory.", key, value)
            config[key] = value

    print("Installation config successfully loaded.")


read_config()
