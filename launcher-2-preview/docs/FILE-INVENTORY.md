# Files and folders — migration planning

This stage adds a second action to the existing **Files** page:

- **Scan folders:** read-only inventory of all files and likely purpose.
- **Preview cleanup:** read-only proposed folder organization, warnings and SHA-256 fingerprints for small recognized legacy launcher archive ZIPs.
- **Copy report:** copies either report for later discussion and review.

No file is moved, renamed, deleted, overwritten, copied or created by either feature.

## Behavior and boundaries

- Preserves the dedicated-server executable and required adjacent resources, world saves, server configuration, mods, loader DLLs, startup scripts, backups and unknown files.
- Does not traverse junctions or symbolic links. Reports partial/incomplete scans; 12,000 filesystem entries and depth 16 are the traversal limits.
- Notes when the configured save folder is outside the selected installation, but **never visits it**.
- Recognizes only root-level historical launcher ZIP naming patterns as possible archive candidates. Even these are review-only.
- Computes a SHA-256 fingerprint for candidate files no larger than 32 MiB. Larger or changing files are marked blocked.
- Marks proposals blocked if a destination already exists or matching references are found in root-level startup/configuration scripts. No automatic global text replacement.
- Reports whether a game process is visible on the host, but creating a preview never shuts it down.
- Refuses to describe a malformed or missing server JSON configuration as ready for migration.

## Later: when the desktop is connected

The actual user-host implementation needs source manifest snapshots, a complete path reference graph, reliable disk-space checks, a verified independent recovery copy, an explicit stopped-server check, staged/atomic file operations, an operation journal and full rollback fixture tests. Do not enable relocation before these gates pass.

The last known-working October 16 server must remain available for rollback. Neither a successful preview nor passing simulated CI checks proves safe migration of that live installation.

The desktop preview is a planning tool you can use before the later restart window; the server should be stopped **before** any actual relocation, whenever that feature is added and validated.
