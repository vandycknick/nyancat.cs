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
        private readonly ISceneManager SceneManager;

        private List<string> _messages;

        public IntroScene(IGraphicsDevice graphics, ISceneManager sceneManager)
        {
            Graphics = graphics;
            SceneManager = sceneManager;
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

        public void Update()
        {
        }

        public void Render(IGameTime gameTime)
        {
            var moveOn = 5 - gameTime.TotalGameTime.Seconds;

            if (moveOn < 0)
            {
                SceneManager.GoTo<NyancatScene>();
                return;
            }

            Graphics.Clear(Color.Black);

            var row = 3;

            foreach(var line in _messages)
            {
                int col = (Graphics.Width - line.Length) /2;
                var postion = new Position
                {
                    Row = row,
                    Col =col
                };

                Graphics.Draw(line, postion, Color.White, Color.Black);
                row += 2;
            }

            var starting = $"Starting in {(5 - gameTime.TotalGameTime.Seconds)}...";
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
