namespace Orleans.Contrib.UniversalSilo

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Orleans
open Orleans.Configuration
open Orleans.Hosting
open Orleans.Contrib.UniversalSilo.Configuration
open System
open System.Net

type HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext

type SiloConfigurator (forceAzureClustering : bool) = class
    let loggerFactory = LoggerFactory.Create(fun builder -> builder.AddConsole () |> ignore)
    let logger = loggerFactory.CreateLogger("SiloConfigurator")

    member val public Logger = logger

    abstract ConfigureServices : ISiloBuilder -> ISiloBuilder
    default __.ConfigureServices siloBuilder =
        siloBuilder

    abstract ConfigureClustering : IConfiguration -> ClusteringConfiguration -> ISiloBuilder -> ISiloBuilder
    default __.ConfigureClustering configuration clusteringConfiguration siloBuilder =
        let siloAddress = clusteringConfiguration.GetSiloAddress ()
        let siloPort    = configuration.SiloPort
        let gatewayPort = configuration.GatewayPort

        let configureClusterOptions (options : ClusterOptions) =
            options.ClusterId <- configuration.ClusterId
            options.ServiceId <- configuration.ServiceId

        let configureClusterMembership (sb : ISiloBuilder) =
            match clusteringConfiguration.ClusteringMode with
            | ClusteringModes.Kubernetes ->
                logger.LogInformation ("{ClusteringMode} : NOT SUPPORTED", clusteringConfiguration.ClusteringMode)
                raise <| NotSupportedException("K8s clustering not supported - use Azure or Docker clustering")

            | ClusteringModes.Azure ->
                let connectionString = clusteringConfiguration.ConnectionString
                logger.LogInformation("Configuring Azure Storage Clustering {ConnectionString}", connectionString)
                sb.UseAzureStorageClustering(fun (options : AzureStorageClusteringOptions) ->
                    options.ConnectionString <- connectionString)

            | ClusteringModes.Docker | ClusteringModes.HostLocal | _ ->
                logger.LogInformation("Development Clustering running on [{SiloAddress}]:[{SiloPort}]", siloAddress, siloPort)
                IPEndPoint(siloAddress, siloPort) |> sb.UseDevelopmentClustering

        let configureEndpoints (sb : ISiloBuilder) =
            match clusteringConfiguration.ClusteringMode with
            | ClusteringModes.Kubernetes | ClusteringModes.Azure ->
                logger.LogInformation(
                    "Configuring Endpoints without silo address for clustering mode {ClusteringMode} [({SiloPort}, {GatewayPort})]",
                    clusteringConfiguration.ClusteringMode,
                    siloPort,
                    gatewayPort)
                sb.ConfigureEndpoints(siloPort, gatewayPort, listenOnAnyHostAddress = true)
            | _ ->
                logger.LogInformation(
                    "Configuring Endpoints and Silo Address for clustering mode {ClusteringMode} [{SiloAddress}:({SiloPort}, {GatewayPort})]",
                    clusteringConfiguration.ClusteringMode,
                    siloAddress,
                    siloPort,
                    gatewayPort)
                sb.ConfigureEndpoints(siloAddress, siloPort, gatewayPort, listenOnAnyHostAddress = true)

        siloBuilder.Configure configureClusterOptions
        |> configureClusterMembership
        |> configureEndpoints

    abstract ConfigureStorageProvider : IConfiguration -> StorageProviderConfiguration -> ISiloBuilder -> ISiloBuilder
    default __.ConfigureStorageProvider configuration storageProviderConfiguration siloBuilder =
        try
            match storageProviderConfiguration.PersistenceMode with
            | PersistenceModes.AzureTable ->
                siloBuilder.AddAzureTableGrainStorageAsDefault(fun (option : AzureTableStorageOptions) ->
                    option.ConnectionString <- storageProviderConfiguration.ConnectionString)
            | PersistenceModes.AzureBlob ->
                siloBuilder.AddAzureBlobGrainStorageAsDefault(fun (option : AzureBlobStorageOptions) ->
                    option.ConnectionString <- storageProviderConfiguration.ConnectionString)
            | PersistenceModes.InMemory | _ ->
                siloBuilder.AddMemoryGrainStorageAsDefault()
        finally
            logger.LogInformation(
                "Configuring Persistence for {PersistenceMode} [{ConnectionString}]",
                storageProviderConfiguration.PersistenceMode,
                storageProviderConfiguration.ConnectionString)

    abstract ConfigureReminderService : IConfiguration -> StorageProviderConfiguration -> ISiloBuilder -> ISiloBuilder
    default __.ConfigureReminderService configuration storageProviderConfiguration siloBuilder =
        try
            match storageProviderConfiguration.PersistenceMode with
            | PersistenceModes.AzureTable | PersistenceModes.AzureBlob ->
                siloBuilder.UseAzureTableReminderService(fun (option : AzureTableReminderStorageOptions) ->
                    option.ConnectionString <- storageProviderConfiguration.ConnectionString)
            | PersistenceModes.InMemory | _ ->
                siloBuilder.UseInMemoryReminderService()
        finally
            logger.LogInformation(
                "Configuring Reminders with persistence {PersistenceMode} [{ConnectionString}]",
                storageProviderConfiguration.PersistenceMode,
                storageProviderConfiguration.ConnectionString)

    abstract ConfigureApplicationInsights : string -> ISiloBuilder -> ISiloBuilder
    default __.ConfigureApplicationInsights instrumentationKey siloBuilder =
        siloBuilder.AddApplicationInsightsTelemetryConsumer instrumentationKey

    abstract ConfigureDashboard : ClusteringConfiguration -> ISiloBuilder -> ISiloBuilder
    default __.ConfigureDashboard clusteringConfiguration siloBuilder =
        match clusteringConfiguration.ClusteringMode with
        | ClusteringModes.HostLocal ->
            logger.LogInformation(
                "No dashboard available for clustering mode {ClusteringMode}",
                clusteringConfiguration.ClusteringMode)
            siloBuilder
        | _ ->
            logger.LogInformation(
                "Starting dashboard for clustering mode {ClusteringMode}",
                clusteringConfiguration.ClusteringMode);
            siloBuilder.UseDashboard();

    abstract ConfigureProcessExitHandlingOptions : ClusteringConfiguration -> ISiloBuilder -> ISiloBuilder
    default __.ConfigureProcessExitHandlingOptions clusteringConfiguration siloBuilder =
        let fastKillOnProcessExit =
            match clusteringConfiguration.ClusteringMode with
            | ClusteringModes.Kubernetes | ClusteringModes.Docker -> true
            | _ -> false

        logger.LogInformation(
            "`FastKillOnProcessExit = {FastKillOnProcessExit}` for clustering mode {ClusteringMode}",
            fastKillOnProcessExit,
            clusteringConfiguration.ClusteringMode)

        siloBuilder.Configure(fun (options : ProcessExitHandlingOptions) ->
            options.FastKillOnProcessExit <- fastKillOnProcessExit)

    abstract ConfigureApplicationParts : ISiloBuilder -> ISiloBuilder
    default __.ConfigureApplicationParts siloBuilder =
        siloBuilder.ConfigureApplicationParts(fun parts ->
            parts.AddFromApplicationBaseDirectory ()
            |> (fun apm -> apm.WithCodeGeneration())
            |> ignore)

    abstract ConfigureSiloHost : HostBuilderContext -> ISiloBuilder -> unit
    default this.ConfigureSiloHost hostBuilderContext siloBuilder =
        let configuration                = hostBuilderContext.Configuration;
        let clusteringConfiguration      = new ClusteringConfiguration(logger, configuration);
        let storageProviderConfiguration = new StorageProviderConfiguration(logger, configuration);

        if forceAzureClustering then clusteringConfiguration.ClusteringMode <- ClusteringModes.Azure;

        siloBuilder
        |> this.ConfigureServices
        |> this.ConfigureClustering                 configuration clusteringConfiguration
        |> this.ConfigureStorageProvider            configuration storageProviderConfiguration
        |> this.ConfigureApplicationInsights        configuration.AppInsightsKey
        |> this.ConfigureDashboard                  clusteringConfiguration
        |> this.ConfigureProcessExitHandlingOptions clusteringConfiguration
        |> this.ConfigureApplicationParts
        |> ignore
end
