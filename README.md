
# Spectero Installer
Copyright (c) 2018 Spectero - All Rights Reserved  
https://spectero.com/  

### Description:  
    This utility serves the purpose to install the daemon and command line tools into your operating system  
    It supports Mac OS X and Linux variants.  
    
### Warranty:  
    This software is provided without warranty.  
    
### Liability:  
    The user runs this application at their own risk, and takes responsibility for any system changes.  
    You will be promoted to read the terms service upon execution and installation.      
    
### Command Line Arguments:  
    Release Channels:  
        Release channels are categorized based on how stable the builds are.  
        
        --alpha  
            Use the alpha channel and get bleeding edge builds of the daemon.  
            It is ill-advised to use this channel as bugs may be present.  
        
        --beta  
            Use the beta channel and get semi-stable builds that contain new features.  
            This channel also may contain bugs.  
            
        --stable  
            Use the stable channel and get release candidates that are deemed suitable for production  
            (this flag is used by default)  
        
    Other:  
        --agree  
            Agree to the Spectero Terms of Service located at https://spectero.com/tos.  
            
        --install-dotnet  
            Automatically install the .NET Core Framework if it does not exist.  
            
        --no-prompt  
            Disable all Spectero related prompts and automatically install into /opt/spectero

        --overwrite
            By default spectero's installer
        
