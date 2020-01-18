using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Nyancat.Graphics.Colors
{
    public class ColorSupport
    {

        private static Regex _versionRegex = new Regex(@"(\d+\.\d+\.\d+)");
        public static ColorSupportLevel Detect()
        {
            var support = ColorSupportLevel.None;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var match = _versionRegex.Match(RuntimeInformation.OSDescription);

                if (match.Success)
                {
                    try
                    {
                        var version = match.Value;
                        var osVersion = new Version(version);

                        if (osVersion.Major > 10)
                        {
                            support |= ColorSupportLevel.Basic;
                        }

                        if (osVersion >= new Version("10.0.10586 "))
                        {
                            support |= ColorSupportLevel.Ansi256;
                        }

                        if (osVersion >= new Version("10.0.14931"))
                        {
                            support |= ColorSupportLevel.TrueColor;
                        }
                    }
                    catch {}
                }
            }
            else
            {
                support |= ColorSupportLevel.Basic;
                support |= ColorSupportLevel.Ansi256;
                support |= ColorSupportLevel.TrueColor;
            }

            return support;
        }
    }
}
