param([ValidateSet('Identity','Record','Verify')][string]$Mode='Verify', [string]$BuildDirectory=(Join-Path $PSScriptRoot 'build'), $ExpectedSource)
$ErrorActionPreference='Stop'
$BuildDirectory=[IO.Path]::GetFullPath($BuildDirectory)
# Hash the complete tracked source plus nonignored new files. Never read installed settings.
function SourceIdentity {
 $checkout=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..')).Replace('\','/')
 $repo=(& git -c "safe.directory=$checkout" -C $PSScriptRoot rev-parse --show-toplevel)
 if($LASTEXITCODE -ne 0){throw 'Cannot identify source repository'}
 $paths=@(& git -c "safe.directory=$checkout" -C $repo ls-files --cached --others --exclude-standard) | Sort-Object -Unique
 if($LASTEXITCODE -ne 0){throw 'Cannot enumerate source inputs'}
 $rows=@(foreach($path in $paths){
  $file=Join-Path $repo $path
  if(Test-Path -LiteralPath $file -PathType Leaf){$path+':'+(Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash}
  else {$path+':MISSING'}
 })
 $sha=[Security.Cryptography.SHA256]::Create()
 try {$digest=[BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes(($rows -join "`n")))).Replace('-','').ToLowerInvariant()}finally{$sha.Dispose()}
 $commit=& git -c "safe.directory=$checkout" -C $repo rev-parse HEAD
 if($LASTEXITCODE -ne 0){throw 'Cannot read source commit'}
 $dirty=@(& git -c "safe.directory=$checkout" -C $repo status --porcelain).Count -gt 0
 if($LASTEXITCODE -ne 0){throw 'Cannot read source status'}
 [pscustomobject]@{Commit=$commit;Fingerprint=$digest;Dirty=$dirty;Version=(Get-Content (Join-Path $PSScriptRoot 'package.json') -Raw|ConvertFrom-Json).version}
}
$recordPath=Join-Path $BuildDirectory 'build-provenance.json'
$identity=SourceIdentity
if($Mode -eq 'Identity'){return $identity}
if($ExpectedSource -and ($ExpectedSource|ConvertTo-Json -Compress) -cne ($identity|ConvertTo-Json -Compress)){throw 'Source changed during validation. Build again.'}
$outputs=@(Get-ChildItem -LiteralPath $BuildDirectory -File -Recurse | Where-Object FullName -ne $recordPath | Sort-Object FullName | ForEach-Object {
 [pscustomobject]@{Path=$_.FullName.Substring(([IO.Path]::GetFullPath($BuildDirectory)).Length+1).Replace('\','/');SHA256=(Get-FileHash -LiteralPath $_.FullName).Hash}
})
if($Mode -eq 'Record'){
 @{Source=$identity;BuiltUtc=[DateTime]::UtcNow.ToString('o');Files=$outputs}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath $recordPath -Encoding utf8
}else{
 if(!(Test-Path -LiteralPath $recordPath)){throw 'Build provenance missing. Run Validate-Candidate.ps1 before packaging.'}
 $record=Get-Content -LiteralPath $recordPath -Raw|ConvertFrom-Json
 if($record.Source.Commit -ne $identity.Commit -or $record.Source.Fingerprint -ne $identity.Fingerprint -or $record.Source.Version -ne $identity.Version){throw 'Stale build: source or version changed after validation.'}
 if(($record.Files|ConvertTo-Json -Compress) -cne ($outputs|ConvertTo-Json -Compress)){throw 'Stale build: output files changed after validation.'}
 $record
}
