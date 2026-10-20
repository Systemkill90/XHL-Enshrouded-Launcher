param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'production-results'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$sources=Get-ChildItem (Join-Path $PSScriptRoot 'src') -Filter '*.cs' | ForEach-Object {$_.FullName}
$refs=@('System.dll','System.Core.dll','System.Xml.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Management.dll','System.Web.Extensions.dll') | ForEach-Object {'/reference:'+$_}
$target=Join-Path $OutputDirectory 'ProductionChecks.exe'
& $compiler /nologo /codepage:65001 /target:exe /platform:x64 /main:ProductionChecks ('/out:'+$target) @refs @sources (Join-Path $PSScriptRoot 'ProductionChecks.cs')
if($LASTEXITCODE -ne 0){throw 'Production checks did not compile.'}
& $target $OutputDirectory
if($LASTEXITCODE -ne 0){throw 'Production checks failed.'}
