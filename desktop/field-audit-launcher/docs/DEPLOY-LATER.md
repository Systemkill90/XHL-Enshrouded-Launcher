# Host deployment checklist (later, not on the live server)

**Do not replace the running server with a CI preview executable.** The feature branch is a Windows test build; it does not contain the official server binaries, game mod resources or local server settings.

1. Preserve the known-working launcher, scripts, game executable, mod resources, world saves and full configuration from October 16. Do not remove or rename any of these.
2. At the planned maintenance window, stop the Enshrouded server gracefully and confirm the game process has exited. Make and verify an independent backup.
3. Use a **separate disposable folder** for the new desktop preview binary and supporting local test files; do not put it inside the production game directory or point it at active saves for functional testing.
4. Validate the UI, **Files > Scan folders** and **Files > Preview cleanup** with a disposable clone first.
5. Only after a separately developed migration executor passes backup/rollback tests should an installation-layout change be considered. Today's code cannot execute any file moves.
6. Record actual server paths, startup script dependencies, SteamCMD location, game build, backup location, mod versions and intended target directory structure. Compare the plan against these.
7. Start the original known-good server if any check fails. The preview branch is not an upgrade of your current running installation.

**Not implemented yet:** trusted write-capable migration and integrated installer. Do not manually perform the proposed reorganizations merely because the preview lists them.
