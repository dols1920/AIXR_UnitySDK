# AIXR Unity SDK

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![Unity](https://img.shields.io/badge/Unity-2022.3%2B-green)
![OpenXR](https://img.shields.io/badge/OpenXR-1.0-orange)

AIXR Unity SDK is a comprehensive Unity package that provides seamless integration with the AIXR Agent (Pimax OpenXR Runtime). This SDK automatically configures your Unity project for OpenXR development and provides tools to streamline VR application development.

## Features

- **Automatic OpenXR Setup**: Automatically detects and configures OpenXR packages and settings
- **AIXR Agent Integration**: Seamless integration with AIXR Agent (Pimax OpenXR Runtime)
- **Runtime Detection**: Automatically detects installed OpenXR runtimes
- **Auto-Configuration**: Configures XR Plugin Management, OpenXR, and XR Interaction Toolkit
- **Multi-Platform Support**: Supports Windows Standalone and Android platforms
- **Interaction Profiles**: Pre-configured interaction profiles for major VR controllers
  - Oculus Touch Controllers
  - HTC Vive Controllers
  - Valve Index Controllers

## Requirements

- Unity 2022.3 or later
- Windows 10/11 (for development)
- AIXR Agent (Pimax OpenXR Runtime) installed on your system
- Basic understanding of Unity and VR development

## Installation

### Option 1: Unity Package Manager (Recommended)

1. Open your Unity project
2. Go to `Window > Package Manager`
3. Click the `+` button and select `Add package from git URL`
4. Enter: `https://github.com/dols1920/AIXR_UnitySDK.git`
5. Click `Add`

### Option 2: Manual Installation

1. Clone or download this repository
2. Copy the entire `AIXR_UnitySDK` folder into your Unity project's `Assets` folder
3. Unity will automatically detect and import the SDK

## Getting Started

### Automatic Setup

When you first open a project with the AIXR Unity SDK:

1. The SDK will automatically check for AIXR Agent installation
2. If AIXR Agent is detected, it will:
   - Install required Unity packages (XR Management, OpenXR, XR Interaction Toolkit)
   - Configure OpenXR settings for your project
   - Enable appropriate interaction profiles
   - Start AIXR Agent if it's not already running

3. A dialog will appear when setup is complete

### Manual Tools

The SDK provides several tools accessible from the Unity menu bar:

#### Tools > OpenXR Menu

- **Check OpenXR Environment**: Verify AIXR Agent installation and OpenXR runtime status
- **Manual Trigger OpenXR Install**: Re-run the automatic setup process
- **Reset Auto Enabler**: Reset the auto-enabler (requires editor restart)
- **Open XR Settings**: Quick access to XR Plug-in Management settings

## Configuration

### OpenXR Settings

The SDK automatically configures the following OpenXR settings:

- **Stereo Rendering Mode**: Multi Pass
- **Depth Submission Mode**: Depth 16-bit
- **Automatic Loading**: Enabled
- **Automatic Running**: Enabled

### Interaction Profiles

The following interaction profiles are automatically enabled:

- Oculus Touch Controller Profile
- HTC Vive Controller Profile
- Valve Index Controller Profile

### Customizing Settings

To customize OpenXR settings:

1. Go to `Edit > Project Settings > XR Plug-in Management`
2. Select your target platform (Standalone or Android)
3. Configure OpenXR settings as needed
4. Navigate to `OpenXR > Interaction Profiles` to manage controller profiles

## Platform Support

### Windows Standalone

- Fully supported
- Automatic configuration for Windows builds
- AIXR Agent detection and auto-start

### Android

- Supported for Android XR devices
- Configure Android-specific settings in XR Plug-in Management
- Requires AIXR Agent for development/testing

## Troubleshooting

### AIXR Agent Not Detected

**Problem**: SDK reports AIXR Agent is not installed

**Solutions**:
1. Verify AIXR Agent is installed on your system
2. Check Windows Registry at `HKEY_LOCAL_MACHINE\SOFTWARE\Khronos\OpenXR\1`
3. Ensure `pimax-openxr.json` is set as the active runtime
4. Run `Tools > OpenXR > Check OpenXR Environment` to see detailed status

### Packages Not Installing

**Problem**: Unity packages fail to install

**Solutions**:
1. Check your internet connection
2. Verify Unity Package Manager is not blocked by firewall
3. Try manual installation: `Tools > OpenXR > Manual Trigger OpenXR Install`
4. Check Unity Console for specific error messages

### OpenXR Configuration Issues

**Problem**: OpenXR not properly configured after installation

**Solutions**:
1. Use `Tools > OpenXR > Reset Auto Enabler` and restart Unity
2. Manually verify settings in `Edit > Project Settings > XR Plug-in Management`
3. Ensure OpenXR loader is enabled for your target platform
4. Check Console logs for configuration errors

### AIXR Agent Not Starting

**Problem**: AIXR Agent doesn't start automatically

**Solutions**:
1. Manually launch AIXR Agent from its installation directory
2. Verify AIXR Agent installation path in Console logs
3. Check Windows Task Manager for running AIXR Agent processes
4. Reinstall AIXR Agent if necessary

## Development Workflow

### Creating a VR Scene

1. Create a new scene in Unity
2. Delete the default Main Camera
3. Add XR Origin (from XR Interaction Toolkit)
4. Add XR Interaction Manager
5. Configure your VR interactions and UI
6. Build and run your application

### Testing Your Application

1. Ensure AIXR Agent is running
2. Connect your VR headset
3. Click Play in Unity Editor or build your application
4. The application should automatically connect to AIXR Agent

## SDK Architecture

### Core Components

#### OpenXRAutoEnabler

The main component that handles automatic setup and configuration:
- Detects OpenXR runtime environment
- Manages package installation
- Configures XR settings
- Handles AIXR Agent integration

### Dependencies

The SDK manages the following Unity packages:

- `com.unity.xr.management` (4.5.4+)
- `com.unity.xr.openxr` (1.16.1+)
- `com.unity.xr.interaction.toolkit` (3.3.1+)

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## Support

For support and questions:
- Check the [Troubleshooting](#troubleshooting) section
- Review Unity Console logs for detailed error messages
- Submit issues on the GitHub repository

## License

Please refer to the LICENSE file in this repository.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and updates.

## Acknowledgments

- Built for AIXR Agent (Pimax OpenXR Runtime)
- Uses Unity's XR Plugin Management system
- Integrates OpenXR and XR Interaction Toolkit
