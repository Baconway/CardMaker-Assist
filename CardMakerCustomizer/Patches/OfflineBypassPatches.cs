using System;
using HarmonyLib;
using MelonLoader;
using CardMaker.Common;
using CardMaker.Common.Operation;
using CardMaker.MU3;
using CardMaker.CHU;
using CardMaker.MAI;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.Logic;

namespace CardMakerCustomizer.Patches
{
    // ==========================================
    // OperationManager Offline Bypass Patches
    // ==========================================

    [HarmonyPatch(typeof(OperationManager), "CheckAuth_Proc")]
    public static class Patch_OperationManager_CheckAuth_Proc
    {
        public static bool Prefix(OperationManager __instance)
        {
            if (ModConfig.EnableOfflineBypass == null || !ModConfig.EnableOfflineBypass.Value)
            {
                return true;
            }

            try
            {
                var tr = Traverse.Create(__instance);
                tr.Field("isAuthGood_").SetValue(true);

                string uriStr = OfflineBypassHelper.GetFallbackServerUri(CommonStatic.Settings.ServerUri);

                var dData = tr.Field("downloadData_").GetValue<OperationData>();
                dData.commonServerUri_ = CardMaker.Common.Operation.Uri.resolve(uriStr);
                tr.Field("downloadData_").SetValue(dData);

                var oData = tr.Field("operationData_").GetValue<OperationData>();
                oData.commonServerUri_ = dData.commonServerUri_;
                tr.Field("operationData_").SetValue(oData);

                var mode = tr.Field("mode_").GetValue();
                if (mode != null)
                {
                    // State.GetGameSetting is enum index 1
                    Traverse.Create(mode).Method("set", 1).GetValue();
                }
                return false;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning(StringCompat.Format("[CardMakerCustomizer] Error in CheckAuth_Proc offline bypass: {0}", new object[] { ex.Message }));
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(OperationManager), "IsAuthGood", MethodType.Getter)]
    public static class Patch_OperationManager_IsAuthGood
    {
        public static void Postfix(ref bool __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(OperationManager), "IsAliveAimeServer", MethodType.Getter)]
    public static class Patch_OperationManager_IsAliveAimeServer
    {
        public static void Postfix(ref bool __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(OperationManager), "isAlive", new Type[] { typeof(CommonStatic.Server) })]
    public static class Patch_OperationManager_isAlive
    {
        public static void Postfix(ref bool __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(OperationManager), "isUnderServerMaintenance")]
    public static class Patch_OperationManager_isUnderServerMaintenance
    {
        public static void Postfix(ref bool __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(OperationManager), "NetStatus", MethodType.Getter)]
    public static class Patch_OperationManager_NetStatus
    {
        public static void Postfix(ref CommonStatic.NetStatus __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value)
            {
                __result = CommonStatic.NetStatus.Connect;
            }
        }
    }

    [HarmonyPatch(typeof(OperationManager), "onPacketFinish", new Type[] { typeof(PacketBase.Status), typeof(CommonStatic.Server) })]
    public static class Patch_OperationManager_onPacketFinish
    {
        public static bool Prefix(OperationManager __instance, PacketBase.Status status, CommonStatic.Server serverKind)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value)
            {
                try
                {
                    var downloader = Traverse.Create(__instance).Method("getDownloader", new object[] { serverKind }).GetValue<DataDownloader>();
                    if (downloader != null)
                    {
                        downloader.notifyOnline();
                    }
                    return false;
                }
                catch
                {
                }
            }
            return true;
        }
    }

    // ==========================================
    // DataDownloader Offline Bypass Patches
    // ==========================================

    [HarmonyPatch(typeof(DataDownloader), "IsOnline", MethodType.Getter)]
    public static class Patch_DataDownloader_IsOnline
    {
        public static void Postfix(ref bool __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(DataDownloader), "WasDownloadSuccessOnce", MethodType.Getter)]
    public static class Patch_DataDownloader_WasDownloadSuccessOnce
    {
        public static void Postfix(ref bool __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(DataDownloader), "IsLastAlive", MethodType.Getter)]
    public static class Patch_DataDownloader_IsLastAlive
    {
        public static void Postfix(ref bool __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value)
            {
                __result = true;
            }
        }
    }

    // ==========================================
    // CommonWarning Error Gating Patches
    // ==========================================

    [HarmonyPatch(typeof(CommonWarning), "CurrentError", MethodType.Getter)]
    public static class Patch_CommonWarning_CurrentError
    {
        public static void Postfix(ref CommonWarning.Error __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value)
            {
                if (__result == CommonWarning.Error.All || __result == CommonWarning.Error.Any)
                {
                    __result = CommonWarning.Error.None;
                }
            }
        }
    }

    [HarmonyPatch(typeof(CommonWarning), "setError")]
    public static class Patch_CommonWarning_setError
    {
        public static void Postfix(CommonWarning __instance)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value)
            {
                try
                {
                    Traverse.Create(__instance).Field("error_").SetValue(CommonWarning.Error.None);
                }
                catch
                {
                }
            }
        }
    }

    // ==========================================
    // Common Startup Packet Fallback Patches
    // ==========================================

    [HarmonyPatch(typeof(PacketGetGameConnect), "proc")]
    public static class Patch_PacketGetGameConnect_proc
    {
        public static void Postfix(PacketGetGameConnect __instance, ref PacketBase.State __result)
        {
            if (ModConfig.EnableOfflineBypass == null || !ModConfig.EnableOfflineBypass.Value)
            {
                return;
            }

            if (__result == PacketBase.State.Error)
            {
                try
                {
                    var tr = Traverse.Create(__instance);
                    string baseUri = OfflineBypassHelper.GetFallbackServerUri(CommonStatic.Settings.ServerUri);
                    tr.Field("_titleServersUri").SetValue(OfflineBypassHelper.CreateFallbackTitleServersUri(baseUri));
                    tr.Field("state_").SetValue(PacketBase.State.Done);
                    __result = PacketBase.State.Done;
                    MelonLogger.Msg("[CardMakerCustomizer] Offline bypass: PacketGetGameConnect completed with fallback URIs.");
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning(StringCompat.Format("[CardMakerCustomizer] Failed to apply fallback PacketGetGameConnect: {0}", new object[] { ex.Message }));
                }
            }
        }
    }

    [HarmonyPatch(typeof(PacketGetGameSetting), "proc")]
    public static class Patch_PacketGetGameSetting_proc
    {
        public static void Postfix(PacketGetGameSetting __instance, ref PacketBase.State __result)
        {
            if (ModConfig.EnableOfflineBypass == null || !ModConfig.EnableOfflineBypass.Value)
            {
                return;
            }

            if (__result == PacketBase.State.Error)
            {
                try
                {
                    var tr = Traverse.Create(__instance);
                    tr.Field("_gameSetting").SetValue(OfflineBypassHelper.CreateFallbackGameSetting());
                    tr.Field("state_").SetValue(PacketBase.State.Done);
                    __result = PacketBase.State.Done;
                    MelonLogger.Msg("[CardMakerCustomizer] Offline bypass: PacketGetGameSetting completed with fallback settings.");
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning(StringCompat.Format("[CardMakerCustomizer] Failed to apply fallback PacketGetGameSetting: {0}", new object[] { ex.Message }));
                }
            }
        }
    }

    [HarmonyPatch(typeof(PacketGetGameTitleStop), "proc")]
    public static class Patch_PacketGetGameTitleStop_proc
    {
        public static void Postfix(PacketGetGameTitleStop __instance, ref PacketBase.State __result)
        {
            if (ModConfig.EnableOfflineBypass == null || !ModConfig.EnableOfflineBypass.Value)
            {
                return;
            }

            if (__result == PacketBase.State.Error)
            {
                try
                {
                    if (ContextBase<CommonContext>.Exists && ContextBase<CommonContext>.Instance != null)
                    {
                        var opManager = ContextBase<CommonContext>.Instance.OperationManager;
                        if (opManager != null)
                        {
                            opManager.ClrCloseChunithm();
                            opManager.ClrCloseMaimai();
                            opManager.ClrCloseOngeki();
                        }
                    }
                    Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                    __result = PacketBase.State.Done;
                    MelonLogger.Msg("[CardMakerCustomizer] Offline bypass: PacketGetGameTitleStop completed (all titles open).");
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning(StringCompat.Format("[CardMakerCustomizer] Failed to apply fallback PacketGetGameTitleStop: {0}", new object[] { ex.Message }));
                }
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketPing), "proc")]
    public static class Patch_MU3_PacketPing_proc
    {
        public static void Postfix(CardMaker.MU3.PacketPing __instance, ref PacketBase.State __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value && __result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.CHU.PacketPing), "proc")]
    public static class Patch_CHU_PacketPing_proc
    {
        public static void Postfix(CardMaker.CHU.PacketPing __instance, ref PacketBase.State __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value && __result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MAI.PacketPing), "proc")]
    public static class Patch_MAI_PacketPing_proc
    {
        public static void Postfix(CardMaker.MAI.PacketPing __instance, ref PacketBase.State __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value && __result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
            }
        }
    }

    [HarmonyPatch(typeof(PacketDummyPing), "proc")]
    public static class Patch_PacketDummyPing_proc
    {
        public static void Postfix(PacketDummyPing __instance, ref PacketBase.State __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value && __result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
            }
        }
    }

    // ==========================================
    // Session & Print Offline Fallbacks
    // ==========================================

    [HarmonyPatch(typeof(CardMaker.MAI.PacketGetUserData), "proc")]
    public static class Patch_MAI_PacketGetUserData_proc
    {
        public static void Postfix(CardMaker.MAI.PacketGetUserData __instance, ref PacketBase.State __result)
        {
            bool bypass = (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value) ||
                          (ModConfig.UnlockAllCards != null && ModConfig.UnlockAllCards.Value) ||
                          (ModConfig.EnableProfileOverride != null && ModConfig.EnableProfileOverride.Value);

            if (!bypass)
            {
                return;
            }

            if (__result == PacketBase.State.Error || !__instance.IsExistUser)
            {
                try
                {
                    var tr = Traverse.Create(__instance);
                    var user = tr.Field("user_").GetValue<MAIUser>();
                    if (user != null)
                    {
                        string name = ModConfig.CustomUserName != null && !StringCompat.IsNullOrWhiteSpace(ModConfig.CustomUserName.Value)
                            ? ModConfig.CustomUserName.Value.Trim()
                            : "MAIMAI";

                        var data = new CardMaker.MAI.UserData();
                        data.userName = name;
                        data.playerRating = ModConfig.CustomPlayerRating != null && ModConfig.CustomPlayerRating.Value > 0 ? ModConfig.CustomPlayerRating.Value : 1600;
                        data.highestRating = ModConfig.CustomHighestRating != null && ModConfig.CustomHighestRating.Value > 0 ? ModConfig.CustomHighestRating.Value : 1600;
                        user.Data.copyFrom(data);
                    }
                    __instance.IsExistUser = true;
                    tr.Field("state_").SetValue(PacketBase.State.Done);
                    __result = PacketBase.State.Done;
                    MelonLogger.Msg("[CardMakerCustomizer] Fallback maimai user profile synthesized successfully.");
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning(StringCompat.Format("[CardMakerCustomizer] Failed to synthesize maimai fallback profile: {0}", new object[] { ex.Message }));
                }
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MAI.PacketPrinterLogin), "proc")]
    public static class Patch_MAI_PacketPrinterLogin_proc
    {
        public static void Postfix(CardMaker.MAI.PacketPrinterLogin __instance, ref PacketBase.State __result)
        {
            bool bypass = (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value) ||
                          (ModConfig.UnlockAllCards != null && ModConfig.UnlockAllCards.Value);

            if (!bypass)
            {
                return;
            }

            if (__result == PacketBase.State.Error || __instance.Result != 1)
            {
                try
                {
                    var tr = Traverse.Create(__instance);
                    tr.Field("result_").SetValue(1);
                    tr.Field("state_").SetValue(PacketBase.State.Done);
                    __result = PacketBase.State.Done;
                    var user = tr.Field("user_").GetValue<MAIUser>();
                    if (user != null && user.Data != null)
                    {
                        user.Data.updatePlace();
                    }
                    MelonLogger.Msg("[CardMakerCustomizer] Fallback maimai PrinterLogin applied (Result=1).");
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning(StringCompat.Format("[CardMakerCustomizer] Failed to apply fallback MAI PrinterLogin: {0}", new object[] { ex.Message }));
                }
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketPrinterLogout), "proc")]
    public static class Patch_MU3_PacketPrinterLogout_proc
    {
        public static void Postfix(CardMaker.MU3.PacketPrinterLogout __instance, ref PacketBase.State __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value && __result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MAI.PacketPrinterLogout), "proc")]
    public static class Patch_MAI_PacketPrinterLogout_proc
    {
        public static void Postfix(CardMaker.MAI.PacketPrinterLogout __instance, ref PacketBase.State __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value && __result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketUpsertUserPrint), "proc")]
    public static class Patch_MU3_PacketUpsertUserPrint_proc
    {
        public static void Postfix(CardMaker.MU3.PacketUpsertUserPrint __instance, ref PacketBase.State __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value && __result == PacketBase.State.Error)
            {
                try
                {
                    var tr = Traverse.Create(__instance);
                    var query = tr.Field("printQuery_").GetValue<MU3PrintQueue.MU3Query>();
                    if (query != null)
                    {
                        query.orderId_ = 1;
                        query.serialId_ = OfflineBypassHelper.GenerateOfflineSerial(1);
                    }
                    tr.Field("state_").SetValue(PacketBase.State.Done);
                    __result = PacketBase.State.Done;
                    MelonLogger.Msg("[CardMakerCustomizer] Offline bypass: MU3 PacketUpsertUserPrint simulated as Done.");
                }
                catch
                {
                }
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MAI.PacketUpsertUserPrint), "proc")]
    public static class Patch_MAI_PacketUpsertUserPrint_proc
    {
        public static void Postfix(CardMaker.MAI.PacketUpsertUserPrint __instance, ref PacketBase.State __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value && __result == PacketBase.State.Error)
            {
                try
                {
                    var tr = Traverse.Create(__instance);
                    var query = tr.Field("printQuery_").GetValue<MAIPrintQueue.MAIQuery>();
                    if (query != null)
                    {
                        DateTime now = DateTime.Now;
                        CardMaker.MAI.PacketUpsertUserPrint.procAtReceived(query, 1, OfflineBypassHelper.GenerateOfflineSerial(1), NetPacketUtil.toString(now), NetPacketUtil.toString(now.AddDays(30)));
                    }
                    tr.Field("state_").SetValue(PacketBase.State.Done);
                    __result = PacketBase.State.Done;
                    MelonLogger.Msg("[CardMakerCustomizer] Offline bypass: MAI PacketUpsertUserPrint simulated as Done.");
                }
                catch
                {
                }
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MU3.PacketUpsertUserPrintlog), "proc")]
    public static class Patch_MU3_PacketUpsertUserPrintlog_proc
    {
        public static void Postfix(CardMaker.MU3.PacketUpsertUserPrintlog __instance, ref PacketBase.State __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value && __result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
            }
        }
    }

    [HarmonyPatch(typeof(CardMaker.MAI.PacketUpsertUserPrintlog), "proc")]
    public static class Patch_MAI_PacketUpsertUserPrintlog_proc
    {
        public static void Postfix(CardMaker.MAI.PacketUpsertUserPrintlog __instance, ref PacketBase.State __result)
        {
            if (ModConfig.EnableOfflineBypass != null && ModConfig.EnableOfflineBypass.Value && __result == PacketBase.State.Error)
            {
                Traverse.Create(__instance).Field("state_").SetValue(PacketBase.State.Done);
                __result = PacketBase.State.Done;
            }
        }
    }
}
