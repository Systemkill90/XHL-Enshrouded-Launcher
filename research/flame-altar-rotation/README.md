# Flame Altar — 45° Diamond Placement Research

**Requested:** allow an Enshrouded player to rotate a Flame Altar **45° at a time**, staying perfectly **flat** on the ground (yaw only; pitch and roll always 0). The purpose is a **diamond-shaped base** on the **normal voxel building grid**. The owner explicitly accepts losing some area near the original square's corners.

## Important distinction

The in-game Flame Altar's **visible orientation**, its **placement controls**, and its **persistent world-protection region** may be three independently implemented systems.

**This repository does not yet contain a working mod that changes Flame Altar placement or persistent boundaries.** `diamond_footprint.py` is a mathematically checked *design and feasibility module*, not a game patch, DLL, live mod, or hook.

### Safe interpretation to investigate first: inscribed diamond

Leave the **actual vanilla protected square/cube unchanged** and design an axis-aligned voxel **diamond inside it**. An inscribed 45° diamond touches the midpoint of every outer square edge, but its tips never go outside persistent vanilla protection. Do not change altar upgrade tiers, persistence/region serialization, extents, protected saves, or existing world geometry.

- Vanilla ground footprint for altar zone side `S`: `-S/2 <= x,z <= S/2`
- The intended diamond: **`|x| + |z| <= S/2`** (altar centered at the origin).
- Its theoretical area is `S²/2`, exactly **50% of the vanilla square**.
- Its sides form **45° diagonals** across the unrotated voxel grid.
- Whole grid cells touching/crossing the boundary are conservatively **excluded** by checking all four cell corners.
- Vertical protection from the **existing vanilla cube** remains unchanged. **The real game would still protect the square**, including areas outside the design diamond. This isn't literally a rotated protection region.

| Vanilla level | Existing footprint | Largest internal 45° diamond area | Area intentionally unused |
| :-- | --: | --: | --: |
| 40×40 | 1,600 | 800 | 50% |
| 80×80 | 6,400 | 3,200 | 50% |
| 120×120 | 14,400 | 7,200 | 50% |
| 160×160 | 25,600 | 12,800 | 50% |

Units are idealized horizontal grid units squared; these numbers are **not measured gameplay building-block counts**.

### Rotation choices

`0° → 45° → 90° → 135° → 180° → 225° → 270° → 315°`, then repeat.

At 45° increments a *square-shaped* territory has only **two distinct boundary orientations**: square on cardinal orientations, diamond on diagonal orientations. The flame altar **prop** itself can still have eight visually distinct headings if a genuine placement control accepts it. **No tilt. No off-grid building blocks.**

## What we need to identify in the real game

1. Where the **altar ghost-placement yaw increment** is calculated; it is likely client-owned, but this is **not verified by a decompilation**.
2. Whether the dedicated server **accepts / stores** a 45° yaw for a Flame Altar entity, and if such data reaches an **unmodified client**.
3. Whether altar yaw affects the protection zone, or if the zone is always an **axis-aligned cube** independent of altar orientation.
4. How altar protection is serialized and retained across **server restarts, world regeneration and altar upgrades**.
5. Whether any existing gameplay data/resource patch, **without client installation**, can enable the eight headings. No unverified memory offsets or loader replacements.

The community reports that voxel building uses a world-aligned grid, and [flame altar discussions mention the desired 45° rotation](https://www.reddit.com/r/Enshrouded/comments/1jlxcb4/). The [More Altars mod removed its build-zone size option](https://www.nexusmods.com/enshrouded/mods/104?tab=description) after persistence problems. This does not prove a diamond cannot be built; it **does** make a protection-region rewrite unsafe without specific evidence.

## Proposed game proof: minimal, reversible, fail closed

1. Read-only inspect the *exact owner game build* and existing loader in the approved desktop session. Avoid cloning Velaris saves into source control.
2. Research legally accessible resource schemas and locate placement rotation/territory definitions. Any extraction/decompilation must be source/rights-compliant and limited to assets we can lawfully inspect locally. **No proprietary resources committed.**
3. If a **server-only setting/hook** can change altar *placement yaw* on a vanilla client, make a **single 45° rotation proof** with original bytes preserved and exact-build signature verification. **Do not touch protection, saves or existing altars**.
4. Test whether an *unmodified client* sees the 45° preview and successfully places a new altar; confirm its actual protected region with *temporary expendable blocks*, a backup and an explicitly approved maintenance window. Check restarts before claiming persistence.
5. If client input/preview is hardwired and ignores server changes, **STOP** this no-client-dependency mod direction. Don't substitute a purely visual mockup or break world persistence.
6. If rotating vanilla protection is independently shown safe (not assumed), consider geometry only **after** explicit owner approval and rigorous rollback/world-save testing. The default is unchanged vanilla protection.

### Owner's public-server rule

Velaris is a *public live server* and the owner's test environment. This branch changes **GitHub source and documentation only**. No game installation, server restart, save edit, region/persistence change, proxy DLL injection or new client dependency is authorized.

## Offline files

- [diamond_footprint.py](diamond_footprint.py) — exact 45° model and **fully enclosed voxel cell** test.
- [Synthetic tests](tests/test_diamond_footprint.py) — yaw and zero-tilt verification, boundary containment, area loss, conservative grid-cell checks.
- [Read-only resource-type inspection](resource-type-inspection/) — future exact-build research; optional and never automatically installed.
- [GitHub workflow](../../.github/workflows/flame-altar-geometry.yml) — validates the geometry code, **not** in-game behavior.

**Current decision: research.** Only elevate to a deployable candidate after a real server-only placement API/hook is established on the owner's exact build.
