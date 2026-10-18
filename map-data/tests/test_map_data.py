#!/usr/bin/env python3
"""Synthetic fixture tests. No game/client, network, proprietary assets or servers."""
import json
import pathlib
import subprocess
import sys
import tempfile
import unittest

HERE = pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0, str(HERE / "tools"))
from calibrate import validate, apply, CalibrationError
from build_graph import build, MapDataError, node_key
from mask_to_candidates import trace_skeleton, make_geojson, thin
from poi_index import index_pois


def feature(pts, proof="verified", source="owner-manual", level="surface", road_id="r1"):
    props = {"roadId": road_id, "roadName": "Test only", "proof": proof, "roadType": "road",
             "level": level, "sourceId": source}
    if proof == "verified":
        props.update({"reviewedBy": "fixture-reviewer", "reviewedAt": "2026-10-17"})
    return {"type": "Feature", "geometry": {"type": "LineString", "coordinates": pts},
            "properties": props}


def collection(features, frame="enshrouded-world-xy-calibrated-v1", image=None):
    return {"type": "FeatureCollection", "coordinateSystem": frame,
            "sourceImageId": image, "features": features}


class NavigationDataTests(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.root = pathlib.Path(self.tmp.name)
        self.files = {}
        for name, content in (
            ("verified", collection([])),
            ("candidates", collection([], "source-image-pixels")),
            ("obstacles", collection([])),
            ("catalog", json.loads((HERE / "sources" / "catalog.json").read_text(encoding="utf-8"))),
            ("calibration", json.loads((HERE / "calibration" / "calibrations.json").read_text(encoding="utf-8")))):
            path = self.root / (name + ".json")
            path.write_text(json.dumps(content), encoding="utf-8")
            self.files[name] = path

    def tearDown(self):
        self.tmp.cleanup()

    def write(self, name, obj):
        self.files[name].write_text(json.dumps(obj), encoding="utf-8")

    def run_build(self, include=False):
        return build(*[self.files[x] for x in ("verified", "candidates", "obstacles", "catalog", "calibration")],
                     include_candidates=include)

    def test_repository_starts_empty_and_not_ready(self):
        doc, report = self.run_build()
        self.assertEqual("blocked_no_verified_roads", report["status"])
        self.assertEqual(0, len(doc["Links"]))
        self.assertEqual(0, report["exportedNodes"])

    def test_verified_route_preserves_intermediate_junctions(self):
        self.write("verified", collection([
            feature([[0, 0], [10, 0], [10, 10]], road_id="road-north"),
            feature([[10, 0], [20, 0]], road_id="road-east")]))
        graph, report = self.run_build()
        self.assertEqual(3, report["exportedSegments"])
        self.assertEqual(4, report["exportedNodes"])
        self.assertEqual(1, report["connectedComponents"])
        self.assertTrue(all(e["Proof"] == "verified" for e in graph["Links"]))
        self.assertEqual("ready_for_review", report["status"])

    def test_stacked_bridge_crossing_does_not_connect(self):
        self.write("verified", collection([
            feature([[0, 0], [10, 0]], level="surface", road_id="ground"),
            feature([[5, -5], [5, 0], [5, 5]], level="bridge", road_id="bridge")]))
        graph, report = self.run_build()
        self.assertEqual(2, report["connectedComponents"])
        self.assertEqual(5, report["exportedNodes"])

    def test_at_grade_crossing_requires_manual_junction_or_layer_review(self):
        self.write("verified", collection([
            feature([[0, 0], [10, 10]], road_id="cross-a"),
            feature([[0, 10], [10, 0]], road_id="cross-b")]))
        graph, report = self.run_build()
        self.assertEqual("blocked_topology_review", report["status"])
        self.assertTrue(report["unjoinedCrossingWarnings"])
        # Explicit raised bridge layer means these two trails must not be joined.
        self.write("verified", collection([
            feature([[0, 0], [10, 10]], road_id="cross-a", level="surface"),
            feature([[0, 10], [10, 0]], road_id="cross-b", level="bridge")]))
        graph, report = self.run_build()
        self.assertEqual("ready_for_review", report["status"])
        self.assertFalse(report["unjoinedCrossingWarnings"])

    def test_candidate_not_trusted_by_default(self):
        self.write("verified", collection([feature([[0, 0], [10, 0]])]))
        self.write("candidates", collection([feature([[10, 0], [20, 0]], proof="candidate", road_id="guess")]))
        graph, report = self.run_build()
        self.assertEqual(1, len(graph["Links"]))
        self.assertEqual(1, report["candidateRoadFeatures"])
        self.assertEqual(0, report["exportedCandidateSegments"])
        graph, report = self.run_build(include=True)
        self.assertEqual(2, len(graph["Links"]))
        self.assertEqual(1, report["exportedCandidateSegments"])

    def test_unknown_source_or_unlicensed_source_blocks_export(self):
        self.write("verified", collection([feature([[0, 0], [2, 2]], source="enshrouded-database-map")]))
        with self.assertRaisesRegex(MapDataError, "unapproved"):
            self.run_build()
        self.write("verified", collection([feature([[0, 0], [2, 2]], source="invented-site")]))
        with self.assertRaisesRegex(MapDataError, "Unknown road provenance"):
            self.run_build()

    def test_obstacle_removes_exact_blocked_link(self):
        self.write("verified", collection([feature([[0, 0], [10, 0], [20, 0]])]))
        blocked = {"type": "Feature", "geometry": {"type": "LineString", "coordinates": [[10, 0], [20, 0]]},
                   "properties": {"level": "surface", "status": "confirmed", "reviewedBy": "tester",
                                  "reason": "Collapsed bridge"}}
        self.write("obstacles", collection([blocked]))
        graph, report = self.run_build()
        self.assertEqual(1, len(graph["Links"]))
        self.assertEqual(1, report["appliedObstacles"])

    def test_invalid_evidence_and_degenerate_roads(self):
        self.write("verified", collection([feature([[2, 2], [2, 2]])]))
        with self.assertRaisesRegex(MapDataError, "Degenerate"):
            self.run_build()
        missing = feature([[0, 0], [10, 0]])
        missing["properties"].pop("reviewedBy")
        self.write("verified", collection([missing]))
        with self.assertRaisesRegex(MapDataError, "reviewer"):
            self.run_build()

    def test_calibration_requires_independent_landmark_and_finite_error(self):
        # Known synthetic pixel -> map transform X=2*x+100, Y=-3*y+500.
        training = [{"pixel": [x, y], "world": [2*x+100, -3*y+500]}
                    for x, y in [(0, 0), (100, 0), (0, 100), (50, 70)]]
        entry = {"id": "synthetic", "status": "approved", "sourceImageId": "fixture-image",
                 "targetFrame": "enshrouded-world-xy-calibrated-v1",
                 "trainingPoints": training,
                 "holdoutPoints": [{"pixel": [30, 90], "world": [160, 230]}],
                 "maxHoldoutError": 0.01}
        transform, stats = validate(entry)
        self.assertAlmostEqual(160, apply(transform, [30, 90])[0], places=4)
        self.assertAlmostEqual(230, apply(transform, [30, 90])[1], places=4)
        self.assertLess(stats["maxHoldoutError"], 0.01)
        bad = dict(entry, status="pending")
        with self.assertRaises(CalibrationError):
            validate(bad)
        bad = dict(entry, holdoutPoints=[{"pixel": [30, 90], "world": [180, 230]}])
        with self.assertRaises(CalibrationError):
            validate(bad)

    def test_pixel_candidate_requires_exact_source_image_calibration(self):
        self.write("verified", collection([feature([[0, 0], [10, 0]])]))
        self.write("candidates", collection([feature([[0, 0], [10, 0]], proof="candidate")],
                                           "source-image-pixels", image="wrong-map"))
        with self.assertRaisesRegex(MapDataError, "exact traced source"):
            self.run_build()

    def test_mask_extraction_produces_only_unverified_geojson_candidates(self):
        road = {(x, 3) for x in range(15)}
        skeleton = thin(road)
        polylines = trace_skeleton(skeleton, min_length=8)
        self.assertEqual(1, len(polylines))
        feature_collection = make_geojson(polylines, "synthetic-mask", "owner-manual")
        self.assertEqual("source-image-pixels", feature_collection["coordinateSystem"])
        self.assertEqual("candidate", feature_collection["features"][0]["properties"]["proof"])
        self.assertEqual("unverified-cv-candidate", feature_collection["features"][0]["properties"]["reviewStatus"])

    def test_quest_destination_links_only_to_nearby_reviewed_road(self):
        self.write("verified", collection([feature([[0, 0], [10, 0]])]))
        graph, report = self.run_build()
        catalog = json.loads(self.files["catalog"].read_text(encoding="utf-8"))
        def marker(identifier, x, y, source="owner-manual", status="verified"):
            return {"type": "Feature", "geometry": {"type": "Point", "coordinates": [x, y]},
                    "properties": {"id": identifier, "name": identifier, "category": "quest",
                                   "status": status, "sourceId": source, "reviewedBy": "reviewer"}}
        data = collection([marker("near", 9, 1), marker("distant", 200, 200)])
        result = index_pois(graph, data, catalog, max_snap=4)
        self.assertEqual(1, len(result["Records"]))
        self.assertEqual("near", result["Records"][0]["id"])
        self.assertEqual(1, len(result["Unreachable"]))
        self.assertIn("does not prove walkability", result["Records"][0]["warning"])
        data["features"][0]["properties"]["sourceId"] = "enshrouded-database-map"
        with self.assertRaisesRegex(MapDataError, "redistribution approval"):
            index_pois(graph, data, catalog, max_snap=4)

    def test_cli_blocks_publish_when_no_world_road_data(self):
        command = [sys.executable, str(HERE / "tools" / "build_graph.py"), "--publish",
                   "--verified", str(self.files["verified"]), "--candidates", str(self.files["candidates"]),
                   "--obstacles", str(self.files["obstacles"]), "--catalog", str(self.files["catalog"]),
                   "--calibration", str(self.files["calibration"]),
                   "--output", str(self.root / "published.json")]
        result = subprocess.run(command, capture_output=True, text=True)
        self.assertEqual(2, result.returncode)
        self.assertFalse((self.root / "published.json").exists())


if __name__ == "__main__":
    unittest.main(verbosity=2)
