# ShroudForge World HUD — isolated research prototype

**Status: staged source prototype; NOT an Enshrouded in-game mod, NOT connected to game telemetry, NOT tested on a live server.**

Owner request (October 20, 2026): top-right minimap, game weather, and in-world clock, delivered only by the dedicated server, without players downloading client mods.

## Feasibility boundary

A dedicated Enshrouded server **cannot install new HUD code into unchanged player game clients**. The existing open-source Enshrouded minimap uses Shroudtopia hooks and draws through the **client Vulkan renderer**. A weather-and-clock variant is also client-side. A server-hosted browser companion can be served to players without a mod download, but it is **not** a top-right HUD inside Enshrouded.

This prototype takes the only honest server-hosted/no-player-mod route: a lightweight local HTTP browser HUD surface. An actual game-corner overlay would require independently installed client code or an officially supported game UI-mod distribution feature (neither is established).

Existing GPS/quest-routing development remains canceled. This new experiment adds neither quests, routes, nor autopilot.

## What is staged

- `ShroudForge.WorldHud`: standalone ASP.NET Core 8 web host, bound to **127.0.0.1:8765** by default.
- A compact upper-right dark-fantasy circular map panel plus world time and weather indicator.
- Read-only `GET /api/hud` file-backed state adapter, and `GET /health` HTTP readiness endpoint.
- Explicit unknown/stale states when a game telemetry adapter isn't connected.
- Validation of weather labels, 24-hour time, map URL, world coordinates and bounds.
- No loader, hook, game binary, player data, admin functionality or server install changes.
- JavaScript unit tests and an opt-in GitHub build/test workflow.

**Missing before an actual gameplay feature:** game-time API, weather API, player-coordinates API, legally usable calibrated map asset, authorized connector, standalone host acceptance testing, client/game compatibility proof. No such APIs are assumed to exist.

## Development run — disposable machine only

Requires **.NET SDK 8**. In a development checkout from the repo root:

```sh
dotnet run --project experiments/world-hud/ShroudForge.WorldHud/ShroudForge.WorldHud.csproj
```

Open `http://127.0.0.1:8765/` on **that same machine**. The weather, world time and player location should say **Unknown**, **--:--**, and **Position unknown** until real telemetry is provided. They are *not* Windows time or outdoor weather.

Run tests with Node.js 20+:

```sh
node --test experiments/world-hud/tests/hud.test.mjs
dotnet build experiments/world-hud/ShroudForge.WorldHud/ShroudForge.WorldHud.csproj -c Release
```

The checked-in `state/example.json` is stale, synthetic and **not loaded automatically**. To preview with synthetic readings, copy it to a development-only state path and replace `updatedAt` with the current UTC time; then set `SHROUDFORGE_WORLDHUD_STATE` to that path **before starting** the host. This verifies display behavior only, not Enshrouded integration.

A future *verified and authorized* telemetry bridge would atomically write UTF-8 JSON updates to the configured state file. The host checks freshness (30 seconds) and rejects malformed/oversized data; it does not discover player/time/weather data from the dedicated game server on its own.

### Expected snapshot

```json
{
  "updatedAt": "2026-10-20T00:00:00Z",
  "source": "verified-game-adapter-name",
  "worldTime": "14:20",
  "weather": "rain",
  "player": { "x": 0, "z": 0, "heading": 45 },
  "map": {
    "imageUrl": "/maps/embervale.png",
    "bounds": { "minX": -100, "maxX": 100, "minZ": -100, "maxZ": 100 }
  }
}
```

These coordinates and bounds are **schema examples only**. Do not infer actual Enshrouded world extents from them. Place a lawfully usable map image under `wwwroot/maps/` and calibrate pixel bounds before claiming a true minimap.

Weather vocabulary: `clear`, `cloudy`, `rain`, `snow`, `blizzard`, `fog`, `storm`. Unsupported values are shown as Unknown.

## Security / live-server limits

- Default bind is loopback-only; no authentication, TLS, authorization checks, or port firewall configuration is implemented. **Do not set** `SHROUDFORGE_WORLDHUD_BIND` to a LAN/public interface or put the service on the internet without an approved authenticated HTTPS gateway.
- The HUD has no game-admin endpoints. Never write snapshots containing passwords, tokens, Steam identifiers or other private player information into this public repository.
- Source staging and CI do not authorize deployment. Do not touch the live game server, its mods, game loader, or save files.
- Do not package third-party Vulkan hooks, extracted game art, or unknown-license game assets as original ShroudForge code.

## References (research, not copied or bundled)

- `elxokker/Enshrouded-minimap` (MIT): https://github.com/elxokker/Enshrouded-minimap — C++ native client renderer, current reference version 0.4.46 at review.
- Enshrouded Minimap and Weather Clock, third-party client mod. Consult upstream licensing and current compatibility before using any code.
- This implementation is original ASP.NET/HTML/CSS/JS; **no third-party mod source or asset is included**.

## Next safe step

1. Check GitHub workflow and actual build/test outcomes.
2. Evaluate whether game telemetry can expose real time/weather/positions without touching or destabilizing the running server.
3. If no such provider exists, decide between a browser companion (no game client download, not in-game) or a client-side in-game mod (requires client installation).
4. Test on a disposable server/game installation before requesting any production deployment.

**Staging as a draft PR does not complete the requested in-game HUD.**
