using System;
using HarmonyLib;
using MU3;
using UnityEngine;
using CardMakerCustomizer.Config;
using CardMakerCustomizer.Logic;
using MelonLoader;

namespace CardMakerCustomizer.Patches
{
    [HarmonyPatch(typeof(QRImage), "createBytes", new Type[] { typeof(int), typeof(int), typeof(uint) })]
    public static class Patch_QRImage_createBytes
    {
        public static void Prefix(ref int cardID, ref int serial, ref uint gameID)
        {
            if (ModConfig.EnableQROverride.Value && ModConfig.CustomSerial.Value >= 0)
            {
                serial = ModConfig.CustomSerial.Value;
            }
        }
    }

    [HarmonyPatch(typeof(QRImage), "encode", new Type[] { typeof(byte[]), typeof(int), typeof(int), typeof(int), typeof(int) })]
    public static class Patch_QRImage_encode
    {
        public static bool Prefix(byte[] src, int cellSize, int margin, int errorCorrectLevel, int version, ref Texture2D __result)
        {
            if (ModConfig.EnableQROverride.Value && ModConfig.UseCustomQRText.Value)
            {
                string customText = ModConfig.CustomQRText.Value;
                if (!StringCompat.IsNullOrWhiteSpace(customText))
                {
                    try
                    {
                        // Use auto-versioning (version = 0) with a buffer large enough for max QR version (40)
                        // This prevents encoding failure and blank white square for strings > 14 bytes
                        int maxSize = QRImage.QRGetSize(40);
                        byte[] grid = new byte[maxSize * maxSize];
                        int matrixSize = QRImage.QREncodeString(grid, errorCorrectLevel, 0, customText.Trim());
                        if (matrixSize > 0)
                        {
                            int targetPixelSize = 105;
                            int dynamicCellSize = Math.Max(1, targetPixelSize / (matrixSize + (margin * 2)));
                            int padded = (matrixSize + (margin * 2)) * dynamicCellSize;
                            Color32[] pixels = new Color32[padded * padded];
                            Color32 white = new Color32(255, 255, 255, 255);
                            Color32 black = new Color32(0, 0, 0, 255);

                            for (int i = 0; i < pixels.Length; i++)
                            {
                                pixels[i] = white;
                            }

                            for (int j = 0; j < matrixSize; j++)
                            {
                                for (int k = 0; k < matrixSize; k++)
                                {
                                    if (grid[(matrixSize - j - 1) * matrixSize + k] != 0)
                                    {
                                        int startX = (k + margin) * dynamicCellSize;
                                        int startY = (j + margin) * dynamicCellSize;
                                        for (int l = 0; l < dynamicCellSize; l++)
                                        {
                                            for (int m = 0; m < dynamicCellSize; m++)
                                            {
                                                pixels[(startY + l) * padded + (startX + m)] = black;
                                            }
                                        }
                                    }
                                }
                            }

                            Texture2D texture2D = new Texture2D(padded, padded, TextureFormat.RGB565, false, true);
                            texture2D.SetPixels32(pixels);
                            texture2D.Apply();
                            __result = texture2D;
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"[CardMakerCustomizer] Failed to encode custom QR string: {ex.Message}");
                    }
                }
            }
            return true;
        }
    }
}
