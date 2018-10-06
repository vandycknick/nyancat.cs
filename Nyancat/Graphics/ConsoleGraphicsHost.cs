using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Nyancat.Graphics
{
    public class ConsoleGraphicsHost : IHostedService, IDisposable
    {
        private readonly IGraphicsDevice Graphics;
        private readonly ISceneManager SceneManager;
        private readonly IApplicationLifetime _appLifeTime;

        private Task RenderLoop;

        public ConsoleGraphicsHost(IApplicationLifetime appLifetime, IGraphicsDevice graphics, ISceneManager sceneManager)
        {
            Graphics = graphics;
            SceneManager = sceneManager;
            _appLifeTime = appLifetime;
        }

        private void OnStarted()
        {
            var gameTime = new GameTime();
            var watch = new Stopwatch();

            RenderLoop = Task.Run(() =>
            {
                try
                {
                    while (Graphics.IsRunning)
                    {
                        var scene = SceneManager.GetCurrentScene();
                        scene.Update();
                        gameTime.Update(watch.Elapsed);
                        watch.Restart();
                        scene.Render(gameTime);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    watch.Stop();
                    Graphics.Exit();
                    _appLifeTime.StopApplication();
                }

            });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Debug.WriteLine("Starting application");

            _appLifeTime.ApplicationStarted.Register(OnStarted);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Debug.WriteLine("Stopping");
            Graphics.Exit();
            return RenderLoop;
        }

        public void Dispose()
        {
            Debug.WriteLine("Dispose");
        }
    }
}
