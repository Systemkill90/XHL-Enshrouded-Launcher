# ShroudForge — decisions and scope record

**Last consolidated:** October 19, 2026. This is a human-reviewed synthesis of owner instructions, dated project notes and GitHub source. Older experiments remain in version control for reference; historical discussion does not imply current approval.

| ID | Date recorded | Decision / constraint | Consequence |
| :-- | :-- | :-- | :-- |
| D-01 | Ongoing; reaffirmed Oct 19 | **Protect live Velaris.** Never touch the operating server, saves, settings, three mods or loader without explicit permission. | Isolate builds; test on disposable copies; require rollback and a separate deployment go-ahead. |
| D-02 | Oct 19 | GitHub is the **shared project record**, while ChatGPT Project/Work/Codex may have separate chat threads. | Keep main-branch project docs, tracker, decision log and handoff current; branch and test evidence remain in Git history. No automatic transcript sync is implied. |
| D-03 | Oct 16–19 | User-facing product name is **ShroudForge**, not internal legacy `XHL` names. | Preserve compatibility-related filenames/class names until safely migrated; favor human-readable public paths and docs. |
| D-04 | Oct 16–19 | **Minimal, refined dark-fantasy UI** inspired by Enshrouded is the highest development priority. | Dashboard and navigation, informative telemetry, accessible interaction, professional splash identity, no generic clutter. |
| D-05 | Oct 16–19 | **Personal-use reliability first**; eventual public distribution is secondary. | Prioritize stability and editable installation paths over premature polished public installer promises. |
| D-06 | Oct 19 | GPS/ShroudNav/quest GPS, route-following and replacement quest tracking **canceled**. No autopilot. | Preserve earlier map/quest prototypes as research only; no new development without explicit reversal. |
| D-07 | Oct 18–19 | Companion phone app **paused** as a larger product area. | No new companion roadmap. Owner authorized a focused October 19 iframe white-screen fix only. |
| D-08 | Oct 18 | Flame altar 45° placement/rotation **canceled** due to build-space compromise. | Do not implement or track as active work. |
| D-09 | Oct 15–19 | Show only **observed per-player data**: sessions, ping/network events, player-related activity, attributable identities. | Do not invent inventory, levels, exact locations, character-save data or mod-granted admin capabilities. |
| D-10 | Oct 20 reconciliation | Recovered and installed informational metadata is 3.0.0-private.1; unified acceptance candidate is 3.0.0-private.2. The earlier 2.2 label was provisional. | Do not downgrade to historical Launcher 2.0 source or infer deployment from candidate version. |
| D-11 | Oct 19 | Three existing mods and dedicated server are confirmed **working by owner**. | Preserve proven flight/gifting/stacking and installed loader rather than redeploying compatibility patches during UI work. |
| D-12 | Oct 16–19 | Flight stamina toggle and pitch persistence, and one-player / 60× sleeping, have been requested or tested historically. | Track separately from verification of installed functionality and game-version compatibility. |
| D-13 | Oct 19 | External 3D GameVault map **visually works in iPhone Safari Compatibility iframe**; restrictive iframe produces white. | Default the staged map tab to Compatibility, retain Protected setting, click-to-load and no-referrer. Validate actual hosted phone page before claiming installed. |
| D-14 | Oct 15–19 | Backups/restores, crash recovery and server controls must be guarded and reversible; player progress takes priority. | CI-only fixtures aren't sufficient for a production rollback, world restore or game update. |
| D-15 | Oct 19 | Development history must be transparent: percentages are **planning estimates**, not proof. | Record started/last active dates, commit SHA, tests, visually verified behavior, installation and release separately. |
| D-16 | Oct 20 | Owner requests running-server close choices and separate create/scan functionality. | Add explicit modern UI prompts and bounded read-only discovery; create in separate empty folders, no automatic start or world migration. Preserve existing managed wrapper behavior. |
| D-17 | Oct 20 | Owner authorized one server and launcher restart; it was completed and query-confirmed. | That one-time server restart is consumed. Subsequent sizing/unit corrections were launcher-only updates; no automatic repeat server restart. |
| D-18 | Oct 20 | Owner requests minimum window sizing, uncovered View players and proper megabytes. | Native minimum protects 1100 × 900 CSS content with DPI/zoom adjustment; actions and metrics use separate rows. Display decimal MB with actual conversion, and label unchanged binary safety limits MiB. |
| D-19 | Oct 20 | Owner requests New server beside the selected-realm box, opening a create/scan wizard or guide. | Keep both top controls visible; show a dedicated Server setup guide with direct new-server form and discovery choices. No automatic real-server creation, selection or restart. |
| D-20 | Oct 20 | **Native iOS App Store companion canceled.** Owner instead authorized a substantial UI overhaul of the **existing private-LAN web phone companion**. | Stage a mobile-first redesign with an owner-confirmed working top-level HTTPS GameVault map action and preserve existing authenticated controls. Wider phone feature development remains paused; no automatic deployment or server restart is approved. |

## Security / publishing rule

The repository is **public**. Do not put private LAN addresses/links, phone bearer keys, passwords, API tokens, personal player data, raw live logs, proprietary game files, save contents, private desktop paths, source-copied credentials or sensitive host diagnostics in issues, source or documentation. Detailed private debugging notes belong in a properly restricted location, not GitHub public docs.

## Explicit scope change procedure

When an owner reverses a cancellation, changes production rules, or approves deployment: (1) date a new decision here; (2) update [CURRENT-STATE.md](CURRENT-STATE.md), [development tracker](development-tracker.md), and [ROADMAP.md](../../ROADMAP.md); (3) use a reviewable PR and record validation; (4) never infer permission for server changes from GitHub activity.

[Current state](CURRENT-STATE.md) · [Work log](WORK-LOG.md) · [Session handoff](SESSION-HANDOFF.md)

## October 19, 11:50 PM EDT — explicit deployment authorization

Owner: “Forget the test server … Pause phone stuff for today … Publish and restart live server and launcher now.” This supersedes the staged-only deployment boundary for this specific candidate. The owner waived test-server acceptance; it was not tested. Private.2 was installed with rollback and one graceful server restart verified. Phone development remains paused; no public release or merge was requested or performed.

## October 20 — selective refactor and launcher-only deployment

Owner first authorized the focused review refactor with no live access. After verified commit 507d86c, the owner explicitly authorized deployment, then preapproved related GitHub updates. The launcher-only deployment completed with rollback and installed hashes/readiness verified; the existing server process/files/settings stayed unchanged and no game-server restart was requested. Preserve existing working integration. Current authorization applies to the related work, not unrelated scope. Publish matching source plus exact validation/deployment limits; do not claim unresolved phone or gameplay acceptance passed.

## October 20 — accept existing behavior, start installation polish, keep GitHub open

The owner accepts items 1–3 from the remaining-work list (current Windows UI, backup/restore/recovery and mod/settings) as they are. Record acceptance without inventing new independent verification. Start item 4 installation/update polish while preserving working server behavior and compatible paths. Keep item 5 open with ongoing GitHub updates; do not merge/close PRs or declare project closeout. The first tooling slice is tested/staged and not a new live deployment.
