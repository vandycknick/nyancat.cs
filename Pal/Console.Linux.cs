using System.Runtime.InteropServices;

namespace System
{
    static class Console
    {
        private enum BOOL : int
        {
            FALSE = 0,
            TRUE = 1,
        }

        public static int WindowWidth = 64;
        public static int WindowHeight = 64;

        public static unsafe string Title
        {
            set
            {
            }
        }

        public static void Write(char c)
        { }

        public static void WriteLine()
        { }

        public static void WriteLine(string msg)
        { }

        public static void Clear()
        { }
    }
}
