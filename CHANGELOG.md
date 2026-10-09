# Changelog

All notable changes to this package will be documented in this file.

## [1.0.0] - 2026-10-09
### Added
- Initial package release of Asset Store Fetcher (`com.amaan.assetstorefetcher`).
- Headless Asset Store catalog indexing and local caching (`UserSettings/asset_catalog.json`).
- Parallel downloading of missing packages directly from Asset Store cache.
- Atomic batch installation pipeline with locked assembly compilation (`LockReloadAssemblies`) for a single compilation pass.
- Automation PowerShell scripts (`install.ps1`, `refresh.ps1`) for seamless agent and CLI invocation via `unity cmd eval`.
- Editor assembly definition (`AssetStoreFetcher.Editor.asmdef`).
