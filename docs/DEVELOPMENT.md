# Development and Windows build

## Baseline

The recovered **legacy v1.0.5** source is a traditional **C# WinForms/.NET Framework** project compiled by batch script and the .NET Framework `csc.exe`, rather than a modern .NET SDK project. **The newer 2.0 preview code is already in [PR #1](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/1)** under `launcher-2-preview/` and has its own `Build.ps1`, `Test.ps1`, `Test-Native.ps1`, and `VERIFICATION.md`. Review those artifacts and their remaining operator checks before a merge; do not assume they describe v1.0.5. Do not introduce a new toolchain or claim the version changed until a Windows build and smoke test succeed.

## Expected source layout after import

```text
/
  src/             # Eleven original C# source files
  assets/          # Three image assets
  XHL-Launcher.ico
  XHL-Launcher.exe.manifest
  VERSION.txt
  BUILD-XHL-LAUNCHER.bat
  RUN-XHL-LAUNCHER.bat
  RUN-XHL-LAUNCHER-DIAGNOSTIC.bat
  REBUILD-AND-RUN.bat
```

The original build script compiles into `%LOCALAPPDATA%\XHL Enshrouded Launcher\XHL-Launcher.exe` and writes `XHL-BUILD.log` to that directory. It requires Windows with the .NET Framework 4.8 compiler at `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe` (or the Framework 32-bit location).

## First acceptance pass

1. Verify all 11 source files and manifest/icon/assets exist; compile on a **Windows test machine**.
2. Start the app against a temporary/test server directory, not the real production folder.
3. Validate startup, settings persistence, stop/start/update error handling, logs and unknown/missing server paths.
4. Test backups/restores on disposable saves with rollback, corrupt archives and interrupted operations.
5. Check UI consistency/permissions and confirm no unexpected external network requests.

**Never equate "ZIP validates" with "the app compiles" or "the mod works."** Only report checks actually performed.

## Branch discipline

Use branches and PRs for source recovery and structural refactors; record changes in the PR. Keep recovered-v1.0.5 and 2.0 experiments distinguishable. Include checksums and reproducible steps for releases.
