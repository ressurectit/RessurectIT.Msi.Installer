using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;
using DryIoc;
using DryIoc.MefAttributedModel;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RessurectIT.Msi.Installer.Configuration;
using Serilog;
using SerilogLogger = Serilog.Core.Logger;

namespace RessurectIT.Msi.Installer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        #region public static methods

        /// <summary>
        /// Sets current directory to directory where app files are located
        /// </summary>
        public static void SetCurrentDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            Environment.CurrentDirectory = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
        }

        /// <summary>
        /// Gets built configuration object
        /// </summary>
        /// <param name="args">Command line args</param>
        /// <returns>Built configuration object</returns>
        public static IConfigurationRoot GetConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", false)
#if DEBUG
                .AddJsonFile("config.dev.json", true)
#endif
                .AddEnvironmentVariables("RIT_MSI_INSTALLER")
                .AddCommandLine(args)
                .Build();
        }

        /// <summary>
        /// Gets built service provider
        /// </summary>
        /// <param name="appConfig">Current app configuration</param>
        /// <param name="serviceBuilder">Callback used for adding custom providers</param>
        /// <param name="config">Application configuration instance</param>
        /// <param name="servicesCollection">Collection of services, used for configuration</param>
        /// <returns>Built service provider</returns>
        public static IContainer GetServiceProvider(IConfiguration appConfig, Action<IServiceCollection> serviceBuilder, ConfigBase config, IServiceCollection servicesCollection = null)
        {
            //get ressurectit assemblies
            IEnumerable<Assembly> assemblies = AssemblyLoadContext.Default.Assemblies
                .Where(assembly => assembly.GetName().Name?.StartsWith("RessurectIT") ?? false);

            IServiceCollection services = servicesCollection ?? new ServiceCollection();
            //initialize DI
            IContainer container = new Container(rules => rules.WithDefaultReuse(Reuse.Singleton)
                                                                       .WithoutThrowOnRegisteringDisposableTransient()
                                                                       .WithoutImplicitCheckForReuseMatchingScope(),
                                                 AsyncExecutionFlowScopeContext.Default);

            services.AddLogging(builder =>
            {
                InitLogger(appConfig, config);

                builder.AddSerilog(dispose: true);
            });

            serviceBuilder(services);

            container.RegisterExports(assemblies);
            container = container.WithDependencyInjectionAdapter(services);
            container = container.WithMefAttributedModel();

            return container;
        }

        /// <summary>
        /// Initialize serilog logger
        /// </summary>
        /// <param name="appConfig">Current app configuration</param>
        /// <param name="config">Application configuration instance</param>
        public static ILogger InitLogger(IConfiguration appConfig, ConfigBase config)
        {
            if (!(Log.Logger is SerilogLogger))
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(appConfig)
                    .WriteTo.RestLogger(config)
                    .CreateLogger();
            }

            return Log.Logger;
        }
        #endregion


        #region protected methods - Overrides of Application

        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SetCurrentDirectory();

            IConfigurationRoot appConfig = GetConfiguration(e.Args);
            AppConfig appConfigObj = new AppConfig();
            appConfig.Bind(appConfigObj);

            LaunchDebugger(appConfigObj);

            IServiceProvider provider = GetServiceProvider(appConfig,
                                                           serviceCollection =>
                                                           {
                                                               serviceCollection.AddSingleton(serviceProvider => appConfigObj);
                                                               serviceCollection.AddSingleton<ConfigBase>(serviceProvider => serviceProvider.GetService<AppConfig>());
                                                           },
                                                           appConfigObj);

            Shutdown();
        }
        #endregion


        #region private methods

        /// <summary>
        /// Launch debugger
        /// </summary>
        /// <param name="config">Current configuration</param>
        [Conditional("DEBUG")]
        private void LaunchDebugger(ConfigBase config)
        {
#if DEBUG
            if (config.Debugging)
            {
                Debugger.Launch();
            }
#endif
        }
        #endregion
    }
}
