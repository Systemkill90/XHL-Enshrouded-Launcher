using System;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
static class DummyServer
{
    [DllImport("kernel32.dll")] static extern bool SetConsoleCtrlHandler(IntPtr handler, bool add);
    static void Main()
    {
        string root = AppDomain.CurrentDomain.BaseDirectory;
        // Reset the ignore flag inherited from the test runner; opt into Ctrl+C handling.
        SetConsoleCtrlHandler(IntPtr.Zero, false);
        Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
        {
            File.WriteAllText(Path.Combine(root, "graceful-signal.txt"), "CTRL_C received by isolated dummy");
            e.Cancel = false;
        };
        File.WriteAllText(Path.Combine(root, "ready.txt"), "ready");
        Thread.Sleep(20000); // Self-exit even if the test harness fails; never force-kill.
    }
}
