# Recovered-source inventory

**Documented:** 2026-10-17. These archives were found in the project's earlier ChatGPT file library, **not** in this GitHub repository.

| Archive | Size | SHA-256 | Contents |
| --- | ---: | --- | --- |
| XHL-Enshrouded-Launcher-v1.0.5-COMPLETE-DROP-IN(1).zip | 138,893 bytes | `b41dbda230975620ce69f2f36e1e309bff9e808eadf1198e346d54eff9e5a22b` | 29 entries; 11 C# files, build/start scripts, icon and artwork |
| XHL-Launcher-2.0-foundation.zip | 5,805 bytes | `e62d204e46ba5fa286bcf6c24eb3714913eff61ef1fe5db7ecbc76718899a6a1` | 4 entries: README, BAT launcher, roadmap, PowerShell foundation |

Both ZIP central directories passed Python `ZipFile.testzip()`, which **does not** prove source quality, binary safety, build success or integration compatibility.

## Original v1.0.5 source files

`src/Program.cs`, `Theme.cs`, `Models.cs`, `Storage.cs`, `Telemetry.cs`, `ServerControl.cs`, `BackupManager.cs`, `Mods.cs`, `ConfigEditorForm.cs`, `UIControls.cs`, `MainForm.cs`.

The archived build script expects those sources and these adjacent files: `XHL-Launcher.ico`, `XHL-Launcher.exe.manifest`, `VERSION.txt`, and `assets/hero_scene.jpg`, `assets/sidebar_scene.jpg`, `assets/status_scene.jpg`. It references WinForms, System.Management, System.Web.Extensions and Compression assemblies through the .NET Framework compiler.

The archive's `README-FIRST.txt` warns that its **v1.0.5 source-only package was not compiled or tested on a live Windows Enshrouded server**. The separate PowerShell 2.0 foundation must not silently replace this WinForms app.

## Import checklist

1. Verify the archive SHA-256; inspect extracted files for secrets, licensing and malware.
2. Import original plain-text source and permitted assets on a review branch. Exclude server EXEs, saves, player data, private configuration, runtime logs and unlicensed third-party DLLs.
3. Compile on Windows, compare behavior against the actual running installation, and record test results.
4. Only then prepare a tagged binary release. Do not publish either source ZIP as a validated release.

This document records provenance, not that the source has been uploaded or a successful release exists.
