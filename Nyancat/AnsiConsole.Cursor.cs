namespace Nyancat
{
    partial struct AnsiConsole
    {
        private const string HIDE_CURSOR = "\x1b[?25l";
        private const string RESET_CURSOR = "\x1b[H";
        private const string SHOW_CURSOR = "\x1b[?25h";
        private const string CLEAR_SCREEN = "\x1b[2J";
        private const string RESET_ALL_ATTRIBUTES = "\x1b[0m";

        public AnsiConsole HideCursor()
        {
            if (_hasAnsiSupport) Write(HIDE_CURSOR);
            return this;
        }

        public AnsiConsole ResetCursor()
        {
            if (_hasAnsiSupport)
            {
                Write(RESET_CURSOR);
            }
            else
            {
                // System.Console.CursorTop = 0;
                // System.Console.CursorLeft = 0;
            }
            return this;
        }

        public AnsiConsole ResetAll()
        {
            if (_hasAnsiSupport)
            {
                Write($"{SHOW_CURSOR}{RESET_ALL_ATTRIBUTES}{RESET_CURSOR}{CLEAR_SCREEN}");
                WriteLine();
                Flush();
            }
            else
            {
                System.Console.Clear();
            }

            return this;
        }
    }
}
