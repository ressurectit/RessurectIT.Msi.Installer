using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RessurectIT.Msi.Installer.Configuration;
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

            IConfigurationRoot appConfig = GetConfiguration(args);
            ServiceConfig appConfigObj = new ServiceConfig();
            appConfig.Bind(appConfigObj);

            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        #endregion
    }
}
