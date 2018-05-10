# Spectero Unix Installer

This is a repository of a multiplatform installer written in `bash` and `python3	`.  
It is designed for easy deployment and automation.


## Compatability Matrix

|Operating System  |Package Manager |Tested |
|-------------------|---------------|-------|
|Debian 8           |apt-get        |✔️    |
|Debian 9           |apt-get        |✔️    |
|Ubuntu 16.04 LTS   |apt-get        |✔️    |
|Ubuntu 18.04 LTS   |apt-get        |✔️    |  
|CentOS 7           |yum            |✔️    |
|Fedora 26          |yum            |      |
|Fedora 27          |yum            |      |
|Fedora 28          |yum            |✔️    |

<sub><sup>At this time we only currently support operating systems that have `systemd` and either the `yum` or `apt-get` package managers.</sup></sub>


## Quick Install
Copy this command into your terminal to install spectero:
```sh
$ sudo bash <(curl -s https://spectero.com/installer)
```

## Command Line Arguments
Please view the [Command Line Arguments](https://github.com/ProjectSpectero/daemon-installer-nix/wiki/Command-Line-Arguments) Wiki Page.

## License
```
Spectero, Inc
Copyright (c) 2018 - All Rights Reserved
https://spectero.com

INTRODUCTION:
This license serves to establish what the end user cannot 
and can do with the scripts in this repository.

USE AND DISTRIBUTION:
The installation scripts provided with this repostory
are able to be freely distributed among private entities 
and personal use to allow easy installation and distribution 
of the software provided by Spectero.

MODIFICATION:
The end user is free to modify the `install.sh` to fit their needs 
for deployment purposes only.

WARRANTY:
This software and dependencies are installed under the discression
of the end user and is provided without warranty at the end user's
own risk.
```
