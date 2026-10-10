# Asset Store Fetcher

Headless Unity Asset Store package installer and atomic batch downloader designed for instant CLI automation and AI coding agents.

> **Zero Project Footprint**: Runs on demand, automatically refreshes your catalog, downloads packages in parallel, installs with locked compilation, and leaves zero residual files in your project.

---

## Usage

Run either via standard terminal or directly through Unity's CLI from the root of your Unity project:

### 1. PowerShell / Terminal (Cloud One-Liner)

No cloning or repository setup needed.

**Batch install packages:**
```powershell
& ([scriptblock]::Create((irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/install.ps1))) "Odin Inspector" "PrimeTween"
```

**Interactive mode (prompts for package names):**
```powershell
irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/install.ps1 | iex
```

### 2. Unity CLI (`unity cmd eval`)

If you have the Unity CLI available in your environment:

```powershell
unity cmd eval --code "return AssetStoreFetcher.AssetStoreAutoInstaller.InstallList('Odin Inspector;PrimeTween');"
```

---

## Features

* **Auto Catalog Refresh**: Automatically rebuilds and synchronizes your purchased asset catalog (`UserSettings/asset_catalog.json`) on every install.
* **Parallel Downloads**: Missing packages download concurrently in the background.
* **Atomic Batch Install**: Freezes assembly compilation (`LockReloadAssemblies`) and unpacks all packages in a **single compilation pass**.
* **Fuzzy Matching**: Matches queries against your catalog using exact, prefix, or substring search.

---

## License

[MIT](LICENSE.md)
