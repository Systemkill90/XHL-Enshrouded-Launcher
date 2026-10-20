# ShroudForge — verified development and decision chronology

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
