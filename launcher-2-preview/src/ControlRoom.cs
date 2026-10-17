using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XHLLauncher
{
    // Monitoring is the default; lifecycle controls require an explicit --controls launch.
    public sealed partial class ControlRoom : Form
    {
        readonly string appBase;
        readonly LauncherSettings settings;
        readonly TelemetryService telemetry;
        readonly bool controlsEnabled;
        readonly ServerLifecycle lifecycle;
        Button startServer, stopServer, restartServer;
        bool operationBusy, updateBusy;
        PhoneView phone;
        TextBox phoneLink;
        Label phoneStatus;
        readonly RecoveryPolicy recovery = new RecoveryPolicy();
        DateTime nextBackup = DateTime.UtcNow.AddMinutes(30);
        Label automationStatus, backupPathLabel;
        CheckBox recoveryOption, backupOption;
        DateTime phoneBackupScan = DateTime.MinValue;
        string lastPhoneBackup = "No verified backup found";
        string phoneWorldFiles="Checking", phoneWorldSize="Checking", phoneWorldSaved="Checking";
        string BackupRoot { get { return string.IsNullOrWhiteSpace(settings.BackupRoot) ? Path.Combine(appBase, "World-Backups") : Path.GetFullPath(settings.BackupRoot); } }
        Label operationStatus;
        readonly Dictionary<string, Control> pages = new Dictionary<string, Control>();
        readonly Dictionary<string, Button> navigation = new Dictionary<string, Button>();
        readonly Timer refresh = new Timer();
        InstallationAudit audit;
        Panel host;
        Label serverName, state, statusDetail, auditSummary, auditDetail, saveSummary, footer;
        Label stackState, flightState, giftingState, worldDetail;
        MetricCard players, cpu, memory, uptime;
        DataGridView playerGrid;
        TextBox logBox, auditBox, inventoryBox;
        TextBox search;
        Button recheck;
        string current = "Overview";
        bool auditBusy, paused;
        int frames;

        public ControlRoom(string appBase, LauncherSettings settings, bool startMonitoring, bool enableOperations = false)
        {
            this.appBase = appBase;
            this.settings = settings;
            nextBackup = DateTime.UtcNow.AddMinutes(Math.Max(5, settings.BackupIntervalMinutes));
            controlsEnabled = enableOperations;
            lifecycle = new ServerLifecycle(new ExistingServerRuntime(settings.ServerRoot, settings), delegate { return ServerLifecycle.CheckInstallation(settings.ServerRoot); });
            // The preview cannot auto-start a server or perform external identity lookups.
            // Recovery is opt-in and requires an observed running session.
            settings.PublicIpLookup = false;
            telemetry = new TelemetryService(appBase, settings);
            Text = "ShroudForge Launcher 2.0 — " + settings.ServerName;
            BackColor = Theme.Bg; ForeColor = Theme.Text; Font = Theme.Font(10, FontStyle.Regular);
            AutoScaleMode = AutoScaleMode.Dpi;
            MinimumSize = new Size(1060, 720); Size = new Size(1320, 900);
            StartPosition = FormStartPosition.CenterScreen;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            BuildShell();
            AddPage("Overview", BuildOverview());
            AddPage("Players", BuildPlayers());
            AddPage("World", BuildWorld());
            AddPage("Mods", BuildMods());
            AddPage("Logs", BuildLogs());
            AddPage("Installation", BuildInstallation());
            AddPage("Files", BuildFiles());
            AddPage("Automation", BuildAutomation());
            AddPage("Phone", BuildPhone());
            AddPage("Backups", BuildBackups());
            AddPage("Settings", BuildSettings());
            AddPage("Updates", BuildUpdates());
            Navigate("Overview");
            refresh.Interval = 1000;
            refresh.Tick += async delegate { frames++; UpdateState(); if(frames % 5 == 0) PublishPhone(); await TickAutomation(); if (current == "Logs" && !paused && frames % 3 == 0) ReadLogs(); };
            if (startMonitoring) { ConfigureTray(); telemetry.Start(); refresh.Start(); Shown += async delegate { await InspectAsync(); if (settings.PhoneViewEnabled) StartPhone(); }; }
            FormClosing += delegate(object sender, FormClosingEventArgs e) { if (operationBusy) { e.Cancel = true; MessageBox.Show("Wait for the current server operation to finish.", "ShroudForge Launcher"); } };
            FormClosed += delegate { refresh.Stop(); refresh.Dispose(); telemetry.Dispose(); if(phone!=null)phone.Dispose(); };
            UpdateState();
        }

        Label TextLabel(string text, float size, Color color, bool strong)
        { return Theme.Label(text, size, color, strong ? FontStyle.Bold : FontStyle.Regular); }

        void BuildShell() { BuildModernShell(); }
        Panel Page(string title, string description)
        {
            var page = new Panel { AutoScroll = true };
            var heading = TextLabel(title, 20, Theme.Text, false); heading.Location = new Point(0, 0); page.Controls.Add(heading);
            var sub = TextLabel(description, 9, Theme.Muted, false); sub.Location = new Point(1, 38); page.Controls.Add(sub);
            return page;
        }
        void AddPage(string name, Control page) { page.Dock = DockStyle.Fill; page.Visible = false; host.Controls.Add(page); pages.Add(name, page); }
        internal void Navigate(string page)
        {
            current = page;
            foreach (var entry in pages) entry.Value.Visible = entry.Key == page;
            foreach (var entry in navigation) { entry.Value.BackColor = entry.Key == page ? Theme.Panel3 : Theme.Sidebar; entry.Value.ForeColor = entry.Key == page ? Theme.Cyan : Theme.Muted; }
            UpdateState(); if (page == "Logs") ReadLogs(); if(page == "Backups") RefreshBackupList();
        }
        Panel Card(Control parent, string title, int x, int y, int width, int height)
        {
            var card = Theme.Card(); card.SetBounds(x, y, width, height); parent.Controls.Add(card);
            var name = TextLabel(title, 8, Theme.Muted, true); name.Location = new Point(20, 16); card.Controls.Add(name); return card;
        }
        Label Body(Panel parent, string text, int y, float size, Color color)
        {
            var label = TextLabel(text, size, color, false); label.Location = new Point(20, y);
            label.AutoSize = false; label.Size = new Size(Math.Max(100, parent.Width - 40), parent.Height - y - 12);
            label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; parent.Controls.Add(label); return label;
        }
        Button ActionButton(Control parent, string text, int x, int y, int width, Action action)
        {
            var button = Theme.Button(text, Theme.Border); button.SetBounds(x, y, width, 36); button.Click += delegate { action(); }; parent.Controls.Add(button); return button;
        }

        Control BuildOverview() { return BuildModernOverview(); }
        Control BuildPlayers()
        {
            var page = Page("Players", "Players identified in server logs and queries. Character inventories remain on game clients.");
            playerGrid = new DataGridView { Location = new Point(0, 78), Size = new Size(1000, 500), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, ReadOnly = true };
            Theme.StyleGrid(playerGrid); foreach (string column in new[] { "Player", "Ping", "Jitter", "Session", "Status" }) playerGrid.Columns.Add(column, column);
            page.Controls.Add(playerGrid); page.Resize += delegate { playerGrid.Size = new Size(Math.Max(100, page.ClientSize.Width - 20), Math.Max(120, page.ClientSize.Height - 90)); }; return page;
        }
        Control BuildWorld(){return BuildRichWorld();}
        Control BuildMods(){return BuildRichMods();}
        Control BuildLogs()
        {
            var page = Page("Logs", "Latest server log. Search locally or pause the view while the server continues.");
            search = new TextBox { Location = new Point(0, 78), Width = 360, AccessibleName = "Search log text" }; Theme.StyleTextBox(search); search.TextChanged += delegate { if (!paused) ReadLogs(); }; page.Controls.Add(search);
            var pause = ActionButton(page, "Pause view", 376, 72, 130, delegate { paused = !paused; });
            pause.Click += delegate { pause.Text = paused ? "Resume view" : "Pause view"; if (!paused) ReadLogs(); };
            logBox = new TextBox { Location = new Point(0, 124), Size = new Size(1000, 450), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Multiline = true, ReadOnly = true, WordWrap = false, ScrollBars = ScrollBars.Both, BackColor = Theme.Panel2, ForeColor = Theme.Text, Font = new Font("Consolas", 9), BorderStyle = BorderStyle.FixedSingle };
            page.Controls.Add(logBox); page.Resize += delegate { logBox.Size = new Size(Math.Max(100, page.ClientSize.Width - 20), Math.Max(120, page.ClientSize.Height - 140)); }; return page;
        }
        Control BuildInstallation()
        {
            var page = Page("Installation", "Read-only checks against the files on disk. No repairs run automatically.");
            var path = TextLabel(settings.ServerRoot, 9, Theme.Muted, false); path.Location = new Point(0, 80); path.MaximumSize = new Size(850, 48); page.Controls.Add(path);
            recheck = ActionButton(page, "Check again", 0, 137, 145, async delegate { await InspectAsync(); });
            ActionButton(page, "Server folder", 160, 137, 145, delegate { try { Process.Start("explorer.exe", "\"" + settings.ServerRoot + "\""); } catch (Exception ex) { MessageBox.Show(ex.Message); } });
            auditBox = new TextBox { Location = new Point(0, 195), Size = new Size(1000, 380), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, ReadOnly = true,
                Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = false, BackColor = Theme.Panel2, ForeColor = Theme.Text, Font = new Font("Consolas", 10), BorderStyle = BorderStyle.FixedSingle, Text = "Checking installation..." };
            page.Controls.Add(auditBox); page.Resize += delegate { auditBox.Size = new Size(Math.Max(100, page.ClientSize.Width - 20), Math.Max(120, page.ClientSize.Height - 210)); }; return page;
        }
        Control BuildFiles()
        {
            var page = Page("Files and folders", "Inspect the current server directory before reorganizing it.");
            var note = TextLabel("Read-only scan. No repairs, moves, renames or cleanup.", 10, Theme.Muted, false);
            note.Location = new Point(0, 75); note.AutoSize = false; note.Size = new Size(840, 30);
            page.Controls.Add(note);
            ActionButton(page, "Scan folders", 0, 120, 160, async delegate { await ScanFilesAsync(); });
            ActionButton(page, "Preview cleanup", 351, 120, 175, async delegate { await PreviewMigrationAsync(); });
            ActionButton(page, "Use server folder", 542, 120, 175, delegate { ChooseServerLocation(); });
            ActionButton(page,"Organize archives",542,72,175,async delegate {
                if(!controlsEnabled||operationBusy||updateBusy||lifecycle.StartupPending)return;
                if(MessageBox.Show("Move only unreferenced, hash-verified old launcher ZIP archives into Manager / Archive? The server must be stopped. A world backup and move journal are created first. Game files and unknown files stay in place.","Organize historical archives",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
                operationBusy=true;var result=await Task.Run(delegate{return Maintenance.Archive(settings.ServerRoot,BackupRoot);});operationBusy=false;inventoryBox.Text=result.Message;
            });
            ActionButton(page, "Copy report", 175, 120, 160, delegate {
                if (inventoryBox != null && !string.IsNullOrEmpty(inventoryBox.Text))
                    try { Clipboard.SetText(inventoryBox.Text); }
                    catch (Exception ex) { MessageBox.Show(ex.Message, "Copy report", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            });
            inventoryBox = new TextBox {
                Location = new Point(0, 174), Size = new Size(1000, 460),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = false,
                BackColor = Theme.Panel2, ForeColor = Theme.Text, Font = new Font("Consolas", 9),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Click Scan folders to inspect the installation without changing it."
            };
            page.Controls.Add(inventoryBox);
            page.Resize += delegate { inventoryBox.Size = new Size(Math.Max(100, page.ClientSize.Width - 20), Math.Max(130, page.ClientSize.Height - 190)); };
            return page;
        }
        async Task ScanFilesAsync()
        {
            if (inventoryBox == null || inventoryBox.Tag != null) return;
            inventoryBox.Tag = "busy";
            inventoryBox.Text = "Reading file metadata. No changes are being made...";
            try
            {
                string folder = settings.ServerRoot;
                var report = await Task.Run(delegate { return InstallationInventory.Scan(folder); });
                if (!IsDisposed) inventoryBox.Text = report.Describe();
            }
            catch (Exception ex) { if (!IsDisposed) inventoryBox.Text = "Scan failed: " + ex.Message + "\r\nNo files changed."; }
            finally { if (!IsDisposed) inventoryBox.Tag = null; }
        }
        void ChooseServerLocation()
        {
            if (!controlsEnabled || operationBusy || updateBusy || lifecycle.StartupPending) return;
            string selected;
            using (var picker = new FolderBrowserDialog {
                Description = "Choose an existing Enshrouded dedicated server folder. No server files will be moved."
            })
            {
                if (Directory.Exists(settings.ServerRoot)) picker.SelectedPath = settings.ServerRoot;
                if (picker.ShowDialog() != DialogResult.OK) return;
                selected = picker.SelectedPath;
            }
            string problem = ServerLocation.Validate(selected);
            if (problem != null)
            {
                MessageBox.Show(problem, "Server folder not accepted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show(
                "Use this existing folder the NEXT time the manager starts?\r\n\r\n" +
                selected + "\r\n\r\nNo files are moved and the currently running server is unchanged." +
                "\r\nKeep the present launcher open until your maintenance window.",
                "Remember server folder for next launch", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            settings.PreferredServerRoot = Path.GetFullPath(selected);
            Storage.SaveSettings(appBase, settings);
            if (inventoryBox != null)
                inventoryBox.Text = "The server folder for NEXT launch is now:\r\n" +
                    settings.PreferredServerRoot + "\r\n\r\nCurrent-session controls still use:\r\n" +
                    settings.ServerRoot + "\r\n\r\nClose and reopen the manager when ready. No files were changed.";
        }

        async Task PreviewMigrationAsync()
        {
            if (inventoryBox == null || inventoryBox.Tag != null) return;
            inventoryBox.Tag = "preview";
            inventoryBox.Text = "Preparing a read-only organization preview. No server files will be changed...";
            try
            {
                string folder = settings.ServerRoot;
                var result = await Task.Run(delegate { return MigrationPlanner.Create(folder); });
                if (!IsDisposed) inventoryBox.Text = result.Describe();
            }
            catch (Exception ex)
            {
                if (!IsDisposed) inventoryBox.Text = "Preview failed: " + ex.Message + "\r\nNothing was changed.";
            }
            finally { if (!IsDisposed) inventoryBox.Tag = null; }
        }
        Control BuildPhone()
        {
            var page=Page("Phone access", "Control your server from your couch on the same home Wi-Fi.");
            phoneLink=new TextBox {ReadOnly=true,Location=new Point(0,90),Width=730}; Theme.StyleTextBox(phoneLink); page.Controls.Add(phoneLink);
            ActionButton(page,"Copy link",0,140,140,delegate {if(phoneLink.Text.StartsWith("http"))Clipboard.SetText(phoneLink.Text);});
            ActionButton(page,"Enable",160,140,140,delegate {settings.PhoneViewEnabled=true;Storage.SaveSettings(appBase,settings);StartPhone();});
            ActionButton(page,"Disable",320,140,140,delegate {settings.PhoneViewEnabled=false;Storage.SaveSettings(appBase,settings);if(phone!=null){phone.Dispose();phone=null;}phoneLink.Text="";phoneStatus.Text="Phone access is off.";});
            phoneStatus=TextLabel("Phone access starts with the launcher. Keep this private link to yourself.",10,Theme.Muted,false);phoneStatus.Location=new Point(0,200);phoneStatus.MaximumSize=new Size(760,140);page.Controls.Add(phoneStatus);
            Body(Card(page,"CONNECTION",0,350,780,160),"Open the link in your phone browser while connected to the same Wi-Fi.\r\nIf Windows asks, allow this launcher on Private networks.\r\nKeep this control link private. No port forwarding or public internet exposure is needed.\r\nLeave the launcher open; minimize it when you leave the desk.",45,10,Theme.Text);
            return page;
        }
        void StartPhone()
        {
            if(phone!=null)return;
            try { if(string.IsNullOrEmpty(settings.PhoneViewKey)){settings.PhoneViewKey=Guid.NewGuid().ToString("N")+Guid.NewGuid().ToString("N");Storage.SaveSettings(appBase,settings);}
                phone=new PhoneView(settings.PhoneViewKey, PhoneCommand);phone.Start(PhoneView.LocalAddress(),8765);phoneLink.Text=phone.Url;phoneStatus.Text="Phone view is on. Open this private link on your home Wi-Fi.";PublishPhone();
            } catch(Exception ex){if(phone!=null)phone.Dispose();phone=null;phoneStatus.Text="Phone view could not start: "+ex.Message;}
        }
        void PublishPhone()
        {
            if(phone==null)return;var h=telemetry.GetHost();
            if(DateTime.UtcNow >= phoneBackupScan) {
                try {var markers=Directory.Exists(BackupRoot)?Directory.GetFiles(BackupRoot,"VERIFIED.txt",SearchOption.AllDirectories):new string[0];
                    lastPhoneBackup=markers.Length==0?"No verified backup found":markers.Select(File.GetLastWriteTime).Max().ToString("MMM d, h:mm tt");
                } catch {lastPhoneBackup="Backup history unavailable";}
                try {var cfg=ConfigReader.Read(settings.ServerRoot);var dir=Path.GetFullPath(Path.Combine(settings.ServerRoot,cfg.SaveDirectory));var files=new DirectoryInfo(dir).GetFiles();phoneWorldFiles=string.Join(", ",files.Select(f=>f.Name));phoneWorldSize=FormatUtil.Bytes(files.Sum(f=>f.Length));phoneWorldSaved=files.Length==0?"No save files found":files.Max(f=>f.LastWriteTime).ToString("MMM d, h:mm tt");}catch{phoneWorldFiles="Unavailable";phoneWorldSize="Unavailable";phoneWorldSaved="Unavailable";}
                phoneBackupScan=DateTime.UtcNow.AddSeconds(30);
            }
            var list=h.ServerRunning?telemetry.GetPlayers():new List<PlayerState>();
            var known=telemetry.GetAllKnownPlayers().Where(p=>!string.IsNullOrWhiteSpace(p.Name)&&!p.Name.StartsWith("Player ")).Select(p=>new {name=p.Name,evidence=p.CharacterSaveBytes>0?"Character save transfer observed ("+FormatUtil.Bytes(p.CharacterSaveBytes)+")":"Seen in server log"}).ToList();
            foreach(var session in Storage.ReadSessions(settings.ServerRoot)) if(!known.Any(p=>p.name==session.Player))known.Add(new {name=session.Player,evidence="Recorded session history"});
            var world=telemetry.GetWorld();
            if(lastRealm!=null)foreach(string name in lastRealm.KnownPlayers)if(!known.Any(p=>p.name==name))known.Add(new {name=name,evidence="Previous server session / saved history"});
            phone.Publish(new {worldFiles=phoneWorldFiles,worldSize=phoneWorldSize,worldSaved=phoneWorldSaved,bases=world.FlameAltarBases<0?"Not observed":world.FlameAltarBases.ToString(),knownPlayers=known.OrderBy(p=>p.name).ToArray(),name=settings.ServerName,online=h.ServerRunning,players=h.ServerRunning?telemetry.PlayerCountText:"—",playerEvidence=telemetry.PlayerCountEvidence,roster=list.Select(p=>new {name=p.Name,session=p.JoinTime==DateTime.MinValue?"Session unavailable":FormatUtil.Duration(DateTime.Now-p.JoinTime)+" observed"}).ToArray(),uptime=h.ServerRunning?FormatUtil.Duration(h.Uptime):"Offline",cpu=h.CpuPercent.ToString("0.0"),memory=FormatUtil.Bytes(h.RamBytes),updated=DateTime.Now.ToString("h:mm:ss tt"),controls=controlsEnabled,busy=operationBusy||updateBusy||lifecycle.StartupPending,operation=operationStatus==null?"":operationStatus.Text,lastBackup=lastPhoneBackup,backupStatus=automationStatus==null?"":automationStatus.Text,autoBackup=settings.AutoBackup,recovery=settings.AutoRestartOnCrash,backupMinutes=settings.BackupIntervalMinutes,nextBackup=nextBackup.ToLocalTime().ToString("h:mm tt")});
        }

        ActionResult PhoneCommand(string operation)
        {
            if(IsDisposed || Disposing)return ActionResult.Fail("Launcher is closing.");
            return (ActionResult)Invoke(new Func<ActionResult>(delegate {
                if(!controlsEnabled || operationBusy || updateBusy || lifecycle.StartupPending)return ActionResult.Fail("Controls unavailable or another operation is in progress.");
                if(operation=="Backup") { BeginPhoneBackup(); }
                else if(operation=="AutoBackupOn" || operation=="AutoBackupOff") {backupOption.Checked=operation=="AutoBackupOn";operationStatus.Text="Automatic backups "+(settings.AutoBackup?"enabled.":"disabled.");}
                else if(operation=="RecoveryOn" || operation=="RecoveryOff") {recoveryOption.Checked=operation=="RecoveryOn";operationStatus.Text="Crash recovery "+(settings.AutoRestartOnCrash?"enabled.":"disabled.");}
                else if(operation=="Start" || operation=="Stop" || operation=="Restart") BeginPhoneOperation(operation);
                else return ActionResult.Fail("Unknown operation.");
                PublishPhone();
                return ActionResult.Ok(operation + " requested. Watch status for the result.");
            }));
        }
        async void BeginPhoneOperation(string operation) { try {await RunOperation(operation,true);} catch(Exception ex){operationBusy=false;operationStatus.Text=ex.Message;PublishPhone();} }
        async void BeginPhoneBackup() { try {await BackupNow();} catch(Exception ex){operationBusy=false;automationStatus.Text="Backup failed: "+ex.Message;} phoneBackupScan=DateTime.MinValue;PublishPhone(); }
        Control BuildAutomation()
        {
            var page = Page("Automation", "Optional protection while this launcher stays open. Existing backups are never removed.");
            var card = Card(page, "SAVE COPIES AND RECOVERY", 0, 78, 780, 360);
            var backup = new CheckBox { Text = "Back up saves automatically", Checked = settings.AutoBackup, AutoCheck = controlsEnabled, TabStop = controlsEnabled, ForeColor = Theme.Muted, AutoSize = true, Location = new Point(20, 48) };
            backupOption = backup; card.Controls.Add(backup);
            var interval = new NumericUpDown { Minimum = 5, Maximum = 1440, Value = Math.Max(5, Math.Min(1440, settings.BackupIntervalMinutes)), Location = new Point(340, 46), Width = 75, Enabled = controlsEnabled };
            card.Controls.Add(interval);
            var minutes = TextLabel("minutes", 10, Theme.Muted, false); minutes.Location = new Point(425, 49); card.Controls.Add(minutes);
            backup.CheckedChanged += delegate { settings.AutoBackup = backup.Checked; nextBackup = DateTime.UtcNow.AddMinutes((double)interval.Value); Storage.SaveSettings(appBase, settings); };
            interval.ValueChanged += delegate { settings.BackupIntervalMinutes = (int)interval.Value; nextBackup = DateTime.UtcNow.AddMinutes((double)interval.Value); Storage.SaveSettings(appBase, settings); };
            recoveryOption = new CheckBox { Text = "Restart after an unexpected stop (60-second delay)", Checked = settings.AutoRestartOnCrash, AutoCheck = controlsEnabled, TabStop = controlsEnabled, ForeColor = Theme.Muted, AutoSize = true, Location = new Point(20, 96) };
            card.Controls.Add(recoveryOption);
            recoveryOption.CheckedChanged += delegate { settings.AutoRestartOnCrash = recoveryOption.Checked; recovery.Disarm(); Storage.SaveSettings(appBase, settings); };
            Body(card, "Recovery makes one attempt after an observed running session.\r\nStop turns recovery off. Start still requires all compatibility checks.\r\n\r\nBackups copy the configured server saves and configuration only.\r\nLive copies are byte-checked, but may span multiple game save cycles.\r\nClient-side character saves outside that folder are not included.", 140, 10, Theme.Muted);
            var now = ActionButton(page, "Back up now", 0, 460, 150, async delegate { await BackupNow(); }); now.Enabled = controlsEnabled;
            ActionButton(page, "Backup folder", 166, 460, 150, delegate { Directory.CreateDirectory(BackupRoot); Process.Start("explorer.exe", "\"" + BackupRoot + "\""); });
            var chooseBackup = ActionButton(page, "Change backup path", 332, 460, 185, delegate { ChooseBackupRoot(); });
            chooseBackup.Enabled = controlsEnabled;
            var backupLocation = TextLabel("Backup destination: " + BackupRoot, 9, Theme.Muted, false);
            backupLocation.Location = new Point(0, 512);
            backupLocation.AutoSize = false;
            backupLocation.Size = new Size(780, 45);
            page.Controls.Add(backupLocation);
            backupPathLabel = backupLocation;
            automationStatus = TextLabel("Options are off by default. No automatic actions run in monitoring mode.", 10, Theme.Muted, false);
            automationStatus.Location = new Point(0, 573); automationStatus.MaximumSize = new Size(780, 160); page.Controls.Add(automationStatus);
            return page;
        }
        void ChooseBackupRoot()
        {
            if (!controlsEnabled || operationBusy || updateBusy || lifecycle.StartupPending) return;
            string chosen;
            using (var dialog = new FolderBrowserDialog {
                Description = "Choose an existing folder OUTSIDE the server and launcher. Existing backups stay where they are."
            })
            {
                if (Directory.Exists(BackupRoot)) dialog.SelectedPath = BackupRoot;
                if (dialog.ShowDialog() != DialogResult.OK) return;
                chosen = dialog.SelectedPath;
            }
            string normalized;
            string error = BackupPathPolicy.Validate(settings.ServerRoot, appBase, chosen, out normalized);
            if (error != null) { MessageBox.Show(error, "Backup folder not accepted", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (MessageBox.Show("Use this folder for FUTURE save copies?\r\n\r\n" + normalized +
                "\r\n\r\nExisting backups will NOT be moved or removed. Confirm it has enough free space.",
                "Change backup location", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            settings.BackupRoot = normalized;
            Storage.SaveSettings(appBase, settings);
            if (backupPathLabel != null) backupPathLabel.Text = "Backup destination: " + BackupRoot;
            phoneBackupScan = DateTime.MinValue;
            if (automationStatus != null) automationStatus.Text = "Future backups will use the selected location. Previous backups remain at their original location.";
        }

        async Task ActivateUpdate()
        {
            if (!controlsEnabled || operationBusy || updateBusy || lifecycle.StartupPending) return;
            string candidate;
            using (var dialog = new FolderBrowserDialog { Description = "Choose a fully prepared server under Staged-Updates" })
            { if (dialog.ShowDialog() != DialogResult.OK) return; candidate = dialog.SelectedPath; }
            if (MessageBox.Show("Switch to this verified installation using a fresh copy of your saves? The existing installation will remain intact. Progress afterward belongs to the selected installation.", "ShroudForge Launcher — Use update", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            operationBusy = true;
            var result = await Task.Run(delegate { return UpdateActivation.Prepare(settings.ServerRoot, candidate, appBase, delegate { return Process.GetProcessesByName("enshrouded_server").Length != 0; }); });
            operationBusy = false;
            if (!result.Success) { MessageBox.Show(result.Message, "ShroudForge Launcher — Update not activated"); return; }
            settings.AutoRestartOnCrash = false; recovery.Disarm();
            settings.ServerRoot = candidate; settings.PreferredServerRoot = candidate;
            Storage.SaveSettings(appBase, settings);
            MessageBox.Show(result.Message + "\r\nThe launcher will reopen. Start the selected server when ready.", "ShroudForge Launcher — Update ready");
            Application.Restart();
        }
        async Task DownloadUpdate()
        {
            if (!controlsEnabled || updateBusy) return;
            string client = settings.SteamCmdPath;
            if (!File.Exists(client))
            {
                using (var dialog = new OpenFileDialog { Title = "Choose official SteamCMD (download is staged separately)", Filter = "SteamCMD|steamcmd.exe", CheckFileExists = true })
                { if (dialog.ShowDialog() != DialogResult.OK) return; client = dialog.FileName; }
            }
            if (MessageBox.Show("Download a separate server installation for compatibility testing? This may use several GB. It will not replace the live server.", "ShroudForge Launcher — Staged update", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            settings.SteamCmdPath = client; Storage.SaveSettings(appBase, settings);
            updateBusy = true; automationStatus.Text = "Downloading isolated update. Server controls remain available.";
            var result = await Task.Run(delegate { return StagedUpdate.Download(client, appBase, settings.ServerRoot); });
            updateBusy = false;
            if (!IsDisposed) { automationStatus.Text = result.Message; MessageBox.Show(result.Message, "ShroudForge Launcher — Staged update"); }
        }
        async Task BackupNow()
        {
            if (!controlsEnabled || operationBusy) return;
            operationBusy = true;
            automationStatus.Text = "Copying and verifying saves…";
            bool running = telemetry.GetHost().ServerRunning;
            var result = await Task.Run(delegate { return SaveBackup.Create(settings.ServerRoot, BackupRoot, running); });
            operationBusy = false;
            if (!IsDisposed) automationStatus.Text = result.Message;
            nextBackup = DateTime.UtcNow.AddMinutes(Math.Max(5, settings.BackupIntervalMinutes));
            phoneBackupScan=DateTime.MinValue; PublishPhone();
        }
        async Task TickAutomation()
        {
            if (!controlsEnabled) return;
            bool busy = operationBusy || lifecycle.StartupPending;
            if (recovery.Poll(settings.AutoRestartOnCrash, telemetry.GetHost().ServerRunning, busy, DateTime.UtcNow))
            {
                operationBusy = true;
                var result = await Task.Run(delegate { return lifecycle.Execute("Start"); });
                operationBusy = false;
                if (!IsDisposed) automationStatus.Text = "Automatic recovery: " + result.Message;
            }
            if (!operationBusy && !lifecycle.StartupPending && settings.AutoBackup && DateTime.UtcNow >= nextBackup) await BackupNow();
        }
        async Task InspectAsync()
        {
            if (auditBusy) return; auditBusy = true; recheck.Enabled = false;
            auditSummary.Text = "Checking compatibility";
            var result = await Task.Run(delegate { return InstallationAudit.Inspect(settings.ServerRoot); });
            if (IsDisposed) return; audit = result; auditBusy = false; recheck.Enabled = true; UpdateState();
        }
        internal void SetAuditForTest(InstallationAudit value) { audit = value; UpdateState(); }
        async Task RunOperation(string operation, bool fromPhone = false)
        {
            if (!controlsEnabled || operationBusy) return;
            string detail = operation == "Start" ? "Start using the existing safe-start wrapper?" : operation == "Stop" ? "Request a graceful server shutdown? Connected players will be disconnected. The server will not be force-killed." : "Restart the server? Compatibility is checked before stopping. Connected players will be disconnected.";
            if (!fromPhone && MessageBox.Show(detail, "ShroudForge Launcher — " + operation, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            if (operation == "Stop") { settings.AutoRestartOnCrash = false; recovery.Disarm(); if (recoveryOption != null) recoveryOption.Checked = false; Storage.SaveSettings(appBase, settings); }
            operationBusy = true; UpdateState(); operationStatus.Text = operation + " requested…";
            ActionResult result = await Task.Run(delegate { return lifecycle.Execute(operation); });
            if (IsDisposed) return;
            operationBusy = false; operationStatus.Text = result.Message;
            if (!fromPhone) MessageBox.Show(result.Message, "ShroudForge Launcher — " + operation, MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            UpdateState(); PublishPhone();
        }
        void UpdateState(){ UpdateStateCore(); if(state!=null)UpdateRichState(); }
        void UpdateStateCore()
        {
            if (state == null) return;
            var h = telemetry.GetHost(); var w = telemetry.GetWorld(); var list = telemetry.GetPlayers();
            serverName.Text = settings.ServerName;
            state.Text = h.ServerRunning ? "Realm online" : "Realm offline"; state.ForeColor = h.ServerRunning ? Theme.Green : Theme.Text;
            statusDetail.Text = h.ServerRunning ? "Process observed  /  PID " + h.Pid + "  /  " + FormatUtil.Duration(h.Uptime) : "No matching server process observed. Check Installation before starting.";
            players.Set(h.ServerRunning ? telemetry.PlayerCountText : "—", h.ServerRunning ? telemetry.PlayerCountEvidence : "Server offline");
            cpu.Set(h.ServerRunning ? h.CpuPercent.ToString("0.0") + "%" : "—", "Selected server process");
            memory.Set(h.ServerRunning ? FormatUtil.Bytes(h.RamBytes) : "—", "Server working set");
            uptime.Set(h.ServerRunning ? FormatUtil.Duration(h.Uptime) : "—", "Current session");
            cpu.Chart.SetValues(telemetry.CpuHistory); memory.Chart.SetValues(telemetry.RamHistory);
            saveSummary.Text = "Last observed save  " + (w.LastSaveTime == DateTime.MinValue ? "Not observed" : w.LastSaveTime.ToString("MMM d, h:mm tt")) + "\r\nWorld size  " + FormatUtil.Bytes(w.WorldSaveBytes);
            if (worldDetail != null) worldDetail.Text = saveSummary.Text + "\r\n\r\nBases  " + (w.FlameAltarBases < 0 ? "Not observed" : w.FlameAltarBases.ToString()) + "\r\nEntities  " + (w.WorldEntities < 0 ? "Not observed" : w.WorldEntities.ToString("N0")) + "\r\n\r\nMetadata may come from earlier logs; it does not prove current world health.";
            if (audit != null)
            {
                auditSummary.Text = audit.Summary; auditSummary.ForeColor = audit.CanStart ? Theme.Green : Theme.Amber;
                auditDetail.Text = audit.CanStart ? "Disk checks passed. Gameplay is not verified by this check." : "File checks need review. Open Installation for the exact results.";
                stackState.Text = (audit.StackMatches ? "Files match attestation" : "Files need verification") + "\r\n\r\n65,535 stacking / preserved";
                giftingState.Text = (audit.FlightCompatible ? "Pinned build matches" : "Build not supported") + "\r\n\r\nApplied by the safe-start routine";
                flightState.Text = (audit.FlightCompatible ? "Pinned build matches" : "Build not supported") + "\r\n\r\nPitch hold / ±80° bounds";
                auditBox.Text = audit.Details;
            }
            UpdateModernDetails();
            footer.Text = (controlsEnabled ? "GUARDED CONTROLS" : "READ-ONLY PREVIEW") + "   /   " + DateTime.Now.ToString("h:mm:ss tt") + "   /   Original server and saves remain in place";
            if (controlsEnabled && startServer != null)
            {
                bool pending = lifecycle.StartupPending;
                startServer.Enabled = !operationBusy && !pending && !h.ServerRunning && audit != null && audit.CanStart;
                restartServer.Enabled = !operationBusy && !pending && h.ServerRunning && audit != null && audit.CanStart;
                stopServer.Enabled = !operationBusy && !pending && h.ServerRunning;
                if (!operationBusy && audit != null && !audit.CanStart) operationStatus.Text = "Start / restart blocked: installation checks need attention. Your existing files are unchanged.";
                else if (!operationBusy && !string.IsNullOrEmpty(lifecycle.StartupStatus)) operationStatus.Text = lifecycle.StartupStatus;
                else if(!operationBusy && (operationStatus.Text=="Checking the existing installation…"||operationStatus.Text.StartsWith("Ready •")||operationStatus.Text.StartsWith("Monitoring the running realm"))) operationStatus.Text=h.ServerRunning?"Monitoring the running realm • Server controls are ready":"Ready • Start uses the verified existing installation";
            }
            if (current == "Players" && playerGrid != null)
            {
                playerGrid.Rows.Clear();
                foreach (var p in telemetry.GetAllKnownPlayers().Where(p => !string.IsNullOrWhiteSpace(p.Name) && !p.Name.StartsWith("Player ")).OrderByDescending(p=>p.Connected)) playerGrid.Rows.Add(p.Name, p.PingMs < 0 ? "Unknown" : p.PingMs + " ms", p.Jitter.ToString("0") + " ms", p.JoinTime == DateTime.MinValue ? "Unknown" : FormatUtil.Duration(DateTime.Now - p.JoinTime), p.Connected ? p.SessionState : "Previously seen in log");
            }
        }
        void ReadLogs()
        {
            if (logBox == null) return;
            try
            {
                string path = telemetry.CurrentLogPath;
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) { logBox.Text = "No server log found yet."; return; }
                var lines = new Queue<string>();
                using (var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    long start = Math.Max(0, input.Length - 512 * 1024); input.Seek(start, SeekOrigin.Begin);
                    using (var reader = new StreamReader(input))
                    {
                        if (start > 0) reader.ReadLine(); string line;
                        while ((line = reader.ReadLine()) != null) { if (search.Text.Length > 0 && line.IndexOf(search.Text, StringComparison.OrdinalIgnoreCase) < 0) continue; lines.Enqueue(line); if (lines.Count > 1000) lines.Dequeue(); }
                    }
                }
                string text = string.Join(Environment.NewLine, lines.ToArray());
                if (logBox.Text != text) { logBox.Text = text; logBox.SelectionStart = logBox.TextLength; logBox.ScrollToCaret(); }
            }
            catch (Exception ex) { logBox.Text = "Log read failed: " + ex.Message; }
        }
    }
    public sealed class Sigil : Control
    {
        public Sigil() { DoubleBuffered = true; }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(Theme.Cyan, 1.4f))
            {
                e.Graphics.DrawPolygon(pen, new[] { new Point(Width / 2, 2), new Point(Width - 3, Height / 3), new Point(Width / 2, Height - 3), new Point(3, Height / 3) });
                e.Graphics.DrawLine(pen, Width / 2, 9, Width / 2, Height - 10);
                e.Graphics.DrawLine(pen, 9, Height / 3, Width - 9, Height / 3);
            }
        }
    }
}


