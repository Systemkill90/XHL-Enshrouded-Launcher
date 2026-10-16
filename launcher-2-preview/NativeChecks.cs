using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using XHLLauncher;
static class NativeChecks
{
    [STAThread]
    static void Main(string[] args)
    {
        string root = Path.GetFullPath(args[0]);
        string report = Path.Combine(root, "native-result.txt");
        try
        {
            // Test only the dedicated fixture executable supplied by Test-Native.ps1.
            if (!File.Exists(Path.Combine(root, "XHL-TEST-FIXTURE.marker"))) throw new Exception("Fixture marker missing");
            string stub = Path.Combine(root, "enshrouded_server.exe");
            var info = new ProcessStartInfo(stub) { UseShellExecute = false, CreateNoWindow = false, WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = root };
            using (Process dummy = Process.Start(info))
            {
                DateTime deadline = DateTime.UtcNow.AddSeconds(5);
                while (!File.Exists(Path.Combine(root, "ready.txt")) && !dummy.HasExited && DateTime.UtcNow < deadline) Thread.Sleep(50);
                if (!File.Exists(Path.Combine(root, "ready.txt"))) throw new Exception("Dummy did not become ready");
                var settings = new LauncherSettings { ServerRoot = root };
                var runtime = new ExistingServerRuntime(root, settings);
                if (!runtime.IsRunning) throw new Exception("Exact-path detection missed dummy");
                var other = new ServerControl(Path.Combine(root, "different-installation"), settings);
                using (Process wrong = other.GetServerProcess()) if (wrong != null) throw new Exception("Different installation was incorrectly matched");
                ActionResult stopped = runtime.Stop();
                if (!stopped.Success) throw new Exception(stopped.Message);
                if (!dummy.HasExited || !File.Exists(Path.Combine(root, "graceful-signal.txt"))) throw new Exception("Graceful signal/exit not verified");
                File.WriteAllText(Path.Combine(root, "XHL-Diagnostic-Wrapper.ps1"), "Start-Sleep -Milliseconds 1000\r\nSet-Content -LiteralPath (Join-Path $PSScriptRoot 'wrapper-ran.txt') -Value 'fixture'\r\nexit 3\r\n");
                if (!runtime.Start().Success) throw new Exception("Fixture wrapper not requested");
                if (runtime.Start().Success) throw new Exception("Duplicate bootstrap was allowed");
                deadline = DateTime.UtcNow.AddSeconds(8);
                while (runtime.StartupPending && DateTime.UtcNow < deadline) Thread.Sleep(50);
                if (runtime.StartupPending || !runtime.StartupStatus.Contains("exit 3") || !File.Exists(Path.Combine(root, "wrapper-ran.txt"))) throw new Exception("Wrapper failure was not reported correctly");
                File.WriteAllText(report, "PASS exact-path process detection\r\nPASS different installation excluded\r\nPASS native Ctrl+C delivered to isolated dummy\r\nPASS dummy exited gracefully without force-kill\r\nPASS existing-wrapper invocation with isolated script\r\nPASS duplicate bootstrap blocked\r\nPASS nonzero wrapper exit reported without false online status\r\n");
            }
        }
        catch (Exception ex) { File.WriteAllText(report, "FAIL " + ex); }
    }
}
