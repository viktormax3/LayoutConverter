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

# 3. Assemble Release Folders
Write-Host "Assembling release folders..."
New-Item -ItemType Directory -Path (Join-Path $repoRoot "Release-Cli") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $repoRoot "Release-Gui") -Force | Out-Null

# Copy files
Copy-Item (Join-Path $publishDir "cli\*") (Join-Path $repoRoot "Release-Cli") -Force
Copy-Item (Join-Path $repoRoot "README.md") (Join-Path $repoRoot "Release-Cli") -Force

Copy-Item (Join-Path $publishDir "gui\*") (Join-Path $repoRoot "Release-Gui") -Force
Copy-Item (Join-Path $repoRoot "README.md") (Join-Path $repoRoot "Release-Gui") -Force

# Remove debug symbols
Get-ChildItem (Join-Path $repoRoot "Release-Cli") -Filter *.pdb | Remove-Item -Force
Get-ChildItem (Join-Path $repoRoot "Release-Gui") -Filter *.pdb | Remove-Item -Force

# 4. Create ZIP Archives
$zipCli = "NW4R-LayoutTools-CLI-v$Version.zip"
$zipGui = "NW4R-LayoutTools-GUI-v$Version.zip"

Write-Host "Creating ZIP archives..."
Compress-Archive -Path (Join-Path $repoRoot "Release-Cli\*") -DestinationPath (Join-Path $repoRoot $zipCli) -Force
Compress-Archive -Path (Join-Path $repoRoot "Release-Gui\*") -DestinationPath (Join-Path $repoRoot $zipGui) -Force

Write-Host "`nSuccessfully packaged! " -NoNewline -ForegroundColor Green
Write-Host "Location: $repoRoot" -ForegroundColor White
Write-Host "Files created:"
Write-Host " - $zipCli"
Write-Host " - $zipGui"
Write-Host "You can now upload these ZIPs to your GitHub Release page.`n"
