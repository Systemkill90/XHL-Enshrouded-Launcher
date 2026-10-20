using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XHLLauncher
{
    // Read-only. An inventory category is never permission to delete or relocate data.
    public sealed class InstallationInventory
    {
        public string Root, SavePath;
        public int Files, Folders, ReviewFiles, Linked;
        public long TotalBytes;
        public bool Partial, ExternalSaves;
        public readonly SortedDictionary<string, int> Groups = new SortedDictionary<string, int>();
        public readonly List<string> Warnings = new List<string>();
        public readonly List<string> Samples = new List<string>();
        const int Limit = 12000, DepthLimit = 16;
        static bool Inside(string path, string parent)
        {
            string p = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string c = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return c.Equals(p, StringComparison.OrdinalIgnoreCase) ||
                c.StartsWith(p + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }
        void Warn(string message) { Partial = true; if (Warnings.Count < 30) Warnings.Add(message); }
        void Sample(string s) { if (Samples.Count < 35) Samples.Add(s); }
        static string Category(string absolute, string relative, string saves)
        {
            string path = relative.Replace('\\', '/').ToLowerInvariant();
            string name = Path.GetFileName(relative).ToLowerInvariant();
            if (Inside(absolute, saves)) return "World saves - protected";
            if (path.StartsWith("savegame/") || path.StartsWith("saves/")) return "Possible saves - review";
            if (path.StartsWith("backups/") || path.StartsWith("world-backups/")) return "Backups - protected";
            if (path.StartsWith("logs/") || path.StartsWith("xhl-diagnostic-logs/") || name.EndsWith(".log")) return "Logs";
            if (name == "enshrouded_server.exe" || name == "enshrouded_server.kfc" ||
                name == "enshrouded_server.kfc_resources" || path.StartsWith("steamapps/")) return "Game server - protected";
            if (name == "enshrouded_server.json" || name.EndsWith(".ini") ||
                name.EndsWith(".cfg") || name.EndsWith(".xml")) return "Configuration - protected";
            if (name.Contains("flight") || name.Contains("gifting") || name.Contains("stack") ||
                path.Contains("mod") || name.EndsWith(".lua")) return "Potential mods - review";
            if (name.EndsWith(".bat") || name.EndsWith(".cmd") || name.EndsWith(".ps1")) return "Scripts - review";
            if (name.EndsWith(".dll")) return "Loader DLL - review";
            return "Unknown - review";
        }
        public static InstallationInventory Scan(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("No server directory selected.");
            string root = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!Directory.Exists(root)) throw new DirectoryNotFoundException("Missing directory: " + root);
            var report = new InstallationInventory { Root = root };
            if ((File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0)
            { report.Warn("The installation root is a linked directory; scan stopped."); return report; }
            if (!File.Exists(Path.Combine(root, "enshrouded_server.exe")))
                report.Warnings.Add("No dedicated-server executable in this directory. Verify the selected path.");
            string saveSetting = ConfigReader.Read(root).SaveDirectory;
            if (string.IsNullOrWhiteSpace(saveSetting)) saveSetting = "./savegame";
            try
            {
                report.SavePath = Path.GetFullPath(Path.IsPathRooted(saveSetting) ?
                    saveSetting : Path.Combine(root, saveSetting));
                report.ExternalSaves = !Inside(report.SavePath, root);
                if (report.ExternalSaves) report.Warnings.Add("Configured saves are outside the scan root; not inspected: " + report.SavePath);
            }
            catch (Exception ex)
            {
                report.SavePath = Path.Combine(root, "savegame");
                report.Warn("Could not resolve save path: " + ex.Message);
            }
            var stack = new Stack<Tuple<string, int>>();
            stack.Push(Tuple.Create(root, 0));
            int seen = 0;
            while (stack.Count > 0)
            {
                var next = stack.Pop();
                if (next.Item2 > DepthLimit) { report.Warn("Depth limit at: " + next.Item1); continue; }
                report.Folders++;
                try
                {
                    foreach (string entry in Directory.EnumerateFileSystemEntries(next.Item1))
                    {
                        if (++seen > Limit) { report.Warn("Scan limit reached; inventory incomplete."); return report; }
                        try
                        {
                            var attrs = File.GetAttributes(entry);
                            string rel = entry.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                            if ((attrs & FileAttributes.ReparsePoint) != 0)
                            { report.Linked++; report.Sample("Linked entry not followed: " + rel); continue; }
                            if ((attrs & FileAttributes.Directory) != 0)
                            { stack.Push(Tuple.Create(entry, next.Item2 + 1)); continue; }
                            string group = Category(entry, rel, report.SavePath);
                            int n; report.Groups.TryGetValue(group, out n); report.Groups[group] = n + 1;
                            if (group.EndsWith("review", StringComparison.Ordinal))
                            { report.ReviewFiles++; report.Sample(group + ": " + rel); }
                            report.Files++;
                            report.TotalBytes += new FileInfo(entry).Length;
                        }
                        catch (Exception ex) { report.Warn("Unreadable file: " + ex.Message); }
                    }
                }
                catch (Exception ex) { report.Warn("Cannot list directory: " + next.Item1 + " (" + ex.Message + ")"); }
            }
            return report;
        }
        public string Describe()
        {
            var s = new StringBuilder();
            s.AppendLine("FILES AND FOLDERS - READ-ONLY INVENTORY");
            s.AppendLine("Server directory: " + Root);
            s.AppendLine("Configured saves: " + SavePath);
            if (ExternalSaves) s.AppendLine("External saves NOT scanned.");
            s.AppendLine();
            s.AppendLine("Files: " + Files + "   Folders: " + Folders + "   Size: " + FormatUtil.Bytes(TotalBytes));
            s.AppendLine("Files needing review: " + ReviewFiles + "   Linked entries skipped: " + Linked);
            s.AppendLine(Partial ? "INCOMPLETE INVENTORY - no migration possible" : "Scan completed (metadata only)");
            s.AppendLine();
            s.AppendLine("GROUPS (for review only)");
            foreach (var g in Groups) s.AppendLine("  " + g.Key + ": " + g.Value + " files");
            s.AppendLine();
            s.AppendLine("POSSIBLE LAYOUT - NO FILES MOVED");
            s.AppendLine("  Server/  Manager/  Mods/  Saves/  Backups/  Logs/  Staging/  Archive/");
            s.AppendLine("Official server files must remain in their game-required locations.");
            if (Warnings.Count > 0)
            { s.AppendLine(); s.AppendLine("WARNINGS"); foreach (string w in Warnings) s.AppendLine("  - " + w); }
            if (Samples.Count > 0)
            { s.AppendLine(); s.AppendLine("EXAMPLES TO REVIEW"); foreach (string x in Samples) s.AppendLine("  - " + x); }
            s.AppendLine();
            s.AppendLine("No file was created, moved, deleted, renamed or modified.");
            s.AppendLine("Classification is not authorization to reorganize folders.");
            return s.ToString();
        }
    }
}
