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
            string attestation = "{\"ExeSha256\":\"" + InstallationAudit.Hash(Path.Combine(server, "enshrouded_server.exe")) + "\",\"KfcSha256\":\"" + InstallationAudit.Hash(Path.Combine(server, "enshrouded_server.kfc")) + "\",\"ResourcesSha256\":\"" + InstallationAudit.Hash(Path.Combine(server, "enshrouded_server.kfc_resources")) + "\"}";
            File.WriteAllText(Path.Combine(server, "XHL-STACK-1.0-ATTESTATION.json"), attestation);
            File.WriteAllText(Path.Combine(server, "XHL-SAFE-MODE.flag"), "fixture");
            var matching = InstallationAudit.Inspect(server);
            Require(matching.StackMatches && matching.LoadersIsolated && !matching.CanStart, "Matching attestation cannot override unsupported Flight build");
            File.WriteAllText(Path.Combine(server, "dbghelp.dll"), "fixture");
            Require(!InstallationAudit.Inspect(server).LoadersIsolated, "Legacy loader is detected without renaming or deleting it");
            File.AppendAllText(Path.Combine(server, "enshrouded_server.kfc"), "changed");
            Require(!InstallationAudit.Inspect(server).StackMatches, "Changed KFC invalidates stacking attestation");
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
}
