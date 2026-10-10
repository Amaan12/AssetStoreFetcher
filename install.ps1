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
        $scriptContent = [System.IO.File]::ReadAllText($localSource)
    } else {
        Write-Host "Fetching installer into memory from GitHub..." -ForegroundColor Cyan
        $rawUrl = "https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/AssetStoreAutoInstaller.cs"
        $scriptContent = (Invoke-WebRequest -Uri $rawUrl -UseBasicParsing).Content
    }

    $csv = ($Packages -join ";")
    Write-Host "Installing packages: $csv" -ForegroundColor Cyan

    # Embed Base64 payload directly into temp script to avoid CLI argument splitting on spaces
    $b64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($csv))
    $scriptContent = $scriptContent.Replace("__PAYLOAD_B64__", $b64)
    [System.IO.File]::WriteAllText($tempScript, $scriptContent)

    # Compile and execute directly in memory via Roslyn (no domain reload, zero project files)
    & $unityBin cmd run_script --file $tempScript --entry "AssetStoreFetcher.AssetStoreAutoInstaller.RunEmbedded"
}
finally {
    if (Test-Path $tempScript) {
        Remove-Item -Path $tempScript -Force -ErrorAction SilentlyContinue
    }
}
