param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Packages
)

$ErrorActionPreference = "Stop"

# 1. Verify current working directory is a Unity project
if (-not (Test-Path "ProjectSettings/ProjectVersion.txt")) {
    Write-Host "[Error] Not a Unity project directory! Run this command inside your Unity project root." -ForegroundColor Red
    exit 1
}

# 2. Prompt for package names if none provided (e.g. when run via irm ... | iex)
if (-not $Packages -or $Packages.Count -eq 0) {
    $inputPackages = Read-Host "Enter package names to install (comma or semicolon separated)"
    if ($inputPackages) {
        $Packages = $inputPackages -split "[,;]" | ForEach-Object { $_.Trim() } | Where-Object { $_ }
    }
    if (-not $Packages -or $Packages.Count -eq 0) {
        Write-Host "[Error] No packages specified." -ForegroundColor Red
        exit 1
    }
}

# 3. Locate unity CLI binary
$unityBin = "unity"
if (-not (Get-Command "unity" -ErrorAction SilentlyContinue)) {
    if (Test-Path "$env:LOCALAPPDATA\Unity\bin\unity.exe") {
        $unityBin = "$env:LOCALAPPDATA\Unity\bin\unity.exe"
    }
}

# 4. Fetch script into OS Temp directory (never touches Unity's Assets/ folder)
$tempScript = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "AssetStoreAutoInstaller_" + [guid]::NewGuid().ToString("N") + ".cs")

try {
    $localSource = if ($PSScriptRoot) { Join-Path $PSScriptRoot "AssetStoreAutoInstaller.cs" } else { $null }
    if ($localSource -and (Test-Path $localSource)) {
        Copy-Item -Path $localSource -Destination $tempScript -Force
    } else {
        Write-Host "Fetching installer into memory from GitHub..." -ForegroundColor Cyan
        $rawUrl = "https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/AssetStoreAutoInstaller.cs"
        Invoke-WebRequest -Uri $rawUrl -OutFile $tempScript -UseBasicParsing
    }

    $csv = ($Packages -join ";")
    Write-Host "Installing packages: $csv" -ForegroundColor Cyan

    # Escape quotes for native CLI JSON array argument
    $escapedCsv = $csv.Replace('\', '\\').Replace('"', '\"')
    $argsParam = "[\`"$escapedCsv\`"]"

    # Compile and execute directly in memory via Roslyn (no domain reload, zero project files)
    & $unityBin cmd run_script --file $tempScript --entry "AssetStoreFetcher.AssetStoreAutoInstaller.InstallList" --args $argsParam
}
finally {
    if (Test-Path $tempScript) {
        Remove-Item -Path $tempScript -Force -ErrorAction SilentlyContinue
    }
}
