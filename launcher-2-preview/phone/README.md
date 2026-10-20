# ShroudForge phone companion — updating without stopping Velaris

The phone controls are hosted by ShroudForge Launcher on the Windows machine. They are not a separately installed iOS App Store binary.

## Current installation and one-time upgrade

The original launcher compiles its phone HTML inside the Windows executable. **Changing the source on GitHub does not update a running launcher.** The next launcher build supports a hot-reloadable file at `phone/phone-ui.html` next to its executable.

The **initial changeover** to that build requires exiting **only ShroudForge Launcher**, using its tray menu item **Exit launcher (server stays running)**, and reopening the updated launcher from the same installation folder. This does **not** invoke the Start, Stop, Restart, update-activation, restore, or mod controls on Enshrouded. The phone page and launcher automation are briefly unavailable; the game server keeps running independently. Wait for any in-progress save/backup operation to complete before exiting the launcher, and confirm the Enshrouded process ID is unchanged afterward.

The automatically produced Windows artifact is an **unsigned preview**, not a live-deployment-approved installer; do not blindly replace a working launcher with it or copy its compatibility scripts over a live server. Test the intended desktop build first, preserve the original executable and settings, and use a separately validated launcher build for live changeover.

To observe the game process PID on the Windows host before and after the launcher-only changeover:

```powershell
Get-Process enshrouded_server | Select-Object Id,StartTime,Path
```

Do not issue `Stop-Process`, restart the game, or run the launcher with `--restart-server`. Reopening the GUI normally only reconnects its telemetry/phone controls to the already-running server.

## Future phone-only updates (no launcher restart)

Once the file-loading version of ShroudForge Launcher has been installed and validated:

1. In the launcher's directory, find the `phone` folder and its `phone-ui.html` file.
2. Download the reviewed version of [phone-ui.html](phone-ui.html) and replace **only** that HTML file. Keep an old copy for rollback.
3. Refresh your phone's ShroudForge private-network page. The phone server reads that HTML from disk for each request, so both the Windows launcher and the Enshrouded game can remain running.

If this file is missing or can't be read, the launcher uses its embedded fallback page. Invalid/partial HTML is not automatically repaired; write a temporary file and rename it into place for safer replacements.

Only the HTML is hot-swappable; changing the phone API, auth controls, player fields, or manager functionality still requires a rebuilt launcher and launcher-only restart.

## Embedded map on iPhone — tested October 19, 2026

The owner confirmed that the GameVault 3D map renders **inside the iPhone test window** when the iframe's `sandbox` attribute is absent (Compatibility mode). The stricter protected iframe displayed a white screen. This is an observed iPhone-browser result, **not** proof that all devices or browsers behave the same.

**New staged behavior:** loading the map in the ShroudForge phone tab defaults to Compatibility. The user still initiates map loading; neither the map nor any of its third-party resources are requested until then. Choose **Use Protected mode** in the map controls to restore stricter iframe restrictions, or **Map blank?** for troubleshooting and a direct map link. Compatibility mode avoids opening a large diagnostic overlay over a normally working map.

**Security tradeoff:** removing `sandbox` gives the third-party map iframe more browser capabilities. It remains cross-origin, keeps `referrerpolicy="no-referrer"` and a fixed provider URL, and cannot read the ShroudForge parent document through normal same-origin browser policy. External map requests disclose normal network/browser metadata to the provider, not the ShroudForge phone access token, server commands or player records. Open the map only if you trust the provider.

A [standalone iPhone map test](map-embed-test.html) also starts in Compatibility mode; it has no private server URL or control functionality. Public GitHub files or a generated Windows preview ZIP do **not** update the installed live phone page. Validate the new launcher in an isolated directory before any host update, and follow the phone-only hot-swap instructions above for a compatible installed launcher.

## Security and map limitations

Keep the unique phone URL confidential and accessible only through your private LAN. The page communicates with the local ShroudForge endpoint and loads the third-party Embervale map on request. The map provider may block framing, in which case use **Open full map**. Live character coordinates are not available from the current telemetry. No dedicated native iPhone app, public endpoint, external tunnel, or map-data redistribution is created by this update.
