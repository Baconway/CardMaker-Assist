using System;
using System.Collections.Generic;

namespace CardMakerCustomizer.Logic
{
    public class MU3UnlockedCardInfo
    {
        public int CardId { get; set; }
        public bool IsAcquired { get; set; }
        public int DigitalStock { get; set; }
        public int LocalDigitalStock { get; set; }
        public int PrintCount { get; set; }
        public int LocalPrintCount { get; set; }
        public int UseCount { get; set; }
        public int LocalUseCount { get; set; }
        public bool IsKaika { get; set; }
        public bool IsChoKaika { get; set; }
        public bool FlagsCleared { get; set; }
    }

    public static class CardUnlockHelper
    {
        public static int GetMaxOwnCount(bool isNormalRarity)
        {
            return isNormalRarity ? 11 : 5;
        }

        public static MU3UnlockedCardInfo CreateMU3UnlockedCard(int cardId, int printCount = 99, bool isNormalRarity = false)
        {
            int maxOwn = GetMaxOwnCount(isNormalRarity);
            return new MU3UnlockedCardInfo
            {
                CardId = cardId,
                IsAcquired = true,
                DigitalStock = maxOwn,
                LocalDigitalStock = maxOwn,
                PrintCount = printCount,
                LocalPrintCount = printCount,
                UseCount = 1,
                LocalUseCount = 1,
                IsKaika = true,
                IsChoKaika = true,
                FlagsCleared = true
            };
        }

        public static bool CanPrintMU3Card(bool unlockAll, bool originalPrintable)
        {
            if (unlockAll)
            {
                return true;
            }

            return originalPrintable;
        }

        public static bool HasChunithmCard(bool unlockAll, int cardId, bool originalHas)
        {
            if (unlockAll)
            {
                return true;
            }

            return originalHas;
        }

        public static List<int> BuildMaimaiSellingCardList(bool unlockAll, IEnumerable<int> masterCardIds, IEnumerable<int> ownedCardIds)
        {
            if (unlockAll)
            {
                return new List<int>(masterCardIds);
            }

            return new List<int>(ownedCardIds);
        }

        public static bool IsCardExpired(DateTime printDeadline, DateTime homeDate, bool disableRestriction, bool unlockAll = false)
        {
            if (disableRestriction || unlockAll)
            {
                return false;
            }

            return DateTime.MinValue.Date < printDeadline && printDeadline < homeDate;
        }

        public static bool ShouldDisablePrintPeriodRestriction(bool disableRestrictionConfig)
        {
            return disableRestrictionConfig;
        }
    }
}
