using System;
using System.Collections.Generic;
using HarmonyLib;
using CardMaker.Common;
using CardMaker.MAI;
using CardMaker.MAI.MaiStudio;
using CardMaker.MAI.MaiStudio.CardTypeName;
using CardMaker.MU3;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.Logic;
using MelonLoader;

namespace CardMakerCustomizer.Patches
{
    // ==========================================
    // maimai DX Card Normalization & Enablement
    // ==========================================

    [HarmonyPatch(typeof(CardTypeData), "IsDisable")]
    public static class Patch_CardTypeData_IsDisable
    {
        public static bool Prefix(CardTypeData __instance, ref bool __result)
        {
            if (ModConfig.LoadCustomOptCards.Value || ModConfig.UnlockAllCards.Value)
            {
                if ((object)__instance != null && (object)__instance.name != null)
                {
                    int typeId = __instance.name.id;
                    // 2: BronzePass, 3: SilverPass, 4: GoldPass, 5: PlatinumPass, 6: FreedomPass
                    if (typeId >= 2 && typeId <= 6)
                    {
                        __result = false;
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CardData), "IsDisable")]
    public static class Patch_CardData_IsDisable
    {
        public static bool Prefix(CardData __instance, ref bool __result)
        {
            if (ModConfig.LoadCustomOptCards.Value || ModConfig.UnlockAllCards.Value)
            {
                if ((object)__instance != null && (object)__instance.name != null)
                {
                    if (ModConfig.UnlockAllCards.Value || CustomOptHelper.IsCustomCard(__instance.name.id))
                    {
                        __result = false;
                        return false;
                    }
                }
            }
            return true;
        }
    }

    // ==========================================
    // maimai DX Selling Card Injection & Tabs
    // ==========================================

    [HarmonyPatch(typeof(MAISellingCardContext), "setupCharaListNewMark")]
    public static class Patch_MAISellingCardContext_setupCharaListNewMark
    {
        public static void Prefix(MAISellingCardContext __instance)
        {
            if (ModConfig.LoadCustomOptCards.Value || ModConfig.UnlockAllCards.Value)
            {
                CustomOptHelper.InjectMaimaiSellingCards(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(MAIHome_SelectPass), "createSellingType")]
    public static class Patch_MAIHome_SelectPass_createSellingType
    {
        public static void Postfix(MAIHome_SelectPass __instance)
        {
            if (ModConfig.LoadCustomOptCards.Value || ModConfig.UnlockAllCards.Value)
            {
                try
                {
                    Traverse.Create(__instance).Field("sellingTypeList_").SetValue(CustomOptHelper.GetValidPassTypes());
                }
                catch (Exception ex)
                {
                    CustomOptHelper.SafeWarn($"[CardMakerCustomizer] Failed to set sellingTypeList_: {ex.Message}");
                }
            }
        }
    }

    [HarmonyPatch(typeof(MAIStatic), "getCredit", new Type[] { typeof(Table) })]
    public static class Patch_MAIStatic_getCredit
    {
        public static void Postfix(Table cardType, ref int __result)
        {
            if (ModConfig.LoadCustomOptCards.Value || ModConfig.UnlockAllCards.Value)
            {
                if (__result <= 0)
                {
                    __result = CustomOptHelper.GetPassCredit(cardType);
                }
            }
        }
    }

    // ==========================================
    // AssetBundleDB Registration on Context Awake
    // ==========================================

    [HarmonyPatch(typeof(MAIContext), "Awake")]
    public static class Patch_MAIContext_Awake
    {
        public static void Postfix()
        {
            if (ModConfig.LoadCustomOptCards.Value)
            {
                CustomOptHelper.EnsureMaimaiAssetBundlesLoaded();
            }
        }
    }

    [HarmonyPatch(typeof(MU3Context), "Awake")]
    public static class Patch_MU3Context_Awake
    {
        public static void Postfix()
        {
            if (ModConfig.LoadCustomOptCards.Value)
            {
                CustomOptHelper.EnsureOngekiAssetBundlesLoaded();
            }
        }
    }

    // ==========================================
    // Ongeki Option Version Mismatch Bypass
    // ==========================================

    [HarmonyPatch(typeof(MU3DataManager.Loader), "equals", new Type[] { typeof(CardMaker.MU3.DataStudio.Version), typeof(CardMaker.MU3.DataStudio.Version) })]
    public static class Patch_MU3DataManager_Loader_equals
    {
        public static void Postfix(ref bool __result)
        {
            if (ModConfig.LoadCustomOptCards.Value)
            {
                __result = true;
            }
        }
    }

    // ==========================================
    // Option Mount Logging Patches
    // ==========================================

    [HarmonyPatch(typeof(MAIDataManager.Loader), "mount")]
    public static class Patch_MAIDataManager_Loader_mount
    {
        public static void Postfix(MAIDataManager.Loader __instance, bool __result)
        {
            if (ModConfig.LoadCustomOptCards.Value && __result)
            {
                try
                {
                    var optPaths = Traverse.Create(__instance).Field("dataManager_").Field("optionPath_").GetValue<List<string>>();
                    if (optPaths != null)
                    {
                        for (int i = 0; i < optPaths.Count; i++)
                        {
                            string p = optPaths[i];
                            if (CustomOptHelper.IsCustomOptionPack(p))
                            {
                                CustomOptHelper.LogCustomOptionMounted("MAI", p);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }

    [HarmonyPatch(typeof(MU3DataManager.Loader), "mount")]
    public static class Patch_MU3DataManager_Loader_mount
    {
        public static void Postfix(MU3DataManager.Loader __instance, bool __result)
        {
            if (ModConfig.LoadCustomOptCards.Value && __result)
            {
                try
                {
                    var optPaths = Traverse.Create(__instance).Field("dataManager_").Field("optionPath_").GetValue<List<string>>();
                    if (optPaths != null)
                    {
                        for (int i = 0; i < optPaths.Count; i++)
                        {
                            string p = optPaths[i];
                            if (CustomOptHelper.IsCustomOptionPack(p))
                            {
                                CustomOptHelper.LogCustomOptionMounted("MU3", p);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.CHUDataManager.Loader), "mount")]
    public static class Patch_CHUDataManager_Loader_mount
    {
        public static void Postfix(CardMaker.CHU.CHUDataManager.Loader __instance, bool __result)
        {
            if (ModConfig.LoadCustomOptCards.Value && __result)
            {
                try
                {
                    var optPaths = CustomOptHelper.GetOptionPackPaths("CHU");
                    for (int i = 0; i < optPaths.Count; i++)
                    {
                        string p = optPaths[i];
                        if (CustomOptHelper.IsCustomOptionPack(p))
                        {
                            CustomOptHelper.LogCustomOptionMounted("CHU", p);
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }
}
