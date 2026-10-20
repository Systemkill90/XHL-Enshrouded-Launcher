$ErrorActionPreference='Stop'
$fixture=Join-Path $PSScriptRoot ('test-results/provenance-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture -Force|Out-Null
Set-Content -LiteralPath (Join-Path $fixture 'fixture.bin') -Value 'original'
$rejected=$false
try{$null=& (Join-Path $PSScriptRoot 'Build-Provenance.ps1') -Mode Verify -BuildDirectory $fixture}catch{if($_ -notmatch 'provenance missing'){throw};$rejected=$true}
if(!$rejected){throw 'Missing provenance accepted'}
Write-Output 'PASS missing provenance rejected'
& (Join-Path $PSScriptRoot 'Build-Provenance.ps1') -Mode Record -BuildDirectory $fixture
$null=& (Join-Path $PSScriptRoot 'Build-Provenance.ps1') -Mode Verify -BuildDirectory $fixture
Write-Output 'PASS unchanged provenance accepted'
Set-Content -LiteralPath (Join-Path $fixture 'fixture.bin') -Value 'changed'
$rejected=$false
try{$null=& (Join-Path $PSScriptRoot 'Build-Provenance.ps1') -Mode Verify -BuildDirectory $fixture}catch{if($_ -notmatch 'Stale build'){throw};$rejected=$true}
if(!$rejected){throw 'Altered build accepted'}
Write-Output 'PASS altered output rejected'
& (Join-Path $PSScriptRoot 'Build-Provenance.ps1') -Mode Record -BuildDirectory $fixture
$path=Join-Path $fixture 'build-provenance.json'
$record=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
$record.Source.Fingerprint='outdated-source'
$record|ConvertTo-Json -Depth 5|Set-Content -LiteralPath $path
$rejected=$false
try{$null=& (Join-Path $PSScriptRoot 'Build-Provenance.ps1') -Mode Verify -BuildDirectory $fixture}catch{if($_ -notmatch 'Stale build'){throw};$rejected=$true}
if(!$rejected){throw 'Stale source accepted'}
Write-Output 'PASS stale source rejected'
