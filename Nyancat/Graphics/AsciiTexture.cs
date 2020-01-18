using System.Collections.Generic;
using System.Drawing;
using Nyancat.Graphics.Colors;

namespace Nyancat.Graphics
{
    public class AsciiTexture : ITexture
    {
        public int Width { get; private set; }

        public int Height { get; private set; }

        private readonly string[] _frame;
        private readonly Dictionary<char, Color> _colorMap;
        private readonly int _scale;
        private ConsoleChar[,] _buffer;
        private readonly bool _hasColorSupport;

        public AsciiTexture(string[] frame, Dictionary<char, Color> colorMap, int scale = 1)
        {
            _frame = frame;
            _colorMap = colorMap;
            _scale = scale;
            Width = _frame[0].Length * _scale;
            Height = _frame.Length;
            _hasColorSupport = ColorSupport.Detect() != ColorSupportLevel.None;
        }

        public ConsoleChar[,] ToBuffer()
        {
            if (_buffer == null)
            {
                _buffer = new ConsoleChar[Height, Width];

                for (var row = 0; row < Height; row++)
                {
                    for (var col = 0; col < Width; col++)
                    {
                        var ch = _frame[row][col / _scale];
                        for (var i = 0; i < _scale; i++)
                        {
                            _buffer[row, col + i] = new ConsoleChar
                            {
                                Character = _hasColorSupport ? ' ' : ch,
                                ForeGround = _colorMap[ch],
                                Background = _colorMap[ch],
                            };
                            col+=i;
                        }
                    }
                }
            }

            return _buffer;
        }
    }
}
