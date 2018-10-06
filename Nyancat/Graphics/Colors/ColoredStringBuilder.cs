using System.Drawing;

namespace Nyancat.Graphics.Colors
{
    public class ColoredStringBuilder
    {
        private static ColorSupportLevel _ansiColorSupport = ColorSupport.Detect();
        private readonly ColorSupportLevel _colorSupport;

        public ColoredStringBuilder() : this(_ansiColorSupport)
        {
        }

        public ColoredStringBuilder(ColorSupportLevel colorSupport)
        {
            _colorSupport = colorSupport;
        }

        public string Write(string text, Color color)
        {
            return Write(text, color, color);
        }

        public string Write(string text, Color color, Color background)
        {
            if (_colorSupport.HasFlag(ColorSupportLevel.TrueColor))
            {
                var rgb = $"{color.R};{color.G};{color.B}";
                var back = "";
                
                if (background != null && background != Color.Transparent)
                {

                    back = $"\x1b[48;2;{background.R};{background.G};{background.B}m";
                }

                return $"\x1b[38;2;{rgb}m{back}{text}";
            }
            else if (_colorSupport.HasFlag(ColorSupportLevel.Ansi256))
            {
                var ansi = ColorConvert.ToAnsi256(color);
                var back = "";
                
                if (background != null && background != Color.Transparent)
                {
                    back = $"\x1b[48;5;{ColorConvert.ToAnsi256(background)}m";
                }

                return $"\x1b[38;5;{ansi}m{back}{text}";
            }
            else if (_colorSupport.HasFlag(ColorSupportLevel.Basic))
            {
                var basic = ColorConvert.ToAnsi16(color);
                var back = "";
                
                if (background != null && background != Color.Transparent)
                {
                    back = $"\x1b[{ColorConvert.ToAnsi16(background) + 10}m";
                }

                return $"\x1b[{basic}m{back}{text}";
            }

            return $"{text}";
        }

        public string Reset()
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
