using System.Collections.Generic;
using System.Drawing;

namespace Nyancat.Graphics
{
    public class AsciiTexture : ITexture
    {
        public int Width => _frame[0].Length * _scale;

        public int Height => _frame.Length;

        private string[] _frame { get; set; }

        private Dictionary<char, Color> _colorMap;

        private int _scale;

        private ConsoleChar[,] _buffer;

        public AsciiTexture(string[] frame, Dictionary<char, Color> colorMap, int scale = 1)
        {
            _frame = frame;
            _colorMap = colorMap;
            _scale = scale;
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
                                Character = ch,
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
