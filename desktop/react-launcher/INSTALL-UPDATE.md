# Install and update ShroudForge

Includes launcher-only updates, close warnings, and server creation/discovery.
The game server does not need to restart for a launcher-only update.

## Closing and choosing servers

Closing the modern launcher while its selected server is running offers **Keep
server running**, **Stop server and exit**, or **Cancel**. Stopping uses the
existing graceful shutdown; failure leaves the launcher open. An unavailable
process observation warns and offers launcher-only exit or cancel. Monitoring,
scheduled backups and automatic recovery pause while the launcher is closed.
An installer update explicitly closes only the launcher, retaining the running
server, and still refuses closure during an active operation.

Use the server name in the sidebar, **Tools → Servers · create or scan**, or the
System page to open server setup. Scanning reads Steam libraries, the current
folder and folders you choose, up to four directory levels / 3,000 directories.
Errors and incomplete scans are shown. Files are never moved or started by a scan.
Selection saves a preference and reopens the launcher. The previous server keeps
running but is no longer monitored by this launcher until you switch back.

Creating requires a separate empty folder and an existing official `steamcmd.exe`.
It downloads dedicated-server app 2278520 with anonymous SteamCMD, requires a
confirmed successful download, and writes a fresh configuration with the chosen
name, administrator join password, UDP query port and slots. Use a distinct port
for each concurrent server; router/firewall setup remains manual. No game, world
or mod files are copied from the current server and nothing starts automatically.
Failed creation leaves partial files only in the new folder; choose another empty
folder after reviewing them. SteamCMD needs internet access and sufficient disk space.

New and explicitly adopted standard servers have separate world, backup/recovery
journals and telemetry history. Automation starts disabled on first selection;
returning to a prior server restores its backup/recovery preferences. Existing
managed/modded installations retain their verified wrapper startup path. Known
mod loaders block standard enrollment. Standard enrollment records fingerprints
of the three game files, **not a vendor signature or mod compatibility claim**.
Changed files block startup until independently reviewed and explicitly enrolled
again (preserve the old registration record outside the server folder first).
The classic tools remain available; create/scan and close choices are modern UI features.

The generated configuration follows [Keen's configuration reference](https://enshrouded.zendesk.com/hc/en-us/articles/16055441447709-Dedicated-Server-Configuration)
and [server roles](https://enshrouded.zendesk.com/hc/en-us/articles/19191581489309-Server-Roles-Configuration).
Fresh Windows dependency installation and relocating existing installations remain separate work.

## What stays where

| Folder | Purpose |
| :-- | :-- |
| Existing launcher folder | Keep its current path, even if its name contains XHL or 2.0. |
| Manager Data inside that folder | Private settings, recovery/backup schedules, offline player history. Preserve it. |
| Extracted update package | A separate temporary folder containing this guide and installer. |
| New rollback folder | A separate, unused path for the previous launcher and private manager data. |
| Existing game/server folder | Left alone by this updater; no patches, settings edits or save restoration. |

Launcher, package and rollback folders must be separate from each other and from
the game folder. Do not place the package or rollback inside the launcher folder.
The updater rejects overlapping locations and refuses to overwrite old rollback
folders. It requires the existing `Manager Data/settings.xml` to identify the
selected game folder.

## Prepare the update

1. Extract the complete Windows x64 package into a new folder. Keep both
   executables, WebView2 DLLs, UI, phone, assets and installer scripts together.
2. Check the package's `SHA256SUMS.txt` and `manifest.json`. The updater verifies
   manifest file hashes and sizes before requesting launcher closure.
3. Read `build-provenance.json` for the source commit and build identity. The
   informational version can remain private.2 across development builds; the
   commit identifies the exact source. Packages are unsigned private builds.
4. Finish any active launcher operation and close classic tools. Keep the game
   running. The updater will not force-close a busy launcher.

Windows, .NET Framework 4.8 and the Edge WebView2 Evergreen Runtime are required.
The package includes the pinned WebView2 SDK DLLs; Node is not required to run the
launcher. The updater does not replace system runtimes or game dependencies.

## Update the existing launcher

Open PowerShell in the **extracted package folder** and use your actual launcher
and a new rollback path. These example folders are placeholders:

```powershell
./Install-Desktop.ps1 `
  -Destination 'C:\Launchers\ShroudForge' `
  -Package (Get-Location).Path `
  -RollbackRoot 'C:\Launcher-Rollbacks\ShroudForge-update-001'
```

The installer requests normal closure of the exact selected launcher, including
when its main window is hidden. Existing busy-operation close guards still apply.
It never terminates a process, closes another installation, or restarts the game.
If the launcher cannot close, no files are replaced. Finish its operation and retry
with a new rollback path only if a previous attempt actually created that folder.

Previous package files and Manager Data are retained for rollback. The volatile
WebView2 browser cache is excluded from that copy. Private manager files remain in
place; they are never replaced from the package. A mid-update file failure restores
files already replaced. If restoration itself fails, the error explicitly reports
that rollback needs attention instead of claiming success.

The updater creates `Open ShroudForge.lnk` in the launcher folder. If
`Open XHL 2.0.lnk` already exists, it is retained and updated too. Both target the
same modern launcher. Existing folders, executable names and private data paths
are not migrated. By default, the interactive launcher reopens; `-NoLaunch` leaves
it closed for a controlled handover.

After reopening, confirm query readiness, a successful inventory scan, and existing
offline player history. Keep the rollback folder private. It can contain phone
keys and player metadata and must not be uploaded to GitHub.

## If a rollback is needed

Close the updated launcher normally and restore only the previous launcher files
and shortcuts retained in the rollback folder. Keep current Manager Data unless
its settings specifically need recovery; its retained copy is available for that
case. Do not automatically revert current schedules or newer player history.
Game saves, mods and server configuration are not part of a launcher rollback.

## Current completion scope

The owner accepts the current UI, backup/recovery and mod/settings behavior as-is;
this is owner acceptance, not a claim of new gameplay tests. Those areas are not
held open as installation gates. GitHub PRs/tracking stay open for running updates.
Phone expansion remains paused. This update work does not publish a public release.
