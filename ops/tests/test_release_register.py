import copy
import json
import pathlib
import sys
import unittest

ROOT=pathlib.Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "ops"))
from validate_release_register import RegisterError, validate

class PolicyTests(unittest.TestCase):
    def setUp(self):
        self.good=json.loads((ROOT/"ops"/"change-register.json").read_text(encoding="utf-8"))

    def test_live_public_changes_are_all_staged_not_deployed(self):
        result=validate(self.good)
        self.assertEqual(result["blockedFromAutoDeploy"],len(self.good["changeSets"]))
        self.assertFalse(result["hostInventoryConfirmed"])

    def test_no_auto_deployment_or_restart(self):
        for key in ("autoDeploy","autoRestart"):
            bad=copy.deepcopy(self.good)
            bad["rules"][key]=True
            with self.assertRaises(RegisterError):
                validate(bad)

    def test_unknown_roster_must_block_disruptive_operations(self):
        bad=copy.deepcopy(self.good)
        bad["rules"]["unknownPlayerCountBlocksDisruptiveWork"]=False
        with self.assertRaises(RegisterError):
            validate(bad)

    def test_closed_browser_guide_cannot_be_reclassified_for_release(self):
        bad=copy.deepcopy(self.good)
        next(e for e in bad["changeSets"] if e["pr"]==22)["phase"]="trial-candidate"
        with self.assertRaises(RegisterError):
            validate(bad)

    def test_gameplay_research_cannot_be_promoted_silently(self):
        for number in (21,24):
            bad=copy.deepcopy(self.good)
            next(e for e in bad["changeSets"] if e["pr"]==number)["phase"]="offline-validated"
            with self.assertRaises(RegisterError):
                validate(bad)

    def test_cannot_assert_fake_owner_approval(self):
        bad=copy.deepcopy(self.good)
        bad["changeSets"][0]["phase"]="release-approved"
        with self.assertRaises(RegisterError):
            validate(bad)

    def test_dependency_ids_must_exist(self):
        bad=copy.deepcopy(self.good)
        bad["changeSets"][0]["dependencies"]=["unknown-change"]
        with self.assertRaises(RegisterError):
            validate(bad)

    def test_date_and_handoff_links(self):
        model=(ROOT/"docs"/"PUBLIC-SERVER-OPERATING-MODEL.md").read_text(encoding="utf-8")
        handoff=(ROOT/"docs"/"DESKTOP-HANDOFF-2026-10-18.md").read_text(encoding="utf-8")
        self.assertIn("Sunday, October 18, 2026",handoff)
        self.assertIn("DESKTOP-HANDOFF-2026-10-18.md",model)
        self.assertNotIn("Monday, October 19, 2026",handoff)

if __name__=="__main__":
    unittest.main(verbosity=2)
