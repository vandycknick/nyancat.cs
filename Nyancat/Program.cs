using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mono.Options;
using Nyancat.Graphics;
using Nyancat.Scenes;

namespace Nyancat
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            bool showIntro = false;
            bool showHelp = false;
            bool showVersion = false;
            bool noCounter = false;
            bool noTitle = false;
            bool sound = false;
            int frames = int.MaxValue;

            var options = new OptionSet
            {
                { "i|intro", "Show the introduction / about information at startup", i => showIntro = true },
                { "n|no-counter", "Do not display the timer", n => noCounter = true },
                { "t|no-title", "Do not set the titlebar text", t => noTitle = true },
                { "f|frames=", "Display the requested number of frames, then quit", (int f) => frames = f },
                { "?|h|help", "Show help information", h => showHelp = h != null },
                { "v|version", "Show version information", v => showVersion = v != null },
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
                    Console.WriteLine();
                    Console.WriteLine("Terminal nyancat runner");
                    Console.WriteLine();
                    Console.WriteLine($"Usage: {GetName()} [options]");
                    Console.WriteLine();
                    Console.WriteLine("Options");
                    Console.WriteLine();
                    options.WriteOptionDescriptions(Console.Out);
                    return 0;
                }

                var host = new HostBuilder()
                    .UseConsoleGraphicsHost()
                    .AddScene<IntroScene>(isStartup: showIntro)
                    .AddScene<NyancatScene>(isStartup: !showIntro)
                    .ConfigureServices((context, services) =>
                    {
                        services.Configure<NyancatSceneOptions>(sceneOptions =>
                        {
                            sceneOptions.ShowTitle = !noTitle;
                            sceneOptions.ShowCounter = !noCounter;
                            sceneOptions.Frames = frames;
                            sceneOptions.Sound = sound;
                        });
                    });

                await host.RunConsoleAsync();
                return 0;
            }
            catch (OptionException e)
            {
                Console.WriteLine($"{GetName()}: {GetVersion()}");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine($"Try `{GetName()} --help' for more information.");
                return 1;
            }
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        private static string GetName()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
    }
}
