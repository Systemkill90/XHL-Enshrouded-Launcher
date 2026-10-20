# ShroudForge — current session handoff

## Offline player/session metadata — October 20, 2026

**Implemented and installed.** Offline member profiles now recover previously recorded Steam identity, peer/handle/machine and character-save transfer size, last completed visit/duration, and historical ping/connection measurements. Existing server logs, session CSVs and the private health-history CSV provide the backfill. Missing fields are filled from prior observations when available; unsupported or never-recorded values remain explicitly unrecorded.

A private, server-scoped metadata cache survives launcher restarts and log rotation. Legacy and diagnostic session files are combined with copied visits deduplicated. Historical measurements are labeled with source/date and never represent a current connection, active visit or online member. Fresh live observations win. Conflicting known Steam IDs do not inherit each other's metadata. Read-only previews do not persist the cache; no game/save data is modified. Initial retained-log work is bounded to the latest 20 logs, 4 MiB each.

**Source:** [`a6c1db8`](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/commit/a6c1db84690798a4412867ac5be7ad05af89dc31) on `dev/offline-player-history` and `feat/ui-navigation-polish-20261019`, in [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31). Installed local commit is `f6d57159fa093b5a845f7ec77edb030f84df275c`; both have verified identical tree `a0b0ce2be8e29bc98ae912179030e8dc13731a25`. Connected-app publication preserved existing remote ancestry; do not force-push local history over it.

**Validation:** modern/classic x64 Windows builds, strict TypeScript, bundled UI, exact phone synchronization, **279 fixtures** (55 desktop, 160 core, 24 resilience, 14 fields, 26 mods), seven native dummy-server checks and four provenance cases passed. Focused checks cover archive reconstruction, health import, persistence/reload after log deletion, full offline snapshot projection, per-server isolation, identity conflicts, live precedence, read-only nonpersistence and duplicate visits.

**Deployment:** completed under the owner's continuing related-work approval. Installed package hashes, query readiness, Running observation and error-free scan verified; retained metadata import succeeded. The running game-server process/start time, checked server files and launcher settings remained unchanged. No game-server restart was requested. Rollback retained privately. The installer initially could not close the hidden launcher and replaced no files; the verified launcher then closed through its normal window-close handler, the package was installed and the interactive launcher reopened. No force-kill was used.

**Current-head CI:** modern Windows [run #8](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39413912020) and historical Windows [run #143](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39413912123) both passed on this exact head, including fresh dependency restore, compilation, checks, packaging and artifact upload.

**Limits:** recoverable fields depend on retained evidence. Archived log-write time is labeled as evidence time when an exact sample timestamp is unavailable. Session totals are matched by display name and can be affected by renames/reused names. Inventory, level, equipment and position remain unsupported. Native visual/DPI/keyboard review, actual iPhone behavior and real-game recovery/mod/restore acceptance remain unverified. The known GameVault crypto.subtle.digest defect is unresolved; phone expansion stays paused and GPS/altar rotation stay canceled.

[Offline history details and changelog](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/ui-navigation-polish-20261019/desktop/react-launcher/PLAYER-HISTORY.md).

## Next smallest action

1. Both current-head CI runs passed. Retain the exact SHA and run links; rerun only for relevant source changes.
2. Use the published remote head as the base for future changes. The clean local candidate and remote branch have identical trees but different commit ancestry.
3. Offline history is now deployed. Preserve the private metadata cache and rollback. Never upload player identities, raw health records, logs, private paths or keys to public GitHub.
4. Continue only scoped follow-up work. Device/gameplay limitations above remain; no new game-server restart or restore was performed.

Application source remains on development branches under desktop/. Main is the shared documentation record; PR #31 remains open/draft.
