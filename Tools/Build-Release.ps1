param(
    [string]$SptRoot = 'D:\Tarkov-SPT-4.1',
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$SptRoot = (Resolve-Path -LiteralPath $SptRoot).Path
Push-Location $repo
try {
    & dotnet build Plugin/DynamicMaps.csproj -c $Configuration "-p:TarkovDir=$SptRoot/" -p:DeployOnBuild=false --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Client build failed' }
    & dotnet build Server/mpstark-dynamicmaps.csproj -c $Configuration "-p:SptInstallRoot=$SptRoot" --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Server build failed' }
    & dotnet run --project Tests/DynamicMaps.RegressionTests.csproj -c $Configuration "-p:SptInstallRoot=$SptRoot" -- $SptRoot
    if ($LASTEXITCODE -ne 0) { throw 'Regression checks failed' }

    $version = (Get-Content Plugin/VERSION.txt -Raw).Trim()
    $artifacts = Join-Path $repo 'artifacts'
    $stage = Join-Path $artifacts ('package-' + [guid]::NewGuid().ToString('N'))
    $client = Join-Path $stage 'BepInEx/plugins/DynamicMaps'
    $server = Join-Path $stage 'SPT_Runtime/user/mods/DynamicMaps'
    New-Item -ItemType Directory -Path $client, $server -Force | Out-Null
    Copy-Item -LiteralPath "Plugin/bin/$Configuration/netstandard2.1/DynamicMaps.dll", "Plugin/bin/$Configuration/netstandard2.1/DynamicMaps.Common.dll", 'Plugin/LICENSE' -Destination $client
    Copy-Item -Path 'Plugin/Resources/*', 'Plugin/Libraries/*' -Destination $client -Recurse
    $serverOutput = "Server/bin/$Configuration/mpstark-dynamicmaps"
    Copy-Item -LiteralPath "$serverOutput/mpstark-dynamicmaps.dll", "$serverOutput/DynamicMaps.Common.dll", 'Server/config.json', 'Server/LICENSE' -Destination $server
    Copy-Item -LiteralPath 'PORT-4.1.md' -Destination $stage

    $archive = Join-Path $artifacts "DynamicMaps-$version-SPT4.1.zip"
    if (Test-Path -LiteralPath $archive) { Remove-Item -LiteralPath $archive }
    [System.IO.Compression.ZipFile]::CreateFromDirectory($stage, $archive)
    $zip = [System.IO.Compression.ZipFile]::OpenRead($archive)
    try {
        foreach ($expected in @('BepInEx/plugins/DynamicMaps/DynamicMaps.dll', 'BepInEx/plugins/DynamicMaps/DynamicMaps.Common.dll',
            'BepInEx/plugins/DynamicMaps/dynamicmaps-composite', 'BepInEx/plugins/DynamicMaps/dynamicmaps-postfx',
            'BepInEx/plugins/DynamicMaps/Unity.VectorGraphics.dll', 'BepInEx/plugins/DynamicMaps/Unity.InternalAPIEngineBridge.003.dll',
            'SPT_Runtime/user/mods/DynamicMaps/mpstark-dynamicmaps.dll', 'SPT_Runtime/user/mods/DynamicMaps/DynamicMaps.Common.dll',
            'SPT_Runtime/user/mods/DynamicMaps/config.json')) {
            if ($null -eq $zip.GetEntry($expected)) { throw "Archive missing $expected" }
        }
        foreach ($entry in $zip.Entries) {
            if ($entry.FullName -match '(^/|(^|/)\.\.(/|$)|Assembly-CSharp\.dll|SPTarkov\..*\.dll)') {
                throw "Unexpected archive entry: $($entry.FullName)"
            }
        }
        Write-Host "Validated $($zip.Entries.Count) archive entries."
    } finally { $zip.Dispose() }
    Get-FileHash -LiteralPath $archive -Algorithm SHA256
} finally { Pop-Location }
