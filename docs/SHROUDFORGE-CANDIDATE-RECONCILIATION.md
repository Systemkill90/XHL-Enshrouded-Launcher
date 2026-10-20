# Reconciliation and acceptance evidence — October 20, 2026

## Source selection

PR #31 head before reconciliation: a5fcf210054dfea7d09ad4ae5c8f6df0cab2ec36. Its Windows run [#135](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39357907800) passed compilation, 172 fixtures, native checks and artifact upload. Launcher 2.0 base at review: 736e4d94fee5d62eadee468f5fd6df884a261c44.

The recovered desktop source baseline 0873d2fbf7519573dd540c78c710786b4bf36d1b is newer and carries package/assembly informational version **3.0.0-private.1**, file version 3.0.0.0. Local recovery and PR checkouts had no uncommitted changes before this session; neither was overwritten. Existing recovery archives and history bundle remain local. The working installed executable's metadata is checked separately from source; a legacy folder label is not a version.

The recovered core already includes PlayerProfiles and QolReadOnly, plus later stale-network/jitter/counter wording, mod library, query telemetry, configuration field audit, realm isolation, backup journals and bounded resilience. Those supersede the older PR core. Preserve that implementation in desktop/ rather than replacing it with the older WinForms screen layout. The historical launcher-2-preview remains available for source comparison. Generated classic-source is excluded; classic tools compile from the same reconciled core.

## Candidate validation

Both Windows executables and the React bundle compiled. TypeScript passed. Local fixtures: **247 passed**, comprising 23 desktop safety/profile/history, 24 resilience/backup, 14 field, 26 mod-management, and 160 regression checks. Seven native dummy-process checks passed, including exact-path detection and graceful Ctrl+C. Game/save sentinels remained unchanged. No live server path was passed to test scripts.

Browser testing used synthetic status and a loopback GET-only preview that rejects POSTs. All twelve pages opened; available player sheets and event search rendered. Minimum 1080x740 layout had no horizontal overflow. Tools navigation and focus were checked. Phone Compatibility uses the fixed GameVault URL without sandbox and with no-referrer; Protected restores its sandbox. Compact iframe heights were 619px at 390x844 and 343px at 375x568. Overview and other existing phone surfaces remain present. The provider document loaded; this does not certify physical-device WebGL terrain. Embedded and standalone phone HTML synchronization passed.

An isolated native preview process was started with disposable settings. Hidden-window UI capture did not expose a targetable candidate window, so native visual startup/DPI acceptance is **not claimed**. Windows compile/fixture evidence and browser rendering are distinct from native visual acceptance.

Initial tooling failures were corrected: sandbox dependency traversal blocked the first bundle; the isolated build then passed outside the sandbox. Incorrect invocation/output paths were corrected before validation. The old desktop map provider was visibly blocked; the candidate now uses GameVault with an external fallback. No known compilation or fixture failure remains. CI for the newly reconciled head must be checked independently; older green CI does not certify new source.

## Deployment boundary

Candidate version: **3.0.0-private.2**. Assembly/file version stays 3.0.0.0; informational/package version distinguishes the private candidate. Dependencies, instructions and checksums are staged separately. Installed launcher and live server are not replaced, stopped, restarted, patched or migrated. No game/mod/config/save/backup write is part of this session. No public release or merge.

Remaining acceptance: actual iPhone Safari terrain and touch interactions, native Windows WebView2/DPI/keyboard review, and owner-authorized disposable-game mod/sleep/backup-restore rehearsals. Preserve installed automation and existing rollback archives until a separate migration is reviewed.
