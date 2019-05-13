using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nyancat.Drivers;

namespace Nyancat.Graphics
{
    public static class HostBuilderExtensions
    {
        private static IHostBuilder UseHostedService<T>(this IHostBuilder hostBuilder) where T : class, IHostedService, IDisposable
        {
            return hostBuilder.ConfigureServices(services => services.AddHostedService<T>());
        }

        public static IHostBuilder UseConsoleGraphicsHost(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .UseHostedService<ConsoleGraphicsHost>()
                .ConfigureServices(services =>
                    services
                        .Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true)
                        .AddSingleton<IConsoleDriver>(provider =>
                            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                                (IConsoleDriver)new WindowsConsoleDriver() :
                                (IConsoleDriver)new UnixConsoleDriver()
                        )
                        .AddSingleton<ISceneManager, SceneManager>()
                        .AddSingleton<IGraphicsDevice, GraphicsDevice>()
                );
        }

        public static IHostBuilder AddScene<T>(this IHostBuilder hostBuilder, bool isStartup = default) where T : IScene
        {
            return hostBuilder
                .ConfigureServices((_, services) =>
                {
                    services.AddTransient(typeof(IScene), typeof(T));

                    if (isStartup)
                    {
                        services.Configure<SceneManagerOptions>(
                            options => options.StartupScene = typeof(T)
                        );
                    }
                });
        }
    }
}