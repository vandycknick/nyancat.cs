using System.Collections.Generic;
using System.Drawing;
using Nyancat.Graphics;
using Nyancat.Graphics.Colors;

namespace Nyancat.Scenes
{
    public class NyncatTailTexture : ITexture
    {
        public int Width { get; private set; }

        public int Height { get; private set; } = 18;

        private readonly int _id;
        private readonly Dictionary<char, Color> _colorMap;
        private readonly string _rainbow = ",,>>&&&+++###==;;;,,";
        private readonly bool _hasColorSupport;

        public NyncatTailTexture(int id, int width, Dictionary<char, Color> colorMap)
        {
            _id = id;
            Width = width;
            _colorMap = colorMap;
            _hasColorSupport = ColorSupport.Detect() != ColorSupportLevel.None;
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
                            Character = _hasColorSupport ? ' ' : pixel,
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