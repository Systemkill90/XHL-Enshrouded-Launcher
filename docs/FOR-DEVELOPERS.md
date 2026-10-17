# For contributors and advanced users

Everything needed to **understand the project as a user** should be on the [main page](../README.md), in [Features](FEATURES.md) and in [Getting started](GETTING-STARTED.md).

This page is for source-code review, rebuilds, compatibility work and release preparation.

## Where the code currently lives

- **Current 2.0 preview source:** [Draft pull request #1](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/1), directory `launcher-2-preview/` on its review branch. The source has its own `Build.ps1`, `Test.ps1`, `Test-Native.ps1` and `VERIFICATION.md`.
- **Main branch:** public project information, documentation and repository checks. It is not a published binary distribution.
- **Earlier versions:** recovered v1.0.5 C# WinForms and early 2.0 PowerShell archives, described in [Recovery inventory](RECOVERY-INVENTORY.md). Don't accidentally substitute them for the newer 2.0 code.

## Read before changing runtime behavior

- [Development and build instructions](DEVELOPMENT.md)
- [Server and save safety](SERVER-SAFETY.md)
- [Security reporting](../SECURITY.md)
- [Detailed roadmap](../ROADMAP.md)
- [Operator release checklist](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/issues/3)

## Naming and documentation conventions

**Public-facing language:** use **Enshrouded Server Companion**, and describe it as a Windows dedicated-server manager for Enshrouded. Explain a feature in plain English before describing its implementation.

**Internal compatibility:** the existing build-script paths, names of source classes, namespaces, and working project files are allowed to keep their original identifiers until a tested renaming/migration is ready. A cosmetic rename must never break the build, backups, or the running server.

**Status language:** distinguish *reported working on the owner's host*, *passing isolated fixture checks*, *verified in actual gameplay*, and *publicly released*. Do not call a development branch an installer.

## Development workflow

Make changes on a branch, test with a separate non-production server directory, avoid committing binaries or saves, then open a pull request describing the checks you actually ran. Never publish game resources or private server data.
