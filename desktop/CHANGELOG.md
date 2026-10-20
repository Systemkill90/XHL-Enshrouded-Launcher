# ShroudForge candidate changelog

## 3.0.0-private.2 — October 20, 2026

- Reconcile the recovered 3.0 private React/WebView2 desktop with PR #31's profiles, event presentation and narrow Compatibility phone fix. Preserve the original 2.0/PR sources for comparison.
- Show CPU and memory histories from process samples. Add full observed player sheets and identities from completed-session records. Stop presenting offline/stale network data as current.
- Add searchable recorded events alongside the bounded live console; improve empty/error states, navigation focus, scroll reset, reduced motion and narrow layouts.
- Keep GameVault Compatibility as phone default, Protected and external fallback; remove forced 420px height on compact phones. Embedded HTML remains byte-equivalent to standalone text.
- Replace the blocked desktop map embed with GameVault and a fixed external fallback. Add a branded loading surface and interface error boundary.
- Harden read-only mode against listeners, automatic recovery and scheduled backup actions. Rebuild classic tools from retained source; pin UI/SDK dependencies and add modern Windows CI.
- Stage a portable unsigned package with dependencies, file checksums, acceptance instructions and rollback guidance. No installation, server action, merge or public release.

Validation: modern and classic Windows builds, TypeScript and UI bundle; 247 fixture checks (23 desktop, 24 resilience/backup, 14 field, 26 mods, 160 regression), plus seven native dummy-process checks. Browser navigation covered all twelve pages, player sheets and console search, minimum-width dashboard, phone tabs, Compatibility/Protected switching and 390x844 / 375x568 viewport fit. Physical iPhone terrain rendering, native DPI/scaling, real-game mod/sleep semantics and disposable-game restore acceptance remain unverified.
