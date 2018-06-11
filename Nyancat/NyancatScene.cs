using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Options;
using Nyancat.Graphics;

namespace Nyancat
{
    public class NyancatScene : IScene
    {
        private IGraphicsDevice Graphics;

        private int min_row = -1;
        private int max_row = -1;
        private int min_col = -1;
        private int max_col = -1;

        private Stopwatch counter = new Stopwatch();

        private Dictionary<char, string> colors = new Dictionary<char, string>();

        private NyancatSceneOptions SceneOptions { get; set; }

        public NyancatScene(IGraphicsDevice graphics, IOptions<NyancatSceneOptions> sceneOptionsAccessor)
        {
            Graphics = graphics;
            SceneOptions = sceneOptionsAccessor.Value;
        }

        public void Init()
        {
            colors[','] = "\x1b[48;5;17m";  /* Blue background */
            colors['.'] = "\x1b[48;5;231m"; /* White stars */
            colors['\''] = "\x1b[48;5;16m"; /* Black border */
            colors['@'] = "\x1b[48;5;230m"; /* Tan poptart */
            colors['$'] = "\x1b[48;5;175m"; /* Pink poptart */
            colors['-'] = "\x1b[48;5;162m"; /* Red poptart */
            colors['>'] = "\x1b[48;5;196m"; /* Red rainbow */
            colors['&'] = "\x1b[48;5;214m"; /* Orange rainbow */
            colors['+'] = "\x1b[48;5;226m"; /* Yellow Rainbow */
            colors['#'] = "\x1b[48;5;118m"; /* Green rainbow */
            colors['='] = "\x1b[48;5;33m";  /* Light blue rainbow */
            colors[';'] = "\x1b[48;5;19m";  /* Dark blue rainbow */
            colors['*'] = "\x1b[48;5;240m"; /* Gray cat face */
            colors['%'] = "\x1b[48;5;175m"; /* Pink cheeks */

            if (min_col == max_col)
            {
                min_col = (NyancatAnimation.FRAME_WIDTH - Graphics.Width / 2) / 2;
                max_col = (NyancatAnimation.FRAME_WIDTH + Graphics.Width / 2) / 2;
            }

            if (min_row == max_row)
            {
                min_row = (NyancatAnimation.FRAME_HEIGHT - (Graphics.Height - 1)) / 2;
                max_row = (NyancatAnimation.FRAME_HEIGHT + (Graphics.Height - 1)) / 2;
            }

            Graphics.OnResize = () =>
            {
                min_col = (NyancatAnimation.FRAME_WIDTH - Graphics.Width / 2) / 2;
                max_col = (NyancatAnimation.FRAME_WIDTH + Graphics.Width / 2) / 2;
                min_row = (NyancatAnimation.FRAME_HEIGHT - (Graphics.Height - 1)) / 2;
                max_row = (NyancatAnimation.FRAME_HEIGHT + (Graphics.Height - 1)) / 2;
            };

            if (SceneOptions.ShowTitle)
            {
                Graphics.Title = "Nyanyanyanyanyanyanya...";
            }

            counter.Reset();
            counter.Start();
        }

        private int frameId = 0;
        private string[] GetFrame()
        {
            var frame = NyancatAnimation.Frames[frameId];

            frameId++;

            if (frameId >= NyancatAnimation.Frames.Count)
                frameId = 0;

            return frame;
        }

        public bool ShouldExit()
        {
            if (SceneOptions.Frames != int.MaxValue)
            {
                SceneOptions.Frames--;
            }

            if (SceneOptions.Frames == 0)
            {
                return true;
            }

            return false;
        }

        public void Render()
        {
            if (ShouldExit())
            {
                Graphics.Exit();
                return;
            }

            var frame = GetFrame();

            Graphics.Fill(' ', colors[',']);

            for (var row = min_row; row < max_row; row++)
            {
                int colFilled = 0;
                for (var col = min_col; col < max_col; col++)
                {
                    char color;

                    if (row > 23 && row < 43 && col < 0)
                    {
                        /*
                         * Generate the rainbow tail.
                         *
                         * This is done with a prettrow simplistic square wave.
                         */
                        int mod_x = ((-col + 2) % 16) / 8;

                        if (((frameId / 2) % 2) != 0)
                        {
                            mod_x++;
                        }

                        /*
                         * Our rainbow, with some padding.
                         */
                        string rainbow = ",,>>&&&+++###==;;;,,";

                        var index = mod_x + row - 23;

                        if (index >= rainbow.Length)
                            index = 0;

                        color = rainbow[index];
                    }
                    else if (col < 0 || row < 0 || row >= NyancatAnimation.FRAME_HEIGHT || col >= NyancatAnimation.FRAME_WIDTH)
                    {
                        color = ',';
                    }
                    else
                    {
                        color = frame[row][col];
                    }

                    Graphics.Write(' ', colors[color]);
                    Graphics.Write(' ', colors[color]);
                    colFilled += 2;
                }

                Graphics.NewLine();
            }

            if (SceneOptions.ShowCounter)
            {

                var seconds = ((int)counter.Elapsed.TotalSeconds).ToString();
                int counterWidth = (Graphics.Width - 29 - seconds.Length) / 2;

                Graphics.MoveTo(Graphics.Height - 1, 0);

                while (counterWidth > 0)
                {
                    Graphics.Write(' ', colors[',']);
                    counterWidth--;
                }

                var message = $"You have nyaned for {seconds} seconds!";

                foreach (var ch in message)
                {
                    Graphics.Write(ch, colors[',']);
                }
            }

            Thread.Sleep(60);
        }
    }
}
