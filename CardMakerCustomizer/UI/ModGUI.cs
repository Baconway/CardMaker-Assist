using System;
using UnityEngine;
using MelonLoader;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.Logic;

namespace CardMakerCustomizer.UI
{
    public static class ModGUI
    {
        public static bool IsOpen = false;
        private static Rect windowRect = new Rect(40, 40, 540, 580);
        private static int currentTab = 0;
        private static bool isInitialized = false;

        // Editable GUI State
        private static bool enableProfile;
        private static string nameStr = "";
        private static string ratingStr = "";
        private static string highestStr = "";
        private static string levelStr = "";
        private static string friendCodeStr = "";

        // Maimai Card State
        private static bool enableMaimaiCustom;
        private static bool useCustomMaimaiVersion;
        private static string maimaiVersionTemplateStr = "";
        private static bool hideMaimaiVersion;

        private static bool useCustomMaimaiFriendCode;
        private static string maimaiFriendCodeStr = "";
        private static bool forceShowMaimaiFriendCode;
        private static bool hideMaimaiFriendCode;

        private static bool useCustomMaimaiSerial;
        private static string maimaiSerialStr = "";
        private static bool hideMaimaiSerial;

        private static bool useCustomMaimaiEndDate;
        private static string maimaiEndDateStr = "";
        private static bool hideMaimaiEndDate;

        private static bool enableQR;
        private static bool useCustomText;
        private static string qrText = "";
        private static string serialStr = "";

        private static bool enableHolo;
        private static bool forceHolo;
        private static float holoAlpha = 1.0f;
        private static float holoPreviewIntensity = 1.0f;
        private static float holoFoilHeat = 1.0f;
        private static bool invertHolo;

        private static bool infiniteTime;
        private static bool infiniteCredits;
        private static string creditAmountStr = "";
        private static bool unlockAll;
        private static string printStockStr = "";
        private static bool disablePrintPeriod;
        private static bool loadCustomOptCards;
        private static bool bypassVersions;
        private static bool disableEncryption;
        private static bool disableTLS;
        private static bool enableNetworkLogging;
        private static bool enableAssetBundleLogging;
        private static bool enableOfflineBypass;

        private static string statusMessage = "Press ~ to show/hide this menu.";
        private static float statusTimer = 0f;

        public static void InitializeFromConfig()
        {
            try
            {
                enableProfile = ModConfig.EnableProfileOverride.Value;
                nameStr = ModConfig.CustomUserName.Value ?? GUIStateHelper.DefaultUserName;
                ratingStr = (ModConfig.CustomPlayerRating.Value >= 0 ? ModConfig.CustomPlayerRating.Value : GUIStateHelper.DefaultRating).ToString();
                highestStr = (ModConfig.CustomHighestRating.Value >= 0 ? ModConfig.CustomHighestRating.Value : GUIStateHelper.DefaultHighestRating).ToString();
                levelStr = (ModConfig.CustomPlayerLevel.Value >= 0 ? ModConfig.CustomPlayerLevel.Value : GUIStateHelper.DefaultLevel).ToString();
                friendCodeStr = ModConfig.CustomFriendCode.Value ?? GUIStateHelper.DefaultFriendCode;

                enableMaimaiCustom = ModConfig.EnableMaimaiCardCustomText.Value;
                useCustomMaimaiVersion = ModConfig.UseCustomMaimaiCardVersionText.Value;
                maimaiVersionTemplateStr = ModConfig.CustomMaimaiCardVersionText.Value ?? GUIStateHelper.DefaultMaimaiVersionTemplate;
                hideMaimaiVersion = ModConfig.HideMaimaiCardVersionText.Value;

                useCustomMaimaiFriendCode = ModConfig.UseCustomMaimaiCardFriendCode.Value;
                maimaiFriendCodeStr = ModConfig.CustomMaimaiCardFriendCode.Value ?? GUIStateHelper.DefaultMaimaiFriendCode;
                forceShowMaimaiFriendCode = ModConfig.ForceShowMaimaiCardFriendCode.Value;
                hideMaimaiFriendCode = ModConfig.HideMaimaiCardFriendCode.Value;

                useCustomMaimaiSerial = ModConfig.UseCustomMaimaiCardSerial.Value;
                maimaiSerialStr = ModConfig.CustomMaimaiCardSerial.Value ?? GUIStateHelper.DefaultMaimaiSerial;
                hideMaimaiSerial = ModConfig.HideMaimaiCardSerial.Value;

                useCustomMaimaiEndDate = ModConfig.UseCustomMaimaiCardEndDate.Value;
                maimaiEndDateStr = ModConfig.CustomMaimaiCardEndDate.Value ?? GUIStateHelper.DefaultMaimaiEndDate;
                hideMaimaiEndDate = ModConfig.HideMaimaiCardEndDate.Value;

                enableQR = ModConfig.EnableQROverride.Value;
                useCustomText = ModConfig.UseCustomQRText.Value;
                qrText = ModConfig.CustomQRText.Value ?? GUIStateHelper.DefaultQRText;
                serialStr = (ModConfig.CustomSerial.Value >= 0 ? ModConfig.CustomSerial.Value : GUIStateHelper.DefaultSerial).ToString();

                enableHolo = ModConfig.EnableHoloCustomization.Value;
                forceHolo = ModConfig.ForceEnableHolo.Value;
                holoAlpha = ModConfig.HoloAlpha.Value;
                holoPreviewIntensity = ModConfig.HoloPreviewIntensity.Value;
                holoFoilHeat = ModConfig.HoloFoilHeat.Value;
                invertHolo = ModConfig.InvertHoloMask.Value;

                infiniteTime = ModConfig.InfiniteTime.Value;
                infiniteCredits = ModConfig.InfiniteCredits.Value;
                creditAmountStr = ModConfig.InfiniteCreditAmount.Value.ToString();
                unlockAll = ModConfig.UnlockAllCards.Value;
                printStockStr = ModConfig.UnlockedPrintCount.Value.ToString();
                disablePrintPeriod = ModConfig.DisablePrintPeriodRestriction.Value;
                loadCustomOptCards = ModConfig.LoadCustomOptCards.Value;
                bypassVersions = ModConfig.BypassAllVersionChecks.Value;
                disableEncryption = ModConfig.DisableEncryption.Value;
                disableTLS = ModConfig.DisableTLS.Value;
                enableNetworkLogging = ModConfig.EnableNetworkLogging != null ? ModConfig.EnableNetworkLogging.Value : GUIStateHelper.DefaultEnableNetworkLogging;
                enableAssetBundleLogging = ModConfig.EnableAssetBundleLogging != null ? ModConfig.EnableAssetBundleLogging.Value : GUIStateHelper.DefaultEnableAssetBundleLogging;
                enableOfflineBypass = ModConfig.EnableOfflineBypass != null ? ModConfig.EnableOfflineBypass.Value : GUIStateHelper.DefaultEnableOfflineBypass;

                isInitialized = true;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Failed to initialize GUI state: {ex.Message}");
            }
        }

        public static void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                IsOpen = !IsOpen;
                if (IsOpen && !isInitialized)
                {
                    InitializeFromConfig();
                }
            }

            if (statusTimer > 0f)
            {
                statusTimer -= Time.deltaTime;
                if (statusTimer <= 0f)
                {
                    statusMessage = "Press ~ to show/hide this menu.";
                }
            }
        }

        public static void OnGUI()
        {
            if (!IsOpen)
            {
                return;
            }

            if (!isInitialized)
            {
                InitializeFromConfig();
            }

            Color prevBg = GUI.backgroundColor;
            Color prevColor = GUI.color;

            GUI.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 0.98f);
            GUI.color = Color.white;

            windowRect = GUICompat.Window(9999, windowRect, DrawWindow, "Card Maker Customizer v1.0.0");

            GUI.backgroundColor = prevBg;
            GUI.color = prevColor;
        }

        private static void DrawWindow(int id)
        {
            GUICompat.BeginVertical();

            // Navigation Tab Bar
            string[] tabs = new string[] { "Profile", "Maimai Card", "QR Code", "Holo Patterns", "Game & Timers" };
            currentTab = GUICompat.Toolbar(currentTab, tabs, 30);
            GUICompat.Space(10);

            if (currentTab == 0)
            {
                DrawProfileTab();
            }
            else if (currentTab == 1)
            {
                DrawMaimaiCardTab();
            }
            else if (currentTab == 2)
            {
                DrawQRTab();
            }
            else if (currentTab == 3)
            {
                DrawHoloTab();
            }
            else
            {
                DrawTimersTab();
            }

            GUICompat.FlexibleSpace();
            GUICompat.Space(10);

            // Action Buttons
            GUICompat.BeginHorizontal();
            if (GUICompat.Button("Save & Apply", 34))
            {
                SaveAndApply();
            }
            if (GUICompat.Button("Reset Defaults", 34))
            {
                ResetDefaults();
            }
            if (GUICompat.Button("Close (~)", 34))
            {
                IsOpen = false;
            }
            GUICompat.EndHorizontal();

            GUICompat.Space(6);
            GUICompat.Label(statusMessage);

            GUICompat.EndVertical();

            GUI.DragWindow(new Rect(0, 0, 10000, 25));
        }

        private static void DrawProfileTab()
        {
            enableProfile = GUICompat.Toggle(enableProfile, " Enable Player Profile Override");
            GUICompat.Space(8);

            GUICompat.BeginHorizontal();
            GUICompat.Label("User Name:", 130);
            nameStr = GUICompat.TextField(nameStr);
            GUICompat.EndHorizontal();

            GUICompat.BeginHorizontal();
            GUICompat.Label("Player Rating:", 130);
            ratingStr = GUICompat.TextField(ratingStr);
            GUICompat.EndHorizontal();

            GUICompat.BeginHorizontal();
            GUICompat.Label("Highest Rating:", 130);
            highestStr = GUICompat.TextField(highestStr);
            GUICompat.EndHorizontal();

            GUICompat.BeginHorizontal();
            GUICompat.Label("Player Level:", 130);
            levelStr = GUICompat.TextField(levelStr);
            GUICompat.EndHorizontal();

            GUICompat.BeginHorizontal();
            GUICompat.Label("Friend Code:", 130);
            friendCodeStr = GUICompat.TextField(friendCodeStr);
            GUICompat.EndHorizontal();

            GUICompat.Space(6);
            GUICompat.Label("Tip: Overrides apply to printed cards and in-game UI displays.");
        }

        private static void DrawMaimaiCardTab()
        {
            enableMaimaiCustom = GUICompat.Toggle(enableMaimaiCustom, " Enable Maimai DX Card Customization");
            GUICompat.Space(6);

            GUICompat.Label("<b>--- Bottom Version & Footer Text ---</b>");
            hideMaimaiVersion = GUICompat.Toggle(hideMaimaiVersion, " Hide Bottom Version Text ([maimaiDX]...)");
            if (!hideMaimaiVersion)
            {
                useCustomMaimaiVersion = GUICompat.Toggle(useCustomMaimaiVersion, " Override Version Text with Template");
                if (useCustomMaimaiVersion)
                {
                    GUICompat.BeginHorizontal();
                    GUICompat.Label("Version Template:", 130);
                    maimaiVersionTemplateStr = GUICompat.TextField(maimaiVersionTemplateStr);
                    GUICompat.EndHorizontal();
                    GUICompat.Label("Tokens: {version}, {charaId}, {rawCharaId}, {cardId}, {prefix}");
                }
            }
            GUICompat.Space(6);

            GUICompat.Label("<b>--- Friend Code ---</b>");
            hideMaimaiFriendCode = GUICompat.Toggle(hideMaimaiFriendCode, " Hide Friend Code from Card");
            if (!hideMaimaiFriendCode)
            {
                forceShowMaimaiFriendCode = GUICompat.Toggle(forceShowMaimaiFriendCode, " Force Show Friend Code (replace hyphen)");
                useCustomMaimaiFriendCode = GUICompat.Toggle(useCustomMaimaiFriendCode, " Override Friend Code on Card");
                if (useCustomMaimaiFriendCode)
                {
                    GUICompat.BeginHorizontal();
                    GUICompat.Label("Custom Friend Code:", 130);
                    maimaiFriendCodeStr = GUICompat.TextField(maimaiFriendCodeStr);
                    GUICompat.EndHorizontal();
                }
            }
            GUICompat.Space(6);

            GUICompat.Label("<b>--- Serial Number & End Date ---</b>");
            hideMaimaiSerial = GUICompat.Toggle(hideMaimaiSerial, " Hide Serial Number");
            if (!hideMaimaiSerial)
            {
                useCustomMaimaiSerial = GUICompat.Toggle(useCustomMaimaiSerial, " Override Serial Number");
                if (useCustomMaimaiSerial)
                {
                    GUICompat.BeginHorizontal();
                    GUICompat.Label("Custom Serial:", 130);
                    maimaiSerialStr = GUICompat.TextField(maimaiSerialStr);
                    GUICompat.EndHorizontal();
                }
            }
            GUICompat.Space(4);

            hideMaimaiEndDate = GUICompat.Toggle(hideMaimaiEndDate, " Hide Expiration / End Date");
            if (!hideMaimaiEndDate)
            {
                useCustomMaimaiEndDate = GUICompat.Toggle(useCustomMaimaiEndDate, " Override Expiration / End Date");
                if (useCustomMaimaiEndDate)
                {
                    GUICompat.BeginHorizontal();
                    GUICompat.Label("Custom End Date:", 130);
                    maimaiEndDateStr = GUICompat.TextField(maimaiEndDateStr);
                    GUICompat.EndHorizontal();
                }
            }
        }

        private static void DrawQRTab()
        {
            enableQR = GUICompat.Toggle(enableQR, " Enable Custom QR Code Override");
            GUICompat.Space(6);

            useCustomText = GUICompat.Toggle(useCustomText, " Render Custom URL / Text (instead of card binary)");
            GUICompat.Space(8);

            if (useCustomText)
            {
                GUICompat.BeginHorizontal();
                GUICompat.Label("Custom QR URL/Text:", 150);
                qrText = GUICompat.TextField(qrText);
                GUICompat.EndHorizontal();
                GUICompat.Label("Any string or URL (e.g. your portfolio, video, or social link).");
            }
            else
            {
                GUICompat.BeginHorizontal();
                GUICompat.Label("Custom Serial Number:", 150);
                serialStr = GUICompat.TextField(serialStr);
                GUICompat.EndHorizontal();
                GUICompat.Label("Injected into the encrypted 14-byte Sega card payload.");
            }
        }

        private static void DrawHoloTab()
        {
            enableHolo = GUICompat.Toggle(enableHolo, " Enable Holographic Customization");
            GUICompat.Space(6);

            forceHolo = GUICompat.Toggle(forceHolo, " Force Holo On All Cards (Normal / Rare / maimai)");
            GUICompat.Space(6);

            invertHolo = GUICompat.Toggle(invertHolo, " Invert Holo Mask (Background foil vs Character)");
            GUICompat.Space(10);

            // Holo Alpha Slider
            GUICompat.BeginHorizontal();
            GUICompat.Label(string.Format("Mask Alpha: {0:F2}", holoAlpha), 170);
            holoAlpha = GUICompat.HorizontalSlider(holoAlpha, 0f, 1f);
            GUICompat.EndHorizontal();

            // Preview Intensity Slider
            GUICompat.BeginHorizontal();
            GUICompat.Label(string.Format("Screen Shimmer: {0:F2}", holoPreviewIntensity), 170);
            holoPreviewIntensity = GUICompat.HorizontalSlider(holoPreviewIntensity, 0f, 1f);
            GUICompat.EndHorizontal();

            // Print Foil Heat Slider
            GUICompat.BeginHorizontal();
            GUICompat.Label(string.Format("Printer Foil Heat: {0:F2}", holoFoilHeat), 170);
            holoFoilHeat = GUICompat.HorizontalSlider(holoFoilHeat, 0f, 1f);
            GUICompat.EndHorizontal();

            GUICompat.Space(10);
            GUICompat.Label("Holo Controls Guide:");
            GUICompat.Label("• Mask Alpha: Transparency of the procedural 7-pass dilated texture.");
            GUICompat.Label("• Screen Shimmer: Opacity of Horoguramu_X, Horoguramu_Y, and Pattern shaders.");
            GUICompat.Label("• Printer Heat: Thermal ribbon printhead heat transfer (0 = none, 1 = max foil).");
        }

        private static void DrawTimersTab()
        {
            infiniteTime = GUICompat.Toggle(infiniteTime, " Infinite Timer (Freeze Countdown at 999s)");
            GUICompat.Space(6);

            infiniteCredits = GUICompat.Toggle(infiniteCredits, " Infinite Credits (Free Printing & Gacha)");
            GUICompat.Space(6);

            GUICompat.BeginHorizontal();
            GUICompat.Label("Display Credits:", 160);
            creditAmountStr = GUICompat.TextField(creditAmountStr);
            GUICompat.EndHorizontal();

            GUICompat.Space(6);
            unlockAll = GUICompat.Toggle(unlockAll, " Unlock All Cards (MU3 / CHU / MAI)");
            GUICompat.Space(6);

            GUICompat.BeginHorizontal();
            GUICompat.Label("Unlocked Print Stock:", 160);
            printStockStr = GUICompat.TextField(printStockStr);
            GUICompat.EndHorizontal();

            GUICompat.Space(6);
            disablePrintPeriod = GUICompat.Toggle(disablePrintPeriod, " Disable Printing Period Restrictions (Print Expired Cards)");
            GUICompat.Space(6);

            loadCustomOptCards = GUICompat.Toggle(loadCustomOptCards, " Load Custom Option Cards (Axxx Opt Packs, e.g. A900)");
            GUICompat.Space(6);

            bypassVersions = GUICompat.Toggle(bypassVersions, " Bypass All Client & Server Version Checks");
            GUICompat.Space(6);

            disableEncryption = GUICompat.Toggle(disableEncryption, " Disable Packet & URL Encryption (No Encryption)");
            GUICompat.Space(6);

            disableTLS = GUICompat.Toggle(disableTLS, " Disable TLS / HTTPS (Force Plain HTTP)");
            GUICompat.Space(6);

            enableNetworkLogging = GUICompat.Toggle(enableNetworkLogging, " Enable Network Request & Error Logging");
            GUICompat.Space(6);

            enableAssetBundleLogging = GUICompat.Toggle(enableAssetBundleLogging, " Enable AssetBundle Load & Diagnostic Logging");
            GUICompat.Space(6);

            enableOfflineBypass = GUICompat.Toggle(enableOfflineBypass, " Enable Offline Bypass (Play Without Game Server)");
        }

        private static void SaveAndApply()
        {
            try
            {
                // Update Config Entries
                ModConfig.EnableProfileOverride.Value = enableProfile;
                ModConfig.CustomUserName.Value = nameStr.Trim();
                ModConfig.CustomPlayerRating.Value = GUIStateHelper.SafeParseInt(ratingStr, GUIStateHelper.DefaultRating);
                ModConfig.CustomHighestRating.Value = GUIStateHelper.SafeParseInt(highestStr, GUIStateHelper.DefaultHighestRating);
                ModConfig.CustomPlayerLevel.Value = GUIStateHelper.SafeParseInt(levelStr, GUIStateHelper.DefaultLevel);
                ModConfig.CustomFriendCode.Value = GUIStateHelper.SanitizeFriendCode(friendCodeStr);

                ModConfig.EnableQROverride.Value = enableQR;
                ModConfig.UseCustomQRText.Value = useCustomText;
                ModConfig.CustomQRText.Value = qrText.Trim();
                ModConfig.CustomSerial.Value = GUIStateHelper.SafeParseInt(serialStr, GUIStateHelper.DefaultSerial);

                ModConfig.EnableHoloCustomization.Value = enableHolo;
                ModConfig.ForceEnableHolo.Value = forceHolo;
                ModConfig.HoloAlpha.Value = HoloHelper.Clamp01(holoAlpha);
                ModConfig.HoloPreviewIntensity.Value = HoloHelper.Clamp01(holoPreviewIntensity);
                ModConfig.HoloFoilHeat.Value = HoloHelper.Clamp01(holoFoilHeat);
                ModConfig.InvertHoloMask.Value = invertHolo;

                ModConfig.InfiniteTime.Value = infiniteTime;
                ModConfig.InfiniteCredits.Value = infiniteCredits;
                ModConfig.InfiniteCreditAmount.Value = GUIStateHelper.SafeParseInt(creditAmountStr, GUIStateHelper.DefaultCreditAmount);
                ModConfig.UnlockAllCards.Value = unlockAll;
                ModConfig.UnlockedPrintCount.Value = GUIStateHelper.SafeParseInt(printStockStr, GUIStateHelper.DefaultPrintStock);
                ModConfig.DisablePrintPeriodRestriction.Value = disablePrintPeriod;
                ModConfig.LoadCustomOptCards.Value = loadCustomOptCards;
                ModConfig.BypassAllVersionChecks.Value = bypassVersions;
                ModConfig.DisableEncryption.Value = disableEncryption;
                ModConfig.DisableTLS.Value = disableTLS;
                if (ModConfig.EnableNetworkLogging != null) ModConfig.EnableNetworkLogging.Value = enableNetworkLogging;
                if (ModConfig.EnableAssetBundleLogging != null) ModConfig.EnableAssetBundleLogging.Value = enableAssetBundleLogging;
                if (ModConfig.EnableOfflineBypass != null) ModConfig.EnableOfflineBypass.Value = enableOfflineBypass;

                // Maimai Card
                ModConfig.EnableMaimaiCardCustomText.Value = enableMaimaiCustom;
                ModConfig.UseCustomMaimaiCardVersionText.Value = useCustomMaimaiVersion;
                ModConfig.CustomMaimaiCardVersionText.Value = maimaiVersionTemplateStr.Trim();
                ModConfig.HideMaimaiCardVersionText.Value = hideMaimaiVersion;

                ModConfig.UseCustomMaimaiCardFriendCode.Value = useCustomMaimaiFriendCode;
                ModConfig.CustomMaimaiCardFriendCode.Value = GUIStateHelper.SanitizeFriendCode(maimaiFriendCodeStr);
                ModConfig.ForceShowMaimaiCardFriendCode.Value = forceShowMaimaiFriendCode;
                ModConfig.HideMaimaiCardFriendCode.Value = hideMaimaiFriendCode;

                ModConfig.UseCustomMaimaiCardSerial.Value = useCustomMaimaiSerial;
                ModConfig.CustomMaimaiCardSerial.Value = maimaiSerialStr.Trim();
                ModConfig.HideMaimaiCardSerial.Value = hideMaimaiSerial;

                ModConfig.UseCustomMaimaiCardEndDate.Value = useCustomMaimaiEndDate;
                ModConfig.CustomMaimaiCardEndDate.Value = maimaiEndDateStr.Trim();
                ModConfig.HideMaimaiCardEndDate.Value = hideMaimaiEndDate;

                MelonPreferences.Save();

                // Live apply to active session in memory
                UserProfileHelper.ApplyLiveProfileUpdates();
                CreditHelper.ApplyLiveCreditUpdate();

                statusMessage = "Settings saved to disk and applied live!";
                statusTimer = 3.5f;
            }
            catch (Exception ex)
            {
                statusMessage = string.Format("Save error: {0}", ex.Message);
                statusTimer = 4.0f;
            }
        }

        private static void ResetDefaults()
        {
            var def = GUIStateHelper.GetDefaultGUIState();
            enableProfile = def.EnableProfileOverride;
            nameStr = def.CustomUserName;
            ratingStr = def.CustomPlayerRating.ToString();
            highestStr = def.CustomHighestRating.ToString();
            levelStr = def.CustomPlayerLevel.ToString();
            friendCodeStr = def.CustomFriendCode;

            enableMaimaiCustom = def.EnableMaimaiCardCustomText;
            useCustomMaimaiVersion = def.UseCustomMaimaiCardVersionText;
            maimaiVersionTemplateStr = def.CustomMaimaiCardVersionText;
            hideMaimaiVersion = def.HideMaimaiCardVersionText;

            useCustomMaimaiFriendCode = def.UseCustomMaimaiCardFriendCode;
            maimaiFriendCodeStr = def.CustomMaimaiCardFriendCode;
            forceShowMaimaiFriendCode = def.ForceShowMaimaiCardFriendCode;
            hideMaimaiFriendCode = def.HideMaimaiCardFriendCode;

            useCustomMaimaiSerial = def.UseCustomMaimaiCardSerial;
            maimaiSerialStr = def.CustomMaimaiCardSerial;
            hideMaimaiSerial = def.HideMaimaiCardSerial;

            useCustomMaimaiEndDate = def.UseCustomMaimaiCardEndDate;
            maimaiEndDateStr = def.CustomMaimaiCardEndDate;
            hideMaimaiEndDate = def.HideMaimaiCardEndDate;

            enableQR = def.EnableQROverride;
            useCustomText = def.UseCustomQRText;
            qrText = def.CustomQRText;
            serialStr = def.CustomSerial.ToString();

            enableHolo = def.EnableHoloCustomization;
            forceHolo = def.ForceEnableHolo;
            holoAlpha = def.HoloAlpha;
            holoPreviewIntensity = def.HoloPreviewIntensity;
            holoFoilHeat = def.HoloFoilHeat;
            invertHolo = def.InvertHoloMask;

            infiniteTime = def.InfiniteTime;
            infiniteCredits = def.InfiniteCredits;
            creditAmountStr = def.InfiniteCreditAmount.ToString();
            unlockAll = def.UnlockAllCards;
            printStockStr = def.UnlockedPrintCount.ToString();
            disablePrintPeriod = def.DisablePrintPeriodRestriction;
            loadCustomOptCards = def.LoadCustomOptCards;
            bypassVersions = def.BypassAllVersionChecks;
            disableEncryption = def.DisableEncryption;
            disableTLS = def.DisableTLS;
            enableNetworkLogging = def.EnableNetworkLogging;
            enableAssetBundleLogging = def.EnableAssetBundleLogging;
            enableOfflineBypass = def.EnableOfflineBypass;

            statusMessage = "Reset to default placeholder values. Click Save & Apply to commit.";
            statusTimer = 3.5f;
        }
    }
}
