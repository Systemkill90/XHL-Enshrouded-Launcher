#!/usr/bin/env python3
"""Read-only audit of real-world reference evidence for Enshrouded road mapping.

Confirms the provenance/consistency of known published landmark and route-guide
claims. It does NOT convert narrative directions into verified road geometry.
No map assets, HTTP calls, client installs or live server access.
"""
import argparse
import json
import math
import pathlib
import sys
from urllib.parse import urlparse

HERE = pathlib.Path(__file__).resolve().parents[1]
URL_WIKI = "https://enshrouded.wiki.gg/wiki/Help:Editing/Map/Ingame_coordinates"


class EvidenceError(ValueError):
    pass


def finite(value):
    return isinstance(value, (float, int)) and not isinstance(value, bool) and math.isfinite(value)


def verify_references(points, routes):
    if points.get("format") != "shroudnav-landmark-references-v1" \
            or points.get("coordinateFrame") != "wiki-map-xy-0-8191" \
            or points.get("coordinateSource") != URL_WIKI:
        raise EvidenceError("Unrecognized landmark source/frame; no automatic coordinate conversion is authorized.")
    ids = set()
    for pt in points.get("landmarks", []):
        ident = pt.get("id")
        if not isinstance(ident, str) or not ident or ident in ids:
            raise EvidenceError("Missing or duplicated landmark ID.")
        ids.add(ident)
        if pt.get("reviewStatus") != "reference_only_not_1_0_verified" \
                or pt.get("independent1_0Match") is not None \
                or pt.get("visibleInNewMap") != "unconfirmed":
            raise EvidenceError("Unverified legacy landmarks must not claim 1.0 compatibility.")
        if not all(finite(pt.get(k)) for k in ("x", "y", "altitude")) \
                or not 0 <= pt["x"] <= 8191 or not 0 <= pt["y"] <= 8191:
            raise EvidenceError("Legacy XY point outside wiki coordinate frame.")
        if not isinstance(pt.get("name"), str) or not pt["name"].strip():
            raise EvidenceError("Unnamed reference landmark.")
    if len(ids) < 3:
        raise EvidenceError("Not enough independent reference landmarks to evaluate a future calibration.")
    if routes.get("format") != "shroudnav-route-observations-v1":
        raise EvidenceError("Unrecognized narrative road-topology format.")
    summaries, observed_ids = [], set()
    for route in routes.get("observations", []):
        ident = route.get("id")
        if not isinstance(ident, str) or not ident or ident in observed_ids:
            raise EvidenceError("Missing or duplicated route observation ID.")
        observed_ids.add(ident)
        if route.get("verifiedPositionCoordinates") is not False \
                or route.get("verifiedPolyline") is not False \
                or route.get("gameplayCheckedOnLiveServer") is not False:
            raise EvidenceError("Narrative routes may not masquerade as surveyed 1.0 geometry.")
        if not isinstance(route.get("through"), list) or len(route["through"]) < 2 \
                or any(not isinstance(p, str) or not p.strip() for p in route["through"]):
            raise EvidenceError("Route must specify ordered, named landmarks, not arbitrary connected coordinates.")
        if route.get("type") not in {"multi_source_topology", "single_source_topology"}:
            raise EvidenceError("Unrecognized evidence class.")
        if not isinstance(route.get("travelMode"), str) or not route["travelMode"] \
                or not isinstance(route.get("blockers"), list):
            raise EvidenceError("Travel modes and known obstructions must be explicit.")
        origins = set()
        for source in route.get("sources", []):
            url = source.get("url", "")
            parsed = urlparse(url)
            if parsed.scheme != "https" or not parsed.netloc or not parsed.path or \
                    not isinstance(source.get("what"), str) or not source["what"].strip():
                raise EvidenceError("Each source needs a concrete HTTPS page URL and described evidence.")
            origins.add(parsed.netloc.lower().removeprefix("www."))
        if route["type"] == "multi_source_topology" and len(origins) < 2:
            raise EvidenceError("Corroborated qualitative topology requires at least two different hosts.")
        if route["type"] == "single_source_topology" and len(origins) < 1:
            raise EvidenceError("Narrative route needs a cited source.")
        summaries.append({"id": ident, "class": route["type"], "sourceHosts": sorted(origins),
                          "landmarkCount": len(route["through"]),
                          "knownBlockers": len(route["blockers"]),
                          "routeGeometryVerified": False})
    if not summaries:
        raise EvidenceError("No route evidence was provided.")
    return {
        "status": "reference_topology_only_not_navigable",
        "legacyPublishedLandmarks": len(ids),
        "independentlyMatched1_0Landmarks": 0,
        "calibratedImageLandmarks": 0,
        "verifiedWorldPolylines": 0,
        "qualitativeRoutes": summaries,
        "blockersToCheck": sum(a["knownBlockers"] for a in summaries),
        "nextActions": [
            "Correlate at least three non-collinear independent 1.0 game positions to reusable map pixels.",
            "Check a fourth independent landmark against the pixel/world transform and error bound.",
            "Trace original road candidates in a legally usable raster and confirm junctions, bridges and cliffs.",
            "Survey or visually corroborate segment positions and source permissions; only then publish verified GeoJSON.",
            "Test actual 1.0 traversal on an isolated server before enabling turn-by-turn route guidance."
        ],
        "warning": "Pass means the research evidence is labelled correctly; NOT that any road polyline or 1.0 gameplay route has been verified."
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--landmarks", default=str(HERE / "research" / "legacy-landmark-control-points.json"))
    parser.add_argument("--routes", default=str(HERE / "research" / "road-topology-evidence.json"))
    parser.add_argument("--report", help="optional output report path (outside production data by default)")
    args = parser.parse_args()
    try:
        with open(args.landmarks, encoding="utf-8") as stream:
            points = json.load(stream)
        with open(args.routes, encoding="utf-8") as stream:
            routes = json.load(stream)
        report = verify_references(points, routes)
        output = json.dumps(report, indent=2) + "\n"
        if args.report:
            dest = pathlib.Path(args.report)
            dest.parent.mkdir(parents=True, exist_ok=True)
            dest.write_text(output, encoding="utf-8")
        print(output)
    except (EvidenceError, OSError, ValueError, KeyError, TypeError) as exc:
        print("ROAD EVIDENCE NOT VERIFIED: " + str(exc), file=sys.stderr)
        return 2
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
