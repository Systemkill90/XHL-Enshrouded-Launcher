# ShroudNav — Quest & Journal Companion

**Status:** Browser-only manual guide prototype, separate from server-only in-game GPS research. Does not access Enshrouded, Steam, saves, personal quest state, or server operations. No client mod installation required. No autopilot.

## What is covered
- `quest-guide.json`: Versioned structured guide entries, searched by quest title, region, key landmark and **journal title/clue**.
- `quest-guide.template.html`: Minimal dark ShroudForge-style standalone frontend.
- `quest-guide.html`: Self-contained standalone page generated from the JSON and template. Can open directly as a local file without hosting or network dependencies (linked citations require internet).
- `../tools/build_quest_guide.py`: Schema/evidence validation and reproducible HTML bundle generation.
- `../tests/test_quest_guides.py`: Source provenance, clue linking, manual progress and unsafe claim checks.

Initial sample data (not the full quest list): **3 source-backed guides with 13 steps and 10 journal/clue items**.

## Features
1. Search quest names, regions, diarists, journal names, clue interpretation, target landmarks and warnings.
2. Click a quest to see requirements, numbered directions, hazards, exact note pickup locations and confirmation steps.
3. Open **Clues & Journals** to distinguish required journal pages from optional lore, explain what a clue implies, and link back to its exact objective.
4. Mark steps complete **manually**. Browser `localStorage` only; nothing is submitted and no game account is queried. Different browsers/devices won't automatically sync progress.
5. Follow per-quest source links; uncertain titles and coordinates are explicitly labelled.

## Test/build commands
```bash
python -m unittest discover -s map-data/tests -p 'test_*.py' -v
python map-data/tools/build_quest_guide.py --check
python map-data/tools/build_quest_guide.py
```

Open `map-data/quests/quest-guide.html` in any modern browser. A static hosting option can come later, but it must not require the manager's administrative control token.

## Evidence standards
- Entries are short original paraphrases and practical instructions, not reproductions of game journal prose.
- `confidence=source-backed` means a guide, lore wiki or other documented reference supports the claim; it does not prove an in-game Enshrouded 1.0 integration test.
- `confidence=guide_only` means a reported feature (like an unnamed camp note) whose specific label needs player verification.
- X/Y values mentioned in guides are **published wiki reference values**, not calibrated ShroudNav 1.0 world coordinates. This guide does not draw routes or know the user's location.
- Never imply live status, automatically completed objectives, accurate travel ETA or GPS map markers. The in-game minimap objective is separately paused under `map-data/research/GO-NO-GO.md`.

## Next priorities
Add a cross-quest index where the journal is the starting point: enter a page title or describe its clue, find every quest where it matters, and show what to look for next. Expand only with source-backed 1.0 descriptions and tests, prioritizing confusing region searches, underground tombs, vertical cave entrances, lore-triggered objectives and multiple-story note chains.
