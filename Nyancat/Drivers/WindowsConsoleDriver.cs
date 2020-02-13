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

        public int Height { get; private set; } = Console.WindowHeight;

        public int Width { get; private set; } = Console.WindowWidth;

        public Action WindowResize { private get; set; }

        private ConsoleCursorInfo OriginalCursorInfo;

        private CharInfo[] SavedOutputBuffer;

        private IntPtr OutputHandle;
        private IntPtr StdInputHandle;
        private IntPtr ScreenBuffer;

        private uint OriginalConsoleInputMode;
        private uint OriginalConsoleOutputMode;

        private uint ConsoleOutputMode
        {
            get
            {
                GetConsoleMode(OutputHandle, out uint v);
                return v;
            }

            set
            {
                SetConsoleMode(OutputHandle, value);
            }
        }

        private uint ConsoleInputMode
        {
            get
            {
                uint v;
                GetConsoleMode(StdInputHandle, out v);
                return v;
            }

            set
            {
                SetConsoleMode(StdInputHandle, value);
            }
        }

        public WindowsConsoleDriver()
        {
            OutputHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            StdInputHandle = GetStdHandle(STD_INPUT_HANDLE);

            if (OutputHandle == INVALID_HANDLE_VALUE)
            {
                throw new Exception("Can't get a std output handle");
            }

            if (StdInputHandle == INVALID_HANDLE_VALUE)
            {
                throw new Exception("Can't get a std input hanlde");
            }

            Init();
        }

        private void Init()
        {
            OriginalConsoleInputMode = ConsoleInputMode;
            OriginalConsoleOutputMode = ConsoleOutputMode;

            ConsoleOutputMode &= ~(uint)ConsoleOutputModeFlags.EnableWrapAtEolOutput;
            ConsoleOutputMode |= (uint)ConsoleOutputModeFlags.EnableVirtualTerminalProcessing;
            ConsoleOutputMode |= (uint)ConsoleOutputModeFlags.DisableNewLineAutoReturn;

            ConsoleInputMode |= (uint)ConsoleInputModeFlags.EnableWindowInput;

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

        public void ResetCursor()
        {
            SetConsoleCursorPosition(ScreenBuffer, new Coord() { X = 0, Y = 0 });
        }

        public void Write(ReadOnlySpan<char> text)
        {
            WriteConsole(ScreenBuffer, text.ToArray(), (uint)text.Length, out var _, null);
        }

        public void ProcessEvents()
        {
            uint eventCount = 0;
            uint eventsRead = 0;

            if (!GetNumberOfConsoleInputEvents(StdInputHandle, out eventCount))
            {
                var err = Marshal.GetLastWin32Error();
                if (err != 0)
                    throw new System.ComponentModel.Win32Exception(err);
            }
            
            if(eventCount > 0)
            {
                var records = new InputRecord[eventCount];

                ReadConsoleInput(StdInputHandle, records, eventCount, out eventsRead);

                if (eventsRead != 0)
                {
                    foreach (var record in records)
                    {
                        ProcessEventRecord(record);
                    }
                }
            }
        }

        private void ProcessEventRecord(InputRecord record)
        {
            if (record.EventType == EventType.WindowBufferSize && WindowResize != null)
            {
                var info = new ConsoleScreenBufferInfo();

                GetConsoleScreenBufferInfo(ScreenBuffer, out info);

                Height = info.srWindow.Bottom - info.srWindow.Top + 1;
                Width = info.srWindow.Right - info.srWindow.Left + 1;

                WindowResize();
            }
        }

        public void Dispose()
        {
            Debug.WriteLine("Disposing windows console driver");

            ConsoleOutputMode = OriginalConsoleOutputMode;
            ConsoleInputMode = OriginalConsoleInputMode;

            if (!SetConsoleActiveScreenBuffer(OutputHandle))
            {
                var err = Marshal.GetLastWin32Error();
                if (err != 0)
                    throw new System.ComponentModel.Win32Exception(err);
            }
        }

        #region Win32ConsoleAPI

        static readonly int STD_INPUT_HANDLE = -10;
        static readonly int STD_OUTPUT_HANDLE = -11;
        // private static readonly int STD_ERROR_HANDLE = -12;

        static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

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
            char[] lpbufer,
            UInt32 NumberOfCharsToWriten,
            out UInt32 lpNumberOfCharsWritten,
            object lpReserved
        );

        [DllImport("kernel32.dll", EntryPoint = "ReadConsoleInputW", CharSet = CharSet.Unicode)]
        public static extern bool ReadConsoleInput(
            IntPtr hConsoleInput,
            [Out] InputRecord[] lpBuffer,
            uint nLength,
            out uint lpNumberOfEventsRead
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
        static extern bool GetNumberOfConsoleInputEvents(IntPtr handle, out uint lpcNumberOfEvents);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleScreenBufferInfo(
            IntPtr hConsoleOutput,
            out ConsoleScreenBufferInfo lpConsoleScreenBufferInfo
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

    #region Win32APIStructures

    [Flags]
    enum ConsoleInputModeFlags : uint
    {
        EnableWindowInput = 8,
        EnableMouseInput = 16,
        EnableQuickEditMode = 64,
        EnableExtendedFlags = 128,
    }

    [Flags]
    enum ConsoleOutputModeFlags : uint
    {
        EnableWrapAtEolOutput = 0x0002,
        EnableVirtualTerminalProcessing = 0x0004,
        DisableNewLineAutoReturn = 0x0008,
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

    [StructLayout(LayoutKind.Sequential)]
    public struct ConsoleScreenBufferInfo
    {
        public Coord dwSize;
        public Coord dwCursorPosition;
        public ushort wAttributes;
        public SmallRect srWindow;
        public Coord dwMaximumWindowSize;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct KeyEventRecord
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.Bool)]
        public bool bKeyDown;
        [FieldOffset(4), MarshalAs(UnmanagedType.U2)]
        public ushort wRepeatCount;
        [FieldOffset(6), MarshalAs(UnmanagedType.U2)]
        public ushort wVirtualKeyCode;
        [FieldOffset(8), MarshalAs(UnmanagedType.U2)]
        public ushort wVirtualScanCode;
        [FieldOffset(10)]
        public char UnicodeChar;
        [FieldOffset(12), MarshalAs(UnmanagedType.U4)]
        public ControlKeyState dwControlKeyState;
    }

    [Flags]
    public enum ButtonState
    {
        Button1Pressed = 1,
        Button2Pressed = 4,
        Button3Pressed = 8,
        Button4Pressed = 16,
        RightmostButtonPressed = 2,

    }

    [Flags]
    public enum ControlKeyState
    {
        RightAltPressed = 1,
        LeftAltPressed = 2,
        RightControlPressed = 4,
        LeftControlPressed = 8,
        ShiftPressed = 16,
        NumlockOn = 32,
        ScrolllockOn = 64,
        CapslockOn = 128,
        EnhancedKey = 256
    }

    [Flags]
    public enum EventFlags
    {
        MouseMoved = 1,
        DoubleClick = 2,
        MouseWheeled = 4,
        MouseHorizontalWheeled = 8
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MouseEventRecord
    {
        [FieldOffset(0)]
        public Coordinate MousePosition;
        [FieldOffset(4)]
        public ButtonState ButtonState;
        [FieldOffset(8)]
        public ControlKeyState ControlKeyState;
        [FieldOffset(12)]
        public EventFlags EventFlags;

        public override string ToString()
        {
            return $"[Mouse({MousePosition},{ButtonState},{ControlKeyState},{EventFlags}";
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Coordinate
    {
        public short X;
        public short Y;

        public Coordinate(short X, short Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public override string ToString() => $"({X},{Y})";
    };

    public struct WindowBufferSizeRecord
    {
        public Coordinate size;

        public WindowBufferSizeRecord(short x, short y)
        {
            this.size = new Coordinate(x, y);
        }

        public override string ToString() => $"[WindowBufferSize{size}";
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MenuEventRecord
    {
        public uint dwCommandId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FocusEventRecord
    {
        public uint bSetFocus;
    }

    public enum EventType : ushort
    {
        Key = 0x01,
        Mouse = 0x02,
        WindowBufferSize = 0x04,
        Menu = 0x08,
        Focus = 0x10,
    }


    [StructLayout(LayoutKind.Explicit)]
    public struct InputRecord
    {
        [FieldOffset(0)]
        public EventType EventType;
        [FieldOffset(4)]
        public KeyEventRecord KeyEvent;
        [FieldOffset(4)]
        public MouseEventRecord MouseEvent;
        [FieldOffset(4)]
        public WindowBufferSizeRecord WindowBufferSizeEvent;
        [FieldOffset(4)]
        public MenuEventRecord MenuEvent;
        [FieldOffset(4)]
        public FocusEventRecord FocusEvent;

        public override string ToString()
        {
            switch (EventType)
            {
                case EventType.Focus:
                    return FocusEvent.ToString();
                case EventType.Key:
                    return KeyEvent.ToString();
                case EventType.Menu:
                    return MenuEvent.ToString();
                case EventType.Mouse:
                    return MouseEvent.ToString();
                case EventType.WindowBufferSize:
                    return WindowBufferSizeEvent.ToString();
                default:
                    return "Unknown event type: " + EventType;
            }
        }
    };

    #endregion
}
