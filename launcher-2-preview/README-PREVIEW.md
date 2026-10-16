# Launcher 2.0 — controls and automation preview

Build with `./Build.ps1` on Windows with .NET Framework 4.x. Run the executable with `--controls` to enable guarded operations, or without arguments for monitoring only. The supplied XHL Server Controls shortcut uses controls mode. Keep this folder outside the server installation.

- Start uses the existing safe-start wrapper. Reset / Restart means graceful server restart, never world or character reset. Stop requests Ctrl+C and never force-kills the server.
- Start and restart require matching build and mod checks. The inspected installation does not pass those checks; these operations are currently blocked there.
- Automation offers scheduled backups (5–1440 minutes, default 30) and recovery 60 seconds after a running process disappears. Options are off by default and work only while the launcher is open in controls mode. Stop disables recovery. Recovery makes one attempt per observed running session, subject to compatibility checks. A stop initiated outside this launcher cannot be distinguished from a crash; disable recovery before externally stopping the server.
- Back up now copies the configured server save directory and server configuration to World-Backups beside this launcher. Every file is SHA256 checked, the source file list is rechecked, and source bytes are checked again after copying. No originals or older backups are deleted. Copies made while the game is running are not transactional snapshots and may span game save cycles. Client-side character saves stored elsewhere are not included.
- Each backup starts with INCOMPLETE.txt. Only a VERIFIED.txt marker confirms completed byte verification and supersedes that initial marker. Failed/incomplete copies remain available for inspection and must not be treated as verified backups. There is no restore action.
- Update (stage) accepts an official steamcmd.exe and copies the downloader into a unique workspace. It downloads app 2278520 into a separate Staged-Updates folder, records output and checks completion evidence. It never deploys into the live installation. A real download was not tested; current network restrictions prevented downloading additional tooling. No SteamCMD installation was located at the known locations. The three mods need build-specific verification before deployment can be added safely.

## Flight requirement

The requested behavior is balanced flight with a vertical limit below 90 degrees. An 85-degree target would provide a margin, but is not yet implemented. Offline inspection found one candidate matching the historical instruction pattern in the current executable, shifted from the old address. This does not verify the hook's gameplay semantics or prove a hard pitch clamp. No new Flight patch was deployed or published. Existing Flight, gifting and stacking files remain untouched.

## Verification and rollback

Run `./Test.ps1` and `./Test-Native.ps1`; they use separate fixtures and never launch the real server. See VERIFICATION.md for actual results and remaining limitations. To stop using this build, close it and use the untouched original launcher. Closing disables its automation; it does not stop the game server. The original wrapper has its own stale-attestation restriction, so returning to it does not resolve compatibility.

Never upload World-Backups, Staged-Updates, launcher runtime data, local configuration or logs. Public source is the explicit source package only.
