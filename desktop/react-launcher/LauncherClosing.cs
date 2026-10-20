using System;
using System.Drawing;
using System.Windows.Forms;

namespace XHLLauncher
{
    internal enum CloseState { Exit, Busy, Running, Unknown }
    internal enum CloseChoice { Cancel, LeaveRunning, StopAndExit }
    internal static class LauncherClosing
    {
        internal static CloseState Evaluate(HostState host,bool busy,bool preview,bool handover)
        {
            if(busy)return CloseState.Busy;
            if(preview||handover)return CloseState.Exit;
            if(host.Observation==ProcessObservation.Running)return CloseState.Running;
            return host.Observation==ProcessObservation.Stopped?CloseState.Exit:CloseState.Unknown;
        }
        internal static CloseChoice Ask(IWin32Window owner,bool running)
        {
            using(var dialog=new Form{Text="Close ShroudForge",ClientSize=new Size(570,205),FormBorderStyle=FormBorderStyle.FixedDialog,StartPosition=FormStartPosition.CenterParent,MinimizeBox=false,MaximizeBox=false}){
                var message=new Label{AutoSize=false,Bounds=new Rectangle(20,18,530,112),Text=running?"The selected server is still running.\r\n\r\nKeep it running, or request a graceful server stop before closing? Closing the launcher pauses its monitoring, scheduled backups and automatic recovery.":"The launcher cannot confirm whether the server is running.\r\n\r\nYou can close the launcher without sending any server command, or cancel and check its status. Monitoring and launcher automation stop when you exit."};dialog.Controls.Add(message);
                var leave=new Button{Text=running?"Keep server running":"Close launcher only",Bounds=new Rectangle(20,145,175,36),DialogResult=DialogResult.No};
                var stop=new Button{Text="Stop server and exit",Bounds=new Rectangle(205,145,175,36),DialogResult=DialogResult.Yes,Enabled=running};
                var cancel=new Button{Text="Cancel",Bounds=new Rectangle(390,145,155,36),DialogResult=DialogResult.Cancel};dialog.Controls.AddRange(new Control[]{leave,stop,cancel});dialog.CancelButton=cancel;dialog.AcceptButton=cancel;
                var result=dialog.ShowDialog(owner);return result==DialogResult.No?CloseChoice.LeaveRunning:result==DialogResult.Yes&&running?CloseChoice.StopAndExit:CloseChoice.Cancel;
            }
        }
    }
}
