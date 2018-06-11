using System;

namespace Nyancat.Drivers
{
    public interface IConsoleDriver : IDisposable
    {
        string Title { get; set; }

        int Height { get; }

        int Width { get; }

        Action WindowResize { set; }

        void Clear();
        void Write(string buffer);
        void ProcessEvents();
    }
}
