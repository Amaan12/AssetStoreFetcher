$unityBin = "unity"
if (-not (Get-Command "unity" -ErrorAction SilentlyContinue)) {
    if (Test-Path "$env:LOCALAPPDATA\Unity\bin\unity.exe") {
        $unityBin = "$env:LOCALAPPDATA\Unity\bin\unity.exe"
    }
}

$code = "return AssetStoreFetcher.AssetStoreAutoInstaller.BuildIndex();"
& $unityBin cmd eval --code $code
