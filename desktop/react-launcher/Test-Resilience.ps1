param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'test-results'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$sources=@(Get-ChildItem (Join-Path $PSScriptRoot '..\field-audit-launcher\src') -Filter '*.cs' |Where-Object {$_.Name -ne 'Program.cs'}|ForEach-Object {$_.FullName})+@((Join-Path $PSScriptRoot 'Resilience.cs'),(Join-Path $PSScriptRoot 'ResilienceChecks.cs'))
$refs=@('System.dll','System.Core.dll','System.Xml.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Management.dll','System.Web.Extensions.dll')|ForEach-Object {'/reference:'+$_}
& $compiler /nologo /target:exe /platform:x64 /main:ResilienceChecks ('/out:'+(Join-Path $OutputDirectory 'ResilienceChecks.exe')) @refs @sources
if($LASTEXITCODE -ne 0){throw 'Tests did not compile'}
& (Join-Path $OutputDirectory 'ResilienceChecks.exe') $OutputDirectory
if($LASTEXITCODE -ne 0){throw 'Resilience checks failed'}
