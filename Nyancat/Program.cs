using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nyancat.Native;

namespace Nyancat
{
    class Program
    {
        const int FRAME_WIDTH = 64;
        const int FRAME_HEIGHT = 64;

        static void Main(string[] args)
        {
            if (Platform.IsWindows())
            {
                WindowsConsole.ConsoleEnableVirtualTerminalProcessing();
            }

            var colors = new Dictionary<char, string>();

            colors[','] = "\x1b[48;5;17m";  /* Blue background */
            colors['.'] = "\x1b[48;5;231m"; /* White stars */
            colors['\''] = "\x1b[48;5;16m"; /* Black border */
            colors['@'] = "\x1b[48;5;230m"; /* Tan poptart */
            colors['$'] = "\x1b[48;5;175m"; /* Pink poptart */
            colors['-'] = "\x1b[48;5;162m"; /* Red poptart */
            colors['>'] = "\x1b[48;5;196m"; /* Red rainbow */
            colors['&'] = "\x1b[48;5;214m"; /* Orange rainbow */
            colors['+'] = "\x1b[48;5;226m"; /* Yellow Rainbow */
            colors['#'] = "\x1b[48;5;118m"; /* Green rainbow */
            colors['='] = "\x1b[48;5;33m";  /* Light blue rainbow */
            colors[';'] = "\x1b[48;5;19m";  /* Dark blue rainbow */
            colors['*'] = "\x1b[48;5;240m"; /* Gray cat face */
            colors['%'] = "\x1b[48;5;175m"; /* Pink cheeks */

            var width = Console.WindowWidth;
            var height = Console.WindowHeight;

            // var first = Animate.Frames.first();

            var frontBuffer = new char[height, width];
            var backBuffer = new char[height, width];

            Action<char, int, int> WriteColor = (char color, int row, int col) =>
            {
                backBuffer[row, col] = color;
            };

            Action SwapBuffers = () =>
            {
                for (var row = 0; row < height; row++)
                {
                    for (var col = 0; col < width; col++)
                    {
                        if (frontBuffer[row, col] != backBuffer[row, col])
                        {
                            Console.CursorTop = row;
                            Console.CursorLeft = col;
                            char color = backBuffer[row, col];
                            Console.Write(colors[color] + " ");
                        }

                    }
                }

                frontBuffer = backBuffer;
                backBuffer = new char[height, width];
            };

            Console.Clear();

            while (true)
            {
                int frameId = 0;
                foreach (var frame in Animation.Frames)
                {

                    for (var row = 0; row < height; row++)
                    {
                        for (var col = 0; col < width; col++)
                        {
                            char color;

                            if (row > 23 && row < 43 && col < 0)
                            {

                                /*
                                 * Generate the rainbow tail.
                                 *
                                 * This is done with a pretty simplistic square wave.
                                 */

                                int mod_x = ((-col + 2) % 16) / 8;

                                if ((frameId / 2) % 2 == 0)
                                {
                                    mod_x = 1 - mod_x;
                                }

                                /*
                                 * Our rainbow, with some padding.
                                 */
                                string rainbow = ",,>>&&&+++###==;;;,,";

                                color = rainbow[mod_x + row - 23];
                                if (color == 0) color = ',';
                            }
                            else if (row >= Animation.FRAME_HEIGHT || col >= Animation.FRAME_WIDTH)
                            {
                                color = ',';
                            }
                            else
                            {
                                color = frame[row][col];
                            }

                            WriteColor(color, row, col);
                        }
                    }

                    SwapBuffers();
                    Thread.Sleep(20);
                }
            }







            // Console.Write("\x1b[31mThis text has a red foreground using SGR.31.\r\n");
            // Console.Write("\x1b[1mThis text has a bright (bold) red foreground using SGR.1 to affect the previous color setting.\r\n");
            // Console.Write("\x1b[mThis text has returned to default colors using SGR.0 implicitly.\r\n");
            // Console.Write("\x1b[34;46mThis text shows the foreground and background change at the same time.\r\n");
            // Console.Write("\x1b[0mThis text has returned to default colors using SGR.0 explicitly.\r\n");
            // Console.Write("\x1b[31;32;33;34;35;36;101;102;103;104;105;106;107mThis text attempts to apply many colors in the same command. Note the colors are applied from left to right so only the right-most option of foreground cyan (SGR.36) and background bright white (SGR.107) is effective.\r\n");

            // Console.Write("\x1b[mThis text has returned to default colors using SGR.0 implicitly.\r\n");
            // Console.Write("\x1b[38;2;203;75;22mHello world \r\n");

            // Console.Write("\x1b[48;5;17m,,,,,,,,,,,,,,,,,,,,,, \r\n");

            // Console.Write("\x1b[39mThis text has restored the foreground color only.\r\n");
            // Console.Write("\x1b[49mThis text has restored the background color only.\r\n");

            Cleanup();
        }

        static void Cleanup()
        {
            if (Platform.IsWindows())
            {
                WindowsConsole.RestoreTerminal();
            }
        }
    }
}
