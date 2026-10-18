#!/usr/bin/env python3
"""Conservative *design footprint* for a 45-degree diamond inside a vanilla altar zone.

This does NOT rotate a vanilla Enshrouded Flame Altar, change its input controls,
rewrite persistent protection regions, hook the server, or install a mod.
All dimensions are illustrative vanilla level side lengths in map/grid units.
"""
import argparse
import json
import math
import sys

SUPPORTED_SIZES = (40, 80, 120, 160)
SUPPORTED_YAW = tuple(range(0, 360, 45))


class FootprintError(ValueError):
    pass


def snap_yaw(degrees):
    if type(degrees) not in (int, float) or not math.isfinite(degrees):
        raise FootprintError("Yaw must be finite")
    nearest = round(degrees / 45)
    if abs(degrees - nearest * 45) > 1e-9:
        raise FootprintError("Allowed yaw increments are exactly 45 degrees")
    return (nearest * 45) % 360


def plan_footprint(side, yaw=45):
    """A conservative build *planning* region inside the vanilla axis-aligned zone.

    The footprint is a square for 0/90/180/270 and an *inscribed* 45 degree
    diamond for 45/135/225/315. Vanilla server persistence still uses its
    original cube; this is merely an inner design footprint.
    """
    if type(side) is not int or side not in SUPPORTED_SIZES:
        raise FootprintError("Unsupported side length; only documented vanilla sizes")
    yaw = snap_yaw(yaw)
    h = side / 2
    diamond = yaw % 90 != 0
    if diamond:
        # Vertices touch vanilla square's edge midpoints, but never extend past.
        vertices = [[0.0, -h], [h, 0.0], [0.0, h], [-h, 0.0]]
    else:
        vertices = [[-h, -h], [h, -h], [h, h], [-h, h]]
    area = side * side / (2 if diamond else 1)
    return {
        "format": "shroudforge-altar-diamond-design-v1",
        "status": "offline-design-only_NOT-a-working-game-mod",
        "sideInGridUnits": side,
        "yawDegrees": yaw,
        "pitchDegrees": 0,
        "rollDegrees": 0,
        "actualServerTerritoryAltered": False,
        "actualPlacementControlsAltered": False,
        "worldGridRemainsAxisAligned": True,
        "planShape": "inscribed-diamond" if diamond else "vanilla-square",
        "coordinates": vertices,
        "intendedPlanAreaGridUnitsSquared": area,
        "vanillaSquareAreaGridUnitsSquared": side * side,
        "fractionOfVanillaArea": area / (side * side),
        "notes": [
            "This is an architect's conservative plan, not the rendered or enforced altar boundary.",
            "The vanilla altar must still protect the enclosing square/cube; do not remove or shrink its protection.",
            "A rotated square has only two distinct shape orientations at 45 degree increments: square or diamond.",
            "No game-side input control or save-format change is attempted.",
        ],
    }


def fully_inside_footprint(cell_x, cell_z, side, yaw=45, cell_size=1):
    """True if all 4 corners of an axis-aligned voxel's ground face fit inside plan.

    Cell is [cell_x, cell_x + size] × [cell_z, cell_z + size] in altar-relative
    coordinates. Model intentionally conservative for possible voxel buildings.
    """
    plan = plan_footprint(side, yaw)
    if type(cell_size) not in (int, float) or not math.isfinite(cell_size) or cell_size <= 0:
        raise FootprintError("Cell size must be positive/finite")
    if any(type(v) not in (float, int) or not math.isfinite(v) for v in (cell_x, cell_z)):
        raise FootprintError("Cell origin must be finite")
    half = side / 2
    corners = [(cell_x, cell_z), (cell_x + cell_size, cell_z),
               (cell_x, cell_z + cell_size), (cell_x + cell_size, cell_z + cell_size)]
    tolerance = 1e-9
    for x, z in corners:
        if abs(x) > half + tolerance or abs(z) > half + tolerance:
            return False
        if plan["planShape"] == "inscribed-diamond" and abs(x) + abs(z) > half + tolerance:
            return False
    return True


def grid_stats(side, yaw=45):
    """Count 1-unit axis-aligned building cells completely within design boundary."""
    plan = plan_footprint(side, yaw)
    half = side // 2
    accepted = [
        (x, z) for z in range(-half, half) for x in range(-half, half)
        if fully_inside_footprint(x, z, side, yaw)
    ]
    plan["wholeOneUnitCells"] = len(accepted)
    plan["fractionOfWholeCells"] = len(accepted) / (side * side)
    plan["wholeCellCountIsAnEstimateOfInGameBlocks"] = False
    return plan


def ascii_view(side=40, yaw=45, resolution=20):
    plan_footprint(side, yaw)
    if type(resolution) is not int or resolution < 4 or resolution > 80:
        raise FootprintError("Preview resolution must be 4..80")
    pitch = side / resolution
    half = side / 2
    rows = []
    for iz in range(resolution):
        z = -half + (iz + 0.5) * pitch
        chars = []
        for ix in range(resolution):
            x = -half + (ix + 0.5) * pitch
            inside = abs(x) <= half and abs(z) <= half
            if yaw % 90 != 0:
                inside = inside and abs(x) + abs(z) <= half + 1e-9
            chars.append("◆" if inside else "·")
        rows.append("".join(chars))
    return "\n".join(rows)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--size", type=int, choices=SUPPORTED_SIZES, default=40)
    parser.add_argument("--yaw", type=int, choices=SUPPORTED_YAW, default=45)
    parser.add_argument("--ascii", action="store_true", help="print safe footprint preview")
    args = parser.parse_args()
    try:
        report = grid_stats(args.size, args.yaw)
        print(json.dumps(report, indent=2))
        if args.ascii:
            print(ascii_view(args.size, args.yaw))
    except FootprintError as exc:
        print("FOOTPRINT NOT READY: " + str(exc), file=sys.stderr)
        return 2
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
