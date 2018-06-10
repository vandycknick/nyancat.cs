using System;

namespace Nyancat.Drivers
{
    public interface IConsoleDriver : IDisposable
    {
        string Title { get; set; }

        int Height { get; }

        int Width { get; }

        void Clear();
        void Write(string buffer);
    }
}
