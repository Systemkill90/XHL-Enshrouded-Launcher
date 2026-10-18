# Change register and release gates

See [operating model](PUBLIC-SERVER-OPERATING-MODEL.md), [desktop handoff](TOMORROW-DESKTOP-HANDOFF-2026-10-19.md), and [machine-readable register](../ops/change-register.json).

Every new mod or manager change gets **one** register entry tied to a source branch/PR, dependency and safety class. IDs must be unique. `phase` is the actual development/release state, not a guess. `risk` is operational impact. A passing GitHub Action is not proof of live gameplay or of the current host installation.

Use a narrow change batch and explicit approval. Never add a pending PR to the default deployment set automatically. When `phase=public-trial-approved` or beyond, record a separately captured operator authorization and evidence on the host; public JSON is not a substitute for an approval action.

Status workflow: `research` → `source-ready` → `offline-validated` → `trial-candidate` → `public-trial-approved` → `live-validated` → `release-approved`. `hold` is reachable at every stage. Moving backward is allowed when regression evidence arrives; skipping forward is not.

Items flagged `no-client-dependency` need demonstrable in-game proof from an unmodified vanilla client, not a browser UI.
