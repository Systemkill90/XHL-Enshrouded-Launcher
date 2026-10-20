# ShroudForge — repository development instructions

This repository contains the public-facing ShroudForge documentation and **development** branches for a Windows Enshrouded dedicated-server launcher. The main branch is not an installed copy of the live Velaris server.

## One shared project record — mandatory first read

The **public GitHub repository is the canonical shared *development* record**, not the user's private ChatGPT conversation store, local desktop checkout or live Enshrouded installation. For every task, first read:

1. [Current verified/reported state](docs/project/CURRENT-STATE.md) — code branches, tested source, version uncertainty and server boundaries.
2. [Current handoff](docs/project/SESSION-HANDOFF.md) — precise next safe task, latest source refs and missing host validation.
3. [Decision log](docs/project/DECISIONS.md) — irreversible scope changes, paused and canceled features.
4. [AI work protocol](docs/project/AI-WORKFLOW.md) — how ChatGPT, desktop Work and Codex synchronize their *work records* through GitHub.
5. [Work chronology](docs/project/WORK-LOG.md) and [issue register](docs/project/ISSUE-REGISTER.md) only when history or prioritization is needed.

**At the end of an authorized development session:** commit code only after relevant validation, then record precise SHA/PR/tests in the work log and update current state and handoff when necessary. Mark **staged source**, **CI tested**, **device tested**, **host installed**, and **live released** separately. Never claim source was uploaded or desktop work was executed without verification.

**Latest deployment note:** October 20 selective refactor is published at 39684d2 (tree-identical to installed local 507d86c). Launcher-only replacement completed with installed hashes, query readiness, unchanged server process/files/settings and rollback verified; zero server restarts for this update. Local builds and 264 fixtures plus seven native checks passed; current-head CI is tracked in current state. Existing phone defect remains unresolved. The owner authorized this deployment and related GitHub updates; use the current task's scope for subsequent operations.

## Start here

- Read [current state](docs/project/CURRENT-STATE.md) and [project status](docs/project/README.md) for the distinction between written code, passed tests, live operation, and release.
- Read the [October 19 development tracker](docs/project/development-tracker.md) for dated *provisional* estimates and current priorities.
- Read [roadmap](ROADMAP.md) and [server safety](docs/operate/README.md).
- The current launcher implementation is under feat/launcher-2-overhaul, not necessarily main. Verify branch, file versions, and installed host state before making claims.

## Engineering rules

1. **Protect the running server and players.** Never restart, shut down, deploy to, overwrite, migrate, or otherwise alter the running installation or saves without explicit user authorization.
2. The owner prioritizes personal usability and preservation of the existing working server and mods over general-purpose public distribution.
3. Inspect only relevant existing code first; favor localized, reversible changes and retain working architecture.
4. Use source control and a reviewable diff; keep experimental code off approved release paths.
5. Never claim a feature is working, deployed, synchronized, or tested based only on a discussion, file existence, PR, or green CI workflow.
6. Label estimates separately from verified test results. Do not inflate completion percentages without evidence.
7. Keep names, directories, instructions and documentation understandable, while preserving legacy paths necessary for runtime or build compatibility.
8. Work efficiently: avoid redundant file sweeps and repeated builds without a reason, but do not trade away tests or safety for speed.
9. When implementation changes materially affect scope or feature progress, propose updating docs/project/development-tracker.md, ROADMAP.md, and relevant user documentation.
10. Never commit live save files, passwords, secrets, authentication tokens, or proprietary game files.

## Current scope (October 19, 2026)

- **Top priority:** complete the dark-fantasy launcher UI, dashboard, navigation and usability improvements.
- Then: player activity displays, mod and server configuration controls, backups, reliability, installation experience and verifiable release processes.
- **Canceled:** GPS / replacement quest navigation; flame altar rotation.
- **Paused:** phone companion app.
- Historical GPS/map/quest research PRs do not mean those features are on the active roadmap.
- Owner reported three existing mods and the live server working on October 19; this is not a blanket verification of draft code.

These instructions align with the ChatGPT Project direction as of October 19. The ChatGPT Project itself does **not** auto-sync with this repository; apply future changes in GitHub deliberately.
