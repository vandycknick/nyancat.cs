using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nyancat.Graphics;
using Nyancat.Scenes;

namespace Nyancat
{
    [Command(Name = "nyancat", FullName = "nyancat", Description = "Terminal nyancat runner")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    class Program
    {
        [Option(Description = "Show the introduction / about information at startup", ShortName = "i")]
        public bool Intro { get; set; } = false;

        [Option(Description = "Do not display the timer", ShortName = "n")]
        public bool NoCounter { get; set; } = false;

        [Option(Description = "Do not set the titlebar text", ShortName = "t")]
        public bool NoTitle { get; set; } = false;

        [Option(Description = "Display the requested number of frames, then quit", ShortName = "f")]
        public int Frames { get; set; } = int.MaxValue;

        // [Option(Description = "Play sound during animation (only works on windows)", ShortName = "s")]
        public bool Sound { get; set; } = false;

        public void OnExecute()
        {
            var host = new ConsoleGraphicsHostBuilder()
                .AddScene<IntroScene>(isStartup: Intro)
                .AddScene<NyancatScene>(isStartup: !Intro)
                .ConfigureServices((context, services) =>
                {
                    services.Configure<NyancatSceneOptions>(options =>
                    {
                        options.ShowTitle = !NoTitle;
                        options.ShowCounter = !NoCounter;
                        options.Frames = Frames;
                        options.Sound = Sound;
                    });
                })
                .Build();

            host.Run();
        }

        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
