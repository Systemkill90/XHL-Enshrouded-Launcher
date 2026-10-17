# What works, and what is still being built?

**Checked:** October 17, 2026.

## The important distinction

There was a **working build on the project owner's Windows server as of the October 16 session**. The development notes on [the 2.0 pull request](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/1) describe successful normal-host start/restart, Steam connection, and three game modifications loading.

That does **not** mean today's 2.0 additions, experimental flight tweaks, or every player-facing feature have been validated. It also does **not** mean a downloadable public version exists.

| Area | Status |
| --- | --- |
| Existing server and mods | Working host build reported on October 16; preserve as rollback |
| Launcher 2.0 code | On a separate [draft review branch](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/1), not merged into the main branch |
| Basic start, stop and restart | Documented as tested on the owner's host |
| Stacking, gifting and flight patches | Current-build startup checks and some isolated runtime tests documented; extended player behavior still needs testing |
| Safe backup copies | Implemented and verified in some tests; full restore/failure scenarios still need coverage |
| Phone management | Desktop-host page and security checks documented; actual phone test outstanding |
| Game update workflow | Offline staging tested; online end-to-end update test outstanding |
| Friendly installer and public download | Not available yet |
| Character inventory management and player locations | Exploratory; unavailable until reliable underlying support is confirmed |

## Why code isn't all on the front page yet

The main GitHub branch holds the public project information. The more recent code is preserved in [draft pull request #1](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/1) rather than overwriting a known-working setup without the remaining validation.

The older v1.0.5 source archive and an early PowerShell foundation are **historical recovery sources**, not the currently preferred development code. They are documented separately for development and must not silently replace the newer branch.

## What happens next?

1. Keep the current working installation available for rollback.
2. Run automated Windows checks on the newer development code.
3. Confirm flight, gifting, stacking, backups, updates and phone controls in normal use.
4. Publish a clearly labeled, installable package after its release criteria are met.

[See the feature guide](FEATURES.md) · [Track release requirements](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/issues/3) · [Back to the main page](../README.md)
