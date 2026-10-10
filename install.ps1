param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Packages,
    [switch]$KeepScript
)

$ErrorActionPreference = "Stop"

# 1. Verify current working directory is a Unity project
if (-not (Test-Path "ProjectSettings/ProjectVersion.txt")) {
    Write-Host "[Error] Not a Unity project directory! Run this command inside your Unity project root." -ForegroundColor Red
    exit 1
}

# 2. Prompt for package names if none provided (e.g. when run via irm ... | iex)
if (-not $Packages -or $Packages.Count -eq 0) {
    $inputPackages = Read-Host "Enter package names to install (comma or space separated)"
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

# 4. Ensure AssetStoreAutoInstaller.cs is available in Assets/Editor
$existingScript = Get-ChildItem -Path "Assets" -Filter "AssetStoreAutoInstaller.cs" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
$tempInstalled = $false
$targetScriptPath = "Assets/Editor/AssetStoreAutoInstaller.cs"

if (-not $existingScript) {
    if (-not (Test-Path "Assets/Editor")) {
        New-Item -ItemType Directory -Path "Assets/Editor" -Force | Out-Null
    }

    $localSource = if ($PSScriptRoot) { Join-Path $PSScriptRoot "AssetStoreAutoInstaller.cs" } else { $null }
    if ($localSource -and (Test-Path $localSource)) {
        Copy-Item -Path $localSource -Destination $targetScriptPath -Force
    } else {
        Write-Host "Fetching AssetStoreAutoInstaller.cs from GitHub..." -ForegroundColor Cyan
        $rawUrl = "https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/AssetStoreAutoInstaller.cs"
        Invoke-WebRequest -Uri $rawUrl -OutFile $targetScriptPath -UseBasicParsing
    }
    $tempInstalled = $true
}

try {
    $csv = ($Packages -join ";")
    Write-Host "Installing packages: $csv" -ForegroundColor Cyan
    $b64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($csv))
    $code = "return AssetStoreFetcher.AssetStoreAutoInstaller.InstallList(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(\`"$b64\`")));"
    & $unityBin cmd eval --code $code
}
finally {
    # Clean up temporary installer script unless -KeepScript was specified
    if ($tempInstalled -and -not $KeepScript) {
        if (Test-Path $targetScriptPath) {
            Remove-Item -Path $targetScriptPath -Force -ErrorAction SilentlyContinue
        }
        if (Test-Path "$targetScriptPath.meta") {
            Remove-Item -Path "$targetScriptPath.meta" -Force -ErrorAction SilentlyContinue
        }
    }
}
