using System;
using System.IO;
using System.Linq;
using XHLLauncher;
static class ModChecks {
 static int count;static void Check(bool ok,string message){if(!ok)throw new Exception(message);count++;Console.WriteLine("PASS "+message);}
 static int Main(string[] args){try{
  string root=Path.Combine(args[0],"mods-"+Guid.NewGuid().ToString("N")),server=Path.Combine(root,"server"),app=Path.Combine(root,"app"),backups=Path.Combine(root,"backups");Directory.CreateDirectory(Path.Combine(server,"savegame"));Directory.CreateDirectory(app);File.WriteAllText(Path.Combine(server,"enshrouded_server.json"),"{\"saveDirectory\":\"./savegame\"}");string save=Path.Combine(server,"savegame","world.ksd");File.WriteAllText(save,"world preservation sentinel");string original=InstallationAudit.Hash(save);
  var initial=ModManagement.List(server,app);Check(initial.Count==4&&initial.Single(m=>m.Id=="stacking").Locked,"Installed catalog exposes three startup controllers and protected stacking");
  Check(!ModManagement.SetStartup(server,app,"stacking",false,backups,()=>false,()=>true).Success,"Stacking removal is blocked");
  Check(!ModManagement.SetStartup(server,app,"flight",false,backups,()=>true,()=>true).Success&&!Directory.Exists(backups),"Running server blocks startup edits before any backup or flag write");
  Check(!ModManagement.SetStartup(server,app,"flight",false,backups,()=>false,()=>false).Success,"Unknown build/controllers block startup edits");
  Check(ModManagement.SetStartup(server,app,"flight",false,backups,()=>false,()=>true).Success&&!ModManagement.Enabled(server,"flight")&&!ModManagement.Enabled(server,"flight-stamina"),"Disabling Flight also disables dependent flight stamina for the next start");
  Check(Directory.GetDirectories(backups).Length==1&&Directory.GetFiles(backups,"VERIFIED.txt",SearchOption.AllDirectories).Length==1,"Startup edits first retain a verified stopped-world backup");
  Check(!ModManagement.SetStartup(server,app,"flight-stamina",true,backups,()=>false,()=>true).Success,"Flight stamina cannot be enabled without Flight");
  Check(ModManagement.SetStartup(server,app,"flight",true,backups,()=>false,()=>true).Success&&ModManagement.Enabled(server,"flight")&&!ModManagement.Enabled(server,"flight-stamina"),"Re-enabling Flight preserves the separate stamina preference");
  Check(ModManagement.SetStartup(server,app,"flight-stamina",true,backups,()=>false,()=>true).Success&&ModManagement.Enabled(server,"flight-stamina"),"Stamina startup switch can be re-enabled independently");
  int checks=0;var race=ModManagement.SetStartup(server,app,"flight",false,backups,()=>++checks>2,()=>true);Console.WriteLine("Race diagnostic: "+race.Message);Check(!race.Success&&ModManagement.Enabled(server,"flight"),"Server-start race rolls back already-written startup flag");
  Check(InstallationAudit.Hash(save)==original,"Every startup configuration operation preserves original world bytes");
  string source=Path.Combine(root,"source");Directory.CreateDirectory(source);File.WriteAllText(Path.Combine(source,"mod.json"),"{\"id\":\"fixture-mod\",\"name\":\"Fixture mod\",\"version\":\"1.0\"}");File.WriteAllText(Path.Combine(source,"config.json"),"{\"amount\":1}");File.WriteAllText(Path.Combine(source,"script.ps1"),"throw 'This package must never execute'");
  string sourceHash=InstallationAudit.Hash(Path.Combine(source,"config.json"));Check(ModManagement.Import(source,app).Success,"Local mod folder imports into inactive library");
  var package=ModManagement.List(server,app).Single(m=>m.Id=="library:fixture-mod");Check(!package.BuiltIn&&!package.Enabled&&package.Support.Contains("Not installed"),"Imported code is explicitly inactive and never represented as loaded");
  Check(!ModManagement.Import(source,app).Success,"Duplicate package id is rejected without overwrite");
  var configs=ModManagement.ConfigFiles(app,package.Id);Check(configs.SequenceEqual(new[]{"config.json"}),"Editor excludes executable code and package identity metadata");
  string file=Path.Combine(package.Directory,"config.json"),hash=InstallationAudit.Hash(file);
  Check(!ModManagement.SaveConfig(app,package.Id,"config.json","{broken",hash).Success&&InstallationAudit.Hash(file)==hash,"Invalid JSON leaves configuration unchanged");
  Check(!ModManagement.SaveConfig(app,package.Id,"config.json","{}","stale").Success,"Stale configuration cannot overwrite a newer version");
  Check(ModManagement.SaveConfig(app,package.Id,"config.json","{\"amount\":3}",hash).Success&&ModManagement.ReadConfig(app,package.Id,"config.json").Contains("3"),"Configuration saves atomically in the library");
  Check(Directory.GetFiles(Path.Combine(Storage.DataDir(app),"Mod Config Backups"),"config.json",SearchOption.AllDirectories).Length==1,"Configuration edit retains rollback copy");
  bool escaped=false;try{ModManagement.ReadConfig(app,package.Id,"..\\..\\settings.xml");}catch(IOException){escaped=true;}Check(escaped,"Configuration path traversal is rejected");
  Check(ModManagement.Remove(app,package.Id).Success&&!Directory.Exists(package.Directory)&&Directory.GetDirectories(ModManagement.Archive(app)).Length==1,"Remove archives the package instead of deleting its files");
  Check(InstallationAudit.Hash(Path.Combine(source,"config.json"))==sourceHash,"Library editing and removal preserve the original imported source");
  Check(ModManagement.Import(Directory.GetDirectories(ModManagement.Archive(app))[0],app).Success,"Removed package can be restored by re-importing its archived folder");
  Check(!ModManagement.Remove(app,"library:..\\escape").Success,"Malformed removal id cannot escape the library");
  File.WriteAllText(Path.Combine(source,"mod.json"),"{\"id\":\"../escape\"}");Check(!ModManagement.Import(source,app).Success,"Imported metadata cannot direct files outside library");
  Check(InstallationAudit.Hash(save)==original&&!File.Exists(Path.Combine(server,"script.ps1")),"Package operations never install code into the game or edit world files");
  Console.WriteLine(count+" mod management checks passed.");return 0;
 }catch(Exception ex){Console.WriteLine("FAIL "+ex);return 1;}}
}
