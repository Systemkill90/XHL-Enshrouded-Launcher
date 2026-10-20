param(
 [Parameter(Mandatory=$true)][int]$TargetProcessId,
 [ValidateSet('Enable','Disable','Status')][string]$Action='Status'
)
$ErrorActionPreference='Stop'
if(-not [Environment]::Is64BitProcess){throw '64-bit PowerShell required.'}
$p=Get-Process -Id $TargetProcessId -ErrorAction Stop
if($p.ProcessName -ne 'enshrouded_server'){throw 'Expected Enshrouded dedicated server.'}
$exe=$p.MainModule.FileName
if((Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash -ne 'B627639CE4A3F8C940FE90AFA1D36ED59DB3FF9CED24E935890EB7C99B04D5BF'){throw 'Unrecognized build; no changes.'}
$src=@'
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
public static class XhlPitchHold1269579 {
 [DllImport("kernel32.dll",SetLastError=true)] static extern IntPtr OpenProcess(uint a,bool b,int p);
 [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool ReadProcessMemory(IntPtr h,IntPtr a,byte[] b,UIntPtr n,out UIntPtr r);
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool WriteProcessMemory(IntPtr h,IntPtr a,byte[] b,UIntPtr n,out UIntPtr r);
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool VirtualProtectEx(IntPtr h,IntPtr a,UIntPtr n,uint p,out uint old);
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool FlushInstructionCache(IntPtr h,IntPtr a,UIntPtr n);
 [DllImport("ntdll.dll")] static extern int NtSuspendProcess(IntPtr h);
 [DllImport("ntdll.dll")] static extern int NtResumeProcess(IntPtr h);
 [DllImport("kernel32.dll",SetLastError=true)] static extern IntPtr OpenThread(uint a,bool b,uint t);
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool GetThreadContext(IntPtr t,IntPtr c);
 static readonly byte[] Original={0x66,0x44,0x0f,0x6e,0xc2};
 static readonly byte[] Hold={0xeb,0x0e,0x90,0x90,0x90};
 static bool Eq(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;i++)if(a[i]!=b[i])return false;return true;}
 static byte[] Read(IntPtr h,long a,int n){var b=new byte[n];UIntPtr r;if(!ReadProcessMemory(h,new IntPtr(a),b,(UIntPtr)(uint)n,out r)||r.ToUInt64()!=(ulong)n)throw new Win32Exception();return b;}
 static void Require(IntPtr h,long a,byte[] b){if(!Eq(Read(h,a,b.Length),b))throw new InvalidOperationException("Instruction signature mismatch; refused.");}
 static void CheckThreads(int pid,long site){
  using(var p=Process.GetProcessById(pid))foreach(ProcessThread t in p.Threads){
   IntPtr th=OpenThread(8,false,(uint)t.Id);if(th==IntPtr.Zero)throw new Win32Exception();
   IntPtr raw=Marshal.AllocHGlobal(1248);long aligned=(raw.ToInt64()+15)&~15L;IntPtr ctx=new IntPtr(aligned);
   try{Marshal.Copy(new byte[1232],0,ctx,1232);Marshal.WriteInt32(ctx,48,0x100001);if(!GetThreadContext(th,ctx))throw new Win32Exception();long rip=Marshal.ReadInt64(ctx,248);if(rip>=site&&rip<site+5)throw new InvalidOperationException("Thread at patch site; retry later.");}
   finally{Marshal.FreeHGlobal(raw);CloseHandle(th);}
  }
 }
 public static string Control(int pid,long imageBase,string action){
  long site=imageBase+0x7cd8b;bool mutate=action!="Status";
  IntPtr h=OpenProcess(mutate?0x1838u:0x1010u,false,pid);if(h==IntPtr.Zero)throw new Win32Exception();bool suspended=false;
  try{
   if(mutate){if(NtSuspendProcess(h)!=0)throw new InvalidOperationException("Cannot pause process; refused.");suspended=true;CheckThreads(pid,site);}
   Require(h,imageBase+0x7cd82,new byte[]{0x84,0xc0,0x74,0x15,0x83,0xf9,0x01,0x75,0x31});
   Require(h,imageBase+0x7cd90,new byte[]{0xf3,0x44,0x0f,0x59,0x05,0x4b,0x18,0xb5,0x00,0xeb,0x21});
   Require(h,imageBase+0x7cd9b,new byte[]{0xf3,0x44,0x0f,0x10,0x87,0xd4,0x04,0x00,0x00,0xf3,0x44,0x0f,0x59,0xc1,0x66,0x0f,0x6e,0xc2,0xf3,0x44,0x0f,0x59,0xc0,0xf3,0x45,0x0f,0x59,0xc2,0xf3,0x45,0x0f,0x58,0xc6});
   var current=Read(h,site,5);bool enabled=Eq(current,Hold);if(!enabled&&!Eq(current,Original))throw new InvalidOperationException("Unknown pitch bytes; refused.");
   if(!mutate)return enabled?"ACTIVE: pitch-hold instruction bytes verified.":"INACTIVE: original pitch instructions verified.";
   var wanted=action=="Enable"?Hold:Original;if(Eq(current,wanted))return "Already in requested state; verified.";
   uint protection;if(!VirtualProtectEx(h,new IntPtr(site),(UIntPtr)5,0x40,out protection))throw new Win32Exception();
   try{
    try{UIntPtr wrote;if(!WriteProcessMemory(h,new IntPtr(site),wanted,(UIntPtr)5,out wrote)||wrote.ToUInt64()!=5)throw new Win32Exception();if(!FlushInstructionCache(h,new IntPtr(site),(UIntPtr)5))throw new Win32Exception();Require(h,site,wanted);}
    catch{UIntPtr restored;if(!WriteProcessMemory(h,new IntPtr(site),current,(UIntPtr)5,out restored)||restored.ToUInt64()!=5)throw new InvalidOperationException("ROLLBACK FAILED.");FlushInstructionCache(h,new IntPtr(site),(UIntPtr)5);Require(h,site,current);throw;}
   }finally{uint ignored;if(!VirtualProtectEx(h,new IntPtr(site),(UIntPtr)5,protection,out ignored))throw new InvalidOperationException("Code protection restoration failed.");}
   return action=="Enable"?"ACTIVE: pitch hold applied and read back; gameplay validation pending.":"INACTIVE: original pitch behavior restored and verified.";
  }finally{if(suspended&&NtResumeProcess(h)!=0)throw new InvalidOperationException("PROCESS RESUME FAILED.");CloseHandle(h);}
 }
}
'@
Add-Type -TypeDefinition $src -ErrorAction Stop
[XhlPitchHold1269579]::Control($TargetProcessId,$p.MainModule.BaseAddress.ToInt64(),$Action)
