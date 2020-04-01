using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RessurectIT.Msi.Installer.Configuration;
using RessurectIT.Msi.Installer.Services;
using Serilog;
using static RessurectIT.Msi.Installer.App;

namespace RessurectIT.Msi.Installer
{
    /// <summary>
    /// Main application entry class
    /// </summary>
    public class Program
    {
        #region public static methods

        /// <summary>
        /// Main application entry method
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static void Main(string[] args)
        {
            SetCurrentDirectory();

            IConfigurationRoot serviceConfig = GetConfiguration(args);
            ServiceConfig serviceConfigObj = new ServiceConfig();
            serviceConfig.Bind(serviceConfigObj);

            Config = serviceConfigObj;

            LaunchDebugger(serviceConfigObj);

            ILogger logger = InitLogger(serviceConfig);

            IContainer container = GetServiceProvider(serviceConfig,
                                                      serviceCollection =>
                                                      {
                                                          serviceCollection.AddSingleton(serviceProvider => serviceConfigObj);
                                                          serviceCollection.AddSingleton<ConfigBase>(serviceProvider => serviceProvider.GetService<ServiceConfig>());
                                                          //serviceCollection.AddSingleton<StopService.StopService>();
                                                      });

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

            if (!serviceConfigObj.LocalServer)
            {
                logger.Debug("Running as Windows Service");
                hostBuilder.UseWindowsService();
            }

            hostBuilder
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseConfiguration(serviceConfig);
                })
                .UseServiceProviderFactory(new DryIocServiceProviderFactory(container))
                .ConfigureServices(services =>
                {
                    services.AddHostedService<UpdateCheckerHostService>();
                })
                .UseSerilog(logger, true)
                .Build()
                .Run();
        }
        #endregion
    }
}
