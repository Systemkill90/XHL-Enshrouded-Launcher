$ErrorActionPreference='Stop'
$root=Join-Path $PSScriptRoot '..\field-audit-launcher\src'
New-Item -ItemType Directory -Path (Join-Path $PSScriptRoot 'build') -Force|Out-Null
$refs=@('System.dll','System.Core.dll','System.Xml.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Management.dll','System.Web.Extensions.dll')|ForEach-Object {'/reference:'+$_}
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$sources=Get-ChildItem $root -Filter '*.cs'|ForEach-Object FullName
& $compiler /nologo /define:CLASSIC_TOOLS /codepage:65001 /target:winexe /platform:x64 /optimize+ ('/win32icon:'+(Join-Path $PSScriptRoot '..\field-audit-launcher\assets\XHL.ico')) ('/win32manifest:'+(Join-Path $PSScriptRoot '..\field-audit-launcher\XHL-Launcher.exe.manifest')) ('/out:'+(Join-Path $PSScriptRoot 'build\ShroudForge-Classic.exe')) @refs @sources
if($LASTEXITCODE -ne 0){throw 'Classic tool build failed'}

