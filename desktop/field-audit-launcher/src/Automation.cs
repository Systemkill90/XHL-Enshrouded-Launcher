using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Script.Serialization;

namespace XHLLauncher
{
    // An automatic recovery requires a previously observed running session.
    // One attempt per session prevents a broken installation entering a restart loop.
    internal sealed class RecoveryPolicy
    {
#if CLASSIC_TOOLS
        internal static readonly bool Available = false;
#else
        internal static readonly bool Available = true;
#endif
        bool armed;
        DateTime stopped = DateTime.MinValue;
        internal void Disarm() { armed = false; stopped = DateTime.MinValue; }
        internal bool Poll(bool enabled, bool running, bool busy, DateTime now)
        {
            if (!Available || !enabled) { Disarm(); return false; }
            if (busy) return false;
            if (running) { armed = true; stopped = DateTime.MinValue; return false; }
            if (!armed) return false;
            if (stopped == DateTime.MinValue) { stopped = now; return false; }
            if (now - stopped < TimeSpan.FromSeconds(60)) return false;
            Disarm(); return true;
        }
    }

    internal static class SaveBackup
    {
        static string Digest(string path)
        {
            using (var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(input)).Replace("-", "");
        }
        static void RejectLinks(string path)
        {
            for (var d = new DirectoryInfo(path); d != null; d = d.Parent)
                if ((d.Attributes & FileAttributes.ReparsePoint) != 0) throw new IOException("Linked directories are not supported for backups.");
        }
        static List<string> Files(string root)
        {
            var result = new List<string>();
            foreach (string entry in Directory.GetFileSystemEntries(root))
            {
                var attrs = File.GetAttributes(entry);
                if ((attrs & FileAttributes.ReparsePoint) != 0) throw new IOException("Linked save entries are not supported.");
                if ((attrs & FileAttributes.Directory) != 0) result.AddRange(Files(entry)); else result.Add(entry);
            }
            result.Sort(StringComparer.OrdinalIgnoreCase); return result;
        }
        internal static ActionResult Create(string serverRoot, string destinationRoot, bool serverRunning)
        {
            string snapshot = null;
            try
            {
                string config = Path.Combine(serverRoot, "enshrouded_server.json");
                string configText = File.ReadAllText(config);
                var json = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(configText);
                object value;
                if (json == null || !json.TryGetValue("saveDirectory", out value) || !(value is string) || string.IsNullOrWhiteSpace((string)value))
                    return ActionResult.Fail("The server configuration must specify a saveDirectory.");
                string saves = Path.GetFullPath(Path.Combine(serverRoot, (string)value)).TrimEnd(Path.DirectorySeparatorChar);
                string destination = Path.GetFullPath(destinationRoot).TrimEnd(Path.DirectorySeparatorChar);
                string installation = Path.GetFullPath(serverRoot).TrimEnd(Path.DirectorySeparatorChar);
                if (destination.Equals(saves, StringComparison.OrdinalIgnoreCase) || destination.StartsWith(saves + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    destination.Equals(installation, StringComparison.OrdinalIgnoreCase) || destination.StartsWith(installation + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    return ActionResult.Fail("Backups must be outside the server and save directories.");
                RejectLinks(saves);
                // Validate existing destination ancestors before creating anything.
                var ancestor = new DirectoryInfo(destination);
                while (ancestor != null && !ancestor.Exists) ancestor = ancestor.Parent;
                if (ancestor != null) RejectLinks(ancestor.FullName);
                var files = Files(saves);
                if (files.Count == 0) return ActionResult.Fail("No save files found; no empty backup created.");
                snapshot = Path.Combine(destination, DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(snapshot);
                File.WriteAllText(Path.Combine(snapshot, "INCOMPLETE.txt"), "This copy is not verified. Do not restore it automatically.");
                var manifest = new Dictionary<string, string>();
                foreach (string file in files)
                {
                    string relative = file.Substring(saves.Length + 1);
                    string target = Path.Combine(snapshot, "saves", relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    string before = Digest(file);
                    using (var input = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    using (var output = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None)) input.CopyTo(output);
                    if (before != Digest(target) || before != Digest(file)) throw new IOException("A save changed during copying; backup left marked incomplete.");
                    manifest.Add(relative, before);
                }
                if (!files.SequenceEqual(Files(saves), StringComparer.OrdinalIgnoreCase)) throw new IOException("The save file list changed during copying.");
                foreach (string file in files) if (Digest(file) != manifest[file.Substring(saves.Length + 1)]) throw new IOException("Saves changed during final verification.");
                if (File.ReadAllText(config) != configText) throw new IOException("Server configuration changed during copying.");
                File.WriteAllText(Path.Combine(snapshot, "enshrouded_server.json"), configText);
                File.WriteAllText(Path.Combine(snapshot, "manifest.json"), new JavaScriptSerializer().Serialize(new { CreatedUtc = DateTime.UtcNow, ServerObservedRunning = serverRunning,
                    Consistency = "Byte-verified stable file copies; cross-file game consistency not guaranteed for a live server.", Files = manifest }));
                // Completion is a separate marker. No source files or prior backups are ever deleted.
                File.WriteAllText(Path.Combine(snapshot, "VERIFIED.txt"), "Byte verification completed. This marker supersedes INCOMPLETE.txt. Live copies are not transactional game snapshots.");
                return ActionResult.Ok("Backup verified: " + snapshot + (serverRunning ? "\r\nLive file copy: game-level consistency is not guaranteed." : ""));
            }
            catch (Exception ex) { return ActionResult.Fail("Backup failed: " + ex.Message + (snapshot == null ? "" : "\r\nIncomplete copy retained: " + snapshot)); }
        }
    }
}
