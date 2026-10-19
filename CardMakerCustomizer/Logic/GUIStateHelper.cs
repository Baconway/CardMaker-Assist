using System;
using System.Globalization;

namespace CardMakerCustomizer.Logic
{
    public class GUIStateData
    {
        // Profile
        public bool EnableProfileOverride { get; set; }
        public string CustomUserName { get; set; }
        public int CustomPlayerRating { get; set; }
        public int CustomHighestRating { get; set; }
        public int CustomPlayerLevel { get; set; }
        public string CustomFriendCode { get; set; }

        // QR Code
        public bool EnableQROverride { get; set; }
        public bool UseCustomQRText { get; set; }
        public string CustomQRText { get; set; }
        public int CustomSerial { get; set; }

        // Holo Patterns
        public bool EnableHoloCustomization { get; set; }
        public bool ForceEnableHolo { get; set; }
        public float HoloAlpha { get; set; }
        public float HoloPreviewIntensity { get; set; }
        public float HoloFoilHeat { get; set; }
        public bool InvertHoloMask { get; set; }

        // Game & Timers
        public bool InfiniteTime { get; set; }
        public bool InfiniteCredits { get; set; }
        public int InfiniteCreditAmount { get; set; }
        public bool UnlockAllCards { get; set; }
        public int UnlockedPrintCount { get; set; }
        public bool DisablePrintPeriodRestriction { get; set; }
        public bool LoadCustomOptCards { get; set; }
        public bool BypassAllVersionChecks { get; set; }

        // Maimai Card Customization
        public bool EnableMaimaiCardCustomText { get; set; }
        public bool UseCustomMaimaiCardVersionText { get; set; }
        public string CustomMaimaiCardVersionText { get; set; }
        public bool HideMaimaiCardVersionText { get; set; }

        public bool UseCustomMaimaiCardFriendCode { get; set; }
        public string CustomMaimaiCardFriendCode { get; set; }
        public bool ForceShowMaimaiCardFriendCode { get; set; }
        public bool HideMaimaiCardFriendCode { get; set; }

        public bool UseCustomMaimaiCardSerial { get; set; }
        public string CustomMaimaiCardSerial { get; set; }
        public bool HideMaimaiCardSerial { get; set; }

        public bool UseCustomMaimaiCardEndDate { get; set; }
        public string CustomMaimaiCardEndDate { get; set; }
        public bool HideMaimaiCardEndDate { get; set; }

        // Network
        public bool DisableEncryption { get; set; }
        public bool DisableTLS { get; set; }
        public bool EnableNetworkLogging { get; set; }
        public bool VerboseNetworkLogging { get; set; }
        public bool EnableOfflineBypass { get; set; }

        // AssetBundle Logging
        public bool EnableAssetBundleLogging { get; set; }
        public bool VerboseAssetBundleLogging { get; set; }
    }

    public static class GUIStateHelper
    {
        public const string DefaultUserName = "CARDMAKER";
        public const int DefaultRating = 1600;
        public const int DefaultHighestRating = 1600;
        public const int DefaultLevel = 99;
        public const string DefaultFriendCode = "1000 2000 3000 4";
        public const string DefaultQRText = "https://github.com";
        public const int DefaultSerial = 12345;
        public const int DefaultPrintStock = 99;
        public const int DefaultCreditAmount = 99;

        // Holo Defaults
        public const bool DefaultEnableHolo = true;
        public const bool DefaultForceHolo = false;
        public const float DefaultHoloAlpha = 1.0f;
        public const float DefaultHoloPreviewIntensity = 1.0f;
        public const float DefaultHoloFoilHeat = 1.0f;
        public const bool DefaultInvertHolo = false;

        // Maimai Card Defaults
        public const bool DefaultEnableMaimaiCustom = true;
        public const bool DefaultUseCustomMaimaiVersion = false;
        public const string DefaultMaimaiVersionTemplate = "[maimaiDX]{version}-{charaId}";
        public const bool DefaultHideMaimaiVersion = false;

        public const bool DefaultUseCustomMaimaiFriendCode = false;
        public const string DefaultMaimaiFriendCode = "1000 2000 3000 4";
        public const bool DefaultForceShowMaimaiFriendCode = false;
        public const bool DefaultHideMaimaiFriendCode = false;

        public const bool DefaultUseCustomMaimaiSerial = false;
        public const string DefaultMaimaiSerial = "0123456789";
        public const bool DefaultHideMaimaiSerial = false;

        public const bool DefaultUseCustomMaimaiEndDate = false;
        public const string DefaultMaimaiEndDate = "2099/12/31";
        public const bool DefaultHideMaimaiEndDate = false;

        // Network Logging Defaults
        public const bool DefaultEnableNetworkLogging = true;
        public const bool DefaultVerboseNetworkLogging = false;

        // Offline Bypass Default
        public const bool DefaultEnableOfflineBypass = true;

        // AssetBundle Logging Defaults
        public const bool DefaultEnableAssetBundleLogging = true;
        public const bool DefaultVerboseAssetBundleLogging = false;

        public static GUIStateData GetDefaultGUIState()
        {
            return new GUIStateData
            {
                EnableProfileOverride = true,
                CustomUserName = DefaultUserName,
                CustomPlayerRating = DefaultRating,
                CustomHighestRating = DefaultHighestRating,
                CustomPlayerLevel = DefaultLevel,
                CustomFriendCode = DefaultFriendCode,

                EnableQROverride = true,
                UseCustomQRText = true,
                CustomQRText = DefaultQRText,
                CustomSerial = DefaultSerial,

                EnableHoloCustomization = DefaultEnableHolo,
                ForceEnableHolo = DefaultForceHolo,
                HoloAlpha = DefaultHoloAlpha,
                HoloPreviewIntensity = DefaultHoloPreviewIntensity,
                HoloFoilHeat = DefaultHoloFoilHeat,
                InvertHoloMask = DefaultInvertHolo,

                InfiniteTime = true,
                InfiniteCredits = true,
                InfiniteCreditAmount = DefaultCreditAmount,
                UnlockAllCards = true,
                UnlockedPrintCount = DefaultPrintStock,
                DisablePrintPeriodRestriction = true,
                LoadCustomOptCards = true,
                BypassAllVersionChecks = true,

                EnableMaimaiCardCustomText = DefaultEnableMaimaiCustom,
                UseCustomMaimaiCardVersionText = DefaultUseCustomMaimaiVersion,
                CustomMaimaiCardVersionText = DefaultMaimaiVersionTemplate,
                HideMaimaiCardVersionText = DefaultHideMaimaiVersion,

                UseCustomMaimaiCardFriendCode = DefaultUseCustomMaimaiFriendCode,
                CustomMaimaiCardFriendCode = DefaultMaimaiFriendCode,
                ForceShowMaimaiCardFriendCode = DefaultForceShowMaimaiFriendCode,
                HideMaimaiCardFriendCode = DefaultHideMaimaiFriendCode,

                UseCustomMaimaiCardSerial = DefaultUseCustomMaimaiSerial,
                CustomMaimaiCardSerial = DefaultMaimaiSerial,
                HideMaimaiCardSerial = DefaultHideMaimaiSerial,

                UseCustomMaimaiCardEndDate = DefaultUseCustomMaimaiEndDate,
                CustomMaimaiCardEndDate = DefaultMaimaiEndDate,
                HideMaimaiCardEndDate = DefaultHideMaimaiEndDate,

                DisableEncryption = true,
                DisableTLS = false,
                EnableNetworkLogging = DefaultEnableNetworkLogging,
                VerboseNetworkLogging = DefaultVerboseNetworkLogging,
                EnableOfflineBypass = DefaultEnableOfflineBypass,

                EnableAssetBundleLogging = DefaultEnableAssetBundleLogging,
                VerboseAssetBundleLogging = DefaultVerboseAssetBundleLogging
            };
        }

        public static int SafeParseInt(string input, int fallback)
        {
            if (StringCompat.IsNullOrWhiteSpace(input))
            {
                return fallback;
            }

            int val;
            if (int.TryParse(input.Trim(), out val))
            {
                return val;
            }

            return fallback;
        }

        public static float SafeParseFloat(string input, float fallback, float min = 0f, float max = 1f)
        {
            if (StringCompat.IsNullOrWhiteSpace(input))
            {
                return fallback;
            }

            string clean = input.Trim().Replace(',', '.');
            float val;
            if (float.TryParse(clean, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
            {
                if (val < min) return min;
                if (val > max) return max;
                return val;
            }

            return fallback;
        }

        public static string SanitizeFriendCode(string input)
        {
            if (StringCompat.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            return input.Trim();
        }
    }
}
