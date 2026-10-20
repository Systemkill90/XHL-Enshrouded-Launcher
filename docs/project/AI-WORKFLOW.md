# ShroudForge — one GitHub record for ChatGPT, Work and Codex

**Established October 19, 2026.** GitHub is the shared record of **source, decisions, validated tests and durable handoffs**, not a transcript-synchronization service, cloud copy of the user's desktop, or direct link to a running server.

## Source-of-truth hierarchy

1. **Live owner host observed at time of inspection:** proves only what was inspected then. Do not confuse process running with proposed source installed.
2. **GitHub code at exact SHA and applicable tests:** proves source and tests, not deployment.
3. **This main-branch [CURRENT-STATE.md](CURRENT-STATE.md) and [SESSION-HANDOFF.md](SESSION-HANDOFF.md):** latest published project summary, subject to exact source and host reconciliation.
4. **[Decisions](DECISIONS.md), [tracker](development-tracker.md), [issue register](ISSUE-REGISTER.md), [work log](WORK-LOG.md):** durable background and prioritization. Past numerical completion estimates are not verified completion.
5. **ChatGPT/Work/Codex conversations:** design discussion or uncommitted clues. They are not evidence of a GitHub push or installed change.

## Before development (phone or desktop)

- Open [AGENTS.md](../../AGENTS.md), [CURRENT-STATE.md](CURRENT-STATE.md), and the [handoff](SESSION-HANDOFF.md). Avoid rereading old project chat threads unless something material is missing.
- Inspect only relevant existing files on the **actual target branch**. Never assume `main` has the latest launcher implementation.
- Compare the exact PR head with the local desktop working tree before editing or pulling; preserve uncommitted work.
- Identify the requested scope, whether it touches a live operation, tests needed, and a safe rollback path. For production changes, explicit authorization is required.

## During development

- Use reviewable Git branches and focused commits. Never push secrets or personal server/player data.
- Test only the changed surface where possible; Windows CI for C# builds, fixture/native checks, and isolated desktop UI validation when authorized.
- Leave previously working game binaries and mods untouched during launcher UI work.
- Distinguish **idea / staged source / CI verified / owner device tested / desktop installed / live released**.

## End of each substantive session

1. Record exact repo, branch, commit/PR and changed paths.
2. Record **what was actually run** and the result (fixture count, CI link, screenshot review, owner test). If something is unverified, say so.
3. Update [WORK-LOG.md](WORK-LOG.md) for decisions and work completed; [CURRENT-STATE.md](CURRENT-STATE.md) for genuinely changed status.
4. Update [ISSUE-REGISTER.md](ISSUE-REGISTER.md), [development tracker](development-tracker.md) and [DECISIONS.md](DECISIONS.md) only for changed work, owners' priorities or policy.
5. Update [SESSION-HANDOFF.md](SESSION-HANDOFF.md) with the next **smallest safe action**. Use a commit/PR to publish the update; don't claim a push until GitHub confirms it.
6. Tell the owner: **what changed, where, tests, installed/not installed, remaining blockers, and whether the server was touched**.

## Safety / public-repository hygiene

Never commit private host names/IPs, access tokens, phone URLs, raw server logs, player identifying data, save backups, game binaries, unredacted local diagnostic manifests or other personal information. Exclude exact desktop paths from public handoffs. Store private operational records only in a separately authorized restricted environment; do not invent that such a store exists.

## Start a new session

Copy this compact instruction, changing the task-specific line:

> Continue ShroudForge from GitHub `Systemkill90/XHL-Enshrouded-Launcher`. First read `AGENTS.md`, `docs/project/CURRENT-STATE.md` and `docs/project/SESSION-HANDOFF.md`; inspect the current branch/PR and relevant code before editing. Perform the requested task with the least disruption. Never stop, restart or modify the live Enshrouded server or its three known-working mods, saves or configuration without explicit permission. Verify and commit reviewed source changes; finish by updating the work log and handoff with exact SHA, test evidence, installed/not-installed status and next action.

[Current state](CURRENT-STATE.md) · [Roadmap](../../ROADMAP.md) · [Main documentation](../README.md)
