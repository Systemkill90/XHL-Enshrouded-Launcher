# Velaris operating model — public production server

**Effective 2026-10-18. Owner-led deployment; source changes are never permission to change the running server.**

## What is real

Velaris is a publicly accessible Enshrouded dedicated server with players. The owner tests mods **on the public server** rather than maintaining a disposable parallel public game server. Development may occur without host access, but live validation requires an owner-selected maintenance window and explicit permission. Previous references to a mandatory disposable *game server* as the only test path are not operationally realistic. We still require offline/source testing before touching production.

## Stages and definitions

| Stage | Meaning | May alter Velaris? |
|---|---|---|
| `research` | Unknown compatibility, reverse-engineering, unsupported hooks, proposals | **No** |
| `source-ready` | Source committed, reviewed scope, reversible plan; no verified compile | **No** |
| `offline-validated` | Correct tests/CI passed and exact input/output compatibility evidence recorded; no live gameplay proof | **No** |
| `trial-candidate` | Exact build pinned, owner reviewed risk, originals/hash baseline, backup plan and revert tested without impacting users | **Not automatically** |
| `public-trial-approved` | Explicit owner authorization for **that exact** change, host, and maintenance window | **Only specified changes** |
| `live-validated` | Owner tested on public server, documented impact, verified actual game behavior and logs | **Only with approval** |
| `release-approved` | Owner accepted and chose to retain version in production; recovery documented | **Yes for approved version only** |
| `hold` | Blocked, rejected, superseded or missing evidence | **No** |

Never equate: Git commit = merged = built = deployed = working. A successful workflow validates its stated tests only. A PR, ZIP, downloadable Actions artifact, or today's calendar date must not start any deployment.

## Approved work sequence

1. **Develop off-host** in feature/research branch, with source-level tests, lint, unit fixtures and private/sanitized sample data. Do not add alternate mod loaders or update Steam binaries by default.
2. **Prepare candidate** with supported game build, exact SHA-256 of affected original bytes, minimal patch surface, dependency list, independent enable/disable, prospective impact to players, and a tested rollback procedure. A read-only baseline inventory is allowed with owner's desktop-session consent.
3. **Before a maintenance window**, ask for **explicit approval to deploy** a named candidate. Check connected players and warn them; if roster is unavailable/unknown, do not treat it as empty. Avoid disruptive operations with players connected without separate owner approval.
4. **Protect the world:** preserve all preexisting saves and mods. Take an operator-reviewed backup. Byte-verified copies of *live* files may span save cycles; a coherent offline checkpoint may require stopping the game gracefully. Do not call a live copy a transactional consistent restore point.
5. **Apply only approved changes**. Verify exact version/hash match immediately before editing. Unknown game update, changed offset, loader conflict, missing original, failed preflight or backup ambiguity => fail closed.
6. **Test live on Velaris** with owner observing: start, player joins, existing mods (flight/pitch hold, gifting, stacking), quest/inventory/world integrity, connection, latency and server logs. Do not claim that source CI tested gameplay.
7. **Revert if unexpected**: stop further changes, capture sanitized diagnostic evidence, restore original code/mod files and matching world snapshot only when rollback requires it and owner approves. Do not restore an old world blindly after player progress has occurred.
8. **Record outcome** in `ops/change-register.json`, specific PR/issue, exact known behavior, and follow-up. Only then mark `live-validated` or `release-approved`.

## Non-negotiable safety constraints
- No restart, shutdown, update, DLL injection, loader swap, patch activation, directory cleanup, save editing, automatic deployment or backup deletion without owner approval.
- New plugins/mods cannot be enabled simply because CI passed.
- Unknown roster is **not** zero players.
- No sharing public server passwords, auth tokens, exact internal network details, IPs, player identifiers or save files in GitHub.
- For mods based on proprietary assets, store source instructions/patch code only; never publish game binaries, KFC archives or user-extracted texts.
- No unvalidated server-to-client quest tracker changes; quest HUD/no-client-mod approach stays research-only until a vanilla client actually shows the change.
- A client-facing code path needing client installs cannot silently be redefined as server-only.
- Rollback must respect player progress. Restoring backed-up world files can erase sessions; obtain explicit owner approval and warn players first.

## What "ready today" means

**Ready for desktop examination and a deployment decision, not scheduled to update itself.** The host should remain running unless the owner expressly authorizes a maintenance stop. Use [desktop handoff](DESKTOP-HANDOFF-2026-10-18.md) and the change register for exact next actions.
