using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            RenderLoop = Task.Run(() =>
            {
                try
                {
                    while (Graphics.IsRunning)
                    {
                        var scene = SceneManager.GetCurrentScene();
                        scene.Render();
                        Graphics.SwapBuffers();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
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
            Graphics.Dispose();
        }
    }
}
