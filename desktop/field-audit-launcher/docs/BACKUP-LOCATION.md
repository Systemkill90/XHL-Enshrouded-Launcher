# Choose where future world backups are stored

The **Automation → Change backup path** button now lets the owner choose an existing local folder outside both the dedicated server folder and the launcher folder.

This change affects future backups only. It does **not** move, edit or delete any previous backup or any live save. The selected path is stored in existing launcher settings; the legacy backup folder remains the default until you explicitly choose another.

Safety rules: refuse an overlapping game/server/world-save/program directory; refuse a selected root drive, UNC network share or linked/junction path for this stage; reject paths that do not already exist. The actual backup routine also checks paths and copies save files with byte verification.

This is a useful first step toward disentangling runtime data from the location of the launcher executable. It is not a restore engine, a complete migration system or an installer. Verify available disk space and confirm a snapshot can be restored during the later maintenance window.