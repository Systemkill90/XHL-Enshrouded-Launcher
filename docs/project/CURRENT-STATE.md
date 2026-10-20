# ShroudForge — current verified development state

> **October 20 — existing phone companion redesign (staged, not installed):** The owner canceled the proposed native App Store app and authorized an overhaul of the existing ShroudForge private-LAN web companion. [Draft PR #33](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/33), stacked on draft #32/#31, implements an Overview / Players / Activity / Map mobile UI with a direct **owner-confirmed working** HTTPS GameVault map as the primary action and the HTTP-embedded iframe moved into an experimental panel. Existing authenticated controls, backup/recovery, player information and event API are retained. The standalone and C# embedded HTML sources passed exact static synchronization checks; new Node and C# regression tests are staged. **Windows CI passed** on application source `3445a5b9`: [modern](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39438883682) and [historical preview](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39438883680). **Installed desktop HTML and physical iPhone layout remain unverified**; no live-server action occurred in this session.


> **October 20 — GitHub-only follow-up (staged; not installed):** [Draft PR #32](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/32), based on active draft #31, adds a secure-context explanation and top-level HTTPS fallback for the iPhone GameVault map, synced fallback HTML and phone regression checks, plus six isolated new-server setup failure cases and acceptance guides. The actual game server, desktop launcher, companion endpoint and installed source were **not accessed or changed** in this GitHub-only session. New Windows CI and iPhone results are **not yet verified**; do not confuse this source proposal with an embedded-map repair.

## Latest: close choices, server setup, window sizing and MB units — October 20, 2026

**Installed and locally verified.** Published source [2a83af6](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/commit/2a83af68ccfb6d2fd03ae256470678b6c0801cdc) on `dev/server-setup-and-close-prompt` and `feat/ui-navigation-polish-20261019`; local installed commit `2432857f2723f8e8a3cda276b8ed27f3ba3c19aa` has identical tree `d1689eda00df06591378408885c9da59de5109cb`. The initial feature commit is published `e49bfcdf96f3f518e13d4dab025784ffc817a9bd` / local `49115cf009de3918b1ef214eee29872ea791fb77`, tree `0cb757f21f50985081591bd76ed4b243295ec473`. Connected-app publication preserves remote ancestry.

### Changes available in the modern launcher

- Closing while the selected server runs offers **Keep server running**, **Stop server and exit** (graceful), or **Cancel**. Failed/unknown observation warns; failed stop leaves the launcher open. Monitoring and launcher automation pause while closed. Installer handover explicitly closes only the launcher and still honors busy guards.
- **New server beside the selected-server box** opens the dedicated **Server setup** guide with **Create a new server** and **Scan for an existing server** choices. Create opens the native new-server form directly; scan opens discovery. Both top-bar controls remain visible at the supported minimum width. Server selector, Tools and System page also provide bounded read-only discovery and separate standard-server creation. Creation requires a separate empty folder, an official SteamCMD selected by the owner, and confirmed app download. It writes current role-based configuration and leaves the new world unstarted. Existing/nonempty/overlapping destinations are rejected; partial failures stay in the new folder.
- Selection confirms a reopen and leaves the previous server running. Backup/recovery preferences, journals and telemetry are scoped per server; first selection starts automation off. Existing managed/modded servers retain the verified wrapper path. Standard enrollment records game-file fingerprints, rejects known mod loaders and blocks startup after changes; it does not prove vendor authenticity or arbitrary mod compatibility.
- The overview uses compact spacing/charts in supported windows. Native minimum protects **1100 × 900 CSS content pixels**, adjusted for display DPI and WebView zoom. A reproduced 12 px overlap at 1440 × 900 was corrected by placing actions and metrics in separate rows. Twenty width/height combinations also verify visible adjacent realm/New server controls, all navigation within the header, and unchanged overview fit. These combinations from 1100 × 900 through 1920 × 1440 pass no page scrolling, contained metrics, no footer overlap and at least 14 px of separation below View players. Button corners/center are unobstructed and navigation works; six navigation pages also fit minimum width. Long data pages and error details retain scrolling.

- Memory cards and charts use decimal **MB** (1 MB = 1,000,000 bytes), with converted chart values. File/world/player-transfer sizes use matching decimal B/kB/MB/GB/TB. Binary safety limits keep their thresholds and are labeled MiB; internal GiB history and incident schema stay compatible.

### Evidence and installation

Modern/classic builds, strict TypeScript, UI bundling, phone synchronization, **317 fixture checks** (93 desktop, 160 core, 24 resilience, 14 fields, 26 mods), **10 native dummy checks**, 15 installer checks and missing/stale/tampered provenance rejection passed. Installer compatibility also passed in Windows PowerShell 5.1. The full package installed into a disposable fixture before host installation. Exact-head CI: modern [run #20](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39422846373) and historical [run #155](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39422846386) both passed on the exact published head, including package artifacts.

The owner approved related deployment and later **one server and one launcher restart**. The initial launcher-only install preserved the running server. The single subsequent graceful server restart and launcher restart were verified query-ready, with unchanged server/configuration/mod script fingerprints and matching installed package. The sizing, overlap, MB and top-bar-guide follow-ups were then installed as launcher-only updates; that same restarted server process remained running, selected settings and server-file hashes stayed unchanged, process observation was Running, inventory scan had no error, and installed memory output used MB. Rollback/private manager data are retained locally. The one server-restart authorization has been consumed; do not repeat it automatically.

### Continuing scope and limits

Items 1–3 (existing UI, backup/recovery and mod/settings behavior) remain owner-accepted as-is. Item 4 installation polish remains in progress; item 5 GitHub stays **open for running updates**, including draft [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31). Do not merge or close. Phone expansion stays paused; GPS and altar rotation remain canceled.

Real SteamCMD download, fresh-machine dependencies, router/firewall setup, new-server gameplay and manual interaction with the new native dialogs remain unverified. Native screenshot capture timed out; browser viewport/layout measurements and compiled policy checks do not replace owner visual validation. No claim of new gameplay/mod compatibility certification is made. The version label remains private.2; exact commit/provenance identifies the build.

[Setup/update guide](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/ui-navigation-polish-20261019/desktop/react-launcher/INSTALL-UPDATE.md) · [Change log](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/ui-navigation-polish-20261019/desktop/react-launcher/SERVER-SETUP-CHANGES.md)



## Historical notes — superseded by the latest record above

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

## Earlier deployment and verification history — scope superseded above


## Offline player/session metadata — October 20, 2026

**Implemented and installed.** Offline member profiles now recover previously recorded Steam identity, peer/handle/machine and character-save transfer size, last completed visit/duration, and historical ping/connection measurements. Existing server logs, session CSVs and the private health-history CSV provide the backfill. Missing fields are filled from prior observations when available; unsupported or never-recorded values remain explicitly unrecorded.

A private, server-scoped metadata cache survives launcher restarts and log rotation. Legacy and diagnostic session files are combined with copied visits deduplicated. Historical measurements are labeled with source/date and never represent a current connection, active visit or online member. Fresh live observations win. Conflicting known Steam IDs do not inherit each other's metadata. Read-only previews do not persist the cache; no game/save data is modified. Initial retained-log work is bounded to the latest 20 logs, 4 MiB each.

**Source:** [`a6c1db8`](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/commit/a6c1db84690798a4412867ac5be7ad05af89dc31) on `dev/offline-player-history` and `feat/ui-navigation-polish-20261019`, in [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31). Installed local commit is `f6d57159fa093b5a845f7ec77edb030f84df275c`; both have verified identical tree `a0b0ce2be8e29bc98ae912179030e8dc13731a25`. Connected-app publication preserved existing remote ancestry; do not force-push local history over it.

**Validation:** modern/classic x64 Windows builds, strict TypeScript, bundled UI, exact phone synchronization, **279 fixtures** (55 desktop, 160 core, 24 resilience, 14 fields, 26 mods), seven native dummy-server checks and four provenance cases passed. Focused checks cover archive reconstruction, health import, persistence/reload after log deletion, full offline snapshot projection, per-server isolation, identity conflicts, live precedence, read-only nonpersistence and duplicate visits.

**Deployment:** completed under the owner's continuing related-work approval. Installed package hashes, query readiness, Running observation and error-free scan verified; retained metadata import succeeded. The running game-server process/start time, checked server files and launcher settings remained unchanged. No game-server restart was requested. Rollback retained privately. The installer initially could not close the hidden launcher and replaced no files; the verified launcher then closed through its normal window-close handler, the package was installed and the interactive launcher reopened. No force-kill was used.

**Current-head CI:** modern Windows [run #8](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39413912020) and historical Windows [run #143](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39413912123) both passed on this exact head, including fresh dependency restore, compilation, checks, packaging and artifact upload.

**Limits:** recoverable fields depend on retained evidence. Archived log-write time is labeled as evidence time when an exact sample timestamp is unavailable. Session totals are matched by display name and can be affected by renames/reused names. Inventory, level, equipment and position remain unsupported. Native visual/DPI/keyboard review, actual iPhone behavior and real-game recovery/mod/restore acceptance remain unverified. The known GameVault crypto.subtle.digest defect is unresolved; phone expansion stays paused and GPS/altar rotation stay canceled.

[Offline history details and changelog](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/ui-navigation-polish-20261019/desktop/react-launcher/PLAYER-HISTORY.md).

## Earlier reliability refactor and deployment evidence (historical)


## Selective reliability refactor — October 20, 2026

The refactor is implemented, validated locally, and deployed with owner authorization. GitHub source is [39684d2](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/commit/39684d20d05ce1f12f83562f5994c1cf2c3d6956) on `dev/selective-review-refactor` and `feat/ui-navigation-polish-20261019`, tracked in [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31). The installed source commit is local `507d86c764f04784dd88361c8fcea04ff92c3a88`. Both commits have **identical Git tree `9fab889881a443c30f6e2e683f5f5ba956591470`**; GitHub publication retained the existing remote ancestry through the connected app when CLI credentials were unavailable. The candidate checkout remains clean on its local development branch.

Changes: explicit Running/Stopped/Failed/Unknown process observations; recovery waits through observation failures; atomic inventory snapshots retain last-good data and expose errors/age; concrete desktop host/UI types; consistent operation results; verified build provenance; classic recovery capability replaces source-text rewriting. Existing safe-start, graceful stop, backup implementation, settings/journals, and game/mod integration are preserved.

Local modern/classic Windows builds, strict TypeScript, UI bundle, exact phone synchronization, **264 fixture checks** (40 desktop, 160 core, 24 resilience, 14 fields, 26 mods), seven native dummy-server checks, and four provenance acceptance/rejection cases passed. An initial mod fixture hit the existing .NET Framework path-length limit; a shorter isolated evidence path passed without changing mod code.

**Installed:** private.2 rebuilt from the refactor. Package hashes, Running observation, local game query, and error-free inventory scan verified after launcher relaunch. **Zero game-server restarts for this deployment.** The server process/start time, checked server files, and launcher settings remained unchanged. Previous launcher and manager state are retained privately for rollback. The version string stays 3.0.0-private.2; commit and build provenance distinguish the build.

**CI for 39684d2:** modern Windows [run #6](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39407890515) and historical Windows [run #141](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39407890572) both passed on this exact head, including fresh dependency restore, Windows compilation, isolated checks, packaging and artifact upload. Older runs remain historical evidence.

**Compatibility:** process observation errors now propagate rather than confirming absence; uncertain observations pause writes/recovery. A failed inventory section retains the entire last complete scan until a successful retry. Packaging requires committed source and fresh matching provenance; old outputs need validation/rebuild. Wire metadata is additive and existing settings/journal formats are unchanged.

**Still unverified:** native visual/DPI/keyboard acceptance, physical iPhone behavior, real ACL-denial scenarios, real-game recovery/mod/sleep and full restore rehearsal. The GameVault crypto.subtle.digest defect remains unresolved; phone expansion stays paused; GPS and altar rotation stay canceled. No public release or application-code merge into main occurred. Detailed operational paths, keys, process identifiers, logs and rollback data remain private.

[Source details](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/ui-navigation-polish-20261019/desktop/react-launcher/SELECTIVE-REFACTOR.md).

## Historical deployment and acceptance snapshot — superseded where noted above


## Live deployment — owner authorized October 19, 2026, 11:50 PM EDT

The owner explicitly waived a separate test server, paused phone work for today, and authorized taking the candidate online with a server/launcher restart. **3.0.0-private.2 is now installed and running**, not merely staged. Exactly one graceful server restart was submitted; the server was query-confirmed online before and after the launcher replacement. Installed package file hashes matched; checked server executable/configuration/startup/controller files remained unchanged. Previous launcher files and manager state were retained privately for rollback. No game patch, mod replacement, save restoration or public GitHub release/PR merge was performed.

The owner's iPhone screenshot shows GameVault failing with crypto.subtle.digest unavailable. Phone work is paused; this defect is **not fixed** or certified by the deployment. Native visual/DPI and gameplay acceptance remain limited as recorded below. The test-server acceptance gate was waived by the owner, not passed.

Updated October 19 EDT / October 20 UTC, 2026. Main records the authorized host deployment; application source remains on the development branch.

| Surface | Confirmed state |
| :-- | :-- |
| Installed launcher | **3.0.0-private.2**, deployed with explicit owner authorization. Installed package hashes verified; previous launcher and settings retained for rollback. |
| Unified candidate | **3.0.0-private.2**, [draft PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31), development head [3e86858](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/commit/3e868582d47f21df547fcc554e8ad172814c3ed2). Modern source under desktop/, historical Launcher 2.0/PR source retained under launcher-2-preview/. |
| Reconciliation | Recovered local baseline 0873d2f was clean and newer than PR head a5fcf21. Preserved modern query telemetry, mod management, field audits, recovery/backup journals, classic tools and phone functions. Original local checkouts, recovery archives and uncommitted work were not overwritten. |
| Local validation | Modern and classic Windows builds, TypeScript/UI bundle, **247 fixture checks** and seven native dummy-process lifecycle checks passed. These used disposable fixtures, not a live game path. |
| Browser validation | All twelve pages, observed player sheets, searchable events, minimum 1080x740 dashboard, phone tabs and mode switching checked with synthetic status. Phone frame fit checked at 390x844 and 375x568. Provider document loaded; physical iPhone terrain is not certified. |
| CI | Prior PR head a5fcf21 passed Windows run #135 (172 fixtures + native checks). New modern Windows [run #4](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39365624611) and historical Windows [run #139](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39365624690) passed for this head, including fresh dependency restore, compilation, fixtures, packaging and artifact upload. |
| Package | Portable unsigned private.2 package installed and verified; checksummed archive and rollback retained privately. No public release. |
| Live server | Exactly one authorized graceful restart completed; local game query confirmed readiness after server restart and launcher replacement. Checked server executable/configuration/startup/controller files remained unchanged. |

## Completed candidate work

CPU and memory histories from process samples; full available player profiles and completed-session identities; offline/stale network labels; searchable recorded events plus bounded live console; empty/error states, navigation focus, responsive styling and reduced motion; branded loading/error surface; GameVault desktop map with external fallback. Existing settings, mod, world, backup/restore and resilience interfaces are preserved with their operation guards.

Phone scope remains narrow: GameVault Compatibility default, reversible Protected mode, no-referrer, external fallback, compact viewport fit, exact embedded/standalone synchronization. Other companion functions are retained. Broader phone development stays paused; GPS and altar rotation remain canceled.

## Remaining acceptance gates

- Phone work is paused for today. Owner screenshot confirms the GameVault crypto.subtle.digest failure; physical-device terrain remains broken/unverified.
- Native Windows WebView2 visual, DPI (100/125/150/200%) and keyboard review. Native WebView2 startup and the isolated read-only dashboard were verified through accessibility. Native screenshot capture timed out and click input lacked geometry; visual/DPI acceptance is not claimed.
- A separate test server and its acceptance gate were explicitly waived by the owner. Real-game mod/sleep/restore behavior is not newly certified by fixture tests.
- CI on the deployed source passed. Future restarts, server changes or migrations require separate authorization.

The specifically authorized launcher replacement and one server restart are complete. No further server action, merge or public release is authorized. See [reconciliation evidence](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/ui-navigation-polish-20261019/docs/SHROUDFORGE-CANDIDATE-RECONCILIATION.md), [handoff](SESSION-HANDOFF.md), [issue register](ISSUE-REGISTER.md) and [work log](WORK-LOG.md).
