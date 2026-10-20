using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Web.Script.Serialization;using XHLLauncher;
static class ServerSetupChecks {
 internal static void Run(string root,Action<bool,string> check){
  check(DesktopWindow.MinimumClient(96,1)==new System.Drawing.Size(1100,900),"Desktop minimum protects the measured compact content area");
  check(DesktopWindow.MinimumClient(120,1)==new System.Drawing.Size(1375,1125),"Window minimum accounts for 125 percent display scaling");
  check(DesktopWindow.MinimumClient(144,1.25)==new System.Drawing.Size(2063,1688),"Window minimum accounts for display scaling combined with WebView zoom");
  foreach(var observation in new[]{ProcessObservation.Unknown,ProcessObservation.Failed})check(LauncherClosing.Evaluate(new HostState{Observation=observation,ServerRunning=true},false,false,false)==CloseState.Unknown,"Uncertain process observation warns before launcher exit: "+observation);
  check(LauncherClosing.Evaluate(new HostState{Observation=ProcessObservation.Running},false,false,false)==CloseState.Running,"Running server requires an explicit close choice");
  check(LauncherClosing.Evaluate(new HostState{Observation=ProcessObservation.Stopped},false,false,false)==CloseState.Exit,"Confirmed stopped server permits normal exit");
  check(LauncherClosing.Evaluate(new HostState{Observation=ProcessObservation.Running},true,false,true)==CloseState.Busy,"Update handover cannot bypass an active operation");
  check(LauncherClosing.Evaluate(new HostState{Observation=ProcessObservation.Running},false,false,true)==CloseState.Exit,"Explicit launcher-only update handover leaves the server running");
  string area=Path.GetFullPath(Path.Combine(root,"setup")),app=Path.Combine(area,"launcher"),old=Path.Combine(area,"old"),fresh=Path.Combine(area,"new"),steam=Path.Combine(area,"steamcmd.exe");Directory.CreateDirectory(app);Directory.CreateDirectory(old);File.WriteAllText(steam,"fixture, never executed");
  Action<string> files=folder=>{Directory.CreateDirectory(folder);foreach(string file in VanillaServer.GameFiles)File.WriteAllText(Path.Combine(folder,file),"fixture "+file);};files(old);File.WriteAllText(Path.Combine(old,"enshrouded_server.json"),"{\"name\":\"Original\",\"saveDirectory\":\"savegame\"}");Directory.CreateDirectory(Path.Combine(old,"savegame"));File.WriteAllText(Path.Combine(old,"savegame/world.ksd"),"existing world");
  string before=InstallationAudit.Hash(Path.Combine(old,"enshrouded_server.json")),world=InstallationAudit.Hash(Path.Combine(old,"savegame/world.ksd"));
  Func<string,string,Action<string>,int> success=(client,target,output)=>{files(target);output("Success! App '2278520' fully installed");return 0;};
  var result=ServerSetup.Create(app,old,fresh,steam,"Separate server","fixture password",15647,8,null,success);
  check(result.Success&&VanillaServer.Check(fresh).Success,"Confirmed download creates a separately registered standard server");
  var config=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(fresh,"enshrouded_server.json")));
  check(Convert.ToInt32(config["queryPort"])==15647&&Convert.ToInt32(config["slotCount"])==8&&config.ContainsKey("userGroups")&&!config.ContainsKey("password"),"New server uses current role-based configuration and chosen port/slots");
  check(!Directory.GetFiles(Path.Combine(fresh,"savegame")).Any()&&!File.Exists(Path.Combine(fresh,".shroudforge-creating")),"Creation leaves an empty separate world and does not launch the game");
  foreach(string target in new[]{old,Path.Combine(old,"nested"),app,fresh})check(!ServerSetup.Create(app,old,target,steam,"Blocked","fixture password",15647,8,null,success).Success,"Creation refuses an existing or overlapping destination");
  string failed=Path.Combine(area,"failed");check(!ServerSetup.Create(app,old,failed,steam,"Failed","fixture password",15647,8,null,(client,target,output)=>{files(target);return 0;}).Success&&!VanillaServer.Managed(failed),"Exit zero without Steam success is rejected without enrolling partial files");
  var scan=ServerSetup.Scan(new[]{area,old});check(scan.Servers.Count(s=>s.Root==old)==1&&scan.Servers.Any(s=>s.Root==fresh&&s.Error==null),"Folder scan finds and deduplicates nested installations");
  File.WriteAllText(Path.Combine(failed,"enshrouded_server.json"),"invalid");check(ServerSetup.Scan(new[]{failed}).Servers.Single().Error!=null,"Invalid existing configuration is visible in scan results");
  check(ServerSetup.Scan(new[]{area},1).Limited,"Bounded scan exposes its limit instead of claiming completeness");
  File.WriteAllText(Path.Combine(fresh,"enshrouded_server.exe"),"changed build");check(!VanillaServer.Check(fresh).Success&&!VanillaServer.Register(fresh).Success,"Changed standard build fails closed without silently refreshing fingerprints");
  File.WriteAllText(Path.Combine(old,"winhttp.dll"),"mod loader sentinel");check(!VanillaServer.Register(old).Success,"Existing mod loader cannot be enrolled as a standard server");File.Delete(Path.Combine(old,"winhttp.dll"));
  var settings=new LauncherSettings{ServerRoot=old,ServerName="Original",BackupRoot=Path.Combine(area,"original-backups"),AutoBackup=true,AutoRestartOnCrash=true,BackupIntervalMinutes=45};Storage.SaveSettings(app,settings);
  check(ServerSetup.Select(app,settings,fresh).Success,"Selection saves the new server without controlling the previous server");var next=Storage.LoadSettings(app);
  check(next.ServerRoot==fresh&&!next.AutoBackup&&!next.AutoRestartOnCrash&&next.BackupRoot.StartsWith(ServerSetup.Scope(app,next)),"New selection has isolated backup/recovery state with automation initially off");
  File.WriteAllText(Path.Combine(Storage.DataDir(app),"player-health.csv"),"legacy sentinel");check(ServerSetup.HistoryApp(app,next)!=app&&!File.Exists(Path.Combine(Storage.DataDir(ServerSetup.HistoryApp(app,next)),"player-health.csv")),"Selected server cannot import or overwrite the original server's legacy health CSV");
  check(settings.ServerRoot==old&&settings.LegacyServerRoot=="","Selection does not mutate the active engine settings");
  check(ServerSetup.Select(app,next,old).Success,"Previous server can be selected again");var restored=Storage.LoadSettings(app);
  check(restored.AutoBackup&&restored.AutoRestartOnCrash&&restored.BackupIntervalMinutes==45&&restored.BackupRoot==settings.BackupRoot&&ServerSetup.Scope(app,restored)==Storage.DataDir(app),"Returning restores the original backup settings and journal scope");
  string saved=InstallationAudit.Hash(Path.Combine(Storage.DataDir(app),"settings.xml"));check(!ServerSetup.Select(app,restored,Path.Combine(area,"missing")).Success&&InstallationAudit.Hash(Path.Combine(Storage.DataDir(app),"settings.xml"))==saved,"Failed selection retains the saved active server");
  check(InstallationAudit.Hash(Path.Combine(old,"enshrouded_server.json"))==before&&InstallationAudit.Hash(Path.Combine(old,"savegame/world.ksd"))==world,"Creation, scanning and switching preserve existing configuration and world bytes");
 }
}

