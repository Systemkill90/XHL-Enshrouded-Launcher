# ShroudForge 3.0.0-private.2 — acceptance candidate

Unsigned Windows x64 portable candidate. Stage separately; this package does not install itself or change the running launcher or game. The older 2.2 label was a planning milestone. Recovered source metadata identifies 3.0.0-private.1; this reconciled candidate is private.2.

Requires Windows, .NET Framework 4.8 and the Microsoft Edge WebView2 Evergreen Runtime already installed. WebView2 SDK 1.0.4258.31 managed references and the x64 loader are included. React and icons are bundled into ui/app.js; Node is not needed to run. Internet is needed for the external map provider. Do not replace dependencies in the working installation.

## Verify

Extract into a NEW folder outside every server, save, backup and installed-launcher folder. Compare every file with SHA256SUMS.txt and manifest.json before use. Keep both executables, three WebView2 DLLs, ui, assets and phone together. Checksums establish integrity, not signing or gameplay compatibility.

Do not launch this candidate against live settings for acceptance. The source contains a loopback synthetic UI preview (`node Preview-UI.mjs`). `--preview-from <fixture launcher folder>` supports an isolated read-only Windows check. Only supply disposable settings whose ServerRoot is a disposable fixture. No game binary is needed. In preview, phone listeners, automatic starts and backups are disabled and write commands are rejected. Diagnostics and browser cache stay in the candidate folder.

## Acceptance

On October 20 the owner accepted the existing Windows UI, backup/recovery and mod/settings behavior as-is. These are not held open as completion gates; owner acceptance does not claim new independent gameplay or device tests. Current work is installation/update polish; see INSTALL-UPDATE.md. GitHub work stays open with running updates. Phone expansion stays paused, and the known iPhone GameVault crypto.subtle.digest defect remains unresolved.

## Rollback

This staged candidate requires no rollback of the installed launcher: leave the working installation in place. Close only the candidate window to end an isolated preview; discard its separate folder later if unwanted. Preserve the existing launcher, settings/recovery journals and prior rollback archives.

Use the included launcher-only updater for an authorized update of an existing installation. It retains previous launcher files and Manager Data, excluding the volatile WebView2 cache, and requests normal launcher closure before replacing files. Restore retained launcher files/settings if needed; do not restore game saves, replace mods, or restart the server as part of launcher rollback. Existing folders and private data paths remain unchanged. The package itself grants no deployment permission; the owner's current task authorization governs updates. No public release, merge or closing of ongoing GitHub work is implied.
