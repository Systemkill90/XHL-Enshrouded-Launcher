# Verification — 2026-10-16

## Completed

- x64 .NET Framework build; 59 fixture/UI checks plus 7 native fixture checks.
- All eight pages rendered, with narrow overview bounds checked.
- The actual current executable is build 1269579, SHA256 B627639CE4A3F8C940FE90AFA1D36ED59DB3FF9CED24E935890EB7C99B04D5BF.
- Offline audit of current unmodified resources found 0 stackable entries at 65535 and 2267 below it. Stacking was rebuilt in a copy, then independently read back: 2267 at 65535, 0 below, and 1995 single-stack entries preserved.
- Flight lower-bound instruction relocated to RVA 0x7CF5E. Disassembly confirms the lower comparison and existing +80-degree min clamp. The new displacement references the existing -80-degree constant at 0xBD893C. Exact executable, original/replacement bytes, clamp instructions and both constants are checked before changes. No shared constant, speed, stamina, archive or save data is modified by Flight.
- On an isolated real game process: original Flight status, enable/readback, disable/restoration, re-enable all passed.
- Gifting getter relocated to RVA 0x931830 with the single direct call at 0x198389; distance-check call context and constants checked. Runtime application passed on the isolated process.
- The full copied safe-start sequence verified a backup, validated stacking, started the game, applied gifting and Flight, and reported the process alive after 10 seconds. The server loaded copied world data and reached Run.
- Actual game-world shutdown observed after Ctrl+Break. Current-build handler supports that signal; new runtime sends it directly for this exact executable. Unknown/older builds retain Ctrl+C. The launcher does not force-kill or claim stopped while a process persists.
- All 16 game archive copies were hash verified; an incomplete copy was replaced and rechecked before the final launch-sequence test.
- Current-build update preparation profile tested against a clean copied resource set; final hashes match the prepared profile. The local resource bundle is excluded from public source.
- Update selection exercised with actual current save copies: separate imported save directory, configuration preserved from the source, byte verification, compatibility passed. The prepared package is selected and running in the normal PC session.
- Original installation files checked against the original 120-file baseline, including 22 saves; no original file edits were made.

## Remaining operator checks

- Extended multiplayer Flight/camera testing. The operator reports all three mods working in-game; target bounds do not guarantee every client-side camera/animation cannot overshoot.
- Phone access from an actual handset remains unverified; Windows network permission awaits the operator.
- SteamCMD download end-to-end. Offline preparation and update activation were tested independently; unknown future builds are deliberately unsupported.
- Multiplayer load, DPI/multiple monitors, and live-backup cross-file game consistency.

No game binaries/assets, saves, configuration, runtime logs or backup contents belong in the public PR. The local resource bundle is pinned for this exact build; future builds require fresh compatibility work.

## Host quality-of-life verification

- Normal-session graceful stop completed and current configured saves were backed up before replacement; old launcher and startup script retained for rollback.
- Restart connected to Steam and reached Host_Online; all three mod startup checks passed. No separate console window appeared.
- Production launcher title and sidebar labels replace test labels.
- Phone tab offers a private LAN control link; dashboard and status endpoint passed HTTP checks, including rejection of wrong keys, missing command headers, foreign origins, GET commands and unknown commands. The running launcher returned HTTP 200 with online status and recent logs on the host.
- Open the Phone tab link on the same home Wi-Fi. Leave the launcher open. Allow Private networks in the Windows prompt; no router forwarding is needed. Remote-phone connectivity has not been claimed as tested.


- Phone page now provides Start, Stop and Restart with browser confirmation and operation feedback; logs are omitted. Commands reuse guarded lifecycle operations and intentional Stop disarms recovery. A live duplicate Start returned already-running without restarting the server. Actual handset Stop/Restart still awaits operator testing.


- Expanded mobile dashboard: uptime, resources, observed online players, world file metadata, known player history, backup status/action, and persisted automation switches. Renamed desktop Reset / Restart to Restart. Player history is not represented as proof of individual save files.
- Mobile layout checked at 390px with no horizontal overflow or JavaScript errors. Live backup command created a verified snapshot while the same server process remained online. Existing automation settings were preserved.

