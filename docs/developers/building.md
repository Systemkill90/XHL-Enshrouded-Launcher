# Building ShroudForge Launcher 2.0

The current development source uses **C# WinForms on Windows**, with Windows PowerShell and the .NET Framework compiler.

From the [2.0 source branch](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/tree/feat/launcher-2-overhaul/launcher-2-preview):

```powershell
./launcher-2-preview/Build.ps1
./launcher-2-preview/Test.ps1
./launcher-2-preview/Test-Native.ps1
```

You need a Windows installation with the .NET Framework tooling referenced by the build scripts. The resulting `ShroudForge-Launcher.exe` is **not** an approved public installer.

These commands compile and run source/fixture validation. They do **not** establish gameplay compatibility, correctly saved player inventory, client HUD interoperability or the safety of updating the public server.

## Before touching live files

- Review [public-server safety](../operate/README.md).
- Record exact supported game version, loader dependencies and file hashes.
- Keep all experiments isolated in branches until a change-specific owner-approved maintenance session.
- **Never put proprietary game files, credentials, unsanitized player logs or world saves in GitHub.**

[Code branch](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/tree/feat/launcher-2-overhaul/launcher-2-preview) · [Main developer page](README.md)
