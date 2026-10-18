# ShroudForge Launcher

Windows launcher and control room for Enshrouded dedicated servers, formerly XHL Enshrouded Launcher. The historic folder and repository names remain for compatibility. Current source: [launcher-2-preview](launcher-2-preview).

## Production philosophy: one live public server

**Velaris is a real public server with players, and the owner's gameplay testing occurs there.** Treat source development, GitHub CI, operator desktop inspection, and public deployment as distinct stages. This project must never treat the owner's running installation as a disposable sandbox.

No GitHub push, merged PR, passing CI run, uploaded artifact or day on a calendar authorizes a game update, mod injection, restart, save restoration, directory migration or live change. Owner authorization for the exact change and maintenance window is required. Unknown connected-player counts must not be treated as zero.

**Sunday, October 18 desktop handoff:** [Owner's live-server handoff](docs/DESKTOP-HANDOFF-2026-10-18.md). It is staged for a desktop connection later today; **no deployment is scheduled or automatic**.

- [Public-server operating model](docs/PUBLIC-SERVER-OPERATING-MODEL.md)
- [Current staged changes and statuses](ops/change-register.json)
- [Release/change status definitions](docs/CHANGE-REGISTER.md)
- [Preflight (read-only; review the script before running)](ops/Read-Only-Host-Preflight.ps1)
- [Build and artifact policy](docs/BUILD-ARTIFACTS.md)

## Current launcher source features

- Dashboard with observed CPU/memory trends, player count, uptime and world-save records.
- Grouped settings, source-backed player history and protected installation checks.
- Backups, guarded offline restores, update staging and private-LAN phone controls.
- Splash, tray and restrained dark-fantasy interface.

**Scope disclaimer:** A source feature or passing isolated test is not evidence the currently installed manager/server contains that code or that an Enshrouded multiplayer session validated it. For historical isolated test results, see [overhaul notes](launcher-2-preview/OVERHAUL.md). The active public server must be inspected before any new deployment.

## Build and isolated validation

On Windows with the .NET Framework compiler:

```powershell
./launcher-2-preview/Build.ps1
./launcher-2-preview/Test.ps1
./launcher-2-preview/Test-Native.ps1
```

On any machine with Python 3:

```bash
python ops/validate_release_register.py
python -m unittest discover -s ops/tests -p 'test_*.py' -v
```

These commands run source validation and fixture/native tests; none is an authorization to change Velaris. **Do not install test artifacts into the running server just because they compile.** Never commit game files, modded proprietary KFC resources, live world backups, credentials, or unsanitized player data.

## Development branches and release status

Experimental work stays in separate draft branches/PRs. The [change register](ops/change-register.json) tracks launcher overhaul (#18), player sheets (#19), QoL (#20), pitch hold (#17), road/GPS research (#21), rejected browser guide (#22, closed) and vanilla-client quest tracker feasibility (#24). All require owner review and relevant proof before inclusion in a public-server update.

## Further documentation

- [Roadmap](ROADMAP.md)
- [Contributing](CONTRIBUTING.md)
- [Security](SECURITY.md)
- [Preview technical notes](launcher-2-preview/README-PREVIEW.md)

Unofficial fan project. Not affiliated with Keen Games, Steam, or Valve.
