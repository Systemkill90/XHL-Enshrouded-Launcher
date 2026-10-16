using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace XHLLauncher
{
    [Serializable]
    public class LauncherSettings
    {
        public string ServerRoot = "";
        public string ServerName = "VELARIS";
        public string SteamCmdPath = "";
        public bool PublicIpLookup = true;
        public bool AutoRestartOnCrash = false;
        public bool AutoBackup = false;
        public int BackupIntervalMinutes = 30;
        public int RefreshMilliseconds = 1000;
        public int BackupRetentionCount = 30;
        public string ActiveModProfile = "Velaris Normal";
        public List<ModProfile> ModProfiles = new List<ModProfile>();
        public List<ExternalTool> ExternalTools = new List<ExternalTool>();
    }

    [Serializable]
    public class ModProfile
    {
        [XmlAttribute] public string Name = "";
        [XmlArrayItem("ModId")] public List<string> EnabledModIds = new List<string>();
    }

    [Serializable]
    public class ExternalTool
    {
        [XmlAttribute] public string Name = "";
        public string Path = "";
        public string Arguments = "";
    }

    public class PlayerState
    {
        public string Name = "Unknown";
        public int PlayerIndex = -1;
        public string Peer = "";
        public string SteamId = "";
        public int MachineIndex = -1;
        public string Handle = "";
        public bool Connected = false;
        public DateTime JoinTime = DateTime.MinValue;
        public DateTime LastSeen = DateTime.MinValue;
        public int PingMs = -1;
        public int LostTotal = 0;
        public int LostDelta = 0;
        public int PendingPackets = 0;
        public int PeakPending = 0;
        public int TimeoutCount = 0;
        public int ReconnectCount = 0;
        public string SessionState = "Connecting";
        public double UpRate = 0;
        public double DownRate = 0;
        public double RemoteRate = 0;
        public double BandwidthLimit = 0;
        public double CongestionRttMs = -1;
        public double ReferenceBandwidth = 0;
        public long CharacterSaveBytes = 0;
        public DateTime LastIssueTime = DateTime.MinValue;
        public string LastIssue = "";
        public readonly List<PingSample> PingHistory = new List<PingSample>();
        public readonly List<IssueEvent> Issues = new List<IssueEvent>();
        public int HealthScore = 100;
        public string HealthReason = "Stable";
        public double Jitter = 0;
        public double OneMinuteAverage = 0;
        public int OneMinutePeak = 0;
        public int FiveMinutePeak = 0;
    }

    public class PingSample
    {
        public DateTime Time;
        public int Ping;
    }

    public class IssueEvent
    {
        public DateTime Time;
        public string Kind = "";
        public int Count = 1;
        public int Pending = 0;
        public string Detail = "";
    }

    public class WorldState
    {
        public int FlameAltarBases = -1;
        public int WorldEntities = -1;
        public int SnapshotEntities = -1;
        public int Storages = -1;
        public int Chunks = -1;
        public long SnapshotBytes = -1;
        public long VoxelBytes = -1;
        public long WaterBytes = -1;
        public long WorldSaveBytes = -1;
        public double SaveDurationMs = -1;
        public int SaveTicks = -1;
        public DateTime LastSaveTime = DateTime.MinValue;
        public DateTime LastSnapshotTime = DateTime.MinValue;
        public int SavesObserved = 0;
        public bool PriorSaveDetected = false;
        public readonly List<DateTime> SaveTimes = new List<DateTime>();
    }

    public class EngineState
    {
        public double UpdateAverageMs = -1;
        public double UpdateMaxMs = -1;
        public long EntitiesProcessed = -1;
        public int BadPerformanceLast60 = 0;
        public double BadPerformanceLastMs = -1;
        public double BadPerformancePeakMs = -1;
        public readonly List<DateTime> BadPerformanceEvents = new List<DateTime>();
    }

    public class HostState
    {
        public bool ServerRunning;
        public int Pid;
        public TimeSpan Uptime;
        public double CpuPercent;
        public long RamBytes;
        public long TotalRamBytes;
        public double UploadMbps;
        public double DownloadMbps;
        public long NicErrors;
        public long NicDiscards;
        public string AdapterName = "";
        public string AdapterDescription = "";
        public string LocalIp = "";
        public string Gateway = "";
        public string Dns = "";
        public long LinkSpeed = 0;
        public string PublicIp = "";
        public string PublicIpv4 = "";
        public string PublicIpv6 = "";
        public string Isp = "";
        public string Asn = "";
        public string ApproxLocation = "";
        public string NetworkType = "Unknown";
        public string VpnStatus = "Not detected";
        public long DiskFreeBytes;
        public long DiskTotalBytes;
    }

    public class ActivityItem
    {
        public DateTime Time;
        public string Level = "Info";
        public string Text = "";
        public ActivityItem() { }
        public ActivityItem(DateTime time, string level, string text) { Time = time; Level = level; Text = text; }
    }

    public class PlayerSession
    {
        public string Player = "";
        public DateTime Joined;
        public DateTime Left;
        public double DurationSeconds;
        public string Source = "";
    }

    [Serializable]
    public class ModManifest
    {
        public string Id = "";
        public string Name = "";
        public string Version = "";
        public string SourcePackage = "";
        public DateTime InstalledAt;
        public bool Enabled = true;
        public string InstallTarget = "Server Root";
        public string InstallRoot = "";
        public List<ModFileEntry> Files = new List<ModFileEntry>();
        public List<string> ConfigFiles = new List<string>();
        public string Notes = "";
    }

    [Serializable]
    public class ModFileEntry
    {
        public string RelativePath = "";
        public bool HadOriginal = false;
        public string OriginalBackup = "";
        public string DisabledCopy = "";
    }

    public class AutoSaveRecord
    {
        public DateTime Time;
        public double DurationMs;
        public long WorldSaveBytes;
        public long SnapshotBytes;
        public int Bases;
        public int Entities;
        public int Chunks;
    }
}
