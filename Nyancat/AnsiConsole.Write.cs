using System;

namespace Nyancat
{
    partial struct AnsiConsole
    {
        public AnsiConsole Write(char ch)
        {
            _console.Write(ch);
            return this;
        }
        // public AnsiConsole Write(ReadOnlySpan<char> value)
        // {
        //     _console.Write(value);
        //     return this;
        // }

        public AnsiConsole Write(string value)
        {
            _console.Write(value);
            return this;
        }

        public AnsiConsole WriteLine(string value)
        {
            _console.WriteLine(value);
            return this;
        }

        public AnsiConsole WriteLine()
        {
            _console.WriteLine();
            return this;
        }

        public AnsiConsole Flush()
        {
            _console.Flush();
            return this;
        }
    }
}
