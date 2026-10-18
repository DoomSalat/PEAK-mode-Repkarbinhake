param(
    [string]$BepInExDirectory = (Join-Path $PSScriptRoot '..\BepInEx\BepInEx_win_x64_5.4.23.5')
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$pluginProject = Join-Path $root 'src\Repkarbinhake.Plugin'
$version = (Select-String -Path (Join-Path $pluginProject 'RepkaPlugin.cs') -Pattern 'Version = "([^"]+)"').Matches[0].Groups[1].Value
$dist = Join-Path $root 'dist'
$tar = Join-Path $env:SystemRoot 'System32\tar.exe'

if (-not (Test-Path (Join-Path $BepInExDirectory 'winhttp.dll'))) {
    throw "BepInEx not found in '$BepInExDirectory'. Pass -BepInExDirectory with an unpacked BepInEx_win_x64 5.4.x."
}

function Pack([string]$configuration, [string]$flavor, [string]$debugKeys) {
    dotnet build $pluginProject -c $configuration --nologo -v quiet
    if ($LASTEXITCODE -ne 0) { throw "Build failed ($configuration)" }

    $output = Join-Path $pluginProject "bin\$configuration\netstandard2.1"
    $stage = Join-Path $dist 'stage'
    if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }

    $pluginStage = Join-Path $stage 'plugin'
    $modFolder = Join-Path $pluginStage 'BepInEx\plugins\Repkarbinhake'
    New-Item -ItemType Directory -Force (Join-Path $modFolder 'Assets') | Out-Null
    foreach ($dll in 'Repkarbinhake.dll', 'Repkarbinhake.Core.dll', 'Repkarbinhake.Unity.dll') {
        Copy-Item (Join-Path $output $dll) $modFolder
    }
    Copy-Item (Join-Path $pluginProject 'Assets\*') (Join-Path $modFolder 'Assets')

    $install = (Get-Content (Join-Path $PSScriptRoot 'INSTALL.txt') -Raw -Encoding UTF8).Replace('{VERSION}', "$version $flavor").Replace('{DEBUG_KEYS}', $debugKeys)
    Set-Content (Join-Path $pluginStage 'Repkarbinhake-INSTALL.txt') $install -Encoding UTF8

    $fullStage = Join-Path $stage 'full'
    New-Item -ItemType Directory -Force $fullStage | Out-Null
    Copy-Item (Join-Path $BepInExDirectory '*') $fullStage -Recurse -Force
    Copy-Item (Join-Path $pluginStage '*') $fullStage -Recurse -Force

    $pluginZip = Join-Path $dist "Repkarbinhake-$version-$flavor-plugin.zip"
    $fullZip = Join-Path $dist "Repkarbinhake-$version-$flavor-full.zip"
    Remove-Item $pluginZip, $fullZip -ErrorAction SilentlyContinue
    # Windows tar writes standard forward-slash zip entries, Compress-Archive in PowerShell 5.1 does not
    & $tar -a -c -f $pluginZip -C $pluginStage @(Get-ChildItem $pluginStage -Force | ForEach-Object Name)
    & $tar -a -c -f $fullZip -C $fullStage @(Get-ChildItem $fullStage -Force | ForEach-Object Name)
    if ($LASTEXITCODE -ne 0) { throw "Packing failed ($flavor)" }
    Remove-Item $stage -Recurse -Force

    Get-Item $pluginZip, $fullZip | ForEach-Object { Write-Host ("  {0}  {1:N0} KB" -f $_.Name, ($_.Length / 1KB)) }
}

$debugLines = "  F7                                  — окно настроек мода прямо в игре (Применить — сразу в силу)`r`n" +
              "  F9                                  — тестовая капсула: поставить / убрать (к ней можно привязаться)`r`n"

function PackThunderstore {
    $output = Join-Path $pluginProject 'bin\Public\netstandard2.1'
    $source = Join-Path $PSScriptRoot 'thunderstore'
    $stage = Join-Path $dist 'stage-thunderstore'
    if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }

    $plugins = Join-Path $stage 'plugins'
    New-Item -ItemType Directory -Force (Join-Path $plugins 'Assets') | Out-Null
    foreach ($dll in 'Repkarbinhake.dll', 'Repkarbinhake.Core.dll', 'Repkarbinhake.Unity.dll') {
        Copy-Item (Join-Path $output $dll) $plugins
    }
    Copy-Item (Join-Path $pluginProject 'Assets\*') (Join-Path $plugins 'Assets')

    $utf8 = New-Object System.Text.UTF8Encoding $false
    $manifest = (Get-Content (Join-Path $source 'manifest.json') -Raw -Encoding UTF8).Replace('{VERSION}', $version)
    [System.IO.File]::WriteAllText((Join-Path $stage 'manifest.json'), $manifest, $utf8)
    foreach ($file in 'CHANGELOG.md', 'icon.png') {
        Copy-Item (Join-Path $source $file) $stage
    }

    # Images without a hosted link would show broken on Thunderstore, so they are left out of the packed README
    $readme = Get-Content (Join-Path $source 'README.md') -Encoding UTF8
    $unlinked = @($readme | Where-Object { $_ -match '\]\(IMAGE_' })
    $kept = ($readme | Where-Object { $_ -notmatch '\]\(IMAGE_' }) -join "`n"
    $kept = [regex]::Replace($kept, "`n{3,}", "`n`n")
    [System.IO.File]::WriteAllText((Join-Path $stage 'README.md'), $kept, $utf8)
    if ($unlinked.Count -gt 0) { Write-Warning "README: $($unlinked.Count) image(s) without a link left out of the package" }

    $zip = Join-Path $dist "Repkarbinhake-$version-thunderstore.zip"
    Remove-Item $zip -ErrorAction SilentlyContinue
    & $tar -a -c -f $zip -C $stage @(Get-ChildItem $stage -Force | ForEach-Object Name)
    if ($LASTEXITCODE -ne 0) { throw 'Packing failed (thunderstore)' }
    Remove-Item $stage -Recurse -Force

    Get-Item $zip | ForEach-Object { Write-Host ("  {0}  {1:N0} KB" -f $_.Name, ($_.Length / 1KB)) }
}

Pack 'Public' 'release' ''
PackThunderstore
Pack 'Release' 'debug' $debugLines
Write-Host "Packed Repkarbinhake $version"
