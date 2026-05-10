param(
    [Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
    [Alias("FullName")]
    [string[]] $InputPath,

    [string] $OutputRoot = "scratch_rlyt_roundtrip_validation",

    [string] $CliPath = "src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll",

    [switch] $Banner,

    [switch] $KeepGoing
)

$ErrorActionPreference = "Stop"

function Resolve-RepoPath([string] $Path) {
    if ([System.IO.Path]::IsPathRooted($Path)) {
        return (Resolve-Path -LiteralPath $Path).Path
    }

    return (Resolve-Path -LiteralPath (Join-Path $repoRoot $Path)).Path
}

function Invoke-LayoutConverter([string[]] $Arguments) {
    & dotnet $cliFullPath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "layout-converter failed with exit code ${LASTEXITCODE}: $($Arguments -join ' ')"
    }
}

function Get-HashText([string] $Path) {
    return (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash
}

function Get-OutputBrlyt([string] $OutputDir, [string] $BaseName) {
    $expected = Join-Path $OutputDir (Join-Path "blyt" ($BaseName + ".brlyt"))
    if (Test-Path -LiteralPath $expected) {
        return (Resolve-Path -LiteralPath $expected).Path
    }

    $candidate = Get-ChildItem -LiteralPath $OutputDir -Recurse -Filter "*.brlyt" | Select-Object -First 1
    if ($candidate -eq $null) {
        throw "No BRLYT was produced in $OutputDir"
    }

    return $candidate.FullName
}

function Get-OutputRlyt([string] $OutputDir, [string] $BaseName) {
    $expected = Join-Path $OutputDir (Join-Path "Layout" ($BaseName + ".rlyt"))
    if (Test-Path -LiteralPath $expected) {
        return (Resolve-Path -LiteralPath $expected).Path
    }

    $candidate = Get-ChildItem -LiteralPath $OutputDir -Recurse -Filter "*.rlyt" | Select-Object -First 1
    if ($candidate -eq $null) {
        throw "No RLYT was produced in $OutputDir"
    }

    return $candidate.FullName
}

function Expand-RlytInputs([string[]] $Paths) {
    foreach ($path in $Paths) {
        $resolved = Resolve-RepoPath $path
        $item = Get-Item -LiteralPath $resolved
        if ($item.PSIsContainer) {
            Get-ChildItem -LiteralPath $item.FullName -Recurse -Filter "*.rlyt" | ForEach-Object { $_.FullName }
        }
        else {
            $resolved
        }
    }
}

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$cliFullPath = Resolve-RepoPath $CliPath
$outputFullRoot = if ([System.IO.Path]::IsPathRooted($OutputRoot)) {
    $OutputRoot
}
else {
    Join-Path $repoRoot $OutputRoot
}

New-Item -ItemType Directory -Force -Path $outputFullRoot | Out-Null

$failures = 0
$results = @()
$allInputs = @(Expand-RlytInputs $InputPath)

foreach ($input in $allInputs) {
    try {
        $inputItem = Get-Item -LiteralPath $input
        if ($inputItem.Extension -ne ".rlyt") {
            throw "Input is not an .rlyt file: $input"
        }

        $caseName = [System.IO.Path]::GetFileNameWithoutExtension($inputItem.Name)
        $caseRoot = Join-Path $outputFullRoot $caseName
        $firstOutput = Join-Path $caseRoot "01_from_source_xml"
        $reverseOutput = Join-Path $caseRoot "02_reversed_xml"
        $secondOutput = Join-Path $caseRoot "03_from_reversed_xml"

        New-Item -ItemType Directory -Force -Path $firstOutput, $reverseOutput, $secondOutput | Out-Null

        $compileArgs = @()
        if ($Banner) {
            $compileArgs += "--banner"
        }

        Invoke-LayoutConverter ($compileArgs + @($inputItem.FullName, $firstOutput))
        $firstBrlyt = Get-OutputBrlyt $firstOutput $caseName

        Invoke-LayoutConverter @($firstBrlyt, $reverseOutput)
        $reversedRlyt = Get-OutputRlyt $reverseOutput ([System.IO.Path]::GetFileNameWithoutExtension($firstBrlyt))

        Invoke-LayoutConverter ($compileArgs + @($reversedRlyt, $secondOutput))
        $secondBrlyt = Get-OutputBrlyt $secondOutput ([System.IO.Path]::GetFileNameWithoutExtension($reversedRlyt))

        $sourceHash = Get-HashText $firstBrlyt
        $roundTripHash = Get-HashText $secondBrlyt
        $xmlHash = Get-HashText $inputItem.FullName
        $reversedXmlHash = Get-HashText $reversedRlyt
        $passed = $sourceHash -eq $roundTripHash

        if (-not $passed) {
            $failures++
        }

        $results += [pscustomobject]@{
            Result = if ($passed) { "PASS" } else { "FAIL" }
            Input = $inputItem.FullName
            FirstBrlyt = $firstBrlyt
            ReversedRlyt = $reversedRlyt
            SourceHash = $sourceHash
            RoundTripHash = $roundTripHash
            SourceHashShort = $sourceHash.Substring(0, 12)
            RoundTripHashShort = $roundTripHash.Substring(0, 12)
            XmlChanged = $xmlHash -ne $reversedXmlHash
        }

        Write-Host ("{0} {1}" -f $(if ($passed) { "PASS" } else { "FAIL" }), $inputItem.FullName)
    }
    catch {
        $failures++
        Write-Error $_
        if (-not $KeepGoing) {
            throw
        }
    }
}

Write-Host ""
Write-Host "Summary:"
foreach ($result in $results) {
    Write-Host ("  {0} xmlChanged={1} source={2} roundtrip={3} {4}" -f `
        $result.Result, `
        $result.XmlChanged, `
        $result.SourceHashShort, `
        $result.RoundTripHashShort, `
        $result.Input)
}

foreach ($result in $results) {
    if ($result.Result -ne "PASS") {
        Write-Host ""
        Write-Host "Failure details:"
        Write-Host "  Input:          $($result.Input)"
        Write-Host "  First BRLYT:    $($result.FirstBrlyt)"
        Write-Host "  Reversed RLYT:  $($result.ReversedRlyt)"
        Write-Host "  Source hash:    $($result.SourceHash)"
        Write-Host "  Roundtrip hash: $($result.RoundTripHash)"
    }
}

if ($failures -gt 0) {
    exit 1
}
