# Windows preview downloads and release policy

The source of truth is the repository. The owner's **known-working Enshrouded installation** is separately maintained, and GitHub automation must never alter its live game files.

## Temporary Windows previews

The workflow [Windows preview build and download](../.github/workflows/windows-preview-artifact.yml) builds `launcher-2-preview/Build.ps1` on a clean Windows runner, runs both fixture suites, and uploads a short-lived **Actions artifact** containing:

- `Enshrouded-Server-Companion-2-Preview.exe` (unsigned)
- `SHA256SUMS.txt` for integrity verification
- `READ-FIRST.txt` with safety limitations
- `TECHNICAL-NOTES.md` documenting the preview

Find an artifact under **Actions → Windows preview build and download → a successful run → Artifacts**. Sign-in may be required. Artifacts expire after 14 days. A green workflow means compilation and its included tests passed; it does **not** certify that the server, mods, multiplayer, or file migration works on the owner's actual computer.

This is a test-only application binary, not a full installer or supported package. The game, SteamCMD, third-party proprietary components, private configuration, local mod resources, world saves, and server data are **never bundled**.

## Rules for proper releases

A GitHub Release will be created only after:

1. A clean Windows build and fixture/native tests pass.
2. The owner validates a disposable installation with their exact supported game build.
3. Save backups, restore, rollback, startup/shutdown and the currently supported mods are verified.
4. All game-related redistributable assets have been reviewed for licensing, malware and private data.
5. A final release archive or installer, checksum, supported game-build matrix, clear notes, and limitations are ready.
6. The release is deliberately approved; no push or draft pull request automatically deploys to the owner's PC or publishes binaries as official releases.

Versioned GitHub Releases should be immutable snapshots when practical. Do not overwrite release archives silently. Never auto-update an actively running game server without staging, backup verification and a rollback path.

## Proposed GitHub workflow

- `main`: stable, documented code; not a claim that the latest live-server build is reproduced.
- `launcher-2-readonly-preview`: development candidate isolated from the stable branch.
- Feature branches + pull requests: review specific changes and collect Windows check results.
- Issues: failures, feature requests, real-world acceptance test findings.
- Actions artifacts: temporary executable evidence during development.
- Releases: only verified downloadable versions with checksums and release notes.

## Caveat: package dependencies

The preview's build output is **not** a self-contained installation of Enshrouded. Some update/mod paths refer to locally prepared, exact-version resources deliberately excluded from public source. A CI artifact cannot replace those resources, and it must not be used to improvise a migration or replace a working server. Consult [preview technical notes](../launcher-2-preview/README-PREVIEW.md).
