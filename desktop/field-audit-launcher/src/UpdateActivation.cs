using System;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace XHLLauncher
{
    internal static class UpdateActivation
    {
        // Select a complete, verified side-by-side installation. Never replace the old one.
        internal static ActionResult Prepare(string current, string candidate, string appBase, Func<bool> anyServerRunning)
        {
            try
            {
                string oldRoot = Path.GetFullPath(current).TrimEnd(Path.DirectorySeparatorChar);
                string newRoot = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar);
                string allowed = Path.GetFullPath(Path.Combine(appBase, "Staged-Updates")).TrimEnd(Path.DirectorySeparatorChar);
                if (!newRoot.StartsWith(allowed + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || newRoot.Equals(oldRoot, StringComparison.OrdinalIgnoreCase))
                    return ActionResult.Fail("Choose a separate verified installation under this launcher's Staged-Updates folder.");
                for (var d = new DirectoryInfo(newRoot); d != null; d = d.Parent)
                    if ((d.Attributes & FileAttributes.ReparsePoint) != 0) return ActionResult.Fail("Linked update folders are not supported.");
                if (anyServerRunning()) return ActionResult.Fail("Stop the server before switching installations.");
                var audit = InstallationAudit.Inspect(newRoot);
                if (!audit.CanStart) return ActionResult.Fail("The update is not compatible with all three mods. " + audit.Details);
                foreach (string file in new[] { "XHL-Diagnostic-Wrapper.ps1", "XHL-Safe-Start.ps1", "XHL-1.0-compat-manifest.json" })
                    if (!File.Exists(Path.Combine(newRoot, file))) return ActionResult.Fail("Prepared update is missing " + file);
                string snapshotRoot = Path.Combine(appBase, "World-Backups", "before-update-" + Guid.NewGuid().ToString("N"));
                ActionResult backup = SaveBackup.Create(oldRoot, snapshotRoot, false);
                if (!backup.Success) return backup;
                if (anyServerRunning()) return ActionResult.Fail("Server started during backup; update selection cancelled.");
                string snapshot = Directory.GetDirectories(snapshotRoot).Single();
                string saveName = "savegame-import-" + Guid.NewGuid().ToString("N");
                string destination = Path.Combine(newRoot, saveName);
                CopyTree(Path.Combine(snapshot, "saves"), destination);
                var serializer = new JavaScriptSerializer();
                var config = serializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(Path.Combine(snapshot, "enshrouded_server.json")));
                config["saveDirectory"] = "./" + saveName; config["logDirectory"] = "./logs";
                if (anyServerRunning()) return ActionResult.Fail("Server started during preparation; update selection cancelled.");
                string configPath = Path.Combine(newRoot, "enshrouded_server.json");
                if (File.Exists(configPath)) File.Copy(configPath, configPath + ".before-" + Guid.NewGuid().ToString("N"), false);
                File.WriteAllText(configPath, serializer.Serialize(config));
                var ready = ServerLifecycle.CheckInstallation(newRoot);
                if (!ready.Success) return ready;
                return ActionResult.Ok("Update prepared with verified save copies. Previous installation and saves remain untouched.");
            }
            catch (Exception ex) { return ActionResult.Fail("Update activation preparation failed: " + ex.Message); }
        }
        static void CopyTree(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (string path in Directory.GetFiles(source))
            {
                string target = Path.Combine(destination, Path.GetFileName(path)); File.Copy(path, target, false);
                if (InstallationAudit.Hash(path) != InstallationAudit.Hash(target)) throw new IOException("Transferred save verification failed.");
            }
            foreach (string path in Directory.GetDirectories(source)) CopyTree(path, Path.Combine(destination, Path.GetFileName(path)));
        }
    }
}
