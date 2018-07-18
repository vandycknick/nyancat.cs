using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Nyancat.Graphics
{
    public class SceneManager : ISceneManager
    {
        private readonly IServiceProvider Provider;
        
        private IScene CurrentScene;
        private IEnumerable<IScene> RegisteredScenes;
        private Type MoveToScene;

        public SceneManager(IServiceProvider provider)
        {
            Provider = provider;
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

                CurrentScene.Init();
                MoveToScene = null;
            }

            return CurrentScene;
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
