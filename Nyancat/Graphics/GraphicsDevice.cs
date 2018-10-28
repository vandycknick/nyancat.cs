using System;
using System.Drawing;
using System.Text;
using Nyancat.Drivers;
using Nyancat.Graphics.Colors;

namespace Nyancat.Graphics
{
    public class ConsoleChar
    {
        public char Character { get; set; }

        public Color ForeGround { get; set; }

        public Color Background { get; set; }
    }

    public class GraphicsDevice : IGraphicsDevice
    {
        public int Width => _cols;
        public int Height => _rows;

        public string Title
        {
            get => ConsoleDriver.Title;
            set => ConsoleDriver.Title = value;
        }

        public bool IsRunning { get; private set; } = true;

        private int _rows, _cols;

        private ConsoleChar[,] _backBuffer;

        private StringBuilder _frontBuffer = new StringBuilder();

        private ConsoleChar _backGround;

        private IConsoleDriver ConsoleDriver;

        public GraphicsDevice(IConsoleDriver driver)
        {
            ConsoleDriver = driver;

            ResetWindowSize();
            ConsoleDriver.WindowResize = ResetWindowSize;
        }

        private void ResetWindowSize()
        {
            _rows = ConsoleDriver.Height;
            _cols = ConsoleDriver.Width;

            _backBuffer = new ConsoleChar[_rows, _cols];
        }

        public void Clear(Color color)
        {
            Clear(' ', color);
        }

        public void Clear(char character, Color color)
        {
            _backGround = new ConsoleChar()
            {
                Character = character,
                ForeGround = color,
                Background = color,
            };
        }

        public void Draw(ReadOnlySpan<char> message, Position position, Color foreGround, Color background)
        {
            if (position.Row >= Height)
                return;

            for (var i = position.Col; i >= 0 && i < Width; i++)
            {
                var index = i - position.Col;

                if (index < 0 || index >= message.Length)
                    break;

                _backBuffer[position.Row, i] = new ConsoleChar
                {
                    Character = message[i - position.Col],
                    ForeGround = foreGround,
                    Background = background,
                };
            }
        }

        public void Draw(ITexture texture, Position position)
        {
            var buffer = texture.ToBuffer();
            var textureRow = -1;
            for (var row = position.Row; row < Height && row < position.Row + texture.Height; row++)
            {
                textureRow++;
                if (row < 0)
                    continue;

                var textureCol = -1;
                for (var col = position.Col; col < Width && col < position.Col + texture.Width; col++)
                {
                    textureCol++; 
                    if (col >= 0)
                        _backBuffer[row, col] = buffer[textureRow, textureCol];
                }
            }
        }

        public void Render()
        {
            SwapBuffers();
        }

        // TODO: colored string builder is quite nasty, please improve 😅
        private ColoredStringBuilder _colorBuilder = new ColoredStringBuilder();
        public void SwapBuffers()
        {
            _frontBuffer.Clear();
            for (var row = 0; row < _rows; row++)
            {
                var previous = _backBuffer[row, 0] ?? _backGround;

                for (var col = 0; col < _cols; col++)
                {
                    var current = _backBuffer[row, col] ?? _backGround;

                    if (previous.ForeGround != current.ForeGround || col == 0)
                    {
                        _frontBuffer.Append(_colorBuilder.Write(current.Character.ToString(), current.ForeGround, current.Background));
                    }
                    else
                    {
                        _frontBuffer.Append(current.Character);
                    }

                    previous = current;
                }

                if (row + 1 < _rows)
                {
                    _frontBuffer.Append(System.Environment.NewLine);
                }
            }

            ConsoleDriver.Clear();
            ConsoleDriver.Write(_frontBuffer.ToString());
            ConsoleDriver.Write(_colorBuilder.Reset());

            _backBuffer = new ConsoleChar[_rows, _cols];

            ConsoleDriver.ProcessEvents();
        }

        public void Exit()
        {
            IsRunning = false;
        }
    }
}
