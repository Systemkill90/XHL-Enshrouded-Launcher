#Requires -Version 5.1
<#
.SYNOPSIS
Read-only, privacy-conscious inventory of a running public Enshrouded host.
.DESCRIPTION
No file writes, copies, memory patches, backup operations, process changes,
network access, saves reads, restarts, launches, SteamCMD, or new mod loaders.
Outputs a JSON snapshot to standard output only. Choose paths explicitly.
This is NOT a compatibility or release approval test.
.EXAMPLE
.\ops\Read-Only-Host-Preflight.ps1 -ServerExecutable 'C:\Games\Enshrouded\enshrouded_server.exe' -ManagerExecutable 'C:\ShroudForge\ShroudForge-Launcher.exe' -ModDirectories @('C:\Games\Enshrouded\mods') -SaveDirectory 'D:\EnshroudedSaves'
#>
[CmdletBinding()]
param(
  [string]$ServerExecutable = '',
  [string]$ManagerExecutable = '',
  [string[]]$ModDirectories = @(),
  [string]$SaveDirectory = '',
  [string]$ServerProcessName = 'enshrouded_server',
  [string]$ManagerProcessName = 'ShroudForge-Launcher',
  [switch]$IncludeHashes
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-FileEvidence {
  param([string]$Path)
  if ([string]::IsNullOrWhiteSpace($Path)) {
    return [ordered]@{ supplied = $false; exists = $null; name = $null; lengthBytes = $null; sha256 = $null; shaStatus = 'not-requested' }
  }
  $basename = [System.IO.Path]::GetFileName($Path)
  if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
    return [ordered]@{ supplied = $true; exists = $false; name = $basename; lengthBytes = $null; sha256 = $null; shaStatus = 'missing' }
  }
  $item = Get-Item -LiteralPath $Path -ErrorAction Stop
  $hash = $null; $hashState = 'disabled-by-default-or-file-over-50mb'
  if ($IncludeHashes -and $item.Length -le 52428800) {
    try {
      $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $Path -ErrorAction Stop).Hash.ToLowerInvariant()
      $hashState = 'ok'
    } catch {
      $hashState = 'unavailable-file-in-use-or-permission'
    }
  }
  return [ordered]@{ supplied = $true; exists = $true; name = $basename; lengthBytes = $item.Length; sha256 = $hash; shaStatus = $hashState }
}

function Get-ProcessEvidence {
  param([string]$Name)
  # Exact process name, never a wildcard query or process mutation.
  $result = @(Get-Process -Name $Name -ErrorAction SilentlyContinue)
  return [ordered]@{ processName = $Name; observedCount = $result.Count;
    instances = @($result | ForEach-Object {
      $start = $null
      try { $start = $_.StartTime.ToUniversalTime().ToString('o') } catch { }
      [ordered]@{ pid = $_.Id; startedUtc = $start }
    })
  }
}

$inventory = @()
$loaderNames = @('dbghelp.dll', 'dinput8.dll', 'dinput.dll', 'winmm.dll', 'version.dll')
$loaderCandidates = @()
$modIndex = 0
foreach ($dir in $ModDirectories) {
  $modIndex++
  $label = 'mod-root-' + $modIndex
  if ([string]::IsNullOrWhiteSpace($dir) -or -not (Test-Path -LiteralPath $dir -PathType Container)) {
    $inventory += [ordered]@{ root = $label; exists = $false; files = @(); truncated = $false }
    continue
  }
  # No raw directory paths, credentials, save file contents or log text printed.
  $items = @(Get-ChildItem -LiteralPath $dir -File -Recurse -ErrorAction SilentlyContinue |
    Where-Object { $_.Extension -in @('.dll','.asi','.kfc','.pak','.json','.toml','.lua') } |
    Select-Object -First 101)
  $truncated = ($items.Count -gt 100)
  $items = @($items | Select-Object -First 100)
  $entries = @()
  foreach ($item in $items) {
    $details = Get-FileEvidence -Path $item.FullName
    $entries += $details
    if ($loaderNames -contains $item.Name.ToLowerInvariant()) {
      $loaderCandidates += [ordered]@{ root = $label; filename = $item.Name; classification = 'possible-proxy-loader-review-only' }
    }
  }
  $inventory += [ordered]@{ root = $label; exists = $true; files = $entries; truncated = $truncated }
}

$savesAvailable = $null
if (-not [string]::IsNullOrWhiteSpace($SaveDirectory)) {
  $savesAvailable = Test-Path -LiteralPath $SaveDirectory -PathType Container
}
$data = [ordered]@{
  format = 'shroudforge-host-preflight-v1'
  capturedUtc = [DateTime]::UtcNow.ToString('o')
  environment = 'owner-public-server-observation-only'
  deploymentApproved = $false
  worldConsistencyVerified = $false
  modGameplayVerified = $false
  playerRoster = 'not-queried-unknown'
  serverProcess = Get-ProcessEvidence -Name $ServerProcessName
  managerProcess = Get-ProcessEvidence -Name $ManagerProcessName
  serverExecutable = Get-FileEvidence -Path $ServerExecutable
  managerExecutable = Get-FileEvidence -Path $ManagerExecutable
  modDirectories = $inventory
  loaderCandidates = $loaderCandidates
  saveDirectorySpecified = (-not [string]::IsNullOrWhiteSpace($SaveDirectory))
  saveDirectoryExists = $savesAvailable
  notes = @(
    'Read-only metadata only; not a full installation scanner.'
    'An existing loader name is not proof of a harmful conflict.'
    'Do not publish this output unreviewed if filenames reveal personal details.'
    'Hash success does not imply runtime gameplay or save consistency.'
    'Unknown player count must not be treated as zero.'
  )
}
$data | ConvertTo-Json -Depth 12 -Compress
