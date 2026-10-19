using System;

namespace CardMakerCustomizer.Logic
{
    public static class NetworkEncryptionHelper
    {
        public static int ResolveCryptVersion(int originalVersion, bool disableEncryption)
        {
            if (disableEncryption)
            {
                return 0;
            }

            return originalVersion;
        }

        public static string ResolveURL(string url, bool disableEncryption, Func<string, string> scrambler = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            if (disableEncryption || scrambler == null)
            {
                return url;
            }

            return scrambler(url);
        }

        public static string ResolveUserAgent(string url, long userId, string clientId, bool disableEncryption, Func<string, string> scrambler = null)
        {
            string apiPart = ResolveURL(url, disableEncryption, scrambler);
            if (userId <= 0L)
            {
                return $"{apiPart}#{clientId ?? string.Empty}";
            }

            return $"{apiPart}#{userId}";
        }

        public static bool ShouldEncryptPayload(bool disableEncryption, bool originalEncrypt)
        {
            if (disableEncryption)
            {
                return false;
            }

            return originalEncrypt;
        }

        public static bool ShouldBypassTLS(bool disableTLS, bool originalUseTLS)
        {
            if (disableTLS)
            {
                return false; // Plain TCP/HTTP
            }

            return originalUseTLS;
        }

        public static string BuildRequestPath(string basePath, string url, bool disableEncryption, Func<string, string> scrambler = null)
        {
            string baseP = basePath ?? string.Empty;
            string apiPart = ResolveURL(url, disableEncryption, scrambler);
            return baseP + apiPart;
        }
    }
}
