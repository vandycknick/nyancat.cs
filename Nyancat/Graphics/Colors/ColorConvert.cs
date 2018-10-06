using System;
using System.Drawing;
using System.Globalization;
using System.Linq;

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

        static int[] CubeLevels = new int[] { 0x00, 0x5f, 0x87, 0xaf, 0xd7, 0xff };
        static int[] CubeLevels2 = new int[] { 0x00, 0x00, 0x5f, 0x87, 0xaf, 0xd7, 0xff };
        static double[] Snaps = CubeLevels.Zip(CubeLevels2, (x, y) => (x + y) / 2.0).Skip(1).ToArray();
        public static int ToAnsi256(Color color)
        {
            var rgb = new int[] { color.R, color.G, color.B };

            rgb = rgb
                .Select(x => Snaps.Select(s => s < x).Count(p => p == true))
                .ToArray();

            return rgb[0] * 36 + rgb[1] * 6 + rgb[2] + 16;
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
            var b = (int)((rem % 6) / 5.0 * 255);

            return Color.FromArgb(255, r, g, b);
        }

        public static Color FromHex(string hex)
        {
            var colorValue = int.Parse(hex.Replace("#", ""), NumberStyles.HexNumber);
            return Color.FromArgb(colorValue);
        }
    }
}
