# ShroudForge — repository development instructions

This repository contains the public-facing ShroudForge documentation and **development** branches for a Windows Enshrouded dedicated-server launcher. The main branch is not an installed copy of the live Velaris server.

## Start here

- Read [project status](docs/project/README.md) for the distinction between written code, passed tests, live operation, and release.
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
