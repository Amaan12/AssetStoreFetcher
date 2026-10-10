# Quick catalog refresh one-liner
& ([scriptblock]::Create((Invoke-RestMethod https://raw.githubusercontent.com/Amaan12/AssetStoreFetcher/main/install.ps1))) -RefreshOnly
