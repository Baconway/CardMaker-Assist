using System;
using System.Collections.Generic;
using HarmonyLib;
using CardMaker.Common;
using CardMaker.MU3;
using CardMaker.MU3.DataStudio;
using CardMaker.CHU;
using CardMaker.MAI;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.Logic;
using MelonLoader;

namespace CardMakerCustomizer.Patches
{
    // ==========================================
    // Ongeki (MU3) Card Unlock Patches
    // ==========================================

    [HarmonyPatch(typeof(MU3UserCard), "IsPrintable", MethodType.Getter)]
    public static class Patch_MU3UserCard_IsPrintable
    {
        public static bool Prefix(ref bool __result)
        {
            if (ModConfig.UnlockAllCards.Value || ModConfig.ForcePrintable.Value)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MU3UserCard), "IsExpired", MethodType.Getter)]
    public static class Patch_MU3UserCard_IsExpired
    {
        public static bool Prefix(ref bool __result)
        {
            if ((ModConfig.DisablePrintPeriodRestriction != null && ModConfig.DisablePrintPeriodRestriction.Value) ||
                (ModConfig.UnlockAllCards != null && ModConfig.UnlockAllCards.Value))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketGetUserCard), "proc")]
    public static class Patch_PacketGetUserCard_proc
    {
        public static void Postfix(CardMaker.MU3.PacketGetUserCard __instance, ref PacketBase.State __result)
        {
            if (!ModConfig.UnlockAllCards.Value && !ModConfig.LoadCustomOptCards.Value && (ModConfig.EnableOfflineBypass == null || !ModConfig.EnableOfflineBypass.Value))
            {
                return;
            }

            // Recover from server error if needed
            if (__result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki PacketGetUserCard applied (recovered from server error).");
            }

            if (__result != PacketBase.State.Done)
            {
                return;
            }

            try
            {
                var traverse = Traverse.Create(__instance);
                var user = traverse.Field("user_").GetValue<MU3User>();
                var cards = traverse.Field("cards_").GetValue<List<MU3UserCard>>();

                if (user == null)
                {
                    return;
                }

                if (cards == null)
                {
                    cards = new List<MU3UserCard>();
                    traverse.Field("cards_").SetValue(cards);
                }

                var dataManager = ContextBase<CommonContext>.Instance?.MU3DataManager;
                if (dataManager == null)
                {
                    return;
                }

                int printStock = ModConfig.UnlockedPrintCount.Value;
                var existingCardIds = new HashSet<int>();
                DateTime now = NetPacketUtil.LocalNow;

                // Ensure existing cards have max local/server stocks, awaken dates, and clear dirty flags
                for (int i = 0; i < cards.Count; i++)
                {
                    var card = cards[i];
                    if (card == null)
                    {
                        continue;
                    }

                    existingCardIds.Add(card.ID);
                    int maxOwn = CardUnlockHelper.GetMaxOwnCount(card.Rarity == RarityType.N);

                    if (card.PrintableCount < printStock)
                    {
                        card.addPrintableCount(printStock - card.PrintableCount);
                    }
                    if (card.LocalPrintableCount < printStock)
                    {
                        card.addLocalPrintableCount(printStock - card.LocalPrintableCount);
                    }

                    if (card.OwnCount < maxOwn)
                    {
                        card.addOwnCount(maxOwn - card.OwnCount);
                    }
                    if (card.LocalOwnCount < maxOwn)
                    {
                        card.addLocalOwnCount(maxOwn - card.LocalOwnCount);
                    }

                    if (card.PrintedCount < 1)
                    {
                        card.addPrintedCount(1);
                    }
                    if (card.LocalPrintedCount < 1)
                    {
                        card.addLocalPrintedCount(1);
                    }

                    if (card.KaikaDate <= DateTime.MinValue)
                    {
                        card.KaikaDate = now;
                    }
                    if (card.ChoKaikaDate <= DateTime.MinValue)
                    {
                        card.ChoKaikaDate = now;
                    }

                    // Clear dirty flags so PacketUpsertUserAll doesn't try to sync 2,000+ cards
                    card.clearFlags();
                }

                // Unlock all remaining master cards (or custom cards if UnlockAllCards is false)
                var enumerator = dataManager.getCardEnumerator();
                while (enumerator.MoveNext())
                {
                    int cardId = enumerator.Current.Key;
                    var cardData = enumerator.Current.Value;
                    bool shouldUnlock = ModConfig.UnlockAllCards.Value || CustomOptHelper.IsCustomCard(cardId);

                    if (cardData != null && !cardData.IsDigital && cardData.Name.IsValid() && !existingCardIds.Contains(cardId) && shouldUnlock)
                    {
                        MU3UserCard unlockedCard = new MU3UserCard(cardId);
                        if (!unlockedCard.HasValidData || !unlockedCard.CardData.Name.IsValid() || !unlockedCard.CharaData.Name.IsValid())
                        {
                            continue;
                        }

                        int maxOwn = CardUnlockHelper.GetMaxOwnCount(unlockedCard.Rarity == RarityType.N);
                        unlockedCard.addOwnCount(maxOwn);
                        unlockedCard.addLocalOwnCount(maxOwn);
                        unlockedCard.addPrintableCount(printStock);
                        unlockedCard.addLocalPrintableCount(printStock);
                        unlockedCard.addPrintedCount(1);
                        unlockedCard.addLocalPrintedCount(1);
                        unlockedCard.KaikaDate = now;
                        unlockedCard.ChoKaikaDate = now;
                        unlockedCard.clearFlags();

                        cards.Add(unlockedCard);
                        existingCardIds.Add(cardId);

                        if (CustomOptHelper.IsCustomCard(cardId))
                        {
                            CustomOptHelper.SafeLog($"[CardMakerCustomizer] [CustomOpt] [MU3] Unlocked custom card {cardId} ({unlockedCard.CardData.Name.str}) for user profile.");
                        }
                    }
                }

                user.Cards = cards;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed to unlock Ongeki cards: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketGetUserCharacter), "proc")]
    public static class Patch_PacketGetUserCharacter_proc_MU3
    {
        public static void Postfix(CardMaker.MU3.PacketGetUserCharacter __instance, ref PacketBase.State __result)
        {
            if (!ModConfig.UnlockAllCards.Value && !ModConfig.LoadCustomOptCards.Value && (ModConfig.EnableOfflineBypass == null || !ModConfig.EnableOfflineBypass.Value))
            {
                return;
            }

            // Recover from server error if needed
            if (__result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Ongeki PacketGetUserCharacter applied (recovered from server error).");
            }

            if (__result == PacketBase.State.Done)
            {
                try
                {
                    var user = Traverse.Create(__instance).Field("user_").GetValue<MU3User>();
                    if (user != null)
                    {
                        UnlockAllOngekiCharacters(user);
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[CardMakerCustomizer] Failed to unlock Ongeki characters: {ex.Message}");
                }
            }
        }

        public static void UnlockAllOngekiCharacters(MU3User user)
        {
            if (user == null || user.Cards == null)
            {
                return;
            }

            var characters = user.Characters;
            if (characters == null)
            {
                characters = new List<MU3UserCharacter>();
            }

            var existingCharaIds = new HashSet<int>();
            for (int i = 0; i < characters.Count; i++)
            {
                existingCharaIds.Add(characters[i].ID);
                characters[i].clearFlags();
            }

            var dataManager = ContextBase<CommonContext>.Instance?.MU3DataManager;
            if (dataManager != null)
            {
                for (int i = 0; i < user.Cards.Count; i++)
                {
                    var card = user.Cards[i];
                    if (card != null && card.CharaData != null && card.CharaData.GetID() > 0)
                    {
                        int charaId = card.CharaData.GetID();
                        if (!existingCharaIds.Contains(charaId) && dataManager.getChara(charaId) != null)
                        {
                            var chara = new MU3UserCharacter(charaId);
                            chara.clearFlags();
                            characters.Add(chara);
                            existingCharaIds.Add(charaId);
                        }
                    }
                }
            }

            user.Characters = characters;
        }
    }

    // ==========================================
    // CHUNITHM (CHU) Card Unlock Patches
    // ==========================================

    [HarmonyPatch(typeof(CHUUser), "hasUserCharaByCard", new Type[] { typeof(int) })]
    public static class Patch_CHUUser_hasUserCharaByCard
    {
        public static bool Prefix(int cardId, ref bool __result)
        {
            if (ModConfig.UnlockAllCards.Value || (ModConfig.LoadCustomOptCards.Value && CustomOptHelper.IsCustomCard(cardId)))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CHUUser), "isPrintedCard", new Type[] { typeof(int) })]
    public static class Patch_CHUUser_isPrintedCard
    {
        public static bool Prefix(int cardId, ref bool __result)
        {
            if (ModConfig.UnlockAllCards.Value || (ModConfig.LoadCustomOptCards.Value && CustomOptHelper.IsCustomCard(cardId)))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketGetUserCharacter), "proc")]
    public static class Patch_PacketGetUserCharacter_proc
    {
        public static void Postfix(CardMaker.CHU.PacketGetUserCharacter __instance, ref PacketBase.State __result)
        {
            // Recover from server error if needed
            if (__result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
                MelonLogger.Msg("[CardMakerCustomizer] Fallback Chunithm PacketGetUserCharacter applied (recovered from server error).");
            }

            if ((!ModConfig.UnlockAllCards.Value && !ModConfig.LoadCustomOptCards.Value && (ModConfig.EnableOfflineBypass == null || !ModConfig.EnableOfflineBypass.Value)) || __result != PacketBase.State.Done)
            {
                return;
            }

            try
            {
                var traverse = Traverse.Create(__instance);
                var user = traverse.Field("user_").GetValue<CHUUser>();
                var characters = traverse.Field("characters_").GetValue<List<CHUUserCharacter>>();

                if (user == null || characters == null)
                {
                    return;
                }

                var chuDataManager = ContextBase<CommonContext>.Instance?.CHUDataManager;
                if (chuDataManager == null)
                {
                    return;
                }

                var existingCharaIds = new HashSet<int>();
                for (int i = 0; i < characters.Count; i++)
                {
                    existingCharaIds.Add(characters[i].ID);
                }

                var enumerator = chuDataManager.getCardEnumerator();
                while (enumerator.MoveNext())
                {
                    int cardId = enumerator.Current.Key;
                    int charaId = chuDataManager.CardIdToCharaId(cardId);

                    if (charaId > 0 && !existingCharaIds.Contains(charaId))
                    {
                        if (ModConfig.UnlockAllCards.Value || CustomOptHelper.IsCustomCard(cardId))
                        {
                            characters.Add(new CHUUserCharacter((long)charaId));
                            existingCharaIds.Add(charaId);

                            if (CustomOptHelper.IsCustomCard(cardId))
                            {
                                CustomOptHelper.SafeLog($"[CardMakerCustomizer] [CustomOpt] [CHU] Unlocked character {charaId} for custom card {cardId}.");
                            }
                        }
                    }
                }

                user.Characters = characters;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed to unlock CHUNITHM characters: {ex.Message}");
            }
        }
    }

    // ==========================================
    // maimai DX (MAI) Card Unlock Patches
    // ==========================================

    [HarmonyPatch(typeof(MAISellingCardContext), "GetSellingCardAll")]
    public static class Patch_MAISellingCardContext_GetSellingCardAll
    {
        public static void Postfix(MAISellingCardContext __instance, ref List<MAISellingCard> __result)
        {
            if (ModConfig.UnlockAllCards.Value || ModConfig.LoadCustomOptCards.Value)
            {
                CustomOptHelper.InjectMaimaiSellingCards(__instance);
            }
        }
    }
}
