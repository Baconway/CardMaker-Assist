using System;
using HarmonyLib;
using CardMaker.Common;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.Logic;

namespace CardMakerCustomizer.Patches
{
    // =========================================================================
    // Network Packet Logging & Diagnostics Patches
    // Hooks Sega CardMaker's PacketBase lifecycle to provide detailed logging
    // for outgoing requests, server responses, retries, and network errors.
    // =========================================================================

    [HarmonyPatch(typeof(PacketBase), "procImpl")]
    public static class Patch_PacketBase_procImpl
    {
        public static void Prefix(PacketBase __instance, out PacketBase.State __state)
        {
            __state = PacketBase.State.Ready;
            if ((object)__instance == null) return;

            try
            {
                __state = __instance.state;
            }
            catch
            {
                __state = PacketBase.State.Ready;
            }
        }

        public static void Postfix(PacketBase __instance, PacketBase.State __state, PacketBase.State __result)
        {
            if (ModConfig.EnableNetworkLogging != null && !ModConfig.EnableNetworkLogging.Value)
            {
                return;
            }

            if ((object)__instance == null) return;

            try
            {
                NetworkDiagnosticHelper.HandlePacketStateTransition(__instance, __state, __result);
            }
            catch (Exception ex)
            {
                NetworkDiagnosticHelper.SafeError(StringCompat.Format(
                    "[CardMakerCustomizer] [Network] Error in procImpl logging hook: {0}", ex.Message));
            }
        }
    }

    [HarmonyPatch(typeof(PacketBase), "create", new Type[] {
        typeof(INetQuery),
        typeof(ushort),
        typeof(string),
        typeof(string),
        typeof(string),
        typeof(PacketBase.OnFinishDeligate),
        typeof(CommonStatic.Server)
    })]
    public static class Patch_PacketBase_create
    {
        public static void Postfix(PacketBase __instance, INetQuery query, ushort port, string host, string hostPort, string basePath, PacketBase.OnFinishDeligate onFinishDelegate, CommonStatic.Server server, bool __result)
        {
            if (ModConfig.EnableNetworkLogging != null && !ModConfig.EnableNetworkLogging.Value)
            {
                return;
            }

            if ((object)__instance == null) return;

            try
            {
                NetworkDiagnosticHelper.LogPacketCreated(__instance, query, host, port, basePath, server, __result);
            }
            catch (Exception ex)
            {
                NetworkDiagnosticHelper.SafeError(StringCompat.Format(
                    "[CardMakerCustomizer] [Network] Error in create logging hook: {0}", ex.Message));
            }
        }
    }
}
