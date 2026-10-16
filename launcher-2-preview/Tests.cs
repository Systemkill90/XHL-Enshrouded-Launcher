using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XHLLauncher;

static class Checks
{
    static int passed;
    static void Require(bool condition, string message) { if (!condition) throw new Exception(message); Console.WriteLine("PASS " + message); passed++; }
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            string root = Path.GetFullPath(args[0]); Directory.CreateDirectory(root);
            string server = Path.Combine(root, "fixture-server-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(server);
            Directory.CreateDirectory(Path.Combine(server, "savegame"));
            byte[] save = { 0, 12, 99, 255, 32, 14 };
            string savePath = Path.Combine(server, "savegame", "preservation-test"); File.WriteAllBytes(savePath, save);
            var missing = InstallationAudit.Inspect(server);
            Require(!missing.CanStart, "Missing executable fails closed");
            File.WriteAllText(Path.Combine(server, "enshrouded_server.exe"), "Synthetic test file; never execute");
            var unknown = InstallationAudit.Inspect(server);
            Require(!unknown.FlightCompatible && !unknown.CanStart, "Unknown executable is unsupported");
            File.WriteAllText(Path.Combine(server, "XHL-STACK-1.0-ATTESTATION.json"), "{}");
            Require(!InstallationAudit.Inspect(server).StackMatches, "Empty attestation is not proof of stacking");
            File.WriteAllText(Path.Combine(server, "XHL-STACK-1.0-ATTESTATION.json"), "not json");
            Require(!InstallationAudit.Inspect(server).CanStart, "Malformed attestation fails closed");
            File.WriteAllText(Path.Combine(server, "enshrouded_server.kfc"), "Fixture KFC");
            File.WriteAllText(Path.Combine(server, "enshrouded_server.kfc_resources"), "Fixture resources");
            string attestation = "{\"Format\":\"XHL-EML-OFFLINE-STAGING-v1\",\"MaxStack\":65535,\"ExeSha256\":\"" + InstallationAudit.Hash(Path.Combine(server, "enshrouded_server.exe")) + "\",\"KfcSha256\":\"" + InstallationAudit.Hash(Path.Combine(server, "enshrouded_server.kfc")) + "\",\"ResourcesSha256\":\"" + InstallationAudit.Hash(Path.Combine(server, "enshrouded_server.kfc_resources")) + "\"}";
            File.WriteAllText(Path.Combine(server, "XHL-STACK-1.0-ATTESTATION.json"), attestation);
            File.WriteAllText(Path.Combine(server, "XHL-SAFE-MODE.flag"), "fixture");
            var matching = InstallationAudit.Inspect(server);
            Require(matching.StackMatches && matching.LoadersIsolated && !matching.CanStart, "Matching attestation cannot override unsupported Flight build");
            File.WriteAllText(Path.Combine(server, "XHL-STACK-1.0-ATTESTATION.json"), attestation.Replace("65535", "100"));
            Require(!InstallationAudit.Inspect(server).StackMatches, "Incorrect stack size is rejected even when file hashes match");
            File.WriteAllText(Path.Combine(server, "XHL-STACK-1.0-ATTESTATION.json"), attestation);
            File.WriteAllText(Path.Combine(server, "dbghelp.dll"), "fixture");
            Require(!InstallationAudit.Inspect(server).LoadersIsolated, "Legacy loader is detected without renaming or deleting it");
            File.AppendAllText(Path.Combine(server, "enshrouded_server.kfc"), "changed");
            Require(!InstallationAudit.Inspect(server).StackMatches, "Changed KFC invalidates stacking attestation");
            CheckLifecycle();
            string sentinelHash = InstallationAudit.Hash(savePath);
            var settings = new LauncherSettings { ServerRoot = server, ServerName = "VELARIS", PublicIpLookup = false };
            using (var form = new ControlRoom(root, settings, false))
            {
                form.ClientSize = new Size(1384, 870);
                form.ShowInTaskbar = false; form.Opacity = 0; form.Show(); Application.DoEvents();
                form.SetAuditForTest(new InstallationAudit { Summary = "Compatibility needs attention", Details = "Fixture: unsupported executable. No live patch or save changes." });
                foreach (string page in new[] { "Overview", "Players", "World", "Mods", "Logs", "Installation" })
                {
                    form.Navigate(page); form.PerformLayout(); Application.DoEvents();
                    using (var bmp = new Bitmap(form.Width, form.Height)) { form.DrawToBitmap(bmp, new Rectangle(0, 0, form.Width, form.Height)); bmp.Save(Path.Combine(root, page + ".png"), ImageFormat.Png); }
                    Require(form.Controls.Count > 0 && form.Width > 1000, page + " constructs and renders (visual review also required)");
                }
                form.Navigate("Overview"); form.ClientSize = new Size(1084, 701); form.PerformLayout(); Application.DoEvents();
                Dump(form);
                using (var bmp = new Bitmap(form.Width, form.Height)) { form.DrawToBitmap(bmp, new Rectangle(0, 0, form.Width, form.Height)); bmp.Save(Path.Combine(root, "Overview-small.png"), ImageFormat.Png); }
            }
            using (var controls = new ControlRoom(root, settings, false, true))
            {
                controls.ClientSize = new Size(1384, 970); controls.ShowInTaskbar = false; controls.Opacity = 0; controls.Show(); Application.DoEvents();
                controls.SetAuditForTest(new InstallationAudit { Summary = "Compatibility needs attention", Details = "Fixture: no live operations are run." });
                using (var bmp = new Bitmap(controls.Width, controls.Height)) { controls.DrawToBitmap(bmp, new Rectangle(0, 0, controls.Width, controls.Height)); bmp.Save(Path.Combine(root, "Controls.png"), ImageFormat.Png); }
                Require(Buttons(controls).Where(b => b.Text == "Start" || b.Text == "Restart").All(b => !b.Enabled), "Start and restart remain disabled for unsupported fixture");
            }
            Require(sentinelHash == InstallationAudit.Hash(savePath), "Save sentinel unchanged after every page and audit");
            Require(!Directory.EnumerateFiles(server, "*", SearchOption.AllDirectories).Any(p => p.EndsWith(".log")), "Preview creates no server logs or runtime artifacts");
            Console.WriteLine(passed + " checks passed."); return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    static void Dump(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            if (!c.Visible) continue;
            if (c.Right > parent.ClientSize.Width + 2) throw new Exception("Layout overflow: " + c.GetType().Name);
            Dump(c);
        }
    }
    static System.Collections.Generic.IEnumerable<Button> Buttons(Control parent)
    {
        foreach (Control child in parent.Controls) { if (child is Button) yield return (Button)child; foreach (Button b in Buttons(child)) yield return b; }
    }
    sealed class FakeRuntime : IServerRuntime
    {
        public bool Running, Pending, FailStop;
        public int Starts, Stops;
        public bool IsRunning { get { return Running; } }
        public bool StartupPending { get { return Pending; } }
        public string StartupStatus { get { return ""; } }
        public ActionResult Start() { Starts++; Running = true; return ActionResult.Ok("fixture start"); }
        public ActionResult Stop() { Stops++; if (FailStop) return ActionResult.Fail("fixture stop timeout"); Running = false; return ActionResult.Ok("fixture stop"); }
    }
    static void CheckLifecycle()
    {
        var runtime = new FakeRuntime { Running = true };
        var blocked = new ServerLifecycle(runtime, delegate { return ActionResult.Fail("fixture incompatibility"); });
        Require(!blocked.Execute("Restart").Success && runtime.Running && runtime.Stops == 0, "Blocked restart leaves running server untouched");
        Require(!blocked.Execute("Start").Success && runtime.Starts == 0, "Duplicate start does not launch another process");
        runtime.Running = false;
        Require(!blocked.Execute("Start").Success && runtime.Starts == 0, "Failed preflight never calls start");
        runtime.Running = true; runtime.FailStop = true;
        var permitted = new ServerLifecycle(runtime, delegate { return ActionResult.Ok("fixture check"); });
        Require(!permitted.Execute("Restart").Success && runtime.Starts == 0 && runtime.Running, "Failed graceful stop prevents restart without forced termination");
        runtime.FailStop = false; runtime.Stops = 0; int calls = 0;
        var changed = new ServerLifecycle(runtime, delegate { calls++; return calls == 1 ? ActionResult.Ok("fixture check") : ActionResult.Fail("fixture files changed"); });
        Require(!changed.Execute("Restart").Success && runtime.Stops == 1 && runtime.Starts == 0, "Changed files after shutdown prevent restart");
        Require(permitted.Execute("Start").Success && runtime.Starts == 1, "Successful preflight requests one start");
        runtime.Pending = true;
        Require(!permitted.Execute("Restart").Success && runtime.Starts == 1, "Pending bootstrap rejects another operation");
        runtime.Pending = false;
        Require(!permitted.Execute("invalid").Success, "Unknown operation is rejected");
        Require(permitted.Execute("Stop").Success && !runtime.Running, "Stop requests the graceful runtime path");
        ServerLifecycle reentrant = null; bool rejected = false;
        reentrant = new ServerLifecycle(runtime, delegate { rejected = !reentrant.Execute("Stop").Success; return ActionResult.Ok("fixture check"); });
        Require(reentrant.Execute("Start").Success && rejected, "Concurrent operation is rejected while preflight is in progress");
    }
}
