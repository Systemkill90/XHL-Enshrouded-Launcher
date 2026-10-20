using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace XHLLauncher
{
    public static class Storage
    {
        static readonly object settingsGate = new object();
        public static string LastSettingsNotice { get; private set; }
        static LauncherSettings ReadSettings(string path) {
            using(var fs=File.OpenRead(path))return (LauncherSettings)new XmlSerializer(typeof(LauncherSettings)).Deserialize(fs);
        }
        public static string DataDir(string baseDir)
        {
            string d = Path.Combine(baseDir, "Manager Data");
            Directory.CreateDirectory(d);
            string old = Path.Combine(baseDir,"XHL-Launcher-Data","settings.xml");
            if(!File.Exists(Path.Combine(d,"settings.xml")) && File.Exists(old)) File.Copy(old,Path.Combine(d,"settings.xml"));
            return d;
        }

        public static LauncherSettings LoadSettings(string baseDir)
        {
            string path = Path.Combine(DataDir(baseDir), "settings.xml");
            LauncherSettings s = null; LastSettingsNotice = null;
            lock(settingsGate) {
                Exception failure=null;
                if(File.Exists(path))try{s=ReadSettings(path);}catch(Exception ex){failure=ex;}
                if(s==null && File.Exists(path+".bak"))try{s=ReadSettings(path+".bak");LastSettingsNotice="The settings file could not be read. The last good settings backup was recovered. Original files were preserved.";}catch(Exception ex){failure=ex;}
                if(s==null && (failure!=null || File.Exists(path)))throw new IOException("Launcher settings and their backup could not be read. They were preserved; automatic defaults were not substituted. Restore a known good settings.xml from your launcher backup.",failure);
                if(s==null)s=new LauncherSettings();
            }
            if (string.IsNullOrEmpty(s.ServerRoot)) s.ServerRoot = DetectServerRoot(baseDir);
            if (s.ModProfiles == null) s.ModProfiles = new List<ModProfile>();
            if (s.ExternalTools == null) s.ExternalTools = new List<ExternalTool>();
            if (s.ModProfiles.Count == 0) s.ModProfiles.Add(new ModProfile { Name = "Velaris Normal" });
            s.RefreshMilliseconds=Math.Max(500,Math.Min(60000,s.RefreshMilliseconds));
            s.BackupIntervalMinutes=Math.Max(5,Math.Min(1440,s.BackupIntervalMinutes));
            return s;
        }

        public static void SaveSettings(string baseDir, LauncherSettings s)
        {
            string path = Path.Combine(DataDir(baseDir), "settings.xml");
            lock(settingsGate) {
                string temp=path+".tmp-"+Guid.NewGuid().ToString("N");
                try {
                    using(var fs=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None)) {new XmlSerializer(typeof(LauncherSettings)).Serialize(fs,s);fs.Flush(true);}
                    ReadSettings(temp);
                    if(File.Exists(path)) {
                        bool valid=true;try{ReadSettings(path);}catch{valid=false;}
                        if(valid)File.Replace(temp,path,path+".bak");
                        else {File.Move(path,path+".corrupt-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N"));File.Move(temp,path);}
                    } else File.Move(temp,path);
                } finally {if(File.Exists(temp))File.Delete(temp);}
            }
        }

        public static string DetectServerRoot(string baseDir)
        {
            if (File.Exists(Path.Combine(baseDir, "enshrouded_server.exe"))) return baseDir;
            string known = @"C:\Program Files (x86)\Steam\steamapps\common\EnshroudedServer";
            if (Directory.Exists(known)) return known;
            return baseDir;
        }

        public static List<PlayerSession> ReadSessions(string serverRoot, bool strict = false)
        {
            List<PlayerSession> result = new List<PlayerSession>();
            string[] candidates = new string[] {
                Path.Combine(serverRoot, "XHL-Diagnostic-Logs", "XHL-PLAYER-SESSIONS.csv"),
                Path.Combine(serverRoot, "XHL-PLAYER-SESSIONS.csv")
            };
            string path = null;
            foreach (string c in candidates) if (File.Exists(c)) { path = c; break; }
            if (path == null) return result;
            try
            {
                using (StreamReader sr = new StreamReader(path, true))
                {
                    string line; bool first = true;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (first) { first = false; continue; }
                        string[] f = Csv.ParseLine(line);
                        if (f.Length < 4) continue;
                        DateTime j, l; double sec;
                        if (!DateTime.TryParse(f[1], out j)) continue;
                        if (!DateTime.TryParse(f[2], out l)) continue;
                        if (!double.TryParse(f[3], NumberStyles.Any, CultureInfo.InvariantCulture, out sec)) sec = (l - j).TotalSeconds;
                        PlayerSession ps = new PlayerSession(); ps.Player = f[0]; ps.Joined = j; ps.Left = l; ps.DurationSeconds = sec; ps.Source = f.Length > 5 ? f[5] : ""; result.Add(ps);
                    }
                }
            }
            catch { if(strict) throw; }
            return result;
        }

        public static List<ActivityItem> ReadRecentPlayerEvents(string serverRoot, int count, bool strict = false)
        {
            List<ActivityItem> all = new List<ActivityItem>();
            string path = Path.Combine(serverRoot, "XHL-Diagnostic-Logs", "XHL-PLAYER-EVENTS.csv");
            if (!File.Exists(path)) return all;
            try
            {
                string[] lines = File.ReadAllLines(path);
                int start = Math.Max(1, lines.Length - count);
                for (int i = start; i < lines.Length; i++)
                {
                    string[] f = Csv.ParseLine(lines[i]);
                    DateTime dt; if (f.Length < 3 || !DateTime.TryParse(f[0], out dt)) continue;
                    string evt = f[1]; string player = f[2];
                    string text = player + (evt.Equals("JOIN", StringComparison.OrdinalIgnoreCase) ? " connected" : " disconnected");
                    all.Add(new ActivityItem(dt, "Info", text));
                }
            }
            catch { if(strict) throw; }
            return all;
        }


        public static void AppendLauncherEvent(string baseDir, string level, string message)
        {
            try
            {
                string path = Path.Combine(DataDir(baseDir), "launcher-events.log");
                string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [" + (level ?? "Info") + "] " + (message ?? "") + "\r\n";
                File.AppendAllText(path, line, Encoding.UTF8);
            }
            catch { }
        }

        public static void AppendHealth(string baseDir, PlayerState p)
        {
            try
            {
                string path = Path.Combine(DataDir(baseDir), "player-health.csv");
                if (!File.Exists(path)) File.AppendAllText(path, "Timestamp,Player,Health,Label,Ping,Avg1m,Peak1m,Jitter,Peak5m,State,Lost,Pending,LastIssue\r\n", Encoding.UTF8);
                string line = string.Join(",", new string[] {
                    Csv.Q(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), Csv.Q(p.Name), p.HealthScore.ToString(CultureInfo.InvariantCulture), Csv.Q(Theme.HealthLabel(p.HealthScore)),
                    p.PingMs.ToString(CultureInfo.InvariantCulture), p.OneMinuteAverage.ToString("0.0", CultureInfo.InvariantCulture), p.OneMinutePeak.ToString(CultureInfo.InvariantCulture),
                    p.Jitter.ToString("0.0", CultureInfo.InvariantCulture), p.FiveMinutePeak.ToString(CultureInfo.InvariantCulture), Csv.Q(p.SessionState), p.LostTotal.ToString(CultureInfo.InvariantCulture), p.PendingPackets.ToString(CultureInfo.InvariantCulture), Csv.Q(p.LastIssue)
                });
                File.AppendAllText(path, line + "\r\n", Encoding.UTF8);
            }
            catch { }
        }

        public static void AppendAutoSave(string baseDir, AutoSaveRecord r)
        {
            try
            {
                string path = Path.Combine(DataDir(baseDir), "autosave-history.csv");
                if (!File.Exists(path)) File.AppendAllText(path, "Timestamp,DurationMs,WorldSaveBytes,SnapshotBytes,Bases,Entities,Chunks\r\n", Encoding.UTF8);
                string line = string.Join(",", new string[] {
                    Csv.Q(r.Time.ToString("yyyy-MM-dd HH:mm:ss")), r.DurationMs.ToString("0.00", CultureInfo.InvariantCulture), r.WorldSaveBytes.ToString(CultureInfo.InvariantCulture), r.SnapshotBytes.ToString(CultureInfo.InvariantCulture), r.Bases.ToString(CultureInfo.InvariantCulture), r.Entities.ToString(CultureInfo.InvariantCulture), r.Chunks.ToString(CultureInfo.InvariantCulture)
                });
                File.AppendAllText(path, line + "\r\n", Encoding.UTF8);
            }
            catch { }
        }

        public static List<AutoSaveRecord> ReadAutoSaves(string baseDir)
        {
            List<AutoSaveRecord> list = new List<AutoSaveRecord>();
            string path = Path.Combine(DataDir(baseDir), "autosave-history.csv");
            if (!File.Exists(path)) return list;
            try
            {
                string[] lines = File.ReadAllLines(path);
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] f = Csv.ParseLine(lines[i]); if (f.Length < 7) continue;
                    DateTime dt; if (!DateTime.TryParse(f[0], out dt)) continue;
                    AutoSaveRecord r = new AutoSaveRecord(); r.Time = dt;
                    double.TryParse(f[1], NumberStyles.Any, CultureInfo.InvariantCulture, out r.DurationMs);
                    long.TryParse(f[2], out r.WorldSaveBytes); long.TryParse(f[3], out r.SnapshotBytes); int.TryParse(f[4], out r.Bases); int.TryParse(f[5], out r.Entities); int.TryParse(f[6], out r.Chunks); list.Add(r);
                }
            }
            catch { }
            return list;
        }
    }

    public static class Csv
    {
        public static string Q(string s) { return "\"" + (s ?? "").Replace("\"", "\"\"") + "\""; }
        public static string[] ParseLine(string line)
        {
            List<string> f = new List<string>(); StringBuilder sb = new StringBuilder(); bool q = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"') { if (q && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; } else q = !q; }
                else if (c == ',' && !q) { f.Add(sb.ToString()); sb.Length = 0; }
                else sb.Append(c);
            }
            f.Add(sb.ToString()); return f.ToArray();
        }
    }

    public static class FormatUtil
    {
        public static string Bytes(long bytes)
        {
            if (bytes < 0) return "n/a";
            string[] u = new string[] { "B", "KiB", "MiB", "GiB", "TiB" }; double v = bytes; int i = 0;
            while (v >= 1024 && i < u.Length - 1) { v /= 1024; i++; }
            return v.ToString(i >= 3 ? "0.0" : "0.##") + " " + u[i];
        }
        public static string Duration(TimeSpan t)
        {
            if (t.TotalDays >= 1) return ((int)t.TotalDays) + "d " + t.Hours + "h";
            if (t.TotalHours >= 1) return ((int)t.TotalHours) + "h " + t.Minutes + "m";
            if (t.TotalMinutes >= 1) return ((int)t.TotalMinutes) + "m " + t.Seconds + "s";
            return Math.Max(0, (int)t.TotalSeconds) + "s";
        }
        public static string Compact(int n)
        {
            if (n < 0) return "n/a";
            if (n >= 1000000) return (n / 1000000.0).ToString("0.0") + "M";
            if (n >= 1000) return (n / 1000.0).ToString("0.#") + "K";
            return n.ToString();
        }
    }
}
