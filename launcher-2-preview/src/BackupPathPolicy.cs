using System;
using System.IO;

namespace XHLLauncher
{
    // This policy validates a proposed directory, and NEVER creates or writes to it.
    public static class BackupPathPolicy
    {
        static bool Within(string path, string parent)
        {
            string candidate = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string root = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return string.Equals(candidate, root, StringComparison.OrdinalIgnoreCase) ||
                candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        public static string Validate(string serverRoot, string appBase, string requested, out string chosen)
        {
            chosen = "";
            try
            {
                if (string.IsNullOrWhiteSpace(serverRoot) || string.IsNullOrWhiteSpace(appBase) ||
                    string.IsNullOrWhiteSpace(requested)) return "Choose a valid backup directory.";
                chosen = Path.GetFullPath(requested).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string root = Path.GetFullPath(serverRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string app = Path.GetFullPath(appBase).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (!Directory.Exists(chosen)) return "The backup directory must already exist.";
                if (chosen.StartsWith(@"\\", StringComparison.Ordinal))
                    return "Network paths need separate testing; select a local drive for now.";
                if (chosen == Path.GetPathRoot(chosen).TrimEnd(Path.DirectorySeparatorChar))
                    return "Choose a dedicated folder, not the root of a drive.";
                if (Within(chosen, root) || Within(root, chosen))
                    return "Backups must be outside the game server directory and its parent folders.";
                if (Within(chosen, app) || Within(app, chosen))
                    return "Backups must be outside the program directory and its parent folders.";
                string saves = ConfigReader.Read(root).SaveDirectory;
                if (string.IsNullOrWhiteSpace(saves)) saves = "./savegame";
                string saveRoot = Path.GetFullPath(Path.IsPathRooted(saves) ? saves : Path.Combine(root, saves));
                if (Within(chosen, saveRoot) || Within(saveRoot, chosen))
                    return "Backups cannot overlap the configured world-save directory.";
                for (var parent = new DirectoryInfo(chosen); parent != null; parent = parent.Parent)
                    if (parent.Exists && (parent.Attributes & FileAttributes.ReparsePoint) != 0)
                        return "Linked/junction backup directories are not supported.";
                return null;
            }
            catch (Exception ex) { return "Could not validate backup location: " + ex.Message; }
        }
    }
}
