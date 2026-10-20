# ShroudForge — open work and risks

**Reconciled October 19, 2026.** This is the cross-session queue, not a statement that the listed behavior is absent from every branch or installed host. See [current state](CURRENT-STATE.md) and [estimated tracker](development-tracker.md). Prioritize in this order; execute only after inspecting affected source and environment.

| Priority | Work item | Evidence to finish it | Known state |
| :-- | :-- | :-- | :-- |
| P0 | Reconcile actual desktop checkout and interrupted Work-session UI with [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31) | Check branch/commit, uncommitted code and installed version; compare without overwriting local files | **Unknown on host**; candidate 2.2 label not established |
| P0 | Verify refined launcher UI on Windows, narrow widths, DPI/keyboard and multi-page navigation | Screenshots/real UI acceptance of Dashboard, Players, Logs, Settings, World and Backups | Source and CI tests passed; visual validation still needed |
| P0 | Maintain live Velaris and the three proven mods throughout testing | Dedicated-process/readiness checks before/after only authorized launcher work, no lost progress | Owner-confirmed working Oct 19; no broad host acceptance of PR #31 |
| P1 | Player profiles and activity information | Confirm log-derived identity/session/event mapping, accurate empty states and privacy limits | Read-only staged implementation from #19/#20; inventory/level/location **not available** without proven source |
| P1 | Mod controls, flight pitch-hold/stamina and game-version compatibility | Disposable-game tests and real observed mod behavior, preserve working installed binaries | Mixed historical testing/requests; don't assume current controls are fixed |
| P1 | Settings and one-player / 60× sleep behavior | Exact game-build validation and safe test of config behavior | Editor code exists; sleep behavior still an open game-compatibility question |
| P1 | Guarded backup/restore and crash recovery | Simulate and recover on disposable world, verify integrity and rollback, no blind restarts | Management UI and fixtures exist; release acceptance incomplete |
| P1 | Companion phone map's exact installed-page compatibility | Test full ShroudForge phone UI on iPhone after isolated launcher build/approved update, not just standalone diagnostic | Compatibility iframe **owner-verified**; default fix staged and CI passed, installation unverified |
| P2 | Release packaging, dependency and installation cleanup | Safe detection of existing directories, human-readable folders, copy/rollback strategy, signed or clearly labeled preview artifact | [Release issue #10](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/issues/10); no production-ready installer |
| P2 | Public repository clarity and branding | Align active ShroudForge name across non-breaking docs, links and release descriptions | [Branding issue #12](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/issues/12) contains **outdated proposed name**: use ShroudForge, not its earlier 'Enshrouded Server Companion' suggestion |
| P2 | Prevent stale docs or status inflation across sessions | Update GitHub handoff/status with exact branch, SHA, tests and device evidence | Repository memory consolidation created; ongoing practice |

## Explicitly not active

- GPS/replacement in-game quest navigation, offline quest guide, routing, no-client-install mapping experiments: **canceled**. [Issue #23](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/issues/23), [PR #21](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/21), [PR #24](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/24) are **historical research**; do not use old open tickets as a reason to resume canceled work.
- Flame altar 45° rotation: canceled.
- Broad companion phone app expansion: paused. The observed map rendering defect was a narrowly authorized exception.
- Public distribution/signed installer: secondary and unapproved for release until owner acceptance.

## How to use tickets

Use GitHub Issues for independently actionable bugs with reproduction steps, expected/observed result, exact source/ref, affected environment and proof-of-fix. **Check existing issues before creating duplicates.** PRs track implementation; this register tracks project-wide priority. Close/cancel stale scope only when clearly reconciled with the user's decisions, without deleting the history.

**Do not publish** server addresses, private phone links, bearer tokens, personal usernames, raw chat text, player records, saves or device fingerprints in a public issue.

[Shared workflow](AI-WORKFLOW.md) · [Session handoff](SESSION-HANDOFF.md) · [Development tracker](development-tracker.md)
