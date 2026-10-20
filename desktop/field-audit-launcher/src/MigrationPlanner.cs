using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace XHLLauncher
{
    // The planner is intentionally unable to modify a directory.
    // Execution will require a separate reviewed, transactional component.
    public sealed class MigrationPreview
    {
        public sealed class Proposal
        {
            public string Source;
            public string Destination;
            public string Reason;
            public string Fingerprint;
            public long Bytes;
            public bool Blocked;
        }

        public string Root;
        public string SavePath;
        public bool Complete = true;
        public bool ServerProcessObserved;
        public int ExaminedFiles;
        public readonly List<string> Warnings = new List<string>();
        public readonly List<Proposal> Proposals = new List<Proposal>();

        public string Describe()
        {
            var text = new StringBuilder();
            text.AppendLine("DIRECTORY ORGANIZATION PREVIEW — NOTHING IS CHANGED");
            text.AppendLine("Existing server: " + Root);
            text.AppendLine("Configured saves: " + SavePath);
            text.AppendLine("Files inspected: " + ExaminedFiles);
            text.AppendLine("Server process detected: " + (ServerProcessObserved ? "YES (read-only planning is allowed)" : "No process detected by name"));
            text.AppendLine("Inventory quality: " + (Complete ? "Complete within scan limits" : "INCOMPLETE — no migration allowed"));
            text.AppendLine();
            text.AppendLine("PRESERVED IN PLACE");
            text.AppendLine("  Official game and Steam files, world saves, backups, mods and DLLs");
            text.AppendLine("  Server configuration and launcher startup/control scripts");
            text.AppendLine("  Active logs, unknown files and directories with linked paths");
            text.AppendLine();
            text.AppendLine("PROPOSED FOR MANUAL REVIEW (NOT APPROVED)");
            if (Proposals.Count == 0) text.AppendLine("  No conservative cleanup candidates detected.");
            foreach (Proposal item in Proposals)
            {
                text.AppendLine("  " + (item.Blocked ? "BLOCKED: " : "REVIEW: ") + item.Source);
                text.AppendLine("    Possible destination: " + item.Destination);
                text.AppendLine("    Why: " + item.Reason);
                text.AppendLine("    Bytes: " + item.Bytes);
                text.AppendLine("    Source SHA-256: " + item.Fingerprint);
            }
            if (Warnings.Count > 0)
            {
                text.AppendLine();
                text.AppendLine("WARNINGS AND REQUIRED CHECKS");
                foreach (string warning in Warnings) text.AppendLine("  - " + warning);
            }
            text.AppendLine();
            text.AppendLine("THE NEXT PHASE MUST REQUIRE:");
            text.AppendLine("  - The server and startup/update processes fully stopped.");
            text.AppendLine("  - Verified independent backup and enough free disk space.");
            text.AppendLine("  - Source/destination hash checks, conflict detection and journaled rollback.");
            text.AppendLine("  - Explicit approval for each move.");
            text.AppendLine();
            text.AppendLine("No files or directories were created, moved, renamed, overwritten or deleted.");
            return text.ToString();
        }
    }

    public static class MigrationPlanner
    {
        const int MaxFiles = 12000;
        const int MaxDepth = 16;
        const long MaxFingerprintBytes = 32L * 1024 * 1024;

        static bool Inside(string path, string parent)
        {
            string p = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string c = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return c.Equals(p, StringComparison.OrdinalIgnoreCase) ||
                c.StartsWith(p + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        static string HashStableFile(string path, long expectedSize, DateTime expectedWrite)
        {
            if (expectedSize > MaxFingerprintBytes) return "Not hashed (over 32 MiB; remains manual review)";
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete))
            using (var hash = SHA256.Create())
            {
                string digest = BitConverter.ToString(hash.ComputeHash(file)).Replace("-", "");
                var after = new FileInfo(path);
                if (after.Length != expectedSize || after.LastWriteTimeUtc != expectedWrite)
                    return "CHANGED WHILE READING";
                return digest;
            }
        }

        static bool IsPotentialArchive(string name)
        {
            string n = name.ToLowerInvariant();
            return n.EndsWith(".zip") &&
                (n.StartsWith("xhl-enshrouded-") || n.StartsWith("xhl-launcher-") ||
                 n.StartsWith("enshrouded-server-companion-"));
        }

        static List<string> FindReferences(string root, string fileName)
        {
            var references = new List<string>();
            foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly))
            {
                string extension = Path.GetExtension(file).ToLowerInvariant();
                if (extension != ".ps1" && extension != ".bat" &&
                    extension != ".cmd" && extension != ".json") continue;
                try
                {
                    if (new FileInfo(file).Length > 1024 * 1024) continue;
                    if (File.ReadAllText(file).IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0)
                        references.Add(Path.GetFileName(file));
                }
                catch { references.Add("A startup/config file could not be read"); }
            }
            return references;
        }

        public static MigrationPreview Create(string directory)
        {
            var inventory = InstallationInventory.Scan(directory);
            var plan = new MigrationPreview();
            plan.Root = inventory.Root;
            plan.SavePath = inventory.SavePath;
            plan.Complete = !inventory.Partial && inventory.Linked == 0;
            if (!plan.Complete)
                plan.Warnings.Add("The inventory was partial or contained directory links. Resolve before any move.");
            foreach (string warning in inventory.Warnings) plan.Warnings.Add(warning);

            // This check is observational only, not a condition for generating a preview.
            try { plan.ServerProcessObserved = Process.GetProcessesByName("enshrouded_server").Length > 0; }
            catch { plan.Warnings.Add("Could not verify server process state."); }

            // Saves can be configured outside the scanned tree; never follow or relocate them.
            if (inventory.ExternalSaves)
                plan.Warnings.Add("External saves are excluded from the proposed layout.");

            // Never silently infer that an absent or malformed config is safe to reorganize.
            string config = Path.Combine(plan.Root, "enshrouded_server.json");
            if (!File.Exists(config))
            {
                plan.Complete = false;
                plan.Warnings.Add("Server configuration missing. No migration may be approved.");
            }
            else
            {
                try
                {
                    var obj = new JavaScriptSerializer().DeserializeObject(File.ReadAllText(config));
                    if (!(obj is Dictionary<string, object>)) throw new InvalidDataException("Expected JSON object");
                }
                catch (Exception ex)
                {
                    plan.Complete = false;
                    plan.Warnings.Add("Server configuration cannot be parsed: " + ex.Message);
                }
            }

            // Candidate moves are deliberately limited to recognizable OLD ARCHIVE ZIPs
            // at the server root. Don't classify scripts, mods, DLLs or game files as safe.
            // This is still a review-only suggestion; there is NO executor in this build.
            var stack = new Stack<Tuple<string, int>>();
            stack.Push(Tuple.Create(plan.Root, 0));
            int seen = 0;
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current.Item2 > MaxDepth)
                {
                    plan.Complete = false;
                    plan.Warnings.Add("Folder-depth limit reached.");
                    continue;
                }
                try
                {
                    foreach (string path in Directory.EnumerateFileSystemEntries(current.Item1))
                    {
                        if (++seen > MaxFiles)
                        {
                            plan.Complete = false;
                            plan.Warnings.Add("File-count limit reached.");
                            return plan;
                        }
                        FileAttributes attr;
                        try { attr = File.GetAttributes(path); }
                        catch { plan.Complete = false; plan.Warnings.Add("A file attribute could not be read."); continue; }
                        if ((attr & FileAttributes.ReparsePoint) != 0)
                        {
                            plan.Complete = false;
                            plan.Warnings.Add("Linked path skipped. Do not migrate until reviewed.");
                            continue;
                        }
                        if ((attr & FileAttributes.Directory) != 0)
                        {
                            stack.Push(Tuple.Create(path, current.Item2 + 1));
                            continue;
                        }
                        plan.ExaminedFiles++;
                        string name = Path.GetFileName(path);
                        if (!string.Equals(Path.GetDirectoryName(path), plan.Root,
                            StringComparison.OrdinalIgnoreCase) || !IsPotentialArchive(name)) continue;
                        if (Inside(path, plan.SavePath)) continue;

                        var record = new MigrationPreview.Proposal();
                        record.Source = name;
                        record.Destination = Path.Combine(plan.Root, "Manager", "Archive", name);
                        record.Reason = "Looks like a previous launcher ZIP. Verify ownership and references before moving.";
                        try
                        {
                            var info = new FileInfo(path);
                            record.Bytes = info.Length;
                            record.Fingerprint = HashStableFile(path, info.Length, info.LastWriteTimeUtc);
                            if (record.Fingerprint.StartsWith("CHANGED") || record.Fingerprint.StartsWith("Not hashed"))
                            {
                                record.Blocked = true;
                                record.Reason = "Source cannot be safely fingerprinted yet. Keep in place.";
                            }
                        }
                        catch (Exception ex)
                        {
                            record.Blocked = true;
                            record.Fingerprint = "Unavailable";
                            record.Reason = "Could not verify source: " + ex.Message;
                        }

                        // Do not follow a destination that has been redirected by junctions.
                        var destination = Path.GetDirectoryName(record.Destination);
                        for (var parent = new DirectoryInfo(destination);
                            parent != null && Inside(parent.FullName, plan.Root); parent = parent.Parent)
                        {
                            if (parent.Exists && (parent.Attributes & FileAttributes.ReparsePoint) != 0)
                            {
                                record.Blocked = true;
                                record.Reason = "Proposed destination contains a linked directory.";
                                break;
                            }
                        }
                        if (File.Exists(record.Destination) || Directory.Exists(record.Destination))
                        {
                            record.Blocked = true;
                            record.Reason = "Destination already exists; never overwrite.";
                        }
                        List<string> references = FindReferences(plan.Root, name);
                        if (references.Count != 0)
                        {
                            record.Blocked = true;
                            record.Reason = "Possible references found in " +
                                string.Join(", ", references.Take(6).ToArray()) + "; keep original.";
                        }
                        plan.Proposals.Add(record);
                    }
                }
                catch (Exception ex)
                {
                    plan.Complete = false;
                    plan.Warnings.Add("Cannot list path: " + current.Item1 + " (" + ex.Message + ")");
                }
            }
            return plan;
        }
    }
}
