# Build artifacts and public-server release policy

GitHub is the **source of truth for development**, not proof of which files are running on Velaris. The live public server is owner-operated and is the owner's real multiplayer test target; there is no guaranteed disposable game server.

**Every build and Actions artifact is a candidate for inspection only.** GitHub PR review, merge, passing tests, and artifact publication do not automatically install, update, start, stop, patch, back up, or restart the owner's host. Nothing is scheduled for deployment merely because the current date is Sunday, October 18.

## Preview workflow

[Windows preview build and download](../.github/workflows/windows-preview-artifact.yml) runs Windows source/build checks and may upload a temporary unsigned application bundle:

- `ShroudForge-Launcher-Preview.exe` — preview manager executable, not a complete game installation.
- `SHA256SUMS.txt` — integrity checksum of preview binary.
- `READ-FIRST.txt` — operating limitations.
- `TECHNICAL-NOTES.md` — build notes.
- Bundled launcher artwork and compatibility scripts, **not** proprietary game resources or private config.

Find artifacts under GitHub **Actions → Windows preview build and download → successful run → Artifacts**. They normally expire after 14 days. A green run means its exact build/tests completed, **not that the mod stack or gameplay works on the public server**. Do not point an unsigned preview at the running server without a planned change/revert review.

## Release gates

Before promoting to `trial-candidate`, document the exact supported game build and SHA-256 checksums of affected files, dependency/loading path, original backup, reversibility, player impact, source/fixture tests, expected UI and commands, and failure behavior. A source-only candidate may be checked offline without touching Velaris.

Before `public-trial-approved`, the owner must authorize **the named version**, install actions and relevant downtime. Check player presence; unknown is not empty. A game-version update, loader replacement, native memory patch, shutdown, restart, save edit, directory migration, or world restore needs explicit approval and must preserve a rollback path.

For player-impacting changes, make a coherent backup and confirm restoration strategy. A byte-verified **live** save copy may be internally inconsistent and cannot safely replace newer player progress. New world data following the deploy must not be blindly overwritten during rollback.

After the owner tests on the public server, record concrete results and known regressions before `live-validated` and `release-approved`. Reject/hold unsupported builds or failed/unknown preflight steps.

Use the [public-server operating model](PUBLIC-SERVER-OPERATING-MODEL.md), [Sunday Oct 18 desktop handoff](DESKTOP-HANDOFF-2026-10-18.md), and [change register](../ops/change-register.json).

## Repository lanes

- `main`: stable source history; no guarantee it exactly matches the deployed host.
- `feat/launcher-2-overhaul`: ongoing launcher 2.0 source baseline for review.
- Other `feature/*`, `research/*`, and `ops/*` branches: separate draft work, not automatically included in a deploy.
- GitHub Actions: CI/source artifacts; no server credentials or deployment keys.
- GitHub Releases: explicitly approved downloadable versions with supported build/known limitations, hashes and notes. Never silently replace a published binary.

## Licensing and distribution

The preview is not a self-contained Enshrouded installer. Exclude game binaries, SteamCMD installs, third-party proprietary resources, player identities/logs, private network configuration, credentials, live saves and backups. Unknown compatibility of mods with a future game version remains blocked until verified for the exact build.
