using System;
using HarmonyLib;
using UnityEngine;
using CardMaker.Common;
using CardMaker.MU3;
using CardMaker.MU3.DataStudio;
using CardMaker.MAI;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.Logic;
using MelonLoader;

namespace CardMakerCustomizer.Patches
{
    // ==========================================
    // Ongeki (MU3) Card Renderer Holo Patches
    // ==========================================

    [HarmonyPatch(typeof(MU3CardRenderer), "renderHolo")]
    public static class Patch_MU3CardRenderer_renderHolo
    {
        public static void Postfix(ref Color32[] __result)
        {
            if (__result == null || !ModConfig.EnableHoloCustomization.Value)
            {
                return;
            }

            try
            {
                HoloHelper.ProcessHoloMaskPixels(__result, ModConfig.HoloAlpha.Value, ModConfig.InvertHoloMask.Value);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed in MU3 renderHolo patch: {ex.Message}");
            }
        }
    }

    // ==========================================
    // Maimai DX (MAI) Card Renderer Holo Patches
    // ==========================================

    [HarmonyPatch(typeof(MAICardRenderer), "renderHolo", new Type[] { typeof(int), typeof(int) })]
    public static class Patch_MAICardRenderer_renderHolo
    {
        public static void Postfix(ref Color32[] __result)
        {
            if (__result == null || !ModConfig.EnableHoloCustomization.Value)
            {
                return;
            }

            try
            {
                HoloHelper.ProcessHoloMaskPixels(__result, ModConfig.HoloAlpha.Value, ModConfig.InvertHoloMask.Value);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed in MAI renderHolo patch: {ex.Message}");
            }
        }
    }

    // ==========================================
    // UI Screen Preview Shimmer & Intensity Patches
    // ==========================================

    [HarmonyPatch(typeof(UI_CMN_CardData_00), "setHoloMask", new Type[] { typeof(Texture2D) })]
    public static class Patch_UI_CMN_CardData_00_setHoloMask
    {
        public static void Postfix(UI_CMN_CardData_00 __instance)
        {
            if (!ModConfig.EnableHoloCustomization.Value || __instance == null)
            {
                return;
            }

            try
            {
                var holoPreview = Traverse.Create(__instance).Field("holoPreview_").GetValue<GameObject>();
                if (holoPreview != null)
                {
                    if (ModConfig.ForceEnableHolo.Value)
                    {
                        holoPreview.SetActive(true);
                    }
                    HoloHelper.ApplyPreviewIntensity(holoPreview.transform, ModConfig.HoloPreviewIntensity.Value);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed in UI_CMN setHoloMask patch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(UI_CMA_CardData_00), "setHoloMask", new Type[] { typeof(Texture2D) })]
    public static class Patch_UI_CMA_CardData_00_setHoloMask
    {
        public static void Postfix(UI_CMA_CardData_00 __instance)
        {
            if (!ModConfig.EnableHoloCustomization.Value || __instance == null)
            {
                return;
            }

            try
            {
                var holoPreview = Traverse.Create(__instance).Field("holoPreview_").GetValue<GameObject>();
                if (holoPreview != null)
                {
                    if (ModConfig.ForceEnableHolo.Value)
                    {
                        holoPreview.SetActive(true);
                    }
                    HoloHelper.ApplyPreviewIntensity(holoPreview.transform, ModConfig.HoloPreviewIntensity.Value);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed in UI_CMA setHoloMask patch: {ex.Message}");
            }
        }
    }

    // ==========================================
    // Force Holo Rarity & Type Identification Patches
    // ==========================================

    [HarmonyPatch(typeof(MU3Static), "hasSpecialHolo", new Type[] { typeof(RarityType) })]
    public static class Patch_MU3Static_hasSpecialHolo
    {
        public static bool Prefix(ref bool __result)
        {
            if (ModConfig.EnableHoloCustomization.Value && ModConfig.ForceEnableHolo.Value)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MAIStatic), "isHolo", new Type[] { typeof(CardMaker.MAI.MaiStudio.CardTypeName.Table) })]
    public static class Patch_MAIStatic_isHolo
    {
        public static bool Prefix(ref bool __result)
        {
            if (ModConfig.EnableHoloCustomization.Value && ModConfig.ForceEnableHolo.Value)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    // ==========================================
    // Physical Printer Thermal Foil Transfer Patches
    // ==========================================

    [HarmonyPatch(typeof(PrintLib.PrintQuery), MethodType.Constructor, new Type[] { typeof(bool), typeof(bool) })]
    public static class Patch_PrintLib_PrintQuery_Constructor
    {
        public static void Postfix(PrintLib.PrintQuery __instance)
        {
            if (ModConfig.EnableHoloCustomization.Value && ModConfig.ForceEnableHolo.Value && __instance != null)
            {
                __instance.isHolo_ = true;
                PrintLib.setWORD(__instance.polish_, 0, 21);
            }
        }
    }

    [HarmonyPatch(typeof(PrintLib.PrintQuery), "copyTextureHolo", new Type[] { typeof(Color32[]) })]
    public static class Patch_PrintLib_PrintQuery_copyTextureHolo_Color32
    {
        public static void Postfix(PrintLib.PrintQuery __instance)
        {
            if (!ModConfig.EnableHoloCustomization.Value || __instance == null || __instance.holo_ == null)
            {
                return;
            }

            try
            {
                HoloHelper.ProcessHoloFoilBuffer(__instance.holo_, ModConfig.HoloFoilHeat.Value);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed in copyTextureHolo Color32 patch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(PrintLib.PrintQuery), "copyTextureHolo", new Type[] { typeof(Texture2D) })]
    public static class Patch_PrintLib_PrintQuery_copyTextureHolo_Texture2D
    {
        public static void Postfix(PrintLib.PrintQuery __instance)
        {
            if (!ModConfig.EnableHoloCustomization.Value || __instance == null || __instance.holo_ == null)
            {
                return;
            }

            try
            {
                HoloHelper.ProcessHoloFoilBuffer(__instance.holo_, ModConfig.HoloFoilHeat.Value);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed in copyTextureHolo Texture2D patch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(PrintLib), "addRequest")]
    public static class Patch_PrintLib_addRequest
    {
        public static void Prefix(PrintLib.IQuery query)
        {
            if (!ModConfig.EnableHoloCustomization.Value || !(query is PrintLib.PrintQuery printQuery))
            {
                return;
            }

            try
            {
                if (ModConfig.ForceEnableHolo.Value)
                {
                    printQuery.isHolo_ = true;
                    PrintLib.setWORD(printQuery.polish_, 0, 21);
                    HoloHelper.EnsureHoloBufferHasData(printQuery.holo_, ModConfig.HoloFoilHeat.Value);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed in PrintLib addRequest patch: {ex.Message}");
            }
        }
    }
}
