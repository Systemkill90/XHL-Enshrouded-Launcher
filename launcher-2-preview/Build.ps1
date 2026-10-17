param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'build'))
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw '.NET Framework 4.x x64 compiler is required.' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$files = @('Program.cs','ControlRoom.cs','InstallationAudit.cs','InstallationInventory.cs','MigrationPlanner.cs','Theme.cs','Models.cs','Storage.cs','Telemetry.cs','ServerControl.cs','ServerLifecycle.cs','Automation.cs','StagedUpdate.cs','UpdateActivation.cs','PhoneView.cs')
$sources = $files | ForEach-Object { Join-Path $PSScriptRoot ('src\' + $_) }
$refs = @('System.dll','System.Core.dll','System.Xml.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Management.dll','System.Web.Extensions.dll') | ForEach-Object { '/reference:' + $_ }
& $compiler /nologo /codepage:65001 /target:winexe /platform:x64 /optimize+ ('/out:' + (Join-Path $OutputDirectory 'XHL-Launcher-2.exe')) ('/win32manifest:' + (Join-Path $PSScriptRoot 'XHL-Launcher.exe.manifest')) @refs @sources
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
Write-Output 'Launcher 2.0 preview built successfully.'
