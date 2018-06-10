using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nyancat.Drivers;

namespace Nyancat.Graphics
{
    public class ConsoleGraphicsHostBuilder : HostBuilder
    {

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
            services.AddSingleton<IGraphicsDevice, GraphicsDevice>();
            services.AddSingleton<IHostedService, ConsoleGraphicsHost>();
        }
    }
}
