# Contributing to ShroudForge

The primary target is **one owner's running public Enshrouded server (Velaris)**. There is no guaranteed separate multiplayer staging server. Feature suggestions, pull requests and issue reports are welcome, but contributions must not assume the production server may be disrupted for experiments.

## Pull request standards

Use the [PR template](.github/PULL_REQUEST_TEMPLATE.md) and include the target game version/build (or 'unknown'), exact file changes, dependency/loader assumptions, player impact, offline tests, rollback procedure and evidence gaps. State the stage from [the release model](docs/PUBLIC-SERVER-OPERATING-MODEL.md). Mark untested server or client behavior as unverified.

- Keep experimental game hooks, offsets, flight controllers and reverse-engineering work in **draft** PRs until exact-build safety and owner-approved live tests.
- Start with read-only inspection and offline fixtures; never test unknown injector chains or save writes on Velaris merely because there is no spare server.
- Server-only features must be demonstrated on an **unmodified game client** if they promise client HUD changes. A website is not an in-game quest tracker.
- Never bundle proprietary game executables, private KFC archives, live saves, personal installation paths, game/user credentials or unsanitized player logs.
- A review, merge or green workflow is **not** deployment permission.

For bug reports include relevant source commit, installed build if known, observable behavior and sanitized reproduction steps. See [security policy](SECURITY.md).

## Staging and acceptance

Follow [desktop handoff](docs/DESKTOP-HANDOFF-2026-10-18.md), the [change register](ops/change-register.json) and the [artifact policy](docs/BUILD-ARTIFACTS.md). Use explicit, owner-approved maintenance windows for any public-server deployment. Never overwrite or restore worlds after players have made new progress without discussing irreversible loss.
