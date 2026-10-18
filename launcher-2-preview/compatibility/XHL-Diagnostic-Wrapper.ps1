param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$ServerArgs
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$flag = Join-Path $root 'XHL-SAFE-MODE.flag'
$manifest = Join-Path $root 'XHL-1.0-compat-manifest.json'
$modeScript = Join-Path $root 'XHL-1.0-Compatibility.ps1'
$safeScript = Join-Path $root 'XHL-Safe-Start.ps1'
Write-Host '[XHL-Enshrouded-1.0-Combined-v1.0.6] Existing public server startup.'
try {
    if (-not (Test-Path -LiteralPath $safeScript -PathType Leaf)) {
        throw 'XHL-Safe-Start.ps1 missing. Refusing to start through the obsolete native injector.'
    }
    if (-not ((Test-Path -LiteralPath $flag -PathType Leaf) -and
               (Test-Path -LiteralPath $manifest -PathType Leaf))) {
        if (-not (Test-Path -LiteralPath $modeScript -PathType Leaf)) {
            throw 'Compatibility activation script missing; old native loader cannot safely start.'
        }
        Write-Host '[XHL-1.0] First launch: isolating known incompatible pre-1.0 native loaders.'
        # The isolated child process can return a reliable exit code despite
        # the compatibility script using explicit exit statements.
        $ps64 = Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
        if (-not (Test-Path -LiteralPath $ps64 -PathType Leaf)) { throw 'Windows PowerShell not found.' }
        & $ps64 -NoLogo -NoProfile -ExecutionPolicy Bypass -File $modeScript Enable
        if ($LASTEXITCODE -ne 0) {
            throw ('Compatibility activation failed (exit ' + $LASTEXITCODE + '). Original world was not touched; read the error above.')
        }
    }
    # Safe-Start checks BOTH files and all three DLL names, then hash-backs up
    # the existing world before running the EXISTING server.exe.
    if (-not (Test-Path -LiteralPath $flag -PathType Leaf) -or
        -not (Test-Path -LiteralPath $manifest -PathType Leaf)) {
        throw 'Safe mode marker/manifest not present after setup. Refusing to inject legacy mods.'
    }
    Write-Host '[XHL-1.0] Verified loader isolation mode selected; native injection disabled.'
    & $safeScript @ServerArgs
    exit $LASTEXITCODE
} catch {
    Write-Host ('[XHL-1.0] SAFE START BLOCKED: ' + $_.Exception.Message) -ForegroundColor Red
    Write-Host 'No server world reset, SteamCMD validation, or game reinstall was attempted.'
    exit 1
}
