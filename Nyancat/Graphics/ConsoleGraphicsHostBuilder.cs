using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nyancat.Drivers;

namespace Nyancat.Graphics
{
    public class ConsoleGraphicsHostBuilder : HostBuilder
    {

        private Type Scene;

        public ConsoleGraphicsHostBuilder()
        {
            ConfigureServices(ConfigureHostSpecificServices);
        }

        private void ConfigureHostSpecificServices(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
            
            services.AddSingleton<IConsoleDriver>(provider =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return new WindowsConsoleDriver();

                return new UnixConsoleDriver();
            });

            services.AddSingleton<ISceneManager>(provider =>
            {
                var manager = new SceneManager(provider);
                manager.GoTo(Scene);
                return manager;
            });

            services.AddSingleton<IGraphicsDevice, GraphicsDevice>();
            services.AddSingleton<IHostedService, ConsoleGraphicsHost>();
        }

        public ConsoleGraphicsHostBuilder AddScene<T>() where T : IScene
        {
            ConfigureServices((_, services) => services.AddTransient(typeof(IScene), typeof(T)));
            return this;
        }

        public ConsoleGraphicsHostBuilder AddScene<T>(bool isStartup=false) where T : IScene
        {
            if (isStartup)
            {
                Scene = typeof(T);
            }

            return AddScene<T>();
        }

    }
}
