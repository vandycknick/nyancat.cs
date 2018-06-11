using System;
using System.Runtime.InteropServices;

namespace Nyancat.Drivers
{
    public class UnixConsoleDriver : IConsoleDriver
    {
        public string Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }

        public int Height => Console.WindowHeight;

        public int Width => Console.WindowWidth;

        public Action WindowResize { private get; set; }

        private const string CLEAR_ANSI_CODE = "\x1b[H";

        public void Clear()
        {
            printf(CLEAR_ANSI_CODE);
        }

        public void Write(string buffer)
        {
            printf(buffer);
        }

        public void ProcessEvents()
        {

        }

        public void Dispose()
        {
            Clear();
            Console.Clear();
            Console.ResetColor();
        }

        [DllImport("libc")]
        extern static void printf(string format);
    }

}
