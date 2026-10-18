#!/usr/bin/env python3
"""Convert a *user-supplied binary road mask* to unverified road-candidate GeoJSON.

Requires Pillow only when loading images (pip install Pillow). Does not extract or
download proprietary game maps. The mask must have white roads on black terrain.
All output is provisional: a thick map line or skeleton can contain false joins.
"""
import argparse
import json
import math
import pathlib
import sys


def neighbors(pixel):
    x, y = pixel
    return [(x+dx, y+dy) for dy in (-1, 0, 1) for dx in (-1, 0, 1)
            if dx != 0 or dy != 0]


def thin(foreground, max_iterations=300):
    """Zhang-Suen skeletonization. Operates on a bounded set of foreground pixels."""
    active = set(foreground)
    for _ in range(max_iterations):
        any_deleted = False
        for phase in (0, 1):
            drop = set()
            for point in active:
                x, y = point
                p = [(x, y-1), (x+1, y-1), (x+1, y), (x+1, y+1),
                     (x, y+1), (x-1, y+1), (x-1, y), (x-1, y-1)]
                b = [1 if pt in active else 0 for pt in p]
                count = sum(b)
                if count < 2 or count > 6:
                    continue
                transitions = sum(b[i] == 0 and b[(i+1) % 8] == 1 for i in range(8))
                if transitions != 1:
                    continue
                if phase == 0 and (b[0] * b[2] * b[4] or b[2] * b[4] * b[6]):
                    continue
                if phase == 1 and (b[0] * b[2] * b[6] or b[0] * b[4] * b[6]):
                    continue
                drop.add(point)
            if drop:
                active.difference_update(drop)
                any_deleted = True
        if not any_deleted:
            return active
    raise ValueError("Skeleton thinning did not converge within iteration bounds.")


def simplify(points, tolerance=1.5):
    """Iterative Douglas-Peucker simplification, with endpoint preservation."""
    if len(points) < 3:
        return points
    keep = {0, len(points)-1}
    stack = [(0, len(points)-1)]
    while stack:
        start, end = stack.pop()
        a, b = points[start], points[end]
        dx, dy = b[0]-a[0], b[1]-a[1]
        length2 = dx*dx + dy*dy
        widest, at = 0.0, None
        for i in range(start+1, end):
            x, y = points[i]
            t = max(0, min(1, ((x-a[0])*dx+(y-a[1])*dy)/length2)) if length2 else 0
            d = math.hypot(x-a[0]-t*dx, y-a[1]-t*dy)
            if d > widest:
                widest, at = d, i
        if at is not None and widest > tolerance:
            keep.add(at)
            stack.extend([(start, at), (at, end)])
    return [list(points[i]) for i in sorted(keep)]


def trace_skeleton(foreground, min_length=8):
    """Extract polylines at ends/forks. Intersections are *unverified*."""
    graph = {p: set(neighbors(p)) & foreground for p in foreground}
    seen = set()
    paths = []

    def key(a, b):
        return tuple(sorted((a, b)))

    def follow(a, b):
        points = [a, b]
        seen.add(key(a, b))
        prev, here = a, b
        while len(graph[here]) == 2:
            nxt = next(p for p in graph[here] if p != prev)
            edge = key(here, nxt)
            if edge in seen:
                break
            seen.add(edge)
            points.append(nxt)
            prev, here = here, nxt
        if len(points) >= 2 and sum(math.dist(u, v) for u, v in zip(points, points[1:])) >= min_length:
            paths.append(simplify(points))

    for start in sorted(graph):
        if len(graph[start]) == 2:
            continue
        for adjacent in sorted(graph[start]):
            if key(start, adjacent) not in seen:
                follow(start, adjacent)
    # Closed loops have no endpoint/fork: trace any remaining unvisited edge.
    for start in sorted(graph):
        for adjacent in sorted(graph[start]):
            if key(start, adjacent) not in seen:
                follow(start, adjacent)
    return [p for p in paths if len(p) >= 2 and p[0] != p[-1]]


def from_mask(path, threshold=190):
    try:
        from PIL import Image
    except ImportError as ex:
        raise RuntimeError("Pillow is required for PNG mask import; install it only on the map-authoring computer.") from ex
    with Image.open(path) as source:
        image = source.convert("L")
        width, height = image.size
        if width*height > 16000000:
            raise ValueError("Mask exceeds 16 million pixels.")
        pixels = image.load()
        selected = {(x, y) for y in range(height) for x in range(width)
                    if pixels[x, y] >= threshold}
        if len(selected) > 1200000:
            raise ValueError("Too many white mask pixels; narrow the source region.")
        if len(selected) < 3:
            raise ValueError("No plausible road mask pixels found.")
        return thin(selected)


def make_geojson(paths, source_image_id, source_id):
    if not source_image_id or not source_id:
        raise ValueError("A specific image ID and catalog source ID are required.")
    return {
        "type": "FeatureCollection", "coordinateSystem": "source-image-pixels",
        "sourceImageId": source_image_id,
        "features": [
            {"type": "Feature", "geometry": {"type": "LineString", "coordinates": p},
             "properties": {"roadId": "mask-" + str(i+1).zfill(6),
                            "sourceId": source_id, "roadName": "", "roadType": "path",
                            "proof": "candidate", "level": "surface",
                            "extraction": "binary-road-mask-skeleton",
                            "reviewStatus": "unverified-cv-candidate"}}
            for i, p in enumerate(paths)
        ]
    }


def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--mask", required=True, help="locally owned/licensed black/white road-segmentation PNG")
    ap.add_argument("--source-image-id", required=True)
    ap.add_argument("--source-id", required=True, help="existing map-data/sources/catalog.json source ID")
    ap.add_argument("--output", required=True)
    ap.add_argument("--threshold", type=int, default=190)
    ap.add_argument("--min-pixels", type=int, default=8)
    args = ap.parse_args()
    try:
        if not (1 <= args.threshold <= 255 and 2 <= args.min_pixels <= 200):
            raise ValueError("Threshold or minimum polyline length is outside limits.")
        skeleton = from_mask(args.mask, args.threshold)
        paths = trace_skeleton(skeleton, args.min_pixels)
        data = make_geojson(paths, args.source_image_id, args.source_id)
        if not paths:
            raise ValueError("Road mask did not yield any candidate lines.")
        destination = pathlib.Path(args.output)
        destination.parent.mkdir(parents=True, exist_ok=True)
        destination.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")
        print("Generated {} UNVERIFIED road candidate polylines; manual review and pixel calibration required.".format(len(paths)))
        return 0
    except (OSError, RuntimeError, ValueError) as exc:
        print("ROAD MASK NOT READY: " + str(exc), file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
