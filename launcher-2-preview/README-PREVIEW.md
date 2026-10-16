# XHL Launcher 2.0 — read-only preview

A restrained charcoal, parchment and muted brass interface for monitoring an existing Enshrouded server. This preview is a separate application; keep using the existing launcher to start or stop the server.

## Run

On Windows x64 with .NET Framework 4.8, build with `powershell -NoProfile -File .\Build.ps1`, then open `build\XHL-Launcher-2.exe`. An optional first argument selects an existing server folder. Otherwise, the launcher checks the standard Steam dedicated-server folder. No game files or third-party art are bundled.

The application stores its own settings and telemetry in `XHL-Launcher-Data` beside its executable. Keep the preview in its own writable folder, outside the server and save directories.

## Implemented

- Overview with process state, players, CPU, memory, uptime and world metadata.
- Player connection table, world observations, bounded searchable logs and pause/resume.
- Separate stacking, gifting and Flight status cards without adjustment controls.
- Read-only executable compatibility, stacking attestation and loader-isolation checks.
- Process detection matches the selected executable path; another server installation is not reported as this one.
- World/player saves and server files are opened only for reading. This build contains no process start/stop, memory patch, save restore, deletion, mod installation or update actions.

## Verification and limits

The native Windows x64 build and fixture checks pass. See `VERIFICATION.md` for the exact scope. A matching attestation proves file correspondence only, not correct gameplay. Presence of a mod DLL or flag does not prove that a mod is active.

Flight-v2 balance changes are **not implemented**. The earlier test plan was unavailable, and the inspected executable differs from the hash accepted by the existing controller. Stamina, speed, ascent, landing and other hooks have not been verified for that executable. No guard was relaxed and no speculative hook was added.

This is a UI and read-only inspection preview, not a replacement for the functioning operational launcher. Runtime mod verification, multiplayer behavior, server control integration, high-DPI/multi-monitor testing and final UI acceptance remain outstanding.

## Rollback

Close the preview and use the existing launcher. There is nothing to restore in the server installation: this preview has not replaced or rearranged it. Keep any pre-existing backups and working launcher files. Never copy a save or game resource from this package into the server; none are supplied.

## Tests

Run `powershell -NoProfile -File .\Test.ps1`. Tests create isolated synthetic fixtures and UI renders under `test-results`. They never start the game server. Test runs use unique fixture folders and do not delete earlier fixtures.
