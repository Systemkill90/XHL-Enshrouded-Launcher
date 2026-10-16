# XHL 2.0 — revised scope (2026-10-16)

## Ground truth
Owner has a functional Enshrouded dedicated server with three mods working, mostly: XHL Flight, max stack 65535, unlimited gifting. Do NOT replace server, reorganize live directory, or change injection/bootstrap during UI work.

## Independent workstream A — Flight fixes
Preserve tested Flight v1.1.0 as fallback. Test anti-inversion pitch limiter at 78/80/82/85 degrees (cap <=89), default target 82. Then implement gliding-only stamina suppression, holding last requested pitch on neutral input, and responsive smoothing. These three require validated runtime hooks and may be client-side; never mark verified unless tested in game. Feature-flag separately, roll back independently. Re-test gifting and stacking after each build. Full details are in earlier XHL Flight v2 engineering/test plan.

## Independent workstream B — launcher/UI overhaul
Replace launcher chrome/splash/visual hierarchy, but continue invoking EXACT existing start/stop/bootstrap scripts once imported and audited. Build Velaris dashboard, clear Server, Players, Flight, Mods, World, Saves, Logs, Settings navigation; readable realtime event console, health badges tied to real checks, distinct icon, animated startup, character/inventory UI as READ ONLY placeholders until verified API. Separate ui presentation from server control actions. Never fabricate location/event telemetry.

## Gates
1. Obtain functioning v1.0.5 launcher source and working Flight v1.1.0 patch + mod code / binary compatibility data.
2. Build UI in isolation using mocked data, then connect to proven commands.
3. Flight engineering separately and optional activation only after game tests.
4. Verify Windows build, zero regression to three mods and existing saves.
