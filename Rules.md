# 🚨 INSTRUCTIONS FOR AI AGENTS: ASSET STORE PACKAGE INSTALLATION

> **CRITICAL DIRECTIVE**: 
> - **DO NOT** write temporary or scratch `.cs` scripts.
> - **DO NOT** inspect or reverse-engineer Unity Editor internals.
> - **DO NOT** run `unity cmd package_*` (Asset Store assets are NOT UPM packages).
> - **DO NOT** manually download or unpack `.unitypackage` files.
> - **EXECUTE THE SINGLE COMMAND BELOW ONLY.**

---

## How to Install Packages (The Only Command You Need)

Whenever the user asks you to install or batch-install any Asset Store package(s), take whatever asset names the user requested and run **THIS SINGLE COMMAND**:

```powershell
powershell -ExecutionPolicy Bypass -Command "& (Resolve-Path 'Library/PackageCache/com.amaan.assetstorefetcher*/install.ps1').Path '<Asset 1>' '<Asset 2>' ..."
```

### Generic Syntax:
Pass every requested asset name as a separate argument in quotes. The script automatically handles fuzzy matching against the user's purchased assets catalog:
```powershell
# Single package:
powershell -ExecutionPolicy Bypass -Command "& (Resolve-Path 'Library/PackageCache/com.amaan.assetstorefetcher*/install.ps1').Path '<PackageName>'"

# Multiple packages (Batched):
powershell -ExecutionPolicy Bypass -Command "& (Resolve-Path 'Library/PackageCache/com.amaan.assetstorefetcher*/install.ps1').Path '<Package 1>' '<Package 2>'"
```

*(Alternative direct CLI invocation without PowerShell script path resolution)*:
```powershell
unity cmd eval --code "return AssetStoreFetcher.AssetStoreAutoInstaller.InstallList('<Package 1>;<Package 2>');"
```

---

## What the Script Automatically Handles:
1. **Always Regenerates Catalog**: Always regenerates/refreshes the asset list before installing to capture recent purchases, replacing the existing `UserSettings/asset_catalog.json` file in-place (never creating duplicate or new catalog files).
2. **Fuzzy Search**: Matches names against `UserSettings/asset_catalog.json`.
3. **Phase 1 (Parallel Download)**: Checks if any are missing from cache and downloads them simultaneously.
4. **Phase 2 (Atomic Install)**: Freezes assembly compilation (`LockReloadAssemblies`), unpackages all packages back-to-back, and unlocks compilation.
5. **Single Compile Pass**: Unity compiles **exactly once** for the entire batch.

---

## Checking Available Packages (0ms Lookup)
Do NOT run Unity commands to search for packages. Simply view:
`UserSettings/asset_catalog.json`

---

## Refreshing the Catalog (Regenerating the List)
The installer automatically regenerates the catalog before installing. To manually refresh/regenerate the list at any time (replacing the existing `UserSettings/asset_catalog.json`):
Run:
```powershell
powershell -ExecutionPolicy Bypass -Command "& (Resolve-Path 'Library/PackageCache/com.amaan.assetstorefetcher*/refresh.ps1').Path"
```
Or direct:
```powershell
unity cmd eval --code "return AssetStoreFetcher.AssetStoreAutoInstaller.BuildIndex();"
```
Or in the Unity Editor menu:
> **Tools** > **Amaan12** > **Asset Store Auto Installer** > **Refresh Asset Catalog**
