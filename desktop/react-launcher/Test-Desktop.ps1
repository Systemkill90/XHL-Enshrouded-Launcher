param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'test-results'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$sdk=Join-Path $PSScriptRoot '../webview-sdk/package'
$sources=@(Get-ChildItem (Join-Path $PSScriptRoot '../field-audit-launcher/src') -Filter '*.cs'|Where-Object Name -notin @('Program.cs','BuildInfo.cs')|ForEach-Object FullName)
$sources+=@('Desktop.cs','ScanSnapshot.cs','DesktopSnapshot.cs','LauncherClosing.cs','ServerManagerWindow.cs','Resilience.cs','IncidentEvidence.cs','Version.cs','DesktopChecks.cs','ServerSetupChecks.cs')|ForEach-Object {Join-Path $PSScriptRoot $_}
$refs=@('System.dll','System.Core.dll','System.Xml.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Management.dll','System.Web.Extensions.dll')|ForEach-Object {'/reference:'+$_}
$refs+=@('Microsoft.Web.WebView2.Core.dll','Microsoft.Web.WebView2.WinForms.dll')|ForEach-Object {'/reference:'+(Join-Path $sdk ('lib/net462/'+$_))}
$target=Join-Path $OutputDirectory 'DesktopChecks.exe'
& $compiler /nologo /codepage:65001 /target:exe /platform:x64 /main:DesktopChecks ('/out:'+$target) @refs @sources
if($LASTEXITCODE -ne 0){throw 'Desktop test compilation failed'}
Copy-Item (Join-Path $sdk 'lib/net462/*.dll') $OutputDirectory -Force
& $target $OutputDirectory
if($LASTEXITCODE -ne 0){throw 'Desktop checks failed'}

