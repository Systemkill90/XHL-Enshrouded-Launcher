# What's ready, and what's still being built?

**Latest verified update — October 20:** Close choices, create/scan server setup, responsive overview with an enforced DPI/zoom-aware minimum, unobstructed View players, and correct decimal MB displays are installed. Source **2a83af6** / local **2432857**, identical tree **d1689eda00df06591378408885c9da59de5109cb**. Current-head CI #20/#155 passed. One explicitly authorized server restart was completed and must not be repeated automatically. Existing UI/recovery/mod behavior stays accepted as-is; installation polish remains in progress and PR #31 stays open. See [current state](CURRENT-STATE.md) for evidence and unverified limits.

**Status consolidated: October 19, 2026.**

ShroudForge distinguishes **code written**, **tests passing**, **working on the live server**, and **publicly released**. They are not the same thing.

> **October 19 planning update:** The owner reported the server and its three existing mods working and confirmed server-launch detection. The UI redesign is now the primary task. GPS navigation and altar rotation were canceled; the companion phone app is paused. These planning updates do not replace a source-code or host audit.

## Shared project memory

Start a new development session with **[Current state](CURRENT-STATE.md) → [Session handoff](SESSION-HANDOFF.md)**. Those files are the GitHub handoff shared by phone ChatGPT, desktop Work and Codex. They do **not** automatically import chats or replace inspection of the owner's desktop.

| Keep in sync | Reference |
| :-- | :-- |
| Current evidence, versions and staged builds | [Current state](CURRENT-STATE.md) |
| Exact next task and validation gates | [Session handoff](SESSION-HANDOFF.md) |
| Owner decisions, cancellations and protection rules | [Decision log](DECISIONS.md) |
| Dated development, builds and owner tests | [Work log](WORK-LOG.md) |
| Remaining work and known problems | [Issue register](ISSUE-REGISTER.md) |
| How to share work across ChatGPT / Work / Codex | [AI workflow](AI-WORKFLOW.md) |
| Started/last-active dates and provisional estimates | [Development tracker](development-tracker.md) |

**GitHub development status:** The consolidated dark-fantasy launcher UI, read-only player profiles, event feed and phone map Compatibility fix are staged in **[draft PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31)**. [Windows preview run #135](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39357907800) passed compilation, 172 fixtures and native tests for the reviewed source; neither deployment to the owner desktop nor production release follows from CI. The owner confirmed the embedded map works in standalone iPhone Safari Compatibility mode. Broader phone development remains paused.

**[Open the October 19 development tracker →](development-tracker.md)** — estimated percentages, discussion dates, active priorities, and verified vs. unverified claims.

**[Next development session →](NEXT-SESSION.md)** — precise launcher-UI focus, existing GitHub branches, tests and live-server safety gates.

| Area | Status |
| :-- | :-- |
| **Public installer** | **Not released** |
| **Existing Velaris server** | Working public installation reported; latest actual host files not inspected in this review |
| **ShroudForge 2.0** | Developed in the [2.0 branch](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/tree/feat/launcher-2-overhaul/launcher-2-preview), with [draft PR #18](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/18) |
| **Flight and existing mods** | Working modifications reported; new changes must be matched to exact build |
| **Player profiles** | Draft [PR #19](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/19); integrated read-only candidate staged in [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31) |
| **Quality-of-life controls** | Draft [PR #20](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/20); integrated read-only candidate staged in [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31) |
| **In-game quest-tracker proof (historical)** | Research [PR #24](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/24); GPS concept canceled from active scope |
| **Road and map studies (historical)** | Research [PR #21](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/21); not an active GPS deliverable |
| **Public-server release process** | Draft [PR #25](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/25) |
| **Second-screen quest guide** | Closed [PR #22](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/22), because it does not meet the desired in-game experience |

## Where is the source?

The `main` branch is the **welcoming project home and documentation**, not a copy of the running Velaris installation. The actual 2.0 code remains on development branches until validated and approved. Look in [Project areas](../areas/README.md) for direct links.

**Why not merge everything now?** Because doing so would make unreleased, partially verified work look like an approved public-server update.

[View current PRs](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pulls) · [Read the roadmap](../../ROADMAP.md) · [Back to docs](../README.md)
