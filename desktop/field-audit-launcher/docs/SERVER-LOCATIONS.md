# Working with existing server directories

The manager now has a **Files → Use server folder** control. It chooses an existing folder containing the official dedicated-server executable and readable JSON configuration.

**Important:** This selection is recorded for the NEXT manager launch. It does not stop the server, move files, restart Windows or change the server that the current manager process controls. The user should close and reopen the manager when ready.

Previously, the old startup BAT could force its original directory every time the manager opened. The new startup resolver honors a deliberately chosen directory first; if the chosen folder is missing/invalid, it refuses to silently select the old one. Installations without a manual choice keep the original startup-argument behavior.

The successful staged-update switch also records the newly selected server as preferred for the next launch.

This is **path adoption**, not file migration. The full in-place migration feature still requires a tested manifest, backup, stopped-server guard, reversible writes, and rollback. No files move in this change.

The existing October 16 working installation should remain untouched until the planned maintenance window, with a known-good backup and startup path.