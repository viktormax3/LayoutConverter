param(
    [Parameter(Mandatory = $false)]
    [string]$Version = "1.0.0",

    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# Resolve paths
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$publishDir = Join-Path $repoRoot "publish"
$releaseDir = Join-Path $repoRoot "Release"
$zipName = "NW4R-LayoutTools-v$Version.zip"
$zipPath = Join-Path $repoRoot $zipName

Write-Host "`n--- Packaging Release v$Version ($Configuration) ---" -ForegroundColor Cyan

# 1. Cleanup previous runs
Write-Host "Cleaning up old artifacts..."
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
if (Test-Path $releaseDir) { Remove-Item $releaseDir -Recurse -Force }
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

# 2. Publish Projects
Write-Host "Publishing CLI (win-x64)..."
& dotnet publish (Join-Path $repoRoot "src\LayoutConverter.Cli\LayoutConverter.Cli.csproj") `
    -c $Configuration -r win-x64 --self-contained false -o (Join-Path $publishDir "cli") | Out-Null

Write-Host "Publishing GUI (win-x64)..."
& dotnet publish (Join-Path $repoRoot "src\LayoutConverter.Gui\LayoutConverter.Gui.csproj") `
    -c $Configuration -r win-x64 --self-contained false -o (Join-Path $publishDir "gui") | Out-Null

# 3. Assemble Release Folder
Write-Host "Assembling release folder..."
New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null

# Copy binaries (GUI and CLI share dependencies, so they can coexist in the same folder)
Copy-Item (Join-Path $publishDir "cli\*") $releaseDir -Force
Copy-Item (Join-Path $publishDir "gui\*") $releaseDir -Force
Copy-Item (Join-Path $repoRoot "README.md") $releaseDir -Force

# Remove debug symbols to keep it clean
Get-ChildItem $releaseDir -Filter *.pdb | Remove-Item -Force

# 4. Create ZIP Archive
Write-Host "Creating ZIP archive: $zipName"
Compress-Archive -Path (Join-Path $releaseDir "*") -DestinationPath $zipPath -Force

Write-Host "`nSuccessfully packaged! " -NoNewline -ForegroundColor Green
Write-Host "Location: $zipPath" -ForegroundColor White
Write-Host "You can now upload this ZIP to your GitHub Release page.`n"
