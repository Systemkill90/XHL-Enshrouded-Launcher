# Where we're going

> **For the current implementation rather than the original planning baseline, use [CURRENT-STATE.md](docs/project/CURRENT-STATE.md) and [SESSION-HANDOFF.md](docs/project/SESSION-HANDOFF.md).** The merged UI/player/event source is staged in [draft PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31); compilation and tests do not mean it was installed. The October 19 iPhone iframe Compatibility bug fix is a narrowly approved exception to the general phone-app pause.

**Planning updated October 19, 2026.** This is a plain-English roadmap, not a list of features already installed on Velaris. See the [dated development tracker](docs/project/development-tracker.md) for provisional percentages and activity dates.

| Focus | What success looks like | Status |
| :-- | :-- | :-- |
| **A reliable desktop launcher** | One clear home for server status, settings, activity, backups and updates | 3.0.0-private.2 acceptance candidate |
| **Protect live player progress** | Exact version checks, reversible changes and safer recovery | Ongoing release-process review |
| **Smooth, consistent flight** | Keep the working flight, gifting and stacking mods stable through game updates | Build-specific |
| **Clearer multiplayer information** | Event activity and honest player/session history without invented data | Observed profiles/events in unified candidate |
| **Quest/map research (historical)** | Preserve exploratory work as reference; do not pursue the canceled GPS concept | Removed from active scope |
| **Public releases** | Downloadable, tested Windows packages with checksums and setup instructions | Not released |

## Right now

Recovered modern desktop source is reconciled in draft PR #31. Both Windows builds, 247 local fixtures and native dummy checks passed; physical-device, native DPI and disposable-game acceptance remain. No deployment or public release.

The immediate priority is finishing the **launcher UI and user experience**, then auditing feature claims against the actual development branch and running Velaris host. The owner reported the server and three mods working on October 19; that does not verify proposed branch changes. Preserve the live server, validate any changes safely, and keep experiments separate from approved public releases. The companion phone app is paused, and flame altar rotation was canceled.

Want the details? [Project status](docs/project/README.md) · [Live branches and areas](docs/areas/README.md) · [Open development work](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pulls) · [Main page](README.md)

> [!NOTE]
> A passing GitHub Action proves only the test it ran. It is not evidence that a feature has been played, installed on Velaris, or released to other hosts.
