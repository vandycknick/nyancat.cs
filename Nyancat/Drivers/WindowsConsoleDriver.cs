using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Nyancat.Drivers
{
    public class WindowsConsoleDriver : IConsoleDriver
    {
        public string Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }

        public int Height => Console.WindowHeight;

        public int Width => Console.WindowWidth;

        private ConsoleCursorInfo OriginalCursorInfo;

        private CharInfo[] SavedOutputBuffer;

        private IntPtr OutputHandle;

        private IntPtr ScreenBuffer;

        private uint OriginalConsoleOutputMode;

        private uint ConsoleOutputMode
        {
            get
            {
                uint v;
                GetConsoleMode(OutputHandle, out v);
                return v;
            }

            set
            {
                SetConsoleMode(OutputHandle, value);
            }
        }

        public WindowsConsoleDriver()
        {
            OutputHandle = GetStdHandle(STD_OUTPUT_HANDLE);

            if (OutputHandle == INVALID_HANDLE_VALUE)
            {
                throw new Exception("Can't get a std output handle");
            }

            Init();
        }

        private void Init()
        {
            OriginalConsoleOutputMode = ConsoleOutputMode;

            ConsoleOutputMode |= (uint)ConsoleOutputModeFlags.EnableVirtualTerminalProcessing;
            ConsoleOutputMode |= (uint)ConsoleOutputModeFlags.EnableNewLineAutoReturn;

            ScreenBuffer = CreateConsoleScreenBuffer(
               DesiredAccess.GenericRead | DesiredAccess.GenericWrite,
               ShareMode.FileShareRead | ShareMode.FileShareWrite,
               IntPtr.Zero,
               1,
               IntPtr.Zero
           );

            if (ScreenBuffer == INVALID_HANDLE_VALUE)
            {
                var err = Marshal.GetLastWin32Error();

                if (err != 0)
                    throw new System.ComponentModel.Win32Exception(err);
            }

            if (!SetConsoleActiveScreenBuffer(ScreenBuffer))
            {
                var err = Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(err);
            }

            SavedOutputBuffer = new CharInfo[Height * Width];

            var coords = new Coord()
            {
                X = (short)Console.WindowWidth,
                Y = (short)Console.WindowHeight
            };

            var window = new SmallRect()
            {
                Top = 0,
                Left = 0,
                Right = (short)Console.WindowWidth,
                Bottom = (short)Console.WindowHeight
            };

            ReadConsoleOutput(OutputHandle, SavedOutputBuffer, coords, new Coord() { X = 0, Y = 0 }, ref window);

            var cursorInfo = new ConsoleCursorInfo();

            if (!GetConsoleCursorInfo(ScreenBuffer, out cursorInfo))
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }

            OriginalCursorInfo = cursorInfo;

            var newCursorInfo = new ConsoleCursorInfo()
            {
                Size = OriginalCursorInfo.Size,
                Visible = false,
            };

            if (!SetConsoleCursorInfo(ScreenBuffer, ref newCursorInfo))
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public void Clear()
        {
        }

        public void Write(string buffer)
        {
            uint charsWritten;
            SetConsoleCursorPosition(ScreenBuffer, new Coord() { X = 0, Y = 0 });
            WriteConsole(ScreenBuffer, buffer, (uint)buffer.Length, out charsWritten, null);
        }
        public void Dispose()
        {
            Debug.WriteLine("Disposing windows console driver");

            ConsoleOutputMode = OriginalConsoleOutputMode;

            if (!SetConsoleActiveScreenBuffer(OutputHandle))
            {
                var err = Marshal.GetLastWin32Error();
                if (err != 0)
                    throw new System.ComponentModel.Win32Exception(err);
            }
        }

        #region Win32ConsoleAPI

        private static readonly int STD_OUTPUT_HANDLE = -11;
        // static readonly int STD_INPUT_HANDLE = -10;
        // static readonly int STD_ERROR_HANDLE = -12;

        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCursorPosition(IntPtr hConsoleOutput, Coord dwCursorPosition);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateConsoleScreenBuffer(
            DesiredAccess dwDesiredAccess,
            ShareMode dwShareMode,
            IntPtr secutiryAttributes,
            UInt32 flags,
            IntPtr screenBufferData
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleActiveScreenBuffer(IntPtr Handle);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool WriteConsole(
            IntPtr hConsoleOutput,
            String lpbufer,
            UInt32 NumberOfCharsToWriten,
            out UInt32 lpNumberOfCharsWritten,
            object lpReserved
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool ReadConsoleOutput(
            IntPtr hConsoleOutput,
            [Out] CharInfo[] lpBuffer,
            Coord dwBufferSize,
            Coord dwBufferCoord,
            ref SmallRect lpReadRegion
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleCursorInfo(
            IntPtr hConsoleOutput,
            out ConsoleCursorInfo lpConsoleCursorInfo
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleCursorInfo(
            IntPtr hConsoleOutput,
            [In] ref ConsoleCursorInfo lpConsoleCursorInfo
        );

        #endregion
    }

    #region Win32APIStructs

    [Flags]
    enum ConsoleInputModeFlags : uint
    {
        EnableMouseInput = 16,
        EnableQuickEditMode = 64,
        EnableExtendedFlags = 128,
    }

    [Flags]
    enum ConsoleOutputModeFlags : uint
    {
        EnableVirtualTerminalProcessing = 4,
        EnableNewLineAutoReturn = 8,
    }

    [Flags]
    enum DesiredAccess : uint
    {
        GenericRead = 2147483648,
        GenericWrite = 1073741824,
    }

    [Flags]
    enum ShareMode : uint
    {
        FileShareRead = 1,
        FileShareWrite = 2,
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]

    public struct CharInfo
    {
        [FieldOffset(0)] public char Char;

        [FieldOffset(2)] public ushort Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Coord
    {
        public short X;
        public short Y;

        public override string ToString() => $"({X},{Y})";
    };


    [StructLayout(LayoutKind.Sequential)]
    public struct SmallRect
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;

        public override string ToString() => $"Left={Left},Top={Top},Right={Right},Bottom={Bottom}";
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ConsoleCursorInfo
    {
        public uint Size;
        public bool Visible;
    }
    #endregion
}
