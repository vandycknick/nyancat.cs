using System;
using Mono.Unix;
using Mono.Unix.Native;

namespace Nyancat.Drivers
{
    public class UnixConsoleDriver : IConsoleDriver
    {
        public string Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }


        public int Height => _height;

        private int _height = Console.WindowHeight;

        public int Width => _width;

        private int _width = Console.WindowWidth;

        public Action WindowResize { private get; set; }

        private const string CLEAR_ANSI_CODE = "\x1b[H";

        private UnixSignal SigWinch;

        public UnixConsoleDriver()
        {
            SigWinch = new UnixSignal(Signum.SIGWINCH);
        }

        public void Clear()
        {
            Stdlib.printf(CLEAR_ANSI_CODE);
        }

        public void Write(string buffer)
        {
            Stdlib.printf(buffer);
        }

        public void ProcessEvents()
        {
            if (SigWinch.IsSet)
            {
                _height = Console.WindowHeight;
                _width = Console.WindowWidth;
                WindowResize();
                SigWinch.Reset();
            }
        }

        public void Dispose()
        {
            Clear();
            Console.Clear();
            Console.ResetColor();
        }
    }

}
