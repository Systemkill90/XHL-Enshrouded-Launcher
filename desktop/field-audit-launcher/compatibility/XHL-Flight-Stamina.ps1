# Exact build 1269579: zero only stamina_depletion_glider contribution; no general stamina refill.
param(
 [Parameter(Mandatory=$true)][int]$TargetProcessId,
 [ValidateSet('Enable','Disable','Status')][string]$Action='Status'
)
$ErrorActionPreference='Stop'
if(-not [Environment]::Is64BitProcess){throw '64-bit PowerShell required.'}
$p=Get-Process -Id $TargetProcessId -ErrorAction Stop
if($p.ProcessName -ne 'enshrouded_server'){throw 'Expected Enshrouded dedicated server.'}
$root=Split-Path -Parent $MyInvocation.MyCommand.Path
$exe=$p.MainModule.FileName
if(-not [string]::Equals([IO.Path]::GetFullPath($exe),[IO.Path]::GetFullPath((Join-Path $root 'enshrouded_server.exe')),[StringComparison]::OrdinalIgnoreCase)){throw 'Different server installation; refused.'}
if(-not (Test-Path -LiteralPath (Join-Path $root 'XHL-SAFE-MODE.flag') -PathType Leaf)){throw 'Safe mode flag missing.'}
foreach($name in @('winmm.dll','dinput8.dll','dbghelp.dll')){if(Test-Path -LiteralPath (Join-Path $root $name)){throw 'Incompatible loader present; refused.'}}
if((Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash -ne 'B627639CE4A3F8C940FE90AFA1D36ED59DB3FF9CED24E935890EB7C99B04D5BF'){throw 'Unrecognized build; no changes.'}
$src=@'
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
public static class XhlFlightStamina1269579 {
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
 static readonly byte[] Original={0xf3,0x0f,0x59,0xf7};
 static readonly byte[] Hold={0x0f,0x57,0xf6,0x90};
 static bool Eq(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;i++)if(a[i]!=b[i])return false;return true;}
 static byte[] Read(IntPtr h,long a,int n){var b=new byte[n];UIntPtr r;if(!ReadProcessMemory(h,new IntPtr(a),b,(UIntPtr)(uint)n,out r)||r.ToUInt64()!=(ulong)n)throw new Win32Exception();return b;}
 static void Require(IntPtr h,long a,byte[] b){if(!Eq(Read(h,a,b.Length),b))throw new InvalidOperationException("Instruction signature mismatch; refused.");}
 static void CheckThreads(int pid,long site){
  using(var p=Process.GetProcessById(pid))foreach(ProcessThread t in p.Threads){
   IntPtr th=OpenThread(8,false,(uint)t.Id);if(th==IntPtr.Zero)throw new Win32Exception();
   IntPtr raw=Marshal.AllocHGlobal(1248);long aligned=(raw.ToInt64()+15)&~15L;IntPtr ctx=new IntPtr(aligned);
   try{Marshal.Copy(new byte[1232],0,ctx,1232);Marshal.WriteInt32(ctx,48,0x100001);if(!GetThreadContext(th,ctx))throw new Win32Exception();long rip=Marshal.ReadInt64(ctx,248);if(rip>=site&&rip<site+4)throw new InvalidOperationException("Thread at patch site; retry later.");}
   finally{Marshal.FreeHGlobal(raw);CloseHandle(th);}
  }
 }
 public static string Control(int pid,long imageBase,string action){
  long site=imageBase+0xd818f;bool mutate=action!="Status";
  IntPtr h=OpenProcess(mutate?0x1838u:0x1010u,false,pid);if(h==IntPtr.Zero)throw new Win32Exception();bool suspended=false;
  try{
   if(mutate){if(NtSuspendProcess(h)!=0)throw new InvalidOperationException("Cannot pause process; refused.");suspended=true;CheckThreads(pid,site);}
   var current=Read(h,site,4);bool enabled=Eq(current,Hold);if(!enabled&&!Eq(current,Original))throw new InvalidOperationException("Unknown glider-stamina bytes; refused.");
   // Symbol registration binds this routine specifically to stamina_depletion_glider.
   Require(h,imageBase+0x13699c0,BitConverter.GetBytes(imageBase+0xd7f40));
   Require(h,imageBase+0x136cb90,System.Text.Encoding.ASCII.GetBytes("stamina_depletion_glider\0"));
   var routine=Read(h,imageBase+0xd7f40,0x312);Array.Copy(Original,0,routine,0x24f,4);
   using(var sha=System.Security.Cryptography.SHA256.Create())if(BitConverter.ToString(sha.ComputeHash(routine)).Replace("-","")!="716BD4DB785D59DF8D7629CE82E8CB8635B9558A411B742B76B0A3E8187A78C3")throw new InvalidOperationException("Glider-stamina routine changed; refused.");
   if(!mutate)return enabled?"ACTIVE: zero gliding-drain instruction bytes verified.":"INACTIVE: original gliding-drain instruction bytes verified.";
   var wanted=action=="Enable"?Hold:Original;if(Eq(current,wanted))return "Already in requested state; verified.";
   uint protection;if(!VirtualProtectEx(h,new IntPtr(site),(UIntPtr)4,0x40,out protection))throw new Win32Exception();
   try{
    try{UIntPtr wrote;if(!WriteProcessMemory(h,new IntPtr(site),wanted,(UIntPtr)4,out wrote)||wrote.ToUInt64()!=4)throw new Win32Exception();if(!FlushInstructionCache(h,new IntPtr(site),(UIntPtr)4))throw new Win32Exception();Require(h,site,wanted);}
    catch{UIntPtr restored;if(!WriteProcessMemory(h,new IntPtr(site),current,(UIntPtr)4,out restored)||restored.ToUInt64()!=4)throw new InvalidOperationException("ROLLBACK FAILED.");FlushInstructionCache(h,new IntPtr(site),(UIntPtr)4);Require(h,site,current);throw;}
   }finally{uint ignored;if(!VirtualProtectEx(h,new IntPtr(site),(UIntPtr)4,protection,out ignored))throw new InvalidOperationException("Code protection restoration failed.");}
   return action=="Enable"?"ACTIVE: zero gliding drain applied and read back; gameplay validation pending.":"INACTIVE: original gliding drain restored and verified.";
  }finally{if(suspended&&NtResumeProcess(h)!=0)throw new InvalidOperationException("PROCESS RESUME FAILED.");CloseHandle(h);}
 }
}
'@
Add-Type -TypeDefinition $src -ErrorAction Stop
[XhlFlightStamina1269579]::Control($TargetProcessId,$p.MainModule.BaseAddress.ToInt64(),$Action)
