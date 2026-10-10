# Asset Store Fetcher

Headless Unity Asset Store package installer and atomic batch downloader for instant CLI and AI automation.

> **Zero Project Footprint**: Runs on demand, refreshes catalog automatically, downloads in parallel, installs with locked compilation, and leaves zero residual files.

---

## Usage

Run inside your Unity project root:

### 1. PowerShell / Terminal (Cloud One-Liner)
```powershell
# Batch install:
& ([scriptblock]::Create((irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/install.ps1))) "<Package_1>" "<Package_2>"

# Interactive mode (prompts for names):
irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/install.ps1 | iex
```

### 2. Unity CLI (`unity cmd eval`)
```powershell
unity cmd eval --code "return AssetStoreFetcher.AssetStoreAutoInstaller.InstallList('<Package_1>;<Package_2>');"
```

---

## Features

* **Auto Catalog Refresh**: Syncs purchased catalog (`UserSettings/asset_catalog.json`) before every install.
* **Parallel Downloads**: Missing packages download concurrently in the background.
* **Atomic Batch Install**: Freezes compilation (`LockReloadAssemblies`) and unpacks all packages in a **single compilation pass**.
* **Fuzzy Matching**: Resolves queries via exact, prefix, or substring search.

---

## License

[MIT](LICENSE.md)
