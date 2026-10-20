param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'build'))
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw '.NET Framework 4.x x64 compiler is required.' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
if(Test-Path (Join-Path $PSScriptRoot 'assets')) { Copy-Item (Join-Path $PSScriptRoot 'assets') $OutputDirectory -Recurse -Force }
if(Test-Path (Join-Path $PSScriptRoot 'phone')) { Copy-Item (Join-Path $PSScriptRoot 'phone') $OutputDirectory -Recurse -Force }
$files = @('Program.cs','ControlRoom.cs','PlayerProfiles.cs','PlayerHistory.cs','QolReadOnly.cs','ModernUI.cs','ConfigurationEditor.cs','RealmHistory.cs','Maintenance.cs','Splash.cs','InstallationAudit.cs','InstallationInventory.cs','MigrationPlanner.cs','BackupPathPolicy.cs','ServerLocation.cs','ServerSetup.cs','VanillaServer.cs','Theme.cs','Models.cs','Storage.cs','Telemetry.cs','ServerControl.cs','ServerLifecycle.cs','Automation.cs','StagedUpdate.cs','UpdateActivation.cs','PhoneView.cs','RealmInstance.cs','ServerQuery.cs','BuildInfo.cs','ModManagement.cs','ModPanel.cs')
$sources = $files | ForEach-Object { Join-Path $PSScriptRoot ('src\' + $_) }
$refs = @('System.dll','System.Core.dll','System.Xml.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Management.dll','System.Web.Extensions.dll') | ForEach-Object { '/reference:' + $_ }
$iconArgs=@(); if(Test-Path (Join-Path $PSScriptRoot 'assets/XHL.ico')) { $iconArgs=@('/win32icon:'+(Join-Path $PSScriptRoot 'assets/XHL.ico')) }
& $compiler /nologo /codepage:65001 /target:winexe /platform:x64 /optimize+ @iconArgs ('/out:' + (Join-Path $OutputDirectory 'ShroudForge-Launcher.exe')) ('/win32manifest:' + (Join-Path $PSScriptRoot 'XHL-Launcher.exe.manifest')) @refs @sources
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
Write-Output 'ShroudForge 2.3 private build completed.'


