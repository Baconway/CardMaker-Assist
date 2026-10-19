using System;
using System.Collections.Generic;
using HarmonyLib;
using CardMaker.Common;
using CardMaker.MU3;
using CardMakerCustomizer.Config;
using MelonLoader;

namespace CardMakerCustomizer.Patches
{
    // ==========================================
    // Ongeki (MU3) Server Compatibility & Error Recovery Patches
    // ==========================================

    [HarmonyPatch(typeof(CardMaker.MU3.APIInitializer), "initialize")]
    public static class Patch_MU3_APIInitializer_initialize
    {
        public static void Postfix()
        {
            try
            {
                CardMaker.Common.APIInitializer.setSafix<GetUserGachaSupply>(1);
                MelonLogger.Msg("[CardMakerCustomizer] Fixed missing GetUserGachaSupply Safix in CardMaker.MU3.APIInitializer.");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed to set Safix for GetUserGachaSupply: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketGetUserData), "proc")]
    public static class Patch_PacketGetUserData_proc
    {
        public static void Postfix(CardMaker.MU3.PacketGetUserData __instance, ref PacketBase.State __result)
        {
            if (!ModConfig.UnlockAllCards.Value && !ModConfig.EnableProfileOverride.Value && (ModConfig.EnableOfflineBypass == null || !ModConfig.EnableOfflineBypass.Value))
            {
                return;
            }

            if (__result == PacketBase.State.Error || (__result == PacketBase.State.Done && !__instance.IsExistUser))
            {
                try
                {
                    var traverse = Traverse.Create(__instance);
                    var user = traverse.Field("user_").GetValue<MU3User>();
                    if (user != null)
                    {
                        UserData userData = new UserData();
                        userData.userName = !string.IsNullOrEmpty(ModConfig.CustomUserName.Value) ? ModConfig.CustomUserName.Value : "CARDMAKER";
                        userData.level = ModConfig.CustomPlayerLevel.Value > 0 ? ModConfig.CustomPlayerLevel.Value : 1;
                        userData.playerRating = ModConfig.CustomPlayerRating.Value > 0 ? ModConfig.CustomPlayerRating.Value : 0;
                        userData.highestRating = ModConfig.CustomHighestRating.Value > 0 ? ModConfig.CustomHighestRating.Value : 0;

                        user.Data.copyFrom(userData);
                        user.Data.IsDirty = false;
                        __instance.IsExistUser = true;
                        traverse.Field("state_").SetValue(PacketBase.State.Done);
                        __result = PacketBase.State.Done;
                        MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki user profile synthesized successfully.");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[CardMakerCustomizer] Failed to synthesize fallback Ongeki profile: {ex.Message}");
                }
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketPrinterLogin), "proc")]
    public static class Patch_PacketPrinterLogin_proc
    {
        public static void Postfix(CardMaker.MU3.PacketPrinterLogin __instance, ref PacketBase.State __result)
        {
            if (!ModConfig.UnlockAllCards.Value && !ModConfig.EnableProfileOverride.Value && (ModConfig.EnableOfflineBypass == null || !ModConfig.EnableOfflineBypass.Value))
            {
                return;
            }

            if (__result == PacketBase.State.Error || __instance.Result != 1)
            {
                try
                {
                    var traverse = Traverse.Create(__instance);
                    traverse.Field("result_").SetValue(1);
                    traverse.Field("state_").SetValue(PacketBase.State.Done);
                    __result = PacketBase.State.Done;
                    var user = traverse.Field("user_").GetValue<MU3User>();
                    if (user != null)
                    {
                        user.Data.updatePlace();
                    }
                    MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki PrinterLogin applied (Result=1).");
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[CardMakerCustomizer] Failed to apply fallback PrinterLogin: {ex.Message}");
                }
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketGetUserGacha), "proc")]
    public static class Patch_PacketGetUserGacha_proc
    {
        public static void Postfix(CardMaker.MU3.PacketGetUserGacha __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                var user = traverse.Field("user_").GetValue<MU3User>();
                if (user != null && user.UserGachaList == null)
                {
                    user.UserGachaList = new List<MU3UserGacha>();
                }
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki PacketGetUserGacha applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketGetUserItem), "proc")]
    public static class Patch_PacketGetUserItem_proc
    {
        public static void Postfix(CardMaker.MU3.PacketGetUserItem __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki PacketGetUserItem applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketGetUserGachaSupply), "proc")]
    public static class Patch_PacketGetUserGachaSupply_proc
    {
        public static void Postfix(CardMaker.MU3.PacketGetUserGachaSupply __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                var user = traverse.Field("user_").GetValue<MU3User>();
                var mu3Context = traverse.Field("mu3Context_").GetValue<MU3Context>();
                if (user != null)
                {
                    user.SupplyData = new MU3User.Supply(0, 0, new int[0]);
                }
                if (mu3Context != null)
                {
                    mu3Context.IsSupply = false;
                }
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki PacketGetUserGachaSupply applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketGetGacha), "proc")]
    public static class Patch_PacketGetGacha_proc
    {
        public static void Postfix(CardMaker.MU3.PacketGetGacha __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                var gachaContext = traverse.Field("gachaContext_").GetValue<MU3GachaContext>();
                if (gachaContext != null)
                {
                    gachaContext.set(new CardMaker.MU3.Gacha[0], new int[0]);
                }
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki PacketGetGacha applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketGetGameTheater), "proc")]
    public static class Patch_PacketGetGameTheater_proc
    {
        public static void Postfix(CardMaker.MU3.PacketGetGameTheater __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                var gachaContext = traverse.Field("gachaContext_").GetValue<MU3GachaContext>();
                if (gachaContext != null)
                {
                    gachaContext.set(new CardMaker.MU3.GameTheater[0], new int[0]);
                }
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki PacketGetGameTheater applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketGetGachaCard), "proc")]
    public static class Patch_PacketGetGachaCard_proc
    {
        public static void Postfix(CardMaker.MU3.PacketGetGachaCard __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                var gacha = traverse.Field("gacha_").GetValue<MU3Gacha>();
                if (gacha != null && gacha.Cards == null)
                {
                    gacha.Cards = new List<MU3GachaCard>();
                }
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki PacketGetGachaCard applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketUpsertUserPrintPlaylog), "proc")]
    public static class Patch_PacketUpsertUserPrintPlaylog_proc
    {
        public static void Postfix(CardMaker.MU3.PacketUpsertUserPrintPlaylog __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki PacketUpsertUserPrintPlaylog applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketUpsertUserPrint), "proc")]
    public static class Patch_PacketUpsertUserPrint_proc
    {
        public static void Postfix(CardMaker.MU3.PacketUpsertUserPrint __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                var query = traverse.Field("printQuery_").GetValue<MU3PrintQueue.MU3Query>();
                if (query != null)
                {
                    if (query.orderId_ == 0L)
                    {
                        query.orderId_ = DateTime.UtcNow.Ticks;
                    }
                    if (string.IsNullOrEmpty(query.serialId_))
                    {
                        query.serialId_ = "MU3-" + Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
                    }
                }
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki PacketUpsertUserPrint applied (generated offline print credentials).");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketUpsertUserAll), "proc")]
    public static class Patch_PacketUpsertUserAll_proc
    {
        private static readonly System.Reflection.MethodInfo ClearFlagsMethod =
            typeof(CardMaker.MU3.PacketUpsertUserAll).GetMethod("clearFlags", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        public static void Postfix(CardMaker.MU3.PacketUpsertUserAll __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                try
                {
                    if ((object)ClearFlagsMethod != null)
                    {
                        ClearFlagsMethod.Invoke(__instance, null);
                    }
                }
                catch
                {
                }
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki PacketUpsertUserAll applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketUpsertUserPrintlog), "proc")]
    public static class Patch_PacketUpsertUserPrintlog_proc
    {
        public static void Postfix(CardMaker.MU3.PacketUpsertUserPrintlog __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki PacketUpsertUserPrintlog applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketPrinterLogout), "proc")]
    public static class Patch_PacketPrinterLogout_proc
    {
        public static void Postfix(CardMaker.MU3.PacketPrinterLogout __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("result_").SetValue(1);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki PacketPrinterLogout applied.");
            }
        }
    }
}
