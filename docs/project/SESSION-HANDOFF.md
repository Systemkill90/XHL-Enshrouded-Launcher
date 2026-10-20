# ShroudForge — current session handoff

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
