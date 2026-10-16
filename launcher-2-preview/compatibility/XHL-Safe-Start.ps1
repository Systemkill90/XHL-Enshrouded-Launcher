param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$ServerArgs
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$exe = Join-Path $root 'enshrouded_server.exe'
$configFile = Join-Path $root 'enshrouded_server.json'
$flag = Join-Path $root 'XHL-SAFE-MODE.flag'
$manifestPath = Join-Path $root 'XHL-1.0-compat-manifest.json'
$backupBase = Join-Path $root 'WorldSafetySnapshots'
$logBase = Join-Path $root 'LauncherLogs'
function Say([string]$line) {
    Write-Host $line
    try { Add-Content -LiteralPath (Join-Path $logBase 'XHL-1.0-LAST-START.txt') -Value $line -Encoding UTF8 } catch { }
}
function Q([string]$s) {
    if ([string]::IsNullOrEmpty($s)) { return '""' }
    $b = '"' + ($s -replace '(\\*)"', '$1$1\"' -replace '(\\+)$', '$1$1') + '"'
    return $b
}
try {
    [void](New-Item -Path $logBase -ItemType Directory -Force)
    Set-Content -LiteralPath (Join-Path $logBase 'XHL-1.0-LAST-START.txt') -Value ('XHL 1.0 safe launch v1.0.7 - ' + (Get-Date).ToString('o')) -Encoding UTF8
    Say '[XHL-Safe-Start-v1.0.7] Existing server, legacy loaders isolated, no PowerShell DLL injection.'
    if (-not (Test-Path -LiteralPath $flag -PathType Leaf) -or -not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        throw 'Safe mode not fully enabled. Run ENABLE-XHL-1.0-SAFE-MODE.bat first.'
    }
    foreach ($n in @('dinput8.dll','winmm.dll','dbghelp.dll')) {
        if (Test-Path -LiteralPath (Join-Path $root $n) -PathType Leaf) {
            throw ('Local mod loader is STILL active: ' + $n + '. Refusing false safe mode.')
        }
    }
    if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) { throw 'Existing Enshrouded server EXE not found.' }
    if (-not (Test-Path -LiteralPath $configFile -PathType Leaf)) { throw 'Existing Enshrouded server configuration missing. Refusing to create new world.' }
    if (@(Get-Process -Name enshrouded_server -ErrorAction SilentlyContinue).Count -gt 0) {
        throw 'Another Enshrouded server is already running. Refusing to launch a duplicate.'
    }
    $cfg = Get-Content -LiteralPath $configFile -Raw -Encoding UTF8 | ConvertFrom-Json -ErrorAction Stop
    $saveEntry = [string]$cfg.saveDirectory
    if ([string]::IsNullOrWhiteSpace($saveEntry)) { $saveEntry = './savegame' }
    $world = if ([IO.Path]::IsPathRooted($saveEntry)) { [IO.Path]::GetFullPath($saveEntry) } else { [IO.Path]::GetFullPath((Join-Path $root $saveEntry)) }
    if (-not (Test-Path -LiteralPath $world -PathType Container)) { throw ('World directory missing: ' + $world + '. Refusing to create a new world.') }
    $saveFiles = @(Get-ChildItem -LiteralPath $world -Recurse -Force -File -ErrorAction Stop)
    if ($saveFiles.Count -lt 1) { throw 'Existing world folder is empty. Refusing to create a fresh world.' }
    $reparse = @(Get-ChildItem -LiteralPath $world -Recurse -Force -ErrorAction Stop | Where-Object { ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0 })
    if ($reparse.Count -gt 0) { throw 'World contains symlinks/reparse points. Backup procedure cannot safely follow them.' }
    $required = [long](($saveFiles | Measure-Object -Property Length -Sum).Sum) + 134217728
    [void](New-Item -ItemType Directory -Path $backupBase -Force)
    $drive = New-Object System.IO.DriveInfo ([IO.Path]::GetPathRoot($backupBase))
    if ($drive.AvailableFreeSpace -lt $required) { throw ('Insufficient free space for verified world copy in ' + $backupBase) }
    $dest = Join-Path $backupBase ((Get-Date).ToString('yyyyMMdd-HHmmss-fff') + '-' + [guid]::NewGuid().ToString('N').Substring(0,8))
    $saveDest = Join-Path $dest 'savegame'
    [void](New-Item -ItemType Directory -Force -Path $saveDest)
    Say ('[XHL-Safe-Start-v1.0.7] Backing up existing world from ' + $world)
    Say ('[XHL-Safe-Start-v1.0.7] Snapshot destination: ' + $dest)
    Copy-Item -LiteralPath $configFile -Destination (Join-Path $dest 'enshrouded_server.json') -ErrorAction Stop
    foreach ($entry in @(Get-ChildItem -LiteralPath $world -Force -ErrorAction Stop)) {
        Copy-Item -LiteralPath $entry.FullName -Destination $saveDest -Recurse -Force -ErrorAction Stop
    }
    $prefix = $world.TrimEnd('\','/').Length + 1
    foreach ($f in $saveFiles) {
        $rel = $f.FullName.Substring($prefix)
        $copy = Join-Path $saveDest $rel
        if (-not (Test-Path -LiteralPath $copy -PathType Leaf)) { throw ('World backup missing ' + $rel) }
        if ($f.Length -ne (Get-Item -LiteralPath $copy).Length -or
            (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash) {
            throw ('World backup verification failed for ' + $rel)
        }
    }
    $cfgHash = (Get-FileHash -LiteralPath $configFile -Algorithm SHA256).Hash
    $copyCfgHash = (Get-FileHash -LiteralPath (Join-Path $dest 'enshrouded_server.json') -Algorithm SHA256).Hash
    if ($cfgHash -ne $copyCfgHash) { throw 'Configuration copy failed SHA256 verification.' }
    Set-Content -LiteralPath (Join-Path $dest 'XHL-VERIFIED.txt') -Value @('World backup hash-verified before launching Enshrouded 1.0.',('Source: ' + $world),('Verified: ' + (Get-Date).ToString('o'))) -Encoding UTF8
    Say ('[XHL-Safe-Start-v1.0.7] VERIFIED world backup: ' + $dest)
    # v1.0.7: This world was previously hosted with an EML 65535-stack mod.
    # The Enshrouded 1.0 EML/KFC4 loader failed and is isolated in safe mode.
    # Never silently start this world under vanilla stack limits. This check
    # occurs AFTER the snapshot is hash verified, BEFORE starting the EXE.
    $stackGuard = Join-Path $root 'XHL-STACK-LOSS-GUARD.flag'
    $riskOverride = Join-Path $root 'XHL-STACK-LOSS-RISK-ACCEPTED.flag'
    # A staging-only offline EML patch is permitted ONLY when both current game
    # resource files exactly match tested patch hashes and the exact server EXE.
    # This does not enable any DLL loader in the live Enshrouded server process.
    $attestation = Join-Path $root 'XHL-STACK-1.0-ATTESTATION.json'
    $stackPatchValid = $false
    $stackPatchStale = $false
    if (Test-Path -LiteralPath $attestation -PathType Leaf) {
        try {
            $a = Get-Content -LiteralPath $attestation -Raw -Encoding UTF8 | ConvertFrom-Json -ErrorAction Stop
            if ($a.Format -ne 'XHL-EML-OFFLINE-STAGING-v1' -or $a.MaxStack -ne 65535) {
                throw 'Unexpected attestation format or stack size.'
            }
            foreach ($item in @(@{Name='enshrouded_server.exe';Hash=$a.ExeSha256},
                                   @{Name='enshrouded_server.kfc';Hash=$a.KfcSha256},
                                   @{Name='enshrouded_server.kfc_resources';Hash=$a.ResourcesSha256})) {
                $f = Join-Path $root $item.Name
                if (-not (Test-Path -LiteralPath $f -PathType Leaf)) { throw ('Missing ' + $item.Name) }
                if ((Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash -ne [string]$item.Hash) {
                    throw ('Game file changed after stacking patch: ' + $item.Name)
                }
            }
            $stackPatchValid = $true
            Say '[XHL-Stack-1.0] Verified 65535 stacking resource patch matches the tested files.'
        } catch {
            $stackPatchStale = $true
            Say ('[XHL-Stack-1.0] Offline patch attestation invalid: ' + $_.Exception.Message)
        }
    }
    if ($stackPatchStale) { throw 'Previously validated stacking resources changed. Refusing even with old vanilla-risk override.' }
    if (Test-Path -LiteralPath $stackGuard -PathType Leaf) {
        if (-not $stackPatchValid) {
            if (-not (Test-Path -LiteralPath $riskOverride -PathType Leaf)) {
                Say '[XHL-Stack-Guard] BLOCKED: No verified stacking patch. World previously used 65535 stacks.'
                throw 'Inventory safety block. Do not launch with vanilla stack limits. Rebuild/test the offline patch first.'
            }
            $ack = Get-Content -LiteralPath $riskOverride -Raw -ErrorAction Stop
            if ($ack.Trim() -ne 'I-UNDERSTAND-STACK-DATA-MAY-BE-LOST') {
                throw 'Invalid stack risk acknowledgement; no server process started.'
            }
            Say '[XHL-Stack-Guard] WARNING: Server starting with VANILLA stacking by explicit risk acceptance.'
        }
    }
    # Start the EXISTING exe and config in their original working directory.
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $exe
    $psi.WorkingDirectory = $root
    $psi.UseShellExecute = $true
    if ($ServerArgs -and $ServerArgs.Count -gt 0) { $psi.Arguments = (($ServerArgs | ForEach-Object { Q $_ }) -join ' ') }
    $p = [Diagnostics.Process]::Start($psi)
    if ($null -eq $p) { throw 'CreateProcess did not return a process.' }
    Say ('[XHL-Safe-Start-v1.0.7] Launched EXE with PID ' + $p.Id)
    # After Windows maps the exact verified 1.0 build, enable gifting range
    # without loading the incompatible pre-1.0 native plugin DLLs.
    Start-Sleep -Seconds 2
    $giftingScript = Join-Path $root 'XHL-Gifting-Range-1.0.ps1'
    if (Test-Path -LiteralPath $giftingScript -PathType Leaf) {
        try {
            $giftingResult = & $giftingScript -TargetProcessId $p.Id -ErrorAction Stop
            foreach ($line in @($giftingResult)) { Say ('[XHL-Gifting-1.0] ' + [string]$line) }
        } catch {
            Say ('[XHL-Gifting-1.0] NOT APPLIED: ' + $_.Exception.Message)
            Say '[XHL-Gifting-1.0] Server continues without the gifting modification.'
        }
    } else {
        Say '[XHL-Gifting-1.0] Patch script not installed; launching without gifting modification.'
    }
    Start-Sleep -Seconds 8
    $p.Refresh()
    if ($p.HasExited) { throw ('Server process exited during first 10 seconds; exit code=' + $p.ExitCode + '. Review server logs.') }
    # XHL-1.1.0 FLIGHT STARTUP HOOK: independently guarded; gifting code above is unchanged.
    $xhlFlightDisabled = Join-Path $root 'XHL-Mods-1.0\Flight\disabled.flag'
    if (Test-Path -LiteralPath $xhlFlightDisabled -PathType Leaf) {
        Say '[XHL-Flight-1.0] OFF: Flight disabled by operator.'
    } else {
        $xhlFlightScript = Join-Path $root 'XHL-Flight-Control.ps1'
        if (Test-Path -LiteralPath $xhlFlightScript -PathType Leaf) {
            try {
                $xhlFlightResult = & $xhlFlightScript -TargetProcessId $p.Id -Action Enable -ErrorAction Stop
                foreach ($xhlFlightLine in @($xhlFlightResult)) { Say ('[XHL-Flight-1.0] ' + [string]$xhlFlightLine) }
            } catch {
                Say ('[XHL-Flight-1.0] NOT APPLIED: ' + $_.Exception.Message)
                Say '[XHL-Flight-1.0] Server remains running; apply from dashboard if necessary.'
            }
        } else {
            Say '[XHL-Flight-1.0] Controller missing: Flight not applied.'
        }
    }
    Say '[XHL-Safe-Start-v1.0.7] Process alive at 10 seconds. Verify actual server readiness from console/log and a client connection.'
    exit 0
} catch {
    Say ('[XHL-Safe-Start-v1.0.7] FAILED: ' + $_.Exception.Message)
    Say 'Existing world/config were not deleted or replaced by this script.'
    exit 1
}
