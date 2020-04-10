namespace Template.StandaloneSilo

open Microsoft.Extensions.Hosting
open Orleans.Contrib.UniversalSilo.Configuration

/// <summary>
/// Override methods in this class to take over how the silo is configured
/// </summary>
type SiloConfigurator () = class
    inherit Orleans.Contrib.UniversalSilo.SiloConfigurator(false)
end

module Program =
    let CreateHostBuilderString args =
        let siloConfigurator =
            new SiloConfigurator()
            |> (fun sc -> sc.ConfigureSiloHost)

        (Host.CreateDefaultBuilder args)
        |> ApplyAppConfiguration
        |> (fun hb -> hb.UseOrleans siloConfigurator)

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
       CreateHostBuilderString args
       |> (fun hb -> hb.Build())
       |> (fun hb -> hb.Run())

       0