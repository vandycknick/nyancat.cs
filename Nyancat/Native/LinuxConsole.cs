using System.Runtime.InteropServices;

namespace Nyancat.Native
{

    public static class LinuxConsole
    {
        [DllImport("libc")]
        extern static void printf (string format);

        public static void Write(string chars)
        {
            printf(chars);
        }
    }

}