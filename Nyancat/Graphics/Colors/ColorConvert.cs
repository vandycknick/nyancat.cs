using System;
using System.Drawing;
using System.Globalization;

namespace Nyancat.Graphics.Colors
{
    public static class ColorConvert
    {
        public static int ToAnsi16(Color input)
        {
            // calculate hsv (partially)
            var r = input.R / 255.0;
            var g = input.G / 255.0;
            var b = input.B / 255.0;

            var value = Math.Max(r, Math.Max(g, b)) * 100;

            value = (int)Math.Round(value / 50.0);

            if (value == 0)
            {
                return 30;
            }

            var ansi = 30 +
                (((int)Math.Round(input.B / 255.0) << 2) |
                ((int)Math.Round(input.G / 255.0) << 1) |
                (int)Math.Round(input.R / 255.0));

            if (value == 2)
            {
                ansi += 60;
            }

            return ansi;
        }

        public static int ToAnsi256(Color color)
        {
            // We use the extended greyscale palette here, with the exception of
            // black and white. normal palette only has 4 greyscale shades.
            if (color.R == color.G && color.G == color.B)
            {
                if (color.R < 8)
                {
                    return 16;
                }

                if (color.R > 248)
                {
                    return 231;
                }

                return Convert.ToInt32(Math.Floor((color.R - 8) / 247.0 * 24) + 232);
            }

            var ansi = 16
                + (36 * Math.Floor(color.R / 255.0 * 5))
                + (6 * Math.Floor(color.G / 255.0 * 5))
                + Math.Floor(color.B / 255.0 * 5);

            return Convert.ToInt32(ansi);
        }

        public static Color FromAnsi256(int input)
        {
            if (input >= 232)
            {
                var c = (input - 232) * 10 + 8;
                return Color.FromArgb(255, c, c, c);
            }

            var next = input - 16;
            int rem;
            var r = (int)(Math.Floor(next / 36.0) / 5.0 * 255);
            var g = (int)(Math.Floor((rem = next % 36) / 6.0) / 5.0 * 255);
            var b = (int)(rem % 6 / 5.0 * 255);

            return Color.FromArgb(255, r, g, b);
        }

        public static Color FromHex(ReadOnlySpan<char> hex)
        {
            if (!(hex[0] == '#') || hex.Length > 7)
            {
                throw new ArgumentException(nameof(hex));
            }

            var colorValue = int.Parse(hex.Slice(1), NumberStyles.HexNumber);
            return Color.FromArgb(colorValue);
        }
    }
}
