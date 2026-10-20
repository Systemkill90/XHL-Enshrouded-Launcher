# ShroudForge launcher — staged UI update

**Prepared:** 2026-10-19  
**Status:** development candidate; **NOT deployed**  
**Source branch:** `feat/ui-navigation-polish-20261019`  
**Review:** [UI consolidation PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31) (draft; base: `feat/launcher-2-overhaul`)

## Version identity

The owner recalls that desktop development reached some form of **2.2** before a Work session ran out of credits. Current checked-in launcher application and documentation still identify the developer preview as **2.0**. Until the interrupted local desktop code and a version manifest can be compared, this stage is a **2.2 UI-candidate scope**, **not** a verified release, installed build number or automatic version upgrade. Do not relabel an installed working version based only on this document.

## Included in this stage

- Complete the previously unfinished launcher UI work using existing project components rather than a fresh rewrite.
- Distinctive dark-fantasy theme, splash and dashboard framework from existing `feat/launcher-2-overhaul`.
- Navigation refinements: persistent active-page indicator, independent scrollable sidebar, keyboard tab stops and accessible names.
- Read-only dashboard refinement: process trends, server readiness, verified backup-marker summary, recent recorded activity and clear shortcuts to player profiles, console, world and backups.
- Player profile implementation from draft [PR #19](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/19): searchable observed identities, online/offline session history, network details, connection incidents, attributable events and explicit missing-data limits.
- Safe quality-of-life implementation from draft [PR #20](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/20): raw-log/parsed-event switch, query filtering, read-only readiness and clipboard-only privacy-safe support report.
- Existing settings, mod controls, backup/restore and server control paths remain in their development state, with no changes to the live installation.
- Windows source/build lists now include `PlayerProfiles.cs` and `QolReadOnly.cs`; combined fixture assertions cover both plus sidebar behavior.

## Out of scope

- No GPS/quest navigation, altar rotation or new phone-app work (canceled/paused).
- No client inventory editor or player location claims: the current server interface cannot verify these.
- No new gameplay patches or changed game/mod bytes.
- No server deployment, restart, file cleanup, migration, save edit or new launcher auto-start.
- No signed public installer or final release claim.

## Windows preview verification gates

The [Windows preview workflow](../../.github/workflows/windows-preview-artifact.yml) is configured on this branch. A successful workflow creates a 14-day **unsigned** artifact containing `ShroudForge-Launcher-Preview.exe`, supporting assets, documentation, `BUILD-INFO.txt` (source SHA, ref, run), and `SHA256SUMS.txt` for the executable.

1. **CI:** Windows .NET Framework launcher compilation and fixture/native test jobs must complete successfully on this exact commit. A submitted workflow alone does not count.
2. **Static/UI:** inspect generated Dashboard, Players, Logs, narrow-window and control-state screenshots; check DPI, scrolling, keyboard focus and selected-item feedback.
3. **Disposable install:** extract to a new isolated test directory. Do not copy over the known-good launcher or point the test executable at the live server.
4. **Compatibility:** verify the exact installed dedicated-server build, three working mods and existing server paths from the Windows host.
5. **Release decision:** only after review, choose a documented version label, preserve rollback and explicitly authorize any actual live change.

## Notes for continuing after the interrupted Work session

The prior desktop Work session's exact uncommitted files have **not** been recovered or compared. This branch consolidates available GitHub implementations matching the agreed UI direction; it must not be treated as a byte-for-byte restoration of unseen local edits. Keep [PR #18](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/18), [PR #19](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/19) and [PR #20](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/20) unmerged until validation and reconciliation.

The working Velaris server and its saves/mods are independent of this staged GitHub candidate.
