using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace XHLLauncher {
 internal static class Maintenance {
  internal static Dictionary<string,object> Config(string root){return new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(root,"enshrouded_server.json")));}
  static bool AnyServer(){var all=Process.GetProcessesByName("enshrouded_server");try{return all.Length!=0;}finally{foreach(var p in all)p.Dispose();}}
  static string Inside(string root,string relative){string r=Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;string f=Path.GetFullPath(Path.Combine(r,relative));if(!f.StartsWith(r,StringComparison.OrdinalIgnoreCase))throw new IOException("Path escapes the selected folder.");return f;}
  static void NoLinks(string path){for(var d=new DirectoryInfo(path);d!=null;d=d.Parent)if(d.Exists&&(d.Attributes&FileAttributes.ReparsePoint)!=0)throw new IOException("Linked folders are not supported.");}
  static void CheckSaveTree(string root){foreach(string entry in Directory.GetFileSystemEntries(root)){var a=File.GetAttributes(entry);if((a&FileAttributes.ReparsePoint)!=0)throw new IOException("Linked save entries are not supported.");if((a&FileAttributes.Directory)!=0)CheckSaveTree(entry);}}
  internal static ActionResult SaveSettings(string root,string name,int slots,int port,Func<bool> running=null){
   try{if((running??AnyServer)())return ActionResult.Fail("Stop the server before editing settings.");if(string.IsNullOrWhiteSpace(name)||name.Trim().Length>64||name.Any(char.IsControl)||slots<1||slots>16||port<1024||port>65535)return ActionResult.Fail("Use a name of 1–64 characters, 1–16 slots, and a query port of 1024–65535.");
    NoLinks(root);string path=Path.Combine(root,"enshrouded_server.json");var data=Config(root);data["name"]=name.Trim();data["slotCount"]=slots;data["queryPort"]=port;
    string temp=path+".pending-"+Guid.NewGuid().ToString("N"),backup=path+".before-settings-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");File.WriteAllText(temp,new JavaScriptSerializer().Serialize(data));ConfigCheck(temp);if((running??AnyServer)())return ActionResult.Fail("Server started; pending settings were not activated.");File.Replace(temp,path,backup);return ActionResult.Ok("Settings saved. Previous configuration retained. Changes apply at the next server start.");
   }catch(Exception ex){return ActionResult.Fail("Settings unchanged: "+ex.Message);}
  }
  static void ConfigCheck(string path){if(new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(path))==null)throw new IOException("Invalid configuration.");}
  internal static ActionResult SaveConfiguration(string root,string text,string expectedHash,Func<bool> running=null){
   try{
    if((running??AnyServer)())return ActionResult.Fail("Stop the server before saving rules. Unsaved edits remain in the editor.");
    NoLinks(root);string path=Path.Combine(root,"enshrouded_server.json");if(InstallationAudit.Hash(path)!=expectedHash)return ActionResult.Fail("The configuration changed on disk. Reload it before saving to avoid overwriting newer settings.");
    var js=new JavaScriptSerializer();var old=Config(root);var data=js.Deserialize<Dictionary<string,object>>(text);if(data==null)throw new InvalidDataException("Expected a JSON configuration object.");
    foreach(string key in new[]{"saveDirectory","logDirectory"}){object before,after;if(old.TryGetValue(key,out before)&&(!data.TryGetValue(key,out after)||Convert.ToString(before)!=Convert.ToString(after)))throw new InvalidDataException(key+" is protected to preserve the existing world and log locations.");}
    object val;if(!data.TryGetValue("name",out val)||!(val is string)||string.IsNullOrWhiteSpace((string)val)||((string)val).Length>64)throw new InvalidDataException("Server name must be 1–64 characters.");
    if(!data.TryGetValue("slotCount",out val)||Convert.ToInt32(val)<1||Convert.ToInt32(val)>16)throw new InvalidDataException("Player slots must be 1–16.");
    if(!data.TryGetValue("queryPort",out val)||Convert.ToInt32(val)<1024||Convert.ToInt32(val)>65535)throw new InvalidDataException("Query port must be 1024–65535.");
    ConfigurationDocument.ValidateCompatibility(data,old);
     var originalFields=ConfigurationDocument.Flatten(old);var newFields=ConfigurationDocument.Flatten(data);
    foreach(var f in originalFields.Where(f=>f.Set!=null)){var replacement=newFields.FirstOrDefault(x=>x.Path==f.Path);if(replacement==null)continue;if(f.Value is bool&&!(replacement.Value is bool))throw new InvalidDataException(f.Label+" must be true or false.");if(f.Value is string&&!(replacement.Value is string))throw new InvalidDataException(f.Label+" must be text.");if(f.Value is ValueType&&!(f.Value is bool)){double number;if(!double.TryParse(Convert.ToString(replacement.Value,System.Globalization.CultureInfo.InvariantCulture),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out number)||double.IsInfinity(number)||double.IsNaN(number))throw new InvalidDataException(f.Label+" must be a finite number.");}}
    if(data.ContainsKey("gameSettings")&&old.ContainsKey("gameSettings")&&js.Serialize(data["gameSettings"])!=js.Serialize(old["gameSettings"]))data["gameSettingsPreset"]="Custom";
    string temp=path+".pending-"+Guid.NewGuid().ToString("N"),backup=path+".before-rules-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");File.WriteAllText(temp,JsonFormatting.Pretty(js.Serialize(data)));ConfigCheck(temp);
    if((running??AnyServer)()||InstallationAudit.Hash(path)!=expectedHash)return ActionResult.Fail("Server or config changed while preparing. Pending file retained; active configuration unchanged.");
    File.Replace(temp,path,backup);return ActionResult.Ok("Server configuration saved. Previous configuration retained at "+backup);
   }catch(Exception ex){return ActionResult.Fail("Configuration was not activated: "+ex.Message);}
  }
  internal static ActionResult Archive(string root,string backups,Func<bool> running=null){
   var moved=new List<KeyValuePair<string,string>>();string journal=null;
   try{
    if((running??AnyServer)())return ActionResult.Fail("Stop the server before organizing archives.");var plan=MigrationPlanner.Create(root);if(!plan.Complete)return ActionResult.Fail("A complete link-free inventory is required.");
    var items=plan.Proposals.Where(p=>!p.Blocked&&p.Fingerprint.Length==64).ToArray();if(items.Length==0)return ActionResult.Ok("No verified historical launcher archives to move. Runtime and unknown files remain in place.");
    var backup=SaveBackup.Create(root,backups,false);if(!backup.Success)return backup;
    string destination=Inside(root,Path.Combine("Manager","Archive"));NoLinks(destination);Directory.CreateDirectory(destination);journal=Path.Combine(destination,"organization-"+Guid.NewGuid().ToString("N")+".txt");File.WriteAllText(journal,"Journal created before moves. Files remain recoverable at their recorded destinations.\r\n");
    foreach(var item in items){string source=Inside(root,item.Source),target=Inside(destination,Path.GetFileName(item.Source));if((running??AnyServer)())throw new IOException("Server started; organization cancelled.");if(File.Exists(target)||InstallationAudit.Hash(source)!=item.Fingerprint)throw new IOException("Archive changed or destination exists.");File.AppendAllText(journal,source+" => "+target+"\r\n");File.Move(source,target);moved.Add(new KeyValuePair<string,string>(source,target));if(InstallationAudit.Hash(target)!=item.Fingerprint)throw new IOException("Archive verification failed.");}
    File.AppendAllText(journal,"COMPLETE\r\n");return ActionResult.Ok(moved.Count+" historical archives organized into Manager / Archive. No game files, mods or saves moved. Journal: "+journal);
   }catch(Exception ex){string errors="";foreach(var item in moved.AsEnumerable().Reverse())try{if(!File.Exists(item.Key))File.Move(item.Value,item.Key);}catch(Exception rollback){errors+=" Rollback requires review: "+rollback.Message;}return ActionResult.Fail("Organization stopped: "+ex.Message+errors+(journal==null?"":" Journal: "+journal));}
  }
  internal static ActionResult Restore(string root,string backups,string snapshot,Func<bool> running=null){
   string staged=null,retained=null,saves=null;bool moved=false;
   try{
    if((running??AnyServer)())return ActionResult.Fail("Stop the server before restoring a world.");
    string normalized=Path.GetFullPath(snapshot);string allowed=Path.GetFullPath(backups).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;if(!normalized.StartsWith(allowed,StringComparison.OrdinalIgnoreCase))throw new IOException("Snapshot is outside the selected backup folder.");NoLinks(normalized);NoLinks(root);
    if(!File.Exists(Path.Combine(snapshot,"VERIFIED.txt")))throw new IOException("Backup completion marker is missing.");
    var manifest=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(snapshot,"manifest.json")));object live;if(!manifest.TryGetValue("ServerObservedRunning",out live)||!(live is bool)||(bool)live)throw new IOException("Automatic restore requires a verified OFFLINE snapshot. Live copies need separate recovery review.");
    var entries=manifest["Files"] as Dictionary<string,object>;if(entries==null||entries.Count==0)throw new IOException("Backup has no file manifest.");
    var config=Config(root);saves=Inside(root,Convert.ToString(config["saveDirectory"]));NoLinks(saves);if(!Directory.Exists(saves))throw new IOException("Current saves are missing; no automatic restore.");
    CheckSaveTree(saves);
    foreach(var e in entries){string file=Inside(Path.Combine(snapshot,"saves"),e.Key);NoLinks(Path.GetDirectoryName(file));if((File.GetAttributes(file)&FileAttributes.ReparsePoint)!=0||InstallationAudit.Hash(file)!=Convert.ToString(e.Value))throw new IOException("Backup hash mismatch: "+e.Key);}
    var safety=SaveBackup.Create(root,backups,false);if(!safety.Success)return safety;
    string tag=DateTime.Now.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N").Substring(0,6);staged=saves+".restore-"+tag;retained=saves+".before-restore-"+tag;Directory.CreateDirectory(staged);
    foreach(var e in entries){string target=Inside(staged,e.Key);Directory.CreateDirectory(Path.GetDirectoryName(target));File.Copy(Inside(Path.Combine(snapshot,"saves"),e.Key),target,false);if(InstallationAudit.Hash(target)!=Convert.ToString(e.Value))throw new IOException("Restore copy failed verification.");}
    if((running??AnyServer)())throw new IOException("Server started during preparation; restore cancelled.");
    Directory.Move(saves,retained);moved=true;Directory.Move(staged,saves);moved=false;
    return ActionResult.Ok("World restored and hash verified. Previous world retained at "+retained+". Current configuration is unchanged.");
   }catch(Exception ex){if(moved&&retained!=null&&saves!=null&&!Directory.Exists(saves)){try{Directory.Move(retained,saves);}catch(Exception rollback){return ActionResult.Fail("Recovery required: "+rollback.Message+". Previous world: "+retained);}}return ActionResult.Fail("Restore refused: "+ex.Message+". Current world retained; prepared copies are kept.");}
  }
 }
}
