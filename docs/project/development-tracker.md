# ShroudForge 2.0 — Development tracker

**For latest source/test/install state:** see [CURRENT-STATE.md](CURRENT-STATE.md) and [SESSION-HANDOFF.md](SESSION-HANDOFF.md). This table intentionally preserves the owner/assistant **October 19 historical planning estimates**; its numeric percentages must not be silently recalculated from passing CI or newly written documentation. Read the [issue register](ISSUE-REGISTER.md) for priority and blockers.

**Recent checked evidence (as of October 19):** staged [draft PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31) contains combined launcher UI, player profiles and event-feed modules; [Windows run #135](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39357907800) completed compile, 172 fixtures and native tests. The owner visually confirmed the *standalone* GameVault iPhone iframe worked in Compatibility mode; the actual installed phone page/desktop version remain unverified. No live deployment is implied.

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
