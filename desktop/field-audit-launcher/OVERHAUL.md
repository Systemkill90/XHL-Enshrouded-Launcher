# ShroudForge Launcher 2.0

The Windows control room now includes a restrained dark-fantasy dashboard, an informative animated startup splash, a dedicated icon, tray access, player history, world details, settings, verified backups and guarded offline restore, archive organization, update staging, and private LAN phone controls.

The launcher attaches to the existing server path and configured world. Starting and restarting retain the established safety wrapper and its verified backup/mod checks. Closing ShroudForge Launcher leaves the server running; phone access and scheduled copies require the manager to remain open or in the tray. One manager instance per selected server path is enforced.

## Current interface

The ShroudForge name and gateway/ember emblem appear in the sidebar, splash, app icon, tray and phone interface. Velaris remains the server name. The dashboard combines a compact status banner, player/process metrics, real CPU and memory history, world save records, verified snapshot counts, backup schedule and direct navigation. Offline charts explicitly report unavailable live data. Server rules use category navigation and individual setting cards instead of a primary editable grid. Unsaved card edits do not touch disk. Protected folders and unverified settings are explicitly read only. Numeric controls enforce pinned-build limits, display allowed ranges, and retain out-of-range saved values until explicitly corrected. Save-time validation also rejects incompatible numbers and enum options in Advanced JSON.

## Safety and data

- Game executable, resources and world locations are preserved.
- Settings edits retain prior configuration and preserve unknown game options.
- Restore requires a verified offline snapshot, validates every manifest hash, backs up the current world, stages new bytes, and retains the old world directory. Live copies are not automatically treated as coherent game snapshots.
- Organization moves only hash-verified, unreferenced historical launcher ZIP archives; game files, unknown files, mods, saves and active logs stay put. Moves are journaled and rolled back on failure.
- Unknown game builds and changed pinned controllers fail closed. The updated current-build preparation profile includes the additional pitch-hold controller.
- Player counts distinguish valid zero replies from unavailable or malformed roster responses. Historical identities are not claimed to be independently editable player saves. Client character inventories cannot be safely edited by this manager.
- New data goes in `Manager Data`; legacy settings are copied forward without removing their original.

## Validation on October 17, 2026

146 fixture/UI checks passed, covering all 12 pages, narrow dashboard bounds, lifecycle guards, phone authentication, full configuration, friendly rule names, offline save and rotated-log history, settings preservation, corrupt/running/live-snapshot restore rejection, exact successful restore, old-world retention, and conservative archive organization. Seven native lifecycle checks passed. The final application passed exact-build compatibility checks on the owner's installation; its authenticated phone endpoint returned the live realm and accepted a real backup request. Existing Flight and pitch-hold memory Status remained ACTIVE. Game executable/resource hashes were unchanged.

The app window was discovered under its production name. Native screenshot capture timed out; visual review used the actual application controls rendered by fixture tests. Artwork is shared by the splash and dashboard. The splash appears during startup and checks the selected folder, configuration and pinned mods; its steps do not start the game.

Remaining external validation: actual-phone access through the owner's Windows network permission, player flight handling, broad multiplayer/DPI coverage and a real SteamCMD download of a future game build. These are not claimed as tested. The local application is usable; this document does not declare a signed public installer or general release.

## Build

Run `Build.ps1` on 64-bit Windows with the .NET Framework compiler. `Test.ps1` and `Test-Native.ps1` run isolated checks. The build copies the icon/artwork; CI archives them and the compatibility scripts alongside its test executable. Proprietary game resources, personal settings, saves and backups are excluded from public source and CI artifacts.


