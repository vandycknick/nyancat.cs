using System;
using System.Collections.Generic;

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

        private Dictionary<char, string> _colors;

        public NyancatScene(int frames = int.MaxValue, int showCounter = 1)
        {
            _frames = frames;
            _showCounter = showCounter;
            _width = 80;
            _height = 24;

            _minCol = _maxCol = _minRow = _maxRow = 0;

            _fc = int.MinValue;
            _startTime = long.MinValue;

            _colors = GetColors();

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

        public void Render(ref ConsoleGraphics console)
        {
            var frame = NyancatFrames.GetFrame(_fc);
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

                    if (ConsoleColorSupport.Level == ColorSupportLevel.None)
                    {
                        lastPixel = pixel;
                        console.Write(_colors[pixel]);
                        console.Write(_colors[pixel]);
                    }
                    else if (pixel != lastPixel && _colors.ContainsKey(pixel))
                    {
                        lastPixel = pixel;
                        console.Write(_colors[pixel]);
                        console.Write(' ');
                        console.Write(' ');
                    }
                    else
                    {
                        console.Write(' ');
                        console.Write(' ');
                    }
                }

                console.WriteLine();
            }

            if (_showCounter == 1)
            {
                var totalTime = (Environment.TickCount64 - _startTime) / 1000;
                var message = string.Concat("You have nyaned for ", totalTime, " seconds!");
                var spacesLength = (_width - message.Length) / 2;

                if (spacesLength > 0)
                {
                    Span<char> spaces = stackalloc char[spacesLength];

                    for (var i = 0; i < spacesLength; i++)
                    {
                        spaces[i] = ConsoleColorSupport.Level == ColorSupportLevel.None ? ',' : ' ';
                    }

                    console.Write(spaces);
                    console.ColorBrightWhite();
                    console.Write(message);
                    console.Write(spaces);
                }
            }

            console.WriteLine();
        }

        private static Dictionary<char, string> GetColors()
        {
            if (ConsoleColorSupport.Level.HasFlag(ColorSupportLevel.TrueColor))
            {
                return new Dictionary<char, string>
                {
                    { ','  , "\x1b[48;2;0;0;95m" },         /* Blue background */
                    { '.'  , "\x1b[48;2;255;255;255m" },    /* White stars */
                    { '\'' , "\x1b[48;2;0;0;0m" },          /* Black border */
                    { '@'  , "\x1b[48;2;255;255;215m" },    /* Tan poptart */
                    { '$'  , "\x1b[48;2;255;192;203m" },    /* Pink poptart */
                    { '-'  , "\x1b[48;2;215;0;135m" },      /* Red poptart */
                    { '>'  , "\x1b[48;2;255;0;0m" },        /* Red rainbow */
                    { '&'  , "\x1b[48;2;255;165;0m" },      /* Orange rainbow */
                    { '+'  , "\x1b[48;2;255;255;0m" },      /* Yellow Rainbow */
                    { '#'  , "\x1b[48;2;135;255;0m" },      /* Green rainbow */
                    { '='  , "\x1b[48;2;0;135;255m" },      /* Light blue rainbow */
                    { ';'  , "\x1b[48;2;0;0;175m" },        /* Dark blue rainbow */
                    { '*'  , "\x1b[48;2;88;88;88m" },       /* Gray cat face */
                    { '%'  , "\x1b[48;2;215;135;175m" },    /* Pink cheeks */
                };
            }
            else if (ConsoleColorSupport.Level.HasFlag(ColorSupportLevel.Ansi256))
            {
                return new Dictionary<char, string>
                {
                    { ','  , "\x1b[48;5;17m" },  /* Blue background */
                    { '.'  , "\x1b[48;5;231m" }, /* White stars */
                    { '\'' , "\x1b[48;5;16m" },  /* Black border */
                    { '@'  , "\x1b[48;5;230m" }, /* Tan poptart */
                    { '$'  , "\x1b[48;5;175m" }, /* Pink poptart */
                    { '-'  , "\x1b[48;5;162m" }, /* Red poptart */
                    { '>'  , "\x1b[48;5;196m" }, /* Red rainbow */
                    { '&'  , "\x1b[48;5;214m" }, /* Orange rainbow */
                    { '+'  , "\x1b[48;5;226m" }, /* Yellow Rainbow */
                    { '#'  , "\x1b[48;5;118m" }, /* Green rainbow */
                    { '='  , "\x1b[48;5;33m" },  /* Light blue rainbow */
                    { ';'  , "\x1b[48;5;19m" },  /* Dark blue rainbow */
                    { '*'  , "\x1b[48;5;240m" }, /* Gray cat face */
                    { '%'  , "\x1b[48;5;175m" }, /* Pink cheeks */
                };
            }
            else if (ConsoleColorSupport.Level.HasFlag(ColorSupportLevel.Basic))
            {
                return new Dictionary<char, string>
                {
                    { ',' , "\x1b[104m" }, /* Blue background */
                    { '.' , "\x1b[107m" }, /* White stars */
                    { '\'' , "\x1b[40m" }, /* Black border */
                    { '@' , "\x1b[47m" },  /* Tan poptart */
                    { '$' , "\x1b[105m" }, /* Pink poptart */
                    { '-' , "\x1b[101m" }, /* Red poptart */
                    { '>' , "\x1b[101m" }, /* Red rainbow */
                    { '&' , "\x1b[43m" },  /* Orange rainbow */
                    { '+' , "\x1b[103m" }, /* Yellow Rainbow */
                    { '#' , "\x1b[102m" }, /* Green rainbow */
                    { '=' , "\x1b[104m" }, /* Light blue rainbow */
                    { ';' , "\x1b[44m" },  /* Dark blue rainbow */
                    { '*' , "\x1b[100m" }, /* Gray cat face */
                    { '%' , "\x1b[105m" }, /* Pink cheeks */
                };
            }
            else
            {
                return new Dictionary<char, string>
                {
                    { ','  , "," }, /* Blue background */
                    { '.'  , "." }, /* White stars */
                    { '\'' , "'" }, /* Black border */
                    { '@'  , "@" }, /* Tan poptart */
                    { '$'  , "$" }, /* Pink poptart */
                    { '-'  , "-" }, /* Red poptart */
                    { '>'  , ">" }, /* Red rainbow */
                    { '&'  , "&" }, /* Orange rainbow */
                    { '+'  , "+" }, /* Yellow Rainbow */
                    { '#'  , "#" }, /* Green rainbow */
                    { '='  , "=" }, /* Light blue rainbow */
                    { ';'  , ";" }, /* Dark blue rainbow */
                    { '*'  , "*" }, /* Gray cat face */
                    { '%'  , "%" }, /* Pink cheeks */
                };
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

        public void Render(ref ConsoleGraphics console)
        {
            WriteBlankLine(ref console);
            WriteBlankLine(ref console);
            WriteBlankLine(ref console);
            WriteBlankLine(ref console);

            WriteLineCentered("Nyancat Dotnet Core", ref console);
            WriteLineCentered("written by Nick Van Dyck @vandycknick", ref console);
            WriteBlankLine(ref console);
            WriteLineCentered("Found any issues?", ref console);
            WriteLineCentered("Please report here: https://github.com/nickvdyck/nyancat.cs/issues", ref console);

            var timeLeft = string.Concat("Starting in ", TIME_TO_SHOW - _totalTimeInSeconds, "...");
            WriteLineCentered(timeLeft, ref console);

            for (var i = 10; i < _height; i++)
            {
                WriteBlankLine(ref console);
            }

            console.Flush();
        }

        public void WriteBlankLine(ref ConsoleGraphics console) => console.WriteLine("\x1b[48;5;16m\x1b[J\x1b[0m");

        public void WriteLineCentered(string message, ref ConsoleGraphics console)
        {
            console.Write("\x1b[48;5;16m\x1b[38;5;15m");
            WriteCentered(message, ref console);
            console.Write("\x1b[J\x1b[0m");
            console.WriteLine();
        }

        public void WriteCentered(string message, ref ConsoleGraphics console)
        {
            var spacesLength = (_width - message.Length) / 2;
            Span<char> buffer = stackalloc char[spacesLength + message.Length];

            if (message.Length < _width)
            {
                var i = 0;
                for (; i < spacesLength; i++)
                {
                    buffer[i] = ' ';
                }

                foreach (var ch in message)
                {
                    buffer[i++] = ch;
                }

                console.Write(buffer.Slice(0, i));
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
        void Render(ref ConsoleGraphics console);
    }
}