# XHL Launcher 2.0 — guarded controls preview

A restrained charcoal, parchment and muted brass interface for an existing Enshrouded server. This preview is a separate application. Monitoring is the default; optional guarded controls are available without replacing the existing launcher.

## Run

On Windows x64 with .NET Framework 4.8, build with `powershell -NoProfile -File .\Build.ps1`, then open `build\XHL-Launcher-2.exe`. An optional first argument selects an existing server folder. Otherwise, the launcher checks the standard Steam dedicated-server folder. No game files or third-party art are bundled.

To enable the optional Start, Stop and Restart interface, launch `build\XHL-Launcher-2.exe --controls`. Actions require confirmation inside the app. Start and Restart require matching compatibility checks and delegate to the existing `XHL-Diagnostic-Wrapper.ps1`; there is no fallback to a direct game executable. Restart checks compatibility before stopping and checks again before starting. Stop sends Ctrl+C to the matching installation and never force-kills it. The current inspected installation fails compatibility checks, so Start and Restart remain disabled.

The application stores its own settings and telemetry in `XHL-Launcher-Data` beside its executable. Keep the preview in its own writable folder, outside the server and save directories.

## Implemented

- Overview with process state, players, CPU, memory, uptime and world metadata.
- Player connection table, world observations, bounded searchable logs and pause/resume.
- Separate stacking, gifting and Flight status cards without adjustment controls.
- Read-only executable compatibility, stacking attestation and loader-isolation checks.
- Process detection matches the selected executable path; another server installation is not reported as this one.
- Monitoring opens world/player saves and server files only for reading. No save restore, deletion, mod installation or update actions are exposed.
- Optional lifecycle actions invoke the existing wrapper or send a graceful console signal. Normal game saving and the existing wrapper's snapshot/mod behavior occur only when the operator explicitly requests a permitted operation. There is no automatic start or restart.

## Verification and limits

The native Windows x64 build, 27 fixture/UI checks and seven isolated native-runtime checks pass. See `VERIFICATION.md` for the exact scope. A matching attestation proves file correspondence only, not correct gameplay. Presence of a mod DLL or flag does not prove that a mod is active.

Flight-v2 balance changes are **not implemented**. The earlier test plan was unavailable, and the inspected executable differs from the hash accepted by the existing controller. Stamina, speed, ascent, landing and other hooks have not been verified for that executable. No guard was relaxed and no speculative hook was added.

This remains a preview, not a verified production replacement. Runtime mod verification, multiplayer behavior, actual-game lifecycle validation and high-DPI/multi-monitor testing remain outstanding. The UI direction was approved; Flight-v2 requirements are pending from the operator.

## Rollback

Close the preview and use the existing launcher. There is nothing to restore in the server installation: this preview has not replaced or rearranged it. Keep any pre-existing backups and working launcher files. Never copy a save or game resource from this package into the server; none are supplied.

## Tests

Run `powershell -NoProfile -File .\Test.ps1` and `powershell -NoProfile -File .\Test-Native.ps1`. Tests create isolated synthetic fixtures and UI renders. The native test compiles a tiny dummy console executable named `enshrouded_server.exe` under a unique test folder; this is not the game. It also runs a harmless fixture startup script. The dummy self-exits after 20 seconds if signaling fails. Tests never launch the production game or delete earlier fixtures.
