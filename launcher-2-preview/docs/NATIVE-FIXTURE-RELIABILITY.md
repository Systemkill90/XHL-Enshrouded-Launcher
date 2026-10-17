# Windows native fixture timing

On October 17, the combined 2.0 development branch intermittently failed the isolated native fixture with "Wrapper failure was not reported correctly". The feature pull request had passed the same test. The simulated PowerShell bootstrap uses a deliberately nonzero exit code, and the original harness allotted eight seconds for wrapper startup+shutdown.

This change increases the **test-only** wait to 22 seconds and prints actual startup state, exit status, and marker presence when the assertion fails. The outer isolated harness timeout is raised to 60 seconds. This is not a bypass or retry of a failed assertion.

The real server shutdown logic, production process timeouts, mod changes, startup command and actual installation remain unchanged. The fixture still requires the nonzero exit status to be observed correctly; it fails if that never happens.

If the failure continues, investigate the full reported state instead of increasing delays indefinitely.