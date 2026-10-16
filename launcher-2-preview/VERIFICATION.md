# Verification — 2026-10-16

## Passed

- x64 .NET Framework build.
- 41 fixture/UI checks: strict compatibility checks, lifecycle ordering, duplicate operations, failed graceful stop handling, reset/restart preservation, recovery delay and disarming, no start merely opening offline, verified backup copies, retention of previous copies, source-directory write rejection, update result parsing and all seven page renders.
- Seven native checks against an isolated dummy executable and harmless wrapper: exact process path, exclusion of another installation, Ctrl+C delivery and graceful exit, wrapper invocation, duplicate startup rejection and nonzero exit reporting.
- Actual offline server backup through the new backup implementation completed successfully. Configured server save copies are byte-verified. No restore was performed.
- 120 original installation files, including 22 server saves, matched the baseline hashes. No production lifecycle operation or memory patch was issued.
- Rendered UI reviewed at 1400 px, with overview also checked at 1100 px. Backup/recovery controls are readable in monitoring and controls modes.

## Not verified / not complete

- Actual game start, stop and restart from this build. Start/restart are blocked because the current executable and resources do not match existing mod verification records. Passing dummy-process tests is not production verification.
- Flight pitch changes, current-build Flight/gifting hooks and stacking gameplay. One offline Flight pattern candidate was found; this is insufficient to certify a safe runtime clamp. The requested sub-90-degree cap remains unimplemented.
- SteamCMD download end-to-end, update deployment and mod compatibility after update. Update code only stages a separate installation; it cannot deploy to production.
- Cross-file consistency for backups made while the game is saving; client-side character backups outside the configured server save folder.
- Multiplayer load, high DPI and multiple monitors.

Original server and mod files remain in place. World/player save deletion, restore, migration and retention deletion are absent. This is a tested launcher preview, not a claim that the full production upgrade is finished.
