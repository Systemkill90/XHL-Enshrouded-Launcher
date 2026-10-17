# Enshrouded Server Companion

**An easier way to run and manage a dedicated Enshrouded server on Windows.**

Start your server, see what's happening, protect your world saves, and handle updates and supported mods without constantly digging through folders, commands, and configuration files.

This project is for people who **want to host and play Enshrouded**, not learn server administration just to keep their game running.

> **Development status — October 17, 2026:** A working installation has been reported on the project owner's Windows host. Today's 2.0 improvements are under development. The [current 2.0 source and testing notes](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/1) are on a separate review branch, not yet a public release. **There is no downloadable installer here yet.**

## What can it do?

| Feature | In plain English | Where it stands |
| --- | --- | --- |
| Server controls | Start, stop, and restart your Enshrouded world from one window | Demonstrated on the owner's host; public release pending |
| Live dashboard | See whether the server is up, how long it has been running, and its activity | In the current 2.0 preview |
| Save backups | Make separate backup copies so a mistake or update doesn't wipe your world | Implemented and tested in the preview; longer-term restore testing remains |
| Mods | Help manage the current, game-version-specific flight, gifting-distance, and item-stacking changes | Reported working on the owner's server; compatibility varies by game update |
| Updates | Prepare an updated server separately before switching to it | Staging tested; full online update test outstanding |
| Phone controls | View status and use basic controls on your home network | Host endpoint tested; real-phone check outstanding |

**All controls are intended for an Enshrouded *dedicated server*, not the normal game client.** The server must run on a Windows PC. Enshrouded itself, Steam, and third-party game files are not included.

## Who is this for?

- **You host for friends:** one place to check whether your world is online and restart when needed.
- **You care about your world:** keep separate backups and avoid risky in-place update experiments.
- **You like a customized server:** keep compatible gameplay tweaks organized without treating every update like a guessing game.

You should not have to understand programming to use the finished app. More complicated technical details are separated into the [developer information](docs/FOR-DEVELOPERS.md).

## How do I get it?

**A public installer has not been released yet.** The working build on the owner's PC and the source in the development branch are not the same thing as a downloadable, supported version for everyone.

When a tested package is ready, it will appear under [Releases](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/releases) with simple setup instructions, supported game version, and known limitations. Don't download random "server manager" builds from other websites assuming they belong to this project.

[Read the setup and release guide](docs/GETTING-STARTED.md) · [Feature details](docs/FEATURES.md) · [Current status](docs/STATUS.md)

## What we're working on for 2.0

A cleaner desktop interface, safer updates and backups, a better player/activity view, server settings, and refinements to the game-version-specific mods. Features such as character inventory editing or exact player locations will only be advertised after we confirm the game provides reliable access to that data.

[See the roadmap](ROADMAP.md) · [Follow the development build](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/1) · [Report a problem or idea](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/issues)

## Frequently asked questions

**Will it change or delete my current world?**  
Simply viewing this repository changes nothing on your PC. The app is being designed to preserve existing installs and world saves, with separate update folders and backups. No software can promise zero risk: use a verified backup before switching installations.

**Do I need to know how to code?**  
The intended answer is no. A friendly installer and first-run setup are part of the release work.

**Can I manage it from my phone?**  
The current preview includes local-network phone controls, but access from an actual phone still needs confirmation. It is not intended to be exposed directly to the public internet.

**Will it work with every Enshrouded update and every mod?**  
No. Supported mods may require changes after the game updates. This project deliberately blocks unverified game builds rather than pretending everything will work.

More answers: [Getting started](docs/GETTING-STARTED.md).

---

**Independent fan project.** Not affiliated with or endorsed by Keen Games, Enshrouded, Steam, or Valve. Game names belong to their respective owners. No proprietary game assets or personal server files are distributed here.
