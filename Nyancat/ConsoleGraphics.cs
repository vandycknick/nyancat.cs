using System;
using System.Buffers;
using System.Runtime.CompilerServices;

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
                // _buffer = Array.Empty<char>();
            }
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
        }
    }
}