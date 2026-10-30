#   .\installer\build.ps1            build only
#   .\installer\build.ps1 -Release   build, tag and upload a GitHub release
param([switch]$Release, [switch]$Draft)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$match = Select-String -Path "$root\AppInfo.cs" -Pattern 'VERSION = "(v(\d+\.\d+\.\d+))"'
if (-not $match) { throw 'Could not find VERSION = "vX.X.X" in AppInfo.cs' }
$tag = $match.Matches[0].Groups[1].Value
$version = $match.Matches[0].Groups[2].Value
Write-Host "Building OrangWallpapers $tag" -ForegroundColor Cyan

if ($Release) {
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { throw "GitHub CLI not found. Install it with: winget install GitHub.cli, then run: gh auth login" }
    if (git -C $root status --porcelain) { throw "Working tree has uncommitted changes. Commit them before releasing." }
    git -C $root fetch --tags --quiet
    if (git -C $root tag --list $tag) { throw "Tag $tag already exists. Bump VERSION in AppInfo.cs first." }
}

Remove-Item "$PSScriptRoot\publish", "$PSScriptRoot\output" -Recurse -Force -ErrorAction SilentlyContinue
dotnet publish "$root\OrangWallpapers.csproj" -c Release -r win-x86 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "$PSScriptRoot\publish"
if ($LASTEXITCODE) { exit $LASTEXITCODE }

$iscc = @("$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe", "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe", "$env:ProgramFiles\Inno Setup 6\ISCC.exe") |
    Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) { throw "Inno Setup 6 not found. Install it with: winget install JRSoftware.InnoSetup" }

& $iscc "/DAppVersion=$version" "$PSScriptRoot\OrangWallpapers.iss"
if ($LASTEXITCODE) { exit $LASTEXITCODE }

# portable exe next to the installer
$setup = "$PSScriptRoot\output\OrangWallpapers-Setup-$tag.exe"
$portable = "$PSScriptRoot\output\OrangWallpapers-$tag-portable.exe"
Copy-Item "$PSScriptRoot\publish\OrangWallpapers.exe" $portable
Write-Host "Built:`n  $setup`n  $portable" -ForegroundColor Green

if ($Release) {
    git -C $root tag $tag
    git -C $root push origin $tag
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
    $ghArgs = @('release', 'create', $tag, $setup, $portable, '--title', "OrangWallpapers $tag", '--generate-notes')
    if ($Draft) { $ghArgs += '--draft' }
    gh @ghArgs
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
    Write-Host "Released $tag" -ForegroundColor Green
}