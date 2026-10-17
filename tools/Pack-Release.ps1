param(
    [string]$BepInExDirectory = (Join-Path $PSScriptRoot '..\BepInEx\BepInEx_win_x64_5.4.23.5')
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$pluginProject = Join-Path $root 'src\Repkarbinhake.Plugin'
$version = (Select-String -Path (Join-Path $pluginProject 'RepkaPlugin.cs') -Pattern 'Version = "([^"]+)"').Matches[0].Groups[1].Value
$dist = Join-Path $root 'dist'
$stage = Join-Path $dist 'stage'

if (-not (Test-Path (Join-Path $BepInExDirectory 'winhttp.dll'))) {
    throw "BepInEx not found in '$BepInExDirectory'. Pass -BepInExDirectory with an unpacked BepInEx_win_x64 5.4.x."
}

dotnet build $pluginProject -c Release --nologo -v quiet
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }

$output = Join-Path $pluginProject 'bin\Release\netstandard2.1'
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }

$pluginStage = Join-Path $stage 'plugin'
$modFolder = Join-Path $pluginStage 'BepInEx\plugins\Repkarbinhake'
New-Item -ItemType Directory -Force (Join-Path $modFolder 'Assets') | Out-Null
foreach ($dll in 'Repkarbinhake.dll', 'Repkarbinhake.Core.dll', 'Repkarbinhake.Unity.dll') {
    Copy-Item (Join-Path $output $dll) $modFolder
}
Copy-Item (Join-Path $pluginProject 'Assets\*') (Join-Path $modFolder 'Assets')

$install = (Get-Content (Join-Path $PSScriptRoot 'INSTALL.txt') -Raw -Encoding UTF8).Replace('{VERSION}', $version)
Set-Content (Join-Path $pluginStage 'Repkarbinhake-INSTALL.txt') $install -Encoding UTF8

$fullStage = Join-Path $stage 'full'
New-Item -ItemType Directory -Force $fullStage | Out-Null
Copy-Item (Join-Path $BepInExDirectory '*') $fullStage -Recurse -Force
Copy-Item (Join-Path $pluginStage '*') $fullStage -Recurse -Force

$pluginZip = Join-Path $dist "Repkarbinhake-$version-plugin.zip"
$fullZip = Join-Path $dist "Repkarbinhake-$version-full.zip"
Remove-Item $pluginZip, $fullZip -ErrorAction SilentlyContinue
# Windows tar writes standard forward-slash zip entries, Compress-Archive in PowerShell 5.1 does not
tar -a -c -f $pluginZip -C $pluginStage @(Get-ChildItem $pluginStage -Force | ForEach-Object Name)
tar -a -c -f $fullZip -C $fullStage @(Get-ChildItem $fullStage -Force | ForEach-Object Name)
if ($LASTEXITCODE -ne 0) { throw 'Packing failed' }
Remove-Item $stage -Recurse -Force

Write-Host "Packed Repkarbinhake $version"
Get-Item $pluginZip, $fullZip | ForEach-Object { Write-Host ("  {0}  {1:N0} KB" -f $_.Name, ($_.Length / 1KB)) }
