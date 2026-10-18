# Maps, roads & quest tracking

**Goal:** Better ways to navigate and track quests *inside Enshrouded*, without requiring players to install client-side mods.

### Two separate research paths

| Research | Where to look | What is proven? |
| :-- | :-- | :-- |
| **Road and map data** | [ShroudNav map work (#21)](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/21) | Offline routing tools and reference data; **not** a verified in-game GPS |
| **Vanilla quest-tracker changes** | [One-text proof (#24)](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/24) | Test compiler and CI passed; client-visible server-only changes **not proven** |

The rejected browser-only guide is [archived as closed PR #22](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/22) because it didn't solve the actual in-game tracker problem.

> [!CAUTION]
> An unmodified Enshrouded client may simply ignore custom server-supplied text or HUD instructions. Until a real vanilla-client proof succeeds, **don't describe either branch as an installable feature** or put it into a public-server update.

[← Explore project areas](README.md)
