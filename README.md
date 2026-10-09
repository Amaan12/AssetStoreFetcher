# Asset Store Fetcher for Unity (`com.amaan.assetstorefetcher`)

A fast, headless Unity Asset Store package fetcher and atomic batch installer designed for instant CLI automation and AI coding agents.

## Installation

### Install Asset Store Fetcher
In Unity Package Manager (`Window > Package Manager`), click `+`, select **Add package from git URL...**, and enter:
```
https://github.com/Amaan12/AssetStoreFetcher.git
```

Or add directly to `Packages/manifest.json`:
```json
"dependencies": {
  "com.amaan.assetstorefetcher": "https://github.com/Amaan12/AssetStoreFetcher.git"
}
```

## Features

- **0ms Asset Lookup**: Fast local asset catalog cache stored in `UserSettings/asset_catalog.json`.
- **Parallel Downloading**: Downloads missing packages concurrently in the background.
- **Atomic Batch Installation**: Freezes domain reload (`LockReloadAssemblies`) and unpacks all packages in a single compilation pass.
- **CLI Automation**: One-line PowerShell scripts (`install.ps1`, `refresh.ps1`) callable by AI agents or CI/CD pipelines via `unity cmd eval`.
- **In-Editor Tools**: Menu items under `Tools > Amaan12 > Asset Store Auto Installer`.

## Usage & Automation

### For AI Coding Agents & CLI
Install single or multiple packages with fuzzy name matching in a single pass:
```powershell
powershell -ExecutionPolicy Bypass -Command "& (Resolve-Path 'Library/PackageCache/com.amaan.assetstorefetcher*/install.ps1').Path '<PackageName1>' '<PackageName2>'"
```

Or direct `unity cmd eval`:
```powershell
unity cmd eval --code "return AssetStoreFetcher.AssetStoreAutoInstaller.InstallList('<PackageName1>;<PackageName2>');"
```

Refresh/regenerate the asset catalog cache:
```powershell
powershell -ExecutionPolicy Bypass -Command "& (Resolve-Path 'Library/PackageCache/com.amaan.assetstorefetcher*/refresh.ps1').Path"
```

### In Unity Editor
- **Tools > Amaan12 > Asset Store Auto Installer > Refresh Asset Catalog**: Regenerates the local asset catalog.
- **Tools > Amaan12 > Asset Store Auto Installer > Open Catalog JSON**: Inspects `UserSettings/asset_catalog.json`.
- **Tools > Amaan12 > Asset Store Auto Installer > Open Rules File**: Opens `Rules.md`.

## Instructions for AI Agents
Refer to [`Rules.md`](Rules.md) for directives and syntax guidelines.

## Dependencies
None.
