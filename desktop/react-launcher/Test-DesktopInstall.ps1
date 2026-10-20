param([Parameter(Mandatory=$true)][string]$Package)
$ErrorActionPreference='Stop'
$root=Join-Path $PSScriptRoot ('installer-fixture-'+[guid]::NewGuid().ToString('N'));$app=Join-Path $root 'app';$fake=Join-Path $root 'package';$rollback=Join-Path $root 'rollback'
New-Item -ItemType Directory -Path (Join-Path $app 'Manager Data'),$fake -Force|Out-Null
$settings='<LauncherSettings><ServerRoot>'+[Security.SecurityElement]::Escape((Join-Path $root 'game'))+'</ServerRoot><PhoneViewKey>fixture-private-key</PhoneViewKey></LauncherSettings>'
Set-Content -LiteralPath (Join-Path $app 'Manager Data\settings.xml') $settings
Set-Content -LiteralPath (Join-Path $app 'ShroudForge-Launcher.exe') 'old fixture binary, never executed'
Set-Content -LiteralPath (Join-Path $fake 'ShroudForge-Launcher.exe') 'new fixture binary, never executed'
New-Item -ItemType Directory -Path (Join-Path $fake 'ui')|Out-Null
Set-Content -LiteralPath (Join-Path $fake 'ui\index.html') 'fixture page'
$entries=@(Get-ChildItem $fake -File -Recurse|ForEach-Object{[pscustomobject]@{Path=$_.FullName.Substring($fake.Length+1);Length=$_.Length;SHA256=(Get-FileHash $_.FullName).Hash}})
@{Files=$entries}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $fake 'manifest.json')
& (Join-Path $PSScriptRoot 'Install-Desktop.ps1') -Destination $app -Package $fake -RollbackRoot $rollback -NoLaunch
if((Get-FileHash (Join-Path $app 'ShroudForge-Launcher.exe')).Hash -ne (Get-FileHash (Join-Path $fake 'ShroudForge-Launcher.exe')).Hash){throw 'Fixture install mismatch'}
if((Get-Content (Join-Path $app 'Manager Data\settings.xml') -Raw).Trim() -ne $settings){throw 'Settings changed'}
if(!(Test-Path (Join-Path $rollback 'ShroudForge-Launcher.exe'))){throw 'Rollback binary missing'}
Write-Output 'PASS atomic desktop files, previous launcher retention, private settings preservation'
$hash=(Get-FileHash (Join-Path $app 'ShroudForge-Launcher.exe')).Hash
Set-Content -LiteralPath (Join-Path $fake 'ui\index.html') 'tampered'
$rejected=$false;try{& (Join-Path $PSScriptRoot 'Install-Desktop.ps1') -Destination $app -Package $fake -RollbackRoot (Join-Path $root 'rejected') -NoLaunch}catch{$rejected=$true}
if(!$rejected -or (Get-FileHash (Join-Path $app 'ShroudForge-Launcher.exe')).Hash -ne $hash){throw 'Tamper guard failed'}
Write-Output 'PASS tampered package rejected before changes'
