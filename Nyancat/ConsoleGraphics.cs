using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nyancat
{
    public struct ConsoleGraphics : IDisposable
    {
        private const int DEFAULT_BUFFER_SIZE = 32768; // use 32K default buffer.

        private static char newLine1;
        private static char newLine2;
        private static bool crlf;

        static ConsoleGraphics()
        {
            var newLine = Environment.NewLine.ToCharArray();
            if (newLine.Length == 1)
            {
                // cr or lf
                newLine1 = newLine[0];
                crlf = false;
            }
            else
            {
                // crlf(windows)
                newLine1 = newLine[0];
                newLine2 = newLine[1];
                crlf = true;
            }

            // This is only needed for windows
            StoreInitialConsoleMode();
        }

        private char[] _buffer;
        private int _index;

        private bool _buffered;

        public string Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }

        public ConsoleGraphics(bool buffered = true)
        {
            _buffered = buffered;
            _index = 0;

            if (_buffered)
            {
                _buffer = ArrayPool<char>.Shared.Rent(DEFAULT_BUFFER_SIZE);
            }
            else
            {
                throw new Exception("Unbuffered not implemented");
            }

            // Windows only
            EnableVTMode();
        }

        private void Grow(int sizeHint)
        {
            var nextSize = _buffer.Length * 2;
            if (sizeHint != 0)
            {
                nextSize = Math.Max(nextSize, _index + sizeHint);
            }

            var newBuffer = ArrayPool<char>.Shared.Rent(nextSize);

            _buffer.CopyTo(newBuffer, 0);
            ArrayPool<char>.Shared.Return(_buffer);

            _buffer = newBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char value)
        {
            if (_buffer.Length - _index < 1)
            {
                Grow(1);
            }

            _buffer[_index++] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string value)
        {
            if (_buffer.Length - _index < value.Length)
            {
                Grow(value.Length);
            }

            value.CopyTo(0, _buffer, _index, value.Length);
            _index += value.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLine()
        {
            if (crlf)
            {
                if (_buffer.Length - _index < 2) Grow(2);
                _buffer[_index] = newLine1;
                _buffer[_index + 1] = newLine2;
                _index += 2;
            }
            else
            {
                if (_buffer.Length - _index < 1) Grow(1);
                _buffer[_index] = newLine1;
                _index += 1;
            }
        }

        public void Flush()
        {
            Console.Out.Write(_buffer, 0, _index - 1);
            Reset();
        }

        private void Reset()
        {
            _index = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_buffer != null)
            {
                ArrayPool<char>.Shared.Return(_buffer);
            }

            _buffer = null;
            _index = 0;

            // Windows only
            RestoreConsoleMode();
        }

        private static ConsoleOutputModeFlags InitialConsoleMode;
        private static bool HasInitialConsoleMode;
        private static bool HasUpdatedConsoleMode;

        private static void StoreInitialConsoleMode()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            var stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);

            if (stdOutHandle == INVALID_HANDLE_VALUE) return;

            if (GetConsoleMode(stdOutHandle, out InitialConsoleMode))
            {
                HasInitialConsoleMode = true;
            }
        }

        private static void EnableVTMode()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            if (!HasInitialConsoleMode) return;
            if (HasUpdatedConsoleMode) return;

            if (!InitialConsoleMode.HasFlag(ConsoleOutputModeFlags.ENABLE_VIRTUAL_TERMINAL_PROCESSING))
            {
                var stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                if (stdOutHandle == INVALID_HANDLE_VALUE) return;

                var vtConsoleMode = InitialConsoleMode |
                    ConsoleOutputModeFlags.ENABLE_VIRTUAL_TERMINAL_PROCESSING |
                    ConsoleOutputModeFlags.DISABLE_NEWLINE_AUTO_RETURN;

                if (SetConsoleMode(stdOutHandle, vtConsoleMode))
                {
                    HasUpdatedConsoleMode = true;
                }
            }
        }

        private static void RestoreConsoleMode()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            if (!HasUpdatedConsoleMode) return;

            var stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            if (stdOutHandle == INVALID_HANDLE_VALUE) return;

            SetConsoleMode(stdOutHandle, InitialConsoleMode);
        }


        #region Win32ConsoleAPI
        private static readonly int STD_OUTPUT_HANDLE = -11;
        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleOutputModeFlags lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleOutputModeFlags dwMode);

        [Flags]
        private enum ConsoleOutputModeFlags : uint
        {
            ENABLE_PROCESSED_OUTPUT = 0x0001,
            ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
            ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
            DISABLE_NEWLINE_AUTO_RETURN = 0x0008,
            ENABLE_LVB_GRID_WORLDWIDE = 0x0010
        }
        #endregion
    }
}