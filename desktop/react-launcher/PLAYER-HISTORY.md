# Offline player metadata

The modern launcher now fills offline member profiles from previously recorded
server metadata. Identity (Steam ID, peer, handle, machine and save-transfer size)
is backfilled from retained server logs. Historical ping, connection score, rolling
measurements and issue notes can be recovered from the existing player-health CSV.
Completed sessions are combined from both legacy and diagnostic CSV locations;
copied visits are deduplicated.

The player cards show the last completed session, last recorded ping/connection,
and last departure or labeled metadata evidence while the member is offline.
Historical measurements have a source/date label and never count as fresh network
samples, an active session, or proof that a player is online. Current observations
take precedence. Conflicting known Steam IDs do not inherit each other's data.
Names without a recorded value still say Not recorded; unsupported levels,
inventory, equipment and character positions are not invented.

Private metadata is stored atomically under Manager Data in a server-scoped
player-history JSON file with a backup. It survives launcher restarts and log
rotation, is never written into the game/save folders, and is not included in a
release package or public evidence. Read-only previews do not persist it. Initial
backfill is bounded to the newest 20 retained logs (4 MiB each); existing cache
preserves older records. Log write times are labeled as evidence dates when exact
sample times cannot be established. The legacy health CSV is imported only when
saved launcher settings bind it to the selected server.

Validation: modern/classic Windows builds, strict TypeScript, UI bundle, phone
synchronization, 55 desktop checks, 160 core checks, 24 resilience checks, 14 field
checks, 26 mod checks, seven native fixture checks and provenance checks. Focused
cases cover archived identity/network reconstruction, legacy health import,
offline snapshot projection, last completed session, cache reload after log
deletion, server isolation, Steam-ID conflicts, live precedence, read-only
nonpersistence, and duplicate session history.
