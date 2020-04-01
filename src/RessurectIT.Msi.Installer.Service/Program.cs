using System;
using System.Text;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RessurectIT.Msi.Installer.Configuration;
using RessurectIT.Msi.Installer.Gatherer.Dto;
using RessurectIT.Msi.Installer.Installer.Dto;
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
        /// Creates serialized version of update that is used as command line argument
        /// </summary>
        /// <param name="update">Update to be serialized</param>
        /// <param name="jsonSerializerSettings">Json serializer settings</param>
        /// <returns>Serialized update</returns>
        public static string SerializeUpdate(IMsiUpdate update, JsonSerializerSettings jsonSerializerSettings)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new MsiUpdate
                                                                                                       {
                                                                                                           WaitForProcessNameEnd = update.WaitForProcessNameEnd,
                                                                                                           AdminPrivilegesRequired = update.AdminPrivilegesRequired,
                                                                                                           ComputedHash = update.ComputedHash,
                                                                                                           Id = update.Id,
                                                                                                           InstallParameters = update.InstallParameters,
                                                                                                           MsiPath = update.MsiPath,
                                                                                                           StartProcessPath = update.StartProcessPath,
                                                                                                           StopProcessName = update.StopProcessName,
                                                                                                           UninstallParameters = update.UninstallParameters,
                                                                                                           UninstallProductCode = update.UninstallProductCode,
                                                                                                           Version = update.Version,
                                                                                                           ForceStop = update.ForceStop
                                                                                                       },
                                                                                                       jsonSerializerSettings)));
        }

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
                                                          serviceCollection.AddSingleton(serviceProvider =>
                                                          {
                                                              return new StopService.StopService
                                                              {
                                                                  StopCallback = () => serviceProvider.GetService<IHostApplicationLifetime>().StopApplication()
                                                              };
                                                          });
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
