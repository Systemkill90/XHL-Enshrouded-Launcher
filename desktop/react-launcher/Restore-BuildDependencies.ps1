$ErrorActionPreference='Stop'
$version='1.0.4258.31'
$root=Join-Path $PSScriptRoot '../webview-sdk'
if(Test-Path -LiteralPath (Join-Path $root 'package')){throw 'SDK already exists; verify it rather than overwriting it.'}
New-Item -ItemType Directory -Path $root -Force|Out-Null
$archive=Join-Path $root ('Microsoft.Web.WebView2.'+$version+'.zip')
Invoke-WebRequest -Uri ('https://www.nuget.org/api/v2/package/Microsoft.Web.WebView2/'+$version) -OutFile $archive
Expand-Archive -LiteralPath $archive -DestinationPath (Join-Path $root 'package')
$dll=Join-Path $root 'package/lib/net462/Microsoft.Web.WebView2.Core.dll'
if([Reflection.AssemblyName]::GetAssemblyName($dll).Version.ToString() -ne $version){throw 'Unexpected SDK version'}
