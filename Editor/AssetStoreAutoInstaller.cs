#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AssetStoreFetcher
{
    [InitializeOnLoad]
    public static class AssetStoreAutoInstaller
    {
        private const string CatalogPath = "UserSettings/asset_catalog.json";
        private const string SessionKeyBatchState = "AssetStoreAutoInstaller_BatchState";

        [Serializable]
        public class AssetEntry
        {
            public string name;
            public long productId;
            public string packageUniqueId;
        }

        [Serializable]
        public class AssetCatalog
        {
            public List<AssetEntry> assets = new List<AssetEntry>();
        }

        [Serializable]
        public class BatchJobState
        {
            public string batchId = "";
            public List<long> allProductIds = new List<long>();
            public List<string> allNames = new List<string>();
            public List<long> pendingDownloads = new List<long>();
            public List<long> pendingInstalls = new List<long>();
            public string phase = "idle"; // "idle", "downloading", "installing", "completed", "error"
            public string errorMessage = "";
        }

        private static AssetCatalog _cachedCatalog;

        static AssetStoreAutoInstaller()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        #region Menu Items

        [MenuItem("Tools/Amaan12/Asset Store Auto Installer/Refresh Asset Catalog")]
        public static void RefreshCatalogMenuItem()
        {
            var res = BuildIndex();
            Debug.Log($"[AssetStoreAutoInstaller] {res}");
        }

        [MenuItem("Tools/Amaan12/Asset Store Auto Installer/Open Catalog JSON")]
        public static void OpenCatalogMenuItem()
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(CatalogPath);
            if (asset != null)
                Selection.activeObject = asset;
            else
                EditorUtility.RevealInFinder(CatalogPath);
        }

        [MenuItem("Tools/Amaan12/Asset Store Auto Installer/Open Rules File")]
        public static void OpenRulesMenuItem()
        {
            string[] searchPaths = {
                "Packages/com.amaan.assetstorefetcher/Rules.md",
                "Assets/AssetStoreFetcher/Rules.md",
                "Assets/AssetStoreAutoInstallerHelper/Rules.md"
            };

            string foundPath = searchPaths.FirstOrDefault(File.Exists);
            if (string.IsNullOrEmpty(foundPath))
            {
                var guids = AssetDatabase.FindAssets("Rules");
                foreach (var guid in guids)
                {
                    var p = AssetDatabase.GUIDToAssetPath(guid);
                    if (p.EndsWith("Rules.md", StringComparison.OrdinalIgnoreCase))
                    {
                        foundPath = p;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(foundPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(foundPath);
                if (asset != null)
                    Selection.activeObject = asset;
                else
                    EditorUtility.RevealInFinder(foundPath);
            }
            else
            {
                Debug.LogWarning("[AssetStoreAutoInstaller] Rules.md not found.");
            }
        }

        #endregion

        #region Catalog Management

        public static string BuildIndex()
        {
            try
            {
                if (!GetContext(out _, out _, out var dlMgr, out var pkgDb, out var myAssetsPage))
                    return "Error: Could not access Package Manager context. Ensure Package Manager window can open.";

                var cacheField = dlMgr.GetType().GetField("m_AssetStoreCache", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var cache = cacheField?.GetValue(dlMgr);
                var pInfosField = cache?.GetType().GetField("m_PurchaseInfos", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var pInfos = pInfosField?.GetValue(cache) as IDictionary;

                if (pInfos == null || pInfos.Count < 50)
                {
                    FetchAllPurchases(myAssetsPage);
                    pInfos = pInfosField?.GetValue(cache) as IDictionary;
                }

                var catalog = new AssetCatalog();

                if (pInfos != null && pInfos.Count > 0)
                {
                    foreach (DictionaryEntry kv in pInfos)
                    {
                        var info = kv.Value;
                        if (info == null) continue;

                        var prodId = (long)(info.GetType().GetField("productId", BindingFlags.Public | BindingFlags.NonInternal | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(info) ?? 0L);
                        var name = info.GetType().GetField("displayName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(info)?.ToString();

                        if (!string.IsNullOrEmpty(name) && prodId > 0)
                        {
                            catalog.assets.Add(new AssetEntry
                            {
                                name = name,
                                productId = prodId,
                                packageUniqueId = prodId.ToString()
                            });
                        }
                    }
                }
                else
                {
                    var allPkgs = pkgDb.GetType().GetProperty("allPackages")?.GetValue(pkgDb) as IEnumerable;
                    if (allPkgs != null)
                    {
                        foreach (var p in allPkgs)
                        {
                            if (p == null) continue;
                            var name = p.GetType().GetProperty("displayName")?.GetValue(p)?.ToString() 
                                       ?? p.GetType().GetProperty("name")?.GetValue(p)?.ToString();
                            var idStr = p.GetType().GetProperty("uniqueId")?.GetValue(p)?.ToString();

                            if (!string.IsNullOrEmpty(name) && long.TryParse(idStr, out var prodId))
                            {
                                catalog.assets.Add(new AssetEntry
                                {
                                    name = name,
                                    productId = prodId,
                                    packageUniqueId = idStr
                                });
                            }
                        }
                    }
                }

                catalog.assets = catalog.assets.OrderBy(a => a.name).ToList();
                _cachedCatalog = catalog;

                var dir = Path.GetDirectoryName(CatalogPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonUtility.ToJson(catalog, true);
                File.WriteAllText(CatalogPath, json);
                if (CatalogPath.StartsWith("Assets"))
                    AssetDatabase.Refresh();

                return $"Successfully indexed {catalog.assets.Count} assets to {CatalogPath}";
            }
            catch (Exception ex)
            {
                return $"Exception while indexing: {ex.Message}";
            }
        }

        private static void FetchAllPurchases(object myAssetsPage)
        {
            try
            {
                var clientField = myAssetsPage?.GetType().GetField("m_AssetStoreClient", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var client = clientField?.GetValue(myAssetsPage);
                if (client == null) return;

                var queryArgsType = typeof(Editor).Assembly.GetType("UnityEditor.PackageManager.UI.Internal.PurchasesQueryArgs");
                var ctor = queryArgsType?.GetConstructor(new[] { typeof(int), typeof(int), typeof(string), typeof(Editor).Assembly.GetType("UnityEditor.PackageManager.UI.Internal.PageFilters") });
                var args = ctor?.Invoke(new object[] { 0, 1000, "", null });

                var listMethod = client.GetType().GetMethod("ListPurchases", new[] { queryArgsType });
                listMethod?.Invoke(client, new object[] { args });
            }
            catch { }
        }

        public static AssetCatalog GetCatalog()
        {
            if (_cachedCatalog != null && _cachedCatalog.assets.Count > 0)
                return _cachedCatalog;

            if (File.Exists(CatalogPath))
            {
                try
                {
                    var json = File.ReadAllText(CatalogPath);
                    _cachedCatalog = JsonUtility.FromJson<AssetCatalog>(json);
                    if (_cachedCatalog != null && _cachedCatalog.assets.Count > 0)
                        return _cachedCatalog;
                }
                catch { }
            }

            BuildIndex();
            return _cachedCatalog;
        }

        public static string Search(string query)
        {
            var catalog = GetCatalog();
            if (catalog == null || catalog.assets.Count == 0)
                return "Catalog is empty. Call BuildIndex() first.";

            var matches = catalog.assets
                .Where(a => a.name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            if (matches.Count == 0)
                return $"No assets found matching '{query}'.";

            var lines = matches.Select(m => $"{m.name} [ID: {m.productId}]");
            return $"Found {matches.Count} match(es):\n" + string.Join("\n", lines);
        }

        public static AssetEntry FindAsset(string query)
        {
            var catalog = GetCatalog();
            if (catalog == null || catalog.assets.Count == 0) return null;

            var exact = catalog.assets.FirstOrDefault(a => a.name.Equals(query, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;

            var startsWith = catalog.assets.FirstOrDefault(a => a.name.StartsWith(query, StringComparison.OrdinalIgnoreCase));
            if (startsWith != null) return startsWith;

            return catalog.assets.FirstOrDefault(a => a.name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        #endregion

        #region Installation & True Batching Pipeline

        public static string Install(string query)
        {
            return InstallBatch(query);
        }

        public static string InstallList(string commaSeparatedQueries)
        {
            if (string.IsNullOrWhiteSpace(commaSeparatedQueries))
                return "{\"status\":\"error\",\"message\":\"Empty query string.\"}";

            var items = commaSeparatedQueries.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            return InstallBatch(items);
        }

        public static string InstallBatch(params string[] queries)
        {
            if (queries == null || queries.Length == 0)
                return "{\"status\":\"error\",\"message\":\"No package queries provided.\"}";

            // Always regenerate the list before installing (replaces the existing catalog file)
            BuildIndex();

            var catalog = GetCatalog();
            if (catalog == null || catalog.assets.Count == 0)
                return "{\"status\":\"error\",\"message\":\"Catalog is empty. Call BuildIndex() first.\"}";

            if (!GetContext(out var opDisp, out var installer, out var dlMgr, out _, out _))
                return "{\"status\":\"error\",\"message\":\"Failed to get Package Manager context.\"}";

            var toInstall = new List<AssetEntry>();
            var missing = new List<string>();

            foreach (var q in queries)
            {
                var asset = FindAsset(q);
                if (asset != null)
                {
                    if (!toInstall.Any(x => x.productId == asset.productId))
                        toInstall.Add(asset);
                }
                else
                {
                    missing.Add(q);
                }
            }

            if (toInstall.Count == 0)
                return $"{{\"status\":\"error\",\"message\":\"None of the specified assets were found: {string.Join(", ", missing)}\"}}";

            var needDownload = new List<AssetEntry>();
            foreach (var a in toInstall)
            {
                SelectPackageInUI(a.packageUniqueId);
                if (IsDownloadNeeded(a.productId, dlMgr))
                {
                    needDownload.Add(a);
                }
            }

            // Create persistent Batch State
            var state = new BatchJobState
            {
                batchId = Guid.NewGuid().ToString("N"),
                allProductIds = toInstall.Select(x => x.productId).ToList(),
                allNames = toInstall.Select(x => x.name).ToList(),
                pendingDownloads = needDownload.Select(x => x.productId).ToList(),
                pendingInstalls = toInstall.Select(x => x.productId).ToList(),
                phase = needDownload.Count > 0 ? "downloading" : "installing"
            };

            SaveBatchState(state);

            // Phase 1: If any require downloading, queue all downloads simultaneously
            if (needDownload.Count > 0)
            {
                foreach (var a in needDownload)
                {
                    TriggerPackageDownload(a.productId, opDisp, dlMgr);
                }

                return $"{{\"status\":\"downloading\",\"batchId\":\"{state.batchId}\",\"total\":{toInstall.Count},\"downloading\":{needDownload.Count},\"message\":\"Phase 1 started: Downloading {needDownload.Count} assets. Compilation locked until all complete.\"}}";
            }

            // Phase 2: Everything already downloaded -> Atomic sequential install with 1 compile pass
            ExecuteAtomicBatchInstall(installer, state);
            return $"{{\"status\":\"completed\",\"batchId\":\"{state.batchId}\",\"total\":{toInstall.Count},\"message\":\"Batch install completed for {toInstall.Count} asset(s). Script compilation started.\"}}";
        }

        public static string GetBatchStatus()
        {
            var state = LoadBatchState();
            if (state == null || string.IsNullOrEmpty(state.batchId) || state.phase == "idle")
                return "{\"status\":\"idle\",\"message\":\"No active batch job.\"}";

            return JsonUtility.ToJson(state);
        }

        private static void TriggerPackageDownload(long productId, object opDisp, object dlMgr)
        {
            try
            {
                // Try via opDisp first (proper license token handshake)
                var dlMethod = opDisp.GetType().GetMethod("Download", new[] { typeof(IEnumerable<long>) });
                if (dlMethod != null)
                {
                    dlMethod.Invoke(opDisp, new object[] { new[] { productId } });
                    return;
                }
            }
            catch { }

            try
            {
                var dlMethod = dlMgr.GetType().GetMethod("Download", new[] { typeof(IEnumerable<long>) });
                dlMethod?.Invoke(dlMgr, new object[] { new[] { productId } });
            }
            catch { }
        }

        private static void OnEditorUpdate()
        {
            var state = LoadBatchState();
            if (state == null || state.phase != "downloading" || state.pendingDownloads.Count == 0)
                return;

            if (!GetContext(out _, out var installer, out var dlMgr, out _, out _))
                return;

            var finished = new List<long>();
            foreach (var id in state.pendingDownloads)
            {
                // Check if operation finished or file is now in local cache
                if (!IsDownloadNeeded(id, dlMgr))
                {
                    finished.Add(id);
                    continue;
                }

                var getOp = dlMgr.GetType().GetMethod("GetDownloadOperation", new[] { typeof(long?) });
                var op = getOp?.Invoke(dlMgr, new object[] { (long?)id });
                if (op != null)
                {
                    var inProg = (bool)(op.GetType().GetProperty("isInProgress")?.GetValue(op) ?? false);
                    var err = op.GetType().GetProperty("errorMessage")?.GetValue(op)?.ToString();
                    if (!string.IsNullOrEmpty(err))
                    {
                        Debug.LogError($"[AssetStoreAutoInstaller] Download failed for asset ID {id}: {err}");
                        state.phase = "error";
                        state.errorMessage = $"Download failed for ID {id}: {err}";
                        SaveBatchState(state);
                        return;
                    }
                    if (!inProg)
                    {
                        finished.Add(id);
                    }
                }
            }

            foreach (var id in finished)
            {
                state.pendingDownloads.Remove(id);
            }

            SaveBatchState(state);

            // Phase 2 Trigger: ONLY when 100% of downloads are finished!
            if (state.pendingDownloads.Count == 0)
            {
                Debug.Log($"[AssetStoreAutoInstaller] All downloads complete! Starting atomic batch installation of {state.pendingInstalls.Count} packages.");
                ExecuteAtomicBatchInstall(installer, state);
            }
        }

        private static void ExecuteAtomicBatchInstall(object installer, BatchJobState state)
        {
            state.phase = "installing";
            SaveBatchState(state);

            EditorApplication.LockReloadAssemblies();
            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (var id in state.pendingInstalls)
                {
                    DoInstallSingle(id, installer);
                }

                state.phase = "completed";
                state.pendingInstalls.Clear();
                SaveBatchState(state);
                Debug.Log($"[AssetStoreAutoInstaller] Batch installation complete for {state.allProductIds.Count} packages! Recompiling scripts once.");
            }
            catch (Exception ex)
            {
                state.phase = "error";
                state.errorMessage = ex.Message;
                SaveBatchState(state);
                Debug.LogError($"[AssetStoreAutoInstaller] Error during batch install: {ex}");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorApplication.UnlockReloadAssemblies();
                AssetDatabase.Refresh();
            }
        }

        private static void DoInstallSingle(long productId, object installer)
        {
            var installMethod = installer.GetType().GetMethod("Install", new[] { typeof(long), typeof(bool) });
            installMethod?.Invoke(installer, new object[] { productId, false });
        }

        private static bool IsDownloadNeeded(long productId, object dlMgr)
        {
            try
            {
                var getOp = dlMgr.GetType().GetMethod("GetDownloadOperation", new[] { typeof(long?) });
                var op = getOp?.Invoke(dlMgr, new object[] { (long?)productId });
                if (op != null)
                {
                    var inProg = (bool)(op.GetType().GetProperty("isInProgress")?.GetValue(op) ?? false);
                    if (inProg) return true;
                }

                var cacheField = dlMgr.GetType().GetField("m_AssetStoreCache", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var cache = cacheField?.GetValue(dlMgr);
                if (cache != null)
                {
                    var getLocalInfo = cache.GetType().GetMethod("GetLocalInfo", new[] { typeof(long?) });
                    var local = getLocalInfo?.Invoke(cache, new object[] { (long?)productId });
                    if (local != null) return false;
                }

                // Check local disk cache directly in Asset Store-5.x
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var cacheDir = Path.Combine(appData, "Unity", "Asset Store-5.x");
                if (Directory.Exists(cacheDir))
                {
                    var catalog = GetCatalog();
                    var entry = catalog?.assets.FirstOrDefault(a => a.productId == productId);
                    if (entry != null)
                    {
                        var safeName = SanitizeFileName(entry.name);
                        var matches = Directory.GetFiles(cacheDir, $"*{safeName}*.unitypackage", SearchOption.AllDirectories);
                        if (matches.Length > 0) return false;
                    }
                }
            }
            catch { }

            return true;
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string(name.Where(c => !invalid.Contains(c) && c != ':' && c != '-').ToArray()).Trim();
        }

        private static void SelectPackageInUI(string uniqueId)
        {
            try
            {
                var windowType = typeof(Editor).Assembly.GetType("UnityEditor.PackageManager.UI.PackageManagerWindow");
                var selectMethod = windowType?.GetMethod("SelectPackageStatic", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                selectMethod?.Invoke(null, new object[] { uniqueId, "MyAssets" });
            }
            catch { }
        }

        private static void SaveBatchState(BatchJobState state)
        {
            SessionState.SetString(SessionKeyBatchState, JsonUtility.ToJson(state));
        }

        private static BatchJobState LoadBatchState()
        {
            var json = SessionState.GetString(SessionKeyBatchState, "");
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                return JsonUtility.FromJson<BatchJobState>(json);
            }
            catch
            {
                return null;
            }
        }

        private static bool GetContext(out object opDisp, out object installer, out object dlMgr, out object pkgDb, out object myAssetsPage)
        {
            opDisp = null;
            installer = null;
            dlMgr = null;
            pkgDb = null;
            myAssetsPage = null;

            try
            {
                var windowType = typeof(Editor).Assembly.GetType("UnityEditor.PackageManager.UI.PackageManagerWindow");
                var instProp = windowType?.GetProperty("instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var inst = instProp?.GetValue(null);

                if (inst == null)
                {
                    var showMethod = windowType?.GetMethod("ShowWindow", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    showMethod?.Invoke(null, null);
                    inst = instProp?.GetValue(null);
                }

                if (inst == null) return false;

                var rootField = windowType.GetField("m_Root", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var root = rootField?.GetValue(inst);
                if (root == null) return false;

                var opDispField = root.GetType().GetField("m_OperationDispatcher", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                opDisp = opDispField?.GetValue(root);

                var pkgDbField = root.GetType().GetField("m_PackageDatabase", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                pkgDb = pkgDbField?.GetValue(root);

                var pageMgrField = root.GetType().GetField("m_PageManager", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var pageMgr = pageMgrField?.GetValue(root);
                myAssetsPage = pageMgr?.GetType().GetMethod("GetPage", new[] { typeof(string) })?.Invoke(pageMgr, new object[] { "MyAssets" });

                if (opDisp != null)
                {
                    var instField = opDisp.GetType().GetField("m_AssetStorePackageInstaller", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    installer = instField?.GetValue(opDisp);

                    var dlField = opDisp.GetType().GetField("m_AssetStoreDownloadManager", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    dlMgr = dlField?.GetValue(opDisp);
                }

                return opDisp != null && installer != null && dlMgr != null && pkgDb != null;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}

namespace AssetStoreAutoInstallerHelper
{
    // Backwards-compatibility alias for scripts or external CLI commands expecting AssetStoreAutoInstallerHelper
    public static class AssetStoreAutoInstaller
    {
        public static string BuildIndex() => AssetStoreFetcher.AssetStoreAutoInstaller.BuildIndex();
        public static string Install(string query) => AssetStoreFetcher.AssetStoreAutoInstaller.Install(query);
        public static string InstallList(string commaSeparatedQueries) => AssetStoreFetcher.AssetStoreAutoInstaller.InstallList(commaSeparatedQueries);
        public static string InstallBatch(params string[] queries) => AssetStoreFetcher.AssetStoreAutoInstaller.InstallBatch(queries);
        public static string Search(string query) => AssetStoreFetcher.AssetStoreAutoInstaller.Search(query);
        public static string GetBatchStatus() => AssetStoreFetcher.AssetStoreAutoInstaller.GetBatchStatus();
    }
}
#endif
