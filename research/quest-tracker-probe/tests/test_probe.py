import copy
import json
import pathlib
import sys
import tempfile
import unittest
import zipfile

ROOT = pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT))
from probe import ProbeError, compile_proof, lua_bytes_literal, package, read_export, scan

TEMPLATE = (ROOT / "probe.lua.template").read_text(encoding="utf-8")
FIXTURE = {
    "version": {"build": 1269579, "branch": "synthetic", "date": "2026-10-17"},
    "language": "En_Us",
    "entries": {
        "123456": "Quest: Reach the old ruined tower",
        "456789": "Journal clue: the hidden doorway",
        "555555": "Recover %s from the cavern",
        "888888": "This is not the quest being changed.",
    },
}


class ProbeTests(unittest.TestCase):
    def test_can_scan_synthetic_export_without_game_connection(self):
        matches = scan(FIXTURE["entries"], ["quest", "journal"])
        self.assertEqual(["123456", "456789"], [item[0] for item in matches])

    def test_localization_reader_checks_build_locale_and_decimal_ids(self):
        with tempfile.TemporaryDirectory() as td:
            inp = pathlib.Path(td) / "synthetic.json"
            inp.write_text(json.dumps(FIXTURE), encoding="utf-8")
            build, entries = read_export(inp)
            self.assertEqual(1269579, build)
            self.assertEqual(FIXTURE["entries"], entries)
            wrong = copy.deepcopy(FIXTURE)
            wrong["language"] = "Fr_Fr"
            inp.write_text(json.dumps(wrong), encoding="utf-8")
            with self.assertRaisesRegex(ProbeError, "not En_Us"):
                read_export(inp)
            wrong["language"] = "En_Us"
            wrong["version"]["build"] = "1269579"
            inp.write_text(json.dumps(wrong), encoding="utf-8")
            with self.assertRaisesRegex(ProbeError, "valid numeric"):
                read_export(inp)

    def test_exact_one_id_and_original_text_guard_in_lua(self):
        code, manifest, report = compile_proof(1269579, FIXTURE["entries"], "123456", TEMPLATE)
        self.assertIn("local TARGET_BUILD = 1269579", code)
        self.assertIn('local TARGET_ID = "123456"', code)
        self.assertIn("if target.text ~= ORIGINAL then", code)
        self.assertIn("game.assets.get_resources_by_type", code)
        self.assertIn("write_resource", code)
        self.assertIn("create_content", code)
        self.assertIn("dataHash =", code)
        self.assertIn("target.text = REPLACEMENT", code)
        self.assertEqual(["patch"], manifest["capabilities"])
        self.assertEqual(False, report["clientDisplayVerified"])
        self.assertEqual(1269579, report["gameBuild"])
        self.assertNotIn("Quest: Reach the old ruined tower", code)  # bytes escaped, not raw literal

    def test_unicode_and_scripting_metacharacters_cannot_inject_lua(self):
        sample = "Æ \"\\\n quest <b>tomb</b> \u2603"
        code = lua_bytes_literal(sample)
        self.assertEqual('"', code[0])
        self.assertEqual('"', code[-1])
        self.assertNotIn("\n", code)
        self.assertNotIn("<b>", code)
        self.assertNotIn("quest", code)

    def test_compiler_never_invents_localization_id(self):
        with self.assertRaisesRegex(ProbeError, "missing"):
            compile_proof(1269579, FIXTURE["entries"], "999999", TEMPLATE)
        with self.assertRaisesRegex(ProbeError, "numeric"):
            compile_proof(1269579, FIXTURE["entries"], "999x", TEMPLATE)

    def test_multiline_entry_is_not_used(self):
        data = dict(FIXTURE["entries"])
        data["111111"] = "one line\nsecond line"
        with self.assertRaisesRegex(ProbeError, "Multi-line"):
            compile_proof(1269579, data, "111111", TEMPLATE)

    def test_recompile_is_reproducible_and_zip_does_not_touch_game_files(self):
        code, manifest, report = compile_proof(1269579, FIXTURE["entries"], "555555", TEMPLATE)
        with tempfile.TemporaryDirectory() as td:
            a = package(td, 1269579, {
                "mod.json": json.dumps(manifest),
                "src/mod.lua": code,
                "evidence.json": json.dumps(report),
            })
            self.assertTrue(a.exists())
            with zipfile.ZipFile(a) as z:
                names = z.namelist()
                self.assertEqual(3, len(names))
                self.assertIn("shroudnav_quest_text_proof/src/mod.lua", names)
                self.assertIn("shroudnav_quest_text_proof/mod.json", names)
                script = z.read("shroudnav_quest_text_proof/src/mod.lua").decode("utf-8")
                self.assertIn("TARGET_BUILD = 1269579", script)
                self.assertNotIn("target.text = ORIGINAL", script)
            with self.assertRaisesRegex(ProbeError, "Refusing to overwrite"):
                package(td, 1269579, {"mod.json": json.dumps(manifest)})

    def test_compiler_rejects_missing_script_placeholders(self):
        with self.assertRaisesRegex(ProbeError, "missing/duplicating"):
            compile_proof(1269579, FIXTURE["entries"], "123456", "not a Lua template")


if __name__ == "__main__":
    unittest.main(verbosity=2)
