using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using HarmonyLib;
using CardMaker.Common;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.Logic;
using UnityEngine;

namespace CardMakerCustomizer.Patches
{
    // =========================================================================
    // AssetBundle Loading & Diagnostic Logging Patches
    // Hooks Sega CardMaker's AssetBundleDB to provide detailed logging
    // when AssetBundles are loaded, and rich diagnostic logging when they fail.
    // =========================================================================

    [HarmonyPatch(typeof(AssetBundleDB), "loadAssetBundleInternal")]
    public static class Patch_AssetBundleDB_loadAssetBundleInternal
    {
        public static void Prefix(AssetBundleDB __instance, int id, out bool __state)
        {
            __state = false;
            if (ModConfig.EnableAssetBundleLogging != null && !ModConfig.EnableAssetBundleLogging.Value)
            {
                return;
            }

            try
            {
                if ((object)__instance == null) return;

                // Check if the bundle is already loaded in the cache
                Array caches = Traverse.Create(__instance).Field("assetBundles_").GetValue<Array>();
                if (caches != null && id >= 0 && id < caches.Length)
                {
                    object cache = caches.GetValue(id);
                    if (cache != null)
                    {
                        var bundleField = cache.GetType().GetField("assetBundle_");
                        if ((object)bundleField != null)
                        {
                            var existingBundle = bundleField.GetValue(cache);
                            if (existingBundle != null)
                            {
                                __state = true; // Already loaded previously
                            }
                        }
                    }
                }
            }
            catch
            {
                __state = false;
            }
        }

        public static void Postfix(AssetBundleDB __instance, int id, bool __state, AssetBundle __result)
        {
            if (ModConfig.EnableAssetBundleLogging != null && !ModConfig.EnableAssetBundleLogging.Value)
            {
                return;
            }

            try
            {
                if ((object)__instance == null) return;

                // Extract Title name
                int titleInt = 0;
                try
                {
                    titleInt = Traverse.Create(__instance).Field("title_").GetValue<int>();
                }
                catch { }
                string titleName = ((AssetBundleDB.Title)titleInt).ToString();

                // Extract bundle info from assetBundleSet_
                string bundleName = null;
                int numDeps = 0;
                int[] deps = null;

                try
                {
                    object setObj = Traverse.Create(__instance).Field("assetBundleSet_").GetValue();
                    if (setObj != null)
                    {
                        var bundlesField = setObj.GetType().GetField("assetBundles_");
                        if ((object)bundlesField != null)
                        {
                            Array infoArray = bundlesField.GetValue(setObj) as Array;
                            if (infoArray != null && id >= 0 && id < infoArray.Length)
                            {
                                object info = infoArray.GetValue(id);
                                if (info != null)
                                {
                                    var nameF = info.GetType().GetField("name_");
                                    if ((object)nameF != null) bundleName = nameF.GetValue(info) as string;

                                    var numDepsF = info.GetType().GetField("numDependencies_");
                                    if ((object)numDepsF != null) numDeps = (int)numDepsF.GetValue(info);

                                    var depsF = info.GetType().GetField("dependencies_");
                                    if ((object)depsF != null) deps = depsF.GetValue(info) as int[];
                                }
                            }
                        }
                    }
                }
                catch { }

                string displayBundleName = string.IsNullOrEmpty(bundleName) ? StringCompat.Format("Bundle #{0}", id) : bundleName;

                // Extract roots
                List<string> roots = null;
                try
                {
                    roots = Traverse.Create(__instance).Field("root_").GetValue<List<string>>();
                }
                catch { }

                // Find file path
                string resolvedFilePath = null;
                if (!string.IsNullOrEmpty(bundleName) && roots != null)
                {
                    for (int r = roots.Count - 1; r >= 0; r--)
                    {
                        string candidate = Path.Combine(roots[r] ?? "", bundleName).Replace('\\', '/');
                        if (File.Exists(candidate))
                        {
                            resolvedFilePath = candidate;
                            break;
                        }
                    }
                }

                if (__result != null)
                {
                    // Successfully loaded!
                    if (!__state) // Freshly loaded from disk
                    {
                        string logKey = StringCompat.Format("{0}:{1}:{2}", titleName, id, displayBundleName);
                        if (AssetBundleDiagnosticHelper.ShouldLogLoaded(logKey))
                        {
                            string sizeStr = "Unknown size";
                            if (!string.IsNullOrEmpty(resolvedFilePath) && File.Exists(resolvedFilePath))
                            {
                                try
                                {
                                    long fLen = new FileInfo(resolvedFilePath).Length;
                                    sizeStr = AssetBundleDiagnosticHelper.FormatFileSize(fLen);
                                }
                                catch { }
                            }

                            string[] internalAssets = AssetBundleDiagnosticHelper.GetSafeAssetNames(__result);
                            string contentSummary = internalAssets.Length > 0
                                ? StringCompat.Format(" ({0} internal asset(s) inside: [{1}])",
                                    internalAssets.Length,
                                    AssetBundleDiagnosticHelper.FormatBundleInternalAssets(internalAssets, 4))
                                : " (0 internal assets inside - empty bundle)";

                            string pathDisplay = !string.IsNullOrEmpty(resolvedFilePath) ? resolvedFilePath : "(Cached / Memory)";
                            string depInfo = numDeps > 0 ? StringCompat.Format(" [{0} dependencies loaded]", numDeps) : "";

                            AssetBundleDiagnosticHelper.SafeLog(StringCompat.Format(
                                "[CardMakerCustomizer] [AssetBundle] Successfully loaded: '{0}' (ID: {1}) from '{2}' [Title: {3}, Size: {4}]{5}{6}",
                                displayBundleName, id, pathDisplay, titleName, sizeStr, depInfo, contentSummary));

                            if (internalAssets.Length == 0)
                            {
                                AssetBundleDiagnosticHelper.SafeWarn(StringCompat.Format(
                                    "[CardMakerCustomizer] [AssetBundle]   -> Warning: AssetBundle '{0}' is empty (contains 0 internal assets). Check build pipeline.",
                                    displayBundleName));
                            }
                        }
                    }
                }
                else
                {
                    // Failed to load! Log detailed diagnostics.
                    string failKey = StringCompat.Format("{0}:{1}:{2}", titleName, id, displayBundleName);
                    if (AssetBundleDiagnosticHelper.ShouldLogFailed(failKey))
                    {
                        if (string.IsNullOrEmpty(resolvedFilePath))
                        {
                            // File was not found in any search root
                            string searched = AssetBundleDiagnosticHelper.FormatSearchedPaths(roots, bundleName ?? StringCompat.Format("ID_{0}", id));
                            AssetBundleDiagnosticHelper.SafeError(StringCompat.Format(
                                "[CardMakerCustomizer] [AssetBundle] [ERROR] Failed to load AssetBundle '{0}' (ID: {1}) for Title: {2}. Reason: File not found on disk. Searched in: [{3}]",
                                displayBundleName, id, titleName, searched));
                        }
                        else
                        {
                            // File was found on disk, but AssetBundle.LoadFromFile returned null!
                            string diag = AssetBundleDiagnosticHelper.DiagnoseBundleFile(resolvedFilePath);
                            AssetBundleDiagnosticHelper.SafeError(StringCompat.Format(
                                "[CardMakerCustomizer] [AssetBundle] [ERROR] Failed to load AssetBundle '{0}' (ID: {1}) for Title: {2} from '{3}'. Reason: AssetBundle.LoadFromFile returned null. Diagnostic: {4}",
                                displayBundleName, id, titleName, resolvedFilePath, diag));
                        }

                        // Check dependencies
                        if (deps != null && deps.Length > 0)
                        {
                            for (int d = 0; d < deps.Length; d++)
                            {
                                int depId = deps[d];
                                AssetBundle depBundle = __instance.getAssetBundle(depId);
                                if ((object)depBundle == null)
                                {
                                    AssetBundleDiagnosticHelper.SafeWarn(StringCompat.Format(
                                        "[CardMakerCustomizer] [AssetBundle]   -> Warning: Dependency ID {0} for '{1}' is not loaded.", depId, displayBundleName));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AssetBundleDiagnosticHelper.SafeError(StringCompat.Format(
                    "[CardMakerCustomizer] [AssetBundle] Error in loadAssetBundleInternal postfix logger: {0}", ex.Message));
            }
        }

        public static Exception Finalizer(AssetBundleDB __instance, int id, Exception __exception, ref AssetBundle __result)
        {
            if (__exception != null)
            {
                AssetBundleDiagnosticHelper.SafeError(StringCompat.Format(
                    "[CardMakerCustomizer] [AssetBundle] [FATAL] Exception while loading AssetBundle (ID: {0}): {1}: {2}\n{3}",
                    id, __exception.GetType().Name, __exception.Message, __exception.StackTrace));

                __result = null;
                // Suppress exception so the game does not crash fatally
                return null;
            }
            return null;
        }
    }

    [HarmonyPatch(typeof(AssetBundleDB), "loadAssetBundleSet", new Type[0])]
    public static class Patch_AssetBundleDB_loadAssetBundleSet
    {
        public static void Postfix(AssetBundleDB __instance)
        {
            if (ModConfig.EnableAssetBundleLogging != null && !ModConfig.EnableAssetBundleLogging.Value)
            {
                return;
            }

            try
            {
                if ((object)__instance == null) return;

                int count = 0;
                object setObj = Traverse.Create(__instance).Field("assetBundleSet_").GetValue();
                if (setObj != null)
                {
                    var f = setObj.GetType().GetField("numAssetBundles_");
                    if ((object)f != null) count = (int)f.GetValue(setObj);
                }

                int titleInt = 0;
                try
                {
                    titleInt = Traverse.Create(__instance).Field("title_").GetValue<int>();
                }
                catch { }
                string titleName = ((AssetBundleDB.Title)titleInt).ToString();

                string manifestRelPath = AssetBundleDB.AssetDirs[titleInt].Trim('/', '\\') + "/" + AssetBundleDB.AssetSetNames[titleInt];
                string fullPath = Path.Combine(Application.streamingAssetsPath, manifestRelPath).Replace('\\', '/');

                if (count > 0)
                {
                    AssetBundleDiagnosticHelper.SafeLog(StringCompat.Format(
                        "[CardMakerCustomizer] [AssetBundle] Loaded base AssetBundleDB manifest from '{0}' for Title: {1} ({2} bundle(s) indexed).",
                        fullPath, titleName, count));
                }
                else
                {
                    if (!File.Exists(fullPath))
                    {
                        AssetBundleDiagnosticHelper.SafeError(StringCompat.Format(
                            "[CardMakerCustomizer] [AssetBundle] [ERROR] Base AssetBundleDB manifest not found at '{0}' for Title: {1}!", fullPath, titleName));
                    }
                    else
                    {
                        AssetBundleDiagnosticHelper.SafeError(StringCompat.Format(
                            "[CardMakerCustomizer] [AssetBundle] [ERROR] Base AssetBundleDB manifest at '{0}' for Title: {1} indexed 0 bundles (corrupted or empty manifest).", fullPath, titleName));
                    }
                }
            }
            catch (Exception ex)
            {
                AssetBundleDiagnosticHelper.SafeError(StringCompat.Format(
                    "[CardMakerCustomizer] [AssetBundle] Error in loadAssetBundleSet logger: {0}", ex.Message));
            }
        }
    }

    [HarmonyPatch(typeof(AssetBundleDB), "appendAssetBundleSet", new Type[] { typeof(string) })]
    public static class Patch_AssetBundleDB_appendAssetBundleSet
    {
        public static void Prefix(AssetBundleDB __instance, string root, out int __state)
        {
            __state = 0;
            try
            {
                if ((object)__instance == null) return;
                object setObj = Traverse.Create(__instance).Field("assetBundleSet_").GetValue();
                if (setObj != null)
                {
                    var f = setObj.GetType().GetField("numAssetBundles_");
                    if ((object)f != null) __state = (int)f.GetValue(setObj);
                }
            }
            catch
            {
                __state = 0;
            }
        }

        public static void Postfix(AssetBundleDB __instance, string root, int __state)
        {
            if (ModConfig.EnableAssetBundleLogging != null && !ModConfig.EnableAssetBundleLogging.Value)
            {
                return;
            }

            try
            {
                if ((object)__instance == null) return;

                int after = 0;
                object setObj = Traverse.Create(__instance).Field("assetBundleSet_").GetValue();
                if (setObj != null)
                {
                    var f = setObj.GetType().GetField("numAssetBundles_");
                    if ((object)f != null) after = (int)f.GetValue(setObj);
                }

                int titleInt = 0;
                try
                {
                    titleInt = Traverse.Create(__instance).Field("title_").GetValue<int>();
                }
                catch { }
                string titleName = ((AssetBundleDB.Title)titleInt).ToString();

                int added = after - __state;
                string manifestRelPath = AssetBundleDB.AssetDirs[titleInt].Trim('/', '\\') + "/" + AssetBundleDB.AssetSetNames[titleInt];
                string manifestPath = Path.Combine(root ?? "", manifestRelPath).Replace('\\', '/');

                if (added > 0)
                {
                    AssetBundleDiagnosticHelper.SafeLog(StringCompat.Format(
                        "[CardMakerCustomizer] [AssetBundle] Appended AssetBundle manifest from '{0}' for Title: {1} (+{2} bundle(s), total registered: {3}).",
                        manifestPath, titleName, added, after));
                }
                else
                {
                    if (!File.Exists(manifestPath))
                    {
                        AssetBundleDiagnosticHelper.SafeLog(StringCompat.Format(
                            "[CardMakerCustomizer] [AssetBundle] Option pack at '{0}' has no AssetBundle manifest for Title: {1} (checked: '{2}').",
                            root, titleName, manifestPath));
                    }
                    else
                    {
                        AssetBundleDiagnosticHelper.SafeWarn(StringCompat.Format(
                            "[CardMakerCustomizer] [AssetBundle] Manifest exists at '{0}' for Title: {1}, but 0 new bundles were added (possible duplicate or empty set).",
                            manifestPath, titleName));
                    }
                }
            }
            catch (Exception ex)
            {
                AssetBundleDiagnosticHelper.SafeError(StringCompat.Format(
                    "[CardMakerCustomizer] [AssetBundle] Error in appendAssetBundleSet logger for '{0}': {1}", root, ex.Message));
            }
        }
    }

    [HarmonyPatch(typeof(AssetBundleDB), "load", new Type[] { typeof(string), typeof(string) })]
    public static class Patch_AssetBundleDB_load
    {
        public static void Postfix(AssetBundleDB __instance, string assetBundleName, string name, UnityEngine.Object __result)
        {
            if ((object)__instance == null) return;
            try
            {
                int titleInt = 0;
                try
                {
                    titleInt = Traverse.Create(__instance).Field("title_").GetValue<int>();
                }
                catch { }
                string titleName = ((AssetBundleDB.Title)titleInt).ToString();

                AssetBundleDiagnosticHelper.LogInternalAssetLookup(__instance, titleName, assetBundleName, name, __result);
            }
            catch (Exception ex)
            {
                AssetBundleDiagnosticHelper.SafeError(StringCompat.Format(
                    "[CardMakerCustomizer] [AssetBundle] Error in load(string, string) logger: {0}", ex.Message));
            }
        }
    }

    [HarmonyPatch(typeof(AssetBundleDB), "load", new Type[] { typeof(int), typeof(string) })]
    public static class Patch_AssetBundleDB_load_by_id
    {
        public static void Postfix(AssetBundleDB __instance, int assetBundleId, string name, UnityEngine.Object __result)
        {
            if ((object)__instance == null) return;
            try
            {
                int titleInt = 0;
                try
                {
                    titleInt = Traverse.Create(__instance).Field("title_").GetValue<int>();
                }
                catch { }
                string titleName = ((AssetBundleDB.Title)titleInt).ToString();

                string bundleName = null;
                try
                {
                    object setObj = Traverse.Create(__instance).Field("assetBundleSet_").GetValue();
                    if (setObj != null)
                    {
                        var bundlesField = setObj.GetType().GetField("assetBundles_");
                        if ((object)bundlesField != null)
                        {
                            Array infoArray = bundlesField.GetValue(setObj) as Array;
                            if (infoArray != null && assetBundleId >= 0 && assetBundleId < infoArray.Length)
                            {
                                object info = infoArray.GetValue(assetBundleId);
                                if (info != null)
                                {
                                    var nameF = info.GetType().GetField("name_");
                                    if ((object)nameF != null) bundleName = nameF.GetValue(info) as string;
                                }
                            }
                        }
                    }
                }
                catch { }

                if (string.IsNullOrEmpty(bundleName))
                {
                    bundleName = StringCompat.Format("ID_{0}", assetBundleId);
                }

                AssetBundleDiagnosticHelper.LogInternalAssetLookup(__instance, titleName, bundleName, name, __result);
            }
            catch (Exception ex)
            {
                AssetBundleDiagnosticHelper.SafeError(StringCompat.Format(
                    "[CardMakerCustomizer] [AssetBundle] Error in load(int, string) logger: {0}", ex.Message));
            }
        }
    }
}
