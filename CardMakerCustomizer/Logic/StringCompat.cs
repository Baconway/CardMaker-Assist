using System;
using System.Globalization;

namespace CardMakerCustomizer.Logic
{
    public static class StringCompat
    {
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
            {
                return true;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Safe string format that always routes through string.Format(IFormatProvider, string, object[])
        /// avoiding .NET 4.0+ overloads missing in Mono 2.0 / .NET 3.5 mscorlib.
        /// </summary>
        public static string Format(string format, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return format ?? string.Empty;
            }
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }
    }
}
