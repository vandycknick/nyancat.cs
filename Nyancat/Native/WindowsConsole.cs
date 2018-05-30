using System;
using System.Runtime.InteropServices;

namespace Nyancat.Native
{
    [Flags]
    enum ConsoleModeInputFlags : uint
    {
        EnableMouseInput = 16,
        EnableQuickEditMode = 64,
        EnableExtendedFlags = 128,
    }

    [Flags]
    enum ConsoleModeOutputFlags : uint
    {
        EnableVirtualTerminalProcessing = 4,
        EnableNewLineAutoReturn = 8,
    }

    public static class WindowsConsole
    {
        static readonly int STD_OUTPUT_HANDLE = -11;
        // static readonly int STD_INPUT_HANDLE = -10;
        // static readonly int STD_ERROR_HANDLE = -12;

        static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        static uint OriginalConsoleMode = 0;

        public static bool ConsoleEnableVirtualTerminalProcessing()
        {
            var stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);

            if (stdOutHandle == INVALID_HANDLE_VALUE)
            {
                return false;
            }

            if (!GetConsoleMode(stdOutHandle, out OriginalConsoleMode))
            {
                return false;
            }

            uint newConsoleMode = OriginalConsoleMode | (uint)ConsoleModeOutputFlags.EnableVirtualTerminalProcessing
                                    | (uint) ConsoleModeOutputFlags.EnableNewLineAutoReturn;

            if (!SetConsoleMode(stdOutHandle, newConsoleMode))
            {
                return false;
            }

            return true;
        }

        public static bool RestoreTerminal()
        {
            var stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);

            if (stdOutHandle == INVALID_HANDLE_VALUE)
            {
                return false;
            }

            if (!SetConsoleMode(stdOutHandle, OriginalConsoleMode))
            {
                return false;
            }

            return true;
        }
    }
}
