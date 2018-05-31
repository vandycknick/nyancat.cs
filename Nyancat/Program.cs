using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            int min_row = -1;
            int max_row = -1;
            int min_col = -1;
            int max_col = -1;

            using (var device = new GraphicsDevice())
            {
                var playing = true;
                ConsoleUtil.AttachCtrlcSigtermShutdown(() =>
                {
                    playing = false;
                });

                var width = device.Width;
                var height = device.Height;

                if (min_col == max_col)
                {
                    min_col = (FRAME_WIDTH - width / 2) / 2;
                    max_col = (FRAME_WIDTH + width / 2) / 2;
                }

                if (min_row == max_row)
                {
                    min_row = (FRAME_HEIGHT - height - 1) / 2;
                    max_row = (FRAME_HEIGHT + height - 1) / 2;
                }

                Console.Clear();

                while (playing)
                {
                    foreach (var frame in Animation.Frames)
                    {
                        var frameId = Animation.Frames.IndexOf(frame);

                        device.Fill(' ',colors[',']);

                        for (var row = min_row; row < max_row; row++)
                        {
                            int colFilled = 0;
                            for (var col = min_col; col < max_col; col++)
                            {
                                char color;
                                
                                
                                if (row > 23 && row < 43 && col < 0)
                                {
                                    /*
                                     * Generate the rainbow tail.
                                     *
                                     * This is done with a prettrow simplistic square wave.
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

                                    var index = mod_x + row - 23;

                                    if (index >= rainbow.Length)
                                        index = 0;

                                    color = rainbow[index];
                                }
                                else if (col < 0 || row < 0 || row >= Animation.FRAME_HEIGHT || col >= Animation.FRAME_WIDTH)
                                {
                                    color = ',';
                                }
                                else
                                {
                                    color = frame[row][col];
                                }

                                device.WriteChar(' ', colors[color]);
                                device.WriteChar(' ', colors[color]);
                                colFilled+=2;
                            }

                            device.NewLine();
                        }

                        device.SwapBuffers();
                        Thread.Sleep(5);
                    }
                }

                Console.Clear();
            }
        }
    }
}
