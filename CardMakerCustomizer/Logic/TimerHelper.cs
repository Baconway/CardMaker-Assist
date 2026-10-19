using System;

namespace CardMakerCustomizer.Logic
{
    public static class TimerHelper
    {
        public const float FrozenCounterValue = 999f;
        public const int FrozenCounterInt = 999;

        public static bool ShouldFreezeTimer(bool infiniteTimeEnabled)
        {
            return infiniteTimeEnabled;
        }

        public static float GetFrozenCounterValue(float originalCounter, bool infiniteTimeEnabled)
        {
            if (infiniteTimeEnabled)
            {
                return FrozenCounterValue;
            }

            return originalCounter;
        }

        public static bool IsTimeOutBypassed(bool infiniteTimeEnabled, bool originalIsTimeOut)
        {
            if (infiniteTimeEnabled)
            {
                return false;
            }

            return originalIsTimeOut;
        }

        public static bool ShouldSuppressWarningSound(bool infiniteTimeEnabled, float counter)
        {
            if (infiniteTimeEnabled)
            {
                return true;
            }

            return counter > 10f;
        }
    }
}
