# 🚨 INSTRUCTIONS FOR AI AGENTS: ASSET STORE PACKAGE INSTALLATION

> **CRITICAL DIRECTIVE**: 
> - **DO NOT** write custom `.cs` scripts.
> - **DO NOT** manually unpack `.unitypackage` files.
> - **DO NOT** modify `Packages/manifest.json`.
> - **USE ONE OF THE TWO COMMANDS BELOW ONLY.**

---

## How to Install Packages

Run inside the Unity project root, passing the requested asset name(s):

### Method 1: PowerShell Cloud Command (Recommended)
```powershell
& ([scriptblock]::Create((irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/install.ps1))) "PackageName1" "PackageName2"
```

### Method 2: Direct Unity CLI (`unity cmd eval`)
```powershell
unity cmd eval --code "return AssetStoreFetcher.AssetStoreAutoInstaller.InstallList('PackageName1;PackageName2');"
```

---

## What It Automatically Handles:
1. **Auto Catalog Refresh**: Automatically rebuilds `UserSettings/asset_catalog.json` before installing.
2. **Parallel Download**: Checks cache and downloads missing packages concurrently in the background.
3. **Atomic Install**: Freezes assembly compilation (`LockReloadAssemblies`) and unpacks all packages in a single pass.
4. **Single Compile Pass**: Unity compiles **exactly once** for the entire batch.
