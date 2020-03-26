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
using Serilog;
using Serilog.Core;

namespace RessurectIT.Msi.Installer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        #region protected methods - Overrides of Application

        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //set current directory to directory where are app files located
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            Environment.CurrentDirectory = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));

            //get configuration
            IConfigurationRoot appConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", false)
#if DEBUG
                .AddJsonFile("config.dev.json", true)
#endif
                .AddEnvironmentVariables("RIT_MSI_INSTALLER")
                .AddCommandLine(e.Args)
                .Build();

            Config appConfigObj = new Config();
            appConfig.Bind(appConfigObj);

            //get ressurectit assemblies
            IEnumerable<Assembly> assemblies = AssemblyLoadContext.Default.Assemblies
                .Where(assembly => assembly.GetName().Name.StartsWith("RessurectIT"));

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

            services.AddSingleton(provider => appConfigObj);

            container.RegisterExports(assemblies);
            container = container.WithDependencyInjectionAdapter(services);
            container = container.WithMefAttributedModel();

            IServiceProvider res = container;
            Config cfg = res.GetService<Config>();
            ILogger<App> logger = res.GetService<ILogger<App>>();


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
