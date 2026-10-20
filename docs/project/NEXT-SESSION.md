# Next development session — ShroudForge 2.0

**Latest verified update — October 20:** Close choices, create/scan server setup, responsive overview with an enforced DPI/zoom-aware minimum, unobstructed View players, and correct decimal MB displays are installed. Source **75952a5** / local **5f35199**, identical tree **9e1cf8c6081b5a5d41c6e6b1a0e982e4fa9f346c**. Current-head CI #18/#153 passed. One explicitly authorized server restart was completed and must not be repeated automatically. Existing UI/recovery/mod behavior stays accepted as-is; installation polish remains in progress and PR #31 stays open. See [current state](CURRENT-STATE.md) for evidence and unverified limits.

> [!IMPORTANT]
> **Superseded as the primary handoff by [SESSION-HANDOFF.md](SESSION-HANDOFF.md) and [CURRENT-STATE.md](CURRENT-STATE.md).** This page is retained as the October 19 *original UI-session plan*. Since then, [draft PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31) integrated the earlier player-profile and event-feed source, and the owner's iPhone map Compatibility result was recorded. Follow the newer documents and current GitHub HEAD, not the original branch-only instructions below.

**Prepared October 19, 2026.** Intended to provide a low-overhead restart point for a new Work/Codex session. This is a task plan, not a claim that the latest development code is on the owner's Windows machine.

## Reliable reference points

- **Public repo:** https://github.com/Systemkill90/XHL-Enshrouded-Launcher
- **Default branch:** `main` is for public project navigation and planning documentation; it is **not** the live server.
- **Active launcher source:** `feat/launcher-2-overhaul`, directory `launcher-2-preview/`; existing [draft PR #18](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/18).
- **Project baseline:** [October 19 tracker](development-tracker.md). Percentages are historical estimates; they are **not** test results.
- **Development conventions:** [main-branch AGENTS.md](../../AGENTS.md). A branch-specific `AGENTS.md` also exists on the active launcher development branch.
- **Host:** the owner reported the original server and its three mods working October 19. Running a process is not the same as proving a particular GitHub commit is installed.
- **Build report:** The existing launcher branch contains [OVERHAUL.md](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/launcher-2-overhaul/launcher-2-preview/OVERHAUL.md) and [VERIFICATION.md](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/launcher-2-overhaul/launcher-2-preview/VERIFICATION.md). Validate their claims against the actual code and current tests.
- **Other review work:** [Player profiles #19](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/19), [read-only QoL #20](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/20), [release safety #25](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/25). These are not automatically approved for merging or deployment.

## One next-work objective

**Make a reviewable, testable pass on the desktop launcher UI.** Start with the existing dark-fantasy dashboard, navigation and setting screens. Do not rebuild working architecture from scratch and do not spend the session on GPS/phone features.

### Suggested execution order

1. **Orient (read-only):** identify the current checkout path, branch and commit; check for uncommitted edits and compare with remote before pulling or overwriting anything. Read the relevant UI files in `launcher-2-preview/src/` (especially `ControlRoom.cs`, `ModernUI.cs`, `Theme.cs`, `ConfigurationEditor.cs`, `Splash.cs`) and existing UI tests. Avoid unnecessary full-repository sweeps.
2. **Audit current behavior:** separate working UI code and tests from unverified feature estimates. Record specific UI problems (layout density, spacing/typography, navigation consistency, clipped controls, slow or ambiguous interactions).
3. **Implement one cohesive UI pass:** keep the established design and preserved functionality; favor reversible component-scoped edits. No changes to game patches, world saves, remote commands, or the active server.
4. **Validate safely:** execute isolated build/fixture/UI tests for the modified source as available and review the result. Record tests skipped, failures, warnings and whether a Windows application was actually visually inspected. CI success alone is not proof the installed owner build changed.
5. **Report precisely:** provide the branch and commit SHA, changed files, tests with outcomes, remaining visual issues, and whether the owner needs to perform a manual desktop check. Update the planning tracker only where evidence warrants.

## Hard stops and boundaries

- **Never** stop, restart, update, relaunch, patch, reconfigure or replace the production Velaris installation, its processes, mods or world files without a new explicit go-ahead.
- Do not clean or migrate live folders, especially when the operator is not at the desktop to validate expected paths and backups.
- Do not merge experimental feature PRs into a release or advertise a working installer on the strength of CI alone.
- On canceled/paused features, **do not resume GPS, in-game quest-navigation replacement, flame altar rotation or companion-phone work** unless the owner deliberately changes direction.
- Do not conflate GitHub branch state, the local desktop checkout and the running server; identify each separately before deciding if synchronization is necessary.

## Efficient Work request

> Continue ShroudForge 2.0 from GitHub branch `feat/launcher-2-overhaul`. Follow that branch's `AGENTS.md` and the linked October 19 tracker. Prioritize one cohesive, reversible desktop launcher UI polish pass: dashboard, navigation, settings and consistent dark-fantasy styling. First inspect the actual checkout state; preserve local edits and avoid unrelated file sweeps. Work only on an isolated development copy and run relevant UI/fixture tests. Do not touch or restart live Velaris, its mods or saves, and do not resume canceled or paused features. Finish with an exact changed-file/commit/test report and a short list of any owner validation needed.

## Planning/implementation boundary

GitHub is the technical repository and this file is the latest written handoff. ChatGPT Projects preserve context, but they do **not** synchronize themselves to GitHub or a Windows checkout. Keep the three in agreement through deliberate reads, reviewable commits and verified updates.
