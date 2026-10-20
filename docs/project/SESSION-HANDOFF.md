# ShroudForge — current session handoff

## Latest: close choices, server setup, window sizing and MB units — October 20, 2026

**Installed and locally verified.** Published source [75952a5](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/commit/75952a5ec4e2461c89e6d783d289ad0ac32b5349) on `dev/server-setup-and-close-prompt` and `feat/ui-navigation-polish-20261019`; local installed commit `5f351990b92da1c4f49df5df07cb160e50438dd5` has identical tree `9e1cf8c6081b5a5d41c6e6b1a0e982e4fa9f346c`. The initial feature commit is published `e49bfcdf96f3f518e13d4dab025784ffc817a9bd` / local `49115cf009de3918b1ef214eee29872ea791fb77`, tree `0cb757f21f50985081591bd76ed4b243295ec473`. Connected-app publication preserves remote ancestry.

### Changes available in the modern launcher

- Closing while the selected server runs offers **Keep server running**, **Stop server and exit** (graceful), or **Cancel**. Failed/unknown observation warns; failed stop leaves the launcher open. Monitoring and launcher automation pause while closed. Installer handover explicitly closes only the launcher and still honors busy guards.
- Server selector, Tools → Servers and System page provide bounded read-only discovery and separate standard-server creation. Creation requires a separate empty folder, an official SteamCMD selected by the owner, and confirmed app download. It writes current role-based configuration and leaves the new world unstarted. Existing/nonempty/overlapping destinations are rejected; partial failures stay in the new folder.
- Selection confirms a reopen and leaves the previous server running. Backup/recovery preferences, journals and telemetry are scoped per server; first selection starts automation off. Existing managed/modded servers retain the verified wrapper path. Standard enrollment records game-file fingerprints, rejects known mod loaders and blocks startup after changes; it does not prove vendor authenticity or arbitrary mod compatibility.
- The overview uses compact spacing/charts in supported windows. Native minimum protects **1100 × 900 CSS content pixels**, adjusted for display DPI and WebView zoom. A reproduced 12 px overlap at 1440 × 900 was corrected by placing actions and metrics in separate rows. Twenty width/height combinations from 1100 × 900 through 1920 × 1440 pass no page scrolling, contained metrics, no footer overlap and at least 14 px of separation below View players. Button corners/center are unobstructed and navigation works; six navigation pages also fit minimum width. Long data pages and error details retain scrolling.

- Memory cards and charts use decimal **MB** (1 MB = 1,000,000 bytes), with converted chart values. File/world/player-transfer sizes use matching decimal B/kB/MB/GB/TB. Binary safety limits keep their thresholds and are labeled MiB; internal GiB history and incident schema stay compatible.

### Evidence and installation

Modern/classic builds, strict TypeScript, UI bundling, phone synchronization, **317 fixture checks** (93 desktop, 160 core, 24 resilience, 14 fields, 26 mods), **10 native dummy checks**, 15 installer checks and missing/stale/tampered provenance rejection passed. Installer compatibility also passed in Windows PowerShell 5.1. The full package installed into a disposable fixture before host installation. Exact-head CI: modern [run #18](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39421833951) and historical [run #153](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39421833866) both passed on the exact published head, including package artifacts.

The owner approved related deployment and later **one server and one launcher restart**. The initial launcher-only install preserved the running server. The single subsequent graceful server restart and launcher restart were verified query-ready, with unchanged server/configuration/mod script fingerprints and matching installed package. The sizing, overlap and MB follow-ups were then installed as launcher-only updates; that same restarted server process remained running, selected settings and server-file hashes stayed unchanged, process observation was Running, inventory scan had no error, and installed memory output used MB. Rollback/private manager data are retained locally. The one server-restart authorization has been consumed; do not repeat it automatically.

### Continuing scope and limits

Items 1–3 (existing UI, backup/recovery and mod/settings behavior) remain owner-accepted as-is. Item 4 installation polish remains in progress; item 5 GitHub stays **open for running updates**, including draft [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31). Do not merge or close. Phone expansion stays paused; GPS and altar rotation remain canceled.

Real SteamCMD download, fresh-machine dependencies, router/firewall setup, new-server gameplay and manual interaction with the new native dialogs remain unverified. Native screenshot capture timed out; browser viewport/layout measurements and compiled policy checks do not replace owner visual validation. No claim of new gameplay/mod compatibility certification is made. The version label remains private.2; exact commit/provenance identifies the build.

[Setup/update guide](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/ui-navigation-polish-20261019/desktop/react-launcher/INSTALL-UPDATE.md) · [Change log](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/ui-navigation-polish-20261019/desktop/react-launcher/SERVER-SETUP-CHANGES.md)


### Next safe work

Keep ongoing GitHub updates open. Await owner feedback on the new dialogs and actual chosen server setup. Do not download/create a real server, change the selected world or consume another server restart merely to certify these fixtures. Fresh-machine dependencies, real SteamCMD installation and new-server gameplay remain unverified. Native screenshot capture failed; the owner-reported button overlap was reproduced and corrected using browser geometry and hit tests. Preserve current running game/mod paths and private rollback/history.


## Historical notes — superseded by the latest record above

## Owner scope update and installation polish — October 20, 2026

| Item | Current status |
| :-- | :-- |
| 1. Windows UI acceptance | **Accepted as-is by the owner.** No further acceptance work requested. |
| 2. Backup/restore and recovery | **Accepted as-is by the owner.** Do not hold completion open for an additional rehearsal. |
| 3. Mod and settings behavior | **Accepted as-is by the owner.** Preserve existing working behavior. |
| 4. Installation/update polish | **In progress. First implementation slice is tested and published.** |
| 5. GitHub/project closeout | **Keep open for running updates.** Do not merge/close PRs or declare project closeout. |

Owner acceptance is recorded as an owner decision; it is not a claim of newly performed independent gameplay, restore, visual or device tests. Earlier lists of acceptance gaps remain historical evidence, not blockers overriding this instruction.

### First installation-polish slice

Normal launcher closure now supports hidden windows while honoring busy-operation close guards. No forced termination or game-server action is used. Updates validate package paths/hashes/sizes, reject overlapping folders and reused rollback locations, preserve private Manager Data (including offline player history), and restore already replaced files after an update failure. The updater creates `Open ShroudForge.lnk` and preserves/updates the existing `Open XHL 2.0.lnk`; existing folders/executable/private-data paths remain compatible.

Portable packages now include the updater, normal-close helper and [install/update guide](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/ui-navigation-polish-20261019/desktop/react-launcher/INSTALL-UPDATE.md). Fresh-machine onboarding and automatic installation-folder migration are not included in this first slice. Further installation polish stays open.

### Exact source, package and running installation

- Published source: [`acc49e5`](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/commit/acc49e5806ca0e3d58bb122d67624a1dc8c7ffaa), on `dev/installation-polish` and `feat/ui-navigation-polish-20261019`, [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31).
- Local commit: `3f57ec163f8c5826e9d1167ee29baea02101f9d7`; matching source tree `e306b55f07f70894575ec6dc7cd2076dd43ca415` verified before publication. Connected-app publication retains existing remote ancestry.
- Updated package is staged and tested with the included installer in a disposable directory. **No live update, launcher relaunch, game-server restart, settings change or folder migration was performed for this installation-tooling work.**
- Running installation remains the offline-history build: local `f6d5715`, published matching source `a6c1db8`. Do not label the new installer/tooling source as installed.

### Validation

Modern/classic Windows builds, strict TypeScript, UI bundle, phone synchronization and the existing 279 fixture checks/seven native dummy-server checks passed. **15 installer checks passed under both PowerShell 7 and built-in Windows PowerShell 5.1**, covering hidden/busy windows, exact-path exclusion, normal relaunch, rollback retention, failed-copy rollback, shortcut compatibility, package tampering, unsafe/duplicate paths and directory overlap. The complete real package also installed into an isolated fixture with every manifest hash verified and no launcher execution.

Current-head modern Windows [run #10](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39416441249) and historical Windows [run #145](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39416441239) both passed on this exact head, including fresh dependencies, builds, installer checks, packaging and artifact upload. These checks maintain build/install integrity; they do not reopen owner-accepted items 1–3.

Phone expansion remains paused and the known GameVault crypto.subtle.digest defect stays recorded. GPS and altar rotation remain canceled. Public release/signing are separate future work.

## Next work

Continue item 4 in small installation/update slices. Keep existing paths and private data compatible; use the packaged update guide rather than old deployment helpers. Fresh-machine setup remains outside the completed first slice.

Keep item 5 open: update exact source, tests, staging/install state and changelog as work proceeds. PR #31 stays open/draft; do not merge or close it for project closeout. Do not reopen items 1–3 as completion gates unless the owner requests a specific change or reports a new defect.

Retain private player metadata and rollback locally; do not publish identities, keys, raw logs or host paths. Future remote commits should build on the published ancestry, not force-push the local tree-equivalent history.
