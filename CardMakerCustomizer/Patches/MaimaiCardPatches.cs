using System;
using HarmonyLib;
using UnityEngine;
using CardMaker.Common;
using CardMaker.MAI;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.Logic;
using MelonLoader;

namespace CardMakerCustomizer.Patches
{
    [HarmonyPatch(typeof(MAIContext), "convertToDisplayinVerCharaId", new Type[] { typeof(int), typeof(int) })]
    public static class Patch_MAIContext_convertToDisplayinVerCharaId
    {
        public static bool Prefix(int cardId, int charaId, ref string __result)
        {
            try
            {
                if (ModConfig.EnableMaimaiCardCustomText == null || !ModConfig.EnableMaimaiCardCustomText.Value)
                {
                    return true;
                }

                if (ModConfig.HideMaimaiCardVersionText != null && ModConfig.HideMaimaiCardVersionText.Value)
                {
                    __result = string.Empty;
                    return false;
                }

                if (ModConfig.UseCustomMaimaiCardVersionText != null && ModConfig.UseCustomMaimaiCardVersionText.Value)
                {
                    string versionStr = MAIStudioHelper.cardIdToVersionStr(cardId);
                    int uniqueCharaId = MAIStudioHelper.charaIdToUniqueId(charaId);
                    string template = ModConfig.CustomMaimaiCardVersionText != null ? ModConfig.CustomMaimaiCardVersionText.Value : null;
                    __result = MaimaiCardHelper.FormatVersionText(template, versionStr, uniqueCharaId, cardId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Error in convertToDisplayinVerCharaId prefix: {ex.Message}");
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(UI_CMA_CardData_00), "reset", new Type[] { typeof(bool) })]
    public static class Patch_UI_CMA_CardData_00_reset
    {
        public static void Postfix(UI_CMA_CardData_00 __instance, bool updateTexture)
        {
            if (ModConfig.EnableMaimaiCardCustomText == null || !ModConfig.EnableMaimaiCardCustomText.Value)
            {
                return;
            }

            try
            {
                var tr = Traverse.Create(__instance);
                var textVerCharaId = tr.Field("textVerCharaId_").GetValue<MU3Text>();
                var rootFriendCode = tr.Field("rootFriendCode_").GetValue<GameObject>();
                var friendCodeText = tr.Field("friendCodeText_").GetValue<GameObject>();
                var friendCodeHyphen = tr.Field("friendCodeHyphen_").GetValue<GameObject>();
                var textSerialCode = tr.Field("textSerialCode_").GetValue<MU3Text>();
                var textEndDate = tr.Field("textEndDate_").GetValue<MU3Text>();
                var dispInfo = tr.Field("dispInfo_").GetValue<UI_CMA_CardData_00.DispInfo>();

                // 1. Version / Footer Text
                if (textVerCharaId != null)
                {
                    if (ModConfig.HideMaimaiCardVersionText != null && ModConfig.HideMaimaiCardVersionText.Value)
                    {
                        textVerCharaId.text = string.Empty;
                        textVerCharaId.gameObject.SetActive(false);
                    }
                    else if (ModConfig.UseCustomMaimaiCardVersionText != null && ModConfig.UseCustomMaimaiCardVersionText.Value)
                    {
                        int cardId = dispInfo != null ? dispInfo.getCardId() : 0;
                        int charaId = dispInfo != null ? dispInfo.getCurCharaId() : 0;
                        string versionStr = MAIStudioHelper.cardIdToVersionStr(cardId);
                        int uniqueCharaId = MAIStudioHelper.charaIdToUniqueId(charaId);
                        string template = ModConfig.CustomMaimaiCardVersionText != null ? ModConfig.CustomMaimaiCardVersionText.Value : null;
                        textVerCharaId.text = MaimaiCardHelper.FormatVersionText(template, versionStr, uniqueCharaId, cardId);
                        textVerCharaId.gameObject.SetActive(true);
                    }
                }

                // 2. Friend Code
                if (ModConfig.HideMaimaiCardFriendCode != null && ModConfig.HideMaimaiCardFriendCode.Value)
                {
                    if (rootFriendCode != null)
                    {
                        rootFriendCode.SetActive(false);
                    }
                }
                else if ((ModConfig.UseCustomMaimaiCardFriendCode != null && ModConfig.UseCustomMaimaiCardFriendCode.Value) ||
                         (ModConfig.ForceShowMaimaiCardFriendCode != null && ModConfig.ForceShowMaimaiCardFriendCode.Value))
                {
                    if (rootFriendCode != null)
                    {
                        rootFriendCode.SetActive(true);
                    }
                    if (friendCodeHyphen != null)
                    {
                        friendCodeHyphen.SetActive(false);
                    }
                    if (friendCodeText != null)
                    {
                        friendCodeText.SetActive(true);
                        var fcMu3 = friendCodeText.GetComponentInChildren<MU3Text>();
                        if (fcMu3 != null)
                        {
                            string customCode = (ModConfig.UseCustomMaimaiCardFriendCode != null && ModConfig.UseCustomMaimaiCardFriendCode.Value)
                                ? ModConfig.CustomMaimaiCardFriendCode.Value
                                : (dispInfo != null ? dispInfo.FriendCode : null);
                            fcMu3.text = customCode ?? string.Empty;
                        }
                    }
                }

                // 3. Serial Number
                if (textSerialCode != null)
                {
                    if (ModConfig.HideMaimaiCardSerial != null && ModConfig.HideMaimaiCardSerial.Value)
                    {
                        textSerialCode.text = string.Empty;
                        textSerialCode.gameObject.SetActive(false);
                    }
                    else if (ModConfig.UseCustomMaimaiCardSerial != null && ModConfig.UseCustomMaimaiCardSerial.Value)
                    {
                        string customSerial = ModConfig.CustomMaimaiCardSerial != null ? ModConfig.CustomMaimaiCardSerial.Value : string.Empty;
                        textSerialCode.text = customSerial;
                        textSerialCode.gameObject.SetActive(true);
                    }
                }

                // 4. End Date / Expiration Period
                if (textEndDate != null)
                {
                    if (ModConfig.HideMaimaiCardEndDate != null && ModConfig.HideMaimaiCardEndDate.Value)
                    {
                        textEndDate.text = string.Empty;
                        textEndDate.gameObject.SetActive(false);
                    }
                    else if (ModConfig.UseCustomMaimaiCardEndDate != null && ModConfig.UseCustomMaimaiCardEndDate.Value)
                    {
                        string customEndDate = ModConfig.CustomMaimaiCardEndDate != null ? ModConfig.CustomMaimaiCardEndDate.Value : string.Empty;
                        textEndDate.text = customEndDate;
                        textEndDate.gameObject.SetActive(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Error in UI_CMA_CardData_00.reset postfix: {ex.Message}");
            }
        }
    }
}
