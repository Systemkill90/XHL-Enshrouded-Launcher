using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace XHLLauncher {
 public sealed class RealmRecord {
  public string ServerRoot="";public DateTime Observed=DateTime.MinValue;public int Bases=-1,Entities=-1,Storages=-1,Chunks=-1;public string[] KnownPlayers=new string[0];
 }
 public static class RealmHistory {
  static string FilePath(string app,string root){string id;using(var sha=System.Security.Cryptography.SHA256.Create())id=BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Path.GetFullPath(root).ToLowerInvariant()))).Replace("-","").Substring(0,16);return Path.Combine(Storage.DataDir(app),"realm-history-"+id+".json");}
  public static RealmRecord Load(string app,string root){RealmRecord record=null;try{record=new JavaScriptSerializer().Deserialize<RealmRecord>(File.ReadAllText(FilePath(app,root)));if(record!=null&&!string.Equals(Path.GetFullPath(record.ServerRoot),Path.GetFullPath(root),StringComparison.OrdinalIgnoreCase))record=null;}catch{}if(record==null)record=new RealmRecord{ServerRoot=root};try{var cfg=ConfigReader.Read(root);string logs=Path.GetFullPath(Path.Combine(root,cfg.LogDirectory));var paths=new List<string>();string active=Path.Combine(logs,"enshrouded_server.log");if(File.Exists(active))paths.Add(active);string backups=Path.Combine(logs,"backup");if(Directory.Exists(backups))paths.AddRange(new DirectoryInfo(backups).GetFiles("*.log").OrderByDescending(f=>f.LastWriteTimeUtc).Take(4).Select(f=>f.FullName));var names=new HashSet<string>(record.KnownPlayers,StringComparer.OrdinalIgnoreCase);foreach(string path in paths){using(var input=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete)){long start=Math.Max(0,input.Length-4*1024*1024);input.Seek(start,SeekOrigin.Begin);using(var reader=new StreamReader(input)){if(start>0)reader.ReadLine();string text=reader.ReadToEnd();foreach(System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(text,@"\[server\]\s+Player\s+'(.+?)'\s+logged in"))names.Add(m.Groups[1].Value);}}}record.KnownPlayers=names.ToArray();}catch{}return record;}
  public static void Save(string app,RealmRecord record){string path=FilePath(app,record.ServerRoot),temp=path+".pending";File.WriteAllText(temp,new JavaScriptSerializer().Serialize(record));if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);}
  public static FileInfo[] WorldFiles(string root){var cfg=ConfigReader.Read(root);string saves=Path.GetFullPath(Path.Combine(root,cfg.SaveDirectory));if(!Directory.Exists(saves))return new FileInfo[0];return new DirectoryInfo(saves).GetFiles().Where(f=>(f.Attributes&FileAttributes.ReparsePoint)==0).OrderByDescending(f=>f.LastWriteTimeUtc).ToArray();}
 }
 public sealed partial class ControlRoom {
  RealmRecord lastRealm;DateTime nextRealmRead=DateTime.MinValue,nextRealmWrite=DateTime.MinValue;DataGridView worldFilesGrid;Label worldSource,modStatus;FileInfo[] savedWorldFiles=new FileInfo[0];
  Control BuildRichWorld(){var page=Page("World & saved data","Current world files and the last observed world statistics remain visible when the server is offline.");
   var card=Card(page,"SAVED WORLD",0,78,870,222);worldDetail=Body(card,"Reading saved world",44,11,Theme.Text);
   worldSource=TextLabel("",9,Theme.Muted,false);worldSource.SetBounds(0,316,870,48);worldSource.AutoSize=false;page.Controls.Add(worldSource);
   worldFilesGrid=new DataGridView{Location=new Point(0,380),Size=new Size(870,220),ReadOnly=true};Theme.StyleGrid(worldFilesGrid);foreach(var c in new[]{"Save file","Size","Last written"})worldFilesGrid.Columns.Add(c,c);page.Controls.Add(worldFilesGrid);
   ActionButton(page,"World backups",0,620,165,delegate{Navigate("Backups");});ActionButton(page,"Server rules",180,620,165,delegate{Navigate("Settings");});
   page.Resize+=delegate{int w=Math.Max(400,page.ClientSize.Width-20);card.Width=worldSource.Width=worldFilesGrid.Width=w;};return page;
  }
  Control BuildRichMods(){var page=Page("Installed mods","The installed profile and last startup evidence from this exact server.");
   var c=Card(page,"CURRENT PROFILE",0,78,870,345);modStatus=Body(c,"Reading installed mod state",46,11,Theme.Text);
   ActionButton(page,"Refresh checks",0,448,170,async delegate{await InspectAsync();UpdateRichState();});ActionButton(page,"Flight rules",185,448,150,delegate{Navigate("Settings");});
   var label=TextLabel("Flight holds the last pitch with ±80° bounds. Speed and stamina are unchanged.\r\nStacking uses the verified 65,535 item profile; gifting uses the pinned distance getter.",10,Theme.Muted,false);label.SetBounds(0,508,850,82);label.AutoSize=false;page.Controls.Add(label);
   page.Resize+=delegate{c.Width=Math.Max(450,page.ClientSize.Width-20);};return page;
  }
  void UpdateRichState(){
   var h=telemetry.GetHost();var w=telemetry.GetWorld();if(lastRealm==null)lastRealm=RealmHistory.Load(appBase,settings.ServerRoot);
   if(DateTime.Now>=nextRealmRead){nextRealmRead=DateTime.Now.AddSeconds(5);try{savedWorldFiles=RealmHistory.WorldFiles(settings.ServerRoot);}catch{savedWorldFiles=new FileInfo[0];}}
   var known=telemetry.GetAllKnownPlayers().Where(p=>!string.IsNullOrWhiteSpace(p.Name)&&!p.Name.StartsWith("Player ")).ToArray();
   if(h.ServerRunning&&DateTime.Now>=nextRealmWrite){nextRealmWrite=DateTime.Now.AddSeconds(30);if(w.FlameAltarBases>=0)lastRealm.Bases=w.FlameAltarBases;if(w.WorldEntities>=0)lastRealm.Entities=w.WorldEntities;if(w.Storages>=0)lastRealm.Storages=w.Storages;if(w.Chunks>=0)lastRealm.Chunks=w.Chunks;lastRealm.KnownPlayers=lastRealm.KnownPlayers.Concat(known.Select(p=>p.Name)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();lastRealm.Observed=DateTime.Now;try{RealmHistory.Save(appBase,lastRealm);}catch{}}
   long size=savedWorldFiles.Sum(f=>f.Length);DateTime saved=savedWorldFiles.Length>0?savedWorldFiles.Max(f=>f.LastWriteTime):DateTime.MinValue;
   if(savedWorldFiles.Length>0)saveSummary.Text="Last file write  "+saved.ToString("MMM d, h:mm tt")+"\r\n"+savedWorldFiles.Length+" active save file(s)  /  "+FormatUtil.Bytes(size);
   int bases=w.FlameAltarBases>=0?w.FlameAltarBases:lastRealm.Bases,entities=w.WorldEntities>=0?w.WorldEntities:lastRealm.Entities,storages=w.Storages>=0?w.Storages:lastRealm.Storages,chunks=w.Chunks>=0?w.Chunks:lastRealm.Chunks;
   if(worldDetail!=null){string extras="";if(bases>=0)extras+="Bases "+ShowCount(bases)+"   ";if(entities>=0)extras+="Entities "+ShowCount(entities)+"   ";if(storages>=0)extras+="Storages "+ShowCount(storages)+"   ";if(chunks>=0)extras+="Chunks "+ShowCount(chunks);string backups="";try{var copies=new DirectoryInfo(BackupRoot).GetDirectories().Where(d=>File.Exists(Path.Combine(d.FullName,"VERIFIED.txt"))).OrderByDescending(d=>d.CreationTimeUtc).ToArray();backups="Verified backups   "+copies.Length+(copies.Length>0?"   /   Latest "+copies[0].CreationTime.ToString("MMM d, h:mm tt"):"");}catch{backups="Backup history unavailable";}worldDetail.Text="World files   "+(savedWorldFiles.Length>0?string.Join(", ",savedWorldFiles.Select(f=>f.Name).ToArray()):"No active files found in configured save directory")+"\r\nSaved size   "+FormatUtil.Bytes(size)+"\r\nLast file write   "+(saved==DateTime.MinValue?"Unavailable":saved.ToString("MMM d, yyyy h:mm tt"))+"\r\n"+backups+"\r\nKnown players   "+string.Join(", ",lastRealm.KnownPlayers)+ (extras.Length>0?"\r\n"+extras:"");}
   if(worldSource!=null)worldSource.Text=(h.ServerRunning?"Server process online. ":"OFFLINE — showing saved files and previous observations. ")+"Statistics from server logs / cached observations"+(lastRealm.Observed==DateTime.MinValue?".":" as of "+lastRealm.Observed.ToString("MMM d, h:mm tt")+".");
   if(current=="World"&&worldFilesGrid!=null){worldFilesGrid.Rows.Clear();foreach(var f in savedWorldFiles)worldFilesGrid.Rows.Add(f.Name,FormatUtil.Bytes(f.Length),f.LastWriteTime.ToString("MMM d, yyyy h:mm tt"));}
   if(current=="Players"&&playerGrid!=null){foreach(string name in lastRealm.KnownPlayers)if(!known.Any(p=>p.Name.Equals(name,StringComparison.OrdinalIgnoreCase)))playerGrid.Rows.Add(name,"—","—","Previous session","Cached player history");foreach(var session in Storage.ReadSessions(settings.ServerRoot).GroupBy(s=>s.Player).Select(g=>g.Last()))if(!playerGrid.Rows.Cast<DataGridViewRow>().Any(r=>Convert.ToString(r.Cells[0].Value)==session.Player))playerGrid.Rows.Add(session.Player,"—","—",session.Joined.ToString("MMM d, h:mm tt"),"Recorded session history");}
   if(modStatus!=null){string log=StartupEvidence();string runtime=h.ServerRunning&&audit!=null&&audit.CanStart?RuntimeModStatus.Read(h.Pid):"Server offline — installed profile will be applied at next safe start.";modStatus.Text="STACKING   "+(audit!=null&&audit.StackMatches?"Verified installed resources / 65,535":"Needs file verification")+"\r\n\r\n"+runtime+"\r\n\r\nLAST STARTUP EVIDENCE\r\n"+log;}
  }
  string StartupEvidence(){try{string root=settings.ServerRoot;string local=Path.Combine(root,"XHL-1.0-LAST-START.txt");string profile=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"XHL","XHL-1.0-LAST-START.txt");string path=File.Exists(local)?local:profile;if(File.Exists(path)){var lines=File.ReadAllLines(path).Where(l=>l.Contains("ACTIVE")||l.Contains("NOT APPLIED")||l.Contains("FAILED")||l.Contains("alive at"));return string.Join("\r\n",lines.TakeLastCompat(5).ToArray());}}catch{}return "No matching startup record found. Disk verification is shown above; runtime status remains separate.";}
  static string ShowCount(int n){return n<0?"Not recorded":n.ToString("N0");}
 }
 internal static class SequenceHelper{internal static IEnumerable<T> TakeLastCompat<T>(this IEnumerable<T> source,int n){var queue=new Queue<T>();foreach(var item in source){queue.Enqueue(item);if(queue.Count>n)queue.Dequeue();}return queue;}}
 internal static class RuntimeModStatus {
  [System.Runtime.InteropServices.DllImport("kernel32.dll")]static extern IntPtr OpenProcess(uint a,bool inherit,int pid);
  [System.Runtime.InteropServices.DllImport("kernel32.dll")]static extern bool CloseHandle(IntPtr h);
  [System.Runtime.InteropServices.DllImport("kernel32.dll")]static extern bool ReadProcessMemory(IntPtr h,IntPtr a,byte[] b,UIntPtr n,out UIntPtr read);
  static string State(IntPtr handle,long address,string expected){byte[] bytes=new byte[expected.Length/2];UIntPtr read;if(!ReadProcessMemory(handle,new IntPtr(address),bytes,(UIntPtr)(uint)bytes.Length,out read)||read.ToUInt64()!=(ulong)bytes.Length)return "Runtime unavailable";return BitConverter.ToString(bytes).Replace("-","")==expected?"Active — memory verified":"Different memory state; not claimed active";}
  internal static string Read(int pid){IntPtr handle=IntPtr.Zero;try{using(var process=System.Diagnostics.Process.GetProcessById(pid)){long b=process.MainModule.BaseAddress.ToInt64();handle=OpenProcess(0x1010,false,pid);if(handle==IntPtr.Zero)return "Runtime memory cannot be read.";return "GIFTING   "+State(handle,b+0x931830,"F30F1005809C2900C3")+"\r\nFLIGHT RANGE   "+State(handle,b+0x7cf62,"D6B9B500")+"\r\nPITCH HOLD   "+State(handle,b+0x7cd8b,"EB0E909090");}}catch{return "Runtime observation unavailable.";}finally{if(handle!=IntPtr.Zero)CloseHandle(handle);}}
 }
}
