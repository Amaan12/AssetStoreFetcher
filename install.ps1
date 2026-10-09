param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Packages
)

if (-not $Packages -or $Packages.Count -eq 0) {
    Write-Host '{"status":"error","message":"No packages specified. Usage: powershell -File Packages/com.amaan.assetstorefetcher/install.ps1 <pkg1> <pkg2>..."}'
    exit 1
}

$unityBin = "unity"
if (-not (Get-Command "unity" -ErrorAction SilentlyContinue)) {
    if (Test-Path "$env:LOCALAPPDATA\Unity\bin\unity.exe") {
        $unityBin = "$env:LOCALAPPDATA\Unity\bin\unity.exe"
    }
}

$csv = ($Packages -join ";")
$b64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($csv))
$code = "return AssetStoreFetcher.AssetStoreAutoInstaller.InstallList(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(\`"$b64\`")));"

& $unityBin cmd eval --code $code
