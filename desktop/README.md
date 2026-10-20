# ShroudForge modern desktop candidate

Recovered from the newer desktop Work implementation (local baseline 0873d2f), reconciled with draft PR #31 at a5fcf21. The earlier 2.2 milestone does not describe its version: recovery metadata is 3.0.0-private.1; this acceptance candidate is **3.0.0-private.2**. No application deployment is performed by this branch.

- react-launcher: React/WebView2 control center, bounded recovery, incident evidence and persistent backup policy.
- field-audit-launcher: retained core/classic UI, verified data fields, mod library and narrow phone-map fix.
- launcher-2-preview at repository root: preserved PR #31 / Launcher 2.0 historical implementation, not the modern candidate's build input.

Build on Windows with .NET Framework 4.8, Node 24 and pnpm 10.17.1. From react-launcher, run Restore-BuildDependencies.ps1 once, pnpm install --frozen-lockfile, then Validate-Candidate.ps1. Package-Release.ps1 -Destination <new isolated folder> stages already validated build files without rebuilding. SDK package/version and UI dependencies are pinned; no developer-specific paths are required.

Classic tools are built directly from the reconciled core with an explicit recovery capability. The package includes a launcher-only update script, normal-close helper and [installation/update guide](react-launcher/INSTALL-UPDATE.md). It preserves existing folders and private Manager Data, retains rollback, supports hidden launcher windows and leaves the game server alone. Fresh-machine onboarding and automatic folder migration are not included in this first polish slice. Historical maintenance utilities remain for source history and are not the supported update path.

See react-launcher/CANDIDATE-INSTRUCTIONS.md for dependencies, rollback and remaining acceptance checks. Phone development stays paused except for Compatibility default, Protected/external fallback and viewport fit.
