using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
namespace XHLLauncher {
 internal sealed class RealmInstance : IDisposable {
  readonly Mutex mutex; readonly EventWaitHandle activation; readonly bool owns;
  internal bool IsPrimary { get { return owns; } }
  internal RealmInstance(string root) {
   string normalized=Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar).ToUpperInvariant();
   string identity;using(var sha=SHA256.Create())identity=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(normalized))).Replace("-","");
   activation=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\ShroudForge.Activate."+identity);
   bool created;mutex=new Mutex(true,"Local\\XHL.Realm."+identity,out created);owns=created;
   if(!owns)activation.Set();
  }
  internal bool ConsumeActivation(){return owns && activation.WaitOne(0);}
  public void Dispose(){if(owns)mutex.ReleaseMutex();mutex.Dispose();activation.Dispose();}
 }
}
