# Asset Store Fetcher

Headless Unity Asset Store package installer and atomic batch downloader designed for instant CLI automation and AI coding agents.

> **Zero Project Footprint**: No package needed in `Packages/manifest.json`. Runs on demand, installs packages with locked compilation, and leaves no residual files.

---

## Quick Install (PowerShell)

Inside your Unity project root, run:

```powershell
& ([scriptblock]::Create((irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/install.ps1))) "Odin Inspector" "PrimeTween"
```

Or run interactively (prompts for package names):
```powershell
irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/install.ps1 | iex
```

---

## Features

* **Parallel Downloads**: Missing packages download concurrently in the background.
* **Atomic Batch Install**: Freezes assembly compilation (`LockReloadAssemblies`) and unpacks all packages in a **single compilation pass**.
* **Fuzzy Matching**: Matches package queries against your purchased assets catalog in `UserSettings/asset_catalog.json`.
* **Zero Project Overhead**: Leaves no Git package dependencies or temporary scripts in your repository.

---

## Refresh Asset Catalog

To refresh/rebuild your local catalog cache (`UserSettings/asset_catalog.json`):
```powershell
irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/refresh.ps1 | iex
```

---

## License

[MIT](LICENSE.md)
