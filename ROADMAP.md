# XHL Roadmap

**Last reviewed:** 2026-10-17. Checkmarks represent confirmed *repository work*, not earlier chat claims or the state of a separate live installation.

## 0 — Recovered baseline and reproducibility
- [x] Recover and inventory v1.0.5 source-only and 2.0 foundation archives.
- [x] Document source provenance, build expectations and live-server precautions.
- [ ] Audit complete sources/assets for secrets, ownership, dependencies and safety before publication.
- [ ] Review the existing [2.0 source preview PR #1](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/1) and its verification claims; preserve it unmerged pending checks.
- [ ] Import v1.0.5 source, assets, scripts and manifest into a **separate legacy/reference path or branch** only if still needed; do not overwrite the newer 2.0 preview.
- [ ] Build on Windows and record compiler output; confirm clean launch without touching a live server.
- [ ] Compare the imported snapshot with the currently working local installation and preserve rollback copy.

## 1 — Launcher reliability
- [ ] Establish reliable startup, environment checks, logs, and SteamCMD update/stop/restart verification.
- [ ] First-run migration preview, existing directory adoption and recoverable rollback.
- [ ] Human-readable runtime and data directories, clear shortcut and animated startup.
- [ ] Document and test the behavior under Windows shutdown, interrupted updates, and crashes.

## 2 — Server management and telemetry
- [ ] Live console and structured events, with truthful attribution and time stamps.
- [ ] Player presence/session history, per-player status and available network measurements.
- [ ] Validate whether player locations or actions are exposed by supported game data before designing the UI.
- [ ] Server JSON configuration editor with schema validation and safe backups.
- [ ] Investigate sleeping/time-speed configuration changes in Enshrouded 1.0; do not assume older settings still work.

## 3 — Mods, characters, saves
- [ ] Audit existing flight, gifting and other mod integrations for compatibility with the current game build.
- [ ] Implement controlled flight on/off and stamina behavior **only where the underlying mod supports it**.
- [ ] Backups with verify, retention, restore staging, integrity and non-destructive recovery.
- [ ] Character/inventory views and guarded editing **only if verified data interfaces permit it**.

## 4 — UX and distribution
- [ ] Minimal dark-fantasy UI system, design reviews and accessible navigation.
- [ ] Windows smoke build and regression tests on every proposed change.
- [ ] Versioned signed or clearly identified unsigned binaries with checksums, documented requirements and rollback.
- [ ] Publish a release only after reproducible build plus documented validation.
