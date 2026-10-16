# Verification record — 2026-10-16

## Passed

- Compiled with the installed Windows .NET Framework x64 compiler; final build has no warnings.
- Missing executable, unknown build, empty/malformed attestation all fail verification.
- A matching local attestation cannot override the pinned Flight executable hash.
- Legacy loader presence is detected without renaming or deleting the loader.
- A changed KFC invalidates the stacking file check.
- All six pages construct and render; rendered layout inspected at 1400 px and overview also at 1100 px window width.
- A synthetic save file retains its SHA256 after all page and audit checks.
- No server logs or runtime artifacts are created in the synthetic server folder.
- 120 original installation files checked against the pre-edit backup matched, including the 22 server save files. Development edits are outside that installation.

## Installation findings

The installed executable, KFC and KFC resources differ from the old stacking attestation. The executable also differs from the existing Flight/gifting controllers' pinned build. This is a compatibility finding, not proof that gameplay currently fails. Existing working files were preserved.

The current build's runtime memory and multiplayer behavior were not tested. No production server was started, stopped, patched, updated or repaired. Player saves outside the server save folder were not scanned or modified.

## Still unverified / incomplete

- Agreed Flight-v2 balance requirements and the necessary hooks on the installed executable.
- Flight, gifting and stacking gameplay on the current build.
- Production start/stop/restart integration in the redesigned interface.
- Player table under live multiplayer load; network timing accuracy.
- High-DPI/multiple-monitor behavior and user acceptance of the visual direction.

The UI preview is intentionally labeled as such. Build success and offline signature checks are not described as gameplay verification.
