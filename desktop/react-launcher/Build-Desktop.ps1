param([string]$Destination=(Join-Path $PSScriptRoot 'build'))
$ErrorActionPreference='Stop'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$sdk=Join-Path $PSScriptRoot '..\webview-sdk\package'
$core=Join-Path $PSScriptRoot '..\field-audit-launcher'
New-Item -ItemType Directory -Path $Destination -Force|Out-Null
$sources=@(Get-ChildItem (Join-Path $core 'src') -Filter '*.cs'|Where-Object {$_.Name -notin @('Program.cs','BuildInfo.cs')}|ForEach-Object {$_.FullName})+@((Join-Path $PSScriptRoot 'Desktop.cs'),(Join-Path $PSScriptRoot 'ScanSnapshot.cs'),(Join-Path $PSScriptRoot 'DesktopSnapshot.cs'),(Join-Path $PSScriptRoot 'Resilience.cs'),(Join-Path $PSScriptRoot 'IncidentEvidence.cs'),(Join-Path $PSScriptRoot 'Version.cs'))
$refs=@('System.dll','System.Core.dll','System.Xml.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Management.dll','System.Web.Extensions.dll')|ForEach-Object {'/reference:'+$_}
$refs+=@('Microsoft.Web.WebView2.Core.dll','Microsoft.Web.WebView2.WinForms.dll')|ForEach-Object {'/reference:'+(Join-Path $sdk ('lib\net462\'+$_))}
& $compiler /nologo /codepage:65001 /target:winexe /platform:x64 /optimize+ /main:XHLLauncher.DesktopEntry ('/win32icon:'+(Join-Path $core 'assets\XHL.ico')) ('/win32manifest:'+(Join-Path $core 'XHL-Launcher.exe.manifest')) ('/out:'+(Join-Path $Destination 'ShroudForge-Launcher.exe')) @refs @sources
if($LASTEXITCODE -ne 0){throw 'Desktop build failed'}
foreach($name in @('Microsoft.Web.WebView2.Core.dll','Microsoft.Web.WebView2.WinForms.dll')){Copy-Item -LiteralPath (Join-Path $sdk ('lib\net462\'+$name)) -Destination $Destination -Force}
Copy-Item -LiteralPath (Join-Path $sdk 'runtimes\win-x64\native\WebView2Loader.dll') -Destination $Destination -Force
foreach($name in @('assets','phone')){Copy-Item -LiteralPath (Join-Path $core $name) -Destination $Destination -Recurse -Force}
