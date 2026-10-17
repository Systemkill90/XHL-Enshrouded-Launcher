# Features, without the jargon

Here's what the Enshrouded server companion is intended to help with. The details below distinguish what's in the current **development preview** from what we still want to build.

## Control the server

Start, stop, and restart your dedicated server from a normal Windows window. See if it is online and whether startup completed. The development preview has these controls, and the owner's October 16 notes describe a successful start, graceful stop, and restart.

## See what's happening

See server uptime, activity, status, and available player information without opening a pile of log files. The preview contains a dashboard and a recent-activity view. **Exact in-game player locations and character activities cannot be promised** without trustworthy game data.

## Protect your world

Make separate copies of world saves and configuration, keep previous copies, and prepare updates without immediately deleting the old server. A backup is only useful if restoring it works, so we're continuing interrupted-operation and real-world consistency tests.

## Keep your server customized

The current development version includes specific work on:
- **Flight:** a game-build-specific pitch-control adjustment; actual movement/camera behavior needs player testing.
- **Gifting distance:** work to keep item gifting functional with the current server build.
- **Item stacks:** current-build resources prepared for larger stack sizes, with single-stack items retained.

These are **not universal plug-and-play mods**. Game updates can change compatibility. Features such as flight stamina switches will only be offered once the underlying mod supports them reliably.

## Update more safely

Instead of overwriting a working installation immediately, the preview can prepare another copy, check supported compatibility, and select it after validation. A complete real SteamCMD download and unknown future game versions are separate tests, still outstanding.

## Optional phone controls

The preview offers a page on your **private home network** for status and simple actions. Host-side tests have passed; real phone access still needs hands-on confirmation. Do not port-forward the control page to the public internet.

## Planned additions

A cleaner guided first launch, friendlier server settings, easier troubleshooting, clearer mod compatibility, richer player/session views, a complete backup/restore experience, and a polished release installer.

See [Current status](STATUS.md), [Getting started](GETTING-STARTED.md), and the [roadmap](../ROADMAP.md).
