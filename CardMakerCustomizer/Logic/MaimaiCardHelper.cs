using System;

namespace CardMakerCustomizer.Logic
{
    public struct VersionDisplayResult
    {
        public bool IsVisible;
        public string Text;

        public VersionDisplayResult(bool isVisible, string text)
        {
            IsVisible = isVisible;
            Text = text;
        }
    }

    public struct FriendCodeDisplayResult
    {
        public bool IsVisible;
        public bool ShowHyphen;
        public bool ShowText;
        public string Text;

        public FriendCodeDisplayResult(bool isVisible, bool showHyphen, bool showText, string text)
        {
            IsVisible = isVisible;
            ShowHyphen = showHyphen;
            ShowText = showText;
            Text = text;
        }
    }

    public struct TextDisplayResult
    {
        public bool IsVisible;
        public string Text;

        public TextDisplayResult(bool isVisible, string text)
        {
            IsVisible = isVisible;
            Text = text;
        }
    }

    public static class MaimaiCardHelper
    {
        public const string DefaultPrefix = "[maimaiDX]";
        public const string DefaultVersionTemplate = "[maimaiDX]{version}-{charaId}";

        public static string FormatUniqueCharaId(int charaId)
        {
            return charaId.ToString("D4");
        }

        public static string FormatVersionText(string template, string versionStr, int charaId, int cardId, string defaultPrefix = DefaultPrefix)
        {
            if (template == null)
            {
                return string.Empty;
            }

            string result = template;
            result = result.Replace("{prefix}", defaultPrefix ?? string.Empty);
            result = result.Replace("{version}", versionStr ?? string.Empty);
            result = result.Replace("{charaId}", FormatUniqueCharaId(charaId));
            result = result.Replace("{rawCharaId}", charaId.ToString());
            result = result.Replace("{cardId}", cardId.ToString());

            return result;
        }

        public static VersionDisplayResult ResolveVersionDisplay(
            bool enabled,
            bool hide,
            bool useCustom,
            string template,
            string versionStr,
            int charaId,
            int cardId,
            string defaultFullText)
        {
            if (!enabled)
            {
                return new VersionDisplayResult(true, defaultFullText ?? string.Empty);
            }

            if (hide)
            {
                return new VersionDisplayResult(false, string.Empty);
            }

            if (useCustom)
            {
                string text = FormatVersionText(template, versionStr, charaId, cardId);
                return new VersionDisplayResult(true, text);
            }

            return new VersionDisplayResult(true, defaultFullText ?? string.Empty);
        }

        public static FriendCodeDisplayResult ResolveFriendCodeDisplay(
            bool enabled,
            bool hide,
            bool useCustom,
            bool forceShow,
            string customFriendCode,
            bool hasFriendCode,
            string currentFriendCode)
        {
            if (!enabled)
            {
                if (hasFriendCode)
                {
                    return new FriendCodeDisplayResult(true, false, true, currentFriendCode ?? string.Empty);
                }
                return new FriendCodeDisplayResult(true, true, false, string.Empty);
            }

            if (hide)
            {
                return new FriendCodeDisplayResult(false, false, false, string.Empty);
            }

            if (useCustom)
            {
                string text = customFriendCode ?? string.Empty;
                return new FriendCodeDisplayResult(true, false, true, text);
            }

            if (forceShow)
            {
                string text = !string.IsNullOrEmpty(currentFriendCode) ? currentFriendCode : (customFriendCode ?? string.Empty);
                return new FriendCodeDisplayResult(true, false, true, text);
            }

            if (hasFriendCode)
            {
                return new FriendCodeDisplayResult(true, false, true, currentFriendCode ?? string.Empty);
            }

            return new FriendCodeDisplayResult(true, true, false, string.Empty);
        }

        public static TextDisplayResult ResolveSerialDisplay(
            bool enabled,
            bool hide,
            bool useCustom,
            string customSerial,
            string defaultSerialText)
        {
            if (!enabled)
            {
                return new TextDisplayResult(true, defaultSerialText ?? string.Empty);
            }

            if (hide)
            {
                return new TextDisplayResult(false, string.Empty);
            }

            if (useCustom)
            {
                return new TextDisplayResult(true, customSerial ?? string.Empty);
            }

            return new TextDisplayResult(true, defaultSerialText ?? string.Empty);
        }

        public static TextDisplayResult ResolveEndDateDisplay(
            bool enabled,
            bool hide,
            bool useCustom,
            string customEndDate,
            string defaultEndDateText)
        {
            if (!enabled)
            {
                return new TextDisplayResult(true, defaultEndDateText ?? string.Empty);
            }

            if (hide)
            {
                return new TextDisplayResult(false, string.Empty);
            }

            if (useCustom)
            {
                return new TextDisplayResult(true, customEndDate ?? string.Empty);
            }

            return new TextDisplayResult(true, defaultEndDateText ?? string.Empty);
        }
    }
}
