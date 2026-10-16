using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace XHLLauncher
{
    internal interface IServerRuntime
    {
        bool IsRunning { get; }
        bool StartupPending { get; }
        string StartupStatus { get; }
        ActionResult Start();
        ActionResult Stop();
    }

    // No file mutations, automatic retries, forced termination, or direct-exe fallback.
    internal sealed class ServerLifecycle
    {
        readonly IServerRuntime runtime;
        readonly Func<ActionResult> preflight;
        int busy;
        internal ServerLifecycle(IServerRuntime runtime, Func<ActionResult> preflight)
        { this.runtime = runtime; this.preflight = preflight; }
        internal bool StartupPending { get { return runtime.StartupPending; } }
        internal string StartupStatus { get { return runtime.StartupStatus; } }
        internal ActionResult Execute(string operation)
        {
            if (Interlocked.CompareExchange(ref busy, 1, 0) != 0) return ActionResult.Fail("Another server operation is in progress.");
            try
            {
                if (operation != "Start" && operation != "Stop" && operation != "Restart") return ActionResult.Fail("Unknown server operation.");
                if (runtime.StartupPending) return ActionResult.Fail("The existing startup wrapper is still running. Wait for its result before another operation.");
                if (operation == "Stop") return runtime.IsRunning ? runtime.Stop() : ActionResult.Ok("The selected server is already stopped.");
                if (operation == "Start" && runtime.IsRunning) return ActionResult.Fail("The selected server is already running.");
                ActionResult check = preflight();
                if (!check.Success) return check; // Restart must never stop first and discover incompatibility afterwards.
                if (operation == "Restart" && runtime.IsRunning)
                {
                    ActionResult stop = runtime.Stop();
                    if (!stop.Success || runtime.IsRunning) return ActionResult.Fail("Restart cancelled. " + stop.Message);
                    check = preflight();
                    if (!check.Success) return ActionResult.Fail("The server stopped, but files changed before restart. " + check.Message);
                }
                return runtime.Start();
            }
            catch (Exception ex) { return ActionResult.Fail(ex.Message); }
            finally { Interlocked.Exchange(ref busy, 0); }
        }
        internal static ActionResult CheckInstallation(string root)
        {
            InstallationAudit audit = InstallationAudit.Inspect(root);
            if (!audit.CanStart) return ActionResult.Fail("Start blocked by compatibility checks.\r\n\r\n" + audit.Details);
            foreach (string name in new[] { "enshrouded_server.json", "XHL-Diagnostic-Wrapper.ps1", "XHL-Safe-Start.ps1", "XHL-1.0-compat-manifest.json" })
                if (!File.Exists(Path.Combine(root, name))) return ActionResult.Fail("Required existing startup file missing: " + name);
            return ActionResult.Ok("Disk checks passed. The existing safe-start wrapper performs its own save backup and validation before launch.");
        }
    }

    internal sealed class ExistingServerRuntime : IServerRuntime
    {
        readonly string root;
        readonly ServerControl processes;
        Process bootstrap;
        readonly object gate = new object();
        string startupStatus = "";
        public string StartupStatus { get { lock (gate) return startupStatus; } }
        internal ExistingServerRuntime(string root, LauncherSettings settings)
        { this.root = root; processes = new ServerControl(root, settings); }
        public bool IsRunning { get { using (Process p = processes.GetServerProcess()) return p != null; } }
        public bool StartupPending
        {
            get
            {
                lock (gate)
                {
                if (bootstrap == null) return false;
                try { if (!bootstrap.HasExited) return true; } catch { }
                try { startupStatus = bootstrap.ExitCode == 0 ? "Startup wrapper completed. Check the observed server state." : "Startup wrapper failed (exit " + bootstrap.ExitCode + "). Review the existing XHL startup log."; }
                catch { startupStatus = "Startup wrapper state could not be read. No success assumed."; }
                bootstrap.Dispose(); bootstrap = null; return false;
                }
            }
        }
        public ActionResult Start()
        {
            lock (gate)
            {
            if (IsRunning || StartupPending) return ActionResult.Fail("Server or startup wrapper is already running.");
            string script = Path.Combine(root, "XHL-Diagnostic-Wrapper.ps1");
            if (!File.Exists(script)) return ActionResult.Fail("Existing startup wrapper is missing. No direct launch attempted.");
            string powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "WindowsPowerShell", "v1.0", "powershell.exe");
            var info = new ProcessStartInfo(powershell, "-NoLogo -NoProfile -ExecutionPolicy Bypass -File \"" + script + "\"");
            info.WorkingDirectory = root; info.UseShellExecute = false; info.CreateNoWindow = true; info.WindowStyle = ProcessWindowStyle.Hidden;
            bootstrap = Process.Start(info);
            startupStatus = "Startup wrapper running; awaiting process observation.";
            return bootstrap == null ? ActionResult.Fail("Startup wrapper did not start.") : ActionResult.Ok("Startup requested through your existing wrapper. The dashboard will report online only after the selected server process appears.");
            }
        }
        public ActionResult Stop()
        {
            using (Process p = processes.GetServerProcess())
            {
                if (p == null) return ActionResult.Ok("The selected server is already stopped.");
                // Keep an open process handle through signaling and waiting, preventing PID reuse.
                IntPtr handle = p.Handle;
                if (!GracefulConsole.Signal((uint)p.Id)) return ActionResult.Fail("Could not signal the server console. The server was left running.");
                if (!p.WaitForExit(30000)) return ActionResult.Fail("Shutdown was requested, but the server has not exited. It was not force-killed.");
                return ActionResult.Ok("The selected server exited after the graceful shutdown request.");
            }
        }
    }

    internal static class GracefulConsole
    {
        delegate bool ConsoleHandler(uint signal);
        static readonly ConsoleHandler Ignore = delegate { return true; };
        [DllImport("kernel32.dll", SetLastError = true)] static extern bool AttachConsole(uint pid);
        [DllImport("kernel32.dll", SetLastError = true)] static extern bool FreeConsole();
        [DllImport("kernel32.dll", SetLastError = true)] static extern bool SetConsoleCtrlHandler(ConsoleHandler handler, bool add);
        [DllImport("kernel32.dll", SetLastError = true)] static extern bool GenerateConsoleCtrlEvent(uint signal, uint group);
        internal static bool Signal(uint pid)
        {
            // A winexe has no console. Do not detach an unrelated console if attachment fails.
            if (!AttachConsole(pid)) return false;
            bool handler = false;
            try
            {
                handler = SetConsoleCtrlHandler(Ignore, true);
                if (!handler) return false;
                bool sent = GenerateConsoleCtrlEvent(0, 0); Thread.Sleep(200); return sent;
            }
            finally { FreeConsole(); if (handler) SetConsoleCtrlHandler(Ignore, false); }
        }
    }
}
