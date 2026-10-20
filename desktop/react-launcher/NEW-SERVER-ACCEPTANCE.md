# New-server setup acceptance — disposable machine / folder

**Prepared:** October 20, 2026. **Current implementation:** `ServerSetup.cs`, modern New server guide, read-only discovery, separate world/configuration, and guarded registration. Use this checklist to finish validation **without touching the running Velaris installation**.

## Validated automatically in CI

- Create requires an **empty new folder** separate from launcher and existing game roots; no file or save migration.
- Reject missing SteamCMD path, invalid server name, invalid admin password, invalid port, invalid slots **before creating the new destination**.
- A SteamCMD success string without zero exit, or zero exit without recognized installation success, must not register a partially downloaded server.
- Failed creation leaves partial files/marker in the new folder for review; never treats it as a verified game installation.
- Read-only bounded scans report invalid configuration, duplicate installations and scan truncation; server switching isolates profile-dependent backup/recovery settings and does not start/stop the prior server.
- Existing modded installations are not silently enrolled as standard ones; changed standard fingerprints block startup.

Windows workflows test the implementation using **fake SteamCMD callbacks and dummy binaries**, not downloaded game files or actual players. The targeted test additions cover explicit rejection before filesystem writes and misleading download success. Report the exact workflow and results separately.

## Outstanding real-machine acceptance

| Gate | How to test safely | Pass condition |
| :-- | :-- | :-- |
| Windows dependencies | Disposable machine/VM with WebView2 and .NET prerequisites; avoid production server | Clear missing-dependency messaging, launcher starts after supported prerequisites |
| Official SteamCMD | Owner selects an official SteamCMD executable in isolated test folder | Official anonymous dedicated-server app download finishes and produces expected files |
| Network | Use a **different query UDP port** and explicit firewall/router review; no automatic public exposure | Server can be found only on intended network |
| Fresh world | Start disposable server only after separate authorization for the test target | Test client joins, saves work, owner can stop safely |
| Creation failure | Simulate unavailable network, partial download, disk full and canceled setup on disposable folder | Existing server and saves unchanged; partial folder reported, no false success |
| Native controls | Exercise Create, Scan, dialog focus, cancel, keyboard, 100/125/150/200% DPI on desktop | Controls visible/operable, no silent changes |
| Discovery and selection | Include a clean test install and intentionally invalid configuration | Correct visible warnings, no game files moved, prior server preserved |
| Rollback | Update/rollback **launcher only** in an isolated test install | Files and private manager metadata recover without modifying game state |

Never import production saves into this test casually; do not configure crash recovery to act on live Velaris. Real-game test results and user acceptance must be recorded separately from fixture CI.

## Status and next task

**Ready for independent acceptance testing; not yet independently approved for first-time production use on a new PC.** Existing owner's installed launcher is separate from this source-only branch. Maintain [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31) as draft and don't change production processes unless specifically authorized.
