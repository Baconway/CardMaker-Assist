using System;
using HarmonyLib;
using CardMaker.MU3;
using CardMaker.CHU;
using CardMaker.MAI;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.Logic;
using MelonLoader;

namespace CardMakerCustomizer.Patches
{
    [HarmonyPatch(typeof(MU3UserData), "copyFrom", new Type[] { typeof(CardMaker.MU3.UserData) })]
    public static class Patch_MU3UserData_copyFrom
    {
        public static void Postfix(MU3UserData __instance)
        {
            if (!ModConfig.EnableProfileOverride.Value)
            {
                return;
            }

            try
            {
                var tr = Traverse.Create(__instance);
                string customName = ModConfig.CustomUserName.Value;
                if (!StringCompat.IsNullOrWhiteSpace(customName))
                {
                    tr.Field("userName_").SetValue(customName.Trim());
                }

                int rating = ModConfig.CustomPlayerRating.Value;
                if (rating >= 0)
                {
                    tr.Field("playerRating_").SetValue(rating);
                }

                int highest = ModConfig.CustomHighestRating.Value;
                if (highest >= 0)
                {
                    tr.Field("highestRating_").SetValue(highest);
                }

                int level = ModConfig.CustomPlayerLevel.Value;
                if (level >= 0)
                {
                    tr.Field("level_").SetValue(level);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed to override MU3 profile: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(CHUUserData), "copyFrom", new Type[] { typeof(CardMaker.CHU.UserData) })]
    public static class Patch_CHUUserData_copyFrom
    {
        public static void Postfix(CHUUserData __instance)
        {
            if (!ModConfig.EnableProfileOverride.Value)
            {
                return;
            }

            try
            {
                var tr = Traverse.Create(__instance);
                var data = tr.Field("userData_").GetValue<CardMaker.CHU.UserData>();
                if (data != null)
                {
                    string customName = ModConfig.CustomUserName.Value;
                    if (!StringCompat.IsNullOrWhiteSpace(customName))
                    {
                        data.userName = customName.Trim();
                        data.userNameEx = customName.Trim();
                    }

                    int rating = ModConfig.CustomPlayerRating.Value;
                    if (rating >= 0)
                    {
                        data.playerRating = rating;
                    }

                    int highest = ModConfig.CustomHighestRating.Value;
                    if (highest >= 0)
                    {
                        data.highestRating = highest;
                    }

                    int level = ModConfig.CustomPlayerLevel.Value;
                    if (level >= 0)
                    {
                        data.level = level;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed to override CHUNITHM profile: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(MAIUserData), "copyFrom", new Type[] { typeof(CardMaker.MAI.UserData) })]
    public static class Patch_MAIUserData_copyFrom
    {
        public static void Postfix(MAIUserData __instance)
        {
            if (!ModConfig.EnableProfileOverride.Value)
            {
                return;
            }

            try
            {
                var tr = Traverse.Create(__instance);
                var data = tr.Field("userData_").GetValue<CardMaker.MAI.UserData>();
                if (data != null)
                {
                    string customName = ModConfig.CustomUserName.Value;
                    if (!StringCompat.IsNullOrWhiteSpace(customName))
                    {
                        data.userName = customName.Trim();
                    }

                    int rating = ModConfig.CustomPlayerRating.Value;
                    if (rating >= 0)
                    {
                        data.playerRating = rating;
                    }

                    int highest = ModConfig.CustomHighestRating.Value;
                    if (highest >= 0)
                    {
                        data.highestRating = highest;
                    }

                    string friendCode = ModConfig.CustomFriendCode.Value;
                    if (!StringCompat.IsNullOrWhiteSpace(friendCode))
                    {
                        data.friendCode = friendCode.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed to override maimai DX profile: {ex.Message}");
            }
        }
    }
}
