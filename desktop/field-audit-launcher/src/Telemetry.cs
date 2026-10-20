using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;

namespace XHLLauncher
{
    public class ServerConfigInfo
    {
        public string Name = "VELARIS";
        public string LogDirectory = "./logs";
        public string SaveDirectory = "./savegame";
        public string Ip = "0.0.0.0";
        public int QueryPort = 15637;
        public int GamePort = 15636;
        public int MaxPlayers = 16;
        public string WorldName = "";
    }

    public class TelemetryService : IDisposable
    {
        readonly Dictionary<string,PlayerHistoryRecord> recordedPlayers=new Dictionary<string,PlayerHistoryRecord>(StringComparer.OrdinalIgnoreCase);
        DateTime bootstrapWrittenUtc;
        private TelemetryService() { }
        internal static PlayerHistoryRecord[] ReadHistoricalPlayers(string path){using(var reader=new TelemetryService()){reader.BootstrapLog(path,true,4L*1024L*1024L);return reader.GetRecordedPlayers();}}
        internal PlayerHistoryRecord[] GetRecordedPlayers(){lock(gate){return recordedPlayers.Values.Select(r=>new PlayerHistoryRecord{Name=r.Name,SteamId=r.SteamId,Peer=r.Peer,Handle=r.Handle,MachineIndex=r.MachineIndex,CharacterSaveBytes=r.CharacterSaveBytes,IdentityUtc=r.IdentityUtc,NetworkUtc=r.NetworkUtc,NetworkTimeEstimated=r.NetworkTimeEstimated,Network=r.Network==null?null:ClonePlayer(r.Network)}).ToArray();}}
        void RememberPlayer(PlayerState p,bool historical){PlayerHistoryRecord.Remember(recordedPlayers,p,historical?bootstrapWrittenUtc:DateTime.UtcNow,historical);}
        private readonly object gate = new object();
        private readonly string appBase;
        private readonly LauncherSettings settings;
        private readonly Func<Process> discoverProcess;
        private readonly Dictionary<int, PlayerState> players = new Dictionary<int, PlayerState>();
        private readonly Dictionary<string, PlayerState> peers = new Dictionary<string, PlayerState>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, PlayerState> machines = new Dictionary<int, PlayerState>();
        private readonly Dictionary<string, PlayerState> names = new Dictionary<string, PlayerState>(StringComparer.OrdinalIgnoreCase);
        private readonly List<ActivityItem> activity = new List<ActivityItem>();
        private readonly WorldState world = new WorldState();
        private readonly EngineState engine = new EngineState();
        private readonly HostState host = new HostState();
        private readonly List<double> cpuHistory = new List<double>();
        private readonly List<double> ramHistory = new List<double>();
        private readonly List<double> uploadHistory = new List<double>();
        private readonly List<double> tickHistory = new List<double>();
        private readonly List<double> pingAverageHistory = new List<double>();
        private ServerConfigInfo config;
        private System.Threading.Timer timer;
        private int updating = 0;
        private string logPath = "";
        private long logPosition = 0;
        private PlayerState pendingConnection = null;
        private PlayerState pendingNamedConnection = null;
        private int lastPid;private DateTime processStarted=DateTime.MinValue;
        private TimeSpan prevProcCpu = TimeSpan.Zero;
        private DateTime prevProcSample = DateTime.MinValue;
        private long prevSent = -1, prevReceived = -1;
        private DateTime prevNicSample = DateTime.MinValue;
        private DateTime lastA2s = DateTime.MinValue;
        private DateTime lastRosterSuccess = DateTime.MinValue;
        private int lastRosterCount;
        private DateTime lastInfoSuccess=DateTime.MinValue; private int lastInfoCount;
        public string PlayerCountText { get {lock(gate){if(DateTime.Now-lastInfoSuccess<TimeSpan.FromSeconds(15))return lastInfoCount.ToString();if(DateTime.Now-lastRosterSuccess<TimeSpan.FromSeconds(15))return lastRosterCount.ToString();int n=players.Values.Count(p=>p.Connected&&!p.Name.StartsWith("Player "));return n>0?n+"+":"—";}} }
        public string PlayerCountEvidence {get {lock(gate){return DateTime.Now-lastInfoSuccess<TimeSpan.FromSeconds(15)?(DateTime.Now-lastRosterSuccess<TimeSpan.FromSeconds(15)&&lastInfoCount!=lastRosterCount?"Server count; named roster differs ("+lastRosterCount+")":"Confirmed by server info query"):DateTime.Now-lastRosterSuccess<TimeSpan.FromSeconds(15)?"Confirmed by player query":"Recent log evidence; exact count unavailable";}}}
        private DateTime lastHealthPersist = DateTime.MinValue;
        private DateTime lastPublicLookup = DateTime.MinValue;
        private int publicLookupBusy = 0;
        private bool disposed;
        private DateTime lastResourcesUtc=DateTime.MinValue;

        public event EventHandler Updated;
        public event Action<string> LogFileChanged;
        public event Action<ActivityItem> ActivityAdded;

        public TelemetryService(string appBase, LauncherSettings settings, Func<Process> discoverProcess = null)
        {
            this.appBase = appBase;
            this.settings = settings;
            this.discoverProcess = discoverProcess ?? (()=>new ServerControl(settings.ServerRoot, settings).GetServerProcess());
            config = ConfigReader.Read(settings.ServerRoot);
            if (!string.IsNullOrEmpty(config.Name)) settings.ServerName = config.Name;
            foreach (ActivityItem item in Storage.ReadRecentPlayerEvents(settings.ServerRoot, 12)) activity.Add(item);
            ResolveAndBootstrapLog();
            UpdateLocalNetwork();
            UpdateDiskAndMemory();
        }

        public void Start()
        {
            timer = new System.Threading.Timer(Tick, null, 0, Math.Max(500, settings.RefreshMilliseconds));
        }

        public ServerConfigInfo Config { get { lock (gate) { return config; } } }

        public List<PlayerState> GetPlayers()
        {
            lock (gate)
            {
                List<PlayerState> list = new List<PlayerState>();
                foreach (PlayerState p in players.Values) if (p.Connected && !string.IsNullOrEmpty(p.Name) && !p.Name.StartsWith("Player ")) list.Add(ClonePlayer(p));
                return list.OrderBy(p => p.HealthScore).ThenBy(p => p.Name).ToList();
            }
        }

        public List<PlayerState> GetAllKnownPlayers()
        {
            lock (gate) { return players.Values.Select(ClonePlayer).ToList(); }
        }

        public WorldState GetWorld()
        {
            lock (gate)
            {
                WorldState w = new WorldState();
                w.FlameAltarBases = world.FlameAltarBases; w.WorldEntities = world.WorldEntities; w.SnapshotEntities = world.SnapshotEntities; w.Storages = world.Storages; w.Chunks = world.Chunks;
                w.SnapshotBytes = world.SnapshotBytes; w.VoxelBytes = world.VoxelBytes; w.WaterBytes = world.WaterBytes; w.WorldSaveBytes = world.WorldSaveBytes; w.SaveDurationMs = world.SaveDurationMs; w.SaveTicks = world.SaveTicks;
                w.LastSaveTime = world.LastSaveTime; w.LastSnapshotTime = world.LastSnapshotTime; w.SavesObserved = world.SavesObserved; w.PriorSaveDetected = world.PriorSaveDetected; w.SaveTimes.AddRange(world.SaveTimes); return w;
            }
        }

        public EngineState GetEngine()
        {
            lock (gate)
            {
                EngineState e = new EngineState(); e.UpdateAverageMs = engine.UpdateAverageMs; e.UpdateMaxMs = engine.UpdateMaxMs; e.EntitiesProcessed = engine.EntitiesProcessed; e.BadPerformanceLast60 = engine.BadPerformanceLast60; e.BadPerformanceLastMs = engine.BadPerformanceLastMs; e.BadPerformancePeakMs = engine.BadPerformancePeakMs; e.BadPerformanceEvents.AddRange(engine.BadPerformanceEvents); return e;
            }
        }

        public HostState GetHost()
        {
            lock (gate)
            {
                HostState h = new HostState();
                h.Observation=host.Observation;h.ObservationError=host.ObservationError;h.ObservationAttemptUtc=host.ObservationAttemptUtc;
                h.SampledUtc=host.SampledUtc;h.LastQueryUtc=host.LastQueryUtc;h.QueryResponsive=host.Observation==ProcessObservation.Running&&host.ServerRunning&&host.QueryResponsive&&DateTime.UtcNow-host.LastQueryUtc<TimeSpan.FromSeconds(15);
                h.ServerRunning = host.ServerRunning; h.Pid = host.Pid; h.Uptime = host.Uptime; h.CpuPercent = host.CpuPercent; h.RamBytes = host.RamBytes; h.TotalRamBytes = host.TotalRamBytes; h.UploadMbps = host.UploadMbps; h.DownloadMbps = host.DownloadMbps; h.NicErrors = host.NicErrors; h.NicDiscards = host.NicDiscards; h.AdapterName = host.AdapterName; h.AdapterDescription = host.AdapterDescription; h.LocalIp = host.LocalIp; h.Gateway = host.Gateway; h.Dns = host.Dns; h.LinkSpeed = host.LinkSpeed; h.PublicIp = host.PublicIp; h.PublicIpv4 = host.PublicIpv4; h.PublicIpv6 = host.PublicIpv6; h.Isp = host.Isp; h.Asn = host.Asn; h.ApproxLocation = host.ApproxLocation; h.NetworkType = host.NetworkType; h.VpnStatus = host.VpnStatus; h.DiskFreeBytes = host.DiskFreeBytes; h.DiskTotalBytes = host.DiskTotalBytes;
                if(h.SampledUtc==DateTime.MinValue||DateTime.UtcNow-h.SampledUtc>TimeSpan.FromSeconds(15)){h.Observation=ProcessObservation.Unknown;h.ObservationError="No recent successful process observation. "+host.ObservationError;h.QueryResponsive=false;}
                return h;
            }
        }

        public List<ActivityItem> GetActivity(int max)
        {
            lock (gate) { return activity.OrderByDescending(x => x.Time).Take(max).ToList(); }
        }

        public double[] CpuHistory { get { lock (gate) { return cpuHistory.ToArray(); } } }
        public double[] RamHistory { get { lock (gate) { return ramHistory.ToArray(); } } }
        public double[] UploadHistory { get { lock (gate) { return uploadHistory.ToArray(); } } }
        public double[] TickHistory { get { lock (gate) { return tickHistory.ToArray(); } } }
        public double[] PingAverageHistory { get { lock (gate) { return pingAverageHistory.ToArray(); } } }
        public string CurrentLogPath { get { lock (gate) { return logPath; } } }

        public void RefreshNetworkIdentity()
        {
            lastPublicLookup = DateTime.MinValue;
            UpdateLocalNetwork(); MaybeLookupPublicNetwork(true);
        }

        private void Tick(object state)
        {
            if (disposed || Interlocked.Exchange(ref updating, 1) != 0) return;
            try
            {
                UpdateProcess();
                UpdateNetworkRates();
                UpdateDiskAndMemory();
                TailLog();
                if ((DateTime.Now - lastA2s).TotalSeconds >= 5) { QueryA2S(); lastA2s = DateTime.Now; }
                lock (gate)
                {
                    PruneAndComputePlayerHealth();
                    foreach(var p in players.Values.Where(p=>p.Connected&&p.PingMs>=0&&DateTime.Now-p.LastNetworkSample<TimeSpan.FromSeconds(15)))RememberPlayer(p,false);
                    engine.BadPerformanceEvents.RemoveAll(x => (DateTime.Now - x).TotalSeconds > 60);
                    engine.BadPerformanceLast60 = engine.BadPerformanceEvents.Count;
                    AddHistory(cpuHistory, host.CpuPercent); AddHistory(ramHistory, host.RamBytes / 1073741824.0); AddHistory(uploadHistory, host.UploadMbps); AddHistory(tickHistory, engine.UpdateAverageMs < 0 ? 0 : engine.UpdateAverageMs);
                    List<PlayerState> current = players.Values.Where(p => p.Connected && p.PingMs >= 0).ToList();
                    AddHistory(pingAverageHistory, current.Count == 0 ? 0 : current.Average(p => p.PingMs));
                }
                if ((DateTime.Now - lastHealthPersist).TotalSeconds >= 5)
                {
                    foreach (PlayerState p in GetPlayers()) Storage.AppendHealth(appBase, p);
                    lastHealthPersist = DateTime.Now;
                }
                if (settings.PublicIpLookup && (DateTime.Now - lastPublicLookup).TotalMinutes > 15) MaybeLookupPublicNetwork(false);
                RaiseUpdated();
            }
            catch (Exception ex)
            {
                AddActivity("Warning", "Telemetry refresh error: " + ex.Message, false);
            }
            finally { Interlocked.Exchange(ref updating, 0); }
        }

        private static void AddHistory(List<double> list, double value)
        {
            list.Add(value); while (list.Count > 600) list.RemoveAt(0);
        }

        private void UpdateProcess() { ObserveProcess(discoverProcess); }

        internal void ObserveProcess(Func<Process> discover)
        {
            Process p = null;
            try { p = discover(); }
            catch (Exception ex) { lock(gate) { MarkObservationFailed(host, ex.Message); } return; }
            lock (gate)
            {
                host.ObservationAttemptUtc=DateTime.UtcNow;
                if (p == null)
                {
                    host.Observation=ProcessObservation.Stopped;host.ObservationError="";host.SampledUtc=DateTime.UtcNow;host.ServerRunning=false;
                    host.Pid = 0; host.Uptime = TimeSpan.Zero; host.CpuPercent = 0; host.RamBytes = 0; lastPid = 0; prevProcSample = DateTime.MinValue;host.QueryResponsive=false;host.LastQueryUtc=DateTime.MinValue;lastRosterSuccess=DateTime.MinValue;lastInfoSuccess=DateTime.MinValue; return;
                }
                try
                {
                    p.Refresh();int pid=p.Id;DateTime started=p.StartTime;long ram=p.WorkingSet64;TimeSpan cpu=p.TotalProcessorTime;if(lastPid!=pid){foreach(var old in players.Values){old.Connected=false;old.PingMs=-1;old.PingHistory.Clear();old.LastNetworkSample=DateTime.MinValue;}host.QueryResponsive=false;host.LastQueryUtc=DateTime.MinValue;lastRosterSuccess=DateTime.MinValue;lastInfoSuccess=DateTime.MinValue;} host.Pid = pid;processStarted=started; host.Uptime = DateTime.Now - started; host.RamBytes = ram;
                    DateTime now = DateTime.Now;
                    if (prevProcSample != DateTime.MinValue && lastPid == p.Id)
                    {
                        double elapsed = (now - prevProcSample).TotalMilliseconds;
                        double used = (cpu - prevProcCpu).TotalMilliseconds;
                        if (elapsed > 0) host.CpuPercent = Math.Max(0, Math.Min(100, used / elapsed / Math.Max(1, Environment.ProcessorCount) * 100.0));
                    }
                    prevProcCpu = cpu; prevProcSample = now; lastPid = p.Id;
                    host.ServerRunning=true;host.Observation=ProcessObservation.Running;host.ObservationError="";host.SampledUtc=DateTime.UtcNow;
                }
                catch (Exception ex) { MarkObservationFailed(host, ex.Message); }
                finally {p.Dispose();}
            }
        }

        internal static void MarkObservationFailed(HostState state, string error)
        {
            state.Observation=ProcessObservation.Failed;state.ObservationError=error;
            state.ObservationAttemptUtc=DateTime.UtcNow;state.QueryResponsive=false;
        }

        private void UpdateDiskAndMemory()
        {
                if(DateTime.UtcNow-lastResourcesUtc<TimeSpan.FromSeconds(30))return;
                lastResourcesUtc=DateTime.UtcNow;long free=0,total=0,ram=0;
                try
                {
                    string root = Path.GetPathRoot(settings.ServerRoot); DriveInfo d = new DriveInfo(root); free = d.AvailableFreeSpace; total = d.TotalSize;
                }
                catch { }
                try
                {
                    using (ManagementObjectSearcher s = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                        foreach (ManagementObject o in s.Get()) { ram = Convert.ToInt64(o["TotalPhysicalMemory"]);o.Dispose(); break; }
                }
                catch { }
                lock(gate){if(total>0){host.DiskFreeBytes=free;host.DiskTotalBytes=total;}if(ram>0)host.TotalRamBytes=ram;}
        }

        private NetworkInterface FindPrimaryNic()
        {
            NetworkInterface best = null;
            foreach (NetworkInterface n in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (n.OperationalStatus != OperationalStatus.Up || n.NetworkInterfaceType == NetworkInterfaceType.Loopback || n.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;
                try
                {
                    IPInterfaceProperties ip = n.GetIPProperties();
                    bool gateway = ip.GatewayAddresses.Any(g => g.Address != null && !g.Address.Equals(IPAddress.Any) && !g.Address.Equals(IPAddress.IPv6Any));
                    if (gateway) { if (best == null || n.Speed > best.Speed) best = n; }
                }
                catch { }
            }
            if (best != null) return best;
            return NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);
        }

        private void UpdateLocalNetwork()
        {
            try
            {
                NetworkInterface nic = FindPrimaryNic(); if (nic == null) return;
                IPInterfaceProperties ip = nic.GetIPProperties();
                lock (gate)
                {
                    host.AdapterName = nic.Name; host.AdapterDescription = nic.Description; host.LinkSpeed = nic.Speed;
                    host.LocalIp = string.Join(", ", ip.UnicastAddresses.Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork).Select(a => a.Address.ToString()).ToArray());
                    host.Gateway = string.Join(", ", ip.GatewayAddresses.Select(g => g.Address.ToString()).Where(s => s != "0.0.0.0" && s != "::").ToArray());
                    host.Dns = string.Join(", ", ip.DnsAddresses.Select(d => d.ToString()).ToArray());
                    string n = (nic.Name + " " + nic.Description).ToLowerInvariant();
                    host.VpnStatus = (n.Contains("wireguard") || n.Contains("tailscale") || n.Contains("openvpn") || n.Contains("vpn") || n.Contains("zerotier")) ? "VPN-like primary adapter" : "Not detected";
                }
            }
            catch { }
        }

        private void UpdateNetworkRates()
        {
            try
            {
                NetworkInterface nic = FindPrimaryNic(); if (nic == null) return;
                IPv4InterfaceStatistics st = nic.GetIPv4Statistics(); DateTime now = DateTime.Now;
                lock (gate)
                {
                    if (prevSent >= 0 && prevNicSample != DateTime.MinValue)
                    {
                        double sec = (now - prevNicSample).TotalSeconds;
                        if (sec > 0) { host.UploadMbps = Math.Max(0, (st.BytesSent - prevSent) * 8.0 / sec / 1000000.0); host.DownloadMbps = Math.Max(0, (st.BytesReceived - prevReceived) * 8.0 / sec / 1000000.0); }
                    }
                    prevSent = st.BytesSent; prevReceived = st.BytesReceived; prevNicSample = now;
                    host.NicErrors = st.IncomingPacketsWithErrors + st.OutgoingPacketsWithErrors; host.NicDiscards = st.IncomingPacketsDiscarded + st.OutgoingPacketsDiscarded;
                    host.AdapterName = nic.Name; host.AdapterDescription = nic.Description; host.LinkSpeed = nic.Speed;
                }
            }
            catch { }
        }

        private void MaybeLookupPublicNetwork(bool force)
        {
            if (!settings.PublicIpLookup) return;
            if (!force && (DateTime.Now - lastPublicLookup).TotalMinutes <= 15) return;
            if (Interlocked.Exchange(ref publicLookupBusy, 1) != 0) return;
            ThreadPool.QueueUserWorkItem(delegate
            {
                try { LookupPublicNetwork(); }
                finally { Interlocked.Exchange(ref publicLookupBusy, 0); RaiseUpdated(); }
            });
        }

        private void LookupPublicNetwork()
        {
            lastPublicLookup = DateTime.Now;
            if (!settings.PublicIpLookup) return;
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string v4 = DownloadText("https://api.ipify.org");
                string v6 = DownloadText("https://api6.ipify.org");
                string lookupIp = !string.IsNullOrEmpty(v4) ? v4.Trim() : (!string.IsNullOrEmpty(v6) ? v6.Trim() : "");
                string json = DownloadText(string.IsNullOrEmpty(lookupIp) ? "https://ipwho.is/" : "https://ipwho.is/" + lookupIp);
                if (!string.IsNullOrEmpty(json))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    Dictionary<string, object> root = js.Deserialize<Dictionary<string, object>>(json);
                    lock (gate)
                    {
                        if (!string.IsNullOrEmpty(v4)) host.PublicIpv4 = v4.Trim();
                        if (!string.IsNullOrEmpty(v6) && v6.IndexOf(':') >= 0) host.PublicIpv6 = v6.Trim();
                        object v;
                        if (root.TryGetValue("ip", out v)) host.PublicIp = Convert.ToString(v);
                        if (!string.IsNullOrEmpty(host.PublicIp))
                        {
                            IPAddress addr; if (IPAddress.TryParse(host.PublicIp, out addr)) { if (addr.AddressFamily == AddressFamily.InterNetwork) host.PublicIpv4 = host.PublicIp; else host.PublicIpv6 = host.PublicIp; }
                        }
                        string city = GetDictString(root, "city"), region = GetDictString(root, "region"), country = GetDictString(root, "country");
                        host.ApproxLocation = string.Join(", ", new string[] { city, region, country }.Where(x => !string.IsNullOrEmpty(x)).ToArray());
                        Dictionary<string, object> conn = GetDict(root, "connection");
                        if (conn != null) { host.Isp = GetDictString(conn, "isp"); object asn; if (conn.TryGetValue("asn", out asn)) host.Asn = "AS" + Convert.ToString(asn); }
                        Dictionary<string, object> sec = GetDict(root, "security");
                        if (sec != null)
                        {
                            bool vpn = GetDictBool(sec, "vpn"), proxy = GetDictBool(sec, "proxy"), tor = GetDictBool(sec, "tor"), hosting = GetDictBool(sec, "hosting");
                            if (vpn) host.VpnStatus = "VPN indicated by public route"; else if (proxy) host.VpnStatus = "Proxy indicated by public route"; else if (tor) host.VpnStatus = "Tor indicated by public route";
                            host.NetworkType = hosting ? "Hosting / data center" : "Public ISP route";
                        }
                    }
                }
            }
            catch { }
        }

        private static string DownloadText(string url)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.UserAgent = "XHLLauncher/1.0"; req.Timeout = 2500; req.ReadWriteTimeout = 2500; req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse()) using (StreamReader sr = new StreamReader(res.GetResponseStream())) return sr.ReadToEnd();
            }
            catch { return ""; }
        }

        private static Dictionary<string, object> GetDict(Dictionary<string, object> d, string k)
        {
            object v; if (!d.TryGetValue(k, out v)) return null; return v as Dictionary<string, object>;
        }
        private static string GetDictString(Dictionary<string, object> d, string k) { object v; return d != null && d.TryGetValue(k, out v) && v != null ? Convert.ToString(v) : ""; }
        private static bool GetDictBool(Dictionary<string, object> d, string k) { object v; bool b; return d != null && d.TryGetValue(k, out v) && v != null && bool.TryParse(Convert.ToString(v), out b) && b; }

        private void ResolveAndBootstrapLog()
        {
            try
            {
                config = ConfigReader.Read(settings.ServerRoot);
                string ld = config.LogDirectory;
                if (!Path.IsPathRooted(ld)) ld = Path.GetFullPath(Path.Combine(settings.ServerRoot, ld));
                if (!Directory.Exists(ld)) return;
                FileInfo fi = new DirectoryInfo(ld).GetFiles("*.log").OrderByDescending(f => f.LastWriteTime).FirstOrDefault(f => f.Name.ToLowerInvariant().Contains("enshrouded"));
                if (fi == null) fi = new DirectoryInfo(ld).GetFiles("*.log").OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                if (fi == null) return;
                logPath = fi.FullName;
                BootstrapLog(logPath);
                if (LogFileChanged != null) LogFileChanged(logPath);
            }
            catch { }
        }

        private void BootstrapLog(string path,bool strict=false,long maxBytes=64L*1024L*1024L)
        {
            try
            {
                FileInfo fi = new FileInfo(path);bootstrapWrittenUtc=fi.LastWriteTimeUtc; long start = 0;
                long max = maxBytes; if (fi.Length > max) start = fi.Length - max;
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    fs.Seek(start, SeekOrigin.Begin); if (start > 0) { while (fs.Position < fs.Length && fs.ReadByte() != '\n') { } }
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, true, 4096, true))
                    {
                        string line; while ((line = sr.ReadLine()) != null) ProcessLine(line, true);
                        logPosition = fs.Position;
                    }
                }
                // Bootstrap reconstructs identities/world state from historical log content, but
                // historical login lines are not proof that a player is online right now.
                lock (gate)
                {
                    foreach (PlayerState p in players.Values)
                    {
                        p.Connected = false;
                        p.JoinTime = DateTime.MinValue;
                        p.LastSeen = DateTime.MinValue;
                        p.SessionState = "Historical";p.PingMs=-1;p.PingHistory.Clear();p.LastNetworkSample=DateTime.MinValue;p.LostDelta=0;
                    }
                    pendingConnection = null;
                    pendingNamedConnection = null;
                }
                // A file write is not evidence of a completed game save.
            }
            catch {if(strict)throw;}
        }

        private void ProbeSaveDirectoryTimestamp()
        {
            try
            {
                string save = config.SaveDirectory;
                if (string.IsNullOrEmpty(save)) save = "./savegame";
                if (!Path.IsPathRooted(save)) save = Path.GetFullPath(Path.Combine(settings.ServerRoot, save));
                if (!Directory.Exists(save)) return;
                DateTime newest = DateTime.MinValue;
                foreach (string f in Directory.GetFiles(save, "*", SearchOption.AllDirectories))
                {
                    try { DateTime t = File.GetLastWriteTime(f); if (t > newest) newest = t; } catch { }
                }
                if (newest != DateTime.MinValue)
                {
                    lock (gate) { if (world.LastSaveTime == DateTime.MinValue || newest > world.LastSaveTime) world.LastSaveTime = newest; }
                }
            }
            catch { }
        }

        private void TailLog()
        {
            string path;
            lock (gate) path = logPath;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) { ResolveAndBootstrapLog(); return; }
            try
            {
                FileInfo fi = new FileInfo(path);
                if (fi.Length < logPosition) { ResolveAndBootstrapLog(); return; }
                if (fi.Length == logPosition) return;
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    fs.Seek(logPosition, SeekOrigin.Begin);
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, true, 4096, true))
                    {
                        string line; while ((line = sr.ReadLine()) != null) ProcessLine(line, false); logPosition = fs.Position;
                    }
                }
            }
            catch { }
        }

        private void ProcessLine(string line, bool bootstrap)
        {
            if (string.IsNullOrWhiteSpace(line)) return;
            DateTime sampleTime=DateTime.Now;var stamp=Regex.Match(line,@"^\[[A-Z]\s+(\d+):(\d+):(\d+)[, .](\d+)\]");if(stamp.Success){sampleTime=DateTime.MinValue;if(processStarted!=DateTime.MinValue){try{sampleTime=processStarted.AddHours(double.Parse(stamp.Groups[1].Value)).AddMinutes(double.Parse(stamp.Groups[2].Value)).AddSeconds(double.Parse(stamp.Groups[3].Value)).AddMilliseconds(double.Parse(stamp.Groups[4].Value));if(sampleTime>DateTime.Now.AddSeconds(5))sampleTime=DateTime.MinValue;}catch{}}}
            line=Regex.Replace(line,@"^\[[A-Z]\s+\d+:\d+:\d+[, .]\d+\]\s*", "");
            Match m;
            lock (gate)
            {
                m = Regex.Match(line, @"\[online\]\s+Added peer\s+(\d+\(\d+\))\s+\(steamid:(\d+)\)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    string peer = m.Groups[1].Value, steam = m.Groups[2].Value; int idx = Int32.Parse(peer.Split('(')[0]); PlayerState p;
                    if (!players.TryGetValue(idx, out p) || p.SteamId != steam) { p = NewPlayer(idx); p.SteamId = steam; players[idx] = p; }
                    else if (p.Peer != peer && p.Connected) p.ReconnectCount++;
                    p.Peer = peer; p.Connected = true; p.LastSeen = DateTime.Now; p.SessionState = "Connecting"; peers[peer] = p; pendingConnection = p; return;
                }
                m = Regex.Match(line, @"\[session\]\s+Add remote machine index\s+(\d+)", RegexOptions.IgnoreCase);
                if (m.Success) { if (pendingConnection != null) { int mi = Int32.Parse(m.Groups[1].Value); pendingConnection.MachineIndex = mi; machines[mi] = pendingConnection; } return; }
                m = Regex.Match(line, @"Remote player added\.\s+Player handle:\s+(\d+\(\d+\))", RegexOptions.IgnoreCase);
                if (m.Success) { string h = m.Groups[1].Value; int idx = Int32.Parse(h.Split('(')[0]); PlayerState p = GetOrCreate(idx); p.Handle = h; p.Connected = true; p.LastSeen = DateTime.Now; pendingConnection = p; return; }
                m = Regex.Match(line, @"\[server\]\s+Machine\s+'(\d+)':\s+Player\s+'(\d+\(\d+\))'\s+logged in", RegexOptions.IgnoreCase);
                if (m.Success) { int mi = Int32.Parse(m.Groups[1].Value); string h = m.Groups[2].Value; int idx = Int32.Parse(h.Split('(')[0]); PlayerState p = GetOrCreate(idx); p.Handle = h; p.MachineIndex = mi; p.Connected = true; p.LastSeen = DateTime.Now; machines[mi] = p; pendingNamedConnection = p; return; }
                m = Regex.Match(line, @"\[server\]\s+Player\s+'(.+?)'\s+logged in", RegexOptions.IgnoreCase);
                if (m.Success) { PlayerState p = pendingNamedConnection ?? pendingConnection; if (p != null) BindName(p, m.Groups[1].Value, bootstrap); pendingNamedConnection = null; return; }
                m = Regex.Match(line, @"\[server\]\s+Remove Player\s+'(.+?)'", RegexOptions.IgnoreCase);
                if (m.Success) { PlayerState p; if (names.TryGetValue(m.Groups[1].Value, out p)) { p.Connected = false; if (!bootstrap) AddActivity("Info", p.Name + " disconnected", true); } return; }
                m = Regex.Match(line, @"Sending Character Savegame\s+'(.+?)'(?:\s+Size:([0-9',]+))?", RegexOptions.IgnoreCase);
                if (m.Success) { PlayerState p; if (names.TryGetValue(m.Groups[1].Value, out p)) { p.LastSeen = DateTime.Now; long b; if (m.Groups[2].Success && long.TryParse(m.Groups[2].Value.Replace("'", "").Replace(",", ""), out b)) p.CharacterSaveBytes = b;RememberPlayer(p,bootstrap); } return; }
                m = Regex.Match(line, @"^\s*m#(\d+)\([^)]+\):\s+up\s+([0-9,]+).*?down\s+([0-9,]+).*?remote\s+([0-9,]+).*?limit\s+([0-9,]+).*?lost\s+([0-9,]+),\s+ping\s+(\d+)\s+ms,\s+(\S+)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    int mi = Int32.Parse(m.Groups[1].Value); PlayerState p; if (machines.TryGetValue(mi, out p))
                    {
                        p.UpRate = Num(m.Groups[2].Value); p.DownRate = Num(m.Groups[3].Value); p.RemoteRate = Num(m.Groups[4].Value); p.BandwidthLimit = Num(m.Groups[5].Value);
                        int lost = (int)Num(m.Groups[6].Value); p.LostDelta = Math.Max(0, lost - p.LostTotal); p.LostTotal = lost; p.PingMs = Int32.Parse(m.Groups[7].Value); p.SessionState = m.Groups[8].Value; p.LastSeen = DateTime.Now;p.LastNetworkSample=bootstrap?DateTime.MinValue:sampleTime;
                        if (!bootstrap && p.PingMs >= 0 && sampleTime!=DateTime.MinValue) p.PingHistory.Add(new PingSample { Time = sampleTime, Ping = p.PingMs });
                        RememberPlayer(p,bootstrap);
                    }
                    return;
                }
                m = Regex.Match(line, @"Snapshot took\s+([0-9.]+)\s+ms:\s+Entities:\s+([0-9,]+).*?Storages:\s+([0-9,]+).*?Chunks:\s+([0-9,]+).*?TotalSize:\s*~?([0-9,]+)", RegexOptions.IgnoreCase);
                if (m.Success) { world.SnapshotEntities = (int)Num(m.Groups[2].Value); world.Storages = (int)Num(m.Groups[3].Value); world.Chunks = (int)Num(m.Groups[4].Value); world.SnapshotBytes = (long)Num(m.Groups[5].Value); world.LastSnapshotTime = bootstrap ? DateTime.MinValue : DateTime.Now; return; }
                m = Regex.Match(line, @"^Entities:\s+([0-9,]+)$", RegexOptions.IgnoreCase); if (m.Success) { world.WorldEntities = (int)Num(m.Groups[1].Value); return; }
                m = Regex.Match(line, @"VoxelSaveSize:\s+([0-9,]+)", RegexOptions.IgnoreCase); if (m.Success) { world.VoxelBytes = (long)Num(m.Groups[1].Value); return; }
                m = Regex.Match(line, @"WaterSaveSize:\s+([0-9,]+)", RegexOptions.IgnoreCase); if (m.Success) { world.WaterBytes = (long)Num(m.Groups[1].Value); return; }
                m = Regex.Match(line, @"\[savexxx\]\s+SAVE\s+(\d+)\s+bases\s+([0-9,]+)\s+entities\s+size\s+([0-9,]+)", RegexOptions.IgnoreCase);
                if (m.Success) { world.FlameAltarBases = Int32.Parse(m.Groups[1].Value); world.WorldEntities = (int)Num(m.Groups[2].Value); world.WorldSaveBytes = (long)Num(m.Groups[3].Value); return; }
                m = Regex.Match(line, @"\[server\]\s+Save Serialization took\s+([0-9.]+)\s+ms\s+over\s+(\d+)\s+Ticks", RegexOptions.IgnoreCase);
                if (m.Success) { world.SaveDurationMs = Double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture); world.SaveTicks = Int32.Parse(m.Groups[2].Value); return; }
                if (Regex.IsMatch(line, @"\[server\]\s+Saved\s*$", RegexOptions.IgnoreCase))
                {
                    if (bootstrap) { world.PriorSaveDetected = true; return; }
                    if (!bootstrap)
                    {
                        world.LastSaveTime = DateTime.Now; world.SavesObserved++; world.SaveTimes.Add(world.LastSaveTime); while (world.SaveTimes.Count > 100) world.SaveTimes.RemoveAt(0);
                        AddActivity("Save", "Autosave completed" + (world.SaveDurationMs >= 0 ? " (" + world.SaveDurationMs.ToString("0") + " ms)" : ""), true);
                        Storage.AppendAutoSave(appBase, new AutoSaveRecord { Time = world.LastSaveTime, DurationMs = world.SaveDurationMs, WorldSaveBytes = world.WorldSaveBytes, SnapshotBytes = world.SnapshotBytes, Bases = world.FlameAltarBases, Entities = world.WorldEntities, Chunks = world.Chunks });
                    }
                    return;
                }
                m = Regex.Match(line, @"\[ecss\]\s+Stats:\s+Upd:[0-9,]+\s+\(([0-9.]+)ms\).*?Max:([0-9.]+)ms\s+Avg:([0-9.]+)ms.*?Ent:([0-9,]+)", RegexOptions.IgnoreCase);
                if (m.Success) { engine.UpdateMaxMs = Double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture); engine.UpdateAverageMs = Double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture); engine.EntitiesProcessed = (long)Num(m.Groups[4].Value); return; }
                m = Regex.Match(line, @"\[server\]\s+Bad Performance\s+([0-9,]+)\s+us\s+\(peak:\s*([0-9,]+)\s+us\)", RegexOptions.IgnoreCase);
                if (m.Success) { engine.BadPerformanceLastMs = Num(m.Groups[1].Value) / 1000.0; engine.BadPerformancePeakMs = Num(m.Groups[2].Value) / 1000.0; if (!bootstrap) engine.BadPerformanceEvents.Add(DateTime.Now); return; }
                m = Regex.Match(line, @"Could not prune enough replication states for player\s+(\d+)", RegexOptions.IgnoreCase);
                if (m.Success) { PlayerState p = GetOrCreate(Int32.Parse(m.Groups[1].Value)); Issue(p, "Replication", 1, 0, line, bootstrap); return; }
                m = Regex.Match(line, @"(?:([0-9]+)x:\s*)?\[session\]\s+Received acknowledge for unknown packet from peer\s+(\d+\(\d+\)).*pending packets:\s*([0-9,]+)", RegexOptions.IgnoreCase);
                if (m.Success) { PlayerState p = FindPeer(m.Groups[2].Value); int c = m.Groups[1].Success ? Int32.Parse(m.Groups[1].Value) : 1; Issue(p, "UnknownAck", c, (int)Num(m.Groups[3].Value), line, bootstrap); return; }
                m = Regex.Match(line, @"\[online\]\s+Timeout for peer\s+(\d+\(\d+\))", RegexOptions.IgnoreCase);
                if (m.Success) { PlayerState p = FindPeer(m.Groups[1].Value); if (p != null) { p.TimeoutCount++; p.Connected = false; } Issue(p, "Timeout", 1, 0, line, bootstrap); return; }
                m = Regex.Match(line, @"Congestion detected for machine\s+(\d+)\([^)]*\)\s+\(rtt\s+([0-9.]+)\s*(ms|s),\s+reference bandwidth\s+([0-9.]+)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    PlayerState p; machines.TryGetValue(Int32.Parse(m.Groups[1].Value), out p);
                    if (p != null)
                    {
                        double rtt = Double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                        if (m.Groups[3].Value.Equals("s", StringComparison.OrdinalIgnoreCase)) rtt *= 1000.0;
                        p.CongestionRttMs = rtt;
                        p.ReferenceBandwidth = Double.Parse(m.Groups[4].Value, CultureInfo.InvariantCulture);
                        p.SessionState = "Congested";
                    }
                    Issue(p, "Congestion", 1, 0, line, bootstrap); return;
                }
                m = Regex.Match(line, @"Congestion detected for machine\s+(\d+)\([^)]*\)\s+\(during learning phase\)", RegexOptions.IgnoreCase);
                if (m.Success) { PlayerState p; machines.TryGetValue(Int32.Parse(m.Groups[1].Value), out p); if (p != null) p.SessionState = "Congested"; Issue(p, "Congestion", 1, 0, line, bootstrap); return; }
                m = Regex.Match(line, @"Congestion state reset for machine\s+(\d+)\([^)]*\)", RegexOptions.IgnoreCase);
                if (m.Success) { PlayerState p; machines.TryGetValue(Int32.Parse(m.Groups[1].Value), out p); if (p != null) { p.SessionState = "OperatingNormally"; p.CongestionRttMs = -1; } return; }
                m = Regex.Match(line, @"Failed to send Session Lobby Info to peer\s+(\d+\(\d+\))", RegexOptions.IgnoreCase);
                if (m.Success) { Issue(FindPeer(m.Groups[1].Value), "SendFailure", 1, 0, line, bootstrap); return; }
                if (Regex.IsMatch(line, @"\[ecs\]\s+Build zones changed", RegexOptions.IgnoreCase)) { if (!bootstrap) AddActivity("World", "Build zones changed", true); return; }
            }
        }

        private void BindName(PlayerState p, string name, bool bootstrap)
        {
            p.Name = name; p.Connected = true; p.LastSeen = DateTime.Now; if (p.JoinTime == DateTime.MinValue) p.JoinTime = DateTime.Now; names[name] = p;
            RememberPlayer(p,bootstrap);
            if (!bootstrap) AddActivity("Player", name + " connected", true);
        }
        private PlayerState NewPlayer(int idx) { return new PlayerState { PlayerIndex = idx, JoinTime = DateTime.Now, LastSeen = DateTime.Now, Connected = true, Name = "Player " + idx }; }
        private PlayerState GetOrCreate(int idx) { PlayerState p; if (!players.TryGetValue(idx, out p)) { p = NewPlayer(idx); players[idx] = p; } return p; }
        private PlayerState FindPeer(string peer) { PlayerState p; if (peers.TryGetValue(peer, out p)) return p; int idx; string s = peer.Split('(')[0]; if (Int32.TryParse(s, out idx) && players.TryGetValue(idx, out p)) return p; return null; }
        private static double Num(string s) { double v; Double.TryParse(s.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out v); return v; }

        private void Issue(PlayerState p, string kind, int count, int pending, string detail, bool bootstrap)
        {
            if (bootstrap || p == null) return;
            p.LastIssue = kind; p.LastIssueTime = DateTime.Now; if (pending > 0) { p.PendingPackets = pending; p.PeakPending = Math.Max(p.PeakPending, pending); }
            p.Issues.Add(new IssueEvent { Time = DateTime.Now, Kind = kind, Count = Math.Max(1, count), Pending = pending, Detail = detail });
            while (p.Issues.Count > 1000) p.Issues.RemoveAt(0);
            RememberPlayer(p,false);
            if (kind == "Timeout") AddActivity("Critical", p.Name + " timed out", true);
            else if (kind == "Congestion") AddActivity("Warning", p.Name + " game connection congested", true);
            else if (kind == "SendFailure") AddActivity("Warning", p.Name + " server send failure", true);
            else if (kind == "Replication" && p.Issues.Count(x => x.Kind == "Replication" && (DateTime.Now - x.Time).TotalSeconds < 10) == 1) AddActivity("Warning", p.Name + " replication falling behind", true);
        }

        private void PruneAndComputePlayerHealth()
        {
            DateTime now = DateTime.Now;
            foreach (PlayerState p in players.Values)
            {
                p.PingHistory.RemoveAll(x => (now - x.Time).TotalMinutes > 5);
                p.Issues.RemoveAll(x => (now - x.Time).TotalMinutes > 10);
                if((now-p.LastSeen).TotalSeconds>30)p.Connected=false;
                if((now-p.LastNetworkSample).TotalSeconds>15){p.PingMs=-1;p.LostDelta=0;}
                p.OneMinuteAverage=-1;p.OneMinutePeak=-1;p.Jitter=-1;
                List<PingSample> one = p.PingHistory.Where(x => (now - x.Time).TotalSeconds <= 60).ToList();
                if (one.Count > 0)
                {
                    p.OneMinuteAverage = one.Average(x => x.Ping); p.OneMinutePeak = one.Max(x => x.Ping);
                    if (one.Count > 1)
                    {
                        double sum = 0; for (int i = 1; i < one.Count; i++) sum += Math.Abs(one[i].Ping - one[i - 1].Ping); p.Jitter = sum / (one.Count - 1);
                    }
                }
                p.FiveMinutePeak = p.PingHistory.Count > 0 ? p.PingHistory.Max(x => x.Ping) : -1;
                if(p.PingMs<0){p.HealthScore=-1;p.HealthReason="No recent network samples";continue;}
                int score = 100; List<string> reasons = new List<string>();
                int ping = p.PingMs >= 0 ? p.PingMs : (int)p.OneMinuteAverage;
                if (ping >= 250) { score -= 45; reasons.Add("very high ping"); } else if (ping >= 160) { score -= 30; reasons.Add("high ping"); } else if (ping >= 100) { score -= 15; reasons.Add("elevated ping"); } else if (ping >= 70) score -= 6;
                if (p.Jitter >= 45) { score -= 30; reasons.Add("severe jitter"); } else if (p.Jitter >= 25) { score -= 20; reasons.Add("high jitter"); } else if (p.Jitter >= 12) { score -= 9; reasons.Add("jitter"); }
                if (p.OneMinutePeak > p.OneMinuteAverage + 120 && p.OneMinutePeak > 180) { score -= 18; reasons.Add("short ping spikes"); }
                else if (p.OneMinutePeak > p.OneMinuteAverage + 60 && p.OneMinutePeak > 100) { score -= 10; reasons.Add("ping spikes"); }
                if (p.LostDelta > 0) { score -= Math.Min(20, p.LostDelta * 3); reasons.Add("packet loss increasing"); }
                if (p.SessionState.Equals("Congested", StringComparison.OrdinalIgnoreCase)) { score -= 30; reasons.Add("game reports congestion"); }
                int rep = p.Issues.Where(x => x.Kind == "Replication" && (now - x.Time).TotalSeconds <= 60).Sum(x => x.Count);
                int ack = p.Issues.Where(x => x.Kind == "UnknownAck" && (now - x.Time).TotalSeconds <= 60).Sum(x => x.Count);
                int send = p.Issues.Where(x => x.Kind == "SendFailure" && (now - x.Time).TotalSeconds <= 60).Sum(x => x.Count);
                int timeout = p.Issues.Where(x => x.Kind == "Timeout" && (now - x.Time).TotalSeconds <= 120).Sum(x => x.Count);
                if (rep >= 20) { score -= 28; reasons.Add("replication backlog"); } else if (rep > 0) { score -= 10; reasons.Add("replication errors"); }
                if (ack >= 50) { score -= 20; reasons.Add("acknowledgement errors"); } else if (ack > 0) score -= 7;
                if (p.PendingPackets >= 250) { score -= 28; reasons.Add("large packet queue"); } else if (p.PendingPackets >= 100) { score -= 12; reasons.Add("packet queue elevated"); }
                if (send > 0) { score -= 18; reasons.Add("send failures"); }
                if (timeout > 0) { score -= 45; reasons.Add("recent timeout"); }
                if (engine.BadPerformanceLast60 >= 20) { score -= 12; reasons.Add("server frame pressure"); } else if (engine.BadPerformanceLast60 >= 5) score -= 5;
                p.HealthScore = Math.Max(0, Math.Min(100, score)); p.HealthReason = reasons.Count == 0 ? "Stable" : string.Join(", ", reasons.Distinct().Take(3).ToArray());
            }
        }

        private void QueryA2S()
        {
            int pid;lock(gate){if(!host.ServerRunning)return;pid=host.Pid;}
            string address=string.IsNullOrWhiteSpace(config.Ip)||config.Ip=="0.0.0.0"?"127.0.0.1":config.Ip;
            try {var info=ServerQuery.Read(address,config.QueryPort,350);lock(gate){if(host.Pid==pid){host.QueryResponsive=true;host.LastQueryUtc=DateTime.UtcNow;lastInfoCount=info.Players;lastInfoSuccess=DateTime.Now;}}}catch{}
            try
            {
                List<A2SPlayer> q = A2S.Query(address, config.QueryPort, 350);
                lock (gate)
                {
                    if(host.Pid!=pid)return;host.QueryResponsive=true;host.LastQueryUtc=DateTime.UtcNow;
                    foreach (A2SPlayer ap in q)
                    {
                        if (string.IsNullOrEmpty(ap.Name)) continue; PlayerState p;
                        if (names.TryGetValue(ap.Name, out p)) { p.Connected = true;p.LastSeen=DateTime.Now; if (p.JoinTime == DateTime.MinValue && ap.DurationSeconds > 0) p.JoinTime = DateTime.Now.AddSeconds(-ap.DurationSeconds); }
                        else { int idx = -1000 - names.Count; p = NewPlayer(idx); p.Name = ap.Name; p.JoinTime = DateTime.Now.AddSeconds(-ap.DurationSeconds); names[ap.Name] = p; players[idx] = p; }
                    }
                    HashSet<string> online = new HashSet<string>(q.Where(x => !string.IsNullOrEmpty(x.Name)).Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
                    foreach (PlayerState p in players.Values) if (!p.Name.StartsWith("Player ") && !online.Contains(p.Name) ) p.Connected = false;
                    lastRosterCount=q.Count;lastRosterSuccess=DateTime.Now;
                }
            }
            catch { }
        }

        private void AddActivity(string level, string text, bool raise)
        {
            ActivityItem item = new ActivityItem(DateTime.Now, level, text);
            lock (gate) { activity.Add(item); while (activity.Count > 200) activity.RemoveAt(0); }
            if (raise && ActivityAdded != null) ActivityAdded(item);
        }

        private void RaiseUpdated() { if (Updated != null) Updated(this, EventArgs.Empty); }

        internal static PlayerState ClonePlayer(PlayerState p)
        {
            PlayerState c = new PlayerState(); c.Name = p.Name; c.PlayerIndex = p.PlayerIndex; c.Peer = p.Peer; c.SteamId = p.SteamId; c.MachineIndex = p.MachineIndex; c.Handle = p.Handle; c.Connected = p.Connected; c.JoinTime = p.JoinTime; c.LastSeen = p.LastSeen; c.LastNetworkSample=p.LastNetworkSample;c.PingMs = p.PingMs; c.LostTotal = p.LostTotal; c.LostDelta = p.LostDelta; c.PendingPackets = p.PendingPackets; c.PeakPending = p.PeakPending; c.TimeoutCount = p.TimeoutCount; c.ReconnectCount = p.ReconnectCount; c.SessionState = p.SessionState; c.UpRate = p.UpRate; c.DownRate = p.DownRate; c.RemoteRate = p.RemoteRate; c.BandwidthLimit = p.BandwidthLimit; c.CongestionRttMs = p.CongestionRttMs; c.ReferenceBandwidth = p.ReferenceBandwidth; c.CharacterSaveBytes = p.CharacterSaveBytes; c.LastIssueTime = p.LastIssueTime; c.LastIssue = p.LastIssue; c.HealthScore = p.HealthScore; c.HealthReason = p.HealthReason; c.Jitter = p.Jitter; c.OneMinuteAverage = p.OneMinuteAverage; c.OneMinutePeak = p.OneMinutePeak; c.FiveMinutePeak = p.FiveMinutePeak; return c;
        }

        public void Dispose() { disposed = true; if (timer != null) timer.Dispose(); }
    }

    public class A2SPlayer { public string Name; public int Score; public double DurationSeconds; }
    public static class A2S
    {
        public static List<A2SPlayer> Query(string host, int port, int timeout)
        {
            List<A2SPlayer> list = new List<A2SPlayer>(); using (UdpClient client = new UdpClient())
            {
                client.Client.ReceiveTimeout = timeout; client.Connect(host, port); IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                byte[] cr = new byte[] { 255,255,255,255,0x55,255,255,255,255 }; client.Send(cr, cr.Length); byte[] cres = client.Receive(ref remote);
                if(cres.Length>=6 && cres[4]==0x44)return Decode(cres);
                if (cres.Length < 9 || cres[4] != 0x41) throw new IOException("Player query unavailable"); byte[] req = new byte[9]; req[0]=req[1]=req[2]=req[3]=255; req[4]=0x55; Array.Copy(cres,5,req,5,4); client.Send(req,req.Length); byte[] res = client.Receive(ref remote);
                return Decode(res);
            }
        }
        internal static List<A2SPlayer> Decode(byte[] res) {
                var list=new List<A2SPlayer>();
                if (res.Length < 6 || res[0]!=255 || res[1]!=255 || res[2]!=255 || res[3]!=255 || res[4] != 0x44) throw new IOException("Invalid player query reply"); int count = res[5], off = 6;
                for (int i=0;i<count && off<res.Length;i++)
                {
                    off++; List<byte> nb = new List<byte>(); while (off<res.Length && res[off]!=0) { nb.Add(res[off]); off++; } if (off<res.Length) off++; if (off+8>res.Length) throw new IOException("Truncated player query reply");
                    string name = Encoding.UTF8.GetString(nb.ToArray()); int score = BitConverter.ToInt32(res,off); off+=4; float dur = BitConverter.ToSingle(res,off); off+=4; list.Add(new A2SPlayer { Name=name, Score=score, DurationSeconds=Math.Max(0,dur) });
                }
            if(list.Count!=count)throw new IOException("Incomplete player query reply");
            return list;
        }
    }

    public static class ConfigReader
    {
        public static ServerConfigInfo Read(string root)
        {
            ServerConfigInfo c = new ServerConfigInfo(); string path = Path.Combine(root, "enshrouded_server.json"); if (!File.Exists(path)) return c;
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer(); Dictionary<string, object> d = js.Deserialize<Dictionary<string, object>>(File.ReadAllText(path)); object v;
                if (d.TryGetValue("name", out v)) c.Name = Convert.ToString(v); if (d.TryGetValue("logDirectory", out v)) c.LogDirectory = Convert.ToString(v); if (d.TryGetValue("saveDirectory", out v)) c.SaveDirectory = Convert.ToString(v);
                if (d.TryGetValue("ip", out v)) c.Ip = Convert.ToString(v); if (d.TryGetValue("queryPort", out v)) Int32.TryParse(Convert.ToString(v), out c.QueryPort); if (d.TryGetValue("gamePort", out v)) Int32.TryParse(Convert.ToString(v), out c.GamePort); if (d.TryGetValue("slotCount", out v)) Int32.TryParse(Convert.ToString(v), out c.MaxPlayers);
                if (d.TryGetValue("worldName", out v)) c.WorldName = Convert.ToString(v);
            }
            catch { }
            if (c.QueryPort <= 0) c.QueryPort = 15637; if (c.GamePort <= 0) c.GamePort = 15636; if (c.MaxPlayers <= 0) c.MaxPlayers = 16; return c;
        }
    }
}

