# Embervale road geometry — verification record
Checked: 2026-10-17 · Game target: Enshrouded 1.0
Status: **partial topological corroboration; exact geometric coordinates NOT verified**

## Methods actually used
1. Compared 1.0-era walkthroughs and a legacy route guide for named roads, junctions, hazards, and transitions.
2. Recorded published coordinates of eight Ancient Spires / locations from the official wiki, preserving wiki X/Y horizontal and Z height — without converting them to the newer 10,240-site X/Z map.
3. Cross-checked regional directions: the Low Meadows spire is east-southeast of the Springlands spire in the legacy coordinate list (delta X +1,403; delta Y -354), which agrees with the independent description of Low Meadows as the eastern subregion of Springlands. The Revelwood spire is north of the Springlands spire (+299 X; +1,663 Y), consistent with the general geography. This checks **orientation only**, not road geometry, exact scale, bridge passability, or alignment with the 1.0 site.
4. Ran isolated map-data GitHub Actions including source/provenance safeguards, explicit route evidence checks, independent landmark calibration fixtures, topology and bridge-layer tests. Passing tests validate the *pipeline*, not Embervale road polylines.

## Corroborated real-world / in-world descriptions

### Springlands: Braelyn Bridge toward Ancient Spire
Three different publications describe broadly the same route:
- [AllThingsHow 1.0 guide, updated October 16](https://allthings.how/enshrouded-the-ancient-spires-walkthrough-springlands-spire/): broken bridge, grappling point, northbound dirt path, barricade, uphill entry.
- [9Puz 1.0 walkthrough, published October 16](https://9puz.com/6935-enshrouded-1-0-beginner-walkthrough/): grappling across Braelyn then road toward Springlands Spire; also describes a pre-grapple Shroud alternative.
- [Gamer Guides legacy guide](https://earth.gamerguides.com/enshrouded/guide/ancient-spires/springlands/springlands-ancient-spire-puzzle-guide-enshrouded): northern path to a blockade and upslope spire approach, plus a Shroud alternative.

**Result:** Narrative corridor corroborated; approximate direction north. **Not safe to tag Braelyn Bridge as a fully walkable continuous road.** It requires an interaction/alternative route and still lacks a surveyed polyline, elevation, and 1.0 in-game traversal validation. No GeoJSON segment emitted.

### Ancient Vault (Farmer) to Revelwood
[Official wiki guide](https://enshrouded.wiki.gg/wiki/Quests/Reclaim_The_Wilderness) describes a northern road from Withered Encampment through Willow Crush, a crevasse/bridge and a glide/cliffside approach toward Revelwood Spire. Because movement includes gaps, ladders and gliding, this is **not an ordinary pedestrian road**.

**Result:** Single source, qualitative topology only. Do not route without more evidence.

### Braelyn east toward Elixir Well
[Older Gamer Guides route](https://earth.gamerguides.com/enshrouded/guide/elixir-wells/locations) describes a branch east from a dirt road onto a northbound stone road and up stairs.

**Result:** Single old source, unconfirmed in 1.0. Mark as reference only.

## Coordinate sources and disagreement
- [Official wiki coordinate instructions](https://enshrouded.wiki.gg/wiki/Help:Editing/Map/Ingame_coordinates): whole map X/Y 0–8191 with Z representing height; reported locations were measured at early-access portals.
- [Enshrouded Database 1.0 map](https://enshroudeddatabase.com/map/): world X/Z 0–10,240, game-derived map texture and POIs.
- [Companion for Enshrouded 2D/3D map](https://enshrouded.gamevault.in/map): separate terrain and 3D visual reference.

**Do not equate the axes merely because both sources label them world coordinates.** Nor can the 8191↔10240 scale factor alone establish a coordinate transform. We still need three genuinely measured matching control points plus at least one independent holdout landmark on a particular *reusable* map raster. The wiki textual content has noncommercial/sharealike terms; game screenshots/tiles remain Keen Games' IP and may not be redistributed without permission.

## Numeric completeness
| Evidence | Count | May become routable immediately? |
|---|---:|---|
| Legacy published reference points | 8 | No |
| Corroborated named corridor (narrative) | 1 | No |
| Single-source corridor descriptions | 2 | No |
| Independently calibrated 1.0 map landmarks | 0 | No |
| Licensed source map raster committed | 0 | No |
| Fully surveyed road centerline points | 0 | No |
| Verified Embervale GeoJSON road edges | 0 | No |
| Roads available to the ShroudNav runtime | 0 | No |

## Next verification milestone
Acquire a reusable **1.0 terrain raster** and measured image/game control-point pairs. Then:

1. Trace only a small pilot area around Braelyn Bridge and the Springlands Spire.
2. Calibrate with independent 1.0 landmarks, compute held-out residuals, and confirm that bridge and road vertices land on the correct terrain features.
3. Validate road continuity in the 3D terrain, including bridge gaps, steps, cliffs and the Shroud detour.
4. Mark each candidate as approved only with reviewer/date, source rights, coordinates and traversal constraints.
5. Route-test the reviewed pilot, and then expand region by region.

**Honest outcome:** external cross-checking substantiates the basic corridor, *not* an exact road graph or a usable live GPS. The pipeline intentionally refuses to promote these claims to routable geometry.
