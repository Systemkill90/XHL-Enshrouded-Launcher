"""No-game, no-server math fixtures for an inscribed 45-degree altar design.

These tests do not establish an Enshrouded placement or protection mod.
"""
import math
import pathlib
import sys
import unittest

HERE = pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0, str(HERE))
from diamond_footprint import (
    FootprintError, SUPPORTED_SIZES, SUPPORTED_YAW, ascii_view,
    fully_inside_footprint, grid_stats, plan_footprint, snap_yaw
)


class FlameAltarDiamondDesignTests(unittest.TestCase):
    def test_yaw_choices_are_multiples_of_45_while_staying_flat(self):
        self.assertEqual(tuple(range(0, 360, 45)), SUPPORTED_YAW)
        for yaw in SUPPORTED_YAW:
            p = plan_footprint(40, yaw)
            self.assertEqual(yaw, p["yawDegrees"])
            self.assertEqual(0, p["pitchDegrees"])
            self.assertEqual(0, p["rollDegrees"])
            self.assertTrue(p["worldGridRemainsAxisAligned"])
            self.assertFalse(p["actualPlacementControlsAltered"])
            self.assertFalse(p["actualServerTerritoryAltered"])
        self.assertEqual(315, snap_yaw(-45))
        self.assertEqual(45, snap_yaw(405))
        with self.assertRaises(FootprintError):
            snap_yaw(30)

    def test_square_and_diamond_have_two_distinct_footprints(self):
        for side in SUPPORTED_SIZES:
            shapes = [plan_footprint(side, yaw) for yaw in SUPPORTED_YAW]
            for yaw, p in zip(SUPPORTED_YAW, shapes):
                self.assertEqual(side * side / (2 if yaw % 90 else 1),
                                 p["intendedPlanAreaGridUnitsSquared"])
                self.assertEqual(0.5 if yaw % 90 else 1.0, p["fractionOfVanillaArea"])
                self.assertEqual("inscribed-diamond" if yaw % 90 else "vanilla-square",
                                 p["planShape"])
                self.assertEqual(side * side, p["vanillaSquareAreaGridUnitsSquared"])

    def test_inscribed_vertices_do_not_escape_original_protection_square(self):
        for side in SUPPORTED_SIZES:
            h = side / 2
            for yaw in SUPPORTED_YAW:
                for x, z in plan_footprint(side, yaw)["coordinates"]:
                    self.assertLessEqual(abs(x), h)
                    self.assertLessEqual(abs(z), h)
                    if yaw % 90:
                        self.assertAlmostEqual(h, abs(x) + abs(z), places=8)

    def test_boundary_uses_all_four_cell_corners_not_only_center(self):
        self.assertTrue(fully_inside_footprint(-0.5, -0.5, 40, 45))
        # Center (10, 10) lies on diamond boundary, but this 1-unit cell extends outside.
        self.assertFalse(fully_inside_footprint(9.5, 9.5, 40, 45))
        self.assertFalse(fully_inside_footprint(19.5, -0.5, 40, 45))
        self.assertTrue(fully_inside_footprint(10, 10, 40, 0))
        self.assertFalse(fully_inside_footprint(19.5, 19.5, 40, 0))

    def test_grid_cells_fit_inside_safe_design_footprint(self):
        for side in SUPPORTED_SIZES:
            plan = grid_stats(side, 45)
            self.assertLess(plan["wholeOneUnitCells"], side*side/2)
            self.assertGreater(plan["wholeOneUnitCells"], 0)
            self.assertLess(plan["fractionOfWholeCells"], 0.5)
            self.assertFalse(plan["wholeCellCountIsAnEstimateOfInGameBlocks"])
            square = grid_stats(side, 0)
            self.assertEqual(side*side, square["wholeOneUnitCells"])

    def test_illegal_sizes_nan_and_invalid_cell_sizes_blocked(self):
        for side in [0, 20, 50, -40, 100.0, "40"]:
            with self.assertRaises(FootprintError):
                plan_footprint(side, 45)
        for yaw in [float("nan"), float("inf"), "45", 22.5]:
            with self.assertRaises(FootprintError):
                snap_yaw(yaw)
        with self.assertRaises(FootprintError):
            fully_inside_footprint(0, 0, 40, 45, 0)
        with self.assertRaises(FootprintError):
            fully_inside_footprint(float("inf"), 0, 40, 45)
        with self.assertRaises(FootprintError):
            ascii_view(40, 45, 1000)

    def test_ascii_preview_has_diamond_taper(self):
        view = ascii_view(40, 45, 20).splitlines()
        self.assertEqual(20, len(view))
        self.assertLess(view[0].count("◆"), view[9].count("◆"))
        self.assertEqual(20, len(view[0]))


if __name__ == "__main__":
    unittest.main(verbosity=2)
