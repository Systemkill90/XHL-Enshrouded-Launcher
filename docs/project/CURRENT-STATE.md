# ShroudForge — current project state

**Snapshot:** October 19, 2026 (America/New_York)  
**Purpose:** authoritative, shareable **public development summary**, **not** a representation of the owner's local desktop or live server.
**Start here for every ChatGPT, Work or Codex development session.** Read [AGENTS.md](../../AGENTS.md), this page, then [session handoff](SESSION-HANDOFF.md).

## State of record

| Item | Evidence and status |
| :-- | :-- |
| Existing dedicated server | **Owner-confirmed operating October 19**, with the three existing gameplay mods working. This confirmation applies to the owner's existing installation only, not the GitHub development build. |
| Owner's desktop / installed launcher | **Current installed commit and version not independently verified** by this GitHub review; no assumption that staging code was applied. |
| Public documentation | `main` is the readable home and project memory; **not a live installation or a release branch**. |
| Existing launcher baseline | [`feat/launcher-2-overhaul`](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/tree/feat/launcher-2-overhaul/launcher-2-preview), [draft PR #18](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/18). |
| Consolidated UI candidate | [`feat/ui-navigation-polish-20261019`](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/tree/feat/ui-navigation-polish-20261019/launcher-2-preview), [draft PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31), targeting the 2.0 development branch, **not merged**. |
| Candidate version | **2.2 is an owner-recalled target/candidate**, not a verified installed binary version. GitHub development sources still carry 2.0 naming. Reconcile actual host files and version metadata before declaring a release. |
| Latest reviewed staged Windows CI | [Run #135](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39357907800) on reviewed PR #31 commit `a5fcf210054dfea7d09ad4ae5c8f6df0cab2ec36`: Windows compile, **172 fixture checks**, native fixture checks and unsigned preview-artifact upload all passed. If the PR moves, recheck the new head's results. |
| Public Windows installer | **Not released.** CI artifact is temporary, unsigned and unsuitable for blind replacement of the working installation. |
| Owner-side visual and device verification | **Incomplete** for the consolidated desktop UI, installed phone page, three-mod/actual-game compatibility, and a fully isolated deployment rehearsal. |

**Temporary unsigned artifact from the reviewed build:** [preview run #135 artifact](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39357907800/artifacts/12236151989); executable SHA256 `984fc854812767aa571e5e00db067fc8c10210763a804540b52fa628d99ed8d7`. Artifact availability is time-limited. Neither a passing CI run nor a download proves a live install.

## Current scope and evidence

### 1. Launcher & Interface

**First priority.** Dark-fantasy UI, splash, dashboard and navigation source exist on the launcher development branch. PR #31 stages the scrollable keyboard-accessible navigation, observable recent activity, readiness/backup indicators and routes to other panels. Read-only player profiles and parsed/raw event views were brought into this staged candidate from earlier draft PRs #19 and #20. The Windows fixture suite passed; desktop visual/DPI/accessibility checks and comparison with any interrupted local Work-session changes remain open. **Profiles do not imply access to player inventory, live position, arbitrary player save files or unsupported character fields.**

### 2. Server, Mods & Data Management

The owner has confirmed the *existing* live server and three mods operating. The development source includes settings, mod management, monitoring, backups, server controls and read-only player event information, but each action requires dedicated validation against a disposable server/build before production claims. Flight pitch-hold, stamina controls, one-player sleep and 60× sleep speed remain tracked requests or validations; do not infer they are fixed from launcher CI. Do not alter the working injector/mod chain as a side effect of UI work.

### 3. Installation, Reliability & GitHub

The documentation landing page is on `main`; active 2.0 and the 2.2 *candidate* remain in draft PRs. Windows CI verifies source compilation and fixture/native tests only. Desktop source reconciliation, executable compatibility, backup/restore recovery rehearsals, predictable installation paths, packaging, launch shortcut and release acceptance remain to be verified.

## Phone companion — narrow exception to pause

Broader phone-app development is **paused**, but the owner authorized a specific map defect correction on October 19. The owner **visually confirmed** the real external GameVault Embervale map rendered inside an iPhone Safari diagnostic iframe in **Compatibility** mode (without the restrictive iframe `sandbox`); the protected iframe showed white. [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31) now stages Compatibility as the **default**, retaining an explicit Protected option, no-referrer policy, click-to-load behavior, troubleshooting and the external map link. The staging sources and CI are verified; the owner has **not confirmed that the updated phone UI was installed on the actual desktop launcher**. The standalone test is not an authenticated server application.

## Explicitly out of scope

- GPS, route guidance, ShroudNav/replacement quest-tracking and autopilot: **canceled** from active work October 19; old [map research PR #21](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/21) and [quest research PR #24](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/24) are archives, not approved features.
- Flame altar 45° rotation: **canceled**.
- A public signed release, in-game inventory editing or exact live-player coordinates: **not delivered/verified**.
- New gameplay mods, replacement loaders, or production server changes: require a separate explicit authorization and an isolated proof.

## Evidence rules

**Owner observed** = reported from actual operation; **Implemented/staged** = code present on a branch; **CI passed** = only specified tests passed; **Host verified** = a recorded check of the actual Windows deployment; **Released** = an explicitly accepted tagged artifact. Never combine these into one completion status.

The numerical progress baseline and dates are at [development tracker](development-tracker.md). Historical estimates are **not** recomputed merely because source or documentation changed.

[Decision log](DECISIONS.md) · [Work log](WORK-LOG.md) · [Issue register](ISSUE-REGISTER.md) · [Shared workflow](AI-WORKFLOW.md)
