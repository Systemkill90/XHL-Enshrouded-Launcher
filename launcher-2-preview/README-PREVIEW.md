# XHL Launcher 2.0 — ready for operator testing

Open XHL-Launcher-2.exe in this folder. Controls are enabled by default; --monitor opens read-only mode. Nothing starts automatically. Click Start when ready. This package selects its own prepared server under Staged-Updates/1269579/Server and uses a fresh, byte-verified copy of your current server saves.

IMPORTANT: This is a separate installation. Your original Steam server and saves remain untouched. Once you play in this copy, new progress belongs to this copy. Do not alternate between the original and this copy expecting saves to synchronize. No automatic restore or synchronization is included.

## Included

- Restrained dark UI; Start, Stop and Reset / Restart (restart never resets a world).
- Flight-v2 for build 1269579: fixed -80/+80 degree target bounds, using the verified current-build instruction and existing constants. No speed/stamina changes. Enable, status, disable, restoration and re-enable were verified on the real isolated process. Actual flight handling awaits your test.
- Current-build gifting getter relocation, verified in the isolated process.
- Rebuilt stacking resources: 2,267 stackable definitions at 65,535; 1,995 single-stack definitions preserved. Independently reopened and checked after patching.
- Optional scheduled save copies and recovery after a stopped process. Both are OFF initially. Turn them on under Automation if wanted. Recovery waits 60 seconds and makes one attempt per observed running session. Stop disables it. Turn recovery off before stopping through another app.
- Backups never overwrite originals or delete previous copies. They cover the configured server saves and configuration, not separate client-side character saves. Live copies are byte-checked but are not transactional game snapshots.
- Update (stage) uses an official SteamCMD chosen by the operator. It downloads separately. The included local resource bundle can prepare the exact supported build; unknown future builds remain blocked until their mods are verified. Use verified update copies fresh saves while stopped and switches the launcher to that prepared installation. It never replaces the prior installation.

## Testing limits

The isolated server loaded the copied world and reached Run. Its full launch sequence applied stacking, gifting and Flight successfully. The current build handles Ctrl+Break as a graceful game-world shutdown; the launcher uses that signal for this exact build and avoids repeated signals. In the restricted test environment, Steam could not connect and the process remained alive after its world shutdown. The launcher correctly refused to claim completion or restart it. Only isolated test processes were cleaned up afterward; no production process was started or stopped. Successful full process exit, public connectivity, player gifting and flight handling still need your normal-PC session.

51 fixture/native checks pass. The update preparation profile and transfer/selection routine were exercised locally. A real SteamCMD download was not possible in this environment. Reset/Restart is wired and tested for ordering/failure handling; it refuses to restart if shutdown has not completed.

## First session

1. Open this folder's XHL-Launcher-2.exe and confirm Installation checks passed.
2. Click Start. Confirm the expected world and inventory appear before playing normally.
3. Test Flight: climb toward vertical, hold/release pitch, dive, turn through headings, land and relaunch. The target bounds are 80 degrees, leaving 10 degrees before vertical; in-game behavior still needs confirmation.
4. Verify stacking and gifting with expendable items. If anything is wrong, Stop and retain both installations. Do not copy old saves over new progress.

Closing the launcher does not stop the server. For rollback before any new play, close it and use the original launcher; the original installation remains exactly as found, including its update-related mod issues. After new play, preserve the new saves before any migration. Never upload this package's game files, backups, configuration or logs to GitHub.
