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
                case ColorSupport.Legacy:
                    WriteThreeBitColor(color, foreground);
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
            var number = 0;

            if (color == Color.Black) number = 30;
            else if (color == Color.Red) number = 31;
            else if (color == Color.DarkRed) number = 31;
            else if (color == Color.Green) number = 32;
            else if (color == Color.Orange) number = 33;
            else if (color == Color.DarkBlue) number = 34;
            else if (color == Color.Pink) number = 35;
            else if (color == Color.White) number = 37;

            else if (color == Color.Gray) number = 90;
            else if (color == Color.Yellow) number = 93;
            else if (color == Color.Tan) number = 97;
            else if (color == Color.Blue) number = 94;
            else if (color == Color.LightBlue) number = 94;
            else if (color == Color.LightPink) number = 95;

            var cc = 0;
            if (number == 0)
            {
                number = ScaleColorToEightBit(color);
                number = ScaleEightBitToFourBit(number);
                Debug.Assert(number >= 0 && number < 16, "Invalid range for 4-bit color");

                var mod = number < 8 ? (foreground ? 30 : 40) : (foreground ? 82 : 92);
                cc = number + mod;
            }
            else
            {
                cc = number + (foreground ? 0 : 10);
            }

            _console.Write($"\x1b[{cc}m");
        }

        private void WriteThreeBitColor(Color color, bool foreground)
        {
            var code = "";

            if (color == Color.Black) code = "25;40";
            else if (color == Color.Red) code = "5;41";
            else if (color == Color.DarkRed) code = "5;41";
            else if (color == Color.Green) code = "5;42";
            else if (color == Color.Orange) code = "25;43";
            else if (color == Color.DarkBlue) code = "5;44";
            else if (color == Color.Pink) code = "5;45";
            else if (color == Color.White) code = "5;47";
            else if (color == Color.Gray) code = "5;40";
            else if (color == Color.Yellow) code = "5;43";
            else if (color == Color.Tan) code = "5;47";
            else if (color == Color.Blue) code = "25;44";
            else if (color == Color.LightBlue) code = "25;44";
            else if (color == Color.LightPink) code = "25;45";
            else code = "5;40";

            _console.Write($"\x1b[{code}m");
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
