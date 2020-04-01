using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Principal;
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
        #region public properties

        /// <summary>
        /// Gets or sets configuration, use only for rest logger
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "CS8618:Non-nullable property is uninitialized. Consider declaring as nullable.", Justification = "<Pending>")]
        public static ConfigBase Config
        {
            get;
            set;
        }
        #endregion


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
        /// <param name="servicesCollection">Collection of services, used for configuration</param>
        /// <returns>Built service provider</returns>
        public static IContainer GetServiceProvider(IConfiguration appConfig, Action<IServiceCollection> serviceBuilder, IServiceCollection? servicesCollection = null)
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
                InitLogger(appConfig);

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
        public static ILogger InitLogger(IConfiguration appConfig)
        {
            if (!(Log.Logger is SerilogLogger))
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(appConfig)
                    .CreateLogger();
            }

            return Log.Logger;
        }

        /// <summary>
        /// Launch debugger
        /// </summary>
        /// <param name="config">Current configuration</param>
        [Conditional("DEBUG")]
        public static void LaunchDebugger(ConfigBase config)
        {
#if DEBUG
            if (config.Debugging)
            {
                Debugger.Launch();
            }
#endif
        }

        /// <summary>
        /// Tests whether application is running with administrator privileges
        /// </summary>
        /// <returns>True if app is running with administrator privileges context</returns>
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        #endregion


        #region protected methods - Overrides of Application

        /// <inheritdoc />
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SetCurrentDirectory();

            IConfigurationRoot appConfig = GetConfiguration(e.Args);
            AppConfig appConfigObj = new AppConfig();
            appConfig.Bind(appConfigObj);

            LaunchDebugger(appConfigObj);

            DispatcherUnhandledException += (sender, args) =>
            {
                //TODO - do write to file as log
            };

            Config = appConfigObj;

            IServiceProvider provider = GetServiceProvider(appConfig,
                                                           serviceCollection =>
                                                           {
                                                               serviceCollection.AddSingleton(serviceProvider => appConfigObj);
                                                               serviceCollection.AddSingleton<ConfigBase>(serviceProvider => serviceProvider.GetService<AppConfig>());
                                                               serviceCollection.AddSingleton<StopService.StopService>();
                                                           });

            using (IServiceScope scope = provider.CreateScope())
            {
                await scope.ServiceProvider.GetService<Installer.Installer>().InstallFromProtocol();
            }

            Shutdown();
        }
        #endregion
    }
}
