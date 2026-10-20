# ShroudForge — verified development and decision chronology

**Public project history, consolidated October 19, 2026.** This is a selective engineering record, **not** a copy of private conversations, local desktop history or sensitive server telemetry. Where testing occurred only in CI or as an owner's observation, that distinction is explicit.

| Date | What happened | Evidence / limitations |
| :-- | :-- | :-- |
| Oct 15, 2026 | Enshrouded 1.0 compatibility and legacy launcher/injector reliability were investigated; preserved existing files and saves. Earlier XHL v1 patches/packages existed. | Historical conversation/files; failed injector attempts were not proof of compatibility. Preserve the known-good mod loader. |
| Oct 16 | Public GitHub landing, documentation and launcher 2.0 UI overhaul began. User emphasized easily navigable branding, personal-use priority, safe mod continuity. | Existing [2.0 development branch and draft #18](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/18); not a public stable installer. |
| Oct 16–17 | Launcher concept broadened to configuration editor, backup/restore, server metrics and read-only player data; flight behavior, stamina and one-player sleep discussed. | Features have different verification states; desktop host/browser compatibility not proved by source alone. |
| Oct 18 | UI, navigation and GitHub housekeeping progressed; mobile companion with external 3D map and separate map research were explored. Flame altar 45° concept was canceled. | Earlier map/quest research kept in draft/archives, not approved active features. |
| Oct 19 | Owner confirmed working Enshrouded server and **three existing mods**; emphasized finishing dark-fantasy launcher UI and removed GPS from scope. Phone expansion paused. | Owner observation; not evidence a new GitHub build is installed. |
| Oct 19 | Provisional three-group [development tracker](development-tracker.md) and [next session instructions](NEXT-SESSION.md) written. | Historical percentages estimated, not code-audited. |
| Oct 19 | Staged a consolidated UI source candidate with sidebar polish, dashboard activity, player profiles and QoL event feed based on drafts #19 and #20. | [Draft PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31) targets #18 source branch; not merged to main/live. |
| Oct 19 | First reviewed consolidated Windows candidate repaired a C# syntax issue and passed compilation, 167 fixtures and native tests. | [Windows CI run #113](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39345947576); unsigned temporary artifact; no visual/production acceptance. |
| Oct 19 | Phone map showed white inside allocated iframe. Added fallback/troubleshooting, then temporary embedded-map Safari diagnostic. | User **confirmed** GameVault rendered inside iframe in **Compatibility** mode; Protected mode remained blank. Standalone web test has no server control functionality. |
| Oct 19 | Updated staged phone tab to default Compatibility while preserving Protected toggle and click-to-load. | [Draft PR #31](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/pull/31); actual owner's installed phone page **not confirmed updated**. |
| Oct 19 | New reviewed PR #31 source committed, passing **172 fixtures**, native checks, Windows build and artifact upload. | [Windows CI run #135](https://github.com/Systemkill90/XHL-Enshrouded-Launcher/actions/runs/39357907800); reviewed head `a5fcf210054dfea7d09ad4ae5c8f6df0cab2ec36`. Recheck if head advances. |
| Oct 19 | Created a new desktop Work/Codex handoff with explicit no-server-shutdown instructions. | **Session creation was acknowledged**, but desktop connection, work execution and installed updates **weren't independently confirmed in this chat**. Never mark host work done based on handoff acceptance. |
| Oct 19 | Consolidated a public cross-session project record on `main`. | The [current state](CURRENT-STATE.md), [handoff](SESSION-HANDOFF.md), [decisions](DECISIONS.md), [issue register](ISSUE-REGISTER.md) and [AI workflow](AI-WORKFLOW.md) are deliberately independent of live-server deployment. |

## Document a future session

Append a new row **only after observing or verifying the stated event**. Record branch, SHA, test run, device/host evidence and unresolved gates. Do not copy private chats or user credentials into this public document.

Use the [handoff report template](SESSION-HANDOFF.md) for substantial implementation, and update [CURRENT-STATE.md](CURRENT-STATE.md) if this record supersedes the current snapshot.

## Prior Work history limitation

Phone ChatGPT chats, desktop Work and Codex sessions are **not** synced to GitHub automatically. Some local Work changes might exist without a corresponding commit. This record summarizes known outcomes; it does not certify that **every** previous file, chat or unpublished desktop change has been recovered.

[Current state](CURRENT-STATE.md) · [Decisions](DECISIONS.md) · [Session handoff](SESSION-HANDOFF.md)
