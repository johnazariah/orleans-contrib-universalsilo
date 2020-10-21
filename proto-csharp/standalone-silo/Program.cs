using Microsoft.Extensions.Hosting;
using Orleans.Contrib.UniversalSilo.Configuration;

namespace GeneratedProjectName.StandaloneSilo
{
    /// <summary>
    /// Override methods in this class to take over how the silo is configured
    /// </summary>
    class SiloConfigurator : Orleans.Contrib.UniversalSilo.SiloConfigurator
    {
        public override SiloConfiguration SiloConfiguration =>
            base.SiloConfiguration
            .With(_c => _c.ServiceId = "GeneratedProjectName");

        public SiloConfigurator() : base()
        { }
    }

    /// <summary>
    ///
    /// This is the entry point to the silo.
    ///
    /// No changes should normally be needed here to start up a silo
    ///
    /// Provide the configuration of the silo to connect by any combination of (in order of override)
    ///    * The default configuration
    ///    * Overriding in the <see cref="SiloConfigurator"/> class
    ///    * Providing a section in the "appSettings.json"/> file. (If at all possible, do not use this option.)
    ///    * Setting user secrets for managing secrets and connection strings in development
    ///    * Setting environment variables
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
            .UseOrleans(new SiloConfigurator().ConfigureSiloHost);
    }
}
