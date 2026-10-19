using System;

namespace CardMakerCustomizer.Logic
{
    public static class VersionHelper
    {
        public const int CHUNITHM_NEW_PLUS_MIN_VERSION = 2010000;
        public const int ONGEKI_SUPPORTED_VERSION = 1035000;

        public static int ToScalarCode(int major, int minor, int release)
        {
            return major * 1000000 + minor * 10000 + release * 100;
        }

        public static int ToCombinedVersion(int major, int minor)
        {
            return 1000 * major + minor;
        }

        public static int GetMaimaiBucket(int minor)
        {
            return (minor / 5) * 5;
        }

        public static bool IsMaimaiLoginBlocked(int userMajor, int userMinor, int cardmakerMajor, int cardmakerMinor, bool bypass)
        {
            if (bypass)
            {
                return false;
            }

            if (userMajor != cardmakerMajor)
            {
                return true;
            }

            return GetMaimaiBucket(userMinor) != GetMaimaiBucket(cardmakerMinor);
        }

        public static bool IsMaimaiSameVersionAsCardMaker(uint majorA, uint minorA, uint majorB, uint minorB, bool bypass)
        {
            if (bypass)
            {
                return true;
            }

            uint num = 1000U * majorA + minorA;
            uint num2 = 1000U * majorB + minorB;
            num /= 5U;
            num2 /= 5U;
            return num == num2;
        }

        public static bool IsChunithmVersionBlocked(int userVersion, int cardmakerVersion, bool bypass)
        {
            if (bypass)
            {
                return false;
            }

            if (userVersion < CHUNITHM_NEW_PLUS_MIN_VERSION)
            {
                return true;
            }

            return userVersion > cardmakerVersion;
        }

        public static bool ShouldEnableGacha(bool originalState, bool bypass)
        {
            if (bypass)
            {
                return true;
            }

            return originalState;
        }
    }
}
