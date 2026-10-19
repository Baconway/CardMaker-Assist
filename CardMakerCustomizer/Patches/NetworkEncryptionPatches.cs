using System;
using System.IO;
using HarmonyLib;
using CardMaker.Common;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.Logic;
using MelonLoader;

namespace CardMakerCustomizer.Patches
{
    [HarmonyPatch(typeof(Cryptography), "getCryptVersion", new Type[] { typeof(int) })]
    public static class Patch_Cryptography_getCryptVersion
    {
        public static bool Prefix(ref int __result)
        {
            if (ModConfig.DisableEncryption.Value)
            {
                __result = 0;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Cryptography), "scramble", new Type[] { typeof(int), typeof(string) })]
    public static class Patch_Cryptography_scramble_id
    {
        public static bool Prefix(string password, ref string __result)
        {
            if (ModConfig.DisableEncryption.Value)
            {
                __result = password;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Cryptography), "scramble", new Type[] { typeof(string), typeof(byte[]), typeof(int) })]
    public static class Patch_Cryptography_scramble_salt
    {
        public static bool Prefix(string password, ref string __result)
        {
            if (ModConfig.DisableEncryption.Value)
            {
                __result = password;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Cryptography), "initialize", new Type[] {
        typeof(int),
        typeof(byte[]),
        typeof(byte[]),
        typeof(byte[]),
        typeof(int)
    })]
    public static class Patch_Cryptography_initialize
    {
        public static void Prefix(ref int cryptVersion)
        {
            if (ModConfig.DisableEncryption.Value)
            {
                cryptVersion = 0;
            }
        }
    }

    [HarmonyPatch(typeof(Cryptography), "encrypt", new Type[] {
        typeof(int),
        typeof(int),
        typeof(byte[]),
        typeof(int),
        typeof(byte[])
    })]
    public static class Patch_Cryptography_encrypt
    {
        public static bool Prefix(int dstSize, byte[] dst, int srcSize, byte[] src)
        {
            if (ModConfig.DisableEncryption.Value)
            {
                if (dst != null && src != null && dst != src)
                {
                    Array.Copy(src, dst, Math.Min(dstSize, srcSize));
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Cryptography), "decrypt", new Type[] {
        typeof(int),
        typeof(int),
        typeof(byte[]),
        typeof(int),
        typeof(byte[])
    })]
    public static class Patch_Cryptography_decrypt
    {
        public static bool Prefix(int dstSize, byte[] dst, int srcSize, byte[] src)
        {
            if (ModConfig.DisableEncryption.Value)
            {
                if (dst != null && src != null && dst != src)
                {
                    Array.Copy(src, dst, Math.Min(dstSize, srcSize));
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PacketBase), "scrambleAPI")]
    public static class Patch_PacketBase_scrambleAPI
    {
        public static bool Prefix(int gameID, INetQuery query, ref string __result)
        {
            if (ModConfig.DisableEncryption.Value)
            {
                __result = (query != null) ? query.URL : string.Empty;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PacketBase), "getPath")]
    public static class Patch_PacketBase_getPath
    {
        public static bool Prefix(out int cryptVersion, int gameID, INetQuery query, string basePath, ref string __result)
        {
            if (ModConfig.DisableEncryption.Value)
            {
                cryptVersion = 0;
                string url = (query != null) ? query.URL : string.Empty;
                __result = (basePath ?? string.Empty) + url;
                return false;
            }
            cryptVersion = 0;
            return true;
        }
    }

    [HarmonyPatch(typeof(NetHttpClient), "descrypt")]
    public static class Patch_NetHttpClient_descrypt
    {
        public static bool Prefix(int size, ref int __result)
        {
            if (ModConfig.DisableEncryption.Value)
            {
                __result = size;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(NetHttpClient), "preprocess")]
    public static class Patch_NetHttpClient_preprocess
    {
        public static void Prefix(ref bool encrypt)
        {
            if (ModConfig.DisableEncryption.Value)
            {
                encrypt = false;
            }
        }
    }

    [HarmonyPatch(typeof(NetConfig), "UseTLS", MethodType.Getter)]
    public static class Patch_NetConfig_UseTLS
    {
        public static bool Prefix(ref bool __result)
        {
            if (ModConfig.DisableTLS.Value)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
