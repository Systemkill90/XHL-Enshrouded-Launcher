#!/usr/bin/env python3
"""Validate/import authored GeoJSON into the ShroudNav roadgraph v1 format.

Python standard library only. No scraping, image downloads, Steam connection or
server mutation. Imports reviewable road segments, never creates junctions where
two differently levelled lines merely cross on the map.
"""
import argparse
import hashlib
import json
import math
import pathlib
import sys
from collections import defaultdict

from calibrate import validate as validate_calibration, apply as project_pixel, CalibrationError
from topology import audit_unjoined_crossings

ROOT = pathlib.Path(__file__).resolve().parents[1]
FRAME = "enshrouded-world-xy-calibrated-v1"
MAX_FILE_BYTES = 12_000_000
MAX_LINKS = 100_000
ROAD_TYPES = {"road", "trail", "bridge", "tunnel", "stairs", "path"}
LEVELS = {"surface", "bridge", "tunnel", "cave", "upper", "lower"}


class MapDataError(ValueError):
    pass


def load_json(path):
    path = pathlib.Path(path)
    if path.stat().st_size > MAX_FILE_BYTES:
        raise MapDataError("Oversized input file: " + str(path))
    with path.open(encoding="utf-8-sig") as stream:
        return json.load(stream)


def valid_point(pair):
    return isinstance(pair, list) and len(pair) == 2 and all(
        isinstance(v, (int, float)) and not isinstance(v, bool)
        and math.isfinite(v) and abs(v) <= 1e6 for v in pair)


def node_key(point, level):
    # Coordinates within 0.001 world/map units collapse to the same node.
    # Distinct elevation/layers NEVER auto-connect (bridges and cave crossings).
    return (round(point[0], 3), round(point[1], 3), level)


def node_id(key):
    return "n_" + hashlib.sha1(repr(key).encode("utf-8")).hexdigest()[:16]


def coordinates(features, transform, image_id, frame):
    if frame == FRAME:
        return features
    if frame != "source-image-pixels" or transform is None or not image_id:
        raise MapDataError("Pixel features require an approved matching affine calibration.")
    output = []
    for feature in features:
        converted = dict(feature)
        geometry = dict(feature["geometry"])
        geometry["coordinates"] = [project_pixel(transform, xy) for xy in geometry["coordinates"]]
        converted["geometry"] = geometry
        output.append(converted)
    return output


def read_lines(path, proof, catalog, cal):
    collection = load_json(path)
    if collection.get("type") != "FeatureCollection" or not isinstance(collection.get("features"), list):
        raise MapDataError("Expected a GeoJSON FeatureCollection in " + str(path))
    features = collection["features"]
    if len(features) > MAX_LINKS:
        raise MapDataError("Too many road features.")
    frame = collection.get("coordinateSystem")
    transform = None
    if frame == "source-image-pixels" and features:
        active = cal.get("activeCalibration")
        match = next((x for x in cal.get("calibrations", [])
                      if isinstance(x, dict) and x.get("id") == active), None)
        if not match or match.get("sourceImageId") != collection.get("sourceImageId"):
            raise MapDataError("No approved calibration for the exact traced source image.")
        try:
            transform, _ = validate_calibration(match)
        except CalibrationError as exc:
            raise MapDataError(str(exc))
    elif frame != FRAME and not (frame == "source-image-pixels" and not features):
        raise MapDataError("Unknown coordinate frame: " + str(frame))
    if frame == "source-image-pixels":
        features = coordinates(features, transform, collection.get("sourceImageId"), frame) if features else []
    results, denied = [], []
    for feature in features:
        if not isinstance(feature, dict) or feature.get("type") != "Feature" or \
                not isinstance(feature.get("geometry"), dict) or not isinstance(feature.get("properties"), dict):
            raise MapDataError("Malformed road feature.")
        geo, p = feature["geometry"], feature["properties"]
        line = geo.get("coordinates")
        if geo.get("type") != "LineString" or not isinstance(line, list) or len(line) < 2 or len(line) > 2000 \
                or not all(valid_point(pt) for pt in line):
            raise MapDataError("Road must be a bounded 2D LineString.")
        if p.get("proof") != proof or p.get("roadType") not in ROAD_TYPES or p.get("level") not in LEVELS \
                or not isinstance(p.get("roadId"), str) or not p["roadId"].strip():
            raise MapDataError("Missing road ID, type, elevation level, or mismatched proof.")
        if proof == "verified" and (not p.get("reviewedBy") or not p.get("reviewedAt")):
            raise MapDataError("Verified roads require reviewer identity and review date.")
        source = catalog.get(p.get("sourceId"))
        if source is None:
            raise MapDataError("Unknown road provenance source.")
        if source.get("rightsStatus") != "approved" or not source.get("redistributionAllowed"):
            denied.append(p["roadId"] + ":" + str(p.get("sourceId")))
        results.append({"points": line, "props": p, "proof": proof})
    return results, denied


def blocked_edges(path):
    doc = load_json(path)
    if doc.get("type") != "FeatureCollection" or doc.get("coordinateSystem") != FRAME:
        raise MapDataError("Obstacles must use normalized world-frame GeoJSON.")
    result = set()
    for feature in doc.get("features", []):
        geo, p = feature.get("geometry", {}), feature.get("properties", {})
        coords = geo.get("coordinates")
        if geo.get("type") != "LineString" or not isinstance(coords, list) or len(coords) != 2 \
                or not all(valid_point(x) for x in coords) or p.get("level") not in LEVELS \
                or p.get("status") != "confirmed" or not p.get("reviewedBy"):
            raise MapDataError("Obstacle must be a confirmed reviewed, exact two-vertex blocked segment.")
        a, b = node_key(coords[0], p["level"]), node_key(coords[1], p["level"])
        result.add(tuple(sorted((a, b))))
    return result


def build(verified_path, candidates_path, obstacles_path, catalog_path, calibration_path, include_candidates=False):
    source_data = load_json(catalog_path)
    catalog = {item["id"]: item for item in source_data.get("sources", [])}
    cal = load_json(calibration_path)
    if cal.get("format") != "shroudnav-calibration-v1":
        raise MapDataError("Unrecognized calibration manifest.")
    verified, denied_v = read_lines(verified_path, "verified", catalog, cal)
    candidate, denied_c = read_lines(candidates_path, "candidate", catalog, cal)
    if denied_v:
        raise MapDataError("Verified roads have unapproved redistribution provenance: " + ", ".join(denied_v[:5]))
    allowed = verified + (candidate if include_candidates else [])
    if include_candidates and denied_c:
        raise MapDataError("Cannot export third-party-derived candidate roads without reuse permission.")
    obstacles = blocked_edges(obstacles_path)
    nodes, links, used_pairs = {}, [], set()
    segments = []
    applied_obstacles = set()
    evidence = set()
    for road in allowed:
        p = road["props"]
        evidence.add(p["sourceId"])
        for a, b in zip(road["points"], road["points"][1:]):
            ka, kb = node_key(a, p["level"]), node_key(b, p["level"])
            if ka == kb:
                raise MapDataError("Degenerate road edge with coincident world coordinates.")
            pair = tuple(sorted((ka, kb)))
            if pair in obstacles:
                applied_obstacles.add(pair)
                continue
            if pair in used_pairs:
                raise MapDataError("Duplicate/conflicting road segments; review line intersections.")
            used_pairs.add(pair)
            for key in (ka, kb):
                if key not in nodes:
                    nodes[key] = {"Id": node_id(key), "X": key[0], "Y": key[1], "Name": ""}
            segments.append((ka, kb, p["level"], p["roadId"]))
            links.append({"From": nodes[ka]["Id"], "To": nodes[kb]["Id"],
                          "Proof": road["proof"], "RoadName": str(p.get("roadName", ""))[:120]})
            if len(links) > MAX_LINKS or len(nodes) > 30000:
                raise MapDataError("ShroudNav runtime graph size limit exceeded.")
    # Eliminate nodes made orphaned by closure removal.
    used_ids = {link["From"] for link in links} | {link["To"] for link in links}
    graph_nodes = sorted((n for n in nodes.values() if n["Id"] in used_ids), key=lambda n: n["Id"])
    graph_links = sorted(links, key=lambda n: (n["From"], n["To"]))
    components = 0
    neighbor = defaultdict(set)
    for link in graph_links:
        neighbor[link["From"]].add(link["To"])
        neighbor[link["To"]].add(link["From"])
    pending = set(used_ids)
    while pending:
        components += 1
        queue = [pending.pop()]
        while queue:
            for next_node in neighbor[queue.pop()] & pending:
                pending.remove(next_node)
                queue.append(next_node)
    crossing_complete, crossings = audit_unjoined_crossings(segments)
    has_verified = any(x["Proof"] == "verified" for x in links)
    status = ("blocked_no_verified_roads" if not has_verified else
              "blocked_topology_review" if (not crossing_complete or crossings) else
              "ready_for_review")
    report = {
        "format": "shroudnav-coverage-v1",
        "status": status,
        "crossingAuditComplete": crossing_complete,
        "unjoinedCrossingWarnings": crossings,
        "verifiedRoadFeatures": len(verified),
        "candidateRoadFeatures": len(candidate),
        "exportedNodes": len(graph_nodes),
        "exportedSegments": len(graph_links),
        "exportedCandidateSegments": sum(x["Proof"] == "candidate" for x in links),
        "connectedComponents": components,
        "obstacles": len(obstacles),
        "appliedObstacles": len(applied_obstacles),
        "unmatchedObstacles": len(obstacles - applied_obstacles),
        "candidateSourceRightsBlocked": denied_c,
        "note": "No crossings, slopes or cliffs are inferred as traversable. Any verified feature is operator-reviewed, not independently observed by this tool."
    }
    doc = {
        "Format": "shroudnav-roadgraph-v1", "CoordinateSystem": FRAME,
        "SourceNote": "Approved original roads: " + ", ".join(sorted(evidence)) if evidence else "No reviewed road evidence",
        "Nodes": graph_nodes, "Links": graph_links
    }
    return doc, report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true", help="validate and print coverage without publishing")
    parser.add_argument("--publish", action="store_true", help="write roadgraph only when reviewed verified routes exist")
    parser.add_argument("--include-candidates", action="store_true",
                        help="explicitly include approved candidates; ShroudNav still excludes them by default")
    parser.add_argument("--output", default=str(ROOT / "reports" / "roadgraph.json"))
    parser.add_argument("--verified", default=str(ROOT / "roads" / "verified.geojson"))
    parser.add_argument("--candidates", default=str(ROOT / "roads" / "candidates.geojson"))
    parser.add_argument("--obstacles", default=str(ROOT / "roads" / "obstacles.geojson"))
    parser.add_argument("--catalog", default=str(ROOT / "sources" / "catalog.json"))
    parser.add_argument("--calibration", default=str(ROOT / "calibration" / "calibrations.json"))
    args = parser.parse_args()
    try:
        graph, coverage = build(args.verified, args.candidates, args.obstacles,
                                args.catalog, args.calibration, args.include_candidates)
        print(json.dumps(coverage, indent=2))
        if args.publish:
            if coverage["status"] != "ready_for_review" or coverage["exportedSegments"] <= 0:
                raise MapDataError("No valid reviewed verified roads; refusing graph publication.")
            output = pathlib.Path(args.output)
            output.parent.mkdir(parents=True, exist_ok=True)
            # Atomic file replacement: truncated exports never become install inputs.
            tmp = output.with_name(output.name + ".tmp")
            try:
                tmp.write_text(json.dumps(graph, indent=2) + "\n", encoding="utf-8")
                tmp.replace(output)
            finally:
                if tmp.exists():
                    tmp.unlink()
            print("Wrote reviewed graph to " + str(output))
        return 0
    except (MapDataError, CalibrationError, ValueError, OSError, KeyError, TypeError) as exc:
        print("MAP DATA NOT READY: " + str(exc), file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
