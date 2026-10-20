# ShroudForge — verified development and decision chronology

> **October 20 GitHub-only follow-up:** Separate [draft PR #32](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/32) stages iPhone HTTP-parent secure-context map diagnostics, a direct HTTPS map fallback, HTML/C# synchronization and phone tests, six server-creation rejection cases and acceptance docs. Source synchronization and expected test text were statically checked. **New Windows CI, the real iPhone map and new-server installation have not yet been verified**. No live or desktop files were touched by this follow-up.

## October 20 — New server beside the realm box

Owner requested a visible top-bar New server entry leading to a create/scan guide. Implemented a dedicated Server setup page: create opens the native new-server form directly, while scan opens discovery. Realm and New server controls remain visible at minimum width. Guide navigation and twenty combined top-bar/overview viewport checks passed, including no page scrolling, no footer/metrics overlap and a visible adjacent button. No real server was created or selected.

Published **2a83af68ccfb6d2fd03ae256470678b6c0801cdc**, local installed **2432857f2723f8e8a3cda276b8ed27f3ba3c19aa**, identical tree **d1689eda00df06591378408885c9da59de5109cb**. Both local Windows builds, 317 fixtures, 10 native checks, installer and provenance passed; exact-head [modern #20](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39422846373) and [historical #155](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39422846386) passed. Installed package hashes, unchanged selected settings/server/mod script hashes, query readiness, Running observation, clean scan and MB output verified. The previously restarted server process continued running. Rollback retained; no further server restart. Native dialog interaction and real Steam download/new-server gameplay remain unverified. PR #31 stays open.

## October 20 — close/server setup, sizing correction and MB units installed

Published source **75952a5ec4e2461c89e6d783d289ad0ac32b5349**, local **5f351990b92da1c4f49df5df07cb160e50438dd5**, identical tree **9e1cf8c6081b5a5d41c6e6b1a0e982e4fa9f346c**, on dev/server-setup-and-close-prompt and the open PR #31 feature branch. Earlier feature source e49bfcdf/local49115cf is included.

Implemented close choices, bounded scan and separate-server creation, per-server automation/history, a 1100 × 900 CSS minimum with DPI/zoom adjustment, normal-flow hero actions/metrics, and decimal MB cards/charts/file sizes. The first fit check missed a 12 px overlap at 1440 × 900; the owner report was reproduced, then corrected. Twenty viewport combinations now pass geometry/scroll/footer checks; button hit tests and Players navigation pass. Native screenshot capture remained unavailable.

**317 fixtures, 10 native dummy checks, 15 installer checks, both Windows builds, TypeScript, bundling, phone synchronization and provenance passed.** Installer also passed PS5.1; full package fixture installation passed. Current-head [modern #18](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39421833951) and [historical #153](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39421833866) both passed and uploaded artifacts.

Installed with private rollback retained. One later explicitly authorized graceful server and launcher restart completed; subsequent launcher-only updates kept that server process running. Final package hashes, unchanged selected settings/server/mod script hashes, query readiness, Running observation, clean inventory scan and MB output verified. No second server restart was issued. No live player/private path data is published here. Real SteamCMD download, fresh-machine dependency installation, new-server gameplay and native-dialog interaction remain unverified. Installation polish and GitHub updates remain open; accepted items 1–3 are not reopened.


**Public project history, consolidated October 19, 2026.** This is a selective engineering record, **not** a copy of private conversations, local desktop history or sensitive server telemetry. Where testing occurred only in CI or as an owner's observation, that distinction is explicit.

| Date | What happened | Evidence / limitations |
| :-- | :-- | :-- |
| Oct 15, 2026 | Enshrouded 1.0 compatibility and legacy launcher/injector reliability were investigated; preserved existing files and saves. Earlier XHL v1 patches/packages existed. | Historical conversation/files; failed injector attempts were not proof of compatibility. Preserve the known-good mod loader. |
| Oct 16 | Public GitHub landing, documentation and launcher 2.0 UI overhaul began. User emphasized easily navigable branding, personal-use priority, safe mod continuity. | Existing [2.0 development branch and draft #18](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/18); not a public stable installer. |
| Oct 16–17 | Launcher concept broadened to configuration editor, backup/restore, server metrics and read-only player data; flight behavior, stamina and one-player sleep discussed. | Features have different verification states; desktop host/browser compatibility not proved by source alone. |
| Oct 18 | UI, navigation and GitHub housekeeping progressed; mobile companion with external 3D map and separate map research were explored. Flame altar 45° concept was canceled. | Earlier map/quest research kept in draft/archives, not approved active features. |
| Oct 19 | Owner confirmed working Enshrouded server and **three existing mods**; emphasized finishing dark-fantasy launcher UI and removed GPS from scope. Phone expansion paused. | Owner observation; not evidence a new GitHub build is installed. |
| Oct 19 | Provisional three-group [development tracker](development-tracker.md) and [next session instructions](NEXT-SESSION.md) written. | Historical percentages estimated, not code-audited. |
| Oct 19 | Staged a consolidated UI source candidate with sidebar polish, dashboard activity, player profiles and QoL event feed based on drafts #19 and #20. | [Draft PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31) targets #18 source branch; not merged to main/live. |
| Oct 19 | First reviewed consolidated Windows candidate repaired a C# syntax issue and passed compilation, 167 fixtures and native tests. | [Windows CI run #113](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39345947576); unsigned temporary artifact; no visual/production acceptance. |
| Oct 19 | Phone map showed white inside allocated iframe. Added fallback/troubleshooting, then temporary embedded-map Safari diagnostic. | User **confirmed** GameVault rendered inside iframe in **Compatibility** mode; Protected mode remained blank. Standalone web test has no server control functionality. |
| Oct 19 | Updated staged phone tab to default Compatibility while preserving Protected toggle and click-to-load. | [Draft PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31); actual owner's installed phone page **not confirmed updated**. |
| Oct 19 | New reviewed PR #31 source committed, passing **172 fixtures**, native checks, Windows build and artifact upload. | [Windows CI run #135](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39357907800); reviewed head `a5fcf210054dfea7d09ad4ae5c8f6df0cab2ec36`. Recheck if head advances. |
| Oct 19 | Created a new desktop Work/Codex handoff with explicit no-server-shutdown instructions. | **Session creation was acknowledged**, but desktop connection, work execution and installed updates **weren't independently confirmed in this chat**. Never mark host work done based on handoff acceptance. |
| Oct 19 | Consolidated a public cross-session project record on `main`; linked root README, AGENTS, roadmap, documentation hub and tracker. Reconciled outdated Issues #12 and #10 to ShroudForge branding and closed canceled quest-tracker Issue #23 as not planned. | The [current state](CURRENT-STATE.md), [handoff](SESSION-HANDOFF.md), [decisions](DECISIONS.md), [issue register](ISSUE-REGISTER.md) and [AI workflow](AI-WORKFLOW.md) are deliberately independent of live-server deployment. |

## Document a future session

Append a new row **only after observing or verifying the stated event**. Record branch, SHA, test run, device/host evidence and unresolved gates. Do not copy private chats or user credentials into this public document.

Use the [handoff report template](SESSION-HANDOFF.md) for substantial implementation, and update [CURRENT-STATE.md](CURRENT-STATE.md) if this record supersedes the current snapshot.

## Prior Work history limitation

Phone ChatGPT chats, desktop Work and Codex sessions are **not** synced to GitHub automatically. Some local Work changes might exist without a corresponding commit. This record summarizes known outcomes; it does not certify that **every** previous file, chat or unpublished desktop change has been recovered.

[Current state](CURRENT-STATE.md) · [Decisions](DECISIONS.md) · [Session handoff](SESSION-HANDOFF.md)

## October 19 EDT / October 20 UTC — pre-deployment candidate validation

Reconciled recovered 3.0 desktop baseline 0873d2f with PR #31 a5fcf21. Published development head 3e868582d47f21df547fcc554e8ad172814c3ed2; main receives documentation only. Installed metadata verified as 3.0.0-private.1; candidate version 3.0.0-private.2. See CURRENT-STATE and the PR reconciliation report for changed features and exact validation. Both Windows builds, TypeScript/bundle, 247 fixtures and seven native dummy checks passed; browser routes and phone modes/fit checked. Native visual/DPI, physical iPhone terrain and disposable-game acceptance remain unverified. Separate checksummed candidate staged; installed launcher and live server untouched. Modern Windows run #4 and historical run #139 passed. Initial frozen-lock specifier mismatch was corrected before the successful rerun. Native startup/accessibility passed; screenshot capture/input geometry prevented full visual acceptance.

## Live deployment — owner authorized October 19, 2026, 11:50 PM EDT

The owner explicitly waived a separate test server, paused phone work for today, and authorized taking the candidate online with a server/launcher restart. **3.0.0-private.2 is now installed and running**, not merely staged. Exactly one graceful server restart was submitted; the server was query-confirmed online before and after the launcher replacement. Installed package file hashes matched; checked server executable/configuration/startup/controller files remained unchanged. Previous launcher files and manager state were retained privately for rollback. No game patch, mod replacement, save restoration or public GitHub release/PR merge was performed.

The owner's iPhone screenshot shows GameVault failing with crypto.subtle.digest unavailable. Phone work is paused; this defect is **not fixed** or certified by the deployment. Native visual/DPI and gameplay acceptance remain limited as recorded below. The test-server acceptance gate was waived by the owner, not passed.

## End-of-session GitHub reconciliation

Current-state table, handoff and tracker now consistently identify private.2 as installed. PR #31 records the authorized deployment, passing CI, retained rollback, waived test-server acceptance and unresolved iPhone map error. Earlier staged-only statements describe the pre-deployment phase. No additional server operation was performed for this documentation update.

## Selective reliability refactor — October 20, 2026

The refactor is implemented, validated locally, and deployed with owner authorization. GitHub source is [39684d2](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/commit/39684d20d05ce1f12f83562f5994c1cf2c3d6956) on `dev/selective-review-refactor` and `feat/ui-navigation-polish-20261019`, tracked in [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31). The installed source commit is local `507d86c764f04784dd88361c8fcea04ff92c3a88`. Both commits have **identical Git tree `9fab889881a443c30f6e2e683f5f5ba956591470`**; GitHub publication retained the existing remote ancestry through the connected app when CLI credentials were unavailable. The candidate checkout remains clean on its local development branch.

Changes: explicit Running/Stopped/Failed/Unknown process observations; recovery waits through observation failures; atomic inventory snapshots retain last-good data and expose errors/age; concrete desktop host/UI types; consistent operation results; verified build provenance; classic recovery capability replaces source-text rewriting. Existing safe-start, graceful stop, backup implementation, settings/journals, and game/mod integration are preserved.

Local modern/classic Windows builds, strict TypeScript, UI bundle, exact phone synchronization, **264 fixture checks** (40 desktop, 160 core, 24 resilience, 14 fields, 26 mods), seven native dummy-server checks, and four provenance acceptance/rejection cases passed. An initial mod fixture hit the existing .NET Framework path-length limit; a shorter isolated evidence path passed without changing mod code.

**Installed:** private.2 rebuilt from the refactor. Package hashes, Running observation, local game query, and error-free inventory scan verified after launcher relaunch. **Zero game-server restarts for this deployment.** The server process/start time, checked server files, and launcher settings remained unchanged. Previous launcher and manager state are retained privately for rollback. The version string stays 3.0.0-private.2; commit and build provenance distinguish the build.

**CI for 39684d2:** modern Windows [run #6](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39407890515) and historical Windows [run #141](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39407890572) both passed on this exact head, including fresh dependency restore, Windows compilation, isolated checks, packaging and artifact upload. Older runs remain historical evidence.

**Compatibility:** process observation errors now propagate rather than confirming absence; uncertain observations pause writes/recovery. A failed inventory section retains the entire last complete scan until a successful retry. Packaging requires committed source and fresh matching provenance; old outputs need validation/rebuild. Wire metadata is additive and existing settings/journal formats are unchanged.

**Still unverified:** native visual/DPI/keyboard acceptance, physical iPhone behavior, real ACL-denial scenarios, real-game recovery/mod/sleep and full restore rehearsal. The GameVault crypto.subtle.digest defect remains unresolved; phone expansion stays paused; GPS and altar rotation stay canceled. No public release or application-code merge into main occurred. Detailed operational paths, keys, process identifiers, logs and rollback data remain private.

[Source details](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/ui-navigation-polish-20261019/desktop/react-launcher/SELECTIVE-REFACTOR.md).

## Offline player/session metadata — October 20, 2026

**Implemented and installed.** Offline member profiles now recover previously recorded Steam identity, peer/handle/machine and character-save transfer size, last completed visit/duration, and historical ping/connection measurements. Existing server logs, session CSVs and the private health-history CSV provide the backfill. Missing fields are filled from prior observations when available; unsupported or never-recorded values remain explicitly unrecorded.

A private, server-scoped metadata cache survives launcher restarts and log rotation. Legacy and diagnostic session files are combined with copied visits deduplicated. Historical measurements are labeled with source/date and never represent a current connection, active visit or online member. Fresh live observations win. Conflicting known Steam IDs do not inherit each other's metadata. Read-only previews do not persist the cache; no game/save data is modified. Initial retained-log work is bounded to the latest 20 logs, 4 MiB each.

**Source:** [`a6c1db8`](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/commit/a6c1db84690798a4412867ac5be7ad05af89dc31) on `dev/offline-player-history` and `feat/ui-navigation-polish-20261019`, in [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31). Installed local commit is `f6d57159fa093b5a845f7ec77edb030f84df275c`; both have verified identical tree `a0b0ce2be8e29bc98ae912179030e8dc13731a25`. Connected-app publication preserved existing remote ancestry; do not force-push local history over it.

**Validation:** modern/classic x64 Windows builds, strict TypeScript, bundled UI, exact phone synchronization, **279 fixtures** (55 desktop, 160 core, 24 resilience, 14 fields, 26 mods), seven native dummy-server checks and four provenance cases passed. Focused checks cover archive reconstruction, health import, persistence/reload after log deletion, full offline snapshot projection, per-server isolation, identity conflicts, live precedence, read-only nonpersistence and duplicate visits.

**Deployment:** completed under the owner's continuing related-work approval. Installed package hashes, query readiness, Running observation and error-free scan verified; retained metadata import succeeded. The running game-server process/start time, checked server files and launcher settings remained unchanged. No game-server restart was requested. Rollback retained privately. The installer initially could not close the hidden launcher and replaced no files; the verified launcher then closed through its normal window-close handler, the package was installed and the interactive launcher reopened. No force-kill was used.

**Current-head CI:** modern Windows [run #8](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39413912020) and historical Windows [run #143](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39413912123) both passed on this exact head, including fresh dependency restore, compilation, checks, packaging and artifact upload.

**Limits:** recoverable fields depend on retained evidence. Archived log-write time is labeled as evidence time when an exact sample timestamp is unavailable. Session totals are matched by display name and can be affected by renames/reused names. Inventory, level, equipment and position remain unsupported. Native visual/DPI/keyboard review, actual iPhone behavior and real-game recovery/mod/restore acceptance remain unverified. The known GameVault crypto.subtle.digest defect is unresolved; phone expansion stays paused and GPS/altar rotation stay canceled.

[Offline history details and changelog](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/ui-navigation-polish-20261019/desktop/react-launcher/PLAYER-HISTORY.md).

## Owner scope update and installation polish — October 20, 2026

| Item | Current status |
| :-- | :-- |
| 1. Windows UI acceptance | **Accepted as-is by the owner.** No further acceptance work requested. |
| 2. Backup/restore and recovery | **Accepted as-is by the owner.** Do not hold completion open for an additional rehearsal. |
| 3. Mod and settings behavior | **Accepted as-is by the owner.** Preserve existing working behavior. |
| 4. Installation/update polish | **In progress. First implementation slice is tested and published.** |
| 5. GitHub/project closeout | **Keep open for running updates.** Do not merge/close PRs or declare project closeout. |

Owner acceptance is recorded as an owner decision; it is not a claim of newly performed independent gameplay, restore, visual or device tests. Earlier lists of acceptance gaps remain historical evidence, not blockers overriding this instruction.

### First installation-polish slice

Normal launcher closure now supports hidden windows while honoring busy-operation close guards. No forced termination or game-server action is used. Updates validate package paths/hashes/sizes, reject overlapping folders and reused rollback locations, preserve private Manager Data (including offline player history), and restore already replaced files after an update failure. The updater creates `Open ShroudForge.lnk` and preserves/updates the existing `Open XHL 2.0.lnk`; existing folders/executable/private-data paths remain compatible.

Portable packages now include the updater, normal-close helper and [install/update guide](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/ui-navigation-polish-20261019/desktop/react-launcher/INSTALL-UPDATE.md). Fresh-machine onboarding and automatic installation-folder migration are not included in this first slice. Further installation polish stays open.

### Exact source, package and running installation

- Published source: [`acc49e5`](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/commit/acc49e5806ca0e3d58bb122d67624a1dc8c7ffaa), on `dev/installation-polish` and `feat/ui-navigation-polish-20261019`, [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31).
- Local commit: `3f57ec163f8c5826e9d1167ee29baea02101f9d7`; matching source tree `e306b55f07f70894575ec6dc7cd2076dd43ca415` verified before publication. Connected-app publication retains existing remote ancestry.
- Updated package is staged and tested with the included installer in a disposable directory. **No live update, launcher relaunch, game-server restart, settings change or folder migration was performed for this installation-tooling work.**
- Running installation remains the offline-history build: local `f6d5715`, published matching source `a6c1db8`. Do not label the new installer/tooling source as installed.

### Validation

Modern/classic Windows builds, strict TypeScript, UI bundle, phone synchronization and the existing 279 fixture checks/seven native dummy-server checks passed. **15 installer checks passed under both PowerShell 7 and built-in Windows PowerShell 5.1**, covering hidden/busy windows, exact-path exclusion, normal relaunch, rollback retention, failed-copy rollback, shortcut compatibility, package tampering, unsafe/duplicate paths and directory overlap. The complete real package also installed into an isolated fixture with every manifest hash verified and no launcher execution.

Current-head modern Windows [run #10](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39416441249) and historical Windows [run #145](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39416441239) both passed on this exact head, including fresh dependencies, builds, installer checks, packaging and artifact upload. These checks maintain build/install integrity; they do not reopen owner-accepted items 1–3.

Phone expansion remains paused and the known GameVault crypto.subtle.digest defect stays recorded. GPS and altar rotation remain canceled. Public release/signing are separate future work.
