# ShroudForge modern desktop candidate

Recovered from the newer desktop Work implementation (local baseline 0873d2f), reconciled with draft PR #31 at a5fcf21. The earlier 2.2 milestone does not describe its version: recovery metadata is 3.0.0-private.1; this acceptance candidate is **3.0.0-private.2**. No application deployment is performed by this branch.

- react-launcher: React/WebView2 control center, bounded recovery, incident evidence and persistent backup policy.
- field-audit-launcher: retained core/classic UI, verified data fields, mod library and narrow phone-map fix.
- launcher-2-preview at repository root: preserved PR #31 / Launcher 2.0 historical implementation, not the modern candidate's build input.

Build on Windows with .NET Framework 4.8, Node 24 and pnpm 10.17.1. From react-launcher, run Restore-BuildDependencies.ps1 once, pnpm install --frozen-lockfile, then Validate-Candidate.ps1. Package-Release.ps1 -Destination <new isolated folder> stages already validated build files without rebuilding. SDK package/version and UI dependencies are pinned; no developer-specific paths are required.

Classic tools are rebuilt from the reconciled core; generated classic-source, SDK, dependencies, tests and binaries are ignored. Historical maintenance/install utilities are preserved for source history, not acceptance steps: they must never be pointed at the live host during candidate review. No installer is shipped in the candidate.

See react-launcher/CANDIDATE-INSTRUCTIONS.md for dependencies, rollback and remaining acceptance checks. Phone development stays paused except for Compatibility default, Protected/external fallback and viewport fit.
