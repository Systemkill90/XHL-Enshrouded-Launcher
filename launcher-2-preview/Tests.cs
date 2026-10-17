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
    static void CheckAutomation(string server, string output, string savePath)
    {
        var policy = new RecoveryPolicy(); var now = DateTime.UtcNow;
        Require(!policy.Poll(true, false, false, now.AddHours(1)), "Recovery never starts an offline installation merely on opening");
        policy.Poll(true, true, false, now); policy.Poll(true, false, false, now);
        Require(!policy.Poll(true, false, false, now.AddSeconds(59)), "Recovery waits 60 seconds");
        Require(!policy.Poll(true, false, true, now.AddSeconds(61)), "Recovery waits for operations to finish");
        Require(policy.Poll(true, false, false, now.AddSeconds(62)) && !policy.Poll(true, false, false, now.AddMinutes(5)), "Recovery makes one attempt per observed session");
        policy.Poll(true, true, false, now); policy.Disarm();
        Require(!policy.Poll(true, false, false, now.AddMinutes(5)), "Intentional stop disarms recovery");
        File.WriteAllText(Path.Combine(server, "enshrouded_server.json"), "{\"saveDirectory\":\"./savegame\"}");
        string before = InstallationAudit.Hash(savePath);
        string backups = Path.Combine(output, "backup-fixture-" + Guid.NewGuid().ToString("N"));
        Require(SaveBackup.Create(server, backups, false).Success, "Configured saves copied and verified");
        string first = Directory.GetDirectories(backups)[0];
        Require(File.Exists(Path.Combine(first, "VERIFIED.txt")) && InstallationAudit.Hash(Path.Combine(first, "saves", "preservation-test")) == before, "Backup bytes match source with completion marker");
        Require(SaveBackup.Create(server, backups, true).Success && Directory.GetDirectories(backups).Length == 2, "Next backup retains previous backup");
        Require(!SaveBackup.Create(server, Path.Combine(server, "savegame", "bad"), false).Success, "Backup cannot write inside source saves");
        Require(InstallationAudit.Hash(savePath) == before, "Backup leaves original save unchanged");
        Require(!StagedUpdate.Confirmed(1, "Success! App '2278520' fully installed", server), "Failed updater exit cannot claim success");
        Require(!StagedUpdate.Confirmed(0, "Another app fully installed", server), "Updater requires exact app success evidence");
        Require(!UpdateActivation.Prepare(server, server, output, delegate { return false; }).Success, "Update activation rejects the current installation as destination");
        string staged = Path.Combine(output, "Staged-Updates", "fixture-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(staged);
        Require(!UpdateActivation.Prepare(server, staged, output, delegate { return true; }).Success, "Update activation refuses while a server runs");
        Require(!UpdateActivation.Prepare(server, staged, output, delegate { return false; }).Success, "Update activation refuses unverified mod files");
        Require(StagedUpdate.Confirmed(0, "Success! App '2278520' fully installed", server), "Updater requires app evidence and all three output files");
    }
    static void CheckPhone() {
        int calls=0; using(var phone=new PhoneView("test-secret", delegate(string op){calls++;return ActionResult.Ok(op);})) {
            phone.Start(System.Net.IPAddress.Loopback,0);phone.Publish(new {name="Fixture",online=true});
            using(var c=new System.Net.WebClient()) {
                Require(c.DownloadString(phone.Url).Contains("Server controls"), "Phone page serves server controls");
                Require(c.DownloadString(phone.Url+"status").Contains("Fixture"), "Phone status returns published data");
                bool rejected=false;try{c.DownloadString(phone.Url.Replace("test-secret","wrong"));}catch(System.Net.WebException){rejected=true;}Require(rejected,"Phone rejects requests without private key");
                rejected=false;try{c.UploadString(phone.Url,"POST","");}catch(System.Net.WebException){rejected=true;}Require(rejected,"Phone rejects unsupported write requests");
                rejected=false;try{c.UploadString(phone.Url+"command/Stop","POST","");}catch(System.Net.WebException){rejected=true;}Require(rejected && calls==0,"Phone rejects commands without authentication header");
                c.Headers["X-XHL-Key"]="test-secret";c.Headers["Origin"]="http://foreign.example";
                rejected=false;try{c.UploadString(phone.Url+"command/Stop","POST","");}catch(System.Net.WebException){rejected=true;}Require(rejected && calls==0,"Phone rejects foreign origin");
                c.Headers.Remove("Origin");
                Require(c.UploadString(phone.Url+"command/Start","POST","").Contains("Start") && calls==1,"Authenticated command reaches handler once");
                rejected=false;try{c.DownloadString(phone.Url+"command/Stop");}catch(System.Net.WebException){rejected=true;}Require(rejected && calls==1,"GET never executes a command");
                rejected=false;try{c.UploadString(phone.Url+"command/Delete","POST","");}catch(System.Net.WebException){rejected=true;}Require(rejected && calls==1,"Unknown command is rejected");
                foreach(string op in new[]{"Backup","AutoBackupOn","AutoBackupOff","RecoveryOn","RecoveryOff"}) {int before=calls;Require(c.UploadString(phone.Url+"command/"+op,"POST","").Contains(op)&&calls==before+1,"Phone dispatches authenticated "+op);}

            }
        }
    }
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
            string beforeInventory = InstallationAudit.Hash(savePath);
            var filesReport = InstallationInventory.Scan(server);
            Require(filesReport.Files >= 4 && filesReport.TotalBytes > 0, "Read-only scan counts server files");
            Require(filesReport.Groups.ContainsKey("World saves - protected"), "Scan protects configured saves");
            Require(filesReport.Groups.ContainsKey("Game server - protected"), "Scan identifies game runtime");
            Require(filesReport.ReviewFiles > 0, "Scan flags unknown and potentially stale files");
            Require(filesReport.Describe().Contains("No file was created, moved, deleted, renamed or modified."), "Inventory visibly promises read-only operation");
            Require(InstallationAudit.Hash(savePath) == beforeInventory, "Inventory leaves existing saves unchanged");
            // Migration preview only inspects files. No user installation is present in fixtures.
            File.WriteAllText(Path.Combine(server, "enshrouded_server.json"), "{\"saveDirectory\":\"./savegame\"}");
            string oldArchive = Path.Combine(server, "XHL-Launcher-v1.0.4.zip");
            File.WriteAllBytes(oldArchive, new byte[] { 1, 4, 9, 16 });
            string saveBeforePreview = InstallationAudit.Hash(savePath);
            string archiveBeforePreview = InstallationAudit.Hash(oldArchive);
            var migration = MigrationPlanner.Create(server);
            Require(migration.Complete, "Clean fixture can produce a full organization preview");
            Require(migration.Proposals.Count == 1, "Recognized historical launcher archive is listed for review");
            Require(!migration.Proposals[0].Blocked, "Unreferenced fixture archive receives a non-destructive suggestion");
            Require(migration.Proposals[0].Destination.EndsWith(Path.Combine("Manager", "Archive", Path.GetFileName(oldArchive))), "Archive proposed inside owner-managed folder");
            Require(migration.Proposals[0].Fingerprint == archiveBeforePreview, "Preview fingerprints candidate without altering it");
            Require(!Directory.Exists(Path.Combine(server, "Manager")), "Preview creates no manager directories");
            Require(InstallationAudit.Hash(savePath) == saveBeforePreview, "Preview leaves configured world saves byte-identical");
            Require(InstallationAudit.Hash(oldArchive) == archiveBeforePreview, "Preview leaves potential archive unchanged");
            File.WriteAllText(Path.Combine(server, "REFERENCE.ps1"), "Write-Output 'XHL-Launcher-v1.0.4.zip'");
            var referenced = MigrationPlanner.Create(server);
            Require(referenced.Proposals.Single(p => p.Source == "XHL-Launcher-v1.0.4.zip").Blocked, "Referenced archive is blocked from future moves");
            File.Delete(Path.Combine(server, "REFERENCE.ps1"));
            Directory.CreateDirectory(Path.Combine(server, "Manager", "Archive"));
            File.WriteAllText(Path.Combine(server, "Manager", "Archive", "XHL-Launcher-v1.0.4.zip"), "already exists");
            var collision = MigrationPlanner.Create(server);
            Require(collision.Proposals.Single(p => p.Source == "XHL-Launcher-v1.0.4.zip").Blocked, "Existing destination blocks proposed relocation");
            File.Delete(Path.Combine(server, "Manager", "Archive", "XHL-Launcher-v1.0.4.zip"));
            Directory.Delete(Path.Combine(server, "Manager", "Archive"));
            Directory.Delete(Path.Combine(server, "Manager"));
            File.WriteAllText(Path.Combine(server, "enshrouded_server.json"), "{bad json");
            var invalidConfig = MigrationPlanner.Create(server);
            Require(!invalidConfig.Complete, "Malformed game configuration prevents migration readiness");
            File.WriteAllText(Path.Combine(server, "enshrouded_server.json"), "{\"saveDirectory\":\"./savegame\"}");
            File.Delete(oldArchive);
            CheckLifecycle();
            CheckPhone();
            CheckAutomation(server, root, savePath);
            string sentinelHash = InstallationAudit.Hash(savePath);
            var settings = new LauncherSettings { ServerRoot = server, ServerName = "VELARIS", PublicIpLookup = false };
            using (var form = new ControlRoom(root, settings, false))
            {
                form.ClientSize = new Size(1384, 870);
                form.ShowInTaskbar = false; form.Opacity = 0; form.Show(); Application.DoEvents();
                form.SetAuditForTest(new InstallationAudit { Summary = "Compatibility needs attention", Details = "Fixture: unsupported executable. No live patch or save changes." });
                foreach (string page in new[] { "Overview", "Players", "World", "Mods", "Logs", "Installation", "Files", "Automation", "Phone" })
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
                controls.Navigate("Automation"); Application.DoEvents();
                using (var bmp = new Bitmap(controls.Width, controls.Height)) { controls.DrawToBitmap(bmp, new Rectangle(0, 0, controls.Width, controls.Height)); bmp.Save(Path.Combine(root, "Automation-controls.png"), ImageFormat.Png); }
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
