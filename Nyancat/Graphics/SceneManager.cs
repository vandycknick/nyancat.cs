using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Nyancat.Graphics
{
    public sealed class SceneManager : ISceneManager
    {
        private readonly IServiceProvider Provider;
        private readonly SceneManagerOptions _options;
        
        private IScene CurrentScene;
        private IEnumerable<IScene> RegisteredScenes;
        private Type MoveToScene;

        public SceneManager(IServiceProvider provider, IOptions<SceneManagerOptions> options)
        {
            Provider = provider;
            _options = options.Value;

            GoToStartup();
        }

        public IScene GetCurrentScene()
        {
            if (RegisteredScenes == null)
                RegisteredScenes = Provider.GetServices<IScene>();

            if (MoveToScene != null)
            {
                CurrentScene = RegisteredScenes
                    .Where(s => s.GetType() == MoveToScene)
                    .FirstOrDefault();

                CurrentScene.Initialize();
                MoveToScene = null;
            }

            return CurrentScene;
        }

        internal void GoToStartup()
        {
            GoTo(_options.StartupScene);
        }

        public void GoTo<T>()
        {
            GoTo(typeof(T));
        }

        public void GoTo(Type scene)
        {
            MoveToScene = scene;
        }
    }
}
