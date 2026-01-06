# AIXR Unity SDK v1.0.0 Release Notes

**Release Date**: January 6, 2026

## Overview

We are excited to announce the initial release of the AIXR Unity SDK! This SDK provides seamless integration between Unity and AIXR Agent (Pimax OpenXR Runtime), making it easier than ever to develop VR applications for AIXR-compatible devices.

## What's New

### 🎉 Initial Release Features

#### Automatic Setup & Configuration
- **One-Click OpenXR Setup**: The SDK automatically detects your environment and configures all necessary OpenXR packages and settings
- **Smart Runtime Detection**: Automatically detects AIXR Agent installation through Windows Registry
- **Auto-Start AIXR Agent**: Launches AIXR Agent automatically if it's installed but not running
- **Package Management**: Automatically installs and configures:
  - Unity XR Plugin Management
  - Unity OpenXR Plugin
  - Unity XR Interaction Toolkit

#### Developer Tools
- **Environment Checker**: Verify AIXR Agent installation and OpenXR runtime configuration
- **Manual Setup Trigger**: Re-run automatic setup when needed
- **Quick Settings Access**: Direct access to XR Plug-in Management settings
- **Reset Tool**: Reset auto-enabler state for troubleshooting

#### VR Controller Support
Pre-configured interaction profiles for major VR controllers:
- ✅ Oculus Touch Controllers
- ✅ HTC Vive Controllers
- ✅ Valve Index Controllers

#### Platform Support
- ✅ Windows Standalone (Primary Platform)
- ✅ Android (Mobile XR)

#### Optimized Settings
- Stereo Rendering Mode: Multi Pass (for best compatibility)
- Depth Submission Mode: 16-bit depth
- Automatic loader initialization

## Installation

### Unity Package Manager
```
https://github.com/dols1920/AIXR_UnitySDK.git
```

### Manual Installation
1. Download the latest release
2. Extract to your Unity project's Assets folder
3. Unity will automatically run the setup process

## Getting Started

1. **Install AIXR Agent** (if not already installed)
2. **Open your Unity project** with the SDK imported
3. **Wait for automatic setup** to complete
4. **Start developing** your VR application!

For detailed instructions, see the [README.md](README.md).

## System Requirements

- **Unity Version**: 2022.3 or later
- **Operating System**: Windows 10/11
- **Runtime**: AIXR Agent (Pimax OpenXR Runtime)
- **Build Targets**: Windows Standalone, Android

## Known Issues

- The auto-enabler runs once per project. Use the "Reset Auto Enabler" tool if you need to run it again
- Some Unity versions may require editor restart after package installation
- First-time setup may take a few minutes depending on internet speed

## Feedback & Support

We welcome your feedback! Please report issues or request features on our GitHub repository:
- **Issues**: https://github.com/dols1920/AIXR_UnitySDK/issues
- **Discussions**: https://github.com/dols1920/AIXR_UnitySDK/discussions

## What's Next

We're committed to improving the AIXR Unity SDK. Future releases will include:
- Additional interaction profiles
- Enhanced debugging tools
- Performance profiling utilities
- Example scenes and sample projects
- Extended platform support

## Credits

Special thanks to the Unity XR team and the OpenXR community for their excellent documentation and tools.

---

**Installation**: See [Installation](#installation) section above or visit the [GitHub repository](https://github.com/dols1920/AIXR_UnitySDK)

**Full Changelog**: [CHANGELOG.md](CHANGELOG.md)
