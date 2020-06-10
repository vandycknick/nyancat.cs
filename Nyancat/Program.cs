using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Options;

namespace Nyancat
{
    public class Program
    {
        private const string RAINBOW_TAIL = ",,>>&&&+++###==;;;,,";
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

                var console = new ConsoleGraphics(buffered: true);

                if (showTitle) console.Title = "Nyanyanyanyanyanyanya...";

                console
                    .HideCursor()
                    .ResetCursor()
                    .Flush();

                var defaultSleep = GetDefaultSleep();
                var startTime = Environment.TickCount64;
                var renderTime = startTime;

                if (showIntro) Console.WriteLine("TODO: implement show intro");
                var scene = new NyancatScene(frames: frames, showCounter: showCounter);

                while (running)
                {
                    if (!scene.Update(console.Width, console.Height))
                    {
                        running = false;
                    }

                    scene.Render(ref console);

                    console.Flush();

                    var elapsed = Environment.TickCount64 - renderTime;
                    var sleep = (defaultSleep * 2) - elapsed;
                    Thread.Sleep((int)Math.Clamp(sleep, 0L, defaultSleep));
                    renderTime = Environment.TickCount64;
                }

                console.Dispose();
                _shutdownBlock.Set();

                return 0;
            }
            catch (OptionException e)
            {
                Console.WriteLine($"{GetName()}: {GetVersion()}");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Try `nyancat --help' for more information.");
                return 1;
            }
        }

        private static long GetDefaultSleep()
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
