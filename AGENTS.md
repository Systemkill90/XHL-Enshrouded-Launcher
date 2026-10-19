# ShroudForge 2.0 — Development branch instructions

This branch contains **experimental / reviewable source** for the ShroudForge Windows launcher. It is not evidence that the same code is deployed to Velaris.

## Project references

- [GitHub main-branch project status](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/main/docs/project/README.md)
- [October 19 planning tracker](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/main/docs/project/development-tracker.md)
- [Current roadmap and canceled features](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/main/ROADMAP.md)
- [Live server safety](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/main/docs/operate/README.md)

Read the current branch source and actual test outputs before making progress claims. The tracker is a provisional planning estimate and does not certify this branch. ChatGPT Projects do not automatically synchronize files or decisions to GitHub.

## Mandatory safety

1. Do not start, stop, restart, deploy to, migrate, overwrite, clean, or patch the **live** server, mods, configurations, or player saves without explicit authorization. GitHub commits and CI are not deployment approvals.
2. Keep staged tests and builds separate from the running installation. Build/test scripts should be reviewed for side effects before execution.
3. Preserve all working server behavior and three reported working mods. Mod compatibility is exact-game-build-specific; fail closed for unknown builds.
4. Do not treat a branch, passing tests, or previously successful host check as current live installation verification.
5. Keep changes small, scoped, reviewable, reversible, and compatible with the existing installation.
6. Never commit secrets, server saves, proprietary game binaries, live logs, or private account details.

## Current development priority — October 19, 2026

- Finish the **minimal dark-fantasy desktop launcher UI**: dashboard layout, navigation, coherent screens, restrained animations, usability.
- Then validate player activity/profile displays, mod sliders, server settings, backups, console and recovery features against the **actual code and test evidence**.
- **Canceled:** in-game GPS / quest-navigation replacement; flame altar rotation.
- **Paused:** companion phone app and its embedded map. Preserve historical code but do not prioritize it.
- Working Velaris server and three mods were reported functional; do not replace them or mark new edits deployed.

## Efficient development sessions

Start with exact repository path, current branch/commit, and relevant files. Avoid whole-tree rereads and redundant builds; run focused tests then one validation build as appropriate. Give a short completion report containing changed files, commit hash, passed/failed tests, remaining risks, and what still needs host validation. Update the main-branch development tracker only after evidence is available and documentation changes have been reviewed.
