using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using CardMaker.Common;
using CardMaker.MAI;
using CardMaker.MAI.MaiStudio;
using CardMaker.MAI.MaiStudio.CardTypeName;
using CardMakerCustomizer.Config;

namespace CardMakerCustomizer.Logic
{
    public static class CustomOptHelper
    {
        // Cache of custom card IDs detected from disk or XML definitions
        private static readonly HashSet<int> KnownCustomCardIds = new HashSet<int>();
        private static readonly Dictionary<int, string> KnownCustomCardNames = new Dictionary<int, string>();
        private static readonly Dictionary<int, string> KnownCustomCardTypes = new Dictionary<int, string>();

        public static void RegisterCustomCard(int cardId, string cardName = null, string cardType = null)
        {
            if (cardId > 0)
            {
                if (!KnownCustomCardIds.Contains(cardId))
                {
                    KnownCustomCardIds.Add(cardId);
                }
                if (!string.IsNullOrEmpty(cardName))
                {
                    KnownCustomCardNames[cardId] = cardName;
                }
                if (!string.IsNullOrEmpty(cardType))
                {
                    KnownCustomCardTypes[cardId] = cardType;
                }
            }
        }

        public static string GetCustomCardName(int cardId)
        {
            string name;
            return KnownCustomCardNames.TryGetValue(cardId, out name) ? name : null;
        }

        public static string GetCustomCardType(int cardId)
        {
            string type;
            return KnownCustomCardTypes.TryGetValue(cardId, out type) ? type : null;
        }

        public static int GetRegisteredCustomCardCount()
        {
            return KnownCustomCardIds.Count;
        }

        public static void LogCustomOptionMounted(string game, string packPath)
        {
            SafeLog($"[CardMakerCustomizer] [CustomOpt] [{game}] Successfully mounted custom option pack: {packPath}");
        }

        public static void SafeLog(string message)
        {
            try
            {
                MelonLogger.Msg(message);
            }
            catch
            {
                Console.WriteLine(message);
            }
        }

        public static void SafeWarn(string message)
        {
            try
            {
                MelonLogger.Warning(message);
            }
            catch
            {
                Console.WriteLine("[WARN] " + message);
            }
        }

        /// <summary>
        /// Checks whether an option folder name matches custom option conventions (e.g. A083..A999, A900).
        /// Standard official Sega option packs in SDED 1.44 end at A082.
        /// </summary>
        public static bool IsCustomOptionPack(string dirName)
        {
            if (string.IsNullOrEmpty(dirName))
            {
                return false;
            }

            string clean = dirName.Trim().Replace('\\', '/');
            int lastSlash = clean.LastIndexOf('/');
            if (lastSlash >= 0 && lastSlash < clean.Length - 1)
            {
                clean = clean.Substring(lastSlash + 1);
            }

            if (clean.Length >= 4 && (clean[0] == 'A' || clean[0] == 'a'))
            {
                int ver;
                if (int.TryParse(clean.Substring(1, 3), out ver))
                {
                    // Official Sega options in SDED 1.44 are between A000 and A082
                    return ver >= 83;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether a card ID belongs to a custom option pack.
        /// - maimai DX: official cards are up to 06008016; custom packs like A900 use IDs like 9767012..9767016 or >= 6009000.
        /// - CHUNITHM: official cards are up to 02298002; custom cards use IDs >= 2299000.
        /// - Ongeki: official cards are up to 102561; custom cards use IDs >= 109000.
        /// </summary>
        public static bool IsCustomCard(int cardId)
        {
            if (KnownCustomCardIds.Contains(cardId))
            {
                return true;
            }

            // maimai DX custom IDs (official up to 6008016; A900 is 9767012..9767016)
            if (cardId >= 6009000)
            {
                return true;
            }

            // CHUNITHM custom card range (official CHU up to 2298002; custom 2299000..2999999)
            if (cardId >= 2299000 && cardId <= 2999999)
            {
                return true;
            }

            // Ongeki custom card range (official MU3 up to 102561; custom 109000..199999)
            if (cardId >= 109000 && cardId <= 199999)
            {
                return true;
            }

            return false;
        }

        public static void RegisterCustomCardId(int cardId)
        {
            if (cardId > 0 && !KnownCustomCardIds.Contains(cardId))
            {
                KnownCustomCardIds.Add(cardId);
            }
        }

        /// <summary>
        /// Returns integer IDs for the 4 selectable pass types (2 = Bronze, 3 = Silver, 4 = Gold, 6 = Freedom).
        /// </summary>
        public static int[] GetValidPassTypeIds()
        {
            return new int[] { 2, 3, 4, 6 };
        }

        /// <summary>
        /// Returns all 4 pass types that are selectable and printable (Bronze, Silver, Gold, Freedom).
        /// In unmodified CardMaker, only GoldPass (4) and FreedomPass (6) are displayed.
        /// </summary>
        public static Table[] GetValidPassTypes()
        {
            return new Table[]
            {
                Table.BronzePass,
                Table.SilverPass,
                Table.GoldPass,
                Table.PlatinumPass,
                Table.FreedomPass
            };
        }

        /// <summary>
        /// Resolves credit costs for all pass type IDs.
        /// MAIStatic.getCredit defaults to -1 for BronzePass and SilverPass, preventing printing.
        /// </summary>
        public static int GetPassCredit(int typeId)
        {
            switch (typeId)
            {
                case 2: // BronzePass
                case 3: // SilverPass
                    return 1;
                case 4: // GoldPass
                    return 2;
                case 5: // PlatinumPass
                case 6: // FreedomPass
                    return 3;
                default:
                    return 1;
            }
        }

        public static int GetPassCredit(Table type)
        {
            return GetPassCredit((int)type);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static string TryGetUnityDataPath()
        {
            try
            {
                return Application.dataPath;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gathers candidate search root directories where option/Axxx folders might reside.
        /// </summary>
        public static List<string> GetOptionRootDirectories()
        {
            List<string> roots = new List<string>();

            // Virtual mount point used by segatools / CardMaker RomConfig.xml
            AddIfDirectoryExists(roots, "C:/mount/Option");
            AddIfDirectoryExists(roots, "C:\\mount\\Option");
            AddIfDirectoryExists(roots, "C:/Users/Baconway/Downloads/sded 1.44/package/option");

            // Relative to Application.dataPath (CardMaker_Data)
            try
            {
                string dataPath = TryGetUnityDataPath();
                if (!string.IsNullOrEmpty(dataPath))
                {
                    AddIfDirectoryExists(roots, Path.Combine(dataPath, "../option"));
                    AddIfDirectoryExists(roots, Path.Combine(dataPath, "../../option"));
                    AddIfDirectoryExists(roots, Path.Combine(dataPath, "../../package/option"));
                    AddIfDirectoryExists(roots, Path.Combine(dataPath, "../../../package/option"));
                }
            }
            catch
            {
            }

            // Current working directory and parent paths
            try
            {
                string cwd = Directory.GetCurrentDirectory();
                if (!string.IsNullOrEmpty(cwd))
                {
                    AddIfDirectoryExists(roots, Path.Combine(cwd, "package/option"));
                    AddIfDirectoryExists(roots, Path.Combine(cwd, "option"));
                    AddIfDirectoryExists(roots, Path.Combine(cwd, "../package/option"));
                    AddIfDirectoryExists(roots, Path.Combine(cwd, "../../package/option"));
                    AddIfDirectoryExists(roots, Path.Combine(cwd, "../../../package/option"));
                }
            }
            catch
            {
            }

            return roots;
        }

        private static void AddIfDirectoryExists(List<string> list, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                string normalized = path.Replace('\\', '/');
                if (Directory.Exists(normalized) && !list.Contains(normalized))
                {
                    list.Add(normalized);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Discovers all existing option subdirectories for a given game (e.g. MAI, MU3, CHU).
        /// </summary>
        public static List<string> GetOptionPackPaths(string gameSubDir)
        {
            List<string> packPaths = new List<string>();
            List<string> roots = GetOptionRootDirectories();

            for (int r = 0; r < roots.Count; r++)
            {
                string root = roots[r];
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(root);
                    DirectoryInfo[] aDirs = dirInfo.GetDirectories("A???");
                    for (int d = 0; d < aDirs.Length; d++)
                    {
                        DirectoryInfo aDir = aDirs[d];
                        string gamePath = Path.Combine(aDir.FullName, gameSubDir).Replace('\\', '/');
                        if (Directory.Exists(gamePath) && !packPaths.Contains(gamePath))
                        {
                            packPaths.Add(gamePath);
                        }
                    }
                }
                catch
                {
                }
            }

            return packPaths;
        }

        /// <summary>
        /// Performs an upfront scan of option root directories and logs all custom option packs (A083+) found.
        /// </summary>
        public static void ScanAndLogCustomOptions()
        {
            if (ModConfig.LoadCustomOptCards != null && !ModConfig.LoadCustomOptCards.Value)
            {
                SafeLog("[CardMakerCustomizer] [CustomOpt] Custom option loading is disabled in configuration.");
                return;
            }

            try
            {
                SafeLog("[CardMakerCustomizer] [CustomOpt] Scanning for custom option packs (A083+)...");
                List<string> roots = GetOptionRootDirectories();
                HashSet<string> scannedDirs = new HashSet<string>();
                int totalCustomPacks = 0;

                for (int r = 0; r < roots.Count; r++)
                {
                    string root = roots[r];
                    if (!Directory.Exists(root))
                    {
                        continue;
                    }

                    try
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(root);
                        DirectoryInfo[] aDirs = dirInfo.GetDirectories("A???");
                        for (int d = 0; d < aDirs.Length; d++)
                        {
                            DirectoryInfo aDir = aDirs[d];
                            string fullPath = aDir.FullName.Replace('\\', '/');
                            if (scannedDirs.Contains(fullPath))
                            {
                                continue;
                            }
                            scannedDirs.Add(fullPath);

                            if (IsCustomOptionPack(aDir.Name))
                            {
                                totalCustomPacks++;
                                InspectAndLogOptionPack(aDir);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SafeWarn($"[CardMakerCustomizer] [CustomOpt] Error scanning option root '{root}': {ex.Message}");
                    }
                }

                if (totalCustomPacks == 0)
                {
                    SafeLog("[CardMakerCustomizer] [CustomOpt] No custom option packs (A083+) detected in search paths.");
                }
                else
                {
                    SafeLog($"[CardMakerCustomizer] [CustomOpt] Initial scan complete: {totalCustomPacks} custom option pack(s) detected, {KnownCustomCardIds.Count} custom card(s) indexed.");
                }
            }
            catch (Exception ex)
            {
                SafeWarn($"[CardMakerCustomizer] [CustomOpt] Failed during custom option scan: {ex.Message}");
            }
        }

        private static void InspectAndLogOptionPack(DirectoryInfo aDir)
        {
            string packName = aDir.Name;
            string fullPath = aDir.FullName.Replace('\\', '/');
            SafeLog($"[CardMakerCustomizer] [CustomOpt] Discovered custom option pack: {packName} ({fullPath})");

            string dataConfigPath = Path.Combine(aDir.FullName, "DataConfig.xml");
            if (File.Exists(dataConfigPath))
            {
                string verStr = ReadDataConfigVersion(dataConfigPath);
                SafeLog($"[CardMakerCustomizer] [CustomOpt]   - DataConfig.xml: {verStr}");
            }
            else
            {
                SafeLog("[CardMakerCustomizer] [CustomOpt]   - DataConfig.xml: Not present at pack root");
            }

            string maiDir = Path.Combine(aDir.FullName, "MAI");
            if (Directory.Exists(maiDir))
            {
                InspectMaiOptionDir(maiDir);
            }

            string mu3Dir = Path.Combine(aDir.FullName, "MU3");
            if (Directory.Exists(mu3Dir))
            {
                InspectMu3OptionDir(mu3Dir);
            }

            string chuDir = Path.Combine(aDir.FullName, "CHU");
            if (Directory.Exists(chuDir))
            {
                InspectChuOptionDir(chuDir);
            }
        }

        private static string ReadDataConfigVersion(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                string major = ExtractXmlValue(content, "major");
                string minor = ExtractXmlValue(content, "minor");
                string release = ExtractXmlValue(content, "release");
                if (!string.IsNullOrEmpty(major) && !string.IsNullOrEmpty(minor))
                {
                    return string.IsNullOrEmpty(release) ? $"Version {major}.{minor}" : $"Version {major}.{minor}.{release}";
                }
            }
            catch
            {
            }
            return "Present";
        }

        private static string ExtractXmlValue(string xml, string tag)
        {
            string openTag = "<" + tag + ">";
            string closeTag = "</" + tag + ">";
            int start = xml.IndexOf(openTag, StringComparison.OrdinalIgnoreCase);
            if (start >= 0)
            {
                start += openTag.Length;
                int end = xml.IndexOf(closeTag, start, StringComparison.OrdinalIgnoreCase);
                if (end > start)
                {
                    return xml.Substring(start, end - start).Trim();
                }
            }
            return null;
        }

        private static void InspectMaiOptionDir(string maiDir)
        {
            string assetBytes = Path.Combine(maiDir, "assets_mai/assets_mai.bytes");
            string assetStatus = File.Exists(assetBytes) ? $"Present ({new FileInfo(assetBytes).Length / 1024} KB)" : "Not found";

            string cardDir = Path.Combine(maiDir, "card");
            List<string> cardSummaries = new List<string>();
            if (Directory.Exists(cardDir))
            {
                DirectoryInfo cDirInfo = new DirectoryInfo(cardDir);
                DirectoryInfo[] cDirs = cDirInfo.GetDirectories("card*");
                for (int i = 0; i < cDirs.Length; i++)
                {
                    string cXmlPath = Path.Combine(cDirs[i].FullName, "Card.xml");
                    int cardId = 0;
                    string cardName = null;
                    string cardType = null;

                    if (File.Exists(cXmlPath))
                    {
                        try
                        {
                            string xml = File.ReadAllText(cXmlPath);
                            string idStr = ExtractXmlValue(xml, "id");
                            int.TryParse(idStr, out cardId);
                            cardName = ExtractXmlValue(xml, "str");
                            int typeIdx = xml.IndexOf("<type>", StringComparison.OrdinalIgnoreCase);
                            if (typeIdx >= 0)
                            {
                                cardType = ExtractXmlValue(xml.Substring(typeIdx), "str");
                            }
                        }
                        catch
                        {
                        }
                    }

                    if (cardId == 0)
                    {
                        string numPart = cDirs[i].Name.Substring(4);
                        int.TryParse(numPart, out cardId);
                    }

                    if (cardId > 0)
                    {
                        RegisterCustomCard(cardId, cardName, cardType);
                        string desc = cardId.ToString();
                        if (!string.IsNullOrEmpty(cardName)) desc += $" ({cardName})";
                        if (!string.IsNullOrEmpty(cardType)) desc += $" [Type: {cardType}]";
                        cardSummaries.Add(desc);
                    }
                }
            }

            SafeLog($"[CardMakerCustomizer] [CustomOpt]   - MAI: {cardSummaries.Count} card(s), AssetBundle: {assetStatus}");
            for (int i = 0; i < cardSummaries.Count; i++)
            {
                SafeLog($"[CardMakerCustomizer] [CustomOpt]       * Card: {cardSummaries[i]}");
            }
        }

        private static void InspectMu3OptionDir(string mu3Dir)
        {
            string assetBytes = Path.Combine(mu3Dir, "assets_mu3/assets_mu3.bytes");
            string assetStatus = File.Exists(assetBytes) ? $"Present ({new FileInfo(assetBytes).Length / 1024} KB)" : "Not found";

            string cardDir = Path.Combine(mu3Dir, "card");
            int cardCount = 0;
            if (Directory.Exists(cardDir))
            {
                DirectoryInfo cDirInfo = new DirectoryInfo(cardDir);
                DirectoryInfo[] cDirs = cDirInfo.GetDirectories("card*");
                cardCount = cDirs.Length;
                for (int i = 0; i < cDirs.Length; i++)
                {
                    string numPart = cDirs[i].Name.Substring(4);
                    int cardId;
                    if (int.TryParse(numPart, out cardId))
                    {
                        RegisterCustomCard(cardId);
                    }
                }
            }

            SafeLog($"[CardMakerCustomizer] [CustomOpt]   - MU3: {cardCount} card(s), AssetBundle: {assetStatus}");
        }

        private static void InspectChuOptionDir(string chuDir)
        {
            string cardDir = Path.Combine(chuDir, "card");
            int cardCount = 0;
            if (Directory.Exists(cardDir))
            {
                DirectoryInfo cDirInfo = new DirectoryInfo(cardDir);
                DirectoryInfo[] cDirs = cDirInfo.GetDirectories("card*");
                cardCount = cDirs.Length;
                for (int i = 0; i < cDirs.Length; i++)
                {
                    string numPart = cDirs[i].Name.Substring(4);
                    int cardId;
                    if (int.TryParse(numPart, out cardId))
                    {
                        RegisterCustomCard(cardId);
                    }
                }
            }

            SafeLog($"[CardMakerCustomizer] [CustomOpt]   - CHU: {cardCount} card(s)");
        }

        /// <summary>
        /// Injects custom opt cards (and all valid cards if UnlockAllCards is enabled)
        /// into MAISellingCardContext.sellingList_ before setupCharaListNewMark() executes.
        /// </summary>
        public static void InjectMaimaiSellingCards(MAISellingCardContext context)
        {
            if ((object)context == null)
            {
                return;
            }

            try
            {
                var maiDataManager = MAIDataManager.GetInstance();
                if ((object)maiDataManager == null || maiDataManager.CardDataAll == null)
                {
                    return;
                }

                var traverse = Traverse.Create(context);
                var sellingList = traverse.Field("sellingList_").GetValue<List<MAISellingCard>>();
                if (sellingList == null)
                {
                    sellingList = new List<MAISellingCard>();
                    traverse.Field("sellingList_").SetValue(sellingList);
                }

                HashSet<int> existingIds = new HashSet<int>();
                for (int i = 0; i < sellingList.Count; i++)
                {
                    if (sellingList[i] != null)
                    {
                        existingIds.Add(sellingList[i].ID);
                    }
                }

                int injectedCount = 0;
                foreach (var kvp in maiDataManager.CardDataAll)
                {
                    int cardId = kvp.Key;
                    var cardData = kvp.Value;

                    if (cardData == null || existingIds.Contains(cardId))
                    {
                        continue;
                    }

                    bool shouldAdd = ModConfig.UnlockAllCards.Value || IsCustomCard(cardId);
                    if (shouldAdd)
                    {
                        RegisterCustomCardId(cardId);
                        GameSellingCard sellingCard = new GameSellingCard
                        {
                            cardId = cardId,
                            startDate = "2020-01-01 00:00:00",
                            endDate = "2099-12-31 23:59:59",
                            noticeStartDate = "2020-01-01 00:00:00",
                            noticeEndDate = "2099-12-31 23:59:59"
                        };

                        sellingList.Add(new MAISellingCard(sellingCard));
                        existingIds.Add(cardId);
                        injectedCount++;

                        if (IsCustomCard(cardId))
                        {
                            string cardName = GetCustomCardName(cardId);
                            string cardType = GetCustomCardType(cardId);
                            string desc = cardId.ToString();
                            if (!string.IsNullOrEmpty(cardName)) desc += $" ({cardName})";
                            if (!string.IsNullOrEmpty(cardType)) desc += $" [Type: {cardType}]";
                            SafeLog($"[CardMakerCustomizer] [CustomOpt] [MAI] Injected custom card {desc} into selling catalog.");
                        }
                    }
                }

                if (injectedCount > 0)
                {
                    SafeLog($"[CardMakerCustomizer] [CustomOpt] [MAI] Injected total {injectedCount} custom/unlocked card(s) into maimai selling catalog.");
                }
            }
            catch (Exception ex)
            {
                SafeWarn($"[CardMakerCustomizer] [CustomOpt] Failed to inject maimai selling cards: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures all option packs containing MAI/assets_mai/assets_mai.bytes
        /// are appended to AssetBundleDB for Title.Maimai.
        /// </summary>
        public static void EnsureMaimaiAssetBundlesLoaded()
        {
            try
            {
                AssetBundleDB db = AssetBundleDB.getInstance(AssetBundleDB.Title.Maimai);
                if ((object)db == null)
                {
                    return;
                }

                var rootList = Traverse.Create(db).Field("root_").GetValue<List<string>>();
                List<string> candidatePacks = GetOptionPackPaths("MAI");

                // Also check MAIDataManager.OptionPath
                try
                {
                    var mgr = ContextBase<CommonContext>.Instance?.MAIDataManager;
                    if ((object)mgr != null && mgr.OptionPath != null)
                    {
                        for (int i = 0; i < mgr.OptionPath.Count; i++)
                        {
                            string p = mgr.OptionPath[i];
                            if (!string.IsNullOrEmpty(p))
                            {
                                string norm = p.Replace('\\', '/');
                                if (!candidatePacks.Contains(norm))
                                {
                                    candidatePacks.Add(norm);
                                }
                            }
                        }
                    }
                }
                catch
                {
                }

                for (int i = 0; i < candidatePacks.Count; i++)
                {
                    string packPath = candidatePacks[i];
                    string bytesPath = Path.Combine(packPath, "assets_mai/assets_mai.bytes").Replace('\\', '/');
                    if (File.Exists(bytesPath))
                    {
                        string assetDir = (packPath + "/assets_mai/").Replace('\\', '/');
                        bool alreadyLoaded = false;
                        if (rootList != null)
                        {
                            for (int r = 0; r < rootList.Count; r++)
                            {
                                if (string.Equals(rootList[r]?.Replace('\\', '/'), assetDir, StringComparison.OrdinalIgnoreCase))
                                {
                                    alreadyLoaded = true;
                                    break;
                                }
                            }
                        }

                        if (!alreadyLoaded)
                        {
                            db.appendAssetBundleSet(packPath);
                            SafeLog($"[CardMakerCustomizer] [CustomOpt] [MAI] Successfully loaded custom AssetBundle set: {packPath}/assets_mai/assets_mai.bytes");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SafeWarn($"[CardMakerCustomizer] Failed to ensure maimai asset bundles: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures all option packs containing MU3/assets_mu3/assets_mu3.bytes
        /// are appended to AssetBundleDB for Title.MU3.
        /// </summary>
        public static void EnsureOngekiAssetBundlesLoaded()
        {
            try
            {
                AssetBundleDB db = AssetBundleDB.getInstance(AssetBundleDB.Title.MU3);
                if ((object)db == null)
                {
                    return;
                }

                var rootList = Traverse.Create(db).Field("root_").GetValue<List<string>>();
                List<string> candidatePacks = GetOptionPackPaths("MU3");

                // Also check MU3DataManager.OptionPath
                try
                {
                    var mgr = ContextBase<CommonContext>.Instance?.MU3DataManager;
                    if ((object)mgr != null && mgr.OptionPath != null)
                    {
                        for (int i = 0; i < mgr.OptionPath.Count; i++)
                        {
                            string p = mgr.OptionPath[i];
                            if (!string.IsNullOrEmpty(p))
                            {
                                string norm = p.Replace('\\', '/');
                                if (!candidatePacks.Contains(norm))
                                {
                                    candidatePacks.Add(norm);
                                }
                            }
                        }
                    }
                }
                catch
                {
                }

                for (int i = 0; i < candidatePacks.Count; i++)
                {
                    string packPath = candidatePacks[i];
                    string bytesPath = Path.Combine(packPath, "assets_mu3/assets_mu3.bytes").Replace('\\', '/');
                    if (File.Exists(bytesPath))
                    {
                        string assetDir = (packPath + "/assets_mu3/").Replace('\\', '/');
                        bool alreadyLoaded = false;
                        if (rootList != null)
                        {
                            for (int r = 0; r < rootList.Count; r++)
                            {
                                if (string.Equals(rootList[r]?.Replace('\\', '/'), assetDir, StringComparison.OrdinalIgnoreCase))
                                {
                                    alreadyLoaded = true;
                                    break;
                                }
                            }
                        }

                        if (!alreadyLoaded)
                        {
                            db.appendAssetBundleSet(packPath);
                            SafeLog($"[CardMakerCustomizer] [CustomOpt] [MU3] Successfully loaded custom AssetBundle set: {packPath}/assets_mu3/assets_mu3.bytes");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SafeWarn($"[CardMakerCustomizer] Failed to ensure Ongeki asset bundles: {ex.Message}");
            }
        }
    }
}
