using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Nyancat.Graphics
{
    public class ConsoleGraphicsHost : IHostedService, IDisposable
    {
        private IScene CurrentScene;

        private IGraphicsDevice Graphics;

        private IApplicationLifetime _appLifeTime;

        private Task RenderLoop;

        public ConsoleGraphicsHost(IApplicationLifetime appLifetime, IGraphicsDevice graphics, IScene scene)
        {
            CurrentScene = scene;
            Graphics = graphics;
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
                        Graphics.Clear();
                        CurrentScene.Render();
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
            CurrentScene.Init();

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
            Debug.WriteLine("Doing some cleanup");
            Graphics.Dispose();
        }
    }
}
