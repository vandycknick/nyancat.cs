using System;
using System.Buffers;
using System.Runtime.InteropServices;

using static Nyancat.Win32Api;

namespace Nyancat
{
    public struct ConsoleDriver : IDisposable
    {
        private const int DEFAULT_BUFFER_SIZE = 32768; // use 32K default buffer.

        private static char newLine1;
        private static char newLine2;
        private static bool crlf;

        static ConsoleDriver()
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
            StoreInitialConsoleCursorMode();
        }

        private char[] _buffer;
        private int _index;

        private bool _buffered;

        public int Width => Console.WindowWidth;

        public int Height => Console.WindowHeight;

        public ConsoleDriver(bool buffered = true)
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

            // Switch to alternate buffer
            StartAlternateBuffer();
        }

        public void SetTitle(string title)
        {
            Console.Title = title;
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

        public void Write(char value)
        {
            if (_buffer.Length - _index < 1)
            {
                Grow(1);
            }

            _buffer[_index++] = value;
        }

        public void Write(ReadOnlySpan<char> value)
        {
            if (_buffer.Length - _index < value.Length)
            {
                Grow(value.Length);
            }

            var buffer = _buffer.AsSpan(_index, value.Length);
            if (value.TryCopyTo(buffer))
            {
                _index += value.Length;
            }
        }

        public void Write(string value)
        {
            if (_buffer.Length - _index < value.Length)
            {
                Grow(value.Length);
            }

            value.CopyTo(0, _buffer, _index, value.Length);
            _index += value.Length;
        }

        public void WriteLine(string value)
        {
            Write(value);
            WriteLine();
        }

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

        public void StartAlternateBuffer()
        {
            Console.Out.Write("\x1b[?1049h");
            Console.Out.Flush();
        }

        public void RestoreBuffer()
        {
            // Switch out of alternative buffer
            Console.Out.Write("\x1b[?1049h");
            Console.Out.Flush();
        }

        public void Flush()
        {
            if (_index == 0) return;

            Console.Out.Write(_buffer, 0, _index - 1);
            Reset();
        }

        private void Reset() => _index = 0;

        public void Dispose()
        {
            // Windows only
            RestoreConsoleMode();
            RestoreConsoleCursorInfo();

            // Restore Console Buffer
            RestoreBuffer();

            // Return buffer
            if (_buffer is object)
            {
                ArrayPool<char>.Shared.Return(_buffer);
            }

            // Reset
            _buffer = null;
            _index = 0;
        }

        private static ConsoleBufferModes InitialConsoleMode;
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

        private static CONSOLE_CURSOR_INFO InitialConsoleCursorInfo;
        private static bool HasInitialConsoleCursorInfo;

        private static void StoreInitialConsoleCursorMode()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            var stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);

            if (stdOutHandle == INVALID_HANDLE_VALUE) return;

            if (GetConsoleCursorInfo(stdOutHandle, out InitialConsoleCursorInfo))
            {
                HasInitialConsoleCursorInfo = true;
            }
        }

        private static void EnableVTMode()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            if (!HasInitialConsoleMode) return;
            if (HasUpdatedConsoleMode) return;

            if (!InitialConsoleMode.HasFlag(ConsoleBufferModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING))
            {
                var stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                if (stdOutHandle == INVALID_HANDLE_VALUE) return;

                var vtConsoleMode = InitialConsoleMode |
                    ConsoleBufferModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING |
                    ConsoleBufferModes.DISABLE_NEWLINE_AUTO_RETURN;

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

        private static void RestoreConsoleCursorInfo()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            if (!HasInitialConsoleCursorInfo) return;

            var stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            if (stdOutHandle == INVALID_HANDLE_VALUE) return;

            SetConsoleCursorInfo(stdOutHandle, ref InitialConsoleCursorInfo);
        }
    }
}
