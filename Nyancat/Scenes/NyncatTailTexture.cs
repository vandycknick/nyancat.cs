using System.Collections.Generic;
using System.Drawing;
using Nyancat.Graphics;

namespace Nyancat.Scenes
{
    public class NyncatTailTexture : ITexture
    {
        public int Width => _width;

        public int Height => _height;

        private const int _height = 18;
        private int _width;
        private int _id;
        private Dictionary<char, Color> _colorMap;
        private string _rainbow = ",,>>&&&+++###==;;;,,";

        public NyncatTailTexture(int id, int width, Dictionary<char, Color> colorMap)
        {
            _id = id;
            _width = width;
            _colorMap = colorMap;
        }

        public ConsoleChar[,] ToBuffer()
        {
            var buffer = new ConsoleChar[Height, Width];

            for (var row = 0; row < Height; row++)
            {
                for (var col = 0; col < Width; col++)
                {
                    // Generate the rainbow tail.
                    // This is done with a prettrow simplistic square wave.
                    int modX = ((col + 2) % 16) / 8;

                    if (((_id / 2) % 2) == 0)
                    {
                        modX = 1 - modX;
                    }

                    var index = modX + row;
                    if (index < _rainbow.Length && index >= 0)
                    {
                        var pixel = _rainbow[index];
                        buffer[row, col] = new ConsoleChar
                        {
                            Character = pixel,
                            ForeGround = _colorMap[pixel],
                            Background = _colorMap[pixel],
                        };
                    }
                }
            }

            return buffer;
        }
    }
}