namespace Template.StandaloneClient
open Microsoft.Extensions.Hosting
open Orleans.Contrib.UniversalSilo.ClusterClient
open System
open System.Threading.Tasks
open Template.Contract


/// This is the service that will host your client application.
type ClientService(applicationLifetime, clusterClient) = class
    inherit HostedServiceBase (applicationLifetime, clusterClient)

    override __.ExecuteAsync () =
        let (l, r) = (5, 6)
        let adder = base.ClusterClient.GetGrain<ICalculatorGrain> <| Guid.NewGuid()
        let result = adder.Add l r |> Async.AwaitTask |> Async.RunSynchronously

        printfn "%d + %d = %d" l r result
        Task.CompletedTask
end

/// This is the configuration class that you can use to override configuration
type ClientConfiguration() = class
    inherit ClientConfiguration<ClientService>()

    override __.SiloConfiguration =
        base.SiloConfiguration.ServiceId <- "Template"
        base.SiloConfiguration
end

/// This is the entry point to the client.
///
/// No changes should normally be needed here to get a running client finding and talking to a silo
///
/// Provide the configuration of the silo to connect by any combination of
///    * Working with the default configuration
///    * Setting environment variables,
///    * Providing a `clustering.json` file
///    * Overriding `ClientConfiguration` and appropriate methods on it
module Program =
    [<EntryPoint>]
    let Main args =
        ClientConfiguration()
            .GetHostBuilder(args)
            .RunConsoleAsync()
        |> Async.AwaitTask
        |> Async.RunSynchronously

        0