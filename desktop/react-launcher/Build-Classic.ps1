$ErrorActionPreference='Stop'
$root=Join-Path $PSScriptRoot 'classic-source'
New-Item -ItemType Directory -Path $root -Force|Out-Null
Copy-Item (Join-Path $PSScriptRoot '..\field-audit-launcher\src\*.cs') $root -Force
$path=Join-Path $root 'Automation.cs';$text=Get-Content -LiteralPath $path -Raw
$start=$text.IndexOf('internal sealed class RecoveryPolicy')
$end=$text.IndexOf('internal static class SaveBackup')
$text=$text.Substring(0,$start)+'internal sealed class RecoveryPolicy { internal void Disarm() {} internal bool Poll(bool enabled,bool running,bool busy,DateTime now) { return false; } }'+"`r`n    "+$text.Substring($end)
Set-Content -LiteralPath $path -Value $text -Encoding utf8
$path=Join-Path $root 'ControlRoom.cs';$text=Get-Content -LiteralPath $path -Raw
$text=$text.Replace('Text = "Restart after an unexpected stop (60-second delay)", Checked = settings.AutoRestartOnCrash, AutoCheck = controlsEnabled, TabStop = controlsEnabled','Text = "Automatic recovery runs in the modern control center", Checked = false, Enabled = false, AutoCheck = false, TabStop = false')
Set-Content -LiteralPath $path -Value $text -Encoding utf8
$refs=@('System.dll','System.Core.dll','System.Xml.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Management.dll','System.Web.Extensions.dll')|ForEach-Object {'/reference:'+$_}
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$sources=Get-ChildItem $root -Filter '*.cs'|ForEach-Object FullName
& $compiler /nologo /codepage:65001 /target:winexe /platform:x64 /optimize+ ('/win32icon:'+(Join-Path $PSScriptRoot '..\field-audit-launcher\assets\XHL.ico')) ('/win32manifest:'+(Join-Path $PSScriptRoot '..\field-audit-launcher\XHL-Launcher.exe.manifest')) ('/out:'+(Join-Path $PSScriptRoot 'build\ShroudForge-Classic.exe')) @refs @sources
if($LASTEXITCODE -ne 0){throw 'Classic tool build failed'}

