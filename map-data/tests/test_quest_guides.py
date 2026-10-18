import json
import pathlib
import sys
import unittest

ROOT=pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/"tools"))
from build_quest_guide import QuestGuideError,validate,render

DATA_PATH=ROOT/"quests"/"quest-guide.json"
HTML_TEMPLATE=ROOT/"quests"/"quest-guide.template.html"
GENERATED=ROOT/"quests"/"quest-guide.html"


class QuestGuideTests(unittest.TestCase):
    def setUp(self):
        self.data=json.loads(DATA_PATH.read_text(encoding="utf-8"))

    def test_all_journal_clues_resolve_to_actual_quest_steps(self):
        result=validate(self.data)
        self.assertGreaterEqual(result["questCount"],3)
        self.assertGreaterEqual(result["clueCount"],10)
        self.assertEqual(result["stepCount"],13)

    def test_in_game_quest_journals_are_distinguished_from_lore(self):
        entries={c["entry"]:c for q in self.data["quests"] for c in q["clues"]}
        self.assertIn("Martha's Diary — First Entry of Midyear, 503 D.A.",entries)
        self.assertIn("Frozen Eternity",entries)
        self.assertIn("Ramone Jones's newspaper writing",entries)
        self.assertEqual("optional",entries["Martha's other diary entry"]["importance"])
        self.assertEqual("quest-chain",entries["Ramone Jones's newspaper writing"]["importance"])

    def test_no_fake_live_gps_or_unsupported_source_assertions(self):
        self.assertTrue(self.data["noLiveTracking"])
        self.assertTrue(self.data["noClientMod"])
        for quest in self.data["quests"]:
            self.assertFalse(quest["liveTracking"])
            self.assertFalse(quest["geometryVerified"])
            self.assertGreater(len(quest["sources"]),0)
            for clue in quest["clues"]:
                self.assertIn(clue["confidence"],("source-backed","guide_only"))
                self.assertGreater(len(clue["easyToMiss"]),0)

    def test_strict_schema_blocks_invented_journal_steps(self):
        data=json.loads(json.dumps(self.data))
        data["quests"][0]["clues"][0]["stepId"]="fictional-next-step"
        with self.assertRaisesRegex(QuestGuideError,"Orphaned"):
            validate(data)
        data=json.loads(json.dumps(self.data))
        data["quests"][0]["geometryVerified"]=True
        with self.assertRaisesRegex(QuestGuideError,"verified GPS"):
            validate(data)
        data=json.loads(json.dumps(self.data))
        data["quests"][0]["clues"][0]["confidence"]="completely_verified"
        with self.assertRaisesRegex(QuestGuideError,"provenance"):
            validate(data)

    def test_bundle_is_complete_and_prevents_html_injection(self):
        template=HTML_TEMPLATE.read_text(encoding="utf-8")
        built=render(self.data,template)
        self.assertIn('"quests":[',built)
        self.assertNotIn("__QUEST_DATA__",built)
        self.assertIn("Clues & journals",built)
        self.assertIn("localStorage",built)
        self.assertIn("No installs",built)
        fake=json.loads(json.dumps(self.data))
        fake["quests"][0]["summary"]="This is unsafe </script><script>alert('xss')</script>"
        safe=render(fake,template)
        self.assertNotIn("</script><script>alert",safe)
        self.assertIn("\\u003c",safe)
        self.assertTrue(GENERATED.exists())
        self.assertIn("Martha",GENERATED.read_text(encoding="utf-8"))

    def test_missing_source_rejected(self):
        data=json.loads(json.dumps(self.data))
        data["quests"][0]["sources"]=["http://invalid-link.test"]
        with self.assertRaisesRegex(QuestGuideError,"source citation"):
            validate(data)


if __name__=="__main__":
    unittest.main(verbosity=2)
