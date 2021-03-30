using System.Diagnostics;

namespace Nyancat
{
    public unsafe partial struct AnsiConsole
    {
        public bool SupportsColors() =>
            _support != ColorSupport.NoColors;

        public void WriteColor(string message, Color color, bool foreground = false)
        {
            switch (_support)
            {
                case ColorSupport.TrueColor:
                    WriteTrueColor(color, foreground);
                    break;
                case ColorSupport.EightBit:
                    WriteEightBitColor(color, foreground);
                    break;
                case ColorSupport.Standard:
                    WriteFourBitColor(color, foreground);
                    break;
                default:
                    break;
            }

            _console.Write(message);
        }

        public void WriteColor(char ch, Color color, bool foreground = false)
        {
            switch (_support)
            {
                case ColorSupport.TrueColor:
                    WriteTrueColor(color, foreground);
                    break;
                case ColorSupport.EightBit:
                    WriteEightBitColor(color, foreground);
                    break;
                case ColorSupport.Standard:
                    WriteFourBitColor(color, foreground);
                    break;
                default:
                    break;
            }

            _console.Write(ch);
        }

        private void WriteTrueColor(Color color, bool foreground)
        {
            var mod = foreground ? (byte)38 : (byte)48;
            _console.Write($"\x1b[{mod};2;{color.R};{color.G};{color.B}m");
        }

        private void WriteEightBitColor(Color color, bool foreground)
        {
            var number = ScaleColorToEightBit(color);
            Debug.Assert(number >= 0 && number <= 255, "Invalid range for 8-bit color");

            var mod = foreground ? (byte)38 : (byte)48;
            _console.Write($"\x1b[{mod};5;{number}m");
        }

        private void WriteFourBitColor(Color color, bool foreground)
        {
            var number = ScaleColorToEightBit(color);
            number = ScaleEightBitToFourBit(number);

            Debug.Assert(number >= 0 && number < 16, "Invalid range for 4-bit color");

            var mod = number < 8 ? (foreground ? 30 : 40) : (foreground ? 82 : 92);
            _console.Write($"\x1b[{number + mod}m");
        }

        private static int ColorTo6Cube(int color)
        {
            if (color < 48)
                return 0;
            if (color < 114)
                return 1;
            return ((color - 35) / 40);
        }

        private static int ColorDistSQ(int R, int G, int B, int r, int g, int b) =>
            ((R - r) * (R - r) + (G - g) * (G - g) + (B - b) * (B - b));

        // Stolen from TMUX: https://github.com/tmux/tmux/blob/dae2868d1227b95fd076fb4a5efa6256c7245943/colour.c#L57
        private int ScaleColorToEightBit(Color color)
        {
            /* Map RGB to 6x6x6 cube. */
            var qr = ColorTo6Cube(color.R);
            var cr = _cubeLevels[qr];

            var qg = ColorTo6Cube(color.G);
            var cg = _cubeLevels[qg];
            var qb = ColorTo6Cube(color.B);
            var cb = _cubeLevels[qb];


            /* If we have hit the color exactly, return early. */
            if (cr == color.R && cg == color.G && cb == color.B)
                return ((16 + (36 * qr) + (6 * qg) + qb));


            // Work out the closest grey (average of RGB).
            var greyAvg = (color.R + color.G + color.B) / 3;
            int greyIdx = 0;

            if (greyAvg > 238)
            {
                greyIdx = 23;
            }
            else
            {
                greyIdx = (greyAvg - 3) / 10;
            }

            var grey = 8 + (10 * greyIdx);

            // Is grey or 6x6x6 color closest?
            var idx = 0;
            var d = ColorDistSQ(cr, cg, cb, color.R, color.G, color.B);

            if (ColorDistSQ(grey, grey, grey, color.R, color.G, color.B) < d)
            {
                idx = 232 + greyIdx;
            }
            else
            {
                idx = 16 + (36 * qr) + (6 * qg) + qb;
            }

            return idx;
        }

        private int ScaleEightBitToFourBit(int number) =>
            _table[number & 0xff];
    }
}
