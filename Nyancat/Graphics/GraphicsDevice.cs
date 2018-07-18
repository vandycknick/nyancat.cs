using System;
using System.Text;
using Nyancat.Drivers;

namespace Nyancat.Graphics
{
    struct DrawableChar
    {
        public char Character { get; set; }

        public string Color { get; set; }
    }

    public class GraphicsDevice : IGraphicsDevice
    {
        public int Width => cols;
        public int Height => rows;

        public string Title
        {
            get => ConsoleDriver.Title;
            set => ConsoleDriver.Title = value;
        }

        public bool IsRunning { get; private set; } = true;

        public Action OnResize { private get; set; }

        private int rows, cols;

        private int ccol = 0;
        private int crow = 0;

        private DrawableChar[,] buffer;

        private IConsoleDriver ConsoleDriver;

        public GraphicsDevice(IConsoleDriver driver)
        {
            ConsoleDriver = driver;

            Init();
        }

        private void Init()
        {
            rows = ConsoleDriver.Height;
            cols = ConsoleDriver.Width;

            ConsoleDriver.WindowResize = () =>
            {
                rows = ConsoleDriver.Height;
                cols = ConsoleDriver.Width;

                buffer = new DrawableChar[rows, cols];

                if (OnResize != null)
                    OnResize();
            };

            buffer = new DrawableChar[rows, cols];

            ccol = 0;
            crow = 0;
        }

        public void Exit()
        {
            IsRunning = false;
        }

        public void Fill(char character, string color)
        {
            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    buffer[row, col] = new DrawableChar()
                    {
                        Character = character,
                        Color = color,
                    };
                }
            }
        }

        public void Write(char character, string color)
        {
            if (ccol >= cols || crow >= rows)
                return;

            buffer[crow, ccol] = new DrawableChar()
            {
                Character = character,
                Color = color,
            };

            ccol++;
            if (ccol == cols)
            {
                ccol = 0;
                if (crow + 1 < rows)
                    crow++;
            }
        }

        public void NewLine()
        {
            ccol = 0;
            crow++;

            if (crow == rows)
                crow = 0;
        }

        public void MoveTo(int row, int col)
        {
            ccol = col;
            crow = row;
        }

        private StringBuilder builder = new StringBuilder();
        public void SwapBuffers()
        {
            builder.Clear();
            for (var row = 0; row < rows; row++)
            {
                var previous = buffer[row, 0];

                for (var col = 0; col < cols; col++)
                {
                    var back = buffer[row, col];

                    if (previous.Color != back.Color || col == 0)
                    {
                        builder.Append(back.Color);
                    }

                    builder.Append(back.Character);

                    previous = back;
                }

                if (row + 1 < rows)
                {
                    builder.Append(System.Environment.NewLine);
                }
            }

            ConsoleDriver.Clear();
            ConsoleDriver.Write(builder.ToString());

            ccol = 0;
            crow = 0;

            buffer = new DrawableChar[rows, cols];

            ConsoleDriver.ProcessEvents();
        }

        public void Dispose()
        {
        }
    }
}
