# ShroudForge — acceptance handoff

## Live deployment — owner authorized October 19, 2026, 11:50 PM EDT

The owner explicitly waived a separate test server, paused phone work for today, and authorized taking the candidate online with a server/launcher restart. **3.0.0-private.2 is now installed and running**, not merely staged. Exactly one graceful server restart was submitted; the server was query-confirmed online before and after the launcher replacement. Installed package file hashes matched; checked server executable/configuration/startup/controller files remained unchanged. Previous launcher files and manager state were retained privately for rollback. No game patch, mod replacement, save restoration or public GitHub release/PR merge was performed.

The owner's iPhone screenshot shows GameVault failing with crypto.subtle.digest unavailable. Phone work is paused; this defect is **not fixed** or certified by the deployment. Native visual/DPI and gameplay acceptance remain limited as recorded below. The test-server acceptance gate was waived by the owner, not passed.

## Earlier acceptance snapshot (historical)

Prepared October 20, 2026. First read AGENTS.md, CURRENT-STATE.md and DECISIONS.md. Do not re-plan or downgrade to the old 2.2 candidate.

## Exact source

Installed binary metadata: **3.0.0-private.1** (file version 3.0.0.0), inspected read-only. Unified candidate: **3.0.0-private.2**, development head **3e868582d47f21df547fcc554e8ad172814c3ed2**, [draft PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31). Modern build input is desktop/react-launcher plus desktop/field-audit-launcher. launcher-2-preview is retained historical source. The validated local source tree exactly matches the GitHub publication tree; the connected GitHub app published after CLI credential handling stalled.

Local build/validation passed: both Windows executables, TypeScript, bundle, embedded phone synchronization, 247 fixture checks and seven native lifecycle checks. Browser verification covered all twelve pages, profile sheets, event search, minimum-width dashboard and compact phone modes. Modern Windows run #4 (39365624611) and historical run #139 (39365624690) passed for this head. The first modern CI failed on stale lockfile specifiers; all seven were corrected and fresh CI passed. Reuse valid local evidence instead of repeating every build.

## Next necessary checks

1. CI and fresh dependency restore passed. Retain run #4 and #139 evidence; rerun only for relevant source changes.
2. Complete native WebView2/DPI/keyboard visual acceptance; a later visible preview verified startup/accessibility, but screenshot capture timed out and click input lacked geometry. Use disposable settings and --preview-from; do not copy live settings.
3. Check actual iPhone Safari terrain/touch on the exact phone page. Compatibility is default; Protected/external fallback retained. Browser iframe dimensions/mode attributes passed, but are not device certification.
4. If separately authorized, rehearse real-game mod/sleep and backup/restore using a disposable server/world. No live server testing.

## Package and rollback

A separately staged, human-readable 3.0.0-private.2 candidate contains required managed/native WebView2 DLLs, UI, phone, assets, instructions, manifest and checksums. Private package paths and operational evidence stay local. No installed file was changed. Rollback for this staged review means close/discard only the candidate; preserve the working launcher and existing rollback archives. A future migration must preserve automation journals and requires separate review/authorization.

No server start, stop, restart, patch, mod/config/save write or backup restoration. No merge/public release. GPS and altar rotation stay canceled; companion expansion stays paused.
