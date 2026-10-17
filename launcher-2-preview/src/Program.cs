using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace XHLLauncher
{
    static class Program
    {
        static System.Threading.Mutex instance;
        private static string ErrorLogPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XHL-LAUNCHER-ERRORS.log"); }
        }

        private static void LogException(string source, Exception ex)
        {
            try
            {
                File.AppendAllText(ErrorLogPath,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [" + source + "]\r\n" + ex + "\r\n\r\n");
            }
            catch { }
        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e)
            {
                LogException("UI thread", e.Exception);
                MessageBox.Show("ShroudForge Launcher encountered an unexpected UI error.\r\n\r\n" + e.Exception.Message +
                    "\r\n\r\nDetails were written to XHL-LAUNCHER-ERRORS.log.",
                    "ShroudForge Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                Exception ex = e.ExceptionObject as Exception;
                if (ex != null) LogException("Unhandled", ex);
            };

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
                bool firstRun = !File.Exists(Path.Combine(baseDir,"Manager Data","settings.xml")) && !File.Exists(Path.Combine(baseDir,"XHL-Launcher-Data","settings.xml"));
                LauncherSettings settings = Storage.LoadSettings(baseDir);
                if(firstRun) {
                    using(var folder=new FolderBrowserDialog {Description="Welcome to ShroudForge Launcher 2.0. Choose your EXISTING Enshrouded dedicated server folder. Your world will remain in place.",SelectedPath=settings.ServerRoot}) {
                        if(folder.ShowDialog()!=DialogResult.OK)return;
                        string error=ServerLocation.Validate(folder.SelectedPath);if(error!=null)throw new IOException(error);
                        settings.PreferredServerRoot=settings.ServerRoot=folder.SelectedPath;Storage.SaveSettings(baseDir,settings);
                    }
                }

                // A manually selected server location wins over legacy BAT arguments.
                // Both paths are resolved before the UI and lifecycle objects are created.
                string serverArgument = args == null ? null : args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal));
                string resolved = ServerLocation.Resolve(settings, serverArgument);
                if (!string.Equals(settings.ServerRoot, resolved, StringComparison.OrdinalIgnoreCase))
                {
                    settings.ServerRoot = resolved;
                    Storage.SaveSettings(baseDir, settings);
                }

                if(args!=null && (args.Contains("--verify")||args.Contains("--start-server")||args.Contains("--restart-server"))) {
                    var check=InstallationAudit.Inspect(settings.ServerRoot);string result=check.Details;
                    if(args.Contains("--start-server")||args.Contains("--restart-server")){
                        var service=new ServerLifecycle(new ExistingServerRuntime(settings.ServerRoot,settings),delegate{return ServerLifecycle.CheckInstallation(settings.ServerRoot);});
                        var action=service.Execute(args.Contains("--restart-server")?"Restart":"Start");result+="\r\n"+action.Message;Environment.ExitCode=action.Success?0:1;
                    }else Environment.ExitCode=check.CanStart?0:1;
                    File.WriteAllText(Path.Combine(baseDir,"Verification.txt"),result);return;
                }
                string identity;
                using(var sha=System.Security.Cryptography.SHA256.Create())identity=BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(settings.ServerRoot.ToLowerInvariant()))).Replace("-","");
                bool created;instance=new System.Threading.Mutex(true,"Local\\XHL.Realm."+identity,out created);
                if(!created){MessageBox.Show("ShroudForge Launcher is already running for this realm. Open it from the system tray.","ShroudForge Launcher 2.0");return;}
                if(args == null || !args.Contains("--no-splash")) { using(var splash = new LaunchSplash(baseDir, settings.ServerRoot)) Application.Run(splash); }
                Application.Run(new ControlRoom(baseDir, settings, true, args == null || !args.Contains("--monitor")));
            }
            catch (Exception ex)
            {
                LogException("Startup", ex);
                MessageBox.Show("ShroudForge Launcher could not start.\r\n\r\n" + ex.Message +
                    "\r\n\r\nDetails were written to:\r\n" + ErrorLogPath,
                    "ShroudForge Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

