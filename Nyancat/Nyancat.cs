using Nyancat.Graphics;
using Nyancat.Scenes;

namespace Nyancat
{
    public class Nyancat : Game
    {
        private readonly NyancatOptions _options;
        private IScene currentScene;
        public Nyancat(NyancatOptions options)
        {
            _options = options;
        }

        private void GotoNyancatScene()
        {
            currentScene = new NyancatScene(Graphics, _options);
            currentScene.Initialize();
        }

        private void GotoIntroScene()
        {
            currentScene = new IntroScene(Graphics, GotoNyancatScene);
            currentScene.Initialize();
        }

        public override void Initialize()
        {
            if (_options.ShowIntro)
            {
                GotoIntroScene();
            }
            else
            {
                GotoNyancatScene();
            }
        }

        public override void Update(GameTime time)
        {
            var last = currentScene;
            currentScene.Update(time);

            if (last != currentScene) currentScene.Update(time);
        }

        public override void Render(GameTime time)
        {
            currentScene.Render(time);
        }
    }
}