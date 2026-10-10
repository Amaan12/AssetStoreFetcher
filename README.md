# Asset Store Fetcher

Headless Unity Asset Store package installer and atomic batch downloader for instant CLI and AI automation.

> **100% Cloud / In-Memory**: Compiles and executes directly in memory via `unity cmd run_script` from `$env:TEMP`. Never touches `Assets/` or `Packages/`.

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

### 2. Direct Unity CLI (`unity cmd run_script`)
```powershell
$f="$env:TEMP\asf.cs"; irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/AssetStoreAutoInstaller.cs -OutFile $f; unity cmd run_script --file $f --entry "AssetStoreFetcher.AssetStoreAutoInstaller.InstallList" --args '[\"<Package_1>;<Package_2>\"]'; rm $f
```

---

## Features

* **In-Memory Execution**: Uses Roslyn (`run_script`) with zero project files and no domain reload overhead.
* **Auto Catalog Refresh**: Syncs purchased catalog (`UserSettings/asset_catalog.json`) before every install.
* **Parallel Downloads**: Missing packages download concurrently in the background.
* **Atomic Batch Install**: Freezes compilation (`LockReloadAssemblies`) and unpacks all packages in a **single compilation pass**.
* **Fuzzy Matching**: Resolves queries via exact, prefix, or substring search.

---

## License

[MIT](LICENSE.md)
