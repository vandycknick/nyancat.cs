using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using McMaster.Extensions.CommandLineUtils;

namespace Nyancat
{
    [Command(Name = "nyancat", FullName = "nyancat", Description = "Terminal nyancat runner")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    class Program
    {

        [Option(Description = "Do not display the timer", ShortName = "n")]
        public bool NoCounter { get; set; } = false;

        [Option(Description = "Do not set the titlebar text", ShortName = "s")]
        public bool NoTitle { get; set; } = false;

        [Option(Description = "Display the requested number of frames, then quit", ShortName = "f")]
        public int Frames { get; set; } = int.MaxValue;

        private bool ShowCounter
        {
            get
            {
                return !NoCounter;
            }
        }

        public void OnExecute()
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

            if (!NoTitle)
            {
                Console.Title = "Nyanyanyanyanyanyanya...";
            }


            using (var device = new GraphicsDevice())
            {
                var playing = true;
                ConsoleUtil.AttachCtrlcSigtermShutdown(() =>
                {
                    playing = false;
                });

                if (min_col == max_col)
                {
                    min_col = (Animation.FRAME_WIDTH - device.Width / 2) / 2;
                    max_col = (Animation.FRAME_WIDTH + device.Width / 2) / 2;
                }

                if (min_row == max_row)
                {
                    min_row = (Animation.FRAME_HEIGHT - (device.Height - 1)) / 2;
                    max_row = (Animation.FRAME_HEIGHT + (device.Height - 1)) / 2;
                }

                Console.Clear();

                var counter = new Stopwatch();
                counter.Start();

                while (playing)
                {
                    foreach (var frame in Animation.Frames)
                    {
                        var frameId = Animation.Frames.IndexOf(frame);

                        device.Fill(' ', colors[',']);

                        for (var row = min_row; row < max_row ; row++)
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
                                colFilled += 2;
                            }

                            device.NewLine();
                        }

                        if (ShowCounter)
                        {
                            var seconds = ((int)counter.Elapsed.TotalSeconds).ToString();
                            int counterWidth = (device.Width - 29 - seconds.Length) / 2;

                            device.MoveTo(device.Height - 1, 0);

                            while (counterWidth > 0)
                            {
                                device.WriteChar(' ', colors[',']);
                                counterWidth--;
                            }

                            var message = $"You have nyaned for {seconds} seconds!";

                            foreach(var ch in message)
                            {
                                device.WriteChar(ch, colors[',']);
                            }
                        }

                        device.SwapBuffers();

                        Thread.Sleep(9);

                        if (Frames != int.MaxValue && Frames > 0)
                        {
                            Frames--;
                            if (Frames <= 0)
                                return;
                        }

                    }
                }
            }

            Console.Clear();
            Console.ResetColor();
        }

        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
