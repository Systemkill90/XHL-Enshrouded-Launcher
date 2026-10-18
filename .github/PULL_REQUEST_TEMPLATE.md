## Purpose and scope

What exactly does this change do, and which feature/game build is it for?

## Operational risk on Velaris

- [ ] This is **source-only / research** and cannot be installed without a separate release decision.
- Risk: low / medium / high / critical
- Exact affected module files, native loaders and version/build:
- Does it require restart, downtime, network change, or world/config/save modification?
- Could players be online during the operation? What happens if the roster query fails?
- Is it server-only, or does it require client installs? If promising vanilla client HUD behavior, provide actual in-game proof.

## Evidence

- [ ] No proprietary binaries, private saves, secrets, personal paths or unsanitized player data.
- [ ] Static/source tests run and reported accurately.
- [ ] Exact build and original hashes checked, or clearly marked **not yet verified**.
- [ ] Behavior demonstrated in-game on the supported build, or explicitly **unverified**.
- CI links, tests performed and known gaps:

## Recovery and release gates

- Pristine originals preserved:
- Backup scope and consistency (stopped world vs live copy):
- Rollback instructions, including how to protect *new* player progress:
- Mod loader/dependency conflicts:
- Owner authorization for live deployment: **not requested / pending / separately approved**
- Proposed phase: research / source-ready / offline-validated / trial-candidate / public-trial-approved / live-validated / release-approved / hold

**PR review/merge is NOT an authorization to deploy, update, stop, restart, roll back or inject on the owner's live public server.** See [operating policy](../docs/PUBLIC-SERVER-OPERATING-MODEL.md) and [change register](../ops/change-register.json).
