param([Parameter(Mandatory=$true)][string]$Destination,[Parameter(Mandatory=$true)][string]$Package,[string]$RollbackRoot,[switch]$NoLaunch)
$ErrorActionPreference='Stop'
$Destination=[IO.Path]::GetFullPath($Destination).TrimEnd('\')
$Package=[IO.Path]::GetFullPath($Package).TrimEnd('\')
$settingsPath=Join-Path $Destination 'Manager Data\settings.xml'
if(!(Test-Path -LiteralPath $settingsPath)){throw 'This installer updates an existing configured launcher only.'}
[xml]$settings=Get-Content -LiteralPath $settingsPath
if($Destination -eq [IO.Path]::GetFullPath($settings.LauncherSettings.ServerRoot).TrimEnd('\')){throw 'Launcher destination must not be the game folder.'}
$manifest=Get-Content -Raw -LiteralPath (Join-Path $Package 'manifest.json') | ConvertFrom-Json
foreach($item in $manifest.Files){
 $relative=$item.Path
 if([IO.Path]::IsPathRooted($relative) -or ($relative -split '[\\/]') -contains '..'){throw 'Invalid package manifest path.'}
 $source=Join-Path $Package $relative
 if((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $item.SHA256){throw ('Package hash mismatch: '+$relative)}
}
$exe=Join-Path $Destination 'ShroudForge-Launcher.exe'
foreach($process in Get-Process -Name 'ShroudForge-Launcher' -ErrorAction SilentlyContinue){
 if($process.Path -ne $exe){continue}
 if(!$process.CloseMainWindow()){throw 'Launcher did not accept normal close. No forced termination attempted.'}
 if(!$process.WaitForExit(10000)){throw 'Launcher still open. Finish its current operation and try again.'}
}
if(!$RollbackRoot){$RollbackRoot=Join-Path $Destination ('Launcher Rollbacks\'+(Get-Date -Format 'yyyyMMdd-HHmmss'))}
$RollbackRoot=[IO.Path]::GetFullPath($RollbackRoot)
New-Item -ItemType Directory -Path $RollbackRoot -Force | Out-Null
Copy-Item -LiteralPath $exe -Destination (Join-Path $RollbackRoot 'ShroudForge-Launcher.exe')
Copy-Item -LiteralPath (Join-Path $Destination 'Manager Data') -Destination (Join-Path $RollbackRoot 'Manager Data') -Recurse
$phone=Join-Path $Destination 'phone\phone-ui.html'
if(Test-Path -LiteralPath $phone){Copy-Item -LiteralPath $phone -Destination (Join-Path $RollbackRoot 'phone-ui.html')}
$exeTemp=$exe+'.pending-'+[guid]::NewGuid().ToString('N')
$phoneTemp=$phone+'.pending-'+[guid]::NewGuid().ToString('N')
try{
 Copy-Item -LiteralPath (Join-Path $Package 'ShroudForge-Launcher.exe') -Destination $exeTemp
 New-Item -ItemType Directory -Path (Split-Path $phone) -Force | Out-Null
 Copy-Item -LiteralPath (Join-Path $Package 'phone\phone-ui.html') -Destination $phoneTemp
 [IO.File]::Replace($exeTemp,$exe,(Join-Path $RollbackRoot 'atomic-previous.exe'))
 if(Test-Path -LiteralPath $phone){[IO.File]::Replace($phoneTemp,$phone,(Join-Path $RollbackRoot 'atomic-previous-phone.html'))}else{[IO.File]::Move($phoneTemp,$phone)}
 $expected=$manifest.Files | Where-Object {$_.Path -eq 'ShroudForge-Launcher.exe'}
 if((Get-FileHash -LiteralPath $exe).Hash -ne $expected.SHA256){throw 'Installed launcher hash did not match.'}
 $shortcut=Join-Path $Destination 'Open XHL 2.0.lnk'
 if(Test-Path -LiteralPath $shortcut){
  Copy-Item -LiteralPath $shortcut -Destination (Join-Path $RollbackRoot 'launcher-shortcut.lnk')
  $shell=New-Object -ComObject WScript.Shell
  $link=$shell.CreateShortcut($shortcut);$link.TargetPath=$exe;$link.Arguments='--no-splash';$link.WorkingDirectory=$Destination;$link.IconLocation=$exe+',0';$link.Description='Open ShroudForge 2.3 — Velaris';$link.Save()
 }
 if(!$NoLaunch){Start-Process -FilePath $exe -ArgumentList '--no-splash' -WorkingDirectory $Destination -WindowStyle Normal}
 Write-Output ('Installed ShroudForge 2.3. Rollback: '+$RollbackRoot)
}catch{
 Copy-Item -LiteralPath (Join-Path $RollbackRoot 'ShroudForge-Launcher.exe') -Destination $exe -Force
 if(Test-Path -LiteralPath (Join-Path $RollbackRoot 'phone-ui.html')){Copy-Item -LiteralPath (Join-Path $RollbackRoot 'phone-ui.html') -Destination $phone -Force}
 if(!$NoLaunch){Start-Process -FilePath $exe -ArgumentList '--no-splash' -WorkingDirectory $Destination -WindowStyle Normal}
 throw
}finally{
 foreach($pending in @($exeTemp,$phoneTemp)){if(Test-Path -LiteralPath $pending){Remove-Item -LiteralPath $pending}}
}


