# ShroudForge development roadmap

**Public-server-first, owner-approved release model.** This roadmap describes intentions, not installed features or a commitment to deploy them. See [change register](ops/change-register.json) for actual PR status and [live operation rules](docs/PUBLIC-SERVER-OPERATING-MODEL.md) for release gates.

## 0. Host baseline and controlled testing

- [x] Define public-server operating model and staged change register (source proposal).
- [x] Prepare Sunday Oct 18 desktop handoff and read-only inventory tooling (source proposal).
- [ ] Connect to owner desktop; inventory game/launcher build, loader variants, actual mod state and backup freshness, without touching running processes.
- [ ] Reconcile differences between source branches and the installed working manager.
- [ ] Identify safe maintenance/rollback procedures for a public server with connected players.

## 1. Launcher reliability and operator experience

- [ ] Review and test existing Launcher 2.0 draft (#18) against exact host baseline.
- [ ] Review player sheets (#19), with data provenance/privacy limitations.
- [ ] Compile and test QoL event/readiness/safe support report (#20).
- [ ] Verify startup, status, backups and guarded restore; keep automated recovery opt-in and player-aware.

## 2. Existing working mods

- [ ] Inventory flight, pitch hold, gifting, stacking and mod loader hashes/activation state.
- [ ] Review pitch-hold draft (#17) only if the installed change actually needs it.
- [ ] Investigate speed/responsiveness refinements offline; don't interfere with working controls.
- [ ] Run specifically approved multiplayer acceptance checks and record outcomes.

## 3. Advanced in-game quest tracking — feasibility only

- [x] Open exact-build localization proof tooling (#24) and reject browser replacement (#22).
- [ ] Obtain legal resource exports from the *actual* game version and verify a **single** server-origin change appears on an unmodified vanilla client.
- [ ] If proof fails, STOP server-only in-game quest tracker work rather than spending time on road digitizing.
- [ ] Only if proven: investigate supported objective markers, journal clues, quest chain data and player-safe progression.

## 4. Map/road research

- [ ] Keep road topology/calibration data in draft (#21); no verified routing, live player tracking or in-game HUD claimed.
- [ ] Resume only if a realistic, accepted output can reach players without violating no-client-install requirements.

## 5. Releases

- [ ] Preflight and record exact supported build/hash/version.
- [ ] Operator reviews risks and approves one candidate batch and maintenance window.
- [ ] Verify backups/restore, live gameplay and no world/quest regression.
- [ ] Publish approved release with checksums, supported-version matrix, change and rollback notes.

Never merge every open PR into a single automatic update.
