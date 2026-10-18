#!/usr/bin/env python3
"""Create a conservative, offline POI-to-road anchor index from approved data.

Does not read players, quests, saves or Steam. No inferred quest completion.
Map-coordinate units remain uncalibrated until landmark agreement is established.
"""
import argparse
import json
import math
import pathlib
import sys
from collections import defaultdict

from build_graph import FRAME, MAX_FILE_BYTES, MapDataError, load_json, valid_point

ROOT = pathlib.Path(__file__).resolve().parents[1]
CATEGORIES = {"quest", "landmark", "village", "camp", "mine", "cave", "fast-travel",
              "bridge", "junction", "resource", "other"}


def index_pois(graph, collection, catalog, max_snap):
    if not math.isfinite(max_snap) or not (0 < max_snap <= 1000):
        raise MapDataError("An explicit bounded maximum snap distance is required.")
    if graph.get("Format") != "shroudnav-roadgraph-v1" or graph.get("CoordinateSystem") != FRAME:
        raise MapDataError("Unrecognized graph or frame.")
    if collection.get("type") != "FeatureCollection" or collection.get("coordinateSystem") != FRAME:
        raise MapDataError("POIs must be in the same independently calibrated world frame.")
    graph_nodes = graph.get("Nodes", [])
    if not isinstance(graph_nodes, list) or len(graph_nodes) > 30000:
        raise MapDataError("Invalid road graph.")
    approved = {v["id"] for v in catalog.get("sources", [])
                if v.get("rightsStatus") == "approved" and v.get("redistributionAllowed")}
    if not graph_nodes:
        return {"Format": "shroudnav-poi-index-v1", "CoordinateSystem": FRAME,
                "status": "blocked_no_roads", "MaxSnapDistanceMapUnits": max_snap,
                "Records": [], "Unreachable": []}
    grid = defaultdict(list)
    for node in graph_nodes:
        p = [node.get("X"), node.get("Y")]
        if not valid_point(p):
            raise MapDataError("Invalid road-node location.")
        grid[(math.floor(p[0]/max_snap), math.floor(p[1]/max_snap))].append(node)
    connected = set()
    for link in graph.get("Links", []):
        connected.add(link.get("From"))
        connected.add(link.get("To"))
    out, unlinked, ids = [], [], set()
    features = collection.get("features", [])
    if not isinstance(features, list) or len(features) > 20000:
        raise MapDataError("Invalid POI count.")
    for f in features:
        if not isinstance(f, dict) or f.get("type") != "Feature":
            raise MapDataError("Malformed POI feature.")
        geo, props = f.get("geometry", {}), f.get("properties", {})
        if geo.get("type") != "Point" or not valid_point(geo.get("coordinates")):
            raise MapDataError("POIs must contain finite two-dimensional Point coordinates.")
        identifier, label = props.get("id"), props.get("name")
        if not isinstance(identifier, str) or not identifier or identifier in ids or len(identifier) > 100 \
                or not isinstance(label, str) or not label or len(label) > 120 \
                or props.get("category") not in CATEGORIES:
            raise MapDataError("Invalid, duplicate, or unlabelled POI.")
        ids.add(identifier)
        if props.get("status") != "verified" or not props.get("reviewedBy") \
                or props.get("sourceId") not in approved:
            raise MapDataError("POI not independently reviewed or lacks redistribution approval.")
        p = geo["coordinates"]
        cx, cy = math.floor(p[0]/max_snap), math.floor(p[1]/max_snap)
        candidates = [node for dx in (-1, 0, 1) for dy in (-1, 0, 1)
                      for node in grid.get((cx+dx, cy+dy), ()) if node.get("Id") in connected]
        matches = sorted(((math.dist(p, [n["X"], n["Y"]]), n) for n in candidates),
                         key=lambda e: (e[0], e[1]["Id"]))
        if not matches or matches[0][0] > max_snap:
            unlinked.append({"id": identifier, "reason": "No connected road node within snap radius."})
            continue
        distance, node = matches[0]
        out.append({"id": identifier, "name": label, "category": props["category"],
                    "x": p[0], "y": p[1], "nearestNodeId": node["Id"],
                    "offsetMapUnits": round(distance, 3),
                    "sourceId": props["sourceId"],
                    "warning": "Road anchor proximity does not prove walkability or a complete quest route."})
    return {"Format": "shroudnav-poi-index-v1", "CoordinateSystem": FRAME,
            "status": "ready_for_manual_review" if out else "no_reachable_destinations",
            "MaxSnapDistanceMapUnits": max_snap, "Records": out, "Unreachable": unlinked}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--graph", required=True, help="Previously reviewed ShroudNav roadgraph JSON")
    parser.add_argument("--markers", default=str(ROOT/"poi"/"markers.geojson"))
    parser.add_argument("--catalog", default=str(ROOT/"sources"/"catalog.json"))
    parser.add_argument("--max-snap", type=float, required=True, help="Map-coordinate units, NOT metres")
    parser.add_argument("--output", help="Only write after manual review; never installs into game")
    args = parser.parse_args()
    try:
        index = index_pois(load_json(args.graph), load_json(args.markers),
                           load_json(args.catalog), args.max_snap)
        print(json.dumps({"status": index["status"], "reachable": len(index["Records"]),
                          "unreachable": len(index["Unreachable"])}, indent=2))
        if args.output:
            if index["status"] != "ready_for_manual_review":
                raise MapDataError("No reachable reviewed POIs to export.")
            output = pathlib.Path(args.output)
            output.parent.mkdir(parents=True, exist_ok=True)
            tmp = output.with_name(output.name + ".tmp")
            try:
                tmp.write_text(json.dumps(index, indent=2)+"\n", encoding="utf-8")
                tmp.replace(output)
            finally:
                if tmp.exists():
                    tmp.unlink()
        return 0
    except (MapDataError, OSError, ValueError, KeyError, TypeError) as exc:
        print("POI INTEGRATION NOT READY: " + str(exc), file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
