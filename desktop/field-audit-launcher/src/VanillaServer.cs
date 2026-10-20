using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;

namespace XHLLauncher
{
    internal sealed class VanillaRecord
    {
        public int Format=1;
        public string Mode="Standard server";
        public Dictionary<string,string> Files=new Dictionary<string,string>();
    }
    // Explicit standard-server enrollment, separate from the existing modded safe-start path.
    internal static class VanillaServer
    {
        internal const string Marker="shroudforge-server.json";
        internal static bool Managed(string root){return File.Exists(Path.Combine(root,Marker));}
        internal static readonly string[] GameFiles={"enshrouded_server.exe","enshrouded_server.kfc","enshrouded_server.kfc_resources"};
        static void NoMods(string root)
        {
            foreach(string name in new[]{"XHL-Diagnostic-Wrapper.ps1","XHL-Safe-Start.ps1","XHL-Mods-1.0","BepInEx","UE4SS","version.dll","winhttp.dll","dinput8.dll","winmm.dll"})
                if(File.Exists(Path.Combine(root,name))||Directory.Exists(Path.Combine(root,name)))throw new IOException("This installation has mod/startup components. Keep its existing verified launch profile; standard-server enrollment is blocked.");
        }
        internal static ActionResult Register(string root)
        {
            try {
                string error=ServerLocation.Validate(root);if(error!=null)throw new IOException(error);
                NoMods(root);if(Managed(root))return Check(root);
                var record=new VanillaRecord();foreach(string name in GameFiles){string file=Path.Combine(root,name);if(!File.Exists(file)||new FileInfo(file).Length==0)throw new IOException("Missing or empty server file: "+name);record.Files[name]=InstallationAudit.Hash(file);}
                using(var file=new FileStream(Path.Combine(root,Marker),FileMode.CreateNew,FileAccess.Write))
                using(var writer=new StreamWriter(file))writer.Write(new JavaScriptSerializer().Serialize(record));
                return ActionResult.Ok("Standard server registered. Game, configuration and world files were not changed.");
            }catch(Exception ex){return ActionResult.Fail(ex.Message);}
        }
        internal static ActionResult Check(string root)
        {
            try {
                string error=ServerLocation.Validate(root);if(error!=null)throw new IOException(error);NoMods(root);
                var record=new JavaScriptSerializer().Deserialize<VanillaRecord>(File.ReadAllText(Path.Combine(root,Marker)));
                if(record==null||record.Format!=1||record.Mode!="Standard server"||record.Files==null)throw new IOException("Standard-server record is invalid.");
                foreach(string name in GameFiles){string hash;if(!record.Files.TryGetValue(name,out hash)||InstallationAudit.Hash(Path.Combine(root,name))!=hash)throw new IOException("Standard server files changed. Review this installation before enrolling its updated files again.");}
                return ActionResult.Ok("Registered standard server files match. No ShroudForge mod profile is installed.");
            }catch(Exception ex){return ActionResult.Fail("Standard server start blocked: "+ex.Message);}
        }
        internal static ActionResult Start(string root)
        {
            var check=Check(root);if(!check.Success)return check;
            var info=new ProcessStartInfo(Path.Combine(root,"enshrouded_server.exe")){WorkingDirectory=root,UseShellExecute=false,CreateNoWindow=false,WindowStyle=ProcessWindowStyle.Hidden};
            using(var process=Process.Start(info)){
                if(process==null)return ActionResult.Fail("Standard server did not start.");
                if(process.WaitForExit(1000))return ActionResult.Fail("Standard server exited during startup. Review its log; no retry was made.");
                return ActionResult.Ok("Standard server started. Waiting for process/query confirmation.");
            }
        }
    }
}
