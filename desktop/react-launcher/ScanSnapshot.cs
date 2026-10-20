using System;using System.IO;using System.Linq;using System.Collections.Generic;
namespace XHLLauncher {
 internal sealed class WorldFileRow {public string name,size,written;}
 internal sealed class WorldFiles {public bool available;public int count;public string size,lastWrite,error;public WorldFileRow[] items;}
 internal sealed class BackupRow {public string id,date,kind;public DateTime sort;public bool offline;}
 internal sealed class ModRow {public string id,name,version,kind,detail;public bool builtIn,locked,configured;}
 internal sealed class ConfigRow {public string path,key,label,section,reason,issue;public object value;public bool editable;public decimal? min,max;public List<ConfigurationDocument.Choice> choices;}
 internal sealed class ScanSnapshot {
  public DateTime AttemptUtc,SuccessUtc;public string Error="";
  internal string phoneFiles="Unknown",phoneSize="Unknown",phoneWritten="Unknown",latestBackupText="Unknown";
  internal string[] historicalNames=new string[0];internal PlayerSession[] profileSessions=new PlayerSession[0];internal ActivityItem[] profileEvents=new ActivityItem[0];
  internal PlayerHistoryRecord[] playerHistory=new PlayerHistoryRecord[0];
  internal WorldFiles files=new WorldFiles{available=false,error="Waiting for scan",items=new WorldFileRow[0]};internal BackupRow[] backups=new BackupRow[0];internal ConfigRow[] config=new ConfigRow[0];internal ModRow[] mods=new ModRow[0];
  internal static ScanSnapshot Collect(string App,LauncherSettings Settings,string Backups){var result=new ScanSnapshot();result.Load(App,Settings,Backups);return result;}
  void Load(string App,LauncherSettings Settings,string Backups){
   try{historicalNames=RealmHistory.Load(App,Settings.ServerRoot,true).KnownPlayers;profileSessions=Storage.ReadSessions(Settings.ServerRoot,true).ToArray();profileEvents=Storage.ReadRecentPlayerEvents(Settings.ServerRoot,250,true).ToArray();}catch(Exception ex){throw new IOException("Player history scan failed",ex);}
   try{var list=RealmHistory.WorldFiles(Settings.ServerRoot,true);phoneFiles=string.Join(", ",list.Select(f=>RealmHistory.RelativeName(Settings.ServerRoot,f)).ToArray());phoneSize=FormatUtil.Bytes(list.Sum(f=>f.Length));phoneWritten=list.Length>0?list.Max(f=>f.LastWriteTime).ToString("MMM d, h:mm tt"):"Unknown";files=new WorldFiles{available=true,count=list.Length,size=FormatUtil.Bytes(list.Sum(f=>f.Length)),lastWrite=list.Length>0?list.Max(f=>f.LastWriteTime).ToString("MMM d, h:mm tt"):"Unknown",items=list.Select(f=>new WorldFileRow{name=RealmHistory.RelativeName(Settings.ServerRoot,f),size=FormatUtil.Bytes(f.Length),written=f.LastWriteTime.ToString("MMM d, h:mm tt")}).ToArray()};}catch(Exception ex){throw new IOException("World scan failed",ex);}
   try{backups=BackupDirectories(Backups).Select(d=>new BackupRow{id=d.Name,date=BackupFacts.When(d.FullName),kind=BackupFacts.Describe(d.FullName),sort=BackupFacts.Created(d.FullName),offline=BackupFacts.Describe(d.FullName).StartsWith("Offline")}).OrderByDescending(b=>b.sort).Take(200).ToArray();}catch(Exception ex){throw new IOException("Backup scan failed",ex);}
   latestBackupText=ScanLatestBackup(Backups);
   try{config=ConfigurationDocument.Flatten(Maintenance.Config(Settings.ServerRoot)).Select(f=>{decimal min,max;bool bounds=ConfigurationDocument.Bounds(f.Key,out min,out max);bool secret=f.Key.IndexOf("password",StringComparison.OrdinalIgnoreCase)>=0||f.Path.StartsWith("bannedAccounts");bool minutes=f.Key=="dayTimeDuration"||f.Key=="nightTimeDuration"||f.Key=="fromHungerToStarving";object value=secret?"Hidden":minutes?(object)(Convert.ToDecimal(f.Value)/60000000000m):f.Value;return new ConfigRow{path=f.Path,key=f.Key,label=f.Label,section=ConfigurationDocument.GroupFor(f),value=value,editable=!secret&&f.Set!=null&&ConfigurationDocument.ReadOnlyReason(f)==null,reason=secret?"Manage passwords and account lists in classic tools":ConfigurationDocument.ReadOnlyReason(f),issue=ConfigurationDocument.FieldIssue(f),min=bounds?(decimal?)min:null,max=bounds?(decimal?)max:null,choices=ConfigurationDocument.Choices(f.Key,"")};}).ToArray();}catch(Exception ex){throw new IOException("Configuration scan failed",ex);}
   try{mods=ModManagement.List(Settings.ServerRoot,App).Select(m=>new ModRow{id=m.Id,name=m.Name,version=m.Version,kind=m.Kind,builtIn=m.BuiltIn,locked=m.Locked,configured=m.Enabled,detail=m.Support}).ToArray();}catch(Exception ex){throw new IOException("Mod scan failed",ex);}

  }
  static DirectoryInfo[] BackupDirectories(string path){try{return new DirectoryInfo(path).GetDirectories();}catch(DirectoryNotFoundException){return new DirectoryInfo[0];}}
  static string ScanLatestBackup(string Backups){var list=BackupDirectories(Backups).Where(d=>File.Exists(Path.Combine(d.FullName,"VERIFIED.txt"))).OrderByDescending(d=>BackupFacts.Created(d.FullName)).ToArray();return list.Length>0?BackupFacts.When(list[0].FullName)+" (recorded completion)":"No completed copy recorded";}

  internal static ScanSnapshot Attempt(ScanSnapshot previous,Func<ScanSnapshot> collect){
   DateTime now=DateTime.UtcNow;try{var next=collect();next.AttemptUtc=now;next.SuccessUtc=DateTime.UtcNow;return next;}
   catch(Exception ex){var failed=(ScanSnapshot)previous.MemberwiseClone();failed.AttemptUtc=now;failed.Error=ex.Message+": "+(ex.InnerException==null?"":ex.InnerException.Message);return failed;}
  }
 }
}
