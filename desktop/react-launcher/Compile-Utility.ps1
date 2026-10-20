param([Parameter(Mandatory=$true)][ValidateSet('DesktopChecks','AlignGameplay','ExportPreview','CompleteMaintenance')][string]$Name,[string]$Target='exe')
$ErrorActionPreference='Stop'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$core=Join-Path $PSScriptRoot '..\field-audit-launcher\src'
$src=@(Get-ChildItem $core -Filter '*.cs'|Where-Object Name -notin @('Program.cs','BuildInfo.cs')|ForEach-Object FullName)+@((Join-Path $PSScriptRoot 'Desktop.cs'),(Join-Path $PSScriptRoot 'Resilience.cs'),(Join-Path $PSScriptRoot 'IncidentEvidence.cs'),(Join-Path $PSScriptRoot ($Name+'.cs')))
$refs=@('System.dll','System.Core.dll','System.Xml.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Management.dll','System.Web.Extensions.dll')|ForEach-Object {'/reference:'+$_}
$refs+=@('Microsoft.Web.WebView2.Core.dll','Microsoft.Web.WebView2.WinForms.dll')|ForEach-Object {'/reference:'+(Join-Path $PSScriptRoot ('build\'+$_))}
& $compiler /nologo /codepage:65001 ('/target:'+$Target) /platform:x64 ('/main:'+$Name) ('/out:'+(Join-Path $PSScriptRoot ('build\'+$Name+'.exe'))) @refs @src
if($LASTEXITCODE -ne 0){throw 'Utility compilation failed'}

