using System;
using MelonLoader;
using CardMaker.Common;
using CardMaker.Common.AM;
using CardMakerCustomizer.Config;

namespace CardMakerCustomizer.Logic
{
    public static class CreditHelper
    {
        public const int DefaultDisplayCredits = 99;

        public static bool IsInfiniteCreditsEnabled()
        {
            try
            {
                return ModConfig.InfiniteCredits != null && ModConfig.InfiniteCredits.Value;
            }
            catch
            {
                return true;
            }
        }

        public static int GetDisplayCreditCount()
        {
            try
            {
                return ModConfig.InfiniteCreditAmount != null ? ModConfig.InfiniteCreditAmount.Value : DefaultDisplayCredits;
            }
            catch
            {
                return DefaultDisplayCredits;
            }
        }

        public static bool CanAffordGameCost(bool infiniteEnabled, int count, int actualCredits)
        {
            if (count <= 0)
            {
                return true;
            }

            if (infiniteEnabled)
            {
                return true;
            }

            return actualCredits >= count;
        }

        public static int CalculatePayedCredits(int currentPayed, int gameCostMultiplier, int count)
        {
            if (count <= 0)
            {
                return currentPayed;
            }

            int multiplier = gameCostMultiplier > 0 ? gameCostMultiplier : 1;
            return currentPayed + (multiplier * count);
        }

        public static int GetEffectiveCreditCount(bool infiniteEnabled, int infiniteAmount, int actualCredits)
        {
            if (infiniteEnabled)
            {
                return infiniteAmount;
            }

            return actualCredits;
        }

        public static void ApplyLiveCreditUpdate()
        {
            try
            {
                if (ContextBase<CommonContext>.Exists)
                {
                    Credit credit = ContextBase<CommonContext>.Instance?.AMManager?.Credit;
                    if (credit != null)
                    {
                        credit.execute();
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed to update live credits: {ex.Message}");
            }
        }
    }
}
