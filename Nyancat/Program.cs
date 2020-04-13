using System;
using System.Reflection;
using Mono.Options;

namespace Nyancat
{
    public class Program
    {
        public static int Main(string[] args)
        {
            bool showIntro = false;
            bool showHelp = false;
            bool showVersion = false;
            bool noCounter = false;
            bool noTitle = false;
            int frames = int.MaxValue;

            var options = new OptionSet
            {
                { "i|intro", "Show the introduction / about information at startup", i => showIntro = true },
                { "n|no-counter", "Do not display the timer", n => noCounter = true },
                { "t|no-title", "Do not set the titlebar text", t => noTitle = true },
                { "f|frames=", "Display the requested number of frames, then quit", (int f) => frames = f },
                { "v|version", "Show version information", v => showVersion = v != null },
                { "?|h|help", "Show help information", h => showHelp = h != null },
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

                var sceneOptions = new NyancatOptions
                {
                    ShowIntro = showIntro,
                    ShowTitle = !noTitle,
                    ShowCounter = !noCounter,
                    Frames = frames,
                    Sound = false,
                };

                using var game = new Nyancat(sceneOptions);
                game.Run();

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

        private static string GetVersion()
        {
            try
            {
                return typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            }
            catch
            {
                return "v1.3.0";
            }
        }

        private static string GetName() => "nyancat";
    }
}
