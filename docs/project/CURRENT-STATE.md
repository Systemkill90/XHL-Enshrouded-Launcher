# ShroudForge — current verified development state

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
