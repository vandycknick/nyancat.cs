using System;

namespace Nyancat
{
    unsafe partial struct AnsiConsole : IDisposable
    {
        private fixed int _cubeLevels[6];
        private fixed byte _table[256];
        private ConsoleDriver _console;
        private readonly ColorSupport _support;
        private readonly bool _hasAnsiSupport;

        public ConsoleDriver Console { get => _console; }

        public static AnsiConsole Create()
        {
            var support = AnsiColorSupport.Detect(true);
            var builder = new AnsiConsole(support);
            builder._cubeLevels[0] = 0x00;
            builder._cubeLevels[1] = 0x5f;
            builder._cubeLevels[2] = 0x87;
            builder._cubeLevels[3] = 0xaf;
            builder._cubeLevels[4] = 0xd7;
            builder._cubeLevels[5] = 0xff;

            builder.SetupTable();

            return builder;
        }

        private AnsiConsole(ColorSupport support)
        {
            _support = support;
            _console = new ConsoleDriver(true);

            _hasAnsiSupport = true;
        }

        public void Dispose()
        {
            ResetAll();
            _console.Dispose();
        }
    }
}
