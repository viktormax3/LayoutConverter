param(
    [Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
    [Alias("FullName")]
    [string[]] $InputPath,

    [string] $OutputRoot = "scratchs\rlan_roundtrip_validation",

    [string] $CliPath = "src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll",

    [switch] $Banner,

    [switch] $SplitByTag,

    [switch] $OmitSameKey,

    [switch] $OmitSameKeyAll,

    [switch] $BakeInfinity,

    [switch] $SkipVersionCheck,

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

function Get-OutputBrlans([string] $OutputDir) {
    $animDir = Join-Path $OutputDir "anim"
    $searchDir = if (Test-Path -LiteralPath $animDir) { $animDir } else { $OutputDir }
    $files = @(Get-ChildItem -LiteralPath $searchDir -Recurse -Filter "*.brlan" | Sort-Object FullName)
    if ($files.Count -eq 0) {
        throw "No BRLAN was produced in $OutputDir"
    }

    return $files
}

function Get-OutputRlan([string] $OutputDir, [string] $BaseName) {
    $expected = Join-Path $OutputDir (Join-Path "Layout" ($BaseName + ".rlan"))
    if (Test-Path -LiteralPath $expected) {
        return (Resolve-Path -LiteralPath $expected).Path
    }

    $candidate = Get-ChildItem -LiteralPath $OutputDir -Recurse -Filter "*.rlan" | Select-Object -First 1
    if ($candidate -eq $null) {
        throw "No RLAN was produced in $OutputDir"
    }

    return $candidate.FullName
}

function Expand-RlanInputs([string[]] $Paths) {
    foreach ($path in $Paths) {
        $resolved = Resolve-RepoPath $path
        $item = Get-Item -LiteralPath $resolved
        if ($item.PSIsContainer) {
            Get-ChildItem -LiteralPath $item.FullName -Recurse -Filter "*.rlan" | ForEach-Object { $_.FullName }
        }
        else {
            $resolved
        }
    }
}

function Get-CompileArgs() {
    $args = @()
    if ($Banner) {
        $args += "--banner"
    }
    if ($SplitByTag) {
        $args += "-g"
    }
    if ($OmitSameKey) {
        $args += "--omit-samekey"
    }
    if ($OmitSameKeyAll) {
        $args += "--omit-samekey-all"
    }
    if ($BakeInfinity) {
        $args += "--bake-infinity"
    }
    if ($SkipVersionCheck) {
        $args += "--no-check-version"
    }

    return $args
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
$allInputs = @(Expand-RlanInputs $InputPath)
$compileArgs = @(Get-CompileArgs)

foreach ($input in $allInputs) {
    try {
        $inputItem = Get-Item -LiteralPath $input
        if ($inputItem.Extension -ne ".rlan") {
            throw "Input is not an .rlan file: $input"
        }

        $caseName = [System.IO.Path]::GetFileNameWithoutExtension($inputItem.Name)
        $caseRoot = Join-Path $outputFullRoot $caseName
        $firstOutput = Join-Path $caseRoot "01_from_source_xml"
        $reverseOutput = Join-Path $caseRoot "02_reversed_xml"
        $secondOutput = Join-Path $caseRoot "03_from_reversed_xml"

        New-Item -ItemType Directory -Force -Path $firstOutput, $reverseOutput, $secondOutput | Out-Null

        Invoke-LayoutConverter ($compileArgs + @($inputItem.FullName, $firstOutput))
        $firstBrlans = @(Get-OutputBrlans $firstOutput)

        $passed = $true
        $sourceHashes = @()
        $roundTripHashes = @()
        $reversedRlans = @()

        foreach ($firstBrlan in $firstBrlans) {
            $brlanBaseName = [System.IO.Path]::GetFileNameWithoutExtension($firstBrlan.Name)
            $reverseCaseOutput = Join-Path $reverseOutput $brlanBaseName
            $secondCaseOutput = Join-Path $secondOutput $brlanBaseName
            New-Item -ItemType Directory -Force -Path $reverseCaseOutput, $secondCaseOutput | Out-Null

            Invoke-LayoutConverter @($firstBrlan.FullName, $reverseCaseOutput)
            $reversedRlan = Get-OutputRlan $reverseCaseOutput $brlanBaseName
            Invoke-LayoutConverter ($compileArgs + @($reversedRlan, $secondCaseOutput))
            $secondBrlans = @(Get-OutputBrlans $secondCaseOutput)

            if ($secondBrlans.Count -ne 1) {
                throw "Expected one BRLAN during second compile for $reversedRlan, got $($secondBrlans.Count)."
            }

            $sourceHash = Get-HashText $firstBrlan.FullName
            $roundTripHash = Get-HashText $secondBrlans[0].FullName
            $sourceHashes += $sourceHash
            $roundTripHashes += $roundTripHash
            $reversedRlans += $reversedRlan

            if ($sourceHash -ne $roundTripHash) {
                $passed = $false
            }
        }

        if (-not $passed) {
            $failures++
        }

        $results += [pscustomobject]@{
            Result = if ($passed) { "PASS" } else { "FAIL" }
            Input = $inputItem.FullName
            BinaryCount = $firstBrlans.Count
            SourceHashShort = ($sourceHashes | ForEach-Object { $_.Substring(0, 12) }) -join ","
            RoundTripHashShort = ($roundTripHashes | ForEach-Object { $_.Substring(0, 12) }) -join ","
            ReversedRlan = $reversedRlans -join "; "
        }

        Write-Host ("{0} {1}" -f $(if ($passed) { "PASS" } else { "FAIL" }), $inputItem.FullName)
    }
    catch {
        $failures++
        Write-Warning $_
        if (-not $KeepGoing) {
            throw
        }
    }
}

Write-Host ""
Write-Host "Summary:"
foreach ($result in $results) {
    Write-Host ("  {0} count={1} source={2} roundtrip={3} {4}" -f `
        $result.Result, `
        $result.BinaryCount, `
        $result.SourceHashShort, `
        $result.RoundTripHashShort, `
        $result.Input)
}

if ($failures -gt 0) {
    exit 1
}
