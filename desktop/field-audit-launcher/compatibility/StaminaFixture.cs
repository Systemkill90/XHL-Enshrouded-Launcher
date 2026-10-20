using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
class StaminaFixture {
 [DllImport("kernel32.dll",SetLastError=true)] static extern IntPtr VirtualAlloc(IntPtr a,UIntPtr n,uint t,uint p);
 [DllImport("kernel32.dll")] static extern bool VirtualFree(IntPtr a,UIntPtr n,uint t);
 [DllImport("kernel32.dll",SetLastError=true)] static extern IntPtr OpenProcess(uint a,bool b,int p);
 [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool WriteProcessMemory(IntPtr h,IntPtr a,byte[] b,UIntPtr n,out UIntPtr w);
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool ReadProcessMemory(IntPtr h,IntPtr a,byte[] b,UIntPtr n,out UIntPtr r);
 static int checks;
 static void Require(bool v,string name){if(!v)throw new Exception(name);checks++;Console.WriteLine("PASS "+name);}
 static void Put(int pid,long address,byte[] bytes){IntPtr h=OpenProcess(0x38,false,pid);try{UIntPtr n;if(h==IntPtr.Zero||!WriteProcessMemory(h,new IntPtr(address),bytes,(UIntPtr)(uint)bytes.Length,out n)||n.ToUInt64()!=(ulong)bytes.Length)throw new Exception("Fixture write failed");}finally{if(h!=IntPtr.Zero)CloseHandle(h);}}
 static byte[] Get(int pid,long address,int length){IntPtr h=OpenProcess(0x10,false,pid);try{var b=new byte[length];UIntPtr n;if(h==IntPtr.Zero||!ReadProcessMemory(h,new IntPtr(address),b,(UIntPtr)(uint)length,out n)||n.ToUInt64()!=(ulong)length)throw new Exception("Fixture read failed");return b;}finally{if(h!=IntPtr.Zero)CloseHandle(h);}}
 static bool Rejected(Action f){try{f();return false;}catch(InvalidOperationException){return true;}}
 static int Main(string[] args){string root=Path.GetFullPath(args[0]);try{
  if(args.Length>1){
   byte[] data=File.ReadAllBytes(Path.Combine(root,"image.bin"));IntPtr memory=VirtualAlloc(IntPtr.Zero,(UIntPtr)(uint)data.Length,0x3000,0x04);if(memory==IntPtr.Zero)throw new Exception("Fixture allocation failed");
   try{Marshal.Copy(data,0,memory,data.Length);Marshal.WriteInt64(memory,0x13699c0,memory.ToInt64()+0xd7f40);File.WriteAllText(Path.Combine(root,"ready.txt"),memory.ToInt64().ToString());
    DateTime deadline=DateTime.UtcNow.AddSeconds(40);int tick=0;while(!File.Exists(Path.Combine(root,"stop.txt"))&&DateTime.UtcNow<deadline){File.WriteAllText(Path.Combine(root,"heartbeat.txt"),(tick++).ToString());Thread.Sleep(30);}
   }finally{VirtualFree(memory,UIntPtr.Zero,0x8000);}return 0;
  }
  var info=new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName,"\""+root+"\" child"){UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden};
  using(var child=Process.Start(info)){try{
   DateTime deadline=DateTime.UtcNow.AddSeconds(8);while(!File.Exists(Path.Combine(root,"ready.txt"))&&DateTime.UtcNow<deadline)Thread.Sleep(30);
   long b=long.Parse(File.ReadAllText(Path.Combine(root,"ready.txt")));int pid=child.Id;
   Require(XhlFlightStamina1269579.Control(pid,b,"Status").StartsWith("INACTIVE"),"Original instruction state verified");
   Require(XhlFlightStamina1269579.Control(pid,b,"Enable").StartsWith("ACTIVE"),"Apply to disposable process and read back");
   Require(BitConverter.ToString(Get(pid,b+0xd818f,4))=="0F-57-F6-90","Exact four replacement bytes");
   Require(XhlFlightStamina1269579.Control(pid,b,"Enable").Contains("Already"),"Enable idempotent");
   Require(XhlFlightStamina1269579.Control(pid,b,"Status").StartsWith("ACTIVE"),"Active state verified");
   Require(XhlFlightStamina1269579.Control(pid,b,"Disable").StartsWith("INACTIVE"),"Disable restores original behavior");
   Require(BitConverter.ToString(Get(pid,b+0xd818f,4))=="F3-0F-59-F7","Exact original bytes restored");
   Require(XhlFlightStamina1269579.Control(pid,b,"Disable").Contains("Already"),"Disable idempotent");
   Put(pid,b+0xd818f,new byte[]{0x90,0x90,0x90,0x90});Require(Rejected(()=>XhlFlightStamina1269579.Control(pid,b,"Enable")),"Unknown patch bytes rejected");Put(pid,b+0xd818f,new byte[]{0xf3,0x0f,0x59,0xf7});
   byte original=Get(pid,b+0xd7f40,1)[0];Put(pid,b+0xd7f40,new byte[]{0x90});Require(Rejected(()=>XhlFlightStamina1269579.Control(pid,b,"Enable")),"Changed surrounding routine rejected");Put(pid,b+0xd7f40,new byte[]{original});
   Put(pid,b+0x13699c0,BitConverter.GetBytes(b+0xd8260));Require(Rejected(()=>XhlFlightStamina1269579.Control(pid,b,"Enable")),"Wrong system registration rejected");Put(pid,b+0x13699c0,BitConverter.GetBytes(b+0xd7f40));
   string heartbeat=File.ReadAllText(Path.Combine(root,"heartbeat.txt"));Thread.Sleep(120);Require(heartbeat!=File.ReadAllText(Path.Combine(root,"heartbeat.txt")),"Process resumed after every rejection");
   Require(XhlFlightStamina1269579.Control(pid,b,"Status").StartsWith("INACTIVE"),"Final state original after negative tests");
   File.WriteAllText(Path.Combine(root,"result.txt"),checks+" native checks passed.");
  }finally{File.WriteAllText(Path.Combine(root,"stop.txt"),"stop");}
  if(!child.WaitForExit(10000))throw new Exception("Fixture did not exit");Require(child.ExitCode==0,"Fixture exited cleanly");}
  Console.WriteLine(checks+" native checks passed.");return 0;
 }catch(Exception ex){Console.WriteLine(ex);return 1;}}
}
