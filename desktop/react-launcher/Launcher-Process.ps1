# Close only the selected launcher normally. Never terminate a process or control the game.
function Close-SelectedLauncher {
 param([Parameter(Mandatory=$true)][string]$Executable,[ValidateRange(1,60)][int]$TimeoutSeconds=15)
 $Executable=[IO.Path]::GetFullPath($Executable)
 foreach($process in Get-Process 'ShroudForge-Launcher' -ErrorAction SilentlyContinue){
  try {
   if($process.Path -ne $Executable){continue}
   $requested=$process.CloseMainWindow()
   if(!$requested){
    if(!('ShroudForgeInstaller.NormalClose' -as [type])){
     Add-Type @'
using System;using System.Text;using System.Runtime.InteropServices;
namespace ShroudForgeInstaller {
 public static class NormalClose {
  delegate bool Callback(IntPtr window,IntPtr state);
  [DllImport("user32.dll")]static extern bool EnumWindows(Callback callback,IntPtr state);
  [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr window,out uint pid);
  [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern int GetWindowText(IntPtr window,StringBuilder text,int count);
  [DllImport("user32.dll")]static extern bool PostMessage(IntPtr window,uint message,IntPtr w,IntPtr l);
  public static bool Request(int pid){
   bool sent=false;
   EnumWindows((window,state)=>{
    uint owner;GetWindowThreadProcessId(window,out owner);if(owner!=(uint)pid)return true;
    var title=new StringBuilder(512);GetWindowText(window,title,512);
    if(title.ToString()!="ShroudForge "+(char)0x2014+" Control Center")return true;
    sent=PostMessage(window,0x0010,IntPtr.Zero,IntPtr.Zero);return false;
   },IntPtr.Zero);
   return sent;
  }
 }
}
'@
    }
    $requested=[ShroudForgeInstaller.NormalClose]::Request($process.Id)
   }
   if(!$requested -or !$process.WaitForExit($TimeoutSeconds*1000)){throw 'Launcher is busy or could not close normally. No files replaced. Finish its operation and retry; no force-close was attempted.'}
  } finally {$process.Dispose()}
 }
}
