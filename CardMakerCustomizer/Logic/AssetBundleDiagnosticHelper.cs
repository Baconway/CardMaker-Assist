using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using MelonLoader;
using UnityEngine;
using CardMaker.Common;
using CardMakerCustomizer.Config;

namespace CardMakerCustomizer.Logic
{
    /// <summary>
    /// Provides detailed diagnostic inspection and logging helpers
    /// for Unity AssetBundles across Card Maker game titles.
    /// </summary>
    public static class AssetBundleDiagnosticHelper
    {
        private static readonly HashSet<string> _loadedLoggedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> _failedLoggedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _lock = new object();

        /// <summary>
        /// Resets the deduplication tracking sets (useful for testing or scene transitions).
        /// </summary>
        public static void ResetLoggingState()
        {
            Monitor.Enter(_lock);
            try
            {
                _loadedLoggedKeys.Clear();
                _failedLoggedKeys.Clear();
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        /// <summary>
        /// Returns true if this successful load has not yet been logged, and marks it as logged.
        /// </summary>
        public static bool ShouldLogLoaded(string key)
        {
            if (string.IsNullOrEmpty(key)) return true;
            Monitor.Enter(_lock);
            try
            {
                if (_loadedLoggedKeys.Contains(key)) return false;
                _loadedLoggedKeys.Add(key);
                return true;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        /// <summary>
        /// Returns true if this failure has not yet been logged, and marks it as logged.
        /// </summary>
        public static bool ShouldLogFailed(string key)
        {
            if (string.IsNullOrEmpty(key)) return true;
            Monitor.Enter(_lock);
            try
            {
                if (_failedLoggedKeys.Contains(key)) return false;
                _failedLoggedKeys.Add(key);
                return true;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public static void SafeLog(string msg)
        {
            try
            {
                MelonLogger.Msg(msg);
            }
            catch
            {
                Console.WriteLine(msg);
            }
        }

        public static void SafeWarn(string msg)
        {
            try
            {
                MelonLogger.Warning(msg);
            }
            catch
            {
                Console.WriteLine("[WARN] " + msg);
            }
        }

        public static void SafeError(string msg)
        {
            try
            {
                MelonLogger.Error(msg);
            }
            catch
            {
                Console.WriteLine("[ERROR] " + msg);
            }
        }

        /// <summary>
        /// Diagnoses an AssetBundle file on disk to determine why AssetBundle.LoadFromFile
        /// might have returned null or failed.
        /// </summary>
        public static string DiagnoseBundleFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "Path is null or empty.";
            }

            if (!File.Exists(path))
            {
                return StringCompat.Format("File does not exist on disk at '{0}'.", path);
            }

            FileInfo fi;
            try
            {
                fi = new FileInfo(path);
            }
            catch (Exception ex)
            {
                return StringCompat.Format("Unable to inspect file info: {0}", ex.Message);
            }

            if (fi.Length == 0)
            {
                return "File is completely empty (0 bytes).";
            }

            if (fi.Length < 16)
            {
                return StringCompat.Format("File is truncated ({0} bytes, smaller than the 16-byte minimum AssetBundle header).", fi.Length);
            }

            byte[] header = new byte[Math.Min(128, (int)fi.Length)];
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    int bytesRead = fs.Read(header, 0, header.Length);
                    if (bytesRead < 16)
                    {
                        return StringCompat.Format("Could only read {0} bytes from file.", bytesRead);
                    }
                }
            }
            catch (Exception ex)
            {
                return StringCompat.Format("Failed to open or read file: {0} - {1}", ex.GetType().Name, ex.Message);
            }

            int formatVersion;
            string playerVersion;
            string engineVersion;
            if (TryParseUnityFSHeader(header, out formatVersion, out playerVersion, out engineVersion))
            {
                string info = StringCompat.Format(
                    "Valid UnityFS signature (Archive Format Version: {0}, Player Version: '{1}', Engine Version: '{2}', File Size: {3}).",
                    formatVersion, playerVersion, engineVersion, FormatFileSize(fi.Length));

                string currentUnityVer = GetCurrentUnityVersion();
                if (!string.IsNullOrEmpty(currentUnityVer) && !string.IsNullOrEmpty(engineVersion))
                {
                    if (!IsEngineVersionCompatible(engineVersion, currentUnityVer))
                    {
                        info += StringCompat.Format(
                            " Warning: Engine version mismatch! Bundle was built with Unity '{0}', but runtime is Unity '{1}'. AssetBundles built with different or newer Unity major versions often fail to load.",
                            engineVersion, currentUnityVer);
                    }
                }
                return info;
            }

            // Check if other known Unity bundle signatures are present
            string rawHeader = Encoding.ASCII.GetString(header, 0, Math.Min(8, header.Length)).TrimEnd('\0');
            if (rawHeader.StartsWith("UnityRaw") || rawHeader.StartsWith("UnityWeb") || rawHeader.StartsWith("UnityArchive"))
            {
                return StringCompat.Format(
                    "Legacy/Raw Unity bundle signature ('{0}'). Modern Unity runtimes typically require UnityFS format.", rawHeader);
            }

            // Check if it's a common non-AssetBundle format mistakenly saved
            if (header[0] == 0x89 && header[1] == 0x40 && header[2] == 0x4E && header[3] == 0x47 ||
                header[0] == 0x89 && header[1] == (byte)'P' && header[2] == (byte)'N' && header[3] == (byte)'G')
            {
                return "File is a raw PNG image, not a Unity AssetBundle.";
            }
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            {
                return "File is a JPEG image, not a Unity AssetBundle.";
            }
            if (header[0] == 0x50 && header[1] == 0x4B && (header[2] == 0x03 || header[2] == 0x05))
            {
                return "File is a standard ZIP archive, not a Unity AssetBundle.";
            }
            if (header[0] == 0x3C && header[1] == 0x3F && header[2] == 0x78 && header[3] == 0x6D) // <?xm
            {
                return "File is an XML document, not a Unity AssetBundle.";
            }

            string printableHeader = GetPrintableAscii(header, 16);
            string hex = BitConverter.ToString(header, 0, Math.Min(8, header.Length)).Replace("-", " ");
            return StringCompat.Format(
                "Invalid AssetBundle signature. Expected 'UnityFS', but found: '{0}' (Hex: {1}). The file is corrupted or not an AssetBundle.",
                printableHeader, hex);
        }

        /// <summary>
        /// Parses the standard UnityFS binary header (format version, player version, engine version).
        /// </summary>
        public static bool TryParseUnityFSHeader(byte[] headerBytes, out int formatVersion, out string playerVersion, out string engineVersion)
        {
            formatVersion = 0;
            playerVersion = null;
            engineVersion = null;

            if (headerBytes == null || headerBytes.Length < 16)
            {
                return false;
            }

            // Check "UnityFS\0"
            if (headerBytes[0] != (byte)'U' || headerBytes[1] != (byte)'n' || headerBytes[2] != (byte)'i' ||
                headerBytes[3] != (byte)'t' || headerBytes[4] != (byte)'y' || headerBytes[5] != (byte)'F' ||
                headerBytes[6] != (byte)'S' || headerBytes[7] != 0)
            {
                return false;
            }

            int offset = 8;
            if (offset + 4 > headerBytes.Length)
            {
                return false;
            }

            formatVersion = (headerBytes[offset] << 24) | (headerBytes[offset + 1] << 16) | (headerBytes[offset + 2] << 8) | headerBytes[offset + 3];
            offset += 4;

            int playerStart = offset;
            while (offset < headerBytes.Length && headerBytes[offset] != 0)
            {
                offset++;
            }
            if (offset >= headerBytes.Length) return false;
            playerVersion = Encoding.ASCII.GetString(headerBytes, playerStart, offset - playerStart);
            offset++; // skip '\0'

            int engineStart = offset;
            while (offset < headerBytes.Length && headerBytes[offset] != 0)
            {
                offset++;
            }
            if (offset >= headerBytes.Length) return false;
            engineVersion = Encoding.ASCII.GetString(headerBytes, engineStart, offset - engineStart);

            return true;
        }

        /// <summary>
        /// Checks whether the bundle engine version is compatible with the runtime engine version.
        /// </summary>
        public static bool IsEngineVersionCompatible(string bundleVer, string gameVer)
        {
            if (string.IsNullOrEmpty(bundleVer) || string.IsNullOrEmpty(gameVer)) return true;
            if (string.Equals(bundleVer, gameVer, StringComparison.OrdinalIgnoreCase)) return true;

            string[] bParts = bundleVer.Split('.');
            string[] gParts = gameVer.Split('.');
            if (bParts.Length > 0 && gParts.Length > 0)
            {
                if (!string.Equals(bParts[0], gParts[0], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        public static string GetCurrentUnityVersion()
        {
            try
            {
                var prop = typeof(Application).GetProperty("unityVersion");
                if ((object)prop != null)
                {
                    return prop.GetValue(null, null) as string;
                }
            }
            catch
            {
            }
            return null;
        }

        public static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return StringCompat.Format("{0:F1} KB", bytes / 1024.0);
            return StringCompat.Format("{0:F2} MB", bytes / (1024.0 * 1024.0));
        }

        private static string GetPrintableAscii(byte[] bytes, int length)
        {
            int len = Math.Min(length, bytes.Length);
            char[] chars = new char[len];
            for (int i = 0; i < len; i++)
            {
                byte b = bytes[i];
                chars[i] = (b >= 32 && b <= 126) ? (char)b : '.';
            }
            return new string(chars);
        }

        /// <summary>
        /// Formats the search paths that were checked when an AssetBundle file could not be found.
        /// </summary>
        public static string FormatSearchedPaths(IEnumerable<string> roots, string bundleName)
        {
            if (roots == null) return "None";
            List<string> list = new List<string>();
            foreach (string r in roots)
            {
                if (!string.IsNullOrEmpty(r))
                {
                    list.Add(Path.Combine(r, bundleName ?? "").Replace('\\', '/'));
                }
            }
            if (list.Count == 0) return "None";
            return string.Join("; ", list.ToArray());
        }

        /// <summary>
        /// Safely retrieves all internal asset names from an AssetBundle without throwing exceptions.
        /// Returns an empty array if bundle is null or unloaded.
        /// </summary>
        public static string[] GetSafeAssetNames(AssetBundle bundle)
        {
            if (object.ReferenceEquals(bundle, null))
            {
                return new string[0];
            }

            try
            {
                var method = typeof(AssetBundle).GetMethod("GetAllAssetNames", Type.EmptyTypes);
                if ((object)method != null)
                {
                    string[] names = method.Invoke(bundle, null) as string[];
                    return names != null ? names : new string[0];
                }
            }
            catch
            {
            }
            return new string[0];
        }

        /// <summary>
        /// Formats a list of internal asset names into a compact single-line preview string.
        /// </summary>
        public static string FormatBundleInternalAssets(string[] assetNames, int maxCount = 5)
        {
            if (assetNames == null || assetNames.Length == 0)
            {
                return "empty";
            }

            int count = Math.Min(maxCount, assetNames.Length);
            List<string> display = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                string name = assetNames[i];
                display.Add("'" + (name ?? "") + "'");
            }

            string result = string.Join(", ", display.ToArray());
            if (assetNames.Length > maxCount)
            {
                result += StringCompat.Format(", ... (+{0} more)", assetNames.Length - maxCount);
            }
            return result;
        }

        /// <summary>
        /// Analyzes available internal assets to provide diagnostic hints when an asset lookup fails.
        /// Detects case mismatches, extension mismatches (.png/.prefab), and path prefix mismatches.
        /// </summary>
        public static string FindMatchingAssetHint(string[] availableNames, string requestedName)
        {
            if (availableNames == null || availableNames.Length == 0 || string.IsNullOrEmpty(requestedName))
            {
                return null;
            }

            string reqClean = requestedName.Trim().ToLowerInvariant();

            // 1. Check exact case mismatch
            for (int i = 0; i < availableNames.Length; i++)
            {
                string name = availableNames[i];
                if (!string.IsNullOrEmpty(name) && string.Equals(name, requestedName, StringComparison.OrdinalIgnoreCase))
                {
                    return StringCompat.Format(
                        "Case mismatch! The bundle contains '{0}', but CardMaker requested '{1}'. AssetBundle lookups are case-sensitive.",
                        name, requestedName);
                }
            }

            // 2. Check path prefix match (e.g. "Assets/UI/card" vs "card")
            for (int i = 0; i < availableNames.Length; i++)
            {
                string name = availableNames[i];
                if (!string.IsNullOrEmpty(name) && (name.Contains("/") || name.Contains("\\")))
                {
                    string fileName = Path.GetFileName(name);
                    string fileNameNoExt = Path.GetFileNameWithoutExtension(name);
                    if (string.Equals(fileName, requestedName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(fileNameNoExt, requestedName, StringComparison.OrdinalIgnoreCase))
                    {
                        return StringCompat.Format(
                            "Path mismatch! The internal asset has path prefix '{0}'. CardMaker requested '{1}'.",
                            name, requestedName);
                    }
                }
            }

            // 3. Check filename without extension match (e.g. "ui_card.png" vs "ui_card")
            for (int i = 0; i < availableNames.Length; i++)
            {
                string name = availableNames[i];
                if (!string.IsNullOrEmpty(name))
                {
                    string fileNameNoExt = Path.GetFileNameWithoutExtension(name);
                    if (string.Equals(fileNameNoExt, requestedName, StringComparison.OrdinalIgnoreCase))
                    {
                        return StringCompat.Format(
                            "Extension mismatch! The bundle contains '{0}'. Try building the asset without an extension or ensure the asset name exactly matches '{1}'.",
                            name, requestedName);
                    }
                }
            }

            // 4. Partial substring match
            for (int i = 0; i < availableNames.Length; i++)
            {
                string name = availableNames[i];
                if (!string.IsNullOrEmpty(name) && name.ToLowerInvariant().Contains(reqClean))
                {
                    return StringCompat.Format(
                        "Partial match! The bundle contains '{0}', which contains '{1}'.",
                        name, requestedName);
                }
            }

            return null;
        }

        /// <summary>
        /// Unified logger for AssetBundleDB.load(string, string) and AssetBundleDB.load(int, string).
        /// Logs whether the internal asset was successfully found inside the AssetBundle,
        /// or details and diagnostics when the asset could not be found.
        /// </summary>
        public static void LogInternalAssetLookup(AssetBundleDB db, string titleName, string assetBundleName, string requestedAssetName, UnityEngine.Object result)
        {
            if (ModConfig.EnableAssetBundleLogging != null && !ModConfig.EnableAssetBundleLogging.Value)
            {
                return;
            }

            if ((object)db == null) return;

            string safeBundleName = !string.IsNullOrEmpty(assetBundleName) ? assetBundleName : "(unnamed)";
            string safeAssetName = !string.IsNullOrEmpty(requestedAssetName) ? requestedAssetName : "(unnamed)";

            if (result != null)
            {
                // Internal asset was found and loaded!
                string logKey = StringCompat.Format("found:{0}:{1}:{2}", titleName, safeBundleName, safeAssetName);
                if (ShouldLogLoaded(logKey))
                {
                    string typeName = result.GetType().Name;
                    SafeLog(StringCompat.Format(
                        "[CardMakerCustomizer] [AssetBundle] [{0}] Found internal asset '{1}' ({2}) inside AssetBundle '{3}'.",
                        titleName, safeAssetName, typeName, safeBundleName));
                }
            }
            else
            {
                // Internal asset was NOT found!
                string failKey = StringCompat.Format("miss:{0}:{1}:{2}", titleName, safeBundleName, safeAssetName);
                if (ShouldLogFailed(failKey))
                {
                    int idx = -1;
                    try
                    {
                        idx = db.exists(safeBundleName);
                    }
                    catch { }

                    if (idx < 0)
                    {
                        SafeError(StringCompat.Format(
                            "[CardMakerCustomizer] [AssetBundle] [ERROR] [{0}] Failed to load internal asset '{1}': AssetBundle '{2}' is not registered in AssetBundleDB manifest (assets_{0}.bytes).",
                            titleName, safeAssetName, safeBundleName));
                    }
                    else
                    {
                        AssetBundle bundle = null;
                        try
                        {
                            bundle = db.getAssetBundle(idx);
                        }
                        catch { }

                        if ((object)bundle == null)
                        {
                            SafeError(StringCompat.Format(
                                "[CardMakerCustomizer] [AssetBundle] [ERROR] [{0}] Failed to load internal asset '{1}': AssetBundle '{2}' (ID: {3}) could not be loaded from disk into memory.",
                                titleName, safeAssetName, safeBundleName, idx));
                        }
                        else
                        {
                            // THE ASSETBUNDLE LOADED, BUT COULD NOT FIND THE INTERNAL ASSET INSIDE!
                            SafeWarn(StringCompat.Format(
                                "[CardMakerCustomizer] [AssetBundle] [WARN] [{0}] AssetBundle '{1}' loaded, but could NOT find internal asset '{2}' inside!",
                                titleName, safeBundleName, safeAssetName));

                            string[] assetNames = GetSafeAssetNames(bundle);
                            if (assetNames.Length == 0)
                            {
                                SafeWarn(StringCompat.Format(
                                    "[CardMakerCustomizer] [AssetBundle]   -> AssetBundle '{0}' contains 0 internal assets (bundle is empty).",
                                    safeBundleName));
                            }
                            else
                            {
                                int maxDisplay = 10;
                                int displayCount = Math.Min(maxDisplay, assetNames.Length);
                                SafeWarn(StringCompat.Format(
                                    "[CardMakerCustomizer] [AssetBundle]   -> Available internal assets in '{0}' ({1} total):",
                                    safeBundleName, assetNames.Length));

                                for (int i = 0; i < displayCount; i++)
                                {
                                    SafeWarn(StringCompat.Format(
                                        "[CardMakerCustomizer] [AssetBundle]       * '{0}'",
                                        assetNames[i]));
                                }

                                if (assetNames.Length > maxDisplay)
                                {
                                    SafeWarn(StringCompat.Format(
                                        "[CardMakerCustomizer] [AssetBundle]       ... and {0} more internal asset(s).",
                                        assetNames.Length - maxDisplay));
                                }

                                string hint = FindMatchingAssetHint(assetNames, safeAssetName);
                                if (!string.IsNullOrEmpty(hint))
                                {
                                    SafeWarn(StringCompat.Format(
                                        "[CardMakerCustomizer] [AssetBundle]   -> Diagnostic: {0}",
                                        hint));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
