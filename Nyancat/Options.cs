using System;

namespace Nyancat
{
    public struct Options
    {
        public bool ShowIntro { get; set; }
        public int ShowCounter { get; set; }
        public bool ShowTitle { get; set; }
        public int Frames { get; set; }
        public bool ShowVersion { get; set; }
        public bool ShowHelp { get; set; }

        public static Options Parse(string[] args)
        {
            var showIntro = false;
            var showCounter = 1;
            var showTitle = true;
            var frames = int.MaxValue;
            var showVersion = false;
            var showHelp = false;

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                switch (arg)
                {
                    case "-i":
                    case "--intro":
                        showIntro = true;
                        break;

                    case "-n":
                    case "--no-counter":
                        showCounter = 0;
                        break;

                    case "-t":
                    case "--no-title":
                        showTitle = false;
                        break;

                    case "-f":
                    case "--frames":
                        if (i < args.Length && int.TryParse(args[++i], out var data))
                        {
                            frames = data;
                        }
                        else
                        {
                            throw new Exception($"Missing required value for option '{arg}'.");
                        }
                        break;

                    case "-v":
                    case "--version":
                        showVersion = true;
                        break;

                    case "-?":
                    case "-h":
                    case "--help":
                        showHelp = true;
                        break;

                    default:
                        throw new Exception($"Unkown argument: {arg}");
                }
            }

            return new Options
            {
                ShowIntro = showIntro,
                ShowCounter = showCounter,
                ShowTitle = showTitle,
                Frames = frames,
                ShowVersion = showVersion,
                ShowHelp = showHelp,
            };
        }
    }
}