using System;
using UnityEngine;
using UnityEngine.UI;

namespace CardMakerCustomizer.Logic
{
    public static class HoloHelper
    {
        public static float Clamp01(float val)
        {
            if (val < 0f) return 0f;
            if (val > 1f) return 1f;
            return val;
        }

        public static byte CalculatePixelChannel(byte original, float alpha, bool invert)
        {
            byte baseVal = original;
            if (invert)
            {
                baseVal = (original > 127) ? (byte)0 : (byte)255;
            }

            float clamped = Clamp01(alpha);
            int scaled = (int)Math.Round(baseVal * clamped);
            if (scaled < 0) scaled = 0;
            if (scaled > 255) scaled = 255;
            return (byte)scaled;
        }

        public static void ProcessHoloMaskPixels(Color32[] pixels, float alpha, bool invert)
        {
            if (pixels == null || pixels.Length == 0)
            {
                return;
            }

            float clampedAlpha = Clamp01(alpha);

            for (int i = 0; i < pixels.Length; i++)
            {
                byte origR = pixels[i].r;
                byte origG = pixels[i].g;
                byte origB = pixels[i].b;
                byte origA = pixels[i].a;

                pixels[i].r = CalculatePixelChannel(origR, clampedAlpha, invert);
                pixels[i].g = CalculatePixelChannel(origG, clampedAlpha, invert);
                pixels[i].b = CalculatePixelChannel(origB, clampedAlpha, invert);
                pixels[i].a = CalculatePixelChannel(origA, clampedAlpha, invert);
            }
        }

        public static void ProcessHoloFoilBuffer(byte[] holoBuffer, float heatMultiplier)
        {
            if (holoBuffer == null || holoBuffer.Length == 0)
            {
                return;
            }

            float clampedHeat = Clamp01(heatMultiplier);
            for (int i = 0; i < holoBuffer.Length; i++)
            {
                int scaled = (int)Math.Round(holoBuffer[i] * clampedHeat);
                if (scaled < 0) scaled = 0;
                if (scaled > 255) scaled = 255;
                holoBuffer[i] = (byte)scaled;
            }
        }

        public static void EnsureHoloBufferHasData(byte[] holoBuffer, float heatMultiplier)
        {
            if (holoBuffer == null || holoBuffer.Length == 0)
            {
                return;
            }

            bool hasData = false;
            for (int i = 0; i < holoBuffer.Length; i++)
            {
                if (holoBuffer[i] > 0)
                {
                    hasData = true;
                    break;
                }
            }

            if (!hasData)
            {
                float clampedHeat = Clamp01(heatMultiplier);
                byte fillVal = (byte)Math.Min(255, Math.Max(0, (int)Math.Round(255f * clampedHeat)));
                for (int i = 0; i < holoBuffer.Length; i++)
                {
                    holoBuffer[i] = fillVal;
                }
            }
        }

        public static void ApplyPreviewIntensity(Transform holoPreviewTransform, float intensity)
        {
            if (holoPreviewTransform == null)
            {
                return;
            }

            float clamped = Clamp01(intensity);
            var images = holoPreviewTransform.GetComponentsInChildren<Image>(true);
            if (images != null)
            {
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i] != null)
                    {
                        Color c = images[i].color;
                        c.a = clamped;
                        images[i].color = c;
                    }
                }
            }
        }

        public static bool ShouldForceHolo(bool enableCustomization, bool forceHolo, bool originalIsHolo)
        {
            if (enableCustomization && forceHolo)
            {
                return true;
            }
            return originalIsHolo;
        }
    }
}
