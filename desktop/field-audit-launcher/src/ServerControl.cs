using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;

namespace XHLLauncher
{
    public class ActionResult
    {
        public bool Success;
        public string Message;
        public static ActionResult Ok(string m) { return new ActionResult { Success = true, Message = m }; }
        public static ActionResult Fail(string m) { return new ActionResult { Success = false, Message = m }; }
    }

    public class ServerControl
    {
        private readonly string root;
        private readonly LauncherSettings settings;
        public ServerControl(string root, LauncherSettings settings) { this.root = root; this.settings = settings; }

        public Process GetServerProcess()
        {
            Exception failure = null;
            Process selected = null;
            try
            {
                string expected = Path.GetFullPath(Path.Combine(root, "enshrouded_server.exe"));
                foreach (Process p in Process.GetProcessesByName("enshrouded_server"))
                {
                    try
                    {
                        if (string.Equals(Path.GetFullPath(p.MainModule.FileName), expected, StringComparison.OrdinalIgnoreCase))
                        { if(selected==null) { selected = p; continue; } }
                    }
                    catch (Exception ex) { failure = ex; }
                    try { p.Dispose(); } catch { }
                }
            }
            catch (Exception ex) { failure = ex; }
            if (selected != null) return selected;
            if (failure != null) throw new IOException("Server process observation failed; absence is unconfirmed.", failure);
            return null;
        }

    }
}

