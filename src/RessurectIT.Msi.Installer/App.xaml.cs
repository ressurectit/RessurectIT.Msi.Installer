using System;
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
using Microsoft.Extensions.Logging;
using RessurectIT.Msi.Installer.Configuration;
using RessurectIT.Msi.Installer.Installer;
using Serilog;
using Serilog.Core;

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
        /// <returns>Built service provider</returns>
        public static IServiceProvider GetServiceProvider(IConfiguration appConfig, Action<IServiceCollection> serviceBuilder)
        {
            //get ressurectit assemblies
            IEnumerable<Assembly> assemblies = AssemblyLoadContext.Default.Assemblies
                .Where(assembly => assembly.GetName().Name?.StartsWith("RessurectIT") ?? false);

            //initialize DI
            IServiceCollection services = new ServiceCollection();
            IContainer container = new Container();

            services.AddLogging(builder =>
            {
                if(!(Log.Logger is Logger))
                {
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(appConfig)
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

            //get configuration
            IConfigurationRoot appConfig = GetConfiguration(e.Args);

            Config appConfigObj = new Config();
            appConfig.Bind(appConfigObj);

            IServiceProvider provider = GetServiceProvider(appConfig,
                                                           serviceCollection =>
                                                           {
                                                               serviceCollection.AddSingleton(serviceProvider => appConfigObj);
                                                           });

            
            Config cfg = provider.GetService<Config>();
            ILogger<App> logger = provider.GetService<ILogger<App>>();
            WindowsInstaller installer = provider.GetService<WindowsInstaller>();


            //bool showHelp = false;

            //OptionSet options = new OptionSet
            //{
            //    {
            //        "i|input=", "Cesta k vstupnému súboru, predvolená hodnota 'input.json'", input => InputFile = input
            //    },
            //    {
            //        "o|output=", "Cesta k výstupnému súboru, predvolená hodnota 'output.json'", output => OutputFile = output
            //    },
            //    {
            //        "n|noGui", "Indikácia, že sa má použiť tyto bez vizuálneho rozhrania, vráti originálny variant, predvolená hodnota 'false'", noGui => NoGui = true
            //    },
            //    {
            //        "f|full", "Indikácia, že sa má použiť plné volanie (pomalšie), parameter funguje iba v kombinácii s 'noGui' parametrom, predvolená hodnota 'false'", noGui => Full = true
            //    },
            //    {
            //        "w|wellFormattedOutput", "Indikácia, že výstup má byť pekne naformátovaný, predvolené nastavenie je minimalistický výstup", wellFormattedOutput => WellFormattedOutput = true
            //    },
            //    {
            //        "t|test", "Indikácia, že sa má spustiť test prístupu do registrov a ich existencia pre používateľa spúšťajúceho aplikáciu", registryTest => RunRegistryTest = true
            //    },
            //    //{
            //    //    "f|format=", "Formát dát, povolené hodnoty json|xml, prevolená hodnota json", SetFormat
            //    //},
            //    {
            //        "h|?|help", "Zobrazí tohto pomocníka", v => showHelp = true
            //    },
            //};

            //options.Parse(e.Args);

            //if (showHelp)
            //{
            //    StringBuilder sb = new StringBuilder();

            //    using (StringWriter writer = new StringWriter(sb))
            //    {
            //        options.WriteOptionDescriptions(writer);
            //    }

            //    WriteConsole("Aplikácia pre prístup k Asseco.Drg.Tyto");
            //    WriteConsole("Asseco.Drg.Tyto.Runner.exe <options>");
            //    WriteConsole("");
            //    WriteConsole("Options:");
            //    WriteConsole(sb.ToString());

            //    Shutdown();
            //}
        }
        #endregion
    }
}
