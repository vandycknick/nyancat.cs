using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Nyancat
{
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            var command = new RootCommand
            {
                new Option(
                    new string[] { "-i", "--intro" },
                    "Show the introduction / about information at startup")
                {
                    Argument = new Argument<bool>()
                },
                new Option(
                    new string[] { "-n", "--no-counter" },
                    "Do not display the timer")
                {
                    Argument = new Argument<bool>()
                },
                new Option(
                    new string[] { "-t", "--no-title" },
                    "Do not set the titlebar text")
                {
                    Argument = new Argument<bool>()
                },
                new Option(
                    new string[] { "-f", "--frames" },
                    "Display the requested number of frames, then quit")
                {
                    Argument = new Argument<int>(() => int.MaxValue)
                },
            };

            command.Description = "Terminal nyancat runner";
            command.Handler = CommandHandler.Create<bool, bool, bool, int>((intro, noCounter, noTitle, frames) =>
            {
                var sceneOptions = new NyancatOptions
                {
                    ShowIntro = intro,
                    ShowTitle = !noTitle,
                    ShowCounter = !noCounter,
                    Frames = frames,
                    Sound = false,
                };

                using var game = new Nyancat(sceneOptions);
                game.Run();
            });

            return command.InvokeAsync(args);
        }
    }
}
