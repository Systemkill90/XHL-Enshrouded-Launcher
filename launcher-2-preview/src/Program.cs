using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace XHLLauncher
{
    static class Program
    {
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
                MessageBox.Show("XHL Launcher encountered an unexpected UI error.\r\n\r\n" + e.Exception.Message +
                    "\r\n\r\nDetails were written to XHL-LAUNCHER-ERRORS.log.",
                    "XHL Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                Exception ex = e.ExceptionObject as Exception;
                if (ex != null) LogException("Unhandled", ex);
            };

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
                LauncherSettings settings = Storage.LoadSettings(baseDir);

                // A manually selected server location wins over legacy BAT arguments.
                // Both paths are resolved before the UI and lifecycle objects are created.
                string serverArgument = args == null ? null : args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal));
                string resolved = ServerLocation.Resolve(settings, serverArgument);
                if (!string.Equals(settings.ServerRoot, resolved, StringComparison.OrdinalIgnoreCase))
                {
                    settings.ServerRoot = resolved;
                    Storage.SaveSettings(baseDir, settings);
                }

                Application.Run(new ControlRoom(baseDir, settings, true, args == null || !args.Contains("--monitor")));
            }
            catch (Exception ex)
            {
                LogException("Startup", ex);
                MessageBox.Show("XHL Launcher could not start.\r\n\r\n" + ex.Message +
                    "\r\n\r\nDetails were written to:\r\n" + ErrorLogPath,
                    "XHL Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

