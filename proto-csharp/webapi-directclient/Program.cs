using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Orleans.Contrib.UniversalSilo.Configuration;
using System;

namespace Template.WebApiDirectClient
{
    /// <summary>
    /// Override methods in this class to take over how the web-api host is configured
    /// </summary>
    class WebApiConfigurator : Orleans.Contrib.UniversalSilo.WebApiConfigurator
    {
        private static readonly OpenApiInfo _apiInfo =
            new OpenApiInfo
            {
                Version = "v1",                                                                // revision this appropriately
                Title = "My Orleans API",                                                      // title this application
                Description = "An application with an Orleans backend and a WebAPI interface", // describe this application
                TermsOfService = new Uri("http://127.0.0.1"),                                  // replace with <Your TOS Uri>
                Contact = new OpenApiContact()
                {
                    Name = "A Sagacious Developer",                                            // replace with <Your Name>
                    Email = "<Your Email>",                                                    // replace with <Your Email>
                    Url = new Uri("http://127.0.0.1"),                                         // replace with <Your Uri>
                },
                License = new OpenApiLicense
                {
                    Name = "A generous license",                                               // replace with <Your License Name>
                    Url = new Uri("http://127.0.01"),                                          // replace with <Your License Uri>
                },
            };

        public WebApiConfigurator() : base(_apiInfo)
        { }
    }

    /// <summary>
    /// Override methods in this class to take over how the silo is configured
    /// </summary>
    class SiloConfigurator : Orleans.Contrib.UniversalSilo.SiloConfigurator
    {
        public override SiloConfiguration SiloConfiguration =>
            base.SiloConfiguration
            .With(_c => _c.ServiceId = "Template");

        public SiloConfigurator() : base()
        { }
    }

    /// <summary>
    ///
    /// This is the entry point to the silo.
    ///
    /// No changes should normally be needed here to start up a silo and a web-api front-end co-hosted in the same executable
    ///
    /// Provide the configuration of the silo to connect by any combination of
    ///    * Working with the default configuration
    ///    * Setting environment variables,
    ///    * Providing a `clustering.json` file to configure clustering options
    ///    * Providing a `persistence.json` file to configure storage provider options
    ///    * Overriding methods in the `WebApiConfigurator` class
    ///    * Overriding methods in the `SiloConfigurator` class
    ///
    /// </summary>
    class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args)
            .Build()
            .Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
            .CreateDefaultBuilder(args)
            .ApplyAppConfiguration()
            .ApplyConfiguration(new WebApiConfigurator().ConfigureWebApiHost)
            .UseOrleans(new SiloConfigurator().ConfigureSiloHost);
    }
}
