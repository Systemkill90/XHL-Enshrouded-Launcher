# ShroudForge Launcher

Windows launcher and control room for Enshrouded dedicated servers, formerly XHL Enshrouded Launcher.

The current source lives in [launcher-2-preview](launcher-2-preview). The existing folder and repository names are retained for compatibility.

## Current features

- Dense dashboard with process CPU/memory trends, player count, uptime, world save records and backup coverage.
- Grouped server settings with friendly names, toggles, dropdowns, numeric controls and search.
- Saved world and player history available while the server is offline.
- Verified backups, guarded offline restore, pinned-build mod checks and the established startup path.
- Splash screen, app icon, tray access and authenticated private-LAN phone controls.

## Build and validation

On 64-bit Windows with the .NET Framework compiler:

```powershell
./launcher-2-preview/Build.ps1
./launcher-2-preview/Test.ps1
./launcher-2-preview/Test-Native.ps1
```

The build produces `ShroudForge-Launcher.exe`. Latest local validation: 146 fixture/UI checks passed; seven native lifecycle checks passed. See [overhaul notes](launcher-2-preview/OVERHAUL.md) for scope and remaining validation.

This is source and a test build, not a signed public installer. CI excludes game binaries, proprietary resources, personal settings, saves and credentials.

## Documentation

- [Overhaul notes](launcher-2-preview/OVERHAUL.md)
- [Preview technical notes](launcher-2-preview/README-PREVIEW.md)
- [Contributing](CONTRIBUTING.md)
- [Security](SECURITY.md)
- [Roadmap](ROADMAP.md)

Unofficial fan project. Not affiliated with Keen Games, Steam or Valve.

