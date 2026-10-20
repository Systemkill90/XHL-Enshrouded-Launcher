# ShroudForge phone companion · mobile UI refresh
**October 20, 2026 · Source-only candidate (not installed)**

This refresh replaces the old crowded two-tab private-LAN companion with a restrained, mobile-first dark-fantasy layout. It **reuses the existing** phone server, authentication, status JSON, and guarded commands. This is **not** a native iOS app, pairing service, game mod or new background service.

## Experience

| Tab | What is shown | Existing source |
| --- | --- | --- |
| Overview | Realm status, uptime, players, CPU/RAM; backup, world metadata, automation | Existing status and command API |
| Players | Online roster/health/connectivity, known historical records | Existing `roster` and `knownPlayers` data; no new data inferred |
| Activity | Recorded server/launcher event feed | Existing `activity`; no fabricated locations |
| Map | Prominent **Open interactive 3D map** direct HTTPS action, with optional collapsed iframe | GameVault HTTPS map, no management-token transfer |

Mobile improvements: safe-area aware bottom tab navigation, larger touch targets, separate views, improved hierarchy and typography, subtle forest/gold surfaces, reduced-motion support, accessible tab state and status announcements. No external UI frameworks, fonts or scripts were introduced. The game Start, Restart and Stop controls remain available, but grouped under an explicit **Server controls** disclosure with the existing confirmation policy; no server action happens merely by opening a tab.

## Map behavior and why

The owner confirmed that **https://enshrouded.gamevault.in/map works on iPhone** when opened directly. The ShroudForge phone page is served over private HTTP, so its embedded HTTPS map may fail on Web Crypto (`crypto.subtle.digest`) due to insecure ancestor context. The Map tab now favors the proven direct HTTPS website, opened in a separate top-level browsing context using `noopener noreferrer` and `referrerpolicy=no-referrer`. The embedded iframe is still available, but only inside an explicitly labeled **experimental** disclosure and only after the user taps Load. This does **not** pretend to fix insecure iframe Web Crypto.

## Safety and compatibility

- All existing status and command paths are unchanged. No private authentication keys, data, settings or host paths enter version control.
- No desktop host, game executable, mod, save, firewall or live server was accessed or changed.
- Existing server-side command allowlist, authentication/CSRF guards and explicit Start/Stop/Restart confirmation remain. Backup/automation command behavior remains unchanged.
- HTML is generated once at `desktop/field-audit-launcher/phone/phone-ui.html` and synchronized into `src/PhoneView.cs`. New offline `Phone-UX-Checks.mjs` is invoked during Windows candidate validation.
- Existing and additional C# phone integration fixtures check required fields, tab IDs, safe external map URL, controls and auth endpoint semantics.
- The *installed* phone page will not change until a later approved launcher-only update on the owner's Windows host. Installing/restarting the game server is unnecessary for this UI-only change.

## Before deployment

1. Review this candidate in an isolated mobile browser at narrow and wide sizes; verify each tab, focus, large text, status refresh, player records and event rows.
2. Verify the direct GameVault map opens and fully renders on iPhone Safari. Check the private ShroudForge address is not leaked through referrer.
3. Attempt the optional iframe only to confirm graceful diagnostic behavior, **not** guaranteed rendering.
4. Run modern/classic Windows CI and fixture tests for the **exact current source commit**.
5. Only after owner approval, perform a **launcher-only** update with rollback. Confirm the game server process remains unchanged, no user commands were triggered, and original manager data is retained.

**Not verified here:** physical iPhone layout, native host installation, private-LAN network behavior and embedded map success. Do not mark those completed from static checks alone.
