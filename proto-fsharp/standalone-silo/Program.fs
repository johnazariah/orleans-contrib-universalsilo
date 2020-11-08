namespace GeneratedProjectName.StandaloneSilo

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open System.IO

/// Override methods in this class to take over how the silo is configured
type SiloConfigurator () = class
    inherit Orleans.Contrib.UniversalSilo.Configuration.SiloConfigurator()

    override __.SiloConfiguration =
        base.SiloConfiguration.ServiceId <- "GeneratedProjectName"
        base.SiloConfiguration
end

module Program =
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
    [<EntryPoint>]
    let Main args =
        (Host.CreateDefaultBuilder args)            
            .ConfigureHostConfiguration(fun builder -> ignore <| builder.SetBasePath(Directory.GetCurrentDirectory()))
            .UseOrleans((new SiloConfigurator()).ConfigurationFunc)
            .Build()
            .Run()
        0