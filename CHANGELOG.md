# Changelog

All notable changes to the AIXR Unity SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-01-06

### Added
- Initial release of AIXR Unity SDK
- Automatic OpenXR environment detection and configuration
- AIXR Agent (Pimax OpenXR Runtime) integration
- Automatic installation of required Unity packages:
  - XR Plugin Management (com.unity.xr.management)
  - OpenXR (com.unity.xr.openxr)
  - XR Interaction Toolkit (com.unity.xr.interaction.toolkit)
- Automatic OpenXR configuration:
  - Stereo Rendering Mode set to Multi Pass
  - Depth Submission Mode set to Depth 16-bit
  - Automatic loading and running enabled
- Interaction profile support:
  - Oculus Touch Controllers
  - HTC Vive Controllers
  - Valve Index Controllers
- AIXR Agent process detection and auto-start functionality
- Unity Editor menu tools:
  - Check OpenXR Environment
  - Manual Trigger OpenXR Install
  - Reset Auto Enabler
  - Open XR Settings
- Windows Registry-based OpenXR runtime detection
- Support for both Windows Standalone and Android platforms
- Comprehensive error handling and user feedback dialogs
- Detailed console logging for troubleshooting

### Features
- **Smart Detection**: Automatically detects AIXR Agent installation via Windows Registry
- **One-Click Setup**: Complete OpenXR configuration with minimal user interaction
- **Runtime Validation**: Verifies OpenXR runtime is properly configured
- **Process Management**: Automatically launches AIXR Agent if installed but not running
- **Multi-Platform**: Configures settings for both Standalone and Android build targets
- **User-Friendly**: Clear dialog messages and progress indicators
- **Extensible**: Modular architecture for future enhancements

### Technical Details
- Built using Unity Editor scripting API
- Utilizes reflection for dynamic package management
- Windows Registry integration for runtime detection
- EditorPrefs-based state management
- Asynchronous package installation with progress tracking

[1.0.0]: https://github.com/dols1920/AIXR_UnitySDK/releases/tag/v1.0.0
