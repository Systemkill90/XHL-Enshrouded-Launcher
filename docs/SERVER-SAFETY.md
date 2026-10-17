# Working-server safety

XHL can eventually stop, update and restart a public game server and modify save files. Treat these as **production-impacting operations**.

Before any launcher upgrade, migration, mod change, update or restore:

1. Confirm the server process is stopped cleanly for file-replacement work.
2. Copy the launcher directory, server JSON and whole savegame directory to a separate offline location; record a restore path.
3. Keep a known-working launch method outside the new project directory.
4. Validate all chosen paths and proposed file operations in a dry-run preview.
5. Test new code against a disposable copy before using it on a live server.

Never delete or overwrite existing runtime folders to "tidy up" without an explicit file-level migration plan and verified backup. Do not publish private saves/configs/logs to GitHub. A new GitHub commit does **not** update an already-running Windows installation.

For auto-restart, SteamCMD updates and save restore, define explicit tests for graceful stop, user cancellation, crash recovery, interrupted network/download, corrupt archives and repeated restarts. A rollback must not depend on the new launcher still working.
