param([string]$Evidence=(Join-Path $PSScriptRoot 'test-results'))
$ErrorActionPreference='Stop'
$Evidence=[IO.Path]::GetFullPath($Evidence)
New-Item -ItemType Directory -Path $Evidence -Force|Out-Null
Push-Location $PSScriptRoot
try {
 $sourceBefore=& ./Build-Provenance.ps1 -Mode Identity
 $oldRecord=Join-Path $PSScriptRoot 'build/build-provenance.json'
 if(Test-Path -LiteralPath $oldRecord){Remove-Item -LiteralPath $oldRecord}
 & node node_modules/typescript/bin/tsc --noEmit
 if($LASTEXITCODE -ne 0){throw 'TypeScript check failed'}
 & node Sync-Phone.mjs --check
 if($LASTEXITCODE -ne 0){throw 'Phone synchronization failed'}
 & node build.mjs
 if($LASTEXITCODE -ne 0){throw 'UI bundle failed'}
 & ./Build-Desktop.ps1
 & ./Build-Classic.ps1
 & ./Test-Desktop.ps1 -OutputDirectory (Join-Path $Evidence 'desktop') *> (Join-Path $Evidence 'desktop.txt')
 & ./Test-Resilience.ps1 -OutputDirectory (Join-Path $Evidence 'resilience') *> (Join-Path $Evidence 'resilience.txt')
 & ./Test-DesktopInstall.ps1 -OutputDirectory (Join-Path $Evidence 'installer') *> (Join-Path $Evidence 'installer.txt')
 $core=Join-Path $PSScriptRoot '../field-audit-launcher'
 foreach($suite in @(@('Test.ps1','regression'),@('Test-Fields.ps1','fields'),@('Test-Mods.ps1','mods'),@('Test-Native.ps1','native'))){
  & (Join-Path $core $suite[0]) -OutputDirectory (Join-Path $Evidence $suite[1]) *> (Join-Path $Evidence ($suite[1]+'.txt'))
 }
 & ./Test-Provenance.ps1
 & ./Build-Provenance.ps1 -Mode Record -ExpectedSource $sourceBefore
 Write-Output 'PASS Windows builds, TypeScript, phone synchronization and isolated desktop/core/resilience/installer/field/mod/native checks; build provenance recorded.'
} finally {Pop-Location}
