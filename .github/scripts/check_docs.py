#!/usr/bin/env python3
"""Validate the human-facing Markdown guide, local links and archived redirects.

Does not check external URLs and doesn't access any servers or game files.
"""
from pathlib import Path
import re
import sys
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parents[2]
LINK = re.compile(r'!?\[[^\]]*\]\(([^)\s]+)(?:\s+["\x27][^)\x27"]*["\x27])?\)')
HTML = re.compile(r'\b(?:src|href)="([^"]+)"')

ESSENTIAL = (
    "README.md", "CONTRIBUTING.md", "SECURITY.md", "ROADMAP.md",
    "docs/README.md", "docs/start/README.md", "docs/start/features.md",
    "docs/operate/README.md", "docs/project/README.md",
    "docs/developers/README.md", "docs/developers/building.md",
    "docs/areas/README.md", "docs/areas/launcher.md",
    "docs/areas/mods.md", "docs/areas/navigation.md",
    "docs/areas/operations.md", "docs/archive/README.md",
    "docs/archive/recovery-inventory.md",
    "assets/shroudforge-realm.png",
)
LEGACY_LINKS = (
    "docs/GETTING-STARTED.md", "docs/FEATURES.md", "docs/STATUS.md",
    "docs/SERVER-SAFETY.md", "docs/FOR-DEVELOPERS.md",
    "docs/DEVELOPMENT.md", "docs/RECOVERY-INVENTORY.md",
)

def verify():
    problems = []
    for name in ESSENTIAL + LEGACY_LINKS:
        if not (ROOT / name).is_file():
            problems.append("Missing essential document/asset: " + name)
    markdown = [ROOT / name for name in ("README.md", "CONTRIBUTING.md", "ROADMAP.md", "SECURITY.md")]
    markdown.extend((ROOT / "docs").rglob("*.md"))
    for file in markdown:
        if not file.is_file():
            continue
        text = file.read_text(encoding="utf-8")
        for match in list(LINK.finditer(text)) + list(HTML.finditer(text)):
            dest = match.group(1).strip()
            if not dest or dest.startswith(("#", "http://", "https://", "mailto:", "data:")):
                continue
            relpath = unquote(dest.split("#")[0].split("?")[0])
            if not relpath:
                continue
            path = (file.parent / relpath).resolve()
            if not path.is_relative_to(ROOT):
                problems.append(str(file.relative_to(ROOT)) + ": link leaves repository: " + dest)
            elif not path.exists():
                problems.append(str(file.relative_to(ROOT)) + ": broken local link: " + dest)
    if problems:
        print("\n".join(problems), file=sys.stderr)
        return 2
    print("PASS: {} Markdown pages checked; essential sections, backward-compatible links and artwork present.".format(len(markdown)))
    return 0

if __name__ == "__main__":
    raise SystemExit(verify())
