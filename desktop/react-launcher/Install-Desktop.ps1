param([Parameter(Mandatory=$true)][string]$Destination,[Parameter(Mandatory=$true)][string]$Package,[Parameter(Mandatory=$true)][string]$RollbackRoot,[switch]$NoLaunch)
$ErrorActionPreference='Stop'
$Destination=[IO.Path]::GetFullPath($Destination).TrimEnd('\');$Package=[IO.Path]::GetFullPath($Package).TrimEnd('\');$RollbackRoot=[IO.Path]::GetFullPath($RollbackRoot)
[xml]$settings=Get-Content -LiteralPath (Join-Path $Destination 'Manager Data\settings.xml')
if($Destination -eq [IO.Path]::GetFullPath($settings.LauncherSettings.ServerRoot).TrimEnd('\')){throw 'Launcher and game folders must be separate'}
$manifest=Get-Content -LiteralPath (Join-Path $Package 'manifest.json') -Raw|ConvertFrom-Json
foreach($item in $manifest.Files){if([IO.Path]::IsPathRooted($item.Path) -or ($item.Path -split '[\\/]') -contains '..'){throw 'Invalid package path'};$source=Join-Path $Package $item.Path;if((Get-FileHash -LiteralPath $source).Hash -ne $item.SHA256 -or (Get-Item -LiteralPath $source).Length -ne $item.Length){throw ('Package validation failed: '+$item.Path)}}
$exe=Join-Path $Destination 'ShroudForge-Launcher.exe'
foreach($process in Get-Process 'ShroudForge-Launcher' -ErrorAction SilentlyContinue){if($process.Path -eq $exe){if(!$process.CloseMainWindow() -or !$process.WaitForExit(15000)){throw 'Launcher is busy or could not close normally. No files replaced.'}}}
New-Item -ItemType Directory -Path $RollbackRoot -Force|Out-Null
$managerBackup=Join-Path $RollbackRoot 'Manager Data'
New-Item -ItemType Directory -Path $managerBackup -Force|Out-Null
# The browser cache is volatile and is not launcher configuration or recovery state.
foreach($item in Get-ChildItem -LiteralPath (Join-Path $Destination 'Manager Data') -Force|Where-Object Name -ne 'WebView2'){Copy-Item -LiteralPath $item.FullName -Destination $managerBackup -Recurse}
$applied=New-Object 'System.Collections.Generic.List[object]'
try{foreach($item in $manifest.Files){$target=Join-Path $Destination $item.Path;$temp=$target+'.pending-v3';$backup=Join-Path $RollbackRoot $item.Path;New-Item -ItemType Directory -Path (Split-Path $target),(Split-Path $backup) -Force|Out-Null;Copy-Item -LiteralPath (Join-Path $Package $item.Path) -Destination $temp;$existed=Test-Path -LiteralPath $target;if($existed){[IO.File]::Replace($temp,$target,$backup)}else{[IO.File]::Move($temp,$target)};$applied.Add([pscustomobject]@{Target=$target;Backup=$backup;Existed=$existed});if((Get-FileHash -LiteralPath $target).Hash -ne $item.SHA256){throw ('Installed file mismatch: '+$item.Path)}}
 $shortcut=Join-Path $Destination 'Open XHL 2.0.lnk';if(Test-Path -LiteralPath $shortcut){Copy-Item -LiteralPath $shortcut -Destination (Join-Path $RollbackRoot 'launcher-shortcut.lnk');$shell=New-Object -ComObject WScript.Shell;$link=$shell.CreateShortcut($shortcut);$link.TargetPath=$exe;$link.Arguments='';$link.WorkingDirectory=$Destination;$link.Description='ShroudForge 3 — Velaris control center';$link.IconLocation=$exe+',0';$link.Save()}
 if(!$NoLaunch){Start-Process -FilePath $exe -WorkingDirectory $Destination -WindowStyle Normal}
 Write-Output ('Installed ShroudForge 3. Previous launcher and settings retained: '+$RollbackRoot)
}catch{$failure=$_;for($i=$applied.Count-1;$i -ge 0;$i--){$entry=$applied[$i];if($entry.Existed){Copy-Item -LiteralPath $entry.Backup -Destination $entry.Target -Force}else{Remove-Item -LiteralPath $entry.Target -Force}};if(!$NoLaunch){Start-Process -FilePath $exe -WorkingDirectory $Destination -WindowStyle Normal};throw $failure}
