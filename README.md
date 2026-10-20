<p align="center">
  <img src="assets/shroudforge-realm.png" alt="Dark fantasy landscape of Velaris, the realm behind ShroudForge" width="100%" />
</p>

<h1 align="center">ShroudForge</h1>
<p align="center"><strong>A better way to run your Enshrouded world.</strong><br/>A Windows control room for dedicated-server management, backups and carefully tested gameplay mods.</p>

<p align="center">
  <a href="docs/start/README.md"><strong>Get started</strong></a> &nbsp;·&nbsp;
  <a href="docs/start/features.md"><strong>Explore features</strong></a> &nbsp;·&nbsp;
  <a href="docs/project/CURRENT-STATE.md"><strong>Current status</strong></a> &nbsp;·&nbsp;
  <a href="docs/areas/README.md"><strong>Browse the project</strong></a>
</p>

<p align="center">
  <a href="https://github.com/Systemkill90/XHL-Enshrouded-Launcher/releases"><img alt="Public installer: not released" src="https://img.shields.io/badge/Public%20installer-Not%20released-5c6265?style=flat-square" /></a>
  <a href="docs/project/README.md"><img alt="Development: active" src="https://img.shields.io/badge/Development-Active-708e83?style=flat-square" /></a>
  <a href="docs/operate/README.md"><img alt="Public server: owner controlled" src="https://img.shields.io/badge/Velaris-Public%20server-b49a68?style=flat-square" /></a>
</p>

---

## Welcome to ShroudForge

**ShroudForge is built for people who want to play Enshrouded—not spend their evening wrestling with server files.** Its goal is to put server status, safe backups, updates, player information and compatible mods in one clear Windows application.

> [!IMPORTANT]
> **There is no supported public installer yet.** The owner has a working public server, but the latest 2.0 code is still on development branches. You can explore the code and previews without touching your own installation.

> **Continuing ShroudForge development?** The shared GitHub record for phone ChatGPT, desktop Work and Codex starts at [Current state](docs/project/CURRENT-STATE.md), then [Session handoff](docs/project/SESSION-HANDOFF.md). [Decisions](docs/project/DECISIONS.md), [work history](docs/project/WORK-LOG.md), and [issues](docs/project/ISSUE-REGISTER.md) are kept separately from the installed/live server. Conversations themselves are not automatically copied into GitHub.

## Find your way

| | Your starting point | |
| :-- | :-- | :-- |
| 🏁 | **[I'm new here](docs/start/README.md)** | Understand ShroudForge and how a future installation will work |
| ✨ | **[What can it do?](docs/start/features.md)** | Explore features without the technical jargon |
| 🔎 | **[What's finished?](docs/project/README.md)** | See working, experimental and unreleased features |
| 🧱 | **[Explore the project](docs/areas/README.md)** | Find the launcher, mods, historical research and development branches |
| 🛡️ | **[Protect your server](docs/operate/README.md)** | Public-server safeguards, saves, testing and rollback |
| 🛠️ | **[Develop or contribute](docs/developers/README.md)** | Build instructions, source links and contribution guidance |

## The project at a glance

| Area | What we're building | Explore |
| :-- | :-- | :-- |
| **Desktop launcher** | Dark-fantasy control room, server status, settings and backups | [Open launcher area](docs/areas/launcher.md) |
| **Gameplay mods** | Build-specific flight, pitch-hold, gifting and stacking adjustments | [Open mods area](docs/areas/mods.md) |
| **Archived quest/map research** | Earlier experiments retained for reference; GPS development is canceled | [Review archived research](docs/areas/navigation.md) |
| **Live-server safety** | Reliable updates and recovery without treating player progress as disposable | [Open operations](docs/areas/operations.md) |

<details>
<summary><strong>Where's the actual source code?</strong></summary>

The default `main` branch is the public project homepage and documentation. **The latest 2.0 launcher source lives on the [development branch](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/tree/feat/launcher-2-overhaul/launcher-2-preview)** while review continues in [PR #18](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/18).

Code files and some historical internal names still say `XHL` for build compatibility. The project is publicly branded **ShroudForge**. We aren't renaming source folders in a way that would break the working server.

</details>

<details>
<summary><strong>What about gameplay mods and quest GPS?</strong></summary>

Flight, gifting and stacking work exists for specific game versions. They're not universal drop-in downloads.

The GPS / quest-navigation replacement was **canceled** on October 19. [Quest-tracker research](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/24) and [map-data work](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/21) remain historical experiments, not a current or installable feature. The phone companion is paused.

</details>

## Built around a real public server

Velaris is an actively used Enshrouded world. **Source changes are not live changes:** merging a PR or passing a GitHub workflow never authorizes restarts, overwriting saves, installing new mod loaders or editing players' progress.

The [public-server operating model](docs/operate/README.md) explains the principles. The [detailed release-safety work](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/25) is tracked separately.

---

<p align="center">
  <a href="docs/README.md">Documentation</a> &nbsp;·&nbsp;
  <a href="ROADMAP.md">Roadmap</a> &nbsp;·&nbsp;
  <a href="https://github.com/Systemkill90/XHL-Enshrouded-Launcher/issues/new/choose">Request a feature / report an issue</a> &nbsp;·&nbsp;
  <a href="https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pulls">Development progress</a>
</p>

<p align="center"><sub>Independent fan project · Not affiliated with Keen Games, Steam or Valve · No proprietary game binaries, live saves or private credentials distributed</sub></p>
