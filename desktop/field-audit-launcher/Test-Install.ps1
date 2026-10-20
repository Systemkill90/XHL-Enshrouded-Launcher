param([Parameter(Mandatory=$true)][string]$OutputDirectory)
$ErrorActionPreference='Stop'
$root=Join-Path $OutputDirectory ('installer-'+[guid]::NewGuid().ToString('N'))
$app=Join-Path $root 'app';$package=Join-Path $root 'package';$rollback=Join-Path $root 'rollback'
foreach($folder in @((Join-Path $app 'Manager Data'),(Join-Path $app 'phone'),(Join-Path $package 'phone'))){New-Item -ItemType Directory -Path $folder -Force | Out-Null}
$settings='<LauncherSettings><ServerRoot>'+[Security.SecurityElement]::Escape((Join-Path $root 'game'))+'</ServerRoot></LauncherSettings>'
Set-Content -LiteralPath (Join-Path $app 'Manager Data\settings.xml') -Value $settings
Set-Content -LiteralPath (Join-Path $app 'ShroudForge-Launcher.exe') -Value 'previous fixture binary; never executed'
Set-Content -LiteralPath (Join-Path $app 'phone\phone-ui.html') -Value 'previous phone'
$shell=New-Object -ComObject WScript.Shell
$shortcut=$shell.CreateShortcut((Join-Path $app 'Open XHL 2.0.lnk'));$shortcut.TargetPath=Join-Path $app 'old-launcher.exe';$shortcut.Save()
Set-Content -LiteralPath (Join-Path $package 'ShroudForge-Launcher.exe') -Value 'candidate fixture binary; never executed'
Set-Content -LiteralPath (Join-Path $package 'phone\phone-ui.html') -Value 'candidate phone'
$files=@('ShroudForge-Launcher.exe','phone\phone-ui.html') | ForEach-Object {[pscustomobject]@{Path=$_;SHA256=(Get-FileHash -LiteralPath (Join-Path $package $_)).Hash}}
[pscustomobject]@{Files=@($files)} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $package 'manifest.json')
& (Join-Path $PSScriptRoot 'Install-Private.ps1') -Destination $app -Package $package -RollbackRoot $rollback -NoLaunch
if((Get-Content -Raw -LiteralPath (Join-Path $app 'ShroudForge-Launcher.exe')) -notmatch '^candidate'){throw 'Install failed'}
if((Get-Content -Raw -LiteralPath (Join-Path $rollback 'ShroudForge-Launcher.exe')) -notmatch '^previous'){throw 'Rollback binary missing'}
if((Get-Content -Raw -LiteralPath (Join-Path $app 'phone\phone-ui.html')) -notmatch '^candidate'){throw 'Phone install failed'}
if((Get-Content -Raw -LiteralPath (Join-Path $app 'Manager Data\settings.xml')).Trim() -ne $settings){throw 'Settings changed'}
if($shell.CreateShortcut((Join-Path $app 'Open XHL 2.0.lnk')).TargetPath -ne (Join-Path $app 'ShroudForge-Launcher.exe')){throw 'Shortcut not updated'}
if(!(Test-Path -LiteralPath (Join-Path $rollback 'launcher-shortcut.lnk'))){throw 'Shortcut backup missing'}
Write-Output 'PASS atomic launcher/page install, previous binary rollback, settings preservation'
$before=(Get-FileHash -LiteralPath (Join-Path $app 'ShroudForge-Launcher.exe')).Hash
Set-Content -LiteralPath (Join-Path $package 'ShroudForge-Launcher.exe') -Value 'tampered'
$rejected=$false;try{& (Join-Path $PSScriptRoot 'Install-Private.ps1') -Destination $app -Package $package -NoLaunch}catch{$rejected=$true}
if(!$rejected -or (Get-FileHash -LiteralPath (Join-Path $app 'ShroudForge-Launcher.exe')).Hash -ne $before){throw 'Tampered package was not safely rejected'}
Write-Output 'PASS tampered package rejected before installed files change'
