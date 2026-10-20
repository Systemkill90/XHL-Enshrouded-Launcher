# ShroudForge — current verified development state

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
