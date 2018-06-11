using System;
using System.Text;
using Nyancat.Drivers;

namespace Nyancat.Graphics
{
    struct CharPoint
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

        private CharPoint[,] frontBuffer;

        private CharPoint[,] backBuffer;

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

                frontBuffer = new CharPoint[rows, cols];
                backBuffer = new CharPoint[rows, cols];

                if (OnResize != null)
                    OnResize();
            };

            frontBuffer = new CharPoint[rows, cols];
            backBuffer = new CharPoint[rows, cols];

            ccol = 0;
            crow = 0;
        }

        public void Exit()
        {
            IsRunning = false;
        }

        public void Clear()
        {
        }

        public void Fill(char rune, string color)
        {
            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    backBuffer[row, col] = new CharPoint()
                    {
                        Character = rune,
                        Color = color,
                    };
                }
            }
        }

        public void Write(char character, string color)
        {
            if (ccol >= cols || crow >= rows)
                return;

            backBuffer[crow, ccol] = new CharPoint()
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
                var previous = backBuffer[row, 0];

                for (var col = 0; col < cols; col++)
                {
                    var back = backBuffer[row, col];

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

            frontBuffer = backBuffer;
            backBuffer = new CharPoint[rows, cols];

            ConsoleDriver.ProcessEvents();
        }

        public void Dispose()
        {
        }
    }
}
