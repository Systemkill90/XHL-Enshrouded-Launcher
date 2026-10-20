using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace XHLLauncher
{
    internal sealed class FoundServer {public string Root,Name,Error,Kind;public override string ToString(){return Name+" — "+Root+(Error==null?"":" — "+Error);}}
    internal sealed class ServerSearch {public List<FoundServer> Servers=new List<FoundServer>();public List<string> Errors=new List<string>();public bool Limited;}
    internal sealed class ServerPreference {public string Root,Name,BackupRoot;public bool AutoBackup,Recovery;public int BackupMinutes;}
    internal static class ServerSetup
    {
        internal static bool Overlaps(string a,string b){a=Path.GetFullPath(a).TrimEnd('\\','/');b=Path.GetFullPath(b).TrimEnd('\\','/');return a.Equals(b,StringComparison.OrdinalIgnoreCase)||a.StartsWith(b+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)||b.StartsWith(a+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase);}
        internal static void NoLinks(string path){for(var dir=new DirectoryInfo(Path.GetFullPath(path));dir!=null;dir=dir.Parent)if(dir.Exists&&(dir.Attributes&FileAttributes.ReparsePoint)!=0)throw new IOException("Linked/junction folders are not supported here.");}
        internal static IEnumerable<string> DefaultRoots(string app,LauncherSettings settings)
        {
            var roots=new List<string>{settings.ServerRoot};
            foreach(string program in new[]{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}){
                string steam=Path.Combine(program,"Steam");roots.Add(Path.Combine(steam,"steamapps","common"));
                string libraries=Path.Combine(steam,"steamapps","libraryfolders.vdf");
                try{if(File.Exists(libraries))foreach(Match match in Regex.Matches(File.ReadAllText(libraries),"\"path\"\\s+\"([^\"]+)\""))roots.Add(Path.Combine(match.Groups[1].Value.Replace("\\\\","\\"),"steamapps","common"));}catch{}
            }
            return roots.Where(p=>!string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase);
        }
        internal static ServerSearch Scan(IEnumerable<string> roots,int limit=3000,int depth=4)
        {
            var result=new ServerSearch();var visited=new HashSet<string>(StringComparer.OrdinalIgnoreCase);var queue=new Queue<Tuple<string,int>>();
            foreach(string root in roots.Where(p=>!string.IsNullOrWhiteSpace(p)))queue.Enqueue(Tuple.Create(Path.GetFullPath(root),0));
            int seen=0;
            while(queue.Count>0){var next=queue.Dequeue();if(!visited.Add(next.Item1))continue;if(++seen>limit){result.Limited=true;break;}
                try{
                    var directory=new DirectoryInfo(next.Item1);if(!directory.Exists)continue;NoLinks(next.Item1);
                    if(File.Exists(Path.Combine(next.Item1,"enshrouded_server.exe"))){
                        string error=ServerLocation.Validate(next.Item1),name=directory.Name;
                        if(error==null){var data=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(next.Item1,"enshrouded_server.json")));object value;if(data.TryGetValue("name",out value))name=Convert.ToString(value);}
                        result.Servers.Add(new FoundServer{Root=next.Item1,Name=name,Error=error,Kind=VanillaServer.Managed(next.Item1)?"Registered standard":File.Exists(Path.Combine(next.Item1,"XHL-Diagnostic-Wrapper.ps1"))?"Existing managed":"Existing standard"});
                        continue; // Do not descend into saves, mods or backups of a found installation.
                    }
                    if(next.Item2<depth)foreach(var child in directory.GetDirectories())queue.Enqueue(Tuple.Create(child.FullName,next.Item2+1));
                }catch(Exception ex){if(result.Errors.Count<20)result.Errors.Add(next.Item1+": "+ex.Message);}
            }
            return result;
        }
        internal static ActionResult Create(string app,string current,string target,string steamCmd,string name,string password,int port,int slots,Action<string> progress,Func<string,string,Action<string>,int> download=null)
        {
            try {
                target=Path.GetFullPath(target);NoLinks(target);
                if(Overlaps(target,app)||(!string.IsNullOrWhiteSpace(current)&&Overlaps(target,current)))throw new IOException("Use a separate new folder outside the launcher and existing server.");
                for(var parent=Directory.GetParent(target);parent!=null;parent=parent.Parent)if(File.Exists(Path.Combine(parent.FullName,"enshrouded_server.exe")))throw new IOException("A new server cannot be placed inside another installation.");
                if(Directory.Exists(target)&&Directory.EnumerateFileSystemEntries(target).Any())throw new IOException("The new server folder must be empty. Existing files are never overwritten.");
                if(string.IsNullOrWhiteSpace(name)||name.Length>64||name.Any(char.IsControl)||password==null||password.Length<8||port<1024||port>65535||slots<1||slots>16)throw new IOException("Enter a server name, administrator password (at least 8 characters), UDP query port 1024–65535 and 1–16 slots.");
                if(!File.Exists(steamCmd)||!Path.GetFileName(steamCmd).Equals("steamcmd.exe",StringComparison.OrdinalIgnoreCase))throw new IOException("Select the official steamcmd.exe.");
                if(target.IndexOfAny(new[]{'"','\r','\n'})>=0)throw new IOException("The new folder path contains unsupported characters.");
                Directory.CreateDirectory(target);
                using(var marker=new FileStream(Path.Combine(target,".shroudforge-creating"),FileMode.CreateNew,FileAccess.Write)){}
                string job=Path.Combine(Storage.DataDir(app),"Server Setup",Guid.NewGuid().ToString("N")),client=Path.Combine(job,"client");Directory.CreateDirectory(client);
                string copy=Path.Combine(client,"steamcmd.exe");File.Copy(steamCmd,copy,false);
                if(progress==null)progress=delegate{};
                var output=new StringBuilder();Action<string> line=text=>{if(text==null)return;lock(output){output.AppendLine(text);if(output.Length>262144)output.Remove(0,output.Length-262144);}progress(text);};
                progress("Downloading a new standard server. No existing world or mod files are copied.");
                int exit=(download??Download)(copy,target,line);
                if(!StagedUpdate.Confirmed(exit,output.ToString(),target))throw new IOException("SteamCMD did not confirm a complete installation. Partial files remain in the new folder for review; the existing server was not changed.");
                // Never start the game merely to generate configuration or a world.
                var config=new{ name=name.Trim(),saveDirectory="./savegame",logDirectory="./logs",ip="0.0.0.0",queryPort=port,slotCount=slots,gameSettingsPreset="Default",userGroups=new[]{new{name="Admin",password=password,canKickBan=true,canAccessInventories=true,canEditBase=true,canExtendBase=true,reservedSlots=0}} };
                using(var stream=new FileStream(Path.Combine(target,"enshrouded_server.json"),FileMode.CreateNew,FileAccess.Write))using(var writer=new StreamWriter(stream))writer.Write(new JavaScriptSerializer().Serialize(config));
                Directory.CreateDirectory(Path.Combine(target,"savegame"));Directory.CreateDirectory(Path.Combine(target,"logs"));
                var enrolled=VanillaServer.Register(target);if(!enrolled.Success)throw new IOException(enrolled.Message);
                File.Delete(Path.Combine(target,".shroudforge-creating"));
                return ActionResult.Ok("New standard server created. It has not been started. Select it to manage its separate world.");
            }catch(Exception ex){return ActionResult.Fail("Server creation did not complete: "+ex.Message);}
        }
        static int Download(string executable,string target,Action<string> output)
        {
            var info=new ProcessStartInfo(executable,"+force_install_dir \""+target+"\" +login anonymous +app_update 2278520 validate +quit"){WorkingDirectory=Path.GetDirectoryName(executable),UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden,RedirectStandardOutput=true,RedirectStandardError=true};
            using(var process=new Process{StartInfo=info}){DataReceivedEventHandler handler=(sender,e)=>{if(e.Data!=null)output(e.Data);};process.OutputDataReceived+=handler;process.ErrorDataReceived+=handler;process.Start();process.BeginOutputReadLine();process.BeginErrorReadLine();process.WaitForExit();return process.ExitCode;}
        }
        internal static string Scope(string app,LauncherSettings settings)
        {
            if(string.IsNullOrWhiteSpace(settings.LegacyServerRoot)||string.Equals(Path.GetFullPath(settings.LegacyServerRoot).TrimEnd('\\','/'),Path.GetFullPath(settings.ServerRoot).TrimEnd('\\','/'),StringComparison.OrdinalIgnoreCase))return Storage.DataDir(app);
            string key;using(var sha=System.Security.Cryptography.SHA256.Create())key=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(Path.GetFullPath(settings.ServerRoot).TrimEnd('\\','/').ToLowerInvariant()))).Replace("-","").Substring(0,16);
            return Path.Combine(Storage.DataDir(app),"Servers",key);
        }
        internal static string HistoryApp(string app,LauncherSettings settings){string scope=Scope(app,settings);return scope==Storage.DataDir(app)?app:Path.Combine(scope,"Telemetry");}
        internal static ActionResult Select(string app,LauncherSettings settings,string selected)
        {
            try {
                string error=ServerLocation.Validate(selected);if(error!=null)throw new IOException(error);selected=Path.GetFullPath(selected);
                string path=Path.Combine(Storage.DataDir(app),"server-profiles.json");var json=new JavaScriptSerializer();
                var profiles=File.Exists(path)?json.Deserialize<List<ServerPreference>>(File.ReadAllText(path)):new List<ServerPreference>();if(profiles==null)throw new IOException("Saved server profiles are invalid.");
                string legacy=string.IsNullOrWhiteSpace(settings.LegacyServerRoot)?(ServerLocation.Validate(settings.ServerRoot)==null?settings.ServerRoot:selected):settings.LegacyServerRoot;
                profiles.RemoveAll(p=>string.Equals(p.Root,settings.ServerRoot,StringComparison.OrdinalIgnoreCase));profiles.Add(new ServerPreference{Root=settings.ServerRoot,Name=settings.ServerName,BackupRoot=settings.BackupRoot,AutoBackup=settings.AutoBackup,Recovery=settings.AutoRestartOnCrash,BackupMinutes=settings.BackupIntervalMinutes});
                var chosen=profiles.FirstOrDefault(p=>string.Equals(p.Root,selected,StringComparison.OrdinalIgnoreCase));
                var config=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(selected,"enshrouded_server.json")));
                var next=json.Deserialize<LauncherSettings>(json.Serialize(settings));next.LegacyServerRoot=legacy;next.ServerRoot=next.PreferredServerRoot=selected;object name;next.ServerName=config.TryGetValue("name",out name)?Convert.ToString(name):new DirectoryInfo(selected).Name;
                next.BackupRoot=chosen==null?Path.Combine(Scope(app,next),"World-Backups"):chosen.BackupRoot;next.AutoBackup=chosen!=null&&chosen.AutoBackup;next.AutoRestartOnCrash=chosen!=null&&chosen.Recovery;next.BackupIntervalMinutes=chosen==null?30:chosen.BackupMinutes;
                string pending=path+".pending";File.WriteAllText(pending,json.Serialize(profiles));if(File.Exists(path))File.Replace(pending,path,path+".bak");else File.Move(pending,path);
                Storage.SaveSettings(app,next);return ActionResult.Ok("Server selection saved. Reopen the launcher to use it; the previous server is left alone.");
            }catch(Exception ex){return ActionResult.Fail("Server selection was not saved: "+ex.Message);}
        }
    }
}
