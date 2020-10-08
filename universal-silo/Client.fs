namespace Orleans.Contrib.UniversalSilo.ClusterClient

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Orleans
open Orleans.Configuration
open Orleans.Hosting
open Orleans.Runtime.Utils
open System
open System.IO
open System.Net
open System.Threading
open System.Threading.Tasks
open Orleans.Contrib.UniversalSilo.Configuration
open Orleans.Clustering.AzureStorage

type HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext

type private ClusterClientHostedService(clusterClient : IClusterClient) = class
    interface IHostedService with
        member __.StartAsync (_: CancellationToken) : Task =
            Task.CompletedTask

        member __.StopAsync(_ : CancellationToken) : Task =
            if (clusterClient <> null) then
                clusterClient.Close () |> Async.AwaitTask |> Async.RunSynchronously
                clusterClient.Dispose ()

            Task.CompletedTask
end

[<AbstractClass>]
type ClientConfiguration() = class
    abstract SiloConfiguration : SiloConfiguration
    default __.SiloConfiguration =
        new SiloConfiguration()

    abstract ClusteringConfiguration : ClusteringConfiguration
    default __.ClusteringConfiguration =
        new ClusteringConfiguration()

    member private this.BuildClusterClient(logger, configuration) =
        let clusteringConfiguration =
            this.ClusteringConfiguration
            |> BuildClusteringConfiguration logger configuration

        let siloConfiguration =
            this.SiloConfiguration
            |> BuildSiloConfiguration logger configuration

        let retryDelayInSeconds = 10
        let mutable retryAttempts = 9

        let retryOnFailure =
            let retryOnFailure' ex =
                if (retryAttempts = 0) then raise ex;

                Task.Delay (retryDelayInSeconds * 1000) |> Async.AwaitTask |> Async.RunSynchronously
                retryAttempts <- retryAttempts - 1
                Task.FromResult true
            in
            System.Func<exn, Task<bool>> retryOnFailure'

        let setClusteringMode (cb : IClientBuilder) =
            match clusteringConfiguration.ClusteringMode with
            | ClusteringModes.Kubernetes ->
                logger.LogInformation ("{ClusteringMode} : NOT SUPPORTED", clusteringConfiguration.ClusteringMode)
                raise <| new NotSupportedException("K8s clustering not supported - use Azure or Docker clustering")

            | ClusteringModes.Azure ->
                let connectionString = clusteringConfiguration.ConnectionString
                cb.UseAzureStorageClustering (fun (options : AzureStorageGatewayOptions) ->
                    options.ConnectionString <- connectionString) |> ignore
                logger.LogInformation ("{ClusteringMode} {ConnectionString}", clusteringConfiguration.ClusteringMode, connectionString)

            | ClusteringModes.Docker | ClusteringModes.HostLocal | _ ->
                let siloAddress = clusteringConfiguration.SiloAddress
                let gatewayPort = siloConfiguration.GatewayPort
                let endpointUri = IPEndPoint (siloAddress, gatewayPort) |> ToGatewayUri
                cb.UseStaticClustering (fun (option : StaticGatewayListProviderOptions) ->
                    option.Gateways.Add <| endpointUri) |> ignore
                logger.LogInformation ("{ClusteringMode} {EndpointUri}", clusteringConfiguration.ClusteringMode, endpointUri)
            cb

        let client =
            (new ClientBuilder() |> setClusteringMode)
                .ConfigureApplicationParts(fun parts -> parts.AddFromApplicationBaseDirectory () |> ignore)
                .Configure<ClusterOptions>(fun (options : ClusterOptions) ->
                    options.ClusterId <- siloConfiguration.ClusterId
                    options.ServiceId <- siloConfiguration.ServiceId)
                .Build()

        client.Connect retryOnFailure |> Async.AwaitTask |> Async.RunSynchronously
        client

    abstract GetClusterClient : IServiceProvider -> IClusterClient
    default this.GetClusterClient serviceProvider =
        let logger = serviceProvider.GetService<ILogger<ClientConfiguration>> ()
        let configuration = serviceProvider.GetService<IConfiguration> ()
        this.BuildClusterClient(logger, configuration)

    abstract ConfigureAppConfiguration : HostBuilderContext -> IConfigurationBuilder -> unit
    default __.ConfigureAppConfiguration _ configBuilder =
        configBuilder
            .SetBasePath(Directory.GetCurrentDirectory ())
            .AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly())
        |> ignore

    abstract ConfigureServicesCore : HostBuilderContext -> IServiceCollection -> unit
    default this.ConfigureServicesCore _ services =
        services
            .AddSingleton<IClusterClient>(this.GetClusterClient)
            .AddHostedService<ClusterClientHostedService>()
        |> ignore

    abstract ConfigureServices : HostBuilderContext -> IServiceCollection -> unit

    abstract GetHostBuilder : string[] -> IHostBuilder
    default this.GetHostBuilder args =
        (Host.CreateDefaultBuilder args)
            .ConfigureAppConfiguration(this.ConfigureAppConfiguration)
            .ConfigureServices(this.ConfigureServicesCore)
            .ConfigureServices(this.ConfigureServices)
end

type ClientConfiguration<'THostedService when 'THostedService : not struct and 'THostedService :> IHostedService>() = class
    inherit ClientConfiguration ()
    override __.ConfigureServices _ sc =
        sc.AddHostedService<'THostedService>()
        |> ignore
end

[<AbstractClass>]
type HostedServiceBase(applicationLifetime : IHostApplicationLifetime, clusterClient : IClusterClient) = class
    abstract ExecuteAsync : unit -> Task

    member val ApplicationLifetime = applicationLifetime with get
    member val ClusterClient = clusterClient with get

    interface IHostedService with
        member this.StartAsync (_: CancellationToken) : Task =
            do applicationLifetime.ApplicationStarted.Register(fun () ->
                do this.ExecuteAsync () |> Async.AwaitTask |> Async.RunSynchronously
                do applicationLifetime.StopApplication())
            |> ignore
            Task.CompletedTask

        member __.StopAsync(_ : CancellationToken) : Task =
            Task.CompletedTask
end
