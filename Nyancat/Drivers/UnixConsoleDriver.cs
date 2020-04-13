using System;
using System.Runtime.InteropServices;

namespace Nyancat.Drivers
{
    public class UnixConsoleDriver : IConsoleDriver
    {
        private const string HIDE_CURSOR = "\x1b[?25l";
        private const string RESET_CURSOR = "\x1b[H";
        private const string SHOW_CURSOR = "\x1b[?25h";
        private const string CLEAR_SCREEN = "\x1b[2J";
        private const string RESET_ALL_ATTRIBUTES = "\x1b[0m";

        public string Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }

        public int Height { get; private set; } = Console.WindowHeight;

        public int Width { get; private set; } = Console.WindowWidth;

        public Action WindowResize { private get; set; }

        public UnixConsoleDriver()
        {
            printf(HIDE_CURSOR);
        }

        public void ResetCursor()
        {
            printf(RESET_CURSOR);
        }

        public void Write(ReadOnlySpan<char> text)
        {
            printf(text.ToString());
        }

        public void ProcessEvents()
        {
            var h = Console.WindowHeight;
            var w = Console.WindowWidth;

            if (Height != h || Width != w)
            {
                Height = h;
                Width = w;
                WindowResize();
            }
        }

        public void Dispose()
        {
            printf($"{SHOW_CURSOR}{RESET_ALL_ATTRIBUTES}{RESET_CURSOR}{CLEAR_SCREEN}");
        }

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        private static extern int printf(string format);
    }
}
