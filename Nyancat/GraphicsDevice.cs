using System;
using Nyancat.Native;

namespace Nyancat
{
    public class GraphicsDevice : IDisposable
    {
        public void Dispose()
        {
            if (Platform.IsWindows())
            {
                WindowsConsole.RestoreTerminal();
            }
        }
    }
}
