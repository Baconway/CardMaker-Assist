using System;
using HarmonyLib;
using MelonLoader;
using CardMaker.Common.AM;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.Logic;

namespace CardMakerCustomizer.Patches
{
    // ==========================================
    // Universal Infinite Credits Patches
    // ==========================================

    [HarmonyPatch(typeof(Credit), "isGameCostEnough")]
    public static class Patch_Credit_isGameCostEnough
    {
        public static bool Prefix(Credit.GameCost gameCost, int count, ref bool __result)
        {
            if (CreditHelper.IsInfiniteCreditsEnabled())
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Credit), "payGameCost")]
    public static class Patch_Credit_payGameCost
    {
        public static bool Prefix(Credit __instance, Credit.GameCost gameCost, int count, ref bool __result)
        {
            if (CreditHelper.IsInfiniteCreditsEnabled())
            {
                if (count > 0)
                {
                    try
                    {
                        var traverse = Traverse.Create(__instance);
                        int current = traverse.Field("_payedCredits").GetValue<int>();
                        traverse.Field("_payedCredits").SetValue(CreditHelper.CalculatePayedCredits(current, 1, count));
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"[CardMakerCustomizer] Failed to update _payedCredits: {ex.Message}");
                    }
                }

                __instance.execute();
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Credit), "Count", MethodType.Getter)]
    public static class Patch_Credit_Count
    {
        public static bool Prefix(ref int __result)
        {
            if (CreditHelper.IsInfiniteCreditsEnabled())
            {
                __result = CreditHelper.GetDisplayCreditCount();
                return false;
            }

            return true;
        }
    }
}
