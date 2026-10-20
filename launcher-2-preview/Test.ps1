param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'test-results'))
$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
if(Test-Path (Join-Path $PSScriptRoot 'assets')) { Copy-Item (Join-Path $PSScriptRoot 'assets') $OutputDirectory -Recurse -Force }
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$files = @('ControlRoom.cs','PlayerProfiles.cs','QolReadOnly.cs','ModernUI.cs','ConfigurationEditor.cs','RealmHistory.cs','Maintenance.cs','Splash.cs','InstallationAudit.cs','InstallationInventory.cs','MigrationPlanner.cs','BackupPathPolicy.cs','ServerLocation.cs','Theme.cs','Models.cs','Storage.cs','Telemetry.cs','ServerControl.cs','ServerLifecycle.cs','Automation.cs','StagedUpdate.cs','UpdateActivation.cs','PhoneView.cs')
$sources = $files | ForEach-Object { Join-Path $PSScriptRoot ('src\' + $_) }
$refs = @('System.dll','System.Core.dll','System.Xml.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Management.dll','System.Web.Extensions.dll') | ForEach-Object { '/reference:' + $_ }
$testExe = Join-Path $OutputDirectory 'XHL-Checks.exe'
& $compiler /nologo /codepage:65001 /target:exe /platform:x64 ('/out:' + $testExe) @refs @sources (Join-Path $PSScriptRoot 'Tests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed.' }
& $testExe $OutputDirectory
if ($LASTEXITCODE -ne 0) { throw 'Checks failed.' }

