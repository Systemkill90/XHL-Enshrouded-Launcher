# Selective review refactor — 2026-10-20

Scope: candidate checkout only. No installation, deployment, game configuration,
mods, player saves, or live launcher files were accessed or changed.

## Behavior and compatibility

- `HostState.Observation` distinguishes Unknown, Running, Stopped, and Failed.
  `ServerRunning` remains the last known value on observation failure; consumers
  must check Observation before treating it as current evidence. Failed samples
  retain the last-success timestamp. Samples older than 15 seconds become Unknown.
  Exact-path discovery now throws when absence cannot be established; existing
  lifecycle exception handling rejects the operation. An inaccessible same-name
  process can therefore block starting a different instance until observation works.
- Recovery only consumes confirmed process states. Observation failure neither
  reports an exit nor executes a pending recovery. Write commands wait for a
  confirmed process state. The existing safe-start wrapper, compatibility checks,
  graceful stop, backup implementation, and attempt budget are retained.
- Scan collection builds one result before publishing its reference. Failed scans
  retain the entire previous complete result, expose the failure, and retain its
  last-success timestamp. Desktop and phone report scan age. A failing section
  delays all inventory updates until the next complete scan succeeds. Missing
  optional history/backup folders remain valid empty inputs. Modern history reads
  opt into error propagation; historical classic callers retain their defaults.
- Desktop snapshots and nested rows now have concrete C# and TypeScript types.
  Existing JSON names and settings/journal formats remain unchanged; observation
  and scan metadata are additive. Operation results share message/failure updates.
- Classic tools compile directly from the maintained source with `CLASSIC_TOOLS`.
  That capability explicitly disables classic automatic recovery and its checkbox,
  preserving the prior packaged behavior. Generated `classic-source` files are no
  longer inputs. No source-text rewriting is performed.
- `Validate-Candidate.ps1` records source commit, working-tree fingerprint, version,
  dirty status, UTC build time, and output hashes after successful validation. It
  invalidates the old record first and rejects source changes during validation.
  `Package-Release.ps1` verifies the record before creating its destination and
  requires committed source. Commit or source/output/version changes require fresh
  validation. Builds predating this record must be rebuilt. Provenance is local
  integrity evidence, not a signature or a claim of gameplay compatibility.

## Validation

Run from `desktop/react-launcher` with a short, isolated evidence path, for example:

```powershell
./Validate-Candidate.ps1 -Evidence ../../../evidence/sr
./Build-Provenance.ps1 -Mode Verify
```

Passed: TypeScript strict check; UI bundle; modern and classic x64 Windows builds;
exact embedded/standalone phone synchronization; 40 desktop checks; 160 core
regressions; 24 resilience checks; 14 field checks; 26 mod checks; seven native
dummy-server checks; missing-provenance rejection, matching-provenance acceptance,
altered-output rejection, and stale-source rejection.

Regression cases include discovery/access failure, process property failure,
last-good observation retention, recovery suppression during failures (including
an already-due attempt), confirmed-exit recovery, failed configuration/backup/
history scans, immutable published results, successful retry, and operation status
reset. Desktop observation tests inject a fixture process provider. Native tests
only start and stop their marked disposable dummy and fixture wrapper.

The first full run used an overly long evidence path and hit the existing .NET
Framework MAX_PATH restriction in mod-library fixtures. The short path passed;
no mod implementation changes were needed.

Unverified: interactive WebView2 rendering/scaling, physical phone behavior,
real access-denied ACL scenarios, real-game recovery/mod behavior, and production
backup/restore. No live validation or deployment was performed.
