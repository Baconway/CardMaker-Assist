using System;
using HarmonyLib;
using CardMaker.Common;
using CardMaker.Common.Operation;
using CardMaker.MAI;
using CardMaker.CHU;
using CardMaker.MU3;
using CardMakerCustomizer.Config;
using MelonLoader;

namespace CardMakerCustomizer.Patches
{
    [HarmonyPatch(typeof(MAIContext), "isBlockLoginForPreviewUserVersion")]
    public static class Patch_MAI_isBlockLoginForPreviewUserVersion
    {
        public static bool Prefix(ref bool __result)
        {
            if (ModConfig.BypassAllVersionChecks.Value || ModConfig.BypassMaimaiBucketLock.Value)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MAIContext), "isNewUserVersionThanCardmaker")]
    public static class Patch_MAI_isNewUserVersionThanCardmaker
    {
        public static bool Prefix(ref bool __result)
        {
            if (ModConfig.BypassAllVersionChecks.Value || ModConfig.BypassMaimaiBucketLock.Value)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MAIContext), "isSameVersionAsCardMaker", new Type[] { typeof(CardMaker.Common.Version), typeof(CardMaker.Common.Version) })]
    public static class Patch_MAI_isSameVersionAsCardMaker
    {
        public static bool Prefix(ref bool __result)
        {
            if (ModConfig.BypassAllVersionChecks.Value || ModConfig.BypassMaimaiBucketLock.Value)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CHUContext), "isBlockLoginForPreviewUserVersion")]
    public static class Patch_CHU_isBlockLoginForPreviewUserVersion
    {
        public static bool Prefix(ref bool __result)
        {
            if (ModConfig.BypassAllVersionChecks.Value || ModConfig.BypassChunithmVersionLock.Value)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CHUContext), "isNewUserVersionThanCardmaker")]
    public static class Patch_CHU_isNewUserVersionThanCardmaker
    {
        public static bool Prefix(ref bool __result)
        {
            if (ModConfig.BypassAllVersionChecks.Value || ModConfig.BypassChunithmVersionLock.Value)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MU3User), "CardMakerVersion", MethodType.Getter)]
    public static class Patch_MU3User_CardMakerVersion
    {
        public static bool Prefix(ref CardMaker.Common.Version __result)
        {
            if (ModConfig.BypassAllVersionChecks.Value || ModConfig.BypassOngekiVersionCheck.Value)
            {
                try
                {
                    __result = ContextBase<CommonContext>.Instance.MU3DataManager.DataConfig.CardMakerVersion;
                    return false;
                }
                catch
                {
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MU3_Topmenu_Top), "isGachaEnable")]
    public static class Patch_MU3_Topmenu_Top_isGachaEnable
    {
        public static bool Prefix(ref bool __result)
        {
            if (ModConfig.BypassAllVersionChecks.Value || ModConfig.BypassServerGachaCheck.Value)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(OperationManager), "MU3CardMakerVersion", MethodType.Getter)]
    public static class Patch_OperationManager_MU3CardMakerVersion
    {
        public static bool Prefix(ref CardMaker.Common.Version __result)
        {
            if (ModConfig.BypassAllVersionChecks.Value || ModConfig.BypassServerGachaCheck.Value)
            {
                try
                {
                    __result = ContextBase<CommonContext>.Instance.MU3DataManager.DataConfig.CardMakerVersion;
                    return false;
                }
                catch
                {
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(OperationManager), "ChuniCardMakerVersion", MethodType.Getter)]
    public static class Patch_OperationManager_ChuniCardMakerVersion
    {
        public static bool Prefix(ref CardMaker.Common.Version __result)
        {
            if (ModConfig.BypassAllVersionChecks.Value || ModConfig.BypassServerGachaCheck.Value)
            {
                try
                {
                    __result = ContextBase<CommonContext>.Instance.CHUDataManager.DataConfig.CardMakerVersion;
                    return false;
                }
                catch
                {
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(OperationManager), "MaimaiCardMakerVersion", MethodType.Getter)]
    public static class Patch_OperationManager_MaimaiCardMakerVersion
    {
        public static bool Prefix(ref CardMaker.Common.Version __result)
        {
            if (ModConfig.BypassAllVersionChecks.Value || ModConfig.BypassServerGachaCheck.Value)
            {
                try
                {
                    __result = MAIDataManager.GetInstance().DataConfig.CardMakerVersion;
                    return false;
                }
                catch
                {
                }
            }
            return true;
        }
    }
}
