# ShroudForge — Development tracker

**For latest source/test/install state:** see [CURRENT-STATE.md](CURRENT-STATE.md) and [SESSION-HANDOFF.md](SESSION-HANDOFF.md). This table intentionally preserves the owner/assistant **October 19 historical planning estimates**; its numeric percentages must not be silently recalculated from passing CI or newly written documentation. Read the [issue register](ISSUE-REGISTER.md) for priority and blockers.

**Latest verified update — October 20:** Close choices, create/scan server setup, responsive overview with an enforced DPI/zoom-aware minimum, unobstructed View players, and correct decimal MB displays are installed. Source **75952a5** / local **5f35199**, identical tree **9e1cf8c6081b5a5d41c6e6b1a0e982e4fa9f346c**. Current-head CI #18/#153 passed. One explicitly authorized server restart was completed and must not be repeated automatically. Existing UI/recovery/mod behavior stays accepted as-is; installation polish remains in progress and PR #31 stays open. See [current state](CURRENT-STATE.md) for evidence and unverified limits.

**Owner planning baseline: October 19, 2026.** This tracker consolidates the October 19 development discussion. It is **not** a source-code audit, deployment manifest, or proof that each feature works on the live server.

**Evidence labels:** percentages are provisional owner/assistant estimates; **Verified** describes an explicit observed runtime check; **Testing**, **Verify**, and **Research** are stages, not completion percentages. The dates reflect recorded discussions or development activity, not necessarily Git commits. The repository's main-branch technical documentation was last reviewed October 18, 2026.

## 1. Launcher & Interface

| Feature | Progress | Started | Last active |
| :-- | :-- | :-- | :-- |
| Dashboard redesign | 75% | Oct 16 | Oct 19 |
| Dark-fantasy visual theme | 85% | Oct 16 | Oct 19 |
| Navigation & tabs | 80% | Oct 16 | Oct 19 |
| UI animations & tooltips | 40% | Oct 16 | Oct 19 |
| Startup splash screen | 20% | Oct 15 | Oct 15 |
| Live server statistics & graphs | 30% | Oct 15 | Oct 17 |
| Player profiles & activity | 20% | Oct 15 | Oct 17 |
| Inventory viewer/editor | Research | Oct 15 | Oct 17 |

## 2. Server, Mods & Data Management

| Feature | Progress | Started | Last active |
| :-- | :-- | :-- | :-- |
| Server launch & detection | Verified | Oct 15 | Oct 19 |
| Server stop/restart | Testing | Oct 15 | Oct 19 |
| Server settings editor | Verify | Oct 16 | Oct 18 |
| Mod configuration sliders | 65% | Oct 18 | Oct 19 |
| Flight pitch persistence | Testing | Oct 17 | Oct 17 |
| Flight stamina toggle | Testing | Oct 16 | Oct 17 |
| One-player sleep / 60× | Testing | Oct 16 | Oct 16 |
| Crash detection & recovery | 35% | Oct 15 | Oct 18 |
| Live console & event logging | 40% | Oct 15 | Oct 17 |
| Mod compatibility checks | 15% | Oct 16 | Oct 17 |
| Dependency validation | 25% | Oct 15 | Oct 17 |
| Backup & restore interface | 70% | Oct 15 | Oct 17 |
| Save history & safeguards | 35% | Oct 15 | Oct 17 |

## 3. Installation, Reliability & GitHub

| Feature | Progress | Started | Last active |
| :-- | :-- | :-- | :-- |
| Directory discovery & cleanup | 20% | Oct 15 | Oct 17 |
| Human-readable file structure | 20% | Oct 15 | Oct 18 |
| GitHub organization | 30% | Oct 16 | Oct 18 |
| GitHub synchronization | Unverified | — | — |
| Installation guide / README | 35% | Oct 16 | Oct 18 |
| Standalone launcher testing | 50% | Oct 15 | Oct 19 |

## Owner-confirmed state and scope

- On October 19, the **existing live server and its three working mods** were reported functional. A server process was separately confirmed running. These observations do **not** validate every proposed code change or development branch.
- Current priority: **launcher dashboard, navigation and overall interface**. Working-server safety takes precedence over speculative functionality.
- **GPS / replacement quest navigation:** canceled from active scope. Older research branches/PRs may remain in the repository for historical reference, but they are not an approved release feature.
- **Companion phone app:** paused.
- **Flame altar rotation:** canceled.
- **No automatic synchronization:** ChatGPT Project conversations/instructions are planning context; this GitHub document is the published planning snapshot. Update this document when decisions change, and audit against actual files and tests before upgrading any feature to verified.

## GitHub implementation references

- [Main project status](README.md) — verified vs. drafted capabilities.
- [Launcher 2.0 development branch](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/tree/feat/launcher-2-overhaul/launcher-2-preview) and [draft PR #18](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/18).
- [Player profile draft PR #19](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/19).
- [Quality-of-life draft PR #20](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/20).
- [Public-server safety draft PR #25](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/25).

**Next audit:** compare this tracker against current launcher branch files, build results, GitHub PR state, and live host observations. Do not treat existence of a branch, script or green CI check as a live deployment.

## Live deployment — owner authorized October 19, 2026, 11:50 PM EDT

The owner explicitly waived a separate test server, paused phone work for today, and authorized taking the candidate online with a server/launcher restart. **3.0.0-private.2 is now installed and running**, not merely staged. Exactly one graceful server restart was submitted; the server was query-confirmed online before and after the launcher replacement. Installed package file hashes matched; checked server executable/configuration/startup/controller files remained unchanged. Previous launcher files and manager state were retained privately for rollback. No game patch, mod replacement, save restoration or public GitHub release/PR merge was performed.

The owner's iPhone screenshot shows GameVault failing with crypto.subtle.digest unavailable. Phone work is paused; this defect is **not fixed** or certified by the deployment. Native visual/DPI and gameplay acceptance remain limited as recorded below. The test-server acceptance gate was waived by the owner, not passed.


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
