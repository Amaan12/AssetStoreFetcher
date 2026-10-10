# Asset Store Fetcher

Zero-footprint Unity Asset Store batch downloader & installer. Runs 100% in memory via `unity cmd run_script`.

## Usage (Run in Project Root)

### PowerShell
```powershell
# Batch install:
& ([scriptblock]::Create((irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/install.ps1))) "<Package_1>" "<Package_2>"

# Interactive:
irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/install.ps1 | iex
```

### Unity CLI
```powershell
$f="$env:TEMP\asf.cs"; (irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/AssetStoreAutoInstaller.cs).Replace("__PAYLOAD__","<Package_1>;<Package_2>") | Set-Content $f; unity cmd run_script --file $f --entry "AssetStoreFetcher.AssetStoreAutoInstaller.RunEmbedded"; rm $f
```

## How It Works
* **Zero Footprint**: Compiles in memory from `$env:TEMP`; never touches `Assets/` or `Packages/`.
* **Auto-Index**: Syncs purchased catalog (`UserSettings/asset_catalog.json`) automatically.
* **Parallel Download → Batch Import**: Downloads all missing packages concurrently, waits for 100% completion, then imports with locked assemblies (`LockReloadAssemblies`).
* **Fuzzy Search**: Matches exact, prefix, or substring package names.
