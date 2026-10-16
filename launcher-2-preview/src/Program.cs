using System;
using System.IO;
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

                // The bootstrap BAT passes the folder it was launched from. If that
                // folder contains the dedicated server, prefer it over auto-detection.
                if (args != null && args.Length > 0)
                {
                    string hintedRoot = (args[0] ?? "").Trim().Trim('"');
                    try { hintedRoot = Path.GetFullPath(hintedRoot); } catch { }
                    if (Directory.Exists(hintedRoot) && File.Exists(Path.Combine(hintedRoot, "enshrouded_server.exe")))
                    {
                        settings.ServerRoot = hintedRoot;
                        Storage.SaveSettings(baseDir, settings);
                    }
                }

                Application.Run(new ControlRoom(baseDir, settings, true));
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

