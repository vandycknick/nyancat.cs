using System;

namespace Nyancat
{
    public struct NyancatScene : IScene
    {
        private const string RAINBOW_TAIL = ",,>>&&&+++###==;;;,,";

        private int _frames;
        private int _showCounter;
        private int _width, _height;
        private int _minCol, _maxCol, _minRow, _maxRow;
        private int _fc;
        private long _startTime;

        public NyancatScene(int frames = int.MaxValue, int showCounter = 1)
        {
            _frames = frames;
            _showCounter = showCounter;
            _width = 80;
            _height = 24;

            _minCol = _maxCol = _minRow = _maxRow = 0;

            _fc = int.MinValue;
            _startTime = long.MinValue;


            CalculateRowsAndCols(width: _width, height: _height);
        }

        private void CalculateRowsAndCols(int width, int height)
        {
            _minCol = (NyancatFrames.FRAME_WIDTH - width / 2) / 2;
            _maxCol = (NyancatFrames.FRAME_WIDTH + width / 2) / 2;

            _minRow = (NyancatFrames.FRAME_HEIGHT - (height - _showCounter)) / 2;
            _maxRow = (NyancatFrames.FRAME_HEIGHT + (height - _showCounter)) / 2;
        }

        private void CheckWindowSize(int width, int height)
        {
            if (_height != height || _width != width)
            {
                _height = height;
                _width = width;

                CalculateRowsAndCols(width: _width, height: _height);
            }
        }

        public bool Update(int width, int height)
        {
            if (_frames == 0) return false;

            CheckWindowSize(width: width, height: height);

            _fc = _fc == int.MinValue ? 0 : _fc + 1;

            if (_fc > NyancatFrames.TOTAL_FRAMES)
            {
                _fc = 0;
            }

            if (_startTime == long.MinValue)
            {
                _startTime = Environment.TickCount64;
            }

            if (_frames != int.MaxValue) _frames--;

            return true;
        }

        private Color AsciiPixelToColor(char ch) =>
            ch switch
            {
                ',' => Color.Blue,
                '.' => Color.White,
                '\'' => Color.Black,
                '@' => Color.Tan,
                '$' => Color.Pink,
                '-' => Color.DarkRed,
                '>' => Color.Red,
                '&' => Color.Orange,
                '+' => Color.Yellow,
                '#' => Color.Green,
                '=' => Color.LightBlue,
                ';' => Color.DarkBlue,
                '*' => Color.Gray,
                '%' => Color.LightPink,
                _ => Color.Blue
            };

        public void Render(ref AnsiConsole console)
        {
            ReadOnlySpan<char> frame = NyancatFrames.GetFrame(_fc);
            var lastPixel = char.MinValue;

            for (var row = _minRow; row < _maxRow; ++row)
            {
                for (var col = _minCol; col < _maxCol; ++col)
                {
                    char pixel;
                    if (row > 23 && row < 43 && col < 0)
                    {

                        int mod_x = ((-col + 2) % 16) / 8;
                        if ((_fc / 2) % 2 == 0)
                        {
                            mod_x = 1 - mod_x;
                        }

                        var index = mod_x + row - 23;

                        pixel = ',';
                        if (index < RAINBOW_TAIL.Length)
                        {
                            pixel = RAINBOW_TAIL[index];
                        }
                    }
                    else if (row < 0 || col < 0 || row >= NyancatFrames.FRAME_HEIGHT || col >= NyancatFrames.FRAME_WIDTH)
                    {
                        pixel = ',';
                    }
                    else
                    {
                        var start = row * NyancatFrames.FRAME_WIDTH + (row * 1);

                        start += col;

                        pixel = frame.Slice(start, 1)[0];
                    }

                    if (!console.SupportsColors())
                    {
                        console.Write(pixel);
                        console.Write(pixel);

                    }
                    else if (pixel != lastPixel)
                    {
                        var color = AsciiPixelToColor(pixel);
                        console.WriteColor(' ', color);
                        console.Write(' ');
                    }
                    else
                    {
                        console.Write(' ');
                        console.Write(' ');
                    }
                }

                console.Write("\x1b[K");
                console.WriteLine();
            }

            if (_showCounter == 1)
            {
                var totalTime = (Environment.TickCount64 - _startTime) / 1000;
                var message = string.Concat("You have nyaned for ", totalTime, " seconds!");
                var spacesLength = (_width - message.Length) / 2;

                if (spacesLength > 0)
                {
                    for (var i = 0; i < spacesLength; i++)
                    {
                        console.Write(console.SupportsColors() ? ' ' : ',');
                    }

                    console.WriteColor(message, Color.White, foreground: true);
                    console.WriteLine("\x1b[J\x1b[0m");
                }
            }
        }
    }

    public struct IntroScene : IScene
    {
        private const int TIME_TO_SHOW = 5;
        private bool _initialized;
        private long _startedAt;
        private long _totalTimeInSeconds;
        private int _width;
        private int _height;

        public bool Update(int width, int height)
        {
            if (_initialized == false)
            {
                _initialized = true;
                _startedAt = Environment.TickCount64;
            }

            _width = width;
            _height = height;

            _totalTimeInSeconds = (Environment.TickCount64 - _startedAt) / 1000;
            return (TIME_TO_SHOW - _totalTimeInSeconds) > 0;
        }

        public void Render(ref AnsiConsole console)
        {
            WriteBlankLine(ref console);
            WriteBlankLine(ref console);
            WriteBlankLine(ref console);
            WriteBlankLine(ref console);

            WriteLineCentered("Nyancat Dotnet Core", ref console);
            WriteLineCentered("written by Nick Van Dyck @vandycknick", ref console);
            WriteBlankLine(ref console);
            WriteLineCentered("Found any issues?", ref console);
            WriteLineCentered("Please report here: https://github.com/vandycknick/nyancat.cs/issues", ref console);

            var timeLeft = string.Concat("Starting in ", TIME_TO_SHOW - _totalTimeInSeconds, "...");
            WriteLineCentered(timeLeft, ref console);

            for (var i = 10; i < _height; i++)
            {
                WriteBlankLine(ref console);
            }

            console.Flush();
        }

        public void WriteBlankLine(ref AnsiConsole console) => console.WriteLine("\x1b[48;5;16m\x1b[K");

        public void WriteLineCentered(string message, ref AnsiConsole console)
        {
            console.Write("\x1b[48;5;16m\x1b[38;5;15m");
            WriteCentered(message, ref console);
            console.Write("\x1b[48;5;16m\x1b[K");
            console.WriteLine();
        }

        public void WriteCentered(string message, ref AnsiConsole console)
        {
            var spacesLength = (_width - message.Length) / 2;

            if (message.Length < _width)
            {
                for (var i = 0; i < spacesLength; i++) console.Write(' ');

                foreach (var ch in message) console.Write(ch);
            }
            else
            {
                WriteBlankLine(ref console);
            }
        }
    }

    interface IScene
    {
        bool Update(int width, int height);
        void Render(ref AnsiConsole console);
    }
}
