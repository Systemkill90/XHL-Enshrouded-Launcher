using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
namespace XHLLauncher {
 internal sealed class ManagedMod {
  internal string Id,Name,Version,Kind,Support,Directory;
  internal bool BuiltIn,Locked,Enabled;
 }
 internal static class ModManagement {
  static readonly Dictionary<string,string> Flags=new Dictionary<string,string>{{"flight","Flight"},{"gifting","Gifting"},{"flight-stamina","FlightStamina"}};
  static readonly string[] ConfigExtensions={".json",".txt",".ini",".toml",".cfg",".yaml",".yml"};
  internal static string Library(string app){return Path.Combine(Storage.DataDir(app),"Mod Library");}
  internal static string Archive(string app){return Path.Combine(Storage.DataDir(app),"Removed Mods");}
  static string Flag(string server,string id){if(!Flags.ContainsKey(id))throw new IOException("This mod has no supported startup switch.");return Path.Combine(server,"XHL-Mods-1.0",Flags[id],"disabled.flag");}
  internal static bool Enabled(string server,string id){return id=="stacking"||!File.Exists(Flag(server,id));}
  static void NoLinks(string path){for(var d=new DirectoryInfo(path);d!=null;d=d.Parent)if(d.Exists&&(d.Attributes&FileAttributes.ReparsePoint)!=0)throw new IOException("Linked folders are not supported.");if(File.Exists(path)&&(File.GetAttributes(path)&FileAttributes.ReparsePoint)!=0)throw new IOException("Linked files are not supported.");}
  static string Inside(string root,string relative){string prefix=Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;string target=Path.GetFullPath(Path.Combine(prefix,relative));if(!target.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new IOException("Mod path leaves its managed folder.");return target;}
  static bool Running(){var processes=Process.GetProcessesByName("enshrouded_server");try{return processes.Length>0;}finally{foreach(var p in processes)p.Dispose();}}
  static List<string> Tree(string root){NoLinks(root);var result=new List<string>();foreach(string path in System.IO.Directory.GetFileSystemEntries(root)){NoLinks(path);if(System.IO.Directory.Exists(path))result.AddRange(Tree(path));else result.Add(path);if(result.Count>2000)throw new IOException("Package exceeds 2,000 files.");}return result;}
  static string Value(Dictionary<string,object> data,string key,string fallback){object value;return data.TryGetValue(key,out value)&&value is string?(string)value:fallback;}
  internal static List<ManagedMod> List(string server,string app){
   var result=new List<ManagedMod>{
    new ManagedMod{Id="stacking",Name="65,535 stacking",Version="Pinned 1269579",Kind="Installed resources",BuiltIn=true,Locked=true,Enabled=true,Support="Required for this world's existing stacks. Changing config.txt does not rebuild resources; removal is blocked."},
    new ManagedMod{Id="flight",Name="Flight + pitch hold",Version="Pinned 1269579",Kind="Startup controller",BuiltIn=true,Enabled=Enabled(server,"flight"),Support="Enable or remove from the next startup. Flight and pitch hold are one controller. Fixed −80° / +80° bounds; no tested speed slider."},
    new ManagedMod{Id="gifting",Name="Unlimited gifting range",Version="Pinned 1269579",Kind="Startup controller",BuiltIn=true,Enabled=Enabled(server,"gifting"),Support="Enable or remove from the next startup. Fixed distance patch; no supported numeric range control."},
    new ManagedMod{Id="flight-stamina",Name="Flight-only zero stamina drain",Version="Pinned 1269579",Kind="Startup controller",BuiltIn=true,Enabled=Enabled(server,"flight-stamina"),Support="Enable or remove from the next startup. Requires Flight enabled. Other stamina systems stay unchanged."}
   };
   string library=Library(app);NoLinks(library);if(System.IO.Directory.Exists(library))foreach(string folder in System.IO.Directory.GetDirectories(library)){
    try{NoLinks(folder);var data=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(folder,"mod.json")));result.Add(new ManagedMod{Id="library:"+Path.GetFileName(folder),Name=Value(data,"name",Path.GetFileName(folder)),Version=Value(data,"version","Unknown"),Kind="Local package library",Directory=folder,Support="Imported and editable in the library. Not installed into the server: its general mod loader is isolated for this game build. No package code is executed."});}
    catch{result.Add(new ManagedMod{Id="library:"+Path.GetFileName(folder),Name=Path.GetFileName(folder),Version="Unreadable metadata",Kind="Local package library",Directory=folder,Support="Package metadata needs review. It is not loaded by the server."});}
   }
   return result;
  }
  internal static ActionResult SetStartup(string server,string app,string id,bool enabled,string backupRoot,Func<bool> running=null,Func<bool> compatible=null){
   var changed=new List<KeyValuePair<string,string>>();string rollback=null;
   try{
    if(!Flags.ContainsKey(id))return ActionResult.Fail("Stacking is required by this world and cannot be removed here.");
    if((running??Running)())return ActionResult.Fail("Stop the server before changing its startup mods. The running process was not modified.");
    if(!(compatible??delegate{return InstallationAudit.Inspect(server).CanStart;})())return ActionResult.Fail("The installed build/controllers must pass compatibility checks before startup settings can change.");
    NoLinks(server);NoLinks(Library(app));
    if(id=="flight-stamina"&&enabled&&!Enabled(server,"flight"))return ActionResult.Fail("Enable Flight first; zero flight drain depends on it.");
    if(Enabled(server,id)==enabled)return ActionResult.Ok("This startup setting is already selected.");
    var backup=SaveBackup.Create(server,backupRoot,false);if(!backup.Success)return backup;
    rollback=Path.Combine(Storage.DataDir(app),"Mod Changes",Guid.NewGuid().ToString("N").Substring(0,8));NoLinks(rollback);System.IO.Directory.CreateDirectory(rollback);
    var changes=new Dictionary<string,bool>{{id,enabled}};if(id=="flight"&&!enabled)changes["flight-stamina"]=false;
    foreach(var change in changes){
     if((running??Running)())throw new IOException("Server started while preparing; startup changes cancelled.");
     string path=Flag(server,change.Key);NoLinks(path);System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));
     string previous=File.Exists(path)?File.ReadAllText(path):null;changed.Add(new KeyValuePair<string,string>(path,previous));
     File.WriteAllText(Path.Combine(rollback,change.Key+".json"),new JavaScriptSerializer().Serialize(new{Path=path,Existed=previous!=null,Contents=previous}));
     if(change.Value){if(File.Exists(path))File.Move(path,Path.Combine(rollback,change.Key+"-disabled.flag"));}
     else if(!File.Exists(path))File.WriteAllText(path,"Disabled through ShroudForge. Applies at next safe start.\r\n",Encoding.UTF8);
    }
    if((running??Running)())throw new IOException("Server started before completion; startup changes rolled back.");
    Storage.AppendLauncherEvent(app,"Info",id+" startup setting: "+(enabled?"enabled":"disabled"));
    return ActionResult.Ok("Startup setting saved. It takes effect at the next safe start. Controller files were retained.\r\nVerified stopped-world backup and change history were kept.");
   }catch(Exception ex){string errors="";foreach(var change in changed.AsEnumerable().Reverse())try{if(change.Value==null){if(File.Exists(change.Key))File.Move(change.Key,Path.Combine(rollback,Guid.NewGuid().ToString("N").Substring(0,8)+".flag"));}else File.WriteAllText(change.Key,change.Value);}catch(Exception restore){errors+=" Review rollback: "+restore.Message;}return ActionResult.Fail("Startup change failed: "+ex.Message+errors);}
  }
  internal static ActionResult Import(string source,string app){
   string staged=null;
   try{
    source=Path.GetFullPath(source).TrimEnd(Path.DirectorySeparatorChar);NoLinks(source);string metadata=Path.Combine(source,"mod.json");if(!File.Exists(metadata)||new FileInfo(metadata).Length>65536)throw new IOException("Choose a mod folder containing mod.json (up to 64 KB).");
    var data=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(metadata));string id=Value(data,"id","");if(!Regex.IsMatch(id,@"\A[a-zA-Z0-9][a-zA-Z0-9._-]{0,63}\z"))throw new IOException("mod.json needs a simple id of 1–64 letters, numbers, dots, dashes or underscores.");
    string library=Library(app);NoLinks(library);if(source.StartsWith(Path.GetFullPath(library)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)||string.Equals(source,Path.GetFullPath(library),StringComparison.OrdinalIgnoreCase))throw new IOException("This package is already in the library.");
    string destination=Inside(library,id);if(System.IO.Directory.Exists(destination))throw new IOException("A package with this id is already in the library. Remove it before importing another version.");
    var files=Tree(source);if(files.Sum(f=>new FileInfo(f).Length)>268435456)throw new IOException("Package exceeds 256 MB.");
    string stageRoot=Path.Combine(Storage.DataDir(app),"Mod Imports");NoLinks(stageRoot);System.IO.Directory.CreateDirectory(stageRoot);staged=Path.Combine(stageRoot,Guid.NewGuid().ToString("N"));System.IO.Directory.CreateDirectory(staged);
    foreach(string file in files){string relative=file.Substring(source.Length+1),target=Inside(staged,relative);System.IO.Directory.CreateDirectory(Path.GetDirectoryName(target));string hash=InstallationAudit.Hash(file);File.Copy(file,target);if(hash!=InstallationAudit.Hash(target)||hash!=InstallationAudit.Hash(file))throw new IOException("Package changed during import.");}
    if(!files.SequenceEqual(Tree(source),StringComparer.OrdinalIgnoreCase))throw new IOException("Package file list changed during import.");
    NoLinks(library);System.IO.Directory.CreateDirectory(library);System.IO.Directory.Move(staged,destination);staged=null;
    Storage.AppendLauncherEvent(app,"Info","Imported local package "+id+" into the inactive library.");return ActionResult.Ok("Mod added to the local library. You can configure or remove its files here. It is NOT loaded: this server's general mod loader is currently isolated.");
   }catch(Exception ex){return ActionResult.Fail("Import stopped: "+ex.Message+(staged==null?"":" Incomplete import retained for review."));}
  }
  static string Package(string app,string id){if(!id.StartsWith("library:",StringComparison.Ordinal))throw new IOException("Only imported packages can be archived or edited.");string name=id.Substring(8);if(!Regex.IsMatch(name,@"\A[a-zA-Z0-9][a-zA-Z0-9._-]{0,63}\z"))throw new IOException("Invalid package id.");string path=Inside(Library(app),name);NoLinks(path);return path;}
  internal static ActionResult Remove(string app,string id){try{string source=Package(app,id);if(!System.IO.Directory.Exists(source))throw new IOException("Package no longer exists.");string archive=Archive(app);NoLinks(archive);System.IO.Directory.CreateDirectory(archive);string destination=Inside(archive,Path.GetFileName(source)+"-"+Guid.NewGuid().ToString("N"));System.IO.Directory.Move(source,destination);Storage.AppendLauncherEvent(app,"Info","Removed inactive package "+id+" to recoverable archive.");return ActionResult.Ok("Package removed from the library. Its files remain recoverable under Removed Mods. Server files were not changed.");}catch(Exception ex){return ActionResult.Fail("Package not removed: "+ex.Message);}}
  internal static string[] ConfigFiles(string app,string id){string root=Package(app,id);return Tree(root).Where(p=>ConfigExtensions.Contains(Path.GetExtension(p).ToLowerInvariant())&&Path.GetFileName(p)!="mod.json"&&new FileInfo(p).Length<=1048576).Select(p=>p.Substring(root.Length+1)).OrderBy(p=>p).ToArray();}
  internal static string ReadConfig(string app,string id,string relative){string hash;return ReadConfig(app,id,relative,out hash);}
  internal static string ReadConfig(string app,string id,string relative,out string hash){string path=Inside(Package(app,id),relative);NoLinks(path);if(new FileInfo(path).Length>1048576||!ConfigFiles(app,id).Contains(relative,StringComparer.OrdinalIgnoreCase))throw new IOException("Not a supported package configuration file.");byte[] bytes=File.ReadAllBytes(path);if(bytes.Length>1048576)throw new IOException("Configuration exceeds 1 MB.");using(var sha=System.Security.Cryptography.SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","");return Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF');}
  internal static ActionResult SaveConfig(string app,string id,string relative,string text,string expected){try{string root=Package(app,id),path=Inside(root,relative);ReadConfig(app,id,relative);if(Encoding.UTF8.GetByteCount(text)>1048576)throw new IOException("Configuration exceeds 1 MB.");if(InstallationAudit.Hash(path)!=expected)throw new IOException("File changed on disk. Reload before saving.");if(Path.GetExtension(path).Equals(".json",StringComparison.OrdinalIgnoreCase))new JavaScriptSerializer().DeserializeObject(text);string backups=Path.Combine(Storage.DataDir(app),"Mod Config Backups",Guid.NewGuid().ToString("N").Substring(0,8));NoLinks(backups);System.IO.Directory.CreateDirectory(backups);string temp=path+".pending-"+Guid.NewGuid().ToString("N");File.WriteAllText(temp,text,new UTF8Encoding(false));if(InstallationAudit.Hash(path)!=expected)throw new IOException("File changed while preparing. Pending file retained.");File.Replace(temp,path,Path.Combine(backups,Path.GetFileName(path)));return ActionResult.Ok("Library configuration saved with rollback copy. This inactive package is not loaded by the server.");}catch(Exception ex){return ActionResult.Fail("Configuration not saved: "+ex.Message);}}
 }
}
