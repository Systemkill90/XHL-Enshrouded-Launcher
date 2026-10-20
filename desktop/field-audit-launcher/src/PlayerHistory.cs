using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace XHLLauncher
{
    // Private launcher data, scoped to the selected server. Never a current roster.
    public sealed class PlayerHistoryRecord
    {
        public string Name = "", SteamId = "", Peer = "", Handle = "";
        public int MachineIndex = -1;
        public long CharacterSaveBytes;
        public DateTime IdentityUtc, NetworkUtc;
        public bool NetworkTimeEstimated;
        public PlayerState Network;

        internal static void Remember(IDictionary<string, PlayerHistoryRecord> records, PlayerState p, DateTime evidenceUtc, bool historical)
        {
            if (p == null || string.IsNullOrWhiteSpace(p.Name) || p.Name.StartsWith("Player ")) return;
            PlayerHistoryRecord record;
            if (!records.TryGetValue(p.Name, out record)) record = new PlayerHistoryRecord { Name = p.Name };
            if (record.SteamId.Length > 0 && p.SteamId.Length > 0 && record.SteamId != p.SteamId)
            {
                if (evidenceUtc < record.IdentityUtc) return;
                record = new PlayerHistoryRecord { Name = p.Name }; // Never mix two identified people.
            }
            if (evidenceUtc >= record.IdentityUtc)
            {
                if (p.SteamId.Length > 0) record.SteamId = p.SteamId;
                if (p.Peer.Length > 0) record.Peer = p.Peer;
                if (p.Handle.Length > 0) record.Handle = p.Handle;
                if (p.MachineIndex >= 0) record.MachineIndex = p.MachineIndex;
                if (p.CharacterSaveBytes > 0) record.CharacterSaveBytes = p.CharacterSaveBytes;
                record.IdentityUtc = evidenceUtc;
            }
            DateTime sampleUtc = historical || p.LastNetworkSample == DateTime.MinValue ? evidenceUtc : p.LastNetworkSample.ToUniversalTime();
            if (p.PingMs >= 0 && sampleUtc >= record.NetworkUtc)
            {
                record.Network = TelemetryService.ClonePlayer(p);
                record.Network.Connected = false;
                record.Network.JoinTime = DateTime.MinValue;
                record.NetworkUtc = sampleUtc;
                record.NetworkTimeEstimated = historical;
                if (historical) { record.Network.LastSeen = DateTime.MinValue; record.Network.LastNetworkSample = DateTime.MinValue; record.Network.HealthScore = -1; record.Network.Jitter = -1; record.Network.OneMinuteAverage = -1; record.Network.OneMinutePeak = -1; record.Network.FiveMinutePeak = -1; }
            }
            records[p.Name] = record;
        }

        internal PlayerState FillIdentity(PlayerState current)
        {
            var p = current == null ? new PlayerState { Name = Name } : TelemetryService.ClonePlayer(current);
            if (p.SteamId.Length > 0 && SteamId.Length > 0 && p.SteamId != SteamId) return p;
            if (p.SteamId.Length == 0) p.SteamId = SteamId;
            if (p.Peer.Length == 0) p.Peer = Peer;
            if (p.Handle.Length == 0) p.Handle = Handle;
            if (p.MachineIndex < 0) p.MachineIndex = MachineIndex;
            if (p.CharacterSaveBytes <= 0) p.CharacterSaveBytes = CharacterSaveBytes;
            return p; // Network, online state and join time are deliberately not restored here.
        }
    }

    internal sealed class PlayerHistoryDocument
    {
        public string ServerRoot;
        public PlayerHistoryRecord[] Players = new PlayerHistoryRecord[0];
    }

    internal sealed class PlayerHistoryStore
    {
        readonly string app, server, path;
        bool loaded;
        readonly Dictionary<string, PlayerHistoryRecord> records = new Dictionary<string, PlayerHistoryRecord>(StringComparer.OrdinalIgnoreCase);
        readonly JavaScriptSerializer json = new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024 };

        internal PlayerHistoryStore(string app, string server)
        {
            this.app = app; this.server = Path.GetFullPath(server);
            string key;
            using (var sha = SHA256.Create()) key = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(this.server.ToLowerInvariant()))).Replace("-", "").Substring(0, 16);
            path = Path.Combine(Storage.DataDir(app), "player-history-" + key + ".json");
        }

        internal PlayerHistoryRecord[] Read(IEnumerable<PlayerHistoryRecord> observed, bool persist)
        {
            if (!loaded)
            {
                var initial = new Dictionary<string, PlayerHistoryRecord>(StringComparer.OrdinalIgnoreCase);
                if (File.Exists(path))
                {
                    var saved = json.Deserialize<PlayerHistoryDocument>(File.ReadAllText(path));
                    if (saved == null || !string.Equals(saved.ServerRoot, server, StringComparison.OrdinalIgnoreCase) || saved.Players == null)
                        throw new InvalidDataException("Player history belongs to a different server or is invalid.");
                    foreach (var record in saved.Players) Merge(initial, record);
                }
                var cfg = ConfigReader.Read(server);
                string logs = Path.GetFullPath(Path.Combine(server, cfg.LogDirectory));
                var files = new List<FileInfo>();
                foreach (string folder in new[] { logs, Path.Combine(logs, "backup") })
                {
                    try { files.AddRange(new DirectoryInfo(folder).GetFiles("*.log").Where(f => folder != logs || f.Name.IndexOf("enshrouded", StringComparison.OrdinalIgnoreCase) >= 0)); }
                    catch (DirectoryNotFoundException) { }
                }
                // Bound startup work; existing cache preserves members after log rotation/deletion.
                foreach (var file in files.OrderByDescending(f => f.LastWriteTimeUtc).Take(20).OrderBy(f => f.LastWriteTimeUtc))
                    foreach (var record in TelemetryService.ReadHistoricalPlayers(file.FullName)) Merge(initial, record);
                ReadLegacyHealth(initial);
                foreach (var item in initial) records[item.Key] = item.Value;
                loaded = true;
            }
            foreach (var record in observed) Merge(records, record);
            var result = records.Values.OrderBy(p => p.Name).ToArray();
            if (persist)
            {
                string text = json.Serialize(new PlayerHistoryDocument { ServerRoot = server, Players = result });
                if (!File.Exists(path) || File.ReadAllText(path) != text)
                {
                    string pending = path + ".pending";
                    File.WriteAllText(pending, text);
                    if (File.Exists(path)) File.Replace(pending, path, path + ".bak"); else File.Move(pending, path);
                }
            }
            // Return detached records: later scans cannot change an already-published snapshot.
            return json.Deserialize<PlayerHistoryRecord[]>(json.Serialize(result));
        }

        static void Merge(IDictionary<string, PlayerHistoryRecord> target, PlayerHistoryRecord incoming)
        {
            if (incoming == null || string.IsNullOrWhiteSpace(incoming.Name)) return;
            var p = new PlayerState { Name = incoming.Name, SteamId = incoming.SteamId, Peer = incoming.Peer, Handle = incoming.Handle,
                MachineIndex = incoming.MachineIndex, CharacterSaveBytes = incoming.CharacterSaveBytes };
            PlayerHistoryRecord.Remember(target, p, incoming.IdentityUtc, true);
            var record = target[incoming.Name];
            if (record.SteamId.Length > 0 && incoming.SteamId.Length > 0 && record.SteamId != incoming.SteamId) return;
            if (incoming.Network != null && incoming.NetworkUtc >= record.NetworkUtc)
            {
                record.Network = TelemetryService.ClonePlayer(incoming.Network); record.Network.Connected = false;
                record.NetworkUtc = incoming.NetworkUtc; record.NetworkTimeEstimated = incoming.NetworkTimeEstimated;
            }
        }

        void ReadLegacyHealth(IDictionary<string, PlayerHistoryRecord> target)
        {
            string settings = Path.Combine(Storage.DataDir(app), "settings.xml");
            // The legacy CSV has no server key. Import only when saved settings bind it to this server.
            if (!File.Exists(settings) || !string.Equals(Path.GetFullPath(Storage.LoadSettings(app).ServerRoot), server, StringComparison.OrdinalIgnoreCase)) return;
            string file = Path.Combine(Storage.DataDir(app), "player-health.csv");
            if (!File.Exists(file)) return;
            using (var input = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                long start = Math.Max(0, input.Length - 4 * 1024 * 1024); input.Seek(start, SeekOrigin.Begin);
                using (var reader = new StreamReader(input))
                {
                    if (start > 0) reader.ReadLine();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var f = Csv.ParseLine(line); DateTime when; int ping, health, lost, pending; double avg, jitter; int peak1, peak5;
                        if (f.Length < 13 || !DateTime.TryParse(f[0], out when) || !int.TryParse(f[4], out ping) || ping < 0) continue;
                        if (!int.TryParse(f[2], out health) || !double.TryParse(f[5], NumberStyles.Any, CultureInfo.InvariantCulture, out avg) ||
                            !int.TryParse(f[6], out peak1) || !double.TryParse(f[7], NumberStyles.Any, CultureInfo.InvariantCulture, out jitter) ||
                            !int.TryParse(f[8], out peak5) || !int.TryParse(f[10], out lost) || !int.TryParse(f[11], out pending)) continue;
                        var p = new PlayerState { Name = f[1], PingMs = ping, HealthScore = health, OneMinuteAverage = avg, OneMinutePeak = peak1,
                            Jitter = jitter, FiveMinutePeak = peak5, SessionState = f[9], LostTotal = lost, PendingPackets = pending,
                            LastIssue = f[12], LastNetworkSample = when, HealthReason = "Recorded health history",
                            UpRate = -1, DownRate = -1, RemoteRate = -1, BandwidthLimit = -1 };
                        PlayerHistoryRecord.Remember(target, p, when.ToUniversalTime(), false);
                    }
                }
            }
        }
    }
}
