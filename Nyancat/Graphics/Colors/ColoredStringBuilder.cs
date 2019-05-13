using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace Nyancat.Graphics.Colors
{
    public sealed class ColoredStringBuilder
    {
        private const char Escape = '\x1b';
        private const string Reset = "\x1b[0m";
        public static ColorSupportLevel AnsiColorSupport = ColorSupport.Detect();
        private readonly ColorSupportLevel _colorSupport;
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public ColoredStringBuilder() : this(AnsiColorSupport)
        {
        }

        public ColoredStringBuilder(ColorSupportLevel colorSupport)
        {
            _colorSupport = colorSupport;
        }

        public ColoredStringBuilder Append(ReadOnlySpan<char> text, Color color, Color background)
        {
            WriteColor(text, color, background);
            return this;
        }

        public ColoredStringBuilder Append(char value, Color color, Color background)
        {
            WriteColor(value.ToString(), color, background);
            return this;
        }

        public ColoredStringBuilder Append(ReadOnlySpan<char> text)
        {
            _stringBuilder.Append(text);
            return this;
        }

        public ColoredStringBuilder Append(char value)
        {
            _stringBuilder.Append(value);
            return this;
        }

        public ColoredStringBuilder Clear()
        {
            _stringBuilder.Clear();
            return this;
        }

        public new string ToString() => _stringBuilder.ToString();

        public ColoredStringBuilder ResetColor()
        {
            if (
                _colorSupport.HasFlag(ColorSupportLevel.TrueColor) ||
                _colorSupport.HasFlag(ColorSupportLevel.Ansi256) ||
                _colorSupport.HasFlag(ColorSupportLevel.Basic)
            )
            {
                Append(Reset);
            }

            return this;
        }

        private void WriteColor(ReadOnlySpan<char> text, Color color, Color background)
        {

            if (_colorSupport.HasFlag(ColorSupportLevel.TrueColor))
            {
                WriteTrueColor(color, true);
                WriteTrueColor(background, false);
            }
            else if (_colorSupport.HasFlag(ColorSupportLevel.Ansi256))
            {
                WriteAnsi256(color, true);
                WriteAnsi256(background, false);
            }
            else if (_colorSupport.HasFlag(ColorSupportLevel.Basic))
            {
                WriteAnsi16(color, true);
                WriteAnsi16(background, false);
            }

            Append(text);
        }

        private void WriteTrueColor(Color color, bool foreground)
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

            Append(output);
        }

        private void WriteAnsi256(Color color, bool foreground)
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

            Append(esc.Slice(0, idx));
        }

        private void WriteAnsi16(Color color, bool foreground)
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
            idx += WriteByte(sub, (byte)ansi);
            sub[0] = 'm';
            idx++;

            Append(esc.Slice(0, idx));
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
    }
}
