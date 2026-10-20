param([Parameter(Mandatory=$true)][string]$Destination,[switch]$Build)
$ErrorActionPreference='Stop'
$Destination=[IO.Path]::GetFullPath($Destination)
if(Test-Path -LiteralPath $Destination){throw 'Use a new isolated staging folder; existing packages are never overwritten.'}
if($Build){& (Join-Path $PSScriptRoot 'Validate-Candidate.ps1')}
$provenance=& (Join-Path $PSScriptRoot 'Build-Provenance.ps1') -Mode Verify
if($provenance.Source.Dirty -ne $false){throw 'Packaging requires committed source. Commit the changes and validate again.'}
$version=(Get-Content (Join-Path $PSScriptRoot 'package.json') -Raw|ConvertFrom-Json).version
New-Item -ItemType Directory -Path $Destination,(Join-Path $Destination 'ui')|Out-Null
foreach($file in @('ShroudForge-Launcher.exe','ShroudForge-Classic.exe','Microsoft.Web.WebView2.Core.dll','Microsoft.Web.WebView2.WinForms.dll','WebView2Loader.dll')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('build/'+$file)) -Destination $Destination}
foreach($file in @('index.html','app.js','style.css','realm-cinematic.png')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('build/ui/'+$file)) -Destination (Join-Path $Destination 'ui')}
foreach($folder in @('assets','phone')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('build/'+$folder)) -Destination $Destination -Recurse}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CANDIDATE-INSTRUCTIONS.md') -Destination (Join-Path $Destination 'READ-FIRST.md')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'build/build-provenance.json') -Destination $Destination
$files=@(Get-ChildItem -LiteralPath $Destination -File -Recurse|ForEach-Object {[pscustomobject]@{Path=$_.FullName.Substring($Destination.Length+1).Replace('\','/');Length=$_.Length;SHA256=(Get-FileHash -LiteralPath $_.FullName).Hash.ToLowerInvariant()}})
$sha=$provenance.Source.Commit
@{Version=$version;SourceCommit=$sha;SourceFingerprint=$provenance.Source.Fingerprint;BuiltUtc=$provenance.BuiltUtc;Unsigned=$true;Deployment='Staged only';Files=$files}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $Destination 'manifest.json') -Encoding utf8
@($files|ForEach-Object {$_.SHA256+' *'+$_.Path})+@((Get-FileHash (Join-Path $Destination 'manifest.json')).Hash.ToLowerInvariant()+' *manifest.json')|Set-Content (Join-Path $Destination 'SHA256SUMS.txt') -Encoding ascii
Write-Output ('Staged '+$version+': '+$Destination)
