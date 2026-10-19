using System;
using System.Collections.Generic;
using HarmonyLib;
using CardMaker.Common;
using CardMaker.CHU;
using CardMaker.CHU.ChuniStudio;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.Logic;
using MelonLoader;

namespace CardMakerCustomizer.Patches
{
    // ==========================================
    // Chunithm (CHU) Server Compatibility & Gacha/Print Fix Patches
    // ==========================================

    [HarmonyPatch(typeof(CardMaker.CHU.PacketUpsertUserGacha), "proc")]
    public static class Patch_CHU_PacketUpsertUserGacha_proc
    {
        private static readonly System.Reflection.MethodInfo ClearChangeByUpdateItemsMethod =
            typeof(CardMaker.CHU.PacketUpsertUserGacha).GetMethod("clearChangeByUpdateItems", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        public static void Postfix(CardMaker.CHU.PacketUpsertUserGacha __instance, ref PacketBase.State __result)
        {
            // 1. Recover from server error (e.g. HTTP 500 on CMUpsertUserGachaApi)
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;

                try
                {
                    if ((object)ClearChangeByUpdateItemsMethod != null)
                    {
                        ClearChangeByUpdateItemsMethod.Invoke(__instance, null);
                    }
                }
                catch
                {
                }

                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketUpsertUserGacha applied (recovered from server error).");
            }

            // 2. Ensure GachaRollInfo has valid UserCardPrintState for every rolled card
            try
            {
                if (CHUContext.Instance != null && CHUContext.Instance.User != null && CHUContext.Instance.User.GachaRollInfo != null)
                {
                    var gachaRollInfo = CHUContext.Instance.User.GachaRollInfo;
                    if (gachaRollInfo.NumCards > 0)
                    {
                        var existingStates = Traverse.Create(gachaRollInfo).Field("printStateList_").GetValue<UserCardPrintState[]>();
                        if (existingStates == null || existingStates.Length != gachaRollInfo.NumCards)
                        {
                            int placeId = 1;
                            try
                            {
                                var upsertQuery = Traverse.Create(__instance).Field("query_").GetValue<UpsertUserGacha>();
                                if (upsertQuery != null && upsertQuery.request_.placeId > 0)
                                {
                                    placeId = upsertQuery.request_.placeId;
                                }
                            }
                            catch
                            {
                            }

                            DateTime now = DateTime.Now;
                            try
                            {
                                now = NetPacketUtil.LocalNow;
                            }
                            catch
                            {
                            }

                            string limitDate = now.AddDays(30).ToString("yyyy-MM-dd HH:mm:ss");
                            try
                            {
                                limitDate = NetPacketUtil.toLogDateString(now.AddDays(30), 7);
                            }
                            catch
                            {
                            }

                            var states = new UserCardPrintState[gachaRollInfo.NumCards];
                            for (int i = 0; i < gachaRollInfo.NumCards; i++)
                            {
                                var cardLog = gachaRollInfo.getCardLog(i);
                                states[i] = new UserCardPrintState
                                {
                                    orderId = i + 1,
                                    hasCompleted = false,
                                    limitDate = limitDate,
                                    placeId = placeId,
                                    cardId = cardLog.CardId,
                                    gachaId = gachaRollInfo.GachaId
                                };
                            }

                            gachaRollInfo.setPrintState(states);
                            MelonLogger.Msg($"[CardMakerCustomizer] Fallback Chunithm print states synthesized for {gachaRollInfo.NumCards} cards.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Error ensuring Chunithm print states: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketUpsertUserPrint), "proc")]
    public static class Patch_CHU_PacketUpsertUserPrint_proc
    {
        public static void Postfix(CardMaker.CHU.PacketUpsertUserPrint __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;

                try
                {
                    var query = traverse.Field("printQuery_").GetValue<CHUPrintQueue.CHUQuery>();
                    if (query != null)
                    {
                        if (query.OrderId == 0L)
                        {
                            query.OrderId = DateTime.UtcNow.Ticks;
                        }
                        if (string.IsNullOrEmpty(query.SerialId))
                        {
                            query.SerialId = "CHU-" + Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[CardMakerCustomizer] Failed to set credentials on CHU printQuery: {ex.Message}");
                }

                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketUpsertUserPrint applied (generated offline print credentials).");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketUpsertUserPrintCancel), "proc")]
    public static class Patch_CHU_PacketUpsertUserPrintCancel_proc
    {
        public static void Postfix(CardMaker.CHU.PacketUpsertUserPrintCancel __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketUpsertUserPrintCancel applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketUpsertUserPrintSubtract), "proc")]
    public static class Patch_CHU_PacketUpsertUserPrintSubtract_proc
    {
        private static readonly System.Reflection.MethodInfo ClearChangedMarkMethod =
            typeof(CardMaker.CHU.PacketUpsertUserPrintSubtract).GetMethod("clearChangedMark", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        public static void Postfix(CardMaker.CHU.PacketUpsertUserPrintSubtract __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;

                try
                {
                    if ((object)ClearChangedMarkMethod != null)
                    {
                        ClearChangedMarkMethod.Invoke(__instance, null);
                    }
                }
                catch
                {
                }

                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketUpsertUserPrintSubtract applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketUpsertUserPrintlog), "proc")]
    public static class Patch_CHU_PacketUpsertUserPrintlog_proc
    {
        public static void Postfix(CardMaker.CHU.PacketUpsertUserPrintlog __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketUpsertUserPrintlog applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketRollGacha), "proc")]
    public static class Patch_CHU_PacketRollGacha_proc
    {
        public static void Postfix(CardMaker.CHU.PacketRollGacha __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                try
                {
                    var traverse = Traverse.Create(__instance);
                    var query = traverse.Field("query_").GetValue<RollGacha>();
                    int gachaId = query != null ? query.request_.gachaId : 0;
                    int times = query != null ? query.request_.times : 1;
                    if (times <= 0)
                    {
                        times = 1;
                    }

                    var gachaData = ContextBase<CommonContext>.Instance?.CHUDataManager?.getGacha(gachaId);
                    List<GameGachaCard> cards = new List<GameGachaCard>();
                    if (gachaData != null && gachaData.infos != null && gachaData.infos.Count > 0)
                    {
                        var rand = new Random();
                        for (int i = 0; i < times; i++)
                        {
                            var info = gachaData.infos[rand.Next(gachaData.infos.Count)];
                            int cardId = info.cardName.id;
                            int rarity = 1;
                            try
                            {
                                var cardData = ContextBase<CommonContext>.Instance?.CHUDataManager?.getCard(cardId);
                                if (cardData != null)
                                {
                                    rarity = (int)cardData.rareType;
                                }
                            }
                            catch
                            {
                            }

                            cards.Add(new GameGachaCard
                            {
                                gachaId = gachaId,
                                cardId = cardId,
                                rarity = rarity,
                                weight = 1,
                                isPickup = false
                            });
                        }
                    }

                    if (cards.Count > 0)
                    {
                        traverse.Field("state_").SetValue(PacketBase.State.Done);
                        __result = PacketBase.State.Done;
                        var gachaCards = PacketRollGacha.NormalizeRollGachaCard(cards.ToArray());
                        CHUContext.Instance?.User?.GachaRollInfo?.setGachaCard(gachaCards);
                        MelonLogger.Msg($"[CardMakerCustomizer] Fallback Chunithm PacketRollGacha applied (synthesized {cards.Count} cards).");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[CardMakerCustomizer] Failed to apply fallback for PacketRollGacha: {ex.Message}");
                }
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketPrinterLogin), "proc")]
    public static class Patch_CHU_PacketPrinterLogin_proc
    {
        public static void Postfix(CardMaker.CHU.PacketPrinterLogin __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("result_").SetValue(1);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketPrinterLogin applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketPrinterLogout), "proc")]
    public static class Patch_CHU_PacketPrinterLogout_proc
    {
        public static void Postfix(CardMaker.CHU.PacketPrinterLogout __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("result_").SetValue(1);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketPrinterLogout applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketGetGameGachaCard), "proc")]
    public static class Patch_CHU_PacketGetGameGachaCard_proc
    {
        public static void Postfix(CardMaker.CHU.PacketGetGameGachaCard __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                var gacha = traverse.Field("gacha_").GetValue<CHUGameGacha>();
                if (gacha != null && gacha.Cards == null)
                {
                    gacha.Cards = new List<CHUGameGachaCard>();
                }
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketGetGameGachaCard applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketGetUserCardPrintError), "proc")]
    public static class Patch_CHU_PacketGetUserCardPrintError_proc
    {
        public static void Postfix(CardMaker.CHU.PacketGetUserCardPrintError __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                try
                {
                    if (CHUContext.Instance != null && CHUContext.Instance.User != null && CHUContext.Instance.User.RecoveryPrintInfo != null)
                    {
                        CHUContext.Instance.User.RecoveryPrintInfo.setPrintState(new UserCardPrintState[0]);
                    }
                }
                catch
                {
                }
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketGetUserCardPrintError applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketGetUserGacha), "proc")]
    public static class Patch_CHU_PacketGetUserGacha_proc
    {
        public static void Postfix(CardMaker.CHU.PacketGetUserGacha __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                try
                {
                    var user = traverse.Field("user_").GetValue<CHUUser>();
                    if (user != null)
                    {
                        user.setUserGachaList(new List<CHUUserGacha>());
                    }
                }
                catch
                {
                }
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketGetUserGacha applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketGetUserPrintedCard), "proc")]
    public static class Patch_CHU_PacketGetUserPrintedCard_proc
    {
        public static void Postfix(CardMaker.CHU.PacketGetUserPrintedCard __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                try
                {
                    var user = traverse.Field("user_").GetValue<CHUUser>();
                    if (user != null)
                    {
                        user.SetHasPrintedCard(new List<int>());
                    }
                }
                catch
                {
                }
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketGetUserPrintedCard applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketGetUserItem), "proc")]
    public static class Patch_CHU_PacketGetUserItem_proc
    {
        public static void Postfix(CardMaker.CHU.PacketGetUserItem __instance, ref PacketBase.State __result)
        {
            if (__result == PacketBase.State.Error)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                try
                {
                    var user = traverse.Field("user_").GetValue<CHUUser>();
                    if (user != null && user.HasItemList == null)
                    {
                        user.HasItemList = new List<CHUUserItem>();
                    }
                }
                catch
                {
                }
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketGetUserItem applied.");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketGetUserData), "proc")]
    public static class Patch_CHU_PacketGetUserData_proc
    {
        public static void Postfix(CardMaker.CHU.PacketGetUserData __instance, ref PacketBase.State __result)
        {
            if (!ModConfig.UnlockAllCards.Value && !ModConfig.EnableProfileOverride.Value && (ModConfig.EnableOfflineBypass == null || !ModConfig.EnableOfflineBypass.Value))
            {
                return;
            }

            if (__result == PacketBase.State.Error || !__instance.IsExistUser)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                __instance.IsExistUser = true;

                try
                {
                    var user = traverse.Field("user_").GetValue<CHUUser>();
                    if (user != null)
                    {
                        string userName = ModConfig.CustomUserName.Value;
                        if (string.IsNullOrEmpty(userName))
                        {
                            userName = "CHUNITHM";
                        }
                        var d = new UserData
                        {
                            userName = userName,
                            level = 100,
                            reincarnationNum = 1,
                            point = 999999
                        };
                        user.Data.copyFrom(d);
                        if (user.UserEmoney == null)
                        {
                            user.UserEmoney = new UserEmoney();
                        }
                    }
                }
                catch
                {
                }

                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketGetUserData applied (synthesized profile).");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketGetUserPreview), "proc")]
    public static class Patch_CHU_PacketGetUserPreview_proc
    {
        public static void Postfix(CardMaker.CHU.PacketGetUserPreview __instance, ref PacketBase.State __result)
        {
            if (!ModConfig.UnlockAllCards.Value && !ModConfig.EnableProfileOverride.Value && (ModConfig.EnableOfflineBypass == null || !ModConfig.EnableOfflineBypass.Value))
            {
                return;
            }

            if (__result == PacketBase.State.Error || !__instance.IsExistUser)
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                __instance.IsExistUser = true;

                try
                {
                    var user = traverse.Field("user_").GetValue<CHUUser>();
                    if (user != null && user.Preview != null)
                    {
                        string userName = ModConfig.CustomUserName.Value;
                        if (string.IsNullOrEmpty(userName))
                        {
                            userName = "CHUNITHM";
                        }
                        var pResp = new GetUserPreviewResponse
                        {
                            userName = userName,
                            level = 100,
                            medal = 0,
                            isLogin = false
                        };
                        user.Preview.copyFrom(pResp);
                    }
                }
                catch
                {
                }

                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketGetUserPreview applied (synthesized preview).");
            }
        }
    }
}
