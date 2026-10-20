param([string]$Package,[string]$OutputDirectory=(Join-Path $PSScriptRoot 'test-results/installer'))
$ErrorActionPreference='Stop'
$root=Join-Path ([IO.Path]::GetFullPath($OutputDirectory)) ([guid]::NewGuid().ToString('N'))
$app=Join-Path $root 'app';$fake=Join-Path $root 'package';$rollback=Join-Path $root 'rollback'
New-Item -ItemType Directory -Path (Join-Path $app 'Manager Data'),$fake -Force|Out-Null
$settings='<LauncherSettings><ServerRoot>'+[Security.SecurityElement]::Escape((Join-Path $root 'game'))+'</ServerRoot><PhoneViewKey>fixture-private-key</PhoneViewKey></LauncherSettings>'
Set-Content -LiteralPath (Join-Path $app 'Manager Data/settings.xml') $settings
Set-Content -LiteralPath (Join-Path $app 'Manager Data/player-history-fixture.json') 'private fixture metadata'
New-Item -ItemType Directory -Path (Join-Path $app 'Manager Data/WebView2')|Out-Null
Set-Content -LiteralPath (Join-Path $app 'Manager Data/WebView2/cache.bin') 'volatile fixture cache'
Set-Content -LiteralPath (Join-Path $app 'ShroudForge-Launcher.exe') 'old fixture binary, never executed'
Set-Content -LiteralPath (Join-Path $fake 'ShroudForge-Launcher.exe') 'new fixture binary, never executed'
New-Item -ItemType Directory -Path (Join-Path $fake 'ui')|Out-Null
Set-Content -LiteralPath (Join-Path $fake 'ui/index.html') 'fixture page'
function Manifest {
 $entries=@(Get-ChildItem $fake -File -Recurse|Where-Object Name -ne 'manifest.json'|ForEach-Object{[pscustomobject]@{Path=$_.FullName.Substring($fake.Length+1).Replace('\','/');Length=$_.Length;SHA256=(Get-FileHash -LiteralPath $_.FullName).Hash}})
 @{Files=$entries}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $fake 'manifest.json')
}
$shell=New-Object -ComObject WScript.Shell
$legacy=$shell.CreateShortcut((Join-Path $app 'Open XHL 2.0.lnk'));$legacy.TargetPath=Join-Path $app 'old.exe';$legacy.Save()
Manifest
& (Join-Path $PSScriptRoot 'Install-Desktop.ps1') -Destination $app -Package $fake -RollbackRoot $rollback -NoLaunch
if((Get-FileHash (Join-Path $app 'ShroudForge-Launcher.exe')).Hash -ne (Get-FileHash (Join-Path $fake 'ShroudForge-Launcher.exe')).Hash){throw 'Fixture install mismatch'}
if((Get-Content (Join-Path $app 'Manager Data/settings.xml') -Raw).Trim() -ne $settings){throw 'Settings changed'}
if(!(Test-Path (Join-Path $rollback 'ShroudForge-Launcher.exe')) -or !(Test-Path (Join-Path $rollback 'Manager Data/player-history-fixture.json')) -or (Test-Path (Join-Path $rollback 'Manager Data/WebView2'))){throw 'Rollback retention or cache exclusion failed'}
Write-Output 'PASS atomic files, retained previous launcher/settings/player metadata, browser cache excluded'
foreach($name in @('Open ShroudForge.lnk','Open XHL 2.0.lnk')){if($shell.CreateShortcut((Join-Path $app $name)).TargetPath -ne (Join-Path $app 'ShroudForge-Launcher.exe')){throw 'Shortcut target mismatch'}}
Write-Output 'PASS ShroudForge shortcut created and existing legacy shortcut preserved and updated'
$hash=(Get-FileHash (Join-Path $app 'ShroudForge-Launcher.exe')).Hash
function Reject([string]$kind,[scriptblock]$action){$rejected=$false;try{& $action}catch{$rejected=$true};if(!$rejected -or (Get-FileHash (Join-Path $app 'ShroudForge-Launcher.exe')).Hash -ne $hash){throw ('Guard failed: '+$kind)};Write-Output ('PASS '+$kind)}
Set-Content -LiteralPath (Join-Path $fake 'ui/index.html') 'tampered'
Reject 'tampered package rejected before changes' {& (Join-Path $PSScriptRoot 'Install-Desktop.ps1') -Destination $app -Package $fake -RollbackRoot (Join-Path $root 'tamper-rejected') -NoLaunch}
Manifest
Reject 'existing rollback cannot be overwritten' {& (Join-Path $PSScriptRoot 'Install-Desktop.ps1') -Destination $app -Package $fake -RollbackRoot $rollback -NoLaunch}
$goodManifest=Get-Content -LiteralPath (Join-Path $fake 'manifest.json') -Raw
foreach($bad in @('../escape.bin','Manager Data/settings.xml','enshrouded_server.exe','ui/../escape.bin','ui/.. /escape.bin')){
 $data=$goodManifest|ConvertFrom-Json;$data.Files[0].Path=$bad;$data|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $fake 'manifest.json')
 Reject ('unsafe package path rejected: '+$bad) {& (Join-Path $PSScriptRoot 'Install-Desktop.ps1') -Destination $app -Package $fake -RollbackRoot (Join-Path $root 'path-rejected') -NoLaunch}
}
$data=$goodManifest|ConvertFrom-Json;$data.Files=@($data.Files)+@($data.Files[0]);$data|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $fake 'manifest.json')
Reject 'duplicate manifest path rejected' {& (Join-Path $PSScriptRoot 'Install-Desktop.ps1') -Destination $app -Package $fake -RollbackRoot (Join-Path $root 'duplicate-rejected') -NoLaunch}
Set-Content -LiteralPath (Join-Path $fake 'manifest.json') $goodManifest
Reject 'overlapping package and rollback rejected' {& (Join-Path $PSScriptRoot 'Install-Desktop.ps1') -Destination $app -Package $fake -RollbackRoot (Join-Path $fake 'nested-rollback') -NoLaunch}
# A target directory reliably blocks the second replacement, after the binary changed.
Remove-Item -LiteralPath (Join-Path $app 'ui/index.html')
New-Item -ItemType Directory -Path (Join-Path $app 'ui/index.html') -Force|Out-Null
Set-Content -LiteralPath (Join-Path $app 'ShroudForge-Launcher.exe') 'old binary before failed transaction'
$hash=(Get-FileHash (Join-Path $app 'ShroudForge-Launcher.exe')).Hash
Reject 'mid-update failure restores files already replaced' {& (Join-Path $PSScriptRoot 'Install-Desktop.ps1') -Destination $app -Package $fake -RollbackRoot (Join-Path $root 'failed-copy-rollback') -NoLaunch}
# Window fixtures use a separate app so previous filesystem sentinels remain intact.
$windowApp=Join-Path $root 'window-app';$otherApp=Join-Path $root 'other-app'
foreach($folder in @($windowApp,$otherApp)){New-Item -ItemType Directory -Path (Join-Path $folder 'Manager Data') -Force|Out-Null;Set-Content -LiteralPath (Join-Path $folder 'Manager Data/settings.xml') $settings;Set-Content -LiteralPath (Join-Path $folder 'installer-fixture.marker') 'Disposable installer window only'}
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
& $compiler /nologo /codepage:65001 /target:winexe /reference:System.dll /reference:System.Windows.Forms.dll ('/out:'+(Join-Path $windowApp 'ShroudForge-Launcher.exe')) (Join-Path $PSScriptRoot 'InstallerWindowFixture.cs')
if($LASTEXITCODE -ne 0){throw 'Installer window fixture compilation failed'}
Copy-Item -LiteralPath (Join-Path $windowApp 'ShroudForge-Launcher.exe') -Destination $otherApp
function StartFixture([string]$folder){$p=Start-Process -FilePath (Join-Path $folder 'ShroudForge-Launcher.exe') -ArgumentList ('"'+$folder+'"') -WindowStyle Hidden -PassThru;$deadline=[DateTime]::UtcNow.AddSeconds(6);while(!(Test-Path (Join-Path $folder 'ready.marker')) -and !$p.HasExited -and [DateTime]::UtcNow -lt $deadline){Start-Sleep -Milliseconds 100};if(!(Test-Path (Join-Path $folder 'ready.marker'))){throw 'Fixture window not ready'};return $p}
$other=StartFixture $otherApp
try {
 $hidden=StartFixture $windowApp
 if($hidden.MainWindowHandle -ne 0){throw 'Expected hidden window fixture'}
 & (Join-Path $PSScriptRoot 'Install-Desktop.ps1') -Destination $windowApp -Package $fake -RollbackRoot (Join-Path $root 'hidden-rollback') -NoLaunch -CloseTimeoutSeconds 3
 if(!$hidden.HasExited -or !(Test-Path (Join-Path $windowApp 'closed.marker')) -or !(Test-Path (Join-Path $windowApp 'maintenance.marker')) -or $other.HasExited){throw 'Normal hidden maintenance close or exact-path exclusion failed'}
 Write-Output 'PASS hidden launcher closes normally; different installation remains running'
 Copy-Item -LiteralPath (Join-Path $otherApp 'ShroudForge-Launcher.exe') -Destination $windowApp -Force
 Remove-Item -LiteralPath (Join-Path $windowApp 'ready.marker'),(Join-Path $windowApp 'closed.marker')
 Set-Content -LiteralPath (Join-Path $windowApp 'busy.marker') 'Refuse normal close until cleared'
 $busy=StartFixture $windowApp;$hash=(Get-FileHash (Join-Path $windowApp 'ShroudForge-Launcher.exe')).Hash
 $rejected=$false;try{& (Join-Path $PSScriptRoot 'Install-Desktop.ps1') -Destination $windowApp -Package $fake -RollbackRoot (Join-Path $root 'busy-rollback') -NoLaunch -CloseTimeoutSeconds 1}catch{$rejected=$true}
 if(!$rejected -or $busy.HasExited -or (Test-Path (Join-Path $root 'busy-rollback')) -or (Get-FileHash (Join-Path $windowApp 'ShroudForge-Launcher.exe')).Hash -ne $hash){throw 'Busy-window guard failed'}
 Write-Output 'PASS busy launcher refuses close; no files replaced and no forced termination'
 Remove-Item -LiteralPath (Join-Path $windowApp 'busy.marker')
 . (Join-Path $PSScriptRoot 'Launcher-Process.ps1')
 Close-SelectedLauncher -Executable (Join-Path $windowApp 'ShroudForge-Launcher.exe') -TimeoutSeconds 3
 # Exercise the default reopen path with a real executable that self-hides and self-exits.
 Copy-Item -LiteralPath (Join-Path $otherApp 'ShroudForge-Launcher.exe') -Destination (Join-Path $fake 'ShroudForge-Launcher.exe') -Force
 Manifest
 & (Join-Path $PSScriptRoot 'Install-Desktop.ps1') -Destination $windowApp -Package $fake -RollbackRoot (Join-Path $root 'relaunch-rollback')
 $deadline=[DateTime]::UtcNow.AddSeconds(6);$reopened=$null
 do{$reopened=@(Get-Process 'ShroudForge-Launcher' -ErrorAction SilentlyContinue|Where-Object Path -eq (Join-Path $windowApp 'ShroudForge-Launcher.exe'))|Select-Object -First 1;if(!$reopened){Start-Sleep -Milliseconds 100}}while(!$reopened -and [DateTime]::UtcNow -lt $deadline)
 if(!$reopened -or $reopened.Id -eq $busy.Id){throw 'Default relaunch failed'}
 $deadline=[DateTime]::UtcNow.AddSeconds(6)
 while((Get-Content -LiteralPath (Join-Path $windowApp 'ready.marker') -Raw).Trim() -ne [string]$reopened.Id -and [DateTime]::UtcNow -lt $deadline){Start-Sleep -Milliseconds 100}
 if((Get-Content -LiteralPath (Join-Path $windowApp 'ready.marker') -Raw).Trim() -ne [string]$reopened.Id){throw 'Reopened window did not finish initializing'}
 Write-Output 'PASS default update reopens the selected launcher'
 Close-SelectedLauncher -Executable (Join-Path $windowApp 'ShroudForge-Launcher.exe') -TimeoutSeconds 3
} finally {. (Join-Path $PSScriptRoot 'Launcher-Process.ps1');Close-SelectedLauncher -Executable (Join-Path $otherApp 'ShroudForge-Launcher.exe') -TimeoutSeconds 3}
