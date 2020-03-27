﻿using System;
using System.Collections.Generic;
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
        /// <returns>Built service provider</returns>
        public static IServiceProvider GetServiceProvider(IConfiguration appConfig, Action<IServiceCollection> serviceBuilder, ConfigBase config)
        {
            //get ressurectit assemblies
            IEnumerable<Assembly> assemblies = AssemblyLoadContext.Default.Assemblies
                .Where(assembly => assembly.GetName().Name?.StartsWith("RessurectIT") ?? false);

            //initialize DI
            IServiceCollection services = new ServiceCollection();
            IContainer container = new Container(rules => rules.WithDefaultReuse(Reuse.InCurrentScope)
                                                                       .WithoutThrowOnRegisteringDisposableTransient()
                                                                       .WithoutImplicitCheckForReuseMatchingScope(),
                                                 AsyncExecutionFlowScopeContext.Default);

            services.AddLogging(builder =>
            {
                if(!(Log.Logger is SerilogLogger))
                {
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(appConfig)
                        .WriteTo.RestLogger(config)
                        .CreateLogger();
                }

                builder.AddSerilog(dispose: true);
            });

            serviceBuilder(services);

            container.RegisterExports(assemblies);
            container = container.WithDependencyInjectionAdapter(services);
            container = container.WithMefAttributedModel();

            return container;
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

            IServiceProvider provider = GetServiceProvider(appConfig,
                                                           serviceCollection =>
                                                           {
                                                               serviceCollection.AddSingleton(serviceProvider => appConfigObj);
                                                               serviceCollection.AddSingleton<ConfigBase>(serviceProvider => serviceProvider.GetService<AppConfig>());
                                                           },
                                                           appConfigObj);
        }
        #endregion
    }
}
