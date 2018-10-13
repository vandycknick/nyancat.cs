using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nyancat.Graphics.Colors
{
    public class ColorSupport
    {
        public static ColorSupportLevel Detect()
        {
            var support = ColorSupportLevel.None;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var strVersion = RuntimeInformation.OSDescription.Split("Microsoft Windows ")
                                    .Where(v => !String.IsNullOrWhiteSpace(v))
                                    .FirstOrDefault();
                var osVersion = new Version(strVersion);

                support |= ColorSupportLevel.Basic;

                if (osVersion >= new Version("10.0.10586 "))
                {
                    support |= ColorSupportLevel.Ansi256;
                }

                if (osVersion >= new Version("10.0.14931"))
                {
                    support |= ColorSupportLevel.TrueColor;
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
