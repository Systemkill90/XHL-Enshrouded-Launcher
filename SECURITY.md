# Security and public-server safety

Velaris is a publicly accessible server with real player activity. Its live world, players, installed mods and operational uptime are production assets. GitHub PRs, Actions workflows and downloaded archives must **never** automatically deploy to or administer the server.

## Sensitive and proprietary material

Do not commit or paste authentication secrets, passwords, Steam tokens, private/public management endpoint credentials, precise installation paths, IP addresses, player identifiers, raw logs, personal configuration, saves, crash dumps that reveal player data, or copyrighted game executables/KFC resource bundles. Sanitize test evidence first.

## Operational boundaries

- Read-only host inspection first. Owner permission before running even a read-only diagnostic with unusual CPU/disk costs.
- No stop/restart, SteamCMD update, proxy DLL installation, memory patch, config rewrite, mod-manager migration, cleanup, backup removal, or world rollback without specific approval.
- Never assume an unavailable player roster means no players are online.
- Strongest rollback baseline is an owner-validated consistent snapshot made at the appropriate game lifecycle boundary. A live byte-verified file copy is not automatically a consistent world snapshot.
- Store pristine original module hashes and a safe revert procedure for exact game builds; changed binaries must fail closed. Loader conflicts must be investigated before a new one is installed.
- World rollback could erase player activity: protect newer saves and require owner approval, even when rolling back a broken mod.
- A client-visible feature cannot be represented as working solely because a server-side tool compiled; it needs confirmation from an unmodified client.

See [public-server operating model](docs/PUBLIC-SERVER-OPERATING-MODEL.md), [desktop handoff](docs/DESKTOP-HANDOFF-2026-10-18.md), and [change register](ops/change-register.json).

Security reports should omit affected private material. GitHub files, drafts, and CI artifacts must not contain secrets or personal player data.
