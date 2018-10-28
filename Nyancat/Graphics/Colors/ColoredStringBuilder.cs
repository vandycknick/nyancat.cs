using System;
using System.Diagnostics;
using System.Drawing;

namespace Nyancat.Graphics.Colors
{
    public class ColoredStringBuilder
    {
        private static ColorSupportLevel _ansiColorSupport = ColorSupport.Detect();
        private readonly ColorSupportLevel _colorSupport;

        private const char Escape = '\x1b';

        public ColoredStringBuilder() : this(_ansiColorSupport)
        {
        }

        public ColoredStringBuilder(ColorSupportLevel colorSupport)
        {
            _colorSupport = colorSupport;
        }

        public string Write(ReadOnlySpan<char> text, Color color)
        {
            return Write(text, color, color);
        }

        public string Write(ReadOnlySpan<char> text, Color color, Color background)
        {

            if (_colorSupport.HasFlag(ColorSupportLevel.TrueColor))
            {
                return WriteTrueColor(color, true) + WriteTrueColor(background, false) + text.ToString();
            }
            else if (_colorSupport.HasFlag(ColorSupportLevel.Ansi256))
            {
                return WriteAnsi256(color, true) + WriteAnsi16(background, false) + text.ToString();
            }
            else if (_colorSupport.HasFlag(ColorSupportLevel.Basic))
            {
                return WriteAnsi16(color, true) + WriteAnsi16(background, false) + text.ToString();
            }

            return text.ToString();
        }

        private string WriteTrueColor(Color color, bool foreground)
        {
            var start = 7;
            var total = start + StringLength(color.R) + StringLength(color.G) + StringLength(color.B) + 3;
            Span<char> esc = stackalloc char[total];

            esc[0] = Escape;
            esc[1] = '[';
            esc[2] = foreground ? '3' : '4';
            esc[3] = '8';
            esc[4] = ';';
            esc[5] = '2';
            esc[6] = ';';

            var idx = start;

            var sub = esc.Slice(idx);
            idx += WriteByte(sub, color.R);
            sub = esc.Slice(idx);
            sub[0] = ';';
            idx++;

            sub = esc.Slice(idx);
            idx += WriteByte(sub, color.G);
            sub = esc.Slice(idx);
            sub[0] = ';';
            idx++;

            sub = esc.Slice(idx);
            idx += WriteByte(sub, color.B);
            sub = esc.Slice(idx);
            sub[0] = 'm';
            idx++;

            var output = esc.Slice(0, idx);

            return output.ToString();
        }

        private string WriteAnsi256(Color color, bool foreground)
        {
            var start = 7;
            var ansi = ColorConvert.ToAnsi256(color);
            var total = start + StringLength((byte)ansi) + 1;
            Span<char> esc = stackalloc char[total];

            esc[0] = Escape;
            esc[1] = '[';
            esc[2] = foreground ? '3' : '4';
            esc[3] = '8';
            esc[4] = ';';
            esc[5] = '5';
            esc[6] = ';';

            var idx = start;

            var sub = esc.Slice(idx);
            idx += WriteByte(sub, (byte)ansi);
            sub = esc.Slice(idx);
            sub[0] = 'm';
            idx++;

            return esc.Slice(0, idx).ToString();
        }

        private string WriteAnsi16(Color color, bool foreground)
        {
            
            var start = 2;
            var ansi = ColorConvert.ToAnsi16(color);
            ansi = foreground ? ansi : ansi + 10;
            var total = start + StringLength((byte)ansi) + 1;
            Span<char> esc = stackalloc char[total];

            esc[0] = Escape;
            esc[1] = '[';

            var idx = start;
            var sub = esc.Slice(idx);
            idx += WriteByte(sub, (byte) ansi);
            sub[0] = 'm';
            idx++;

            return esc.Slice(0, idx).ToString();
        }

        private int WriteByte(Span<char> buffer, byte value)
        {
            int len = StringLength(value);
            int index = len;

            Debug.Assert(buffer.Length >= len);

            do
            {
                byte div = (byte)(value / 10);
                byte digit = (byte)(value - (10 * div));

                buffer[--index] = (char)('0' + digit);
                value = div;
            } while (value != 0);

            return len;
        }

        private byte StringLength(byte value)
        {
            if (value < 10)
            {
                return 1;
            }

            if (value < 100)
            {
                return 2;
            }

            return 3;
        }

        public ReadOnlySpan<char> Reset()
        {
            if (
                _colorSupport.HasFlag(ColorSupportLevel.TrueColor) ||
                _colorSupport.HasFlag(ColorSupportLevel.Ansi256) ||
                _colorSupport.HasFlag(ColorSupportLevel.Basic)
            )
            {
                return "\x1b[0m";
            }

            return "";
        }
    }
}
