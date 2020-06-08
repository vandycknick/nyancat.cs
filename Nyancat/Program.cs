using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Options;

namespace Nyancat
{
    public class Program
    {
        private const string RAINBOW_TAIL = ",,>>&&&+++###==;;;,,";

        private const string HIDE_CURSOR = "\x1b[?25l";
        private const string RESET_CURSOR = "\x1b[H";
        private const string SHOW_CURSOR = "\x1b[?25h";
        private const string CLEAR_SCREEN = "\x1b[2J";
        private const string RESET_ALL_ATTRIBUTES = "\x1b[0m";

        private static readonly ManualResetEvent _shutdownBlock = new ManualResetEvent(false);
        public static int Main(string[] args)
        {
            var running = true;
            var showIntro = false;
            var showHelp = false;
            var showVersion = false;
            var showCounter = 1;
            var showTitle = true;
            var frames = int.MaxValue;

            var options = new OptionSet
            {
                { "i|intro", "Show the introduction / about information at startup", _ => showIntro = true },
                { "n|no-counter", "Do not display the timer", _ => showCounter = 0 },
                { "t|no-title", "Do not set the titlebar text", _ => showTitle = false },
                { "f|frames=", "Display the requested number of frames, then quit", (int f) => frames = f },
                { "v|version", "Show version information", _ => showVersion = true },
                { "?|h|help", "Show help information", _ => showHelp = true },
            };

            var colors = GetColors();

            try
            {
                var extra = options.Parse(args);

                if (showVersion)
                {
                    Console.WriteLine(GetVersion());
                    return 0;
                }

                if (showHelp)
                {
                    Console.WriteLine($"{GetName()}: {GetVersion()}");
                    Console.WriteLine("  Terminal nyancat runner");
                    Console.WriteLine();
                    Console.WriteLine($"Usage:");
                    Console.WriteLine($"  {GetName()} [options]");
                    Console.WriteLine();
                    Console.WriteLine("Options:");
                    options.WriteOptionDescriptions(Console.Out);
                    return 0;
                }

                AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
                {
                    running = false;
                    _shutdownBlock.WaitOne();
                };

                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    running = false;
                };

                if (showIntro) Console.WriteLine("TODO: implement show intro");
            }
            catch (OptionException e)
            {
                Console.WriteLine($"{GetName()}: {GetVersion()}");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Try `nyancat --help' for more information.");
                return 1;
            }

            int height = 0;
            int width = 0;
            int minCol, maxCol, minRow, maxRow;

            void CalculateRowsAndCols()
            {
                minCol = (NyancatFrames.FRAME_WIDTH - Console.WindowWidth / 2) / 2;
                maxCol = (NyancatFrames.FRAME_WIDTH + Console.WindowWidth / 2) / 2;

                minRow = (NyancatFrames.FRAME_HEIGHT - (Console.WindowHeight - showCounter)) / 2;
                maxRow = (NyancatFrames.FRAME_HEIGHT + (Console.WindowHeight - showCounter)) / 2;
            }

            void CheckWindowSize()
            {
                if (height != Console.WindowHeight || width != Console.WindowWidth)
                {
                    height = Console.LargestWindowHeight;
                    width = Console.WindowWidth;

                    CalculateRowsAndCols();
                }
            }

            CalculateRowsAndCols();
            var console = new ConsoleGraphics(buffered: true);

            if (showTitle) console.Title = "Nyanyanyanyanyanyanya...";

            console
                .HideCursor()
                .ResetCursor()
                .Flush();

            var defaultSleep = GetDefaultSleep();
            var elapsed = new Stopwatch();
            var watch = new Stopwatch();
            watch.Start();
            elapsed.Start();

            while (running)
            {
                for (var fc = 0; fc <= NyancatFrames.TOTAL_FRAMES; fc++)
                {
                    CheckWindowSize();

                    if (!running) break;

                    console.ResetCursor();

                    var frame = NyancatFrames.GetFrame(fc);
                    char lastPixel = char.MinValue;

                    for (var row = minRow; row < maxRow; ++row)
                    {
                        for (var col = minCol; col < maxCol; ++col)
                        {
                            char pixel;
                            if (row > 23 && row < 43 && col < 0)
                            {

                                int mod_x = ((-col + 2) % 16) / 8;
                                if ((fc / 2) % 2 == 0)
                                {
                                    mod_x = 1 - mod_x;
                                }

                                var index = mod_x + row - 23;

                                pixel = ',';
                                if (index < RAINBOW_TAIL.Length)
                                {
                                    pixel = RAINBOW_TAIL[index];
                                }
                            }
                            else if (row < 0 || col < 0 || row >= NyancatFrames.FRAME_HEIGHT || col >= NyancatFrames.FRAME_WIDTH)
                            {
                                pixel = ',';
                            }
                            else
                            {
                                var start = row * NyancatFrames.FRAME_WIDTH + (row * 1);

                                start = start + col;

                                pixel = frame.Slice(start, 1)[0];
                            }

                            if (ConsoleColorSupport.Level == ColorSupportLevel.None)
                            {
                                lastPixel = pixel;
                                console.Write(colors[pixel]);
                                console.Write(colors[pixel]);
                            }
                            else if (pixel != lastPixel && colors.ContainsKey(pixel))
                            {
                                lastPixel = pixel;
                                console.Write(colors[pixel]);

                                console.Write(' ');
                                console.Write(' ');
                            }
                            else
                            {
                                console.Write(' ');
                                console.Write(' ');
                            }
                        }
                    }

                    if (showCounter == 1)
                    {
                        var message = string.Concat("You have nyaned for ", (int)(elapsed.Elapsed.TotalSeconds), " seconds!");
                        var spacesLength = (width - message.Length) / 2;
                        Span<char> spaces = stackalloc char[spacesLength];

                        for (var i = 0; i < spacesLength; i++)
                        {
                            spaces[i] = ConsoleColorSupport.Level == ColorSupportLevel.None ? ',' : ' ';
                        }

                        console.Write(spaces);
                        console.ColorBrightWhite();
                        console.Write(message);
                        console.Write(spaces);
                    }

                    console.WriteLine();
                    console.Flush();

                    lastPixel = char.MinValue;
                    var sleep = (defaultSleep * 2) - watch.Elapsed.Milliseconds;
                    Thread.Sleep(Math.Clamp(sleep, 0, defaultSleep));
                    watch.Restart();
                }
            }

            console.Dispose();
            _shutdownBlock.Set();

            return 0;
        }

        private static Dictionary<char, string> GetColors()
        {
            if (ConsoleColorSupport.Level.HasFlag(ColorSupportLevel.Ansi256))
            {
                return new Dictionary<char, string>
                {
                    { ','  , "\x1b[48;5;17m" },  /* Blue background */
                    { '.'  , "\x1b[48;5;231m" }, /* White stars */
                    { '\'' , "\x1b[48;5;16m" },  /* Black border */
                    { '@'  , "\x1b[48;5;230m" }, /* Tan poptart */
                    { '$'  , "\x1b[48;5;175m" }, /* Pink poptart */
                    { '-'  , "\x1b[48;5;162m" }, /* Red poptart */
                    { '>'  , "\x1b[48;5;196m" }, /* Red rainbow */
                    { '&'  , "\x1b[48;5;214m" }, /* Orange rainbow */
                    { '+'  , "\x1b[48;5;226m" }, /* Yellow Rainbow */
                    { '#'  , "\x1b[48;5;118m" }, /* Green rainbow */
                    { '='  , "\x1b[48;5;33m" },  /* Light blue rainbow */
                    { ';'  , "\x1b[48;5;19m" },  /* Dark blue rainbow */
                    { '*'  , "\x1b[48;5;240m" }, /* Gray cat face */
                    { '%'  , "\x1b[48;5;175m" }, /* Pink cheeks */
                };
            }
            else
            {
                return new Dictionary<char, string>
                {
                    { ','  , "," }, /* Blue background */
                    { '.'  , "." }, /* White stars */
                    { '\'' , "'" }, /* Black border */
                    { '@'  , "@" }, /* Tan poptart */
                    { '$'  , "$" }, /* Pink poptart */
                    { '-'  , "-" }, /* Red poptart */
                    { '>'  , ">" }, /* Red rainbow */
                    { '&'  , "&" }, /* Orange rainbow */
                    { '+'  , "+" }, /* Yellow Rainbow */
                    { '#'  , "#" }, /* Green rainbow */
                    { '='  , "=" }, /* Light blue rainbow */
                    { ';'  , ";" }, /* Dark blue rainbow */
                    { '*'  , "*" }, /* Gray cat face */
                    { '%'  , "%" }, /* Pink cheeks */
                };
            }
        }

        private static int GetDefaultSleep()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return 60;
            }
            else
            {
                return 90;
            }
        }

        private static string GetVersion() => "v1.3.0";

        private static string GetName() => "nyancat";
    }
}
