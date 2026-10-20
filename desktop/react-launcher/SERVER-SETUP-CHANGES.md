# Close choices and server setup — October 20, 2026

- Closing the modern launcher warns while its selected server is running, with keep-running, graceful-stop-and-exit, and cancel choices. Failed observation permits only launcher-only exit or cancel. Failed graceful stop leaves the launcher open.
- The server selector, Tools menu and System page open bounded discovery and separate-server creation. Scans are read-only, report invalid configurations and errors, and expose scan limits.
- Create downloads standard dedicated-server files using an isolated copy of a user-selected official SteamCMD, requires confirmed success, and creates a separate empty world/configuration without starting it. Existing/nonempty/overlapping targets are rejected. Partial failures stay in the new folder.
- Selection requires confirmation, preserves the previous running server and reopens the launcher. Backup/recovery preferences are restored per server; new servers begin with automation off, separate schedules and telemetry history. Original legacy paths remain compatible.
- Standard servers use explicit enrollment and game-file fingerprints; changed files block start. Existing managed/modded servers retain their verified wrapper path and exact-build guards. Standard enrollment rejects known mod/startup components and does not certify arbitrary mod absence or vendor authenticity.
- Installer updates explicitly request launcher-only closure and still honor busy-operation refusal. No force-kill or server restart is introduced.

Validation: 82 desktop checks (27 added), 160 core checks, 24 resilience checks,
14 field checks, 26 mod checks; 10 isolated native checks (3 added); 15 installer
checks; modern/classic builds, strict TypeScript, UI bundling, phone synchronization
and missing/stale/tampered provenance rejection. Installer checks also run under
Windows PowerShell 5.1. Final package provenance binds to the committed source.

Limits: real SteamCMD download, fresh-machine dependencies, router/firewall setup,
and new-server gameplay are not covered by fixtures. No existing server/world is
created, stopped or restarted to validate this feature. New-server setup and close
choices are modern UI features. GitHub PR stays open for continuing updates.

See [setup and update instructions](INSTALL-UPDATE.md).
