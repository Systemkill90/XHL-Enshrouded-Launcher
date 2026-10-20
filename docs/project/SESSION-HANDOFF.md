# ShroudForge — current session handoff

Updated October 19 EDT / October 20 UTC, 2026.

## Current installation

**ShroudForge 3.0.0-private.2 is installed and running.** Source: 3e868582d47f21df547fcc554e8ad172814c3ed2 on feat/ui-navigation-polish-20261019, draft [PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31). Modern source is under desktop/. The older 2.2 milestone and installed private.1 are historical.

The owner explicitly authorized taking private.2 online, restarting server and launcher, and waived a separate test server. Exactly one graceful server restart completed. Local game query confirmed readiness before and after launcher replacement; package hashes verified. Checked server executable/configuration/startup/controller files remained unchanged. Previous launcher files and manager state are retained privately for rollback. No game patch, mod replacement or save restoration was performed.

## Validation retained

Modern/classic Windows builds, TypeScript/bundle, 247 fixtures and seven native dummy lifecycle checks passed. All twelve pages were exercised in a synthetic browser preview. Modern Windows run [#4](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39365624611) and historical run [#139](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39365624690) passed. The initial frozen-lock mismatch was corrected before the successful rerun.

Native WebView2 startup/accessibility passed. Screenshot capture timed out and input geometry was unavailable; full native visual/DPI acceptance remains unverified. Test-server acceptance was waived, not passed. Deployment does not newly certify gameplay mod/sleep/restore semantics.

## Phone pause and next scope

Phone development is paused for today. The owner's actual iPhone screenshot shows GameVault failing with "undefined is not an object (evaluating 'crypto.subtle.digest')". Record this as unresolved; do not claim the map fixed or resume phone work automatically. Compatibility, Protected and external fallback remain in source.

Preserve the running installation and rollback. No further restart or server modification is authorized by the completed deployment. Do not demand the waived test server or silently reinstate it as a deployment gate. GPS and altar rotation remain canceled. PR #31 stays draft; no merge or public release occurred.

See [current state](CURRENT-STATE.md), [work log](WORK-LOG.md), [decisions](DECISIONS.md) and [issue register](ISSUE-REGISTER.md).
