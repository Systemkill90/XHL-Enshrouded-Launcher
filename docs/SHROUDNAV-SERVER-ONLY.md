# ShroudNav — server-only navigation foundation

**Status:** Prototype source. Not installed, tested in-game, or deployed.

## Hard boundary
- No mods, DLLs, overlays or other installations required on players' game clients.
- Existing flight, pitch-hold, gifting, stacking, server launcher, game files and saves stay untouched.
- No autopilot.
- Do not claim a custom minimap or GPS HUD can appear inside unmodified game clients. Current in-game minimap projects hook clients.
- A no-install private browser map hosted through ShroudForge is an optional delivery option, not an in-game UI.
- ShroudForge currently has no verified live player-position or active-quest feed. Road routes must start with explicit map origin and destination, never fabricated tracking.
- One third-party hosting panel documents server-memory position tracking on Enshrouded 0.9, but it explicitly has not verified 1.0. Treat this as a research lead only, default OFF.
- The current pitch-hold patch changes pitch, not player-specific flight speed. Separate exact-build research needed; no guessed memory writes.

## Prototype deliverables
1. Versioned offline navigation graph import with coordinate-system declaration and source provenance.
2. Safe input validation (node/link limits; duplicate, disconnected and malformed segments rejected).
3. Road confidence: verified segments used by default; computer-vision candidate segments require an explicit option. No invented roads.
4. Weighted shortest-path calculation, nearest-node snapping, and simple turn cues.
5. Synthetic tests covering safe routing, invalid graph documents and map coordinate handling.
6. Existing Windows build CI configured for this development branch.

## Next verified-data gates
1. Acquire a licensable high-resolution 1.0 map and calibrate its pixels to the game's coordinate system, including independent landmarks. Do not assume third-party map grids use identical coordinates.
2. Use offline computer vision to propose road candidates; manually check bridges, passes, elevation, junctions, waterways, and cliff connections before marking verified.
3. Implement a map and route view behind a low-privilege read-only private-network interface; do not expose ShroudForge's server-control token.
4. Investigate read-only 1.0 player-position telemetry with a cloned server and consent/privacy controls. If unavailable, use manually chosen origin.
5. Link quest destinations only when trustworthy source coordinates exist. No fictitious quest state or GPS updates.
6. Investigate bounded adjustable flight speed and responsiveness in a separate mod patch. Preserve pitch hold, no auto-steering.

## Data contract
JSON with Format equal to shroudnav-roadgraph-v1, CoordinateSystem equal to enshrouded-world-xy-calibrated-v1, a nonempty SourceNote, Nodes containing Id/X/Y/Name, and Links containing From/To/Proof/RoadName. Proof is either verified or candidate. The source note is attribution evidence, not a guarantee a particular road is traversable.

The fixture roads are completely synthetic; this repository does **not** yet include a verified Embervale road database. Values are **map-coordinate units**, not labelled metres until calibration.

No changes to the owner's actual live server are permitted before successful isolated builds, route validation, explicit deployment approval and a rollback plan.
