namespace Template.StandaloneSilo

open Microsoft.Extensions.Hosting
open Orleans.Contrib.UniversalSilo.Configuration.Extensions

/// <summary>
/// Override methods in this class to take over how the silo is configured
/// </summary>
type SiloConfigurator () = class
    inherit Orleans.Contrib.UniversalSilo.SiloConfigurator()
end

module Program =
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
    [<EntryPoint>]
    let Main args =
        (Host.CreateDefaultBuilder args)
            .ApplyAppConfiguration()
            .UseOrleans((new SiloConfigurator()).ConfigureSiloHost)
            .Build()
            .Run()
        0