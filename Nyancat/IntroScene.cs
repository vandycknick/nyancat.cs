using System;
using System.Collections.Generic;
using System.Diagnostics;
using Nyancat.Graphics;

namespace Nyancat
{
    public class IntroScene : IScene
    {
        private readonly IGraphicsDevice Graphics;
        private readonly ISceneManager SceneManager;

        private List<string> Messages { get; set; }

        private Stopwatch counter = new Stopwatch();

        public IntroScene(IGraphicsDevice graphics, ISceneManager sceneManager)
        {
            Graphics = graphics;
            SceneManager = sceneManager;
        }

        public void Init()
        {
            Messages = new List<string>();

            Messages.Add("Nynacat Dotnet Core");
            Messages.Add("written by Nick Van Dyck @vandycknick");
            Messages.Add("Problems? Please report here: https://github.com/nickvdyck/nyancat.cs/issues");

            counter.Reset();
            counter.Start();
        }

        public void Render()
        {
            var moveOn = 5 - counter.Elapsed.TotalSeconds;

            if (moveOn < 0) {
                SceneManager.GoTo<NyancatScene>();
                return;
            }

            Graphics.MoveTo(3, 0);

            foreach(var line in Messages)
            {
                int lineMargin = (Graphics.Width - line.Length) /2;
                while (lineMargin > 0)
                {
                    Graphics.Write(' ', "");
                    lineMargin--;
                }

                foreach(var ch in line)
                {
                    Graphics.Write(ch, "");
                }

                Graphics.NewLine();
                Graphics.NewLine();
            }
            
            var starting = $"Starting in {Math.Round(5 - counter.Elapsed.TotalSeconds)}...";

            int lm = (Graphics.Width - starting.Length) /2;
            while (lm > 0)
            {
                Graphics.Write(' ', "");
                lm--;
            }

            foreach(var ch in starting)
            {
                Graphics.Write(ch, "");
            }
        }
    }
}
