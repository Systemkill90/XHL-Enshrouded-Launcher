# Map Data & Integrations

**ShroudNav's dedicated repository section.** Versioned spatial data, road tracing, integration research, calibration, validation and exports live here. This is source/data development only; it does **not** touch the live Velaris server or install anything on game clients.

## Directory map

| Location | Contents |
|---|---|
| `sources/catalog.json` | Reference maps, rights decisions, source-specific coordinate systems and evidence |
| `integrations/providers.json` | External map, quest, marker and live-position integration research. No credentials |
| `calibration/calibrations.json` | Control points and pixel-to-world affine calibration (inactive until independently verified) |
| `roads/candidates.geojson` | Road and trail traces awaiting review (NOT routed by default) |
| `roads/verified.geojson` | Road segments with confirmed provenance and manual review |
| `roads/obstacles.geojson` | Closures, cliffs, missing bridges and disallowed segments |
| `poi/markers.geojson` | Curated POIs and quest destinations, initially empty |
| `tools/build_graph.py` | Offline GeoJSON to ShroudNav roadgraph converter + validation |
| `tools/trace-roads.html` | No-upload, local-browser road tracing workspace |
| `tests/test_map_data.py` | Synthetic topology, coordinate, safety and importer checks |
| `reports/` | Generated statistics and review reports (never mislabelled as verified world data) |

## Current evidence status
**0 verified world roads; 0 real-world trace samples imported; no verified player-position feed.** The existing ShroudNav router is a tested-in-source prototype, not a working in-game GPS. The map editor and importer are offline tools for preparing legitimate data. Never bundle third-party map images or marker datasets until redistribution rights are established.

### Data safety rules

- Every source is attributed; `reference_only` sources cannot be bundled into data exports.
- Do not conflate Wiki XY (0–8191) with Enshrouded DB XZ (0–10240) or map-image pixels. Map calibration must be measured using matching landmarks, never guessed from dimension ratios.
- Coordinates always include their frame and units; source-image Y points down, world north points toward increasing coordinate as specified by that source.
- No road is `verified` from computer vision alone. Junctions require genuine traversable connections; drawing lines crossing does not create a join unless they share a sampled graph node.
- Elevation, portals, tunnels, destroyed bridges, Shroud hazards and dynamic build changes require review. Until height/terrain is verified, no travel-time, meters or shortest *walkable* guarantee.
- Players install nothing. The future web map is an optional browser view hosted by the private launcher with read-only limited permissions; no client HUD can be promised.
- No auto-steering/autopilot. No queries from the game server or live process are made by these tools.

## Local data workflow

1. Establish a legally reusable source raster; copy it **locally** (do not commit without rights).
2. Open `tools/trace-roads.html` in a browser, select the raster locally, and trace road candidates. Export candidate GeoJSON.
3. Collect at least three *non-collinear*, independent pixel/world control-point pairs from the same map; record them in `calibration/calibrations.json`, keeping status pending until measured.
4. Run `python map-data/tools/build_graph.py --check` for validation and coverage. It refuses to publish when the map lacks a calibrated reference and verified roads.
5. Review bridge/cliff/trail topology and move owner-approved segments to verified GeoJSON with reviewer name/date. Then run `--publish` to generate a compatible roadgraph.
6. Validate in disposable Windows fixtures before ever integrating into the launcher UI.

See `integrations/providers.json` and `sources/catalog.json` for research candidates and evidence limits.
