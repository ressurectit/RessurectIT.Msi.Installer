using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RessurectIT.Msi.Installer.Configuration;

namespace RessurectIT.Msi.Installer
{
    /// <summary>
    /// Startup class for web server
    /// </summary>
    public class Startup
    {
        #region private fields

        /// <summary>
        /// Service configuration
        /// </summary>
        public IConfiguration _configuration;
        #endregion

        #region constructors
        
        /// <summary>
        /// Creates instance of <see cref="Startup"/>
        /// </summary>
        /// <param name="configuration">Service configuration</param>
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion


        #region public methods

        /// <summary>
        /// Configure service providers
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>New service provider</returns>
        public void ConfigureServices(IServiceCollection services)
        {
            ServiceConfig config = new ServiceConfig();

            _configuration.Bind(config);

            services.AddControllers();
        }

        /// <summary>
        /// Configure middleware pipeline
        /// </summary>
        /// <param name="app">App builder</param>
        /// <param name="env">Hosting environment</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        #endregion
    }
}
