using System;
using CardMaker.Common;
using CardMaker.Common.Operation;
using CardMakerCustomizer.Config;

namespace CardMakerCustomizer.Logic
{
    public static class OfflineBypassHelper
    {
        public const string DefaultFallbackUri = "http://127.0.0.1";
        public const string DefaultOfflineVersion = "1.44";

        public static bool? ForceActiveForTesting = null;

        public static bool IsOfflineBypassActive
        {
            get
            {
                if (ForceActiveForTesting.HasValue)
                {
                    return ForceActiveForTesting.Value;
                }
                return ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value;
            }
        }

        public static bool ShouldBypassIsOnline(bool currentIsOnline)
        {
            return IsOfflineBypassActive || currentIsOnline;
        }

        public static bool ShouldBypassWasDownloadSuccessOnce(bool currentSuccess)
        {
            return IsOfflineBypassActive || currentSuccess;
        }

        public static bool ShouldBypassIsAlive(bool currentAlive)
        {
            return IsOfflineBypassActive || currentAlive;
        }

        public static bool ShouldBypassIsAuthGood(bool currentAuthGood)
        {
            return IsOfflineBypassActive || currentAuthGood;
        }

        public static bool ShouldBypassIsAliveAimeServer(bool currentAlive)
        {
            return IsOfflineBypassActive || currentAlive;
        }

        public static bool ShouldBypassUnderMaintenance(bool currentUnderMaintenance)
        {
            return IsOfflineBypassActive ? false : currentUnderMaintenance;
        }

        public static int FilterWarningError(int currentError)
        {
            // 0: None, 1: All, 2: Any
            if (IsOfflineBypassActive)
            {
                if (currentError == 1 || currentError == 2)
                {
                    return 0; // None
                }
            }
            return currentError;
        }

        public static CommonWarning.Error FilterWarningError(CommonWarning.Error currentError)
        {
            return (CommonWarning.Error)FilterWarningError((int)currentError);
        }

        public static int FilterNetStatus(int currentStatus)
        {
            // 0: NotConnect, 1: PoorConnect, 2: Connect
            if (IsOfflineBypassActive)
            {
                if (currentStatus == 0) // NotConnect
                {
                    return 2; // Connect
                }
            }
            return currentStatus;
        }

        public static CommonStatic.NetStatus FilterNetStatus(CommonStatic.NetStatus currentStatus)
        {
            return (CommonStatic.NetStatus)FilterNetStatus((int)currentStatus);
        }

        public static string GetFallbackServerUri(string existingUri)
        {
            if (!StringCompat.IsNullOrWhiteSpace(existingUri))
            {
                return existingUri.Trim();
            }
            return DefaultFallbackUri;
        }

        public static TitleServersUri CreateFallbackTitleServersUri(string baseUri)
        {
            string uri = GetFallbackServerUri(baseUri);
            TitleServersUri result = default(TitleServersUri);
            result.ongekiServerUri = uri;
            result.chunithmServerUri = uri;
            result.maimaiServerUri = uri;
            return result;
        }

        public static CardMaker.Common.Operation.GameSetting CreateFallbackGameSetting()
        {
            CardMaker.Common.Operation.GameSetting setting = default(CardMaker.Common.Operation.GameSetting);
            CardMaker.Common.Version.TryParse(DefaultOfflineVersion, out setting.dataVersion);
            CardMaker.Common.Version.TryParse(DefaultOfflineVersion, out setting.ongekiCmVersion);
            CardMaker.Common.Version.TryParse(DefaultOfflineVersion, out setting.chuniCmVersion);
            CardMaker.Common.Version.TryParse(DefaultOfflineVersion, out setting.maimaiCmVersion);
            setting.isMaintenance = false;
            setting.requestInterval = 300;
            setting.rebootStartTime = DateTime.MaxValue;
            setting.rebootEndTime = DateTime.MinValue;
            setting.isBackgroundDistribute = false;
            setting.watermark = false;
            return setting;
        }

        public static string GenerateOfflineSerial(int orderId)
        {
            int safeId = orderId > 0 ? orderId : 1;
            return "OFFLINE" + safeId.ToString().PadLeft(8, '0');
        }
    }
}
