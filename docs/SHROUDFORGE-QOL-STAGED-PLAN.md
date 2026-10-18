# ShroudForge 2.0 — Owner-first quality-of-life program
Date: 2026-10-17

## Non-negotiable deployment boundary
- The running Velaris server, existing launcher executable, startup scripts, mod controllers, configuration and world saves **must not be touched by development**.
- Keep this work on `feature/v2-qol-safety-pass-1`, based on `feat/launcher-2-overhaul`. Public `main` remains unchanged.
- Use a **draft pull request** and disposable Windows fixture directories. Do not merge, publish, remotely connect to the host, start/stop/restart, update SteamCMD, modify mods, or activate an installation during this pass.
- Before *any* later deployment: obtain a known-good build hash, backed-up configuration, a stopped-server verified snapshot, a tested rollback procedure, passing isolated Windows CI, and explicit owner approval for the maintenance window.
- Never assume A2S query failure means zero players; never schedule a restart from stale/unknown roster data. Unknown game builds fail closed.
- Preserve the owner's approved UI, working flight pitch hold, server settings editor, current backup behavior and known-good startup process.

## Staged delivery with measurable acceptance gates

### Stage 0 — Baseline and safety contract (this PR)
- [x] Inventory existing code and pending branches. Player profile work is separately in draft PR #19; flight fixes are separately in draft PR #17.
- [x] Write the safety and rollout plan. Work only in GitHub source/CI.
- [ ] Verify that a disposable Windows preview compiles and tests successfully.
- [ ] Record exact last-known-good desktop binary/package and hashes **when host access is explicitly granted**; not obtainable through this GitHub-only pass.

### Stage 1 — Read-only daily operations (start now)
- [ ] Replace guesswork with a filtered **parsed event view** alongside unchanged raw logs. Clear provenance; no invented gameplay actions.
- [ ] Add a one-click **privacy-safe troubleshooting summary**, built from explicitly whitelisted health/installation/backup fields, copied to the clipboard; no paths, IP addresses, tokens, passwords, player identities or raw logs.
- [ ] Add a compact **health/readiness digest** based on observed process, backup completion-marker presence, pinned-build checks and disk free space. It must distinguish unknown from healthy.
- [ ] Add focused unit/fixture tests for filtering, redaction and unknown/stale status.
- Gate: pure read-only functionality; no server operations or file modifications during view/report.

### Stage 2 — Safe notifications, then controlled restart
- [ ] Opt-in tray alerts for crash, low storage, backup failure, unusual connection health and players joining. Deduplicate; backoff; no repeated noisy success messages.
- [ ] Add guarded *manual* restart assistant: verified offline backup requirement, compatibility checks, known roster result, warning if players are connected, stop/start confirmation and observed recovery.
- [ ] Consider restart-when-empty **only after** independent tests prove unknown/stale player status blocks any action. Default OFF.
- Gate: simulated server only; success and failure paths tested, rollback/retry limits established. No involuntary changes to live server.

### Stage 3 — History and backup lifecycle
- [ ] Persist bounded server/process/player history with explicit sample times, gap markers and retention bounds.
- [ ] Improve player dossiers after PR #19 is verified, including activity/navigation and read-only session graphs.
- [ ] Add backup retention **preview first**: protected backups, disk usage and eligible candidates. Never auto-delete or assume live copies are coherent.
- Gate: migration/versioned storage tests; old backups remain intact.

### Stage 4 — Updates, Windows workflow, phone
- [ ] Update comparison with exact game build, three-mod validation, install target and manual rollback evidence; SteamCMD download stays staged.
- [ ] Optional start-at-sign-in, minimize-to-tray and page restoration; none implicitly starts the server.
- [ ] Extend authenticated private-LAN phone view with read-only alerts, player profiles and backup freshness. No automatic public exposure or port forwarding.
- Gate: host firewall/phone review, staged-update tests, authentication and permissions verification.

### Stage 5 — Integrate and release deliberately
- [ ] Review PR #19 independently before combining with Stage 1.
- [ ] Windows compile + fixture/UI/native checks + artifact checksum + manual app review.
- [ ] Explicit desktop smoke test with a disposable server clone before a controlled Velaris maintenance window.
- [ ] Prepare an unsigned owner preview package; signing/public release are separate decisions.
- [ ] Record exact changes, backup/restore evidence, rollback instructions and known unsupported data.

## Feature ownership map
| # | Improvement | Stage |
|---|---|---|
| 1 | Smart notifications | 2 |
| 2 | Intelligent restart | 2 |
| 3 | Human-readable event console | 1 |
| 4 | Server health diagnostics | 1 |
| 5 | Historical graphs | 3 |
| 6 | Update comparison and staging UX | 4 |
| 7 | Backup retention management | 3 |
| 8 | One-click redacted support summary | 1 |
| 9 | Windows startup/tray workflow | 4 |
| 10 | Phone companion refinements | 4 |

### Evidence discipline
A source-code commit is not a deployed feature. Passing fixtures does not prove gameplay compatibility. An archive with a `VERIFIED.txt` marker indicates a recorded completion marker, **not fresh re-verification of every hash**. A locally generated report is not an independent health assessment. Report what was observed, when it was observed and what remains unknown.
