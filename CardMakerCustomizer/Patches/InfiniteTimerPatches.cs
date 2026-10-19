using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using CardMaker.Common;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.Logic;
using MelonLoader;

namespace CardMakerCustomizer.Patches
{
    [HarmonyPatch(typeof(CommonTimer), "Update")]
    public static class Patch_CommonTimer_Update
    {
        public static bool Prefix(CommonTimer __instance)
        {
            if (ModConfig.InfiniteTime.Value)
            {
                try
                {
                    var tr = Traverse.Create(__instance);
                    float cur = tr.Field("counter_").GetValue<float>();
                    if (cur != TimerHelper.FrozenCounterValue)
                    {
                        tr.Field("counter_").SetValue(TimerHelper.FrozenCounterValue);
                        var uiCounter = tr.Field("uiCounter_").GetValue<MU3UICounter>();
                        if (uiCounter != null)
                        {
                            uiCounter.CounterAsInt = TimerHelper.FrozenCounterInt;
                        }
                    }
                    return false; // Prevent countdown decrement
                }
                catch
                {
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CommonTimer), "IsTimeOut", MethodType.Getter)]
    public static class Patch_CommonTimer_IsTimeOut
    {
        public static bool Prefix(ref bool __result)
        {
            if (ModConfig.InfiniteTime.Value)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CommonTimer), "start", new Type[] {
        typeof(int),
        typeof(int),
        typeof(UnityAction<CommonTimer>),
        typeof(bool),
        typeof(bool),
        typeof(bool)
    })]
    public static class Patch_CommonTimer_start
    {
        public static void Postfix(CommonTimer __instance)
        {
            if (ModConfig.InfiniteTime.Value)
            {
                try
                {
                    var tr = Traverse.Create(__instance);
                    tr.Field("counter_").SetValue(TimerHelper.FrozenCounterValue);
                    var uiCounter = tr.Field("uiCounter_").GetValue<MU3UICounter>();
                    if (uiCounter != null)
                    {
                        uiCounter.CounterAsInt = TimerHelper.FrozenCounterInt;
                    }
                }
                catch
                {
                }
            }
        }
    }
}
