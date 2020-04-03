using Microsoft.Extensions.Hosting;
using Orleans.Contrib.UniversalSilo.Configuration;

namespace Template.StandaloneSilo
{
    /// <summary>
    /// Override methods in this class to take over how the silo is configured
    /// </summary>
    class SiloConfigurator : Orleans.Contrib.UniversalSilo.SiloConfigurator
    {
        public SiloConfigurator() : base(false)
        { }
    }

    /// <summary>
    ///
    /// This is the entry point to the silo.
    ///
    /// No changes should normally be needed here to start up a silo
    ///
    /// Provide the configuration of the silo to connect by any combination of
    ///    * Working with the default configuration
    ///    * Setting environment variables,
    ///    * Providing a `clustering.json` file to configure clustering options
    ///    * Providing a `persistence.json` file to configure storage provider options
    ///    * Overriding methods in the `SiloConfigurator` class
    ///
    /// </summary>
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
            .CreateDefaultBuilder(args)
            .ApplyAppConfiguration()
            .UseOrleans(new SiloConfigurator().ConfigureSiloHost);
    }
}
