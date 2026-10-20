using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace XHLLauncher
{
    // Presentation only. These profiles never inspect or modify client character saves.
    internal sealed class PlayerSheetSnapshot
    {
        public string Name, Status, Identity, Sessions, Connection, Incidents, Activity, Limitations;
    }

    internal static class PlayerSheet
    {
        static string When(DateTime time) { return time == DateTime.MinValue ? "Not recorded" : time.ToString("MMM d, yyyy  h:mm tt"); }
        static string Known(string value) { return string.IsNullOrWhiteSpace(value) ? "Not recorded" : value; }
        static bool RelevantEvent(string name, string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            foreach (string suffix in new[] { " connected", " disconnected", " timed out", " game connection congested", " server send failure", " replication falling behind" })
                if (string.Equals(value, name + suffix, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
        static string Lines(params string[] lines) { return string.Join(Environment.NewLine, lines); }

        internal static string LastSeen(PlayerState p, IEnumerable<PlayerSession> history, PlayerHistoryRecord recorded=null)
        {
            DateTime last = p.LastSeen;
            foreach (var session in history.Where(s=>string.Equals(s.Player,p.Name,StringComparison.OrdinalIgnoreCase)))
                if(session.Left>last) last=session.Left;
            return last==DateTime.MinValue?(recorded==null||recorded.IdentityUtc==DateTime.MinValue?"Not recorded":recorded.IdentityUtc.ToLocalTime().ToString("MMM d, h:mm tt")+" (recorded metadata)"):last.ToString("MMM d, h:mm tt");
        }
        internal static string SessionSummary(PlayerState p,IEnumerable<PlayerSession> history,bool running,DateTime now)
        {
            if(running&&p.Connected&&p.JoinTime!=DateTime.MinValue)return FormatUtil.Duration(now-p.JoinTime);
            var last=history.Where(s=>string.Equals(s.Player,p.Name,StringComparison.OrdinalIgnoreCase)).OrderByDescending(s=>s.Joined).FirstOrDefault();
            return last==null?"Not recorded":FormatUtil.Duration(TimeSpan.FromSeconds(Math.Max(0,last.DurationSeconds)));
        }
        static PlayerHistoryRecord Recorded(PlayerState p,IEnumerable<PlayerHistoryRecord> records){return records.FirstOrDefault(r=>string.Equals(p.Name,r.Name,StringComparison.OrdinalIgnoreCase)&&(p.SteamId.Length==0||r.SteamId.Length==0||p.SteamId==r.SteamId));}
        internal static int RecordedPing(PlayerState p,IEnumerable<PlayerHistoryRecord> records){var r=Recorded(p,records);return r==null||r.Network==null?-1:r.Network.PingMs;}
        internal static int RecordedQuality(PlayerState p,IEnumerable<PlayerHistoryRecord> records){var r=Recorded(p,records);return r==null||r.Network==null?-1:r.Network.HealthScore;}
        internal static string RecordedNetworkUtc(PlayerState p,IEnumerable<PlayerHistoryRecord> records){var r=Recorded(p,records);return r==null||r.NetworkUtc==DateTime.MinValue?null:r.NetworkUtc.ToString("o");}

        public static PlayerSheetSnapshot Create(string name, PlayerState p, IEnumerable<PlayerSession> history, IEnumerable<ActivityItem> events, bool serverRunning, DateTime now, PlayerHistoryRecord recorded=null)
        {
            if(recorded!=null&&p!=null&&p.SteamId.Length>0&&recorded.SteamId.Length>0&&p.SteamId!=recorded.SteamId)recorded=null;
            if(recorded!=null)p=recorded.FillIdentity(p);
            var sheet = new PlayerSheetSnapshot();
            sheet.Name = name;
            bool online = serverRunning && p != null && p.Connected;
            var sessions = (history ?? Enumerable.Empty<PlayerSession>())
                .Where(s => s != null && string.Equals(s.Player, name, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(s => s.Joined).ToList();
            sheet.Status = online ? "ONLINE / " + Known(p.SessionState) : "OFFLINE / Previously observed";
            sheet.Identity = Lines(
                "PLAYER NAME      " + name,
                "STEAM ID         " + (p == null ? "Not recorded" : Known(p.SteamId)),
                "PEER             " + (p == null ? "Not recorded" : Known(p.Peer)),
                "PLAYER HANDLE    " + (p == null ? "Not recorded" : Known(p.Handle)),
                "MACHINE INDEX    " + (p == null || p.MachineIndex < 0 ? "Not recorded" : p.MachineIndex.ToString()),
                "SAVE TRANSFER    " + (p == null || p.CharacterSaveBytes <= 0 ? "Not observed" : FormatUtil.Bytes(p.CharacterSaveBytes) + " (transfer size only)"),
                "IDENTITY SOURCE   " + (recorded==null?"Available server observations":"Recorded server sessions / logs; last retained " + When(recorded.IdentityUtc.ToLocalTime())));
            double recordedSeconds = sessions.Sum(s => Math.Max(0, s.DurationSeconds));
            string current = online && p.JoinTime != DateTime.MinValue
                ? FormatUtil.Duration(now - p.JoinTime) : "Not available";
            DateTime lastSession = sessions.Count == 0 ? DateTime.MinValue : sessions.Max(s => s.Left);
            DateTime firstSession = sessions.Count == 0 ? DateTime.MinValue : sessions.Min(s => s.Joined);
            sheet.Sessions = Lines(
                "CURRENT SESSION   " + current,
                "CURRENT JOINED    " + (online && p.JoinTime != DateTime.MinValue ? When(p.JoinTime) : "Not available"),
                "LAST OBSERVED    " + (online && p.LastSeen != DateTime.MinValue ? When(p.LastSeen) : lastSession!=DateTime.MinValue?When(lastSession):recorded!=null?When(recorded.IdentityUtc.ToLocalTime())+" (metadata/log evidence; exact visit time unknown)":"Not recorded"),
                "RECORDED VISITS  " + (sessions.Count == 0 ? "None in session CSV" : sessions.Count.ToString()),
                "RECORDED TIME    " + (sessions.Count == 0 ? "No completed sessions recorded" : FormatUtil.Duration(TimeSpan.FromSeconds(recordedSeconds))),
                "FIRST / LAST     " + When(firstSession) + "  /  " + When(lastSession),
                "LAST SESSION     " + (sessions.Count==0?"Not recorded":When(sessions[0].Joined)+" to "+When(sessions[0].Left)+" / "+FormatUtil.Duration(TimeSpan.FromSeconds(Math.Max(0,sessions[0].DurationSeconds)))),
                "RECENT COMPLETED SESSIONS",
                sessions.Count == 0 ? "   No completed sessions in available records" :
                    string.Join(Environment.NewLine, sessions.Take(4).Select(s => "   " + When(s.Joined) + "  •  " + FormatUtil.Duration(TimeSpan.FromSeconds(Math.Max(0, s.DurationSeconds)))).ToArray()));
            var livePlayer=p;
            bool fresh=online&&p!=null&&p.PingMs>=0&&p.LastNetworkSample!=DateTime.MinValue&&(now-p.LastNetworkSample).TotalSeconds<=15;
            bool historicalNetwork=!fresh&&recorded!=null&&recorded.Network!=null;
            if(historicalNetwork)p=recorded.Network;
            sheet.Connection = p == null
                ? "No network telemetry has been recorded for this name."
                : Lines(
                    fresh ? "RECENT NETWORK SAMPLE / received " + When(p.LastNetworkSample) : historicalNetwork?"LAST RECORDED NETWORK SAMPLE (HISTORICAL) / "+(recorded.NetworkTimeEstimated?"log last written ":"sample received ")+When(recorded.NetworkUtc.ToLocalTime()):"NETWORK SAMPLE UNAVAILABLE OR STALE",
                    "PING / JITTER       " + (p.PingMs < 0 ? "Unavailable" : p.PingMs + " ms") + "  /  " + (p.PingMs < 0 ? "Unavailable" : (p.Jitter<0?"Need two samples":p.Jitter.ToString("0.0") + " ms (mean ping change)")),
                    "PING AVG 1 MIN      " + (p.PingMs < 0 || p.OneMinuteAverage<0 ? "Not recorded" : p.OneMinuteAverage.ToString("0.0") + " ms"),
                    "PEAK 1 / 5 MIN     " + (p.PingMs < 0 || p.OneMinutePeak<0 ? "Not recorded" : p.OneMinutePeak + " / " + p.FiveMinutePeak + " ms"),
                    "CONNECTION ESTIMATE " + (p.PingMs < 0 || p.HealthScore<0 ? "Not recorded" : p.HealthScore + "/100  " + Theme.HealthLabel(p.HealthScore)),
                    "CONNECTION NOTES   " + (p.PingMs < 0 ? "No recent samples" : Known(p.HealthReason)),
                    "PENDING PACKETS     " + p.PendingPackets + "  /  Peak " + p.PeakPending,
                    "LOST PACKETS        " + p.LostTotal + " (last server counter; scope unconfirmed)",
                    "CONGESTION RTT      " + (p.CongestionRttMs < 0 ? "Not observed" : p.CongestionRttMs.ToString("0") + " ms"),
                    "UP / DOWN / REMOTE  " + (p.UpRate<0?"Not recorded":p.UpRate.ToString("0") + " / " + p.DownRate.ToString("0") + " / " + p.RemoteRate.ToString("0") + " (raw log values; unit unconfirmed)"),
                    "BANDWIDTH LIMIT     " + (p.BandwidthLimit<0?"Not recorded":p.BandwidthLimit.ToString("0") + " (raw log value)"));
            sheet.Incidents = p == null ? "No connection diagnostics available for this name." :
                Lines("CONNECTION TIMEOUTS  " + p.TimeoutCount + " observed",
                      "RECONNECTS           " + p.ReconnectCount + " observed",
                      "LAST LOSS DELTA      " + p.LostDelta + " added since previous sample",
                      "LAST ISSUE           " + Known(p.LastIssue),
                      "ISSUE OBSERVED       " + When(p.LastIssueTime),
                      "COUNTERS ARE SESSION OBSERVATIONS, NOT LIFETIME TOTALS.");
            p=livePlayer;
            var recent = (events ?? Enumerable.Empty<ActivityItem>())
                .Where(e => e != null && RelevantEvent(name, e.Text))
                .OrderByDescending(e => e.Time).Take(6).ToArray();
            sheet.Activity = recent.Length == 0 ? "No matching join, leave, or diagnostic events found in the available event history." :
                string.Join(Environment.NewLine, recent.Select(e => When(e.Time) + "  •  " + e.Text).ToArray());
            sheet.Limitations =
                "This is a read-only profile built from server logs, queries and locally recorded history." + Environment.NewLine +
                "The dedicated server does not currently expose verified character levels, inventory, equipment, exact position or town names through these sources. No values are invented or taken from private client saves." + Environment.NewLine +
                "Session history is matched by display name; renamed players or reused names may affect totals.";
            return sheet;
        }
    }

    internal sealed class PlayerRosterEntry
    {
        public readonly string Name;
        public readonly bool Online;
        public PlayerRosterEntry(string name, bool online) { Name = name; Online = online; }
        public override string ToString() { return (Online ? "●  " : "○  ") + Name; }
    }

    public sealed partial class ControlRoom
    {
        TextBox profileSearch;
        ListBox profileRoster;
        Panel profileDetail;
        Label profileName, profileStatus, profileIdentity, profileSessions, profileConnection, profileIncidents, profileActivity, profileLimitations, profileCount;
        readonly List<Panel> profileCards = new List<Panel>();
        List<PlayerSession> cachedProfileSessions = new List<PlayerSession>();
        List<ActivityItem> cachedProfileEvents = new List<ActivityItem>();
        DateTime nextProfileHistory = DateTime.MinValue, nextProfileRender = DateTime.MinValue;
        string selectedProfileName = "", rosterSignature = "";
        bool rosterUpdating;

        Control BuildPlayerProfiles()
        {
            var page = Page("Players & profiles", "Select a known player to see every verified field available from server telemetry and saved history.");
            page.AutoScroll = false;
            profileSearch = new TextBox { Location = new Point(0, 78), Width = 234, Height = 30, AccessibleName = "Search known players" };
            Theme.StyleTextBox(profileSearch); page.Controls.Add(profileSearch);
            profileRoster = new ListBox { Location = new Point(0, 118), Width = 234, Height = 550, IntegralHeight = false,
                BackColor = Theme.Panel2, ForeColor = Theme.Text, BorderStyle = BorderStyle.FixedSingle,
                Font = Theme.Font(10, FontStyle.Regular), ItemHeight = 28 };
            page.Controls.Add(profileRoster);
            profileCount = TextLabel("Live and previously observed players", 8, Theme.Muted, false);
            profileCount.AutoSize = false; profileCount.SetBounds(0, 678, 234, 28); page.Controls.Add(profileCount);
            profileDetail = new Panel { Location = new Point(254, 78), Size = new Size(730, 630), AutoScroll = true, BackColor = Theme.Bg };
            page.Controls.Add(profileDetail);
            profileName = TextLabel("Select a player", 20, Theme.Text, false);
            profileName.AutoSize = false; profileName.SetBounds(4, 0, 710, 36); profileName.Font = new Font("Georgia", 20);
            profileDetail.Controls.Add(profileName);
            profileStatus = TextLabel("Current information and recorded history appear here.", 9, Theme.Muted, false);
            profileStatus.AutoSize = false; profileStatus.SetBounds(4, 42, 710, 28); profileDetail.Controls.Add(profileStatus);
            profileIdentity = SheetCard("IDENTITY & CHARACTER SAVE EVIDENCE", 76, 186);
            profileSessions = SheetCard("SESSIONS & VISITS", 275, 258);
            profileConnection = SheetCard("CONNECTION QUALITY & NETWORK", 546, 265);
            profileIncidents = SheetCard("ISSUES & DISCONNECTIONS", 824, 170);
            profileActivity = SheetCard("RECENT PLAYER EVENTS", 1007, 192);
            profileLimitations = SheetCard("AVAILABLE DATA & LIMITS", 1212, 166);
            profileDetail.AutoScrollMinSize = new Size(0, 1390);
            profileSearch.TextChanged += delegate { rosterSignature = ""; RefreshProfileRoster(); };
            profileRoster.SelectedIndexChanged += delegate {
                if (rosterUpdating) return;
                var selected = profileRoster.SelectedItem as PlayerRosterEntry;
                selectedProfileName = selected == null ? "" : selected.Name;
                nextProfileRender = DateTime.MinValue; RenderProfile();
            };
            page.Resize += delegate {
                int height = Math.Max(180, page.ClientSize.Height - 116);
                profileRoster.Height = height - 40; profileCount.Top = 118 + profileRoster.Height + 8;
                profileDetail.SetBounds(254, 78, Math.Max(350, page.ClientSize.Width - 254), Math.Max(160, page.ClientSize.Height - 85));
                int cardWidth = Math.Max(335, profileDetail.ClientSize.Width - 25);
                foreach (Panel c in profileCards) c.Width = cardWidth;
                profileName.Width = profileStatus.Width = cardWidth;
            };
            return page;
        }

        Label SheetCard(string title, int y, int height)
        {
            Panel card = Card(profileDetail, title, 0, y, 700, height);
            profileCards.Add(card);
            Label text = Body(card, "Select a player to view records.", 39, 9, Theme.Text);
            text.Height = height - 48;
            return text;
        }

        void RefreshProfileHistory()
        {
            if (DateTime.Now < nextProfileHistory) return;
            nextProfileHistory = DateTime.Now.AddSeconds(20);
            cachedProfileSessions = Storage.ReadSessions(settings.ServerRoot);
            cachedProfileEvents = Storage.ReadRecentPlayerEvents(settings.ServerRoot, 250);
        }

        List<PlayerState> NamedPlayers()
        {
            return telemetry.GetAllKnownPlayers()
                .Where(p => !string.IsNullOrWhiteSpace(p.Name) && !p.Name.StartsWith("Player ", StringComparison.OrdinalIgnoreCase))
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(p => p.Connected).ThenByDescending(p => p.LastSeen).First())
                .ToList();
        }

        void RefreshProfileRoster()
        {
            if (profileRoster == null) return;
            var players = NamedPlayers();
            bool serverRunning = telemetry.GetHost().ServerRunning;
            var known = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in players) known[p.Name] = serverRunning && p.Connected;
            if (lastRealm != null)
                foreach (string name in lastRealm.KnownPlayers)
                    if (!string.IsNullOrWhiteSpace(name) && !known.ContainsKey(name)) known.Add(name, false);
            foreach (var s in cachedProfileSessions)
                if (!string.IsNullOrWhiteSpace(s.Player) && !known.ContainsKey(s.Player)) known.Add(s.Player, false);
            string filter = profileSearch == null ? "" : profileSearch.Text.Trim();
            var roster = known.Where(p => filter.Length == 0 || p.Key.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderByDescending(p => p.Value).ThenBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
                .Select(p => new PlayerRosterEntry(p.Key, p.Value)).ToList();
            string signature = string.Join("|", roster.Select(p => p.Name + ":" + p.Online).ToArray());
            profileCount.Text = known.Count + " known  /  " + known.Count(k => k.Value) + " currently online";
            if (signature == rosterSignature) return;
            rosterSignature = signature;
            rosterUpdating = true;
            try
            {
                profileRoster.BeginUpdate();
                profileRoster.Items.Clear();
                foreach (PlayerRosterEntry item in roster) profileRoster.Items.Add(item);
                int index = roster.FindIndex(p => string.Equals(p.Name, selectedProfileName, StringComparison.OrdinalIgnoreCase));
                profileRoster.SelectedIndex = index >= 0 ? index : (roster.Count == 0 ? -1 : 0);
                var selected = profileRoster.SelectedItem as PlayerRosterEntry;
                selectedProfileName = selected == null ? "" : selected.Name;
            }
            finally { profileRoster.EndUpdate(); rosterUpdating = false; }
            nextProfileRender = DateTime.MinValue;
        }

        void RenderProfile()
        {
            if (profileDetail == null || DateTime.Now < nextProfileRender) return;
            nextProfileRender = DateTime.Now.AddSeconds(3);
            if (string.IsNullOrWhiteSpace(selectedProfileName))
            {
                profileName.Text = "No player selected";
                profileStatus.Text = "Choose a player or clear the search to see known identities.";
                foreach (Label text in new[] { profileIdentity, profileSessions, profileConnection, profileIncidents, profileActivity, profileLimitations }) text.Text = "No matching player.";
                return;
            }
            string name = selectedProfileName;
            PlayerState player = NamedPlayers().FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            var events = cachedProfileEvents.Concat(telemetry.GetActivity(120)).ToList();
            PlayerSheetSnapshot sheet = PlayerSheet.Create(name, player, cachedProfileSessions, events, telemetry.GetHost().ServerRunning, DateTime.Now);
            profileName.Text = sheet.Name; profileStatus.Text = sheet.Status;
            profileStatus.ForeColor = sheet.Status.StartsWith("ONLINE") ? Theme.Green : Theme.Muted;
            profileIdentity.Text = sheet.Identity; profileSessions.Text = sheet.Sessions;
            profileConnection.Text = sheet.Connection; profileIncidents.Text = sheet.Incidents;
            profileActivity.Text = sheet.Activity; profileLimitations.Text = sheet.Limitations;
        }

        void UpdatePlayerProfiles()
        {
            if (profileRoster == null) return;
            RefreshProfileHistory();
            RefreshProfileRoster();
            RenderProfile();
        }
    }
}
