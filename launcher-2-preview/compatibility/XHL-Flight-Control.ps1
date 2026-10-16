# Flight-v2: fixed -80/+80 degree target bounds for build 1269579. No speed/stamina changes.
param(
    [Parameter(Mandatory=$true)]
    [int]$TargetProcessId,
    [ValidateSet('Enable','Disable','Status')]
    [string]$Action = 'Status'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$exe = Join-Path $root 'enshrouded_server.exe'
$expectedHash = 'B627639CE4A3F8C940FE90AFA1D36ED59DB3FF9CED24E935890EB7C99B04D5BF'

# XHL 1.0.9 Flight - Enshrouded Game SVN 1269579 (pinned executable only).
# Adapted from s0t7x/shroudtopia example-mods/flight_mod/dllmain.cpp logic:
# the old module detours a movss loading a positive minimum glider pitch (~5 degrees),
# and replaces that source float with -1.57 rad. We instead redirect only the
# original movss rel32 operand to a pre-existing -80 degree (-80 deg) constant
# already present in the same game executable. No DLL, shellcode, game archive
# editing, or process allocation is needed. Status confirms MEMORY ONLY;
# gameplay must be tested with unmodified clients before stating 'working'.
if (-not [Environment]::Is64BitProcess) {throw '64-bit PowerShell is required.'}
if (-not (Test-Path -LiteralPath (Join-Path $root 'XHL-SAFE-MODE.flag') -PathType Leaf)) {
  throw 'XHL safe start flag missing; flight patch refused.'
}
foreach($n in @('winmm.dll','dinput8.dll','dbghelp.dll')) {
  if(Test-Path -LiteralPath (Join-Path $root $n) -PathType Leaf) {
    throw ('Incompatible native loader present: '+$n+'; flight patch refused.')
  }
}
if(-not (Test-Path -LiteralPath (Join-Path $root 'XHL-STACK-1.0-ATTESTATION.json') -PathType Leaf)) {
  throw 'Stacking attestation missing; avoid modifying unexpected server.'
}
if ((Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash.ToUpperInvariant() -ne $expectedHash) {
  throw 'Unrecognized server executable SHA256. Enshrouded updated; no memory changes.'
}
$p=Get-Process -Id $TargetProcessId -ErrorAction Stop
$p.Refresh()
if($p.HasExited -or $p.ProcessName -ne 'enshrouded_server'){throw 'Target process is not running Enshrouded.'}
if (-not [string]::Equals([IO.Path]::GetFullPath($p.MainModule.FileName),[IO.Path]::GetFullPath($exe),[StringComparison]::OrdinalIgnoreCase)) {
  throw 'PID points to a different Enshrouded installation; refusing.'
}
$src=@'
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
public static class XhlGliderFlight1269579 {
  const uint VM_OPERATION=0x8, VM_READ=0x10, VM_WRITE=0x20, QUERY=0x1000, EXECUTE_READWRITE=0x40;
  [DllImport("kernel32.dll",SetLastError=true)] static extern IntPtr OpenProcess(uint access, bool inherit, int pid);
  [DllImport("kernel32.dll",SetLastError=true)] static extern bool CloseHandle(IntPtr handle);
  [DllImport("kernel32.dll",SetLastError=true)] static extern bool ReadProcessMemory(IntPtr h, IntPtr a, [Out] byte[] data, UIntPtr n, out UIntPtr read);
  [DllImport("kernel32.dll",SetLastError=true)] static extern bool WriteProcessMemory(IntPtr h, IntPtr a, byte[] data, UIntPtr n, out UIntPtr wrote);
  [DllImport("kernel32.dll",SetLastError=true)] static extern bool VirtualProtectEx(IntPtr h, IntPtr a, UIntPtr n, uint protection, out uint oldProtection);
  [DllImport("kernel32.dll",SetLastError=true)] static extern bool FlushInstructionCache(IntPtr h, IntPtr a, UIntPtr n);
  static void Fail(string action) { throw new Win32Exception(Marshal.GetLastWin32Error(),action); }
  static IntPtr Ptr(long addr) { return new IntPtr(addr); }
  static byte[] Read(IntPtr p, long a, int len) {
    byte[] b=new byte[len]; UIntPtr read;
    if(!ReadProcessMemory(p,Ptr(a),b,(UIntPtr)(uint)len,out read)||read.ToUInt64()!=(ulong)len)Fail("ReadProcessMemory");
    return b;
  }
  static bool Equal(byte[] a,byte[] b) {
    if(a.Length!=b.Length)return false;
    for(int i=0;i<a.Length;i++)if(a[i]!=b[i])return false;
    return true;
  }
  static void Require(IntPtr p,long a,byte[] bytes,string what){
    if(!Equal(Read(p,a,bytes.Length),bytes))throw new InvalidOperationException("Signature mismatch at "+what+"; no change made.");
  }
  public static string Control(int pid,long imageBase,string action){
    // Strictly pinned for SHA256 B627639C.. (the Enshrouded 1.0 server supplied by operator).
    // Original source: movss xmm0,[RIP+0xB5265E], followed by movsd [rsp+0x40],xmm1.
    // Redirect only disp32 to an existing -80 degree float. No opcode or JMP modifications.
    long hook=imageBase+0x7CF5EL;
    long oldFloat=imageBase+0xBCF5C4L;
    long newFloat=imageBase+0xBD893CL;
    byte[] original={0xF3,0x0F,0x10,0x05,0x5E,0x26,0xB5,0x00,0xF2,0x0F,0x11,0x4C,0x24,0x40,0x77,0x15};
    byte[] patched={0xF3,0x0F,0x10,0x05,0xD6,0xB9,0xB5,0x00,0xF2,0x0F,0x11,0x4C,0x24,0x40,0x77,0x15};
    byte[] oldDisp={0x5E,0x26,0xB5,0x00},newDisp={0xD6,0xB9,0xB5,0x00};
    IntPtr h=OpenProcess(action=="Status"?(VM_READ|QUERY):(VM_OPERATION|VM_READ|VM_WRITE|QUERY),false,pid);
    if(h==IntPtr.Zero)Fail("OpenProcess");
    try{
      Require(h,oldFloat,new byte[]{0xC2,0xB8,0xB2,0x3D},"original glider 5 degree constant");
      Require(h,newFloat,new byte[]{0xC2,0xB8,0xB2,0xBF},"existing -80 degree glider float");
      Require(h,imageBase+0xBCE5DCL,new byte[]{0xC2,0xB8,0xB2,0x3F},"existing positive 80 degree bound");
      Require(h,hook+16,new byte[]{0x41,0x0F,0x2F,0xC0,0x77,0x0F,0xF3,0x0F,0x10,0x35,0x60,0x16,0xB5,0x00,0xF3,0x41,0x0F,0x5D,0xF0,0xEB,0x03,0x0F,0x28,0xF0},"pitch clamp branches and upper bound");
      byte[] current=Read(h,hook,16);
      bool isOriginal=Equal(current,original), isPatched=Equal(current,patched);
      if(!isOriginal&&!isPatched)throw new InvalidOperationException("Glider hook has changed or belongs to another mod; refusing.");
      if(action=="Status")return isPatched?"ACTIVE - Glider memory patch verified. In-game flight still needs testing.":"INACTIVE - Original glider bytes verified.";
      if(action=="Enable"&&isPatched)return "ACTIVE - Glider memory bytes verified. Test pitch and ascent in game.";
      if(action=="Disable"&&isOriginal)return "INACTIVE - Original glider bytes already restored.";
      bool enable=action=="Enable";
      byte[] wanted=enable?patched:original;
      byte[] replacement=enable?newDisp:oldDisp;
      byte[] rollback=enable?oldDisp:newDisp;
      long field=hook+4;
      uint before;
      if(!VirtualProtectEx(h,Ptr(field),(UIntPtr)4,EXECUTE_READWRITE,out before))Fail("VirtualProtectEx");
      bool verified=false;
      try{
        UIntPtr wrote;
        if(!WriteProcessMemory(h,Ptr(field),replacement,(UIntPtr)4,out wrote)||wrote.ToUInt64()!=4)Fail("WriteProcessMemory");
        if(!FlushInstructionCache(h,Ptr(hook),(UIntPtr)16))Fail("FlushInstructionCache");
        verified=Equal(Read(h,hook,16),wanted);
        if(!verified)throw new InvalidOperationException("Flight write could not be verified; restoring original state.");
      }finally{
        if(!verified){UIntPtr restored; WriteProcessMemory(h,Ptr(field),rollback,(UIntPtr)4,out restored);FlushInstructionCache(h,Ptr(hook),(UIntPtr)16);}
        uint unused; if(!VirtualProtectEx(h,Ptr(field),(UIntPtr)4,before,out unused))throw new InvalidOperationException("Cannot restore code-page protection.");
      }
      return enable?"ACTIVE - Verified Enshrouded 1.0 Flight instruction patch in process. Confirm ascent in-game.":"INACTIVE - Verified original glider instruction restored.";
    }finally {CloseHandle(h);}
  }
}
'@
Add-Type -TypeDefinition $src -ErrorAction Stop
[XhlGliderFlight1269579]::Control($TargetProcessId,[long]$p.MainModule.BaseAddress.ToInt64(),$Action)
