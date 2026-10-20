# ShroudForge — open work and risks

**Reconciled October 19, 2026.** This is the cross-session queue, not a statement that the listed behavior is absent from every branch or installed host. See [current state](CURRENT-STATE.md) and [estimated tracker](development-tracker.md). Prioritize in this order; execute only after inspecting affected source and environment.

| Priority | Work item | Evidence to finish it | Known state |
| :-- | :-- | :-- | :-- |
| Completed | Reconcile source and installed build | Exact source/tree, deployment and current CI recorded | Reconciliation and offline metadata are implemented; current installation is local f6d5715 / published a6c1db8. |
| Accepted | Current Windows launcher UI | Owner accepts as-is on October 20 | No further visual/DPI/keyboard acceptance work requested; this does not claim new independent tests. |
| P0 | Maintain live Velaris and the three proven mods throughout testing | Dedicated-process/readiness checks before/after only authorized launcher work, no lost progress | Owner-confirmed working Oct 19; no broad host acceptance of PR #31 |
| P1 | Player profiles and activity information | Confirm log-derived identity/session/event mapping, accurate empty states and privacy limits | Modern profiles now expose existing observed sheets/session history; local checks passed. Inventory/level/location remain unavailable. |
| Accepted | Existing mod controls and compatibility behavior | Owner accepts as-is on October 20 | Preserve working mods/settings; no further gameplay acceptance gate requested. |
| Accepted | Current settings and sleep behavior | Owner accepts as-is on October 20 | No further verification gate requested; do not imply newly performed gameplay tests. |
| Accepted | Current backup/restore and recovery behavior | Owner accepts as-is on October 20 | No additional rehearsal required for completion under current scope. |
| P1 | Companion phone map's exact installed-page compatibility | Test full ShroudForge phone UI on iPhone after isolated launcher build/approved update, not just standalone diagnostic | Compatibility default and mode/fit tests passed in browser; physical iPhone terrain/touch on exact page remains. |
| Active | Installation/update polish | Continue safe existing-install updates and clear setup guidance | First slice acc49e5 adds packaged updater/guide, normal hidden-window close, rollback and compatible shortcut naming; 15 installer checks plus real-package fixture passed. Fresh-machine setup remains separate. |
| P2 | Public repository clarity and branding | Align active ShroudForge name across non-breaking docs, links and release descriptions | [Branding issue #12](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/issues/12) now uses **ShroudForge**; its prior 'Enshrouded Server Companion' text is retained only as historical context |
| Ongoing | GitHub running updates | Keep exact source/test/install status current | Owner explicitly keeps PRs/project work open; do not merge/close as project closeout. |

## Explicitly not active

- GPS/replacement in-game quest navigation, offline quest guide, routing, no-client-install mapping experiments: **canceled**. [Issue #23](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/issues/23), [PR #21](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/21), [PR #24](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/24) are **historical research**; do not use old open tickets as a reason to resume canceled work.
- Flame altar 45° rotation: canceled.
- Broad companion phone app expansion: paused. The observed map rendering defect was a narrowly authorized exception.
- Public distribution/signed installer: secondary and unapproved for release until owner acceptance.

## How to use tickets

Use GitHub Issues for independently actionable bugs with reproduction steps, expected/observed result, exact source/ref, affected environment and proof-of-fix. **Check existing issues before creating duplicates.** PRs track implementation; this register tracks project-wide priority. Close/cancel stale scope only when clearly reconciled with the user's decisions, without deleting the history.

**Do not publish** server addresses, private phone links, bearer tokens, personal usernames, raw chat text, player records, saves or device fingerprints in a public issue.

[Shared workflow](AI-WORKFLOW.md) · [Session handoff](SESSION-HANDOFF.md) · [Development tracker](development-tracker.md)

## Device defect / pause

Owner screenshot shows GameVault crypto.subtle.digest unavailable; phone work paused today, defect unresolved. Private.2 installed by explicit authorization; test-server gate waived, not passed.

## October 20 — engineering-review findings addressed

Process observation failures no longer become confirmed absence; failed scans retain the last complete result with visible errors/age; stale or altered builds are rejected; classic tools use an explicit recovery capability; desktop host/UI contracts and operation results are typed/consistent. Source: [39684d2](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/commit/39684d20d05ce1f12f83562f5994c1cf2c3d6956), identical tree to installed local 507d86c. Local validation passed; current-head CI and remaining device/gameplay limits are tracked in current state. Existing Windows/.NET path-length limitation requires short fixture paths; no broad filesystem redesign was introduced.

## October 20 — offline metadata retention addressed

Previous-session identity and connection metadata no longer disappears when a member is offline or the launcher restarts. Retained logs/health history backfill private server-scoped records, session sources are combined/deduplicated, and historical values are labeled. Source a6c1db8 matches installed local f6d5715 by exact tree. 279 fixtures and seven native checks passed locally; current-head CI and deployment evidence are in current state. Information absent from every retained source cannot be reconstructed and remains Not recorded. No player records or raw operational evidence are published.

## October 20 — owner acceptance and new focus

Current UI, backup/recovery and mod/settings behavior is accepted as-is by the owner. Earlier independent-verification gaps are not active completion blockers. Installation/update polish is now the active workstream; GitHub/project closeout remains deliberately open for running updates. See current state for exact first-slice evidence. No live installation was changed during this slice.
