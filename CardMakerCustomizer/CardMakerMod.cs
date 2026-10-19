using System;
using MelonLoader;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.UI;

[assembly: MelonInfo(typeof(CardMakerCustomizer.CardMakerMod), "CardMakerCustomizer", "1.0.0", "Baconway")]
[assembly: MelonGame(null, null)]

namespace CardMakerCustomizer
{
    public class CardMakerMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("==================================================");
            LoggerInstance.Msg("  CardMakerCustomizer v1.0.0 Initializing...     ");
            LoggerInstance.Msg("==================================================");

            ModConfig.Initialize();

            LoggerInstance.Msg($"[Config] Bypass All Version Checks : {ModConfig.BypassAllVersionChecks.Value}");
            LoggerInstance.Msg($"[Config] Unlock All Cards          : {ModConfig.UnlockAllCards.Value} (Stock: {ModConfig.UnlockedPrintCount.Value})");
            LoggerInstance.Msg($"[Config] Load Custom Opt Cards     : {ModConfig.LoadCustomOptCards.Value} (Default: ON)");
            LoggerInstance.Msg($"[Config] Profile Override Enabled  : {ModConfig.EnableProfileOverride.Value} (Default: ON)");
            LoggerInstance.Msg($"[Config] QR Override Enabled       : {ModConfig.EnableQROverride.Value} (Default: ON)");
            LoggerInstance.Msg($"[Config] Infinite Time             : {ModConfig.InfiniteTime.Value} (Default: ON)");
            LoggerInstance.Msg($"[Config] Infinite Credits          : {ModConfig.InfiniteCredits.Value} (Display: {ModConfig.InfiniteCreditAmount.Value})");
            LoggerInstance.Msg($"[Config] Disable Encryption        : {ModConfig.DisableEncryption.Value} (Default: ON)");
            LoggerInstance.Msg($"[Config] Disable TLS / HTTPS       : {ModConfig.DisableTLS.Value} (Default: OFF)");
            LoggerInstance.Msg($"[Config] Holo Customization Enabled: {ModConfig.EnableHoloCustomization.Value} (Default: ON)");
            LoggerInstance.Msg($"[Config] Force Holo On All Cards   : {ModConfig.ForceEnableHolo.Value} (Default: OFF)");
            LoggerInstance.Msg($"[Config] Holo Mask Alpha           : {ModConfig.HoloAlpha.Value:F2}");
            LoggerInstance.Msg($"[Config] Holo Screen Shimmer       : {ModConfig.HoloPreviewIntensity.Value:F2}");
            LoggerInstance.Msg($"[Config] Holo Foil Print Heat      : {ModConfig.HoloFoilHeat.Value:F2}");
            LoggerInstance.Msg($"[Config] Maimai Custom Text Enabled: {ModConfig.EnableMaimaiCardCustomText.Value} (Default: ON)");
            LoggerInstance.Msg($"[Config] Maimai Version Template   : {ModConfig.CustomMaimaiCardVersionText.Value}");
            LoggerInstance.Msg($"[Config] AssetBundle Load Logging  : {ModConfig.EnableAssetBundleLogging.Value} (Default: ON)");

            if (ModConfig.LoadCustomOptCards.Value)
            {
                CardMakerCustomizer.Logic.CustomOptHelper.ScanAndLogCustomOptions();
            }

            try
            {
                CardMaker.Common.APIInitializer.setSafix<CardMaker.MU3.GetUserGachaSupply>(1);
            }
            catch
            {
            }

            LoggerInstance.Msg("CardMakerCustomizer loaded. Press ~ to open in-game GUI.");
        }

        public override void OnUpdate()
        {
            ModGUI.OnUpdate();
        }

        public override void OnGUI()
        {
            ModGUI.OnGUI();
        }
    }
}
