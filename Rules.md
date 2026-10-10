# 🚨 INSTRUCTIONS FOR AI AGENTS: ASSET STORE PACKAGE INSTALLATION

> **CRITICAL DIRECTIVE**: 
> - **DO NOT** write custom `.cs` scripts.
> - **DO NOT** manually unpack `.unitypackage` files.
> - **DO NOT** modify `Packages/manifest.json`.
> - **EXECUTE THE SINGLE COMMAND BELOW ONLY.**

---

## How to Install Packages (The Only Command You Need)

Run this command inside the Unity project root, passing the requested asset name(s):

```powershell
& ([scriptblock]::Create((irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/install.ps1))) "PackageName1" "PackageName2"
```

### Examples:
```powershell
# Single package:
& ([scriptblock]::Create((irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/install.ps1))) "Odin Inspector"

# Multiple packages (Atomic Batch):
& ([scriptblock]::Create((irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/install.ps1))) "Odin Inspector" "PrimeTween" "Feel"
```

---

## Refreshing the Catalog

To rebuild `UserSettings/asset_catalog.json`:
```powershell
irm https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/refresh.ps1 | iex
```
