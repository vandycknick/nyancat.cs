using System;
using System.Collections.Generic;
using Nyancat.Native;

namespace Nyancat
{

    struct CharPoint
    {
        public char Character { get; set; }

        public string Color { get; set; }
    }

    public class GraphicsDevice : IDisposable
    {

        public int Width => cols;
        public int Height => rows;

        private int rows, cols;

        private int ccol = 0;
        private int crow = 0;

        private CharPoint[,] frontBuffer;

        private CharPoint[,] backBuffer;

        public GraphicsDevice()
        {
            if (Platform.IsWindows())
                WindowsConsole.EnableVirtualTerminalProcessing();

            Init();
        }

        private void Init()
        {
            rows = Console.WindowHeight;
            cols = Console.WindowWidth;

            frontBuffer = new CharPoint[rows, cols];
            backBuffer = new CharPoint[rows, cols];

            ccol = 0;
            crow = 0;
        }

        public void WriteChar(char rune, string color)
        {
            if (ccol >= cols || crow >= rows)
                return;

            backBuffer[crow, ccol] = new CharPoint()
            {
                Character = rune,
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

        public void SwapBuffers()
        {
            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    var front = frontBuffer[row, col];
                    var back = backBuffer[row, col];

                    if (front.Character != back.Character || front.Color != back.Color)
                    {
                        Console.CursorTop = row;
                        Console.CursorLeft = col;
                        Console.Write(back.Color + back.Character);
                    }

                }
            }

            ccol = 0;
            crow = 0;

            frontBuffer = backBuffer;
            backBuffer = new CharPoint[rows, cols];
        }

        public void Dispose()
        {
            if (Platform.IsWindows())
            {
                WindowsConsole.RestoreTerminal();
            }
        }
    }
}
