# Desktop handoff — Sunday, October 18, 2026

Purpose: inspect Velaris's actual current installation, compare it with the GitHub branches and prepare **only explicitly approved** public-server changes. This document stages work; it does **not** schedule an unattended job.

## Desktop access and first 10 minutes

1. Owner connects/authorizes their host desktop session. **Observe before acting.** Do not run setup, SteamCMD, restart, stop or update the game.
2. Identify the running dedicated-server process, launcher/manager process, game build and installed mod-loader variants **read-only**. Note uptime and any current player presence. Never log in-game player identities or private IPs to public GitHub.
3. Locate the actual host install, existing launcher source/output, mod directories, current load order, config path, save root and backup location. Do not assume the GitHub folder layout matches the host layout.
4. Compute local file SHA-256 hashes or run `ops/Read-Only-Host-Preflight.ps1` only after the operator has reviewed its scope. That script intentionally avoids file copies, network actions, save reading and process mutation.
5. Compare inventory against the known-working baseline. If unknown DLL loaders coexist, report only; **do not isolate, remove or replace on a live process.**
6. Check the existing world backup age and whether the last backup was captured while stopped. Create no backups until explicitly approved.
7. Review the change register and choose **at most one coherent candidate batch** after discussing any multiplayer downtime and rollback risk.
8. Obtain specific approval for installation, maintenance restart and/or config/saves work. No blanket approval from "today" or a GitHub green check.

## Candidate review order — not a deployment queue

| Topic | GitHub reference | Status ahead of host inspection | Allowed tomorrow without owner approval |
|---|---|---|---|
| Existing Velaris working game, 3 mods, pitch hold | **Host only**; compare original game build | Observed baseline; authoritative source is host | Read-only observation |
| ShroudForge 2.0 manager overhaul | PR #18 | Draft source; does not prove host installed version | Source review only |
| Player profile sheets | PR #19 | Draft; no reliable client inventory access | Source review only |
| QoL event feed/readiness/safe support report | PR #20 | Draft; offline test evidence should be checked | Source review only |
| Glider pitch-hold change | PR #17 | Separate draft; actual on-host state must be confirmed | Read-only status checks |
| Server-only roads/map GPS | PR #21 | Hold/research; in-game server-only HUD not demonstrated | No deployment |
| Browser quest guide | PR #22 | Closed, rejected deliverable | No deployment |
| Vanilla-client quest tracker text proof | PR #24 / Issue #23 | **Research**; one ID compiler CI only; game behavior unverified | Do not install/inject without fresh exact-build proof and explicit risk acceptance |
| Operating model and housekeeping | This PR | Docs/validation only | Review/merge source only with approval |

## Public-server candidate record (complete before any deployment)

- Exact candidate commit/version and linked PR:
- Supported game build / target resource hashes:
- Visible feature and player impact:
- Affected files and whether a restart is required:
- Enabled mod/loader dependencies and conflicts:
- Baseline running state / connected players known or unknown:
- Backup ID, time, stop-state and scope (world, config, mods):
- Exact restore/revert instructions:
- Evidence from offline tests/CI:
- Operator authorization and maintenance window:
- In-game acceptance tests:
- Rollback decision and final status:

If any of build compatibility, rollback or owner approval is missing: **NO GO**.

## Decision at end of desktop session

Record **which files actually match**, whether the current working launcher is ahead/behind GitHub, what has passed in real gameplay, what remains uncertain, and whether the owner authorized a particular build. Do not imply the newest PR is installed. Never auto-merge all open feature branches into one deployable archive.
