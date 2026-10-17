# XHL Enshrouded Launcher

An **unofficial Windows launcher and planned administration suite** for dedicated [Enshrouded](https://enshrouded.com/) servers.

> **Repository status (17 October 2026): documentation-only on `main`; active unmerged 2.0 source preview.** [Pull request #1](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/1) contains a newer `launcher-2-preview/` prototype, tests and verification notes on its own branch. It is **not yet merged or released**. Separately, recoverable v1.0.5 WinForms and early 2.0 PowerShell archives exist outside the repository. **No downloadable, verified release has been published.**

## Direction

XHL is intended to provide server lifecycle controls, SteamCMD updates, logs and player telemetry, safe save backups/restores, mod status, and a restrained dark-fantasy desktop experience. Some of these capabilities exist in a *recovered source snapshot*; the broader 2.0 administration suite is a plan, not a shipped feature.

## Current snapshot and source provenance

The recovered **v1.0.5 Complete Drop-In** archive contains 11 C# WinForms source files, a Windows .NET Framework compiler batch script, an application icon, image assets and release notes. The archive's own notes explicitly warn that it has **not** been compiled or integration-tested as a release. The separate **2.0 foundation** contains an experimental PowerShell entry point. **Neither recovered archive is present on `main`.** The [2.0 preview PR](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/1) contains *different, more recent* source and verification notes; it must be reviewed separately, not overwritten by an older source ZIP.

See [Recovery inventory](docs/RECOVERY-INVENTORY.md) for exact file inventory and archive hashes. Do **not** assume files present in a local running installation match either archive.

## Repo map

- [2.0 Preview PR #1](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/1) — newer source and tests, **awaiting review/operator checks**.
- [Roadmap](ROADMAP.md) — prioritized, honestly marked work.
- [Development and build notes](docs/DEVELOPMENT.md) — prerequisites, expected folder layout, and validation.
- [Server safety](docs/SERVER-SAFETY.md) — backup, rollback and live-server precautions.
- [Contributing](CONTRIBUTING.md) · [Security](SECURITY.md).

When the **legacy v1.0.5** source is imported, the original launcher folder layout will be preserved initially (`src/`, `assets/`, launch and build batch files, icon and manifest). Restructuring comes **after** a reproducible build and tests, not before.

## Build and run (after source import)

Target: Windows 10/11 with .NET Framework 4.8 and its C# compiler available at the traditional .NET Framework `csc.exe` path. The archived `BUILD-XHL-LAUNCHER.bat` compiles C# WinForms source to `%LOCALAPPDATA%\XHL Enshrouded Launcher\XHL-Launcher.exe`. The archived `RUN-XHL-LAUNCHER.bat` launches/rebuilds from the original source layout.

**These commands cannot run from the documentation-only `main` branch.** The 2.0 preview PR uses its own distinct build instructions and should be reviewed on its branch; do not mix the two build systems. Verify imported files and build on Windows before attempting any live-server action.

## Safe working rules

- Do not run experimental builds over an operating public server. Back up and test in a separate folder first.
- Never commit server credentials, configuration containing passwords, players' data, saves, logs, third-party DLLs or the dedicated-server executable.
- Distinguish *source recovered*, *compiles*, *tested against a disposable server*, and *production validated*. They are different states.
- No official affiliation with Keen Games, Valve or Steam.
