param([Parameter(Mandatory=$true)][string]$Stage)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath($Stage).TrimEnd('\')
if($root -notlike '*\Staged-Updates\*'){throw 'Only an isolated Staged-Updates installation is supported.'}
for($d=Get-Item -LiteralPath $root;$null -ne $d;$d=$d.Parent){if($d.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Linked stage directories are not supported.'}}
foreach($p in @(Get-Process enshrouded_server -ErrorAction SilentlyContinue)){if($p.Path -eq (Join-Path $root 'enshrouded_server.exe')){throw 'The selected stage is running.'}}
$expected=@{'enshrouded_server.exe'='B627639CE4A3F8C940FE90AFA1D36ED59DB3FF9CED24E935890EB7C99B04D5BF';'enshrouded_server.kfc'='E39DDDB3C531828B25A71BC8AF30F75EE7B8567489BFE90072CFD94235F045C6';'enshrouded_server.kfc_resources'='0D448AD81E9C4D6925978362F59F04F5D2C610A5F6260BA727FCA53935D7F506'}
foreach($n in $expected.Keys){if((Get-FileHash -LiteralPath (Join-Path $root $n)).Hash -ne $expected[$n]){throw ('Unsupported or already-modified update file: '+$n)}}
if(Test-Path -LiteralPath (Join-Path $root 'mods')){throw 'Stage already contains mods; use a clean download.'}
$backup=Join-Path $root ('Compatibility-Backups\'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $backup | Out-Null
foreach($n in $expected.Keys){Copy-Item -LiteralPath (Join-Path $root $n) -Destination $backup}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'mods') -Destination $root -Recurse
$payload=Join-Path $PSScriptRoot 'resources'
$verified=@{'enshrouded_server.kfc'='5BC0A5117714D69CE07960BFB5DDE87F0D929A1F7CB8A469CBA67016037B6E5D';'enshrouded_server.kfc_resources'='F55CF7101A78D4476F72B2574BA7EE219762AFFD6C470FF2CAB3D73DC20EBF2B'}
foreach($n in $verified.Keys){if((Get-FileHash -LiteralPath (Join-Path $payload $n)).Hash -ne $verified[$n]){throw 'Verified local resource bundle missing or changed.'}}
foreach($n in $verified.Keys){Copy-Item -LiteralPath (Join-Path $payload $n) -Destination (Join-Path $root $n)}
$k=(Get-FileHash -LiteralPath (Join-Path $root 'enshrouded_server.kfc')).Hash
$r=(Get-FileHash -LiteralPath (Join-Path $root 'enshrouded_server.kfc_resources')).Hash
if($k -ne '5BC0A5117714D69CE07960BFB5DDE87F0D929A1F7CB8A469CBA67016037B6E5D' -or $r -ne 'F55CF7101A78D4476F72B2574BA7EE219762AFFD6C470FF2CAB3D73DC20EBF2B'){throw 'Patch output did not match the verified current-build profile.'}
foreach($n in @('XHL-Flight-Control.ps1','XHL-Pitch-Hold.ps1','XHL-Gifting-Range-1.0.ps1','XHL-Safe-Start.ps1','XHL-Diagnostic-Wrapper.ps1')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot $n) -Destination $root}
@{Format='XHL-EML-OFFLINE-STAGING-v1';MaxStack=65535;ExeSha256=$expected['enshrouded_server.exe'];KfcSha256=$k;ResourcesSha256=$r} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $root 'XHL-STACK-1.0-ATTESTATION.json')
Set-Content -LiteralPath (Join-Path $root 'XHL-SAFE-MODE.flag') 'Current-build guarded profile'
Set-Content -LiteralPath (Join-Path $root 'XHL-STACK-LOSS-GUARD.flag') 'Never start without verified stacking resources.'
Set-Content -LiteralPath (Join-Path $root 'XHL-1.0-compat-manifest.json') '{"build":1269579,"mode":"offline-stacking-and-guarded-memory-hooks"}'
Write-Output 'Prepared current-build stacking, gifting and Flight-v2. Player flight test remains pending.'