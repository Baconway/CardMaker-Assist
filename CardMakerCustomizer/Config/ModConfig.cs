using System;
using MelonLoader;
using CardMakerCustomizer.Logic;

namespace CardMakerCustomizer.Config
{
    public static class ModConfig
    {
        // Category 1: Version Bypass
        public static MelonPreferences_Category CategoryVersion;
        public static MelonPreferences_Entry<bool> BypassAllVersionChecks;
        public static MelonPreferences_Entry<bool> BypassServerGachaCheck;
        public static MelonPreferences_Entry<bool> BypassMaimaiBucketLock;
        public static MelonPreferences_Entry<bool> BypassChunithmVersionLock;
        public static MelonPreferences_Entry<bool> BypassOngekiVersionCheck;

        // Category 2: Card Unlock
        public static MelonPreferences_Category CategoryCards;
        public static MelonPreferences_Entry<bool> UnlockAllCards;
        public static MelonPreferences_Entry<int> UnlockedPrintCount;
        public static MelonPreferences_Entry<bool> ForcePrintable;
        public static MelonPreferences_Entry<bool> DisablePrintPeriodRestriction;
        public static MelonPreferences_Entry<bool> LoadCustomOptCards;

        // Category 3: User Profile
        public static MelonPreferences_Category CategoryProfile;
        public static MelonPreferences_Entry<bool> EnableProfileOverride;
        public static MelonPreferences_Entry<string> CustomUserName;
        public static MelonPreferences_Entry<int> CustomPlayerRating;
        public static MelonPreferences_Entry<int> CustomHighestRating;
        public static MelonPreferences_Entry<int> CustomPlayerLevel;
        public static MelonPreferences_Entry<string> CustomFriendCode;

        // Category 4: QR Code
        public static MelonPreferences_Category CategoryQR;
        public static MelonPreferences_Entry<bool> EnableQROverride;
        public static MelonPreferences_Entry<bool> UseCustomQRText;
        public static MelonPreferences_Entry<string> CustomQRText;
        public static MelonPreferences_Entry<int> CustomSerial;

        // Category 5: Timers & Gameplay
        public static MelonPreferences_Category CategoryTimer;
        public static MelonPreferences_Entry<bool> InfiniteTime;

        // Category 6: Credit & Economy
        public static MelonPreferences_Category CategoryCredit;
        public static MelonPreferences_Entry<bool> InfiniteCredits;
        public static MelonPreferences_Entry<int> InfiniteCreditAmount;

        // Category 7: Network & Encryption
        public static MelonPreferences_Category CategoryNetwork;
        public static MelonPreferences_Entry<bool> DisableEncryption;
        public static MelonPreferences_Entry<bool> DisableTLS;
        public static MelonPreferences_Entry<bool> EnableNetworkLogging;
        public static MelonPreferences_Entry<bool> VerboseNetworkLogging;
        public static MelonPreferences_Entry<bool> EnableOfflineBypass;

        // Category 8: Holographic Patterns
        public static MelonPreferences_Category CategoryHolo;
        public static MelonPreferences_Entry<bool> EnableHoloCustomization;
        public static MelonPreferences_Entry<bool> ForceEnableHolo;
        public static MelonPreferences_Entry<float> HoloAlpha;
        public static MelonPreferences_Entry<float> HoloPreviewIntensity;
        public static MelonPreferences_Entry<float> HoloFoilHeat;
        public static MelonPreferences_Entry<bool> InvertHoloMask;

        // Category 9: Maimai Card Customization
        public static MelonPreferences_Category CategoryMaimaiCard;
        public static MelonPreferences_Entry<bool> EnableMaimaiCardCustomText;
        public static MelonPreferences_Entry<bool> UseCustomMaimaiCardVersionText;
        public static MelonPreferences_Entry<string> CustomMaimaiCardVersionText;
        public static MelonPreferences_Entry<bool> HideMaimaiCardVersionText;

        public static MelonPreferences_Entry<bool> UseCustomMaimaiCardFriendCode;
        public static MelonPreferences_Entry<string> CustomMaimaiCardFriendCode;
        public static MelonPreferences_Entry<bool> ForceShowMaimaiCardFriendCode;
        public static MelonPreferences_Entry<bool> HideMaimaiCardFriendCode;

        public static MelonPreferences_Entry<bool> UseCustomMaimaiCardSerial;
        public static MelonPreferences_Entry<string> CustomMaimaiCardSerial;
        public static MelonPreferences_Entry<bool> HideMaimaiCardSerial;

        public static MelonPreferences_Entry<bool> UseCustomMaimaiCardEndDate;
        public static MelonPreferences_Entry<string> CustomMaimaiCardEndDate;
        public static MelonPreferences_Entry<bool> HideMaimaiCardEndDate;

        // Category 10: Asset Bundle Logging
        public static MelonPreferences_Category CategoryAssetBundle;
        public static MelonPreferences_Entry<bool> EnableAssetBundleLogging;
        public static MelonPreferences_Entry<bool> VerboseAssetBundleLogging;

        public static void Initialize()
        {
            try
            {
                // Version Bypass Category
                CategoryVersion = MelonPreferences.CreateCategory("CardMaker_VersionBypass", "Version Bypass Settings");
                BypassAllVersionChecks = CategoryVersion.CreateEntry("BypassAllVersionChecks", true, "Bypass All Version Checks", "Master switch to disable all client and server version gating");
                BypassServerGachaCheck = CategoryVersion.CreateEntry("BypassServerGachaCheck", true, "Bypass Server Gacha Check", "Bypasses server-level isGachaEnable permission check");
                BypassMaimaiBucketLock = CategoryVersion.CreateEntry("BypassMaimaiBucketLock", true, "Bypass Maimai Bucket Lock", "Bypasses the 5-minor version bucket restriction for maimai DX");
                BypassChunithmVersionLock = CategoryVersion.CreateEntry("BypassChunithmVersionLock", true, "Bypass CHUNITHM Version Lock", "Bypasses CHUNITHM version check and preview user locks");
                BypassOngekiVersionCheck = CategoryVersion.CreateEntry("BypassOngekiVersionCheck", true, "Bypass Ongeki Version Check", "Bypasses Ongeki version synchronization checks");

                // Card Unlock Category
                CategoryCards = MelonPreferences.CreateCategory("CardMaker_CardUnlock", "Card Unlock Settings");
                UnlockAllCards = CategoryCards.CreateEntry("UnlockAllCards", true, "Unlock All Cards", "Unlocks every card for printing across Ongeki, Chunithm, and maimai DX");
                UnlockedPrintCount = CategoryCards.CreateEntry("UnlockedPrintCount", GUIStateHelper.DefaultPrintStock, "Unlocked Print Count", "Default print stock count for unlocked cards");
                ForcePrintable = CategoryCards.CreateEntry("ForcePrintable", true, "Force Printable", "Forces IsPrintable property to return true for all cards");
                DisablePrintPeriodRestriction = CategoryCards.CreateEntry("DisablePrintPeriodRestriction", true, "Disable Printing Period Restrictions", "Allows printing Ongeki cards even if their printing period has ended");
                LoadCustomOptCards = CategoryCards.CreateEntry("LoadCustomOptCards", true, "Load Custom Option Cards", "Loads custom cards, passes, and assets from Axxx option update folders (e.g. A900)");

                // User Profile Category (Enabled by default with placeholders)
                CategoryProfile = MelonPreferences.CreateCategory("CardMaker_UserProfile", "User Profile Settings");
                EnableProfileOverride = CategoryProfile.CreateEntry("EnableProfileOverride", true, "Enable Profile Override", "Enable overriding player profile attributes");
                CustomUserName = CategoryProfile.CreateEntry("CustomUserName", GUIStateHelper.DefaultUserName, "Custom User Name", "Override user name (defaults to placeholder)");
                CustomPlayerRating = CategoryProfile.CreateEntry("CustomPlayerRating", GUIStateHelper.DefaultRating, "Custom Player Rating", "Override player rating");
                CustomHighestRating = CategoryProfile.CreateEntry("CustomHighestRating", GUIStateHelper.DefaultHighestRating, "Custom Highest Rating", "Override highest rating");
                CustomPlayerLevel = CategoryProfile.CreateEntry("CustomPlayerLevel", GUIStateHelper.DefaultLevel, "Custom Player Level", "Override player level");
                CustomFriendCode = CategoryProfile.CreateEntry("CustomFriendCode", GUIStateHelper.DefaultFriendCode, "Custom Friend Code", "Override friend code");

                // QR Code Category (Enabled by default with placeholders)
                CategoryQR = MelonPreferences.CreateCategory("CardMaker_QRCode", "QR Code Settings");
                EnableQROverride = CategoryQR.CreateEntry("EnableQROverride", true, "Enable QR Override", "Enable custom QR payload generation");
                UseCustomQRText = CategoryQR.CreateEntry("UseCustomQRText", true, "Use Custom QR Text", "Render custom text or URL into QR code instead of binary card data");
                CustomQRText = CategoryQR.CreateEntry("CustomQRText", GUIStateHelper.DefaultQRText, "Custom QR Text", "Text or URL to encode into QR code");
                CustomSerial = CategoryQR.CreateEntry("CustomSerial", GUIStateHelper.DefaultSerial, "Custom Serial", "Custom serial number for binary QR payload");

                // Timer Category (Enabled by default)
                CategoryTimer = MelonPreferences.CreateCategory("CardMaker_Timer", "Timer & Gameplay Settings");
                InfiniteTime = CategoryTimer.CreateEntry("InfiniteTime", true, "Infinite Time", "Freeze countdown timers across all scenes to prevent timeouts");

                // Credit Category (Enabled by default)
                CategoryCredit = MelonPreferences.CreateCategory("CardMaker_Credit", "Credit & Economy Settings");
                InfiniteCredits = CategoryCredit.CreateEntry("InfiniteCredits", true, "Infinite Credits", "Enable unlimited credits for printing and gacha across Ongeki, Chunithm, and maimai DX");
                InfiniteCreditAmount = CategoryCredit.CreateEntry("InfiniteCreditAmount", GUIStateHelper.DefaultCreditAmount, "Display Credit Amount", "Number of credits to display on the HUD counter when infinite credits is enabled");

                // Network & Encryption Category (Disable encryption enabled by default)
                CategoryNetwork = MelonPreferences.CreateCategory("CardMaker_Network", "Network & Encryption Settings");
                DisableEncryption = CategoryNetwork.CreateEntry("DisableEncryption", true, "Disable Encryption", "Disable AES packet encryption and URL scrambling for private servers");
                DisableTLS = CategoryNetwork.CreateEntry("DisableTLS", false, "Disable TLS", "Force plain HTTP instead of TLS/HTTPS connections");
                EnableNetworkLogging = CategoryNetwork.CreateEntry("EnableNetworkLogging", GUIStateHelper.DefaultEnableNetworkLogging, "Enable Network Logging", "Detailed logging for outgoing network requests, server responses, and error diagnostics");
                VerboseNetworkLogging = CategoryNetwork.CreateEntry("VerboseNetworkLogging", GUIStateHelper.DefaultVerboseNetworkLogging, "Verbose Network Logging", "Logs detailed request payloads and server response bodies");
                EnableOfflineBypass = CategoryNetwork.CreateEntry("EnableOfflineBypass", GUIStateHelper.DefaultEnableOfflineBypass, "Enable Offline Bypass", "Bypass server network connectivity checks, allowing title selection and card printing without server connection");

                // Holographic Patterns Category
                CategoryHolo = MelonPreferences.CreateCategory("CardMaker_Holo", "Holographic Pattern Settings");
                EnableHoloCustomization = CategoryHolo.CreateEntry("EnableHoloCustomization", GUIStateHelper.DefaultEnableHolo, "Enable Holo Customization", "Master switch for custom holographic pattern rendering and foil control");
                ForceEnableHolo = CategoryHolo.CreateEntry("ForceEnableHolo", GUIStateHelper.DefaultForceHolo, "Force Holo On All Cards", "Forces holographic effects and thermal foil printing for all cards regardless of rarity");
                HoloAlpha = CategoryHolo.CreateEntry("HoloAlpha", GUIStateHelper.DefaultHoloAlpha, "Holo Mask Alpha", "Alpha opacity multiplier for holographic mask texture (0.0 to 1.0)");
                HoloPreviewIntensity = CategoryHolo.CreateEntry("HoloPreviewIntensity", GUIStateHelper.DefaultHoloPreviewIntensity, "Holo Preview Intensity", "Brightness/intensity of holographic shaders in on-screen UI preview (0.0 to 1.0)");
                HoloFoilHeat = CategoryHolo.CreateEntry("HoloFoilHeat", GUIStateHelper.DefaultHoloFoilHeat, "Holo Foil Print Heat", "Thermal transfer ribbon printhead heat multiplier for physical holographic foil layer (0.0 to 1.0)");
                InvertHoloMask = CategoryHolo.CreateEntry("InvertHoloMask", GUIStateHelper.DefaultInvertHolo, "Invert Holo Mask", "Invert holographic mask pattern to foil background instead of character outline");

                // Maimai Card Customization Category
                CategoryMaimaiCard = MelonPreferences.CreateCategory("CardMaker_MaimaiCard", "Maimai Card Customization Settings");
                EnableMaimaiCardCustomText = CategoryMaimaiCard.CreateEntry("EnableMaimaiCardCustomText", GUIStateHelper.DefaultEnableMaimaiCustom, "Enable Maimai Card Customization", "Master toggle for customizing maimai DX card footer, version, friend code, and serial text");
                UseCustomMaimaiCardVersionText = CategoryMaimaiCard.CreateEntry("UseCustomMaimaiCardVersionText", GUIStateHelper.DefaultUseCustomMaimaiVersion, "Use Custom Card Version Text", "Override the bottom version string with a custom template");
                CustomMaimaiCardVersionText = CategoryMaimaiCard.CreateEntry("CustomMaimaiCardVersionText", GUIStateHelper.DefaultMaimaiVersionTemplate, "Custom Version Template", "Template for bottom version text. Supports {version}, {charaId}, {cardId}, {prefix}");
                HideMaimaiCardVersionText = CategoryMaimaiCard.CreateEntry("HideMaimaiCardVersionText", GUIStateHelper.DefaultHideMaimaiVersion, "Hide Bottom Version Text", "Hide the version and character ID text at the bottom of maimai DX cards");

                UseCustomMaimaiCardFriendCode = CategoryMaimaiCard.CreateEntry("UseCustomMaimaiCardFriendCode", GUIStateHelper.DefaultUseCustomMaimaiFriendCode, "Use Custom Friend Code", "Override friend code text displayed on maimai DX cards");
                CustomMaimaiCardFriendCode = CategoryMaimaiCard.CreateEntry("CustomMaimaiCardFriendCode", GUIStateHelper.DefaultMaimaiFriendCode, "Custom Card Friend Code", "Friend code text to render on maimai DX cards");
                ForceShowMaimaiCardFriendCode = CategoryMaimaiCard.CreateEntry("ForceShowMaimaiCardFriendCode", GUIStateHelper.DefaultForceShowMaimaiFriendCode, "Force Show Friend Code", "Always render friend code text even if player profile does not have a friend code");
                HideMaimaiCardFriendCode = CategoryMaimaiCard.CreateEntry("HideMaimaiCardFriendCode", GUIStateHelper.DefaultHideMaimaiFriendCode, "Hide Friend Code", "Hide the entire friend code element on maimai DX cards");

                UseCustomMaimaiCardSerial = CategoryMaimaiCard.CreateEntry("UseCustomMaimaiCardSerial", GUIStateHelper.DefaultUseCustomMaimaiSerial, "Use Custom Serial", "Override card serial number text on maimai DX cards");
                CustomMaimaiCardSerial = CategoryMaimaiCard.CreateEntry("CustomMaimaiCardSerial", GUIStateHelper.DefaultMaimaiSerial, "Custom Serial Text", "Serial number text to render on maimai DX cards");
                HideMaimaiCardSerial = CategoryMaimaiCard.CreateEntry("HideMaimaiCardSerial", GUIStateHelper.DefaultHideMaimaiSerial, "Hide Serial Number", "Hide the serial code text on maimai DX cards");

                UseCustomMaimaiCardEndDate = CategoryMaimaiCard.CreateEntry("UseCustomMaimaiCardEndDate", GUIStateHelper.DefaultUseCustomMaimaiEndDate, "Use Custom End Date", "Override the expiration / valid period date on maimai DX cards");
                CustomMaimaiCardEndDate = CategoryMaimaiCard.CreateEntry("CustomMaimaiCardEndDate", GUIStateHelper.DefaultMaimaiEndDate, "Custom End Date Text", "End date text to render on maimai DX cards");
                HideMaimaiCardEndDate = CategoryMaimaiCard.CreateEntry("HideMaimaiCardEndDate", GUIStateHelper.DefaultHideMaimaiEndDate, "Hide End Date", "Hide the expiration / valid period date on maimai DX cards");

                // AssetBundle Logging Category
                CategoryAssetBundle = MelonPreferences.CreateCategory("CardMaker_AssetBundle", "AssetBundle Logging Settings");
                EnableAssetBundleLogging = CategoryAssetBundle.CreateEntry("EnableAssetBundleLogging", GUIStateHelper.DefaultEnableAssetBundleLogging, "Enable AssetBundle Logging", "Detailed logging for when AssetBundles are loaded and diagnostics when they fail");
                VerboseAssetBundleLogging = CategoryAssetBundle.CreateEntry("VerboseAssetBundleLogging", GUIStateHelper.DefaultVerboseAssetBundleLogging, "Verbose Asset Logging", "Log every individual asset loaded from inside AssetBundles");

                MelonPreferences.Save();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[CardMakerCustomizer] Failed to initialize preferences: {ex.Message}");
            }
        }
    }
}
