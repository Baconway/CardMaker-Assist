using System;
using UnityEngine;

namespace CardMakerCustomizer.UI
{
    public static class GUICompat
    {
        public static readonly GUILayoutOption[] None = new GUILayoutOption[0];

        public static Rect Window(int id, Rect screenRect, GUI.WindowFunction func, string text)
        {
            return GUILayout.Window(id, screenRect, func, text, None);
        }

        public static void BeginVertical()
        {
            GUILayout.BeginVertical(None);
        }

        public static void EndVertical()
        {
            GUILayout.EndVertical();
        }

        public static void BeginHorizontal()
        {
            GUILayout.BeginHorizontal(None);
        }

        public static void EndHorizontal()
        {
            GUILayout.EndHorizontal();
        }

        public static bool Toggle(bool value, string text)
        {
            return GUILayout.Toggle(value, text, None);
        }

        public static string TextField(string text)
        {
            return GUILayout.TextField(text ?? string.Empty, None);
        }

        public static void Label(string text)
        {
            GUILayout.Label(text ?? string.Empty, None);
        }

        public static void Label(string text, float width)
        {
            GUILayout.Label(text ?? string.Empty, new GUILayoutOption[] { GUILayout.Width(width) });
        }

        public static void Label(string text, GUILayoutOption opt)
        {
            GUILayout.Label(text ?? string.Empty, new GUILayoutOption[] { opt });
        }

        public static bool Button(string text, float height)
        {
            return GUILayout.Button(text ?? string.Empty, new GUILayoutOption[] { GUILayout.Height(height) });
        }

        public static int Toolbar(int selected, string[] texts, float height)
        {
            return GUILayout.Toolbar(selected, texts, new GUILayoutOption[] { GUILayout.Height(height) });
        }

        public static void Space(float pixels)
        {
            GUILayout.Space(pixels);
        }

        public static void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }

        public static float HorizontalSlider(float value, float leftValue, float rightValue)
        {
            return GUILayout.HorizontalSlider(value, leftValue, rightValue, None);
        }
    }
}
