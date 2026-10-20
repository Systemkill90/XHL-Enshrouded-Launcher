# ShroudForge — session handoff

**Prepared October 19, 2026.** This is the short restart point for phone ChatGPT, desktop Work and Codex. **Do not mistake a handoff for work executed.** Consult [CURRENT-STATE.md](CURRENT-STATE.md) before making claims.

## Mandatory orientation — keep it fast

1. Read [root AGENTS.md](../../AGENTS.md), [CURRENT-STATE.md](CURRENT-STATE.md), and the dated [decision log](DECISIONS.md). No full chat-history reread needed.
2. Identify the exact repo, local checkout, active branch/commit, uncommitted work, and target build **before modifying files**. Main = public documentation. Launcher baseline = `feat/launcher-2-overhaul` ([draft #18](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/18)). Latest consolidated UI candidate = `feat/ui-navigation-polish-20261019` ([draft #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31)).
3. Verify current PR head and CI; the reviewed candidate `a5fcf210054dfea7d09ad4ae5c8f6df0cab2ec36` passed [Windows run #135](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39357907800) with 172 fixtures, native tests and a temporary preview upload. This is not a deployment record.
4. The user's interrupted desktop Work/Codex code and installed launcher may differ from GitHub. **Never overwrite newer local source, assume the repo's 2.2 label is real, or assume a handoff reached the desktop.** Capture a non-sensitive source diff and reconcile it first.

## Priority and acceptance

**One cohesive task:** finish the existing minimalist dark-fantasy launcher UI and UX, dashboard, navigation, real read-only player profiles and event monitoring. Reuse established components, keep the three known-working mods unchanged. Then improve settings/mod controls, backups/reliability, and human-readable project packaging in the order chosen by the owner.

The owner visually verified **GameVault map Compatibility mode** on iPhone Safari. That narrowly scoped fix is staged in #31; phone expansion remains paused. Test the exact *installed* phone page separately before claiming it fixed locally.

## Live-server protection — mandatory

- No shutdown, restart, activation, auto-deploy, backup restoration, world edit, mod swap, injector change or live-folder restructure without specific permission. A GitHub change is not permission to perform a live action.
- Read-only host checks, if desktop access is available, should capture server process presence and startup identity before/after launcher-only testing. Do not publish PIDs, local paths, authenticated URLs, player data or host fingerprints.
- Prefer disposable source/build folders. Verify rollback and preserve the working launcher and saves. If a safe isolated test is not possible, report it as blocked instead of treating production as test infrastructure.

## End-of-session report template

Record each substantive session in [WORK-LOG.md](WORK-LOG.md) and update [CURRENT-STATE.md](CURRENT-STATE.md) only with confirmed state:

```text
Date / environment:
Goal and scope:
Source branch + before/after SHA:
Changed paths / PR or issue:
Validation performed (build, fixture count, visual/runtime):
Host observation (if authorized): verified / not checked
Installed or deployed? yes/no, exact target, authorization:
Live server touched? yes/no/unknown, with evidence:
Unresolved problems:
Next smallest safe step:
```

No commit? Say **not committed**. No desktop connection? Say **not connected/verified**. No actual install? Say **staged only**.

## Important related references

- [PR #31 staging and Windows review](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31)
- [PR #31 validation handoff on staging branch](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/ui-navigation-polish-20261019/docs/PR31-VALIDATION-HANDOFF-2026-10-19.md)
- [Phone compatibility, safe HTML hot-swap guidance](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/blob/feat/ui-navigation-polish-20261019/launcher-2-preview/phone/README.md)
- [Main tracker](development-tracker.md), [unresolved work](ISSUE-REGISTER.md), [sharing procedure](AI-WORKFLOW.md)

**Reminder:** Phone and desktop ChatGPT chat histories are not Git commits; this is the durable *summary* needed to work safely across tools, not an automatic transcript backup.
