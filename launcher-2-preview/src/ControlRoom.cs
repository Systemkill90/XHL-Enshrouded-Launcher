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
    public sealed class ControlRoom : Form
    {
        readonly string appBase;
        readonly LauncherSettings settings;
        readonly TelemetryService telemetry;
        readonly bool controlsEnabled;
        readonly ServerLifecycle lifecycle;
        Button startServer, stopServer, restartServer;
        bool operationBusy;
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
        TextBox logBox, auditBox;
        TextBox search;
        Button recheck;
        string current = "Overview";
        bool auditBusy, paused;
        int frames;

        public ControlRoom(string appBase, LauncherSettings settings, bool startMonitoring, bool enableOperations = false)
        {
            this.appBase = appBase;
            this.settings = settings;
            controlsEnabled = enableOperations;
            lifecycle = new ServerLifecycle(new ExistingServerRuntime(settings.ServerRoot, settings), delegate { return ServerLifecycle.CheckInstallation(settings.ServerRoot); });
            // The preview cannot auto-start a server or perform external identity lookups.
            settings.AutoRestartOnCrash = false;
            settings.PublicIpLookup = false;
            telemetry = new TelemetryService(appBase, settings);
            Text = "XHL | Enshrouded | 2.0 Preview" + (controlsEnabled ? " | Controls" : "");
            BackColor = Theme.Bg; ForeColor = Theme.Text; Font = Theme.Font(10, FontStyle.Regular);
            AutoScaleMode = AutoScaleMode.Dpi;
            MinimumSize = new Size(1100, 740); Size = new Size(1400, 930);
            StartPosition = FormStartPosition.CenterScreen;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            BuildShell();
            AddPage("Overview", BuildOverview());
            AddPage("Players", BuildPlayers());
            AddPage("World", BuildWorld());
            AddPage("Mods", BuildMods());
            AddPage("Logs", BuildLogs());
            AddPage("Installation", BuildInstallation());
            Navigate("Overview");
            refresh.Interval = 1000;
            refresh.Tick += delegate { frames++; UpdateState(); if (current == "Logs" && !paused && frames % 3 == 0) ReadLogs(); };
            if (startMonitoring) { telemetry.Start(); refresh.Start(); Shown += async delegate { await InspectAsync(); }; }
            FormClosing += delegate(object sender, FormClosingEventArgs e) { if (operationBusy) { e.Cancel = true; MessageBox.Show("Wait for the current server operation to finish.", "XHL"); } };
            FormClosed += delegate { refresh.Stop(); refresh.Dispose(); telemetry.Dispose(); };
            UpdateState();
        }

        Label TextLabel(string text, float size, Color color, bool strong)
        { return Theme.Label(text, size, color, strong ? FontStyle.Bold : FontStyle.Regular); }

        void BuildShell()
        {
            var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 204)); shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); Controls.Add(shell);
            var side = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Sidebar, Margin = new Padding(0) }; shell.Controls.Add(side, 0, 0);
            var emblem = new Sigil { Location = new Point(24, 25), Size = new Size(38, 46) }; side.Controls.Add(emblem);
            var brand = TextLabel("XHL", 24, Theme.Text, false); brand.Location = new Point(73, 29); side.Controls.Add(brand);
            var sub = TextLabel("ENSHROUDED", 8, Theme.Cyan, true); sub.Location = new Point(26, 89); side.Controls.Add(sub);
            var category = TextLabel("YOUR REALM", 8, Theme.Muted2, true); category.Location = new Point(26, 143); side.Controls.Add(category);
            int y = 178;
            foreach (string page in new[] { "Overview", "Players", "World", "Mods", "Logs", "Installation" })
            {
                var button = Theme.Button(page, Theme.Border);
                button.TextAlign = ContentAlignment.MiddleLeft; button.Padding = new Padding(16, 0, 0, 0);
                button.Location = new Point(12, y); button.Size = new Size(180, 44); button.FlatAppearance.BorderSize = 0;
                button.Click += delegate { Navigate(page); }; side.Controls.Add(button); navigation.Add(page, button); y += 50;
            }
            var sideFoot = new Label { Dock = DockStyle.Bottom, Height = 110, Padding = new Padding(26, 20, 10, 0), ForeColor = Theme.Muted,
                Text = "2.0 / PREVIEW\r\n\r\n" + (controlsEnabled ? "Guarded server controls" : "Read-only server view"), Font = Theme.Font(8.5f, FontStyle.Regular) }; side.Controls.Add(sideFoot);
            var main = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) }; shell.Controls.Add(main, 1, 0);
            host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(28, 20, 28, 20) }; main.Controls.Add(host);
            var top = new Panel { Dock = DockStyle.Top, Height = 102, BackColor = Theme.Sidebar }; main.Controls.Add(top);
            serverName = TextLabel(settings.ServerName, 25, Theme.Text, false); serverName.Font = new Font("Georgia", 25);
            serverName.Location = new Point(28, 22); top.Controls.Add(serverName);
            var topSub = TextLabel("SERVER CONTROL ROOM", 8, Theme.Muted, true); topSub.Location = new Point(30, 66); top.Controls.Add(topSub);
            var notice = TextLabel(controlsEnabled ? "GUARDED CONTROLS\r\nCompatibility required before start" : "MONITORING ONLY\r\nYour existing launcher remains available", 9, Theme.Cyan, false);
            notice.AutoSize = false; notice.Size = new Size(320, 46); notice.TextAlign = ContentAlignment.MiddleRight;
            notice.Location = new Point(top.Width - 350, 27); notice.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            serverName.AutoSize = false; serverName.Size = new Size(Math.Max(200, top.Width - 410), 42); serverName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            top.Controls.Add(notice);
            footer = new Label { Dock = DockStyle.Bottom, Height = 32, Padding = new Padding(28, 7, 0, 0), ForeColor = Theme.Muted2,
                Font = Theme.Font(8, FontStyle.Regular), Text = "Reading installation. World and player saves are read-only." }; main.Controls.Add(footer);
        }

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
            UpdateState(); if (page == "Logs") ReadLogs();
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

        Control BuildOverview()
        {
            var page = Page("The realm, at a glance", "Live observation, clear checks, and the details that matter.");
            var canvas = new Panel { Location = new Point(0, controlsEnabled ? 220 : 78), Size = new Size(1040, 650) }; page.Controls.Add(canvas);
            var statusCard = Card(canvas, "SERVER", 0, 0, 640, 152);
            state = Body(statusCard, "Checking server", 43, 23, Theme.Text); state.Height = 40;
            statusDetail = Body(statusCard, "Waiting for the first process observation", 96, 9, Theme.Muted); statusDetail.Height = 34;
            var auditCard = Card(canvas, "INSTALLATION", 656, 0, 384, 152);
            auditSummary = Body(auditCard, "Checking compatibility", 44, 13, Theme.Amber); auditSummary.Height = 32;
            auditDetail = Body(auditCard, "A matching build is required for native mods.", 85, 9, Theme.Muted); auditDetail.Height = 44;
            var stats = new TableLayoutPanel { Location = new Point(0, 168), Size = new Size(1040, 100), ColumnCount = 4, RowCount = 1 };
            canvas.Controls.Add(stats);
            players = new MetricCard("Players", Theme.Cyan); cpu = new MetricCard("CPU", Theme.Cyan); memory = new MetricCard("Memory", Theme.Cyan); uptime = new MetricCard("Uptime", Theme.Cyan);
            MetricCard[] metrics = { players, cpu, memory, uptime };
            for (int i = 0; i < 4; i++) { stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25)); metrics[i].Dock = DockStyle.Fill; metrics[i].Margin = new Padding(i == 0 ? 0 : 6, 0, i == 3 ? 0 : 6, 0); stats.Controls.Add(metrics[i], i, 0); }
            var modTitle = TextLabel("THE THREE MODS", 8, Theme.Muted, true); modTitle.Location = new Point(1, 293); canvas.Controls.Add(modTitle);
            var modGrid = new TableLayoutPanel { Location = new Point(0, 322), Size = new Size(1040, 146), ColumnCount = 3, RowCount = 1 }; canvas.Controls.Add(modGrid);
            string[] names = { "01  /  STACKING", "02  /  GIFTING", "03  /  FLIGHT" };
            Label[] states = new Label[3];
            for (int i = 0; i < 3; i++)
            {
                modGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
                var card = Card(modGrid, names[i], 0, 0, 330, 146); card.Dock = DockStyle.Fill; card.Margin = new Padding(i == 0 ? 0 : 6, 0, i == 2 ? 0 : 6, 0);
                modGrid.SetCellPosition(card, new TableLayoutPanelCellPosition(i, 0));
                states[i] = Body(card, "Not verified", 46, 11, Theme.Text);
            }
            stackState = states[0]; giftingState = states[1]; flightState = states[2];
            var save = Card(canvas, "WORLD RECORD", 0, 486, 640, 118); saveSummary = Body(save, "Reading world metadata", 44, 10, Theme.Text);
            var shortcuts = Card(canvas, "QUICK ACCESS", 656, 486, 384, 118);
            var logsButton = ActionButton(shortcuts, "View logs", 20, 48, 132, delegate { Navigate("Logs"); });
            var detailsButton = ActionButton(shortcuts, "Check details", 164, 48, 145, delegate { Navigate("Installation"); });
            shortcuts.Resize += delegate { logsButton.Width = detailsButton.Width = Math.Max(90, (shortcuts.Width - 52) / 2); detailsButton.Left = logsButton.Right + 12; };
            Panel operations = null;
            if (controlsEnabled)
            {
                operations = Card(page, "SERVER OPERATIONS", 0, 78, 1040, 126);
                startServer = ActionButton(operations, "Start", 20, 46, 110, async delegate { await RunOperation("Start"); });
                restartServer = ActionButton(operations, "Restart", 142, 46, 110, async delegate { await RunOperation("Restart"); });
                stopServer = ActionButton(operations, "Stop", 264, 46, 110, async delegate { await RunOperation("Stop"); });
                operationStatus = Body(operations, "Start and restart require current compatibility checks.", 92, 9, Theme.Muted);
            }
            page.Resize += delegate
            {
                int width = Math.Max(760, page.ClientSize.Width - 20); canvas.Width = width;
                int left = (int)(width * .60); statusCard.Width = left; auditCard.Left = left + 16; auditCard.Width = width - left - 16;
                stats.Width = modGrid.Width = width; save.Width = left; shortcuts.Left = left + 16; shortcuts.Width = width - left - 16;
                if (operations != null) operations.Width = width;
            };
            return page;
        }
        Control BuildPlayers()
        {
            var page = Page("Players", "Connection telemetry from the selected server. No character or inventory changes.");
            playerGrid = new DataGridView { Location = new Point(0, 78), Size = new Size(1000, 500), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, ReadOnly = true };
            Theme.StyleGrid(playerGrid); foreach (string column in new[] { "Player", "Ping", "Jitter", "Session", "Status" }) playerGrid.Columns.Add(column, column);
            page.Controls.Add(playerGrid); page.Resize += delegate { playerGrid.Size = new Size(Math.Max(100, page.ClientSize.Width - 20), Math.Max(120, page.ClientSize.Height - 90)); }; return page;
        }
        Control BuildWorld()
        {
            var page = Page("World", "Save metadata is read-only. World and player data stay in their original locations.");
            var card = Card(page, "LATEST OBSERVATION", 0, 78, 740, 270); worldDetail = Body(card, "Waiting for world data", 48, 12, Theme.Text);
            var note = TextLabel("Snapshots and backups remain available through your existing launcher.\r\nThis preview has no restore, delete, migration, or retention actions.", 10, Theme.Muted, false);
            note.Location = new Point(0, 378); page.Controls.Add(note); return page;
        }
        Control BuildMods()
        {
            var page = Page("Mods", "Preserve the working setup. Compatibility checks never apply patches.");
            var card = Card(page, "VERIFICATION LEVELS", 0, 78, 820, 330);
            Body(card, "Stacking\r\nChecks the executable, KFC and resource hashes against the local attestation.\r\n\r\nGifting and Flight\r\nChecks the pinned executable build. Runtime memory and gameplay remain separate tests.\r\n\r\nFlight-v2 balance\r\nPending the agreed test plan and validated hooks for the installed build.", 48, 11, Theme.Text);
            ActionButton(page, "Inspect installation", 0, 435, 185, delegate { Navigate("Installation"); }); return page;
        }
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
        async Task InspectAsync()
        {
            if (auditBusy) return; auditBusy = true; recheck.Enabled = false;
            auditSummary.Text = "Checking compatibility";
            var result = await Task.Run(delegate { return InstallationAudit.Inspect(settings.ServerRoot); });
            if (IsDisposed) return; audit = result; auditBusy = false; recheck.Enabled = true; UpdateState();
        }
        internal void SetAuditForTest(InstallationAudit value) { audit = value; UpdateState(); }
        async Task RunOperation(string operation)
        {
            if (!controlsEnabled || operationBusy) return;
            string detail = operation == "Start" ? "Start using the existing safe-start wrapper?" : operation == "Stop" ? "Request a graceful server shutdown? Connected players will be disconnected. The server will not be force-killed." : "Restart the server? Compatibility is checked before stopping. Connected players will be disconnected.";
            if (MessageBox.Show(detail, "XHL — " + operation, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            operationBusy = true; UpdateState(); operationStatus.Text = operation + " requested…";
            ActionResult result = await Task.Run(delegate { return lifecycle.Execute(operation); });
            if (IsDisposed) return;
            operationBusy = false; operationStatus.Text = result.Success ? result.Message : "Operation did not complete. See details.";
            MessageBox.Show(result.Message, "XHL — " + operation, MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            UpdateState();
        }
        void UpdateState()
        {
            if (state == null) return;
            var h = telemetry.GetHost(); var w = telemetry.GetWorld(); var list = telemetry.GetPlayers();
            serverName.Text = settings.ServerName;
            state.Text = h.ServerRunning ? "The realm is online" : "The realm is resting"; state.ForeColor = h.ServerRunning ? Theme.Green : Theme.Text;
            statusDetail.Text = h.ServerRunning ? "Process observed  /  PID " + h.Pid + "  /  " + FormatUtil.Duration(h.Uptime) : "No matching server process observed. Use your existing launcher to go live.";
            players.Set(h.ServerRunning ? list.Count.ToString() : "—", h.ServerRunning ? "Connected players" : "Server offline");
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
                giftingState.Text = (audit.FlightCompatible ? "Pinned build matches" : "Build not supported") + "\r\n\r\nRuntime not checked";
                flightState.Text = (audit.FlightCompatible ? "Pinned build matches" : "Build not supported") + "\r\n\r\nBalance changes pending";
                auditBox.Text = audit.Details;
            }
            footer.Text = (controlsEnabled ? "GUARDED CONTROLS" : "READ-ONLY PREVIEW") + "   /   " + DateTime.Now.ToString("h:mm:ss tt") + "   /   Original server and saves remain in place";
            if (controlsEnabled && startServer != null)
            {
                bool pending = lifecycle.StartupPending;
                startServer.Enabled = !operationBusy && !pending && !h.ServerRunning && audit != null && audit.CanStart;
                restartServer.Enabled = !operationBusy && !pending && h.ServerRunning && audit != null && audit.CanStart;
                stopServer.Enabled = !operationBusy && !pending && h.ServerRunning;
                if (!operationBusy && audit != null && !audit.CanStart) operationStatus.Text = "Start / restart blocked: installation checks need attention. Your existing files are unchanged.";
                else if (!operationBusy && !string.IsNullOrEmpty(lifecycle.StartupStatus)) operationStatus.Text = lifecycle.StartupStatus;
            }
            if (current == "Players" && playerGrid != null)
            {
                playerGrid.Rows.Clear();
                if (h.ServerRunning) foreach (var p in list) playerGrid.Rows.Add(p.Name, p.PingMs < 0 ? "Unknown" : p.PingMs + " ms", p.Jitter.ToString("0") + " ms", p.JoinTime == DateTime.MinValue ? "Unknown" : FormatUtil.Duration(DateTime.Now - p.JoinTime), p.SessionState);
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
