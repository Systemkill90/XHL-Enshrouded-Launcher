param(
    [Parameter(Mandatory=$true)]
    [int]$TargetProcessId
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
# XHL-1.1.0 GIFTING PREFERENCE GUARD (original verified memory patch follows unchanged).
if (Test-Path -LiteralPath (Join-Path $root 'XHL-Mods-1.0\Gifting\disabled.flag') -PathType Leaf) {
    Write-Output 'INACTIVE: Gifting disabled through XHL.'
    return
}

$exe = Join-Path $root 'enshrouded_server.exe'
$knownSha256 = 'B627639CE4A3F8C940FE90AFA1D36ED59DB3FF9CED24E935890EB7C99B04D5BF'

# XHL Enshrouded 1.0 gifting range (experimental runtime patch).
# This never edits the EXE on disk. Only apply while XHL safe-mode has removed all
# legacy loader DLLs and the original world has been backed up by XHL-Safe-Start.
if (-not [Environment]::Is64BitProcess) { throw 'XHL gifting patch requires 64-bit PowerShell.' }
if (-not (Test-Path -LiteralPath (Join-Path $root 'XHL-SAFE-MODE.flag') -PathType Leaf)) {
    throw 'Gifting patch refuses to run without XHL safe mode.'
}
foreach ($n in @('dinput8.dll','winmm.dll','dbghelp.dll')) {
    if (Test-Path -LiteralPath (Join-Path $root $n) -PathType Leaf) {
        throw ('Legacy loader still active: ' + $n)
    }
}
$actualSha256 = (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash.ToUpperInvariant()
if ($actualSha256 -ne $knownSha256) {
    throw ('UNRECOGNIZED Enshrouded server EXE (SHA256 ' + $actualSha256 + '). No patch applied.')
}
$p = Get-Process -Id $TargetProcessId -ErrorAction Stop
$p.Refresh()
if ($p.HasExited -or $p.ProcessName -ne 'enshrouded_server') {
    throw 'Enshrouded target process exited or has an unexpected name.'
}
$actualExe = [IO.Path]::GetFullPath($p.MainModule.FileName)
if (-not [string]::Equals($actualExe, [IO.Path]::GetFullPath($exe), [StringComparison]::OrdinalIgnoreCase)) {
    throw ('Process EXE path does not match this server directory: ' + $actualExe)
}

$csharp = @'
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
public static class XhlGiftingRangeOneZero {
    const uint PROCESS_VM_OPERATION=0x0008, PROCESS_VM_READ=0x0010, PROCESS_VM_WRITE=0x0020;
    const uint PROCESS_QUERY_LIMITED_INFORMATION=0x1000, PAGE_EXECUTE_READWRITE=0x40;
    [DllImport("kernel32.dll", SetLastError=true)]
    static extern IntPtr OpenProcess(uint access, bool inherit, int processId);
    [DllImport("kernel32.dll", SetLastError=true)]
    static extern bool CloseHandle(IntPtr handle);
    [DllImport("kernel32.dll", SetLastError=true)]
    static extern bool ReadProcessMemory(IntPtr process, IntPtr address, [Out] byte[] data, UIntPtr count, out UIntPtr read);
    [DllImport("kernel32.dll", SetLastError=true)]
    static extern bool WriteProcessMemory(IntPtr process, IntPtr address, byte[] data, UIntPtr count, out UIntPtr written);
    [DllImport("kernel32.dll", SetLastError=true)]
    static extern bool VirtualProtectEx(IntPtr process, IntPtr address, UIntPtr count, uint protection, out uint priorProtection);
    [DllImport("kernel32.dll", SetLastError=true)]
    static extern bool FlushInstructionCache(IntPtr process, IntPtr address, UIntPtr count);
    static IntPtr At(long addr) { return new IntPtr(addr); }
    static void WinFail(string operation) {
        throw new Win32Exception(Marshal.GetLastWin32Error(), operation);
    }
    static byte[] Read(IntPtr process, long addr, int length) {
        byte[] buf=new byte[length]; UIntPtr done;
        if (!ReadProcessMemory(process, At(addr), buf, (UIntPtr)(uint)length, out done) || done.ToUInt64() != (ulong)length)
            WinFail("ReadProcessMemory failed");
        return buf;
    }
    static bool Same(byte[] a,byte[] b) {
        if(a.Length!=b.Length)return false;
        for(int i=0;i<a.Length;i++)if(a[i]!=b[i])return false;
        return true;
    }
    static void MustEqual(IntPtr process, long addr, byte[] expected, string name) {
        if(!Same(Read(process, addr, expected.Length), expected))
            throw new InvalidOperationException("Build signature mismatch at " + name + "; refusing memory patch.");
    }
    public static string Apply(int pid, long imageBase) {
        // RVAs verified against SHA-256 B627639C... of Enshrouded build 1269579 (2026-10-16).
        // Getter at 0x931830 returns squared range 2500.0f and has one direct call
        // at 0x198389. Change only its RIP-relative displacement to a preexisting
        // FLT_MAX in read-only image data at 0xBCB4B8 (no writes to other data).
        long getter=imageBase+0x931830L, call=imageBase+0x198389L;
        long originalValue=imageBase+0xBD7204L, maxValue=imageBase+0xBCB4B8L;
        byte[] initial={0xF3,0x0F,0x10,0x05,0xCC,0x59,0x2A,0x00,0xC3};
        byte[] patched={0xF3,0x0F,0x10,0x05,0x80,0x9C,0x29,0x00,0xC3};
        byte[] oldDisp={0xCC,0x59,0x2A,0x00};
        byte[] newDisp={0x80,0x9C,0x29,0x00};
        IntPtr process=OpenProcess(PROCESS_VM_OPERATION|PROCESS_VM_READ|PROCESS_VM_WRITE|PROCESS_QUERY_LIMITED_INFORMATION,false,pid);
        if(process==IntPtr.Zero)WinFail("OpenProcess failed");
        try {
            MustEqual(process,call,new byte[]{0xE8,0xA2,0x94,0x79,0x00},"gifting distance call");
            MustEqual(process,originalValue,new byte[]{0x00,0x40,0x1C,0x45},"2,500.0 float constant");
            MustEqual(process,maxValue,new byte[]{0xFF,0xFF,0x7F,0x7F},"FLT_MAX float constant");
            byte[] seen=Read(process,getter,9);
            if(Same(seen,patched))return "ALREADY ACTIVE: Enshrouded 1.0 gifting range patch.";
            if(!Same(seen,initial))throw new InvalidOperationException("Gifting getter changed or patched by another mod. No changes made.");
            // Only four bytes change, at an aligned RVA. The function instruction
            // itself, its return opcode, and the game executable on disk are unchanged.
            long field=getter+4; uint previous;
            if(!VirtualProtectEx(process,At(field),(UIntPtr)4,PAGE_EXECUTE_READWRITE,out previous))
                WinFail("VirtualProtectEx failed");
            bool verified=false;
            try {
                UIntPtr written;
                if(!WriteProcessMemory(process,At(field),newDisp,(UIntPtr)4,out written) || written.ToUInt64()!=4)
                    WinFail("WriteProcessMemory failed");
                if(!FlushInstructionCache(process,At(getter),(UIntPtr)9))WinFail("FlushInstructionCache failed");
                verified=Same(Read(process,getter,9),patched);
                if(!verified)throw new InvalidOperationException("Patch verification failed; restoring old displacement.");
            } finally {
                if(!verified) {
                    UIntPtr restored;
                    WriteProcessMemory(process,At(field),oldDisp,(UIntPtr)4,out restored);
                    FlushInstructionCache(process,At(getter),(UIntPtr)9);
                }
                uint unused;
                if(!VirtualProtectEx(process,At(field),(UIntPtr)4,previous,out unused))
                    throw new InvalidOperationException("Cannot restore original memory page protection after patch.");
            }
            return "ACTIVE: Enshrouded 1.0 Unlimited Gifting Range (in-memory, single getter).";
        } finally { CloseHandle(process); }
    }
}
'@
Add-Type -TypeDefinition $csharp -ErrorAction Stop
$result = [XhlGiftingRangeOneZero]::Apply($TargetProcessId, [long]$p.MainModule.BaseAddress.ToInt64())
Write-Output $result
