using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using static Nyancat.Win32Api;

namespace Nyancat
{
    public static class ConsoleColorSupport
    {
        public static ColorSupportLevel Level { get; set; } = ColorSupportLevel.None;

        private static Regex _versionRegex = new Regex(@"(\d+\.\d+\.\d+)");
        static ConsoleColorSupport()
        {
            if (Console.IsOutputRedirected) return;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var match = _versionRegex.Match(RuntimeInformation.OSDescription);

                if (!match.Success) return;

                try
                {
                    var version = match.Value;
                    var osVersion = new Version(version);

                    if (osVersion.Major >= 10)
                    {
                        var stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);

                        if (stdOutHandle == INVALID_HANDLE_VALUE) return;

                        if (!GetConsoleMode(stdOutHandle, out var consoleMode)) return;

                        if (!consoleMode.HasFlag(ConsoleBufferModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING)) return;

                        Level |= ColorSupportLevel.Basic;

                        if (osVersion >= new Version("10.0.10586 "))
                        {
                            Level |= ColorSupportLevel.Ansi256;
                        }

                        if (osVersion >= new Version("10.0.14931"))
                        {
                            Level |= ColorSupportLevel.TrueColor;
                        }
                    }
                }
                catch { }
            }
            else
            {
                Level = ColorSupportLevel.TrueColor;
            }
        }
    }

    [Flags]
    public enum ColorSupportLevel : byte
    {
        None = 0,
        Basic = 1,
        Ansi256 = 2,
        TrueColor = 4,
    }
}
