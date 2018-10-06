using System;

namespace Nyancat.Graphics.Colors
{
    [Flags]
    public enum ColorSupportLevel : byte
    {
        None = 0,
        Basic = 1,
        Ansi256 = 2,
        TrueColor = 4,
    }
}
