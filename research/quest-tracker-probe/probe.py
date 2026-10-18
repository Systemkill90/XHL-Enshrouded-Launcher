#!/usr/bin/env python3
"""Build an isolated, exact-build EML localization transmission proof.

Input is the user's own LOCAL Enshrouded localization JSON exported by EML.
Never touches any installed game/server path, saves, Steam or the network.
Only a single selected localization ID is rebuilt. The generated mod is NOT
a proven way to change the vanilla client; its purpose is to establish that.
"""
import argparse
import hashlib
import json
import pathlib
import re
import sys
import zipfile
from collections import Counter

HERE = pathlib.Path(__file__).resolve().parent
TEMPLATE = HERE / "probe.lua.template"
PACKAGE_NAME = "ShroudNav-Quest-Text-Probe"
MAX_INPUT = 25_000_000
LOCALE = "En_Us"
TOKEN = re.compile(r'%(?:\d+\$)?[-+#0-9.]*[sdifuF]|\{[a-zA-Z_][^{}]{0,70}\}|</?[a-zA-Z][^<>]{0,40}>')


class ProbeError(ValueError):
    pass


def read_export(path):
    path = pathlib.Path(path).resolve(strict=True)
    if path.stat().st_size > MAX_INPUT:
        raise ProbeError("Localization export exceeds size limit.")
    try:
        data = json.loads(path.read_text(encoding="utf-8-sig"))
    except (ValueError, UnicodeError) as e:
        raise ProbeError("Malformed UTF-8 localization JSON") from e
    version = data.get("version")
    if not isinstance(version, dict):
        raise ProbeError("Missing exact original game build")
    build = version.get("build")
    if not isinstance(build, int) or isinstance(build, bool) or not 1 <= build <= 999999999:
        raise ProbeError("Missing valid numeric game build")
    if data.get("language") != LOCALE:
        raise ProbeError("Selected localization is not En_Us")
    entries = data.get("entries")
    if not isinstance(entries, dict) or not 1 <= len(entries) <= 500000:
        raise ProbeError("Missing localization tag dictionary")
    if any(not isinstance(k, str) or not k.isdecimal() or
           not isinstance(v, str) for k, v in entries.items()):
        raise ProbeError("Localization must map decimal tag IDs to strings")
    return build, entries


def scan(entries, words, limit=25):
    terms = [s.casefold() for s in words if s.strip()]
    if not terms:
        terms = ["quest", "journal", "find", "search", "tomb", "ruins"]
    matches = ((key, value) for key, value in entries.items()
               if any(t in value.casefold() for t in terms))
    return sorted(matches, key=lambda pair: int(pair[0]))[:limit]


def lua_bytes_literal(value):
    # Decimal escape (fixed 3 digits) is unambiguous in Lua 5.4 and cannot
    # inject code, quotes, Unicode controls or template markers.
    return '"' + ''.join('\\%03d' % b for b in value.encode('utf-8')) + '"'


def compile_proof(build, entries, tag_id, template, marker=" [SF SERVER TEST]"):
    if not isinstance(tag_id, str) or not tag_id.isdecimal() or not 1 <= len(tag_id) <= 16:
        raise ProbeError("Specify one numeric localization ID from the actual game export")
    original = entries.get(tag_id)
    if not isinstance(original, str) or not original.strip() or len(original) > 200:
        raise ProbeError("Tag is missing, empty or too long for a safe single-ID test")
    if any(x in original for x in ("\r", "\n", "\0")):
        raise ProbeError("Multi-line/control text is not suitable for the proof")
    if marker in original or len(original) + len(marker) > 240:
        raise ProbeError("Marker already present or would make text too long")
    revised = original + marker
    if Counter(TOKEN.findall(original)) != Counter(TOKEN.findall(revised)):
        raise ProbeError("Game text placeholders/markup would be changed")
    placeholders = {
        "__BUILD__": str(build),
        "__ID__": tag_id,
        "__ORIGINAL__": lua_bytes_literal(original),
        "__REPLACEMENT__": lua_bytes_literal(revised),
        "__LANGUAGE__": LOCALE,
    }
    script = template
    for needle, value in placeholders.items():
        if script.count(needle) != 1:
            raise ProbeError("Probe template is missing/duplicating " + needle)
        script = script.replace(needle, value)
    if "__BUILD__" in script or "__ORIGINAL__" in script:
        raise ProbeError("Incomplete generated Lua script")
    manifest = {
        "id": "shroudnav_quest_text_proof",
        "name": "ShroudNav — Isolated Server Quest Text Proof",
        "version": "0.1.0",
        "capabilities": ["patch"],
        "authors": ["ShroudForge research"],
        "description": "One ID changed on one exact server build, disposable test only; no client mod required",
    }
    report = {
        "status": "COMPILED_NOT_GAMEPLAY_VERIFIED",
        "gameBuild": build,
        "language": LOCALE,
        "tagId": tag_id,
        "originalLength": len(original),
        "originalSha256": hashlib.sha256(original.encode("utf-8")).hexdigest(),
        "generatedChange": "append visible test marker without changing placeholders",
        "clientDisplayVerified": False,
        "serverOnly": True,
        "noSaveChanges": True,
        "target": "isolated server clone, never live Velaris",
    }
    return script, manifest, report


def package(folder, build, contents):
    folder = pathlib.Path(folder)
    folder.mkdir(parents=True, exist_ok=True)
    target = folder / (PACKAGE_NAME + "-" + str(build) + ".zip")
    if target.exists():
        raise ProbeError("Refusing to overwrite an existing probe package")
    with zipfile.ZipFile(target, "x", zipfile.ZIP_DEFLATED) as z:
        for name, content in contents.items():
            z.writestr("shroudnav_quest_text_proof/" + name, content)
    return target


def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--export", required=True, help="local En_Us.json localization dump created by EML")
    ap.add_argument("--scan", nargs="*", metavar="KEYWORD", help="search own export for candidate quest text")
    ap.add_argument("--tag-id", help="one observed numeric localization tag ID to test")
    ap.add_argument("--output-dir", help="folder for generated isolated-test zip")
    args = ap.parse_args()
    try:
        build, entries = read_export(args.export)
        if args.scan is not None:
            for tag, value in scan(entries, args.scan):
                print(tag + "  " + value.replace("\t", " ")[:130])
            print("Scanned export for build " + str(build) + "; no game files modified.")
            return 0
        if not args.tag_id or not args.output_dir:
            raise ProbeError("Pass --scan or both --tag-id and --output-dir")
        template = TEMPLATE.read_text(encoding="utf-8")
        script, manifest, report = compile_proof(build, entries, args.tag_id, template)
        usage = (
            "SHROUDNAV QUEST TEXT TRANSMISSION TEST — NOT A GAME MOD RELEASE\n\n"
            "This package modifies a single localization tag on an EXACT-build\n"
            "DISPOSABLE server clone, using the community EML patch loader.\n"
            "NEVER install it in your working Velaris server or normal client.\n"
            "EML proxy loaders can conflict with other mods. Use one isolated loader\n"
            "and keep the dedicated-server binary and KFC files backed up.\n"
            "Before: inspect the same existing quest text on an UNMODIFIED client.\n"
            "After: start isolated server with this patch and connect the same unmodified\n"
            "client. Record whether the visible marker appears in the IN-GAME tracker.\n"
            "Restore the cloned server from pristine files afterward.\n"
            "No effect in the vanilla client = NO-GO for this delivery route.\n"
            "The package contains no copied game binaries or localization catalog.\n"
        )
        contents = {
            "mod.json": json.dumps(manifest, indent=2) + "\n",
            "src/mod.lua": script,
            "TEST-ONLY.txt": usage,
            "evidence.json": json.dumps(report, indent=2) + "\n",
        }
        path = package(args.output_dir, build, contents)
        print(json.dumps({"package": str(path), "proof": report}, indent=2))
        return 0
    except (ProbeError, OSError, UnicodeError, ValueError) as e:
        print("QUEST TRACKER PROOF BLOCKED: " + str(e), file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
