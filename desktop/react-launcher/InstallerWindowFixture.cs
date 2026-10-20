using System;
using System.IO;
using System.Windows.Forms;

// Disposable installer test window. No game, settings or network code.
static class InstallerWindowFixture
{
    sealed class Window : Form
    {
        readonly string root;
        readonly DateTime started=DateTime.UtcNow;
        [System.Runtime.InteropServices.DllImport("user32.dll",CharSet=System.Runtime.InteropServices.CharSet.Unicode)]static extern uint RegisterWindowMessage(string value);
        protected override void WndProc(ref Message message){if(message.Msg==(int)RegisterWindowMessage("ShroudForge.Update.CloseLauncherOnly")){message.Result=new IntPtr(1);File.WriteAllText(Path.Combine(root,"maintenance.marker"),"Acknowledged launcher-only update");BeginInvoke(new Action(()=>Close()));return;}base.WndProc(ref message);}
        protected override void SetVisibleCore(bool visible) { base.SetVisibleCore(false); }
        internal Window(string directory)
        {
            root=directory;Text="ShroudForge — Control Center";
            IntPtr created=Handle;
            FormClosing+=delegate(object sender,FormClosingEventArgs e) {
                if(File.Exists(Path.Combine(root,"busy.marker"))&&DateTime.UtcNow-started<TimeSpan.FromSeconds(30))e.Cancel=true;
            };
            FormClosed+=delegate { File.WriteAllText(Path.Combine(root,"closed.marker"),"Normal FormClosing/FormClosed"); };
            var timer=new Timer{Interval=250};timer.Tick+=delegate {if(DateTime.UtcNow-started>TimeSpan.FromSeconds(30))Close();};timer.Start();
            File.WriteAllText(Path.Combine(root,"ready.marker"),System.Diagnostics.Process.GetCurrentProcess().Id.ToString());
        }
    }
    [STAThread]static void Main(string[] args)
    {
        string root=args.Length==1?args[0]:AppDomain.CurrentDomain.BaseDirectory;
        if(!File.Exists(Path.Combine(root,"installer-fixture.marker")))return;
        Application.Run(new Window(Path.GetFullPath(root)));
    }
}
