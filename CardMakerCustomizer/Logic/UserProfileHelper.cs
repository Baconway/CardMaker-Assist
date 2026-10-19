using System;
using System.Text;
using HarmonyLib;
using CardMaker.MU3;
using CardMaker.CHU;
using CardMaker.MAI;
using CardMakerCustomizer.Config;
using MelonLoader;

namespace CardMakerCustomizer.Logic
{
    public static class UserProfileHelper
    {
        public static string ResolveUserName(string original, string custom)
        {
            if (!StringCompat.IsNullOrWhiteSpace(custom))
            {
                return custom.Trim();
            }

            return original;
        }

        public static int ResolveRating(int original, int custom)
        {
            if (custom >= 0)
            {
                return custom;
            }

            return original;
        }

        public static int ResolveLevel(int original, int custom)
        {
            if (custom >= 0)
            {
                return custom;
            }

            return original;
        }

        public static string ResolveFriendCode(string original, string custom)
        {
            if (!StringCompat.IsNullOrWhiteSpace(custom))
            {
                return custom.Trim();
            }

            return original;
        }

        public static string FormatFriendCodeDisplay(string rawCode)
        {
            if (string.IsNullOrEmpty(rawCode))
            {
                return string.Empty;
            }

            string clean = rawCode.Replace(" ", "");
            if (clean.Length == 13)
            {
                return $"{clean.Substring(0, 4)} {clean.Substring(4, 4)} {clean.Substring(8, 4)} {clean.Substring(12, 1)}";
            }

            if (clean.Length == 12)
            {
                return $"{clean.Substring(0, 4)} {clean.Substring(4, 4)} {clean.Substring(8, 4)}";
            }

            return rawCode;
        }

        public static void ApplyLiveProfileUpdates()
        {
            try
            {
                if (!ModConfig.EnableProfileOverride.Value)
                {
                    return;
                }

                // Live update Ongeki active session
                if (MU3Context.Exists && MU3Context.Instance != null && MU3Context.Instance.User != null)
                {
                    var mu3Data = MU3Context.Instance.User.Data;
                    if (mu3Data != null)
                    {
                        var tr = Traverse.Create(mu3Data);
                        if (!StringCompat.IsNullOrWhiteSpace(ModConfig.CustomUserName.Value))
                        {
                            tr.Field("userName_").SetValue(ModConfig.CustomUserName.Value.Trim());
                        }
                        if (ModConfig.CustomPlayerRating.Value >= 0)
                        {
                            tr.Field("playerRating_").SetValue(ModConfig.CustomPlayerRating.Value);
                        }
                        if (ModConfig.CustomHighestRating.Value >= 0)
                        {
                            tr.Field("highestRating_").SetValue(ModConfig.CustomHighestRating.Value);
                        }
                        if (ModConfig.CustomPlayerLevel.Value >= 0)
                        {
                            tr.Field("level_").SetValue(ModConfig.CustomPlayerLevel.Value);
                        }
                    }
                }

                // Live update CHUNITHM active session
                if (CHUContext.Exists && CHUContext.Instance != null && CHUContext.Instance.User != null)
                {
                    var chuData = CHUContext.Instance.User.Data;
                    if (chuData != null)
                    {
                        var inner = Traverse.Create(chuData).Field("userData_").GetValue<CardMaker.CHU.UserData>();
                        if (inner != null)
                        {
                            if (!StringCompat.IsNullOrWhiteSpace(ModConfig.CustomUserName.Value))
                            {
                                inner.userName = ModConfig.CustomUserName.Value.Trim();
                                inner.userNameEx = ModConfig.CustomUserName.Value.Trim();
                            }
                            if (ModConfig.CustomPlayerRating.Value >= 0)
                            {
                                inner.playerRating = ModConfig.CustomPlayerRating.Value;
                            }
                            if (ModConfig.CustomHighestRating.Value >= 0)
                            {
                                inner.highestRating = ModConfig.CustomHighestRating.Value;
                            }
                            if (ModConfig.CustomPlayerLevel.Value >= 0)
                            {
                                inner.level = ModConfig.CustomPlayerLevel.Value;
                            }
                        }
                    }
                }

                // Live update maimai DX active session
                if (MAIContext.Exists && MAIContext.Instance != null && MAIContext.Instance.User != null)
                {
                    var maiData = MAIContext.Instance.User.Data;
                    if (maiData != null)
                    {
                        var inner = Traverse.Create(maiData).Field("userData_").GetValue<CardMaker.MAI.UserData>();
                        if (inner != null)
                        {
                            if (!StringCompat.IsNullOrWhiteSpace(ModConfig.CustomUserName.Value))
                            {
                                inner.userName = ModConfig.CustomUserName.Value.Trim();
                            }
                            if (ModConfig.CustomPlayerRating.Value >= 0)
                            {
                                inner.playerRating = ModConfig.CustomPlayerRating.Value;
                            }
                            if (ModConfig.CustomHighestRating.Value >= 0)
                            {
                                inner.highestRating = ModConfig.CustomHighestRating.Value;
                            }
                            if (!StringCompat.IsNullOrWhiteSpace(ModConfig.CustomFriendCode.Value))
                            {
                                inner.friendCode = ModConfig.CustomFriendCode.Value.Trim();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CardMakerCustomizer] Live profile update error: {ex.Message}");
            }
        }
    }
}
