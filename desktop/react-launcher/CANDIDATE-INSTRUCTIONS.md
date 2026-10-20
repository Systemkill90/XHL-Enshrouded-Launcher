# ShroudForge 3.0.0-private.2 — acceptance candidate

Unsigned Windows x64 portable candidate. Stage separately; this package does not install itself or change the running launcher or game. The older 2.2 label was a planning milestone. Recovered source metadata identifies 3.0.0-private.1; this reconciled candidate is private.2.

Requires Windows, .NET Framework 4.8 and the Microsoft Edge WebView2 Evergreen Runtime already installed. WebView2 SDK 1.0.4258.31 managed references and the x64 loader are included. React and icons are bundled into ui/app.js; Node is not needed to run. Internet is needed for the external map provider. Do not replace dependencies in the working installation.

## Verify

Extract into a NEW folder outside every server, save, backup and installed-launcher folder. Compare every file with SHA256SUMS.txt and manifest.json before use. Keep both executables, three WebView2 DLLs, ui, assets and phone together. Checksums establish integrity, not signing or gameplay compatibility.

Do not launch this candidate against live settings for acceptance. The source contains a loopback synthetic UI preview (`node Preview-UI.mjs`). `--preview-from <fixture launcher folder>` supports an isolated read-only Windows check. Only supply disposable settings whose ServerRoot is a disposable fixture. No game binary is needed. In preview, phone listeners, automatic starts and backups are disabled and write commands are rejected. Diagnostics and browser cache stay in the candidate folder.

## Acceptance

Review Dashboard, Players, World, Mods, Resilience, Incidents, Console, Backups, Settings, Map, Phone and System. Test keyboard focus, Tools/Escape, Windows scaling at 100/125/150/200%, and native WebView2 startup. Phone map defaults to GameVault Compatibility with Protected and external fallback. Physical iPhone Safari/WebGL, real-game mod behavior, sleep settings, and an approved disposable-game backup/restore rehearsal remain acceptance gates. Never use the live server for these checks.

## Rollback

This staged candidate requires no rollback of the installed launcher: leave the working installation in place. Close only the candidate window to end an isolated preview; discard its separate folder later if unwanted. Preserve the existing launcher, settings/recovery journals and prior rollback archives.

Any later installation needs a separately reviewed migration and explicit authorization. First retain byte-for-byte copies and hashes of the previous launcher files and Manager Data, excluding the volatile WebView2 cache. Hand over automation safely before replacing launcher files. Restore retained launcher files/settings if acceptance fails; do not restore game saves, replace mods, or restart the server as part of launcher rollback. No deployment, public release or merge is authorized by this candidate.
