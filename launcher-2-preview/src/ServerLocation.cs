using System;
using System.IO;
using System.Web.Script.Serialization;

namespace XHLLauncher
{
    // Explicit server-folder choices are resolved at startup, never during a live session.
    public static class ServerLocation
    {
        public static string Validate(string folder)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(folder)) return "Select an existing server folder.";
                string root = Path.GetFullPath(folder);
                if (!Directory.Exists(root)) return "The chosen folder no longer exists.";
                for (var current = new DirectoryInfo(root); current != null; current = current.Parent)
                    if (current.Exists && (current.Attributes & FileAttributes.ReparsePoint) != 0)
                        return "Linked and junction directory paths require separate validation.";
                if (!File.Exists(Path.Combine(root, "enshrouded_server.exe")))
                    return "This folder does not contain enshrouded_server.exe.";
                string configuration = Path.Combine(root, "enshrouded_server.json");
                if (!File.Exists(configuration)) return "The dedicated-server configuration is missing.";
                object data = new JavaScriptSerializer().DeserializeObject(File.ReadAllText(configuration));
                if (!(data is System.Collections.Generic.Dictionary<string, object>))
                    return "The game server configuration does not contain a JSON object.";
                return null;
            }
            catch (Exception ex) { return "Could not validate server location: " + ex.Message; }
        }

        public static string Resolve(LauncherSettings settings, string launcherHint)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            // A manually selected folder is authoritative: never silently fall back
            // to the previous folder when the preferred location becomes unavailable.
            if (!string.IsNullOrWhiteSpace(settings.PreferredServerRoot))
            {
                string error = Validate(settings.PreferredServerRoot);
                if (error != null)
                    throw new IOException("Your selected server location is unavailable: " + error +
                        "\r\nNo alternate installation was selected automatically.");
                return Path.GetFullPath(settings.PreferredServerRoot);
            }

            // Legacy startup wrappers passed their server folder as a positional hint.
            // Preserve that behavior for installations without an explicit selection.
            if (!string.IsNullOrWhiteSpace(launcherHint))
            {
                string root = launcherHint.Trim().Trim('"');
                if (Validate(root) == null) return Path.GetFullPath(root);
            }
            return settings.ServerRoot;
        }
    }
}
