using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Nyancat.Drivers;

namespace Nyancat.Graphics
{
    public abstract class Game : IDisposable
    {
        private readonly IConsoleDriver _driver;
        protected readonly IGraphicsDevice Graphics;

        public Game()
        {
            _driver = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                    (IConsoleDriver)new WindowsConsoleDriver() :
                    new UnixConsoleDriver();

            Graphics = new GraphicsDevice(_driver);
        }

        public abstract void Initialize();

        public abstract void Update(GameTime time);

        public abstract void Render(GameTime time);

        private readonly ManualResetEvent _shutdownBlock = new ManualResetEvent(false);

        public void Run()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                Graphics.Exit();
                _shutdownBlock.WaitOne();
            };

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                Graphics.Exit();
            };

            Initialize();

            var gameTime = GameTime.Zero;
            var watch = new Stopwatch();

            while (Graphics.IsRunning)
            {
                gameTime.Update(watch.Elapsed);
                watch.Restart();
                Update(gameTime);
                gameTime.Update(watch.Elapsed);
                watch.Restart();
                Render(gameTime);
            }

            watch.Stop();
        }

        public void Dispose()
        {
            _driver.Dispose();
            _shutdownBlock.Set();
        }
    }
}