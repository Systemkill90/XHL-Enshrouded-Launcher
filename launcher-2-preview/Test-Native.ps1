param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'native-results'))
$ErrorActionPreference='Stop'
$fixture=Join-Path $OutputDirectory ([guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture -Force | Out-Null
$fixture=(Resolve-Path $fixture).Path
Set-Content -LiteralPath (Join-Path $fixture 'XHL-TEST-FIXTURE.marker') -Value 'Isolated lifecycle test; not a game installation.'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $compiler /nologo /target:exe /platform:x64 ('/out:'+(Join-Path $fixture 'enshrouded_server.exe')) (Join-Path $PSScriptRoot 'DummyServer.cs')
if($LASTEXITCODE -ne 0){throw 'Dummy compilation failed'}
$sources=@('ServerLifecycle.cs','ServerControl.cs','InstallationAudit.cs','Models.cs') | ForEach-Object {Join-Path $PSScriptRoot ('src\'+$_)}
$harness=Join-Path $fixture 'NativeChecks.exe'
& $compiler /nologo /target:winexe /platform:x64 ('/out:'+$harness) /reference:System.dll /reference:System.Core.dll /reference:System.Web.Extensions.dll (Join-Path $PSScriptRoot 'NativeChecks.cs') @sources
if($LASTEXITCODE -ne 0){throw 'Native harness compilation failed'}
$proc=Start-Process -FilePath $harness -ArgumentList ('"'+$fixture+'"') -WindowStyle Hidden -PassThru
if(-not $proc.WaitForExit(60000)){throw 'Harness timed out; inspect the isolated fixture result before retrying.'}
$report=Get-Content -LiteralPath (Join-Path $fixture 'native-result.txt') -Raw
Write-Output $report
if($report -match '^FAIL'){throw 'Native checks failed'}
