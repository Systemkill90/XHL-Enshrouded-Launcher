using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace XHLLauncher
{
    internal static class StagedUpdate
    {
        internal static bool Confirmed(int exitCode, string output, string stage)
        {
            return exitCode == 0 && output.IndexOf("Success! App '2278520' fully installed", StringComparison.OrdinalIgnoreCase) >= 0 &&
                File.Exists(Path.Combine(stage, "enshrouded_server.exe")) && File.Exists(Path.Combine(stage, "enshrouded_server.kfc")) && File.Exists(Path.Combine(stage, "enshrouded_server.kfc_resources"));
        }
        internal static ActionResult Download(string steamCmd, string appBase, string serverRoot)
        {
            try
            {
                if (!File.Exists(steamCmd) || !Path.GetFileName(steamCmd).Equals("steamcmd.exe", StringComparison.OrdinalIgnoreCase)) return ActionResult.Fail("Select the official steamcmd.exe first.");
                string root = Path.GetFullPath(serverRoot).TrimEnd(Path.DirectorySeparatorChar);
                string outputRoot = Path.GetFullPath(Path.Combine(appBase, "Staged-Updates"));
                if (outputRoot.Equals(root, StringComparison.OrdinalIgnoreCase) || outputRoot.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return ActionResult.Fail("The update workspace must be outside the live server.");
                for (var d = new DirectoryInfo(outputRoot); d != null; d = d.Parent)
                    if (d.Exists && (d.Attributes & FileAttributes.ReparsePoint) != 0) return ActionResult.Fail("Linked update directories are not supported.");
                string job = Path.Combine(outputRoot, Guid.NewGuid().ToString("N"));
                string client = Path.Combine(job, "client"); string stage = Path.Combine(job, "server");
                Directory.CreateDirectory(client); Directory.CreateDirectory(stage);
                string copy = Path.Combine(client, "steamcmd.exe"); File.Copy(steamCmd, copy, false);
                var info = new ProcessStartInfo(copy, "+force_install_dir \"" + stage + "\" +login anonymous +app_update 2278520 validate +quit") {
                    WorkingDirectory = client, UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true, RedirectStandardError = true };
                var output = new StringBuilder(); object gate = new object();
                using (var p = new Process { StartInfo = info })
                using (var log = new StreamWriter(Path.Combine(job, "download.log")))
                {
                    DataReceivedEventHandler record = delegate(object sender, DataReceivedEventArgs e) { if (e.Data == null) return; lock (gate) { log.WriteLine(e.Data); log.Flush(); output.AppendLine(e.Data); if (output.Length > 262144) output.Remove(0, output.Length - 262144); } };
                    p.OutputDataReceived += record; p.ErrorDataReceived += record;
                    p.Start(); p.BeginOutputReadLine(); p.BeginErrorReadLine(); p.WaitForExit();
                    if (!Confirmed(p.ExitCode, output.ToString(), stage)) return ActionResult.Fail("Download was not confirmed. Review " + Path.Combine(job, "download.log"));
                }
                File.WriteAllText(Path.Combine(job, "compatibility.txt"), InstallationAudit.Inspect(stage).Details);
                return ActionResult.Ok("Update downloaded to " + stage + "\r\nNot deployed: all three mods must be verified for that build first. Live server and saves were not changed.");
            }
            catch (Exception ex) { return ActionResult.Fail("Staged update failed: " + ex.Message); }
        }
    }
}
