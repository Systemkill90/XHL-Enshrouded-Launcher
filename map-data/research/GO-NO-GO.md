# ShroudNav viability gate — 2026-10-17

## Decision: STOP server-only in-game minimap/GPS implementation

**Do not continue adding road datasets or navigation mechanics with an in-game HUD as the goal until a verified server-only display channel exists.** Existing work in this draft branch is research and offline tooling, not a near-shippable GPS.

### User requirements that must be satisfied together
- Unmodified Enshrouded game client; no player-side mods, DLLs, injected overlays, installers, or software dependencies.
- Responsive in-game minimap and visible quest GPS/turn arrow while walking, approaching a Google Maps experience.
- Manual steering and adjustable flight responsiveness/speed; explicitly NO autopilot.
- Don't modify the owner's proven live Velaris installation in pursuit of research.

### Evidence behind this decision
1. **Client rendering constraint.** A currently working Enshrouded 1.0 minimap mod explicitly runs client-side and draws into the game client. A server-only route database cannot force a vanilla game client to display novel HUD elements.
   - https://www.nexusmods.com/enshrouded/mods/143
   - https://www.nexusmods.com/enshrouded/mods/97
2. **Server-side location is feasible in principle but not currently integrated.** GameGrid reports that its own privately installed server module reads positions because the normal dedicated server does not report them. Loafhosts documents a server-read live map tested on 0.9, with 1.0 explicitly unverified. These are external browser/admin maps, not vanilla-client minimaps.
   - https://wdcgamegrid.com/knowledge-base/games/enshrouded/enshrouded-live-map (verified 2026-10-01)
   - https://loafhosts.com/guides/enshrouded-live-map/ (checked on 0.9, October 13, 2026)
3. **Road data alone is insufficient.** Precise coordinates, path feasibility (cliffs, caves, bridges, Shroud hazards), optional quest location/selection, and a display channel must all work together. The repo has only reference landmark data, cited narrative corridors, offline topology/calibration tools, and synthetic fixture coverage. **0 verified 1.0 road polylines and 0 independently calibrated 1.0 landmarks.**
4. **No known server-supported HUD channel.** The official documented dedicated-server config contains server/game settings, not an API for creating new UI widgets on unmodified clients.
   - https://enshrouded.zendesk.com/hc/en-us/articles/16055441447709-Dedicated-Server-Configuration

### Alternative: route-first mapping as a conditional side project
A lighter *journey graph* can connect named landmarks (e.g. Bridge → barricade → Spire), include travel abilities, impassable spans, and warnings without extracting all roads from raster imagery. This is useful as **human-readable external directions**, not a continuously updating in-game GPS. It is **not** a workaround for the client HUD constraint or unavailable 1.0 position feed.

### Strict restart gates — require real evidence, not a hopeful inference
Resume in-game GPS development **only if** one of:
- A tested server-only mechanism demonstrably causes the unmodified Enshrouded 1.0 client to render suitable minimap/quest directions (without asking players to install/alter anything); OR
- The owner explicitly changes scope to allow a separate private browser/phone navigation page (no player install), where route-first mapping may be useful.

For the browser route, still require an approved map license, measured pixel/world calibration, a reviewed route pilot, and either confirmed read-only position telemetry or a clear manual starting-point fallback. Test it on a disposable server and preserve player privacy.

### Scope we should retain
- Existing GitHub data section, offline source/catalog, importer, synthetic road graph, geometry evidence and test reports are a reusable research archive.
- Existing flight and pitch-hold mods are working and remain unchanged.
- Do NOT merge PR #21 or imply any live deployment.

**Bottom line:** Under today's strict requirements, stop spending development time on the in-game GPS and focus on feasible mods unless the technical conditions change.
