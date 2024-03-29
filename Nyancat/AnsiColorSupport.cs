using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Nyancat
{
    internal static class AnsiColorSupport
    {
        private static Regex _versionRegex = new Regex("^Microsoft Windows (?'major'[0-9]*).(?'minor'[0-9]*).(?'build'[0-9]*)\\s*$");

        /// <summary>
        /// Detect colort support
        ///
        /// A mish mash from:
        /// - https://github.com/willmcgugan/rich/blob/f0c29052c22d1e49579956a9207324d9072beed7/rich/console.py#L391
        /// - https://github.com/termstandard/colors
        /// </summary>
        public static ColorSupport Detect(bool supportsAnsi)
        {
            if (Environment.GetEnvironmentVariables().Contains("NO_COLOR"))
            {
                // No colors supported
                return ColorSupport.NoColors;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!supportsAnsi)
                {
                    return ColorSupport.NoColors;
                }

                var match = _versionRegex.Match(RuntimeInformation.OSDescription);
                if (match.Success && int.TryParse(match.Groups["major"].Value, out var major))
                {
                    if (major > 10)
                    {
                        return ColorSupport.TrueColor;
                    }

                    if (major == 10 && int.TryParse(match.Groups["build"].Value, out var build) && build >= 15063)
                    {
                        return ColorSupport.TrueColor;
                    }
                }

                return ColorSupport.Standard;
            }
            else
            {
                var colorTerm = (Environment.GetEnvironmentVariable("COLORTERM") ?? "").ToLower();
                var colorSupport = colorTerm switch
                {
                    "truecolor" => ColorSupport.TrueColor,
                    "24bit" => ColorSupport.TrueColor,
                    _ => DetectSupportFromTerm(),
                };

                return colorSupport;
            }
        }

        private static ColorSupport DetectSupportFromTerm()
        {
            var term = (Environment.GetEnvironmentVariable("TERM") ?? "").ToLower();

            return term switch
            {
                string t when t.StartsWith("xterm") => ColorSupport.EightBit, // 256-color, spaces
                string t when t.StartsWith("toaru") => ColorSupport.EightBit, // Emulates xterm
                string t when t.StartsWith("linux") => ColorSupport.Legacy, // spaces and blink attribute
                "st-256color" => ColorSupport.EightBit, // suckless simple terminal is xterm-256color compatible
                _ => ColorSupport.Standard,
            };
        }
    }

    [Flags]
    public enum ColorSupport : byte
    {

        /// <summary>
        /// No colors.
        /// </summary>
        NoColors = 0,

        /// <summary>
        /// Legacy, 3-bit mode.
        /// </summary>
        Legacy = 1,

        /// <summary>
        /// Standard, 4-bit mode.
        /// </summary>
        Standard = 2,

        /// <summary>
        /// 8-bit mode.
        /// </summary>
        EightBit = 4,

        /// <summary>
        /// 24-bit mode.
        /// </summary>
        TrueColor = 8,
    }

}
