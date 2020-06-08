using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static Nyancat.Win32Api;

namespace Nyancat
{
    public struct ConsoleGraphics : IDisposable
    {
        private const int DEFAULT_BUFFER_SIZE = 32768; // use 32K default buffer.

        private const string HIDE_CURSOR = "\x1b[?25l";
        private const string RESET_CURSOR = "\x1b[H";
        private const string SHOW_CURSOR = "\x1b[?25h";
        private const string CLEAR_SCREEN = "\x1b[2J";
        private const string RESET_ALL_ATTRIBUTES = "\x1b[0m";

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
            _index = 0;
        }

        public ConsoleGraphics HideCursor()
        {
            if (SupportsAnsi()) Write(HIDE_CURSOR);
            return this;
        }

        public ConsoleGraphics ResetCursor()
        {
            if (SupportsAnsi())
            {
                Write(RESET_CURSOR);
            }
            else
            {
                Console.CursorTop = 0;
                Console.CursorLeft = 0;
            }
            return this;
        }

        public ConsoleGraphics ColorBrightWhite()
        {
            if (SupportsAnsi()) Write("\x1b[1;37m");
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Boolean SupportsAnsi() => ConsoleColorSupport.Level != ColorSupportLevel.None;

        public void Dispose()
        {
            // Windows only
            RestoreConsoleMode();

            // Restore Console
            if (ConsoleColorSupport.Level == ColorSupportLevel.None)
            {
                Console.Clear();
            }
            else
            {
                Write($"{SHOW_CURSOR}{RESET_ALL_ATTRIBUTES}{RESET_CURSOR}{CLEAR_SCREEN}");
                WriteLine();
                Flush();
            }

            // Return buffer
            if (_buffer != null)
            {
                ArrayPool<char>.Shared.Return(_buffer);
            }
            
            // Reset
            _buffer = null;
            _index = 0;
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
    }
}