using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Nyancat.Graphics;

namespace Nyancat.Scenes
{
    public class IntroScene : IScene
    {
        private readonly IGraphicsDevice Graphics;

        private List<string> _messages;
        private readonly Action _gotoNextScene;

        private const int TIME_TO_SHOW = 5;

        public IntroScene(IGraphicsDevice graphics, Action gotoNextScene)
        {
            Graphics = graphics;
            _gotoNextScene = gotoNextScene;
        }

        public void Initialize()
        {
            _messages = new List<string>
            {
                "Nyancat Dotnet Core",
                "written by Nick Van Dyck @vandycknick",
                "Problems? Please report here: https://github.com/nickvdyck/nyancat.cs/issues"
            };
        }

        public void Update(IGameTime gameTime)
        {
            var moveOn = TIME_TO_SHOW - gameTime.TotalGameTime.Seconds;

            if (moveOn < 0)
            {
                _gotoNextScene();
                return;
            }
        }

        public void Render(IGameTime gameTime)
        {
            Graphics.Clear(Color.Black);

            var row = 3;

            foreach (var line in _messages)
            {
                int col = (Graphics.Width - line.Length) / 2;
                var postion = new Position
                {
                    Row = row,
                    Col = col
                };

                Graphics.Draw(line, postion, Color.White, Color.Black);
                row += 2;
            }

            var starting = $"Starting in {TIME_TO_SHOW - gameTime.TotalGameTime.Seconds}...";
            var position = new Position
            {
                Row = row,
                Col = (Graphics.Width - starting.Length) / 2,
            };

            Graphics.Draw(starting, position, Color.White, Color.Black);
            Graphics.Render();
        }
    }
}
