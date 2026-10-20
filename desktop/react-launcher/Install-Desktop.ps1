param([Parameter(Mandatory=$true)][string]$Destination,[Parameter(Mandatory=$true)][string]$Package,[Parameter(Mandatory=$true)][string]$RollbackRoot,[switch]$NoLaunch,[ValidateRange(1,60)][int]$CloseTimeoutSeconds=15)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Launcher-Process.ps1')
$Destination=[IO.Path]::GetFullPath($Destination).TrimEnd('\')
$Package=[IO.Path]::GetFullPath($Package).TrimEnd('\')
$RollbackRoot=[IO.Path]::GetFullPath($RollbackRoot).TrimEnd('\')
function Overlaps([string]$first,[string]$second){return $first -eq $second -or $first.StartsWith($second+'\',[StringComparison]::OrdinalIgnoreCase) -or $second.StartsWith($first+'\',[StringComparison]::OrdinalIgnoreCase)}
[xml]$settings=Get-Content -LiteralPath (Join-Path $Destination 'Manager Data/settings.xml')
$server=[IO.Path]::GetFullPath([string]$settings.LauncherSettings.ServerRoot).TrimEnd('\')
if((Overlaps $Destination $server) -or (Overlaps $Package $server) -or (Overlaps $RollbackRoot $server)){throw 'Launcher, package and rollback folders must be separate from the game folder.'}
if((Overlaps $Destination $Package) -or (Overlaps $Destination $RollbackRoot) -or (Overlaps $Package $RollbackRoot)){throw 'Use separate launcher, package and rollback folders.'}
if(Test-Path -LiteralPath $RollbackRoot){throw 'Use a new rollback folder. Previous rollback files are never overwritten.'}
$manifest=Get-Content -LiteralPath (Join-Path $Package 'manifest.json') -Raw|ConvertFrom-Json
if(!$manifest.Files -or !($manifest.Files|Where-Object Path -eq 'ShroudForge-Launcher.exe')){throw 'Package does not contain the modern launcher.'}
$paths=New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
$rootFiles=@('ShroudForge-Launcher.exe','ShroudForge-Classic.exe','Microsoft.Web.WebView2.Core.dll','Microsoft.Web.WebView2.WinForms.dll','WebView2Loader.dll','READ-FIRST.md','build-provenance.json','INSTALL-UPDATE.md','Install-Desktop.ps1','Launcher-Process.ps1')
foreach($item in $manifest.Files){
 $relative=([string]$item.Path).Replace('/','\');$parts=$relative -split '\\'
 if(!$relative -or [IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or ($parts|Where-Object {$_ -eq '..' -or $_ -eq '.' -or $_ -eq '' -or $_.TrimEnd(' ','.') -cne $_}) -or !$paths.Add($relative)){throw 'Invalid or duplicate package path.'}
 if(($parts.Count -eq 1 -and $relative -notin $rootFiles) -or ($parts.Count -gt 1 -and $parts[0] -notin @('ui','assets','phone'))){throw ('Package cannot replace private data or unsupported files: '+$item.Path)}
 $source=Join-Path $Package $relative
 if((Get-FileHash -LiteralPath $source).Hash -ne $item.SHA256 -or (Get-Item -LiteralPath $source).Length -ne $item.Length){throw ('Package validation failed: '+$item.Path)}
}
$exe=Join-Path $Destination 'ShroudForge-Launcher.exe'
$classic=Join-Path $Destination 'ShroudForge-Classic.exe'
foreach($process in Get-Process 'ShroudForge-Classic' -ErrorAction SilentlyContinue){if($process.Path -eq $classic){throw 'Close classic tools before updating so unsaved work is preserved.'}}
Close-SelectedLauncher -Executable $exe -TimeoutSeconds $CloseTimeoutSeconds
$applied=New-Object 'System.Collections.Generic.List[object]'
$shortcuts=New-Object 'System.Collections.Generic.List[object]'
try {
 New-Item -ItemType Directory -Path $RollbackRoot|Out-Null
 $managerBackup=Join-Path $RollbackRoot 'Manager Data'
 New-Item -ItemType Directory -Path $managerBackup|Out-Null
 # Private settings, journals and player metadata stay in place. Retain a separate recovery copy.
 foreach($item in Get-ChildItem -LiteralPath (Join-Path $Destination 'Manager Data') -Force|Where-Object Name -ne 'WebView2'){Copy-Item -LiteralPath $item.FullName -Destination $managerBackup -Recurse}
 foreach($item in $manifest.Files){
  $target=Join-Path $Destination $item.Path;$temp=$target+'.pending-v3';$backup=Join-Path $RollbackRoot $item.Path
  New-Item -ItemType Directory -Path (Split-Path $target),(Split-Path $backup) -Force|Out-Null
  Copy-Item -LiteralPath (Join-Path $Package $item.Path) -Destination $temp
  $existed=Test-Path -LiteralPath $target
  if($existed){[IO.File]::Replace($temp,$target,$backup)}else{[IO.File]::Move($temp,$target)}
  $applied.Add([pscustomobject]@{Target=$target;Backup=$backup;Existed=$existed})
  if((Get-FileHash -LiteralPath $target).Hash -ne $item.SHA256){throw ('Installed file mismatch: '+$item.Path)}
 }
 $shell=New-Object -ComObject WScript.Shell
 foreach($name in @('Open ShroudForge.lnk','Open XHL 2.0.lnk')){
  $shortcut=Join-Path $Destination $name;$existed=Test-Path -LiteralPath $shortcut
  if($name -eq 'Open XHL 2.0.lnk' -and !$existed){continue}
  $backup=Join-Path $RollbackRoot $name
  if($existed){Copy-Item -LiteralPath $shortcut -Destination $backup}
  $shortcuts.Add([pscustomobject]@{Target=$shortcut;Backup=$backup;Existed=$existed})
  $link=$shell.CreateShortcut($shortcut);$link.TargetPath=$exe;$link.Arguments='';$link.WorkingDirectory=$Destination
  $link.Description='ShroudForge control center';$link.IconLocation=$exe+',0';$link.Save()
 }
 # This is the user's interactive launcher, not a background service/helper.
 if(!$NoLaunch){Start-Process -FilePath $exe -WorkingDirectory $Destination -WindowStyle Normal}
 Write-Output ('Updated ShroudForge. Previous launcher and private manager data retained: '+$RollbackRoot)
} catch {
 $failure=$_;$rollbackErrors=New-Object 'System.Collections.Generic.List[string]'
 for($i=$shortcuts.Count-1;$i -ge 0;$i--){$entry=$shortcuts[$i];try{if($entry.Existed){Copy-Item -LiteralPath $entry.Backup -Destination $entry.Target -Force}else{Remove-Item -LiteralPath $entry.Target -Force -ErrorAction SilentlyContinue}}catch{$rollbackErrors.Add($_.Exception.Message)}}
 for($i=$applied.Count-1;$i -ge 0;$i--){$entry=$applied[$i];try{if($entry.Existed){Copy-Item -LiteralPath $entry.Backup -Destination $entry.Target -Force}else{Remove-Item -LiteralPath $entry.Target -Force}}catch{$rollbackErrors.Add($_.Exception.Message)}}
 if($rollbackErrors.Count -gt 0){throw ('Update failed; rollback needs attention: '+($rollbackErrors -join '; '))}
 if(!$NoLaunch){Start-Process -FilePath $exe -WorkingDirectory $Destination -WindowStyle Normal}
 throw $failure
}
