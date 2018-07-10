# Spectero Unix Installer

This is a repository of a multiplatform installer written in `bash` and `python3	`.  
It is designed for easy deployment and automation.


## Compatability Matrices

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
|macOS High Sierra  |brew           |      |

### Virtualization
|Virtualization Type|Tested |
|-------------------|-------|
|OpenVZ             |✔️     |
|KVM                |✔️     |

<sub><sup>At this time we only currently support operating systems that have `systemd` and either the `yum`, `apt-get` or `brew` package managers.</sup></sub>

## Command Line Arguments
Please view the [Unix Command Line Arguments](https://github.com/ProjectSpectero/daemon-installers/wiki/Unix-Command-Line-Arguments) Wiki Page.


