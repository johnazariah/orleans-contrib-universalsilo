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

type SiloConfigurator() = class
    let loggerFactory = LoggerFactory.Create(fun builder -> builder.AddConsole () |> ignore)
    let logger = loggerFactory.CreateLogger("SiloConfigurator")

    member val public Logger = logger

    abstract SiloConfiguration : SiloConfiguration
    default __.SiloConfiguration =
        new SiloConfiguration()

    abstract StorageProviderConfiguration : StorageProviderConfiguration
    default __.StorageProviderConfiguration =
        new StorageProviderConfiguration()

    abstract ClusteringConfiguration : ClusteringConfiguration
    default __.ClusteringConfiguration =
        new ClusteringConfiguration()

    abstract TelemetryConfiguration : TelemetryConfiguration
    default __.TelemetryConfiguration =
        new TelemetryConfiguration()

    abstract ConfigureServices : IConfiguration -> UniversalSiloConfiguration -> ISiloBuilder -> ISiloBuilder
    default __.ConfigureServices configuration siloSettings siloBuilder =
        siloBuilder

    abstract ConfigureClustering : IConfiguration -> UniversalSiloConfiguration -> ISiloBuilder -> ISiloBuilder
    default this.ConfigureClustering configuration siloSettings siloBuilder =
        let configureClusterOptions (options : ClusterOptions) =
            options.ClusterId <- siloSettings.SiloConfiguration.ClusterId
            options.ServiceId <- siloSettings.SiloConfiguration.ServiceId

        let siloAddress = siloSettings.ClusteringConfiguration.SiloAddress
        let siloPort    = siloSettings.SiloConfiguration.SiloPort
        let gatewayPort = siloSettings.SiloConfiguration.GatewayPort

        let configureClusterMembership (sb : ISiloBuilder) =
            match siloSettings.ClusteringConfiguration.ClusteringMode with
            | ClusteringModes.Kubernetes ->
                logger.LogInformation ("{ClusteringMode} : NOT SUPPORTED", siloSettings.ClusteringConfiguration.ClusteringMode)
                raise <| NotSupportedException("K8s clustering not supported - use Azure or Docker clustering")

            | ClusteringModes.Azure ->
                let connectionString = siloSettings.ClusteringConfiguration.ConnectionString
                logger.LogInformation("Configuring Azure Storage Clustering {ConnectionString}", connectionString)
                sb.UseAzureStorageClustering(fun (options : AzureStorageClusteringOptions) ->
                    options.ConnectionString <- connectionString)

            | ClusteringModes.Docker | ClusteringModes.HostLocal | _ ->
                logger.LogInformation("Development Clustering running on [{SiloAddress}]:[{SiloPort}]", siloAddress, siloPort)
                IPEndPoint(siloAddress, siloPort) |> sb.UseDevelopmentClustering

        let configureEndpoints (sb : ISiloBuilder) =
            match siloSettings.ClusteringConfiguration.ClusteringMode with
            | ClusteringModes.Kubernetes | ClusteringModes.Azure ->
                logger.LogInformation(
                    "Configuring Endpoints without silo address for clustering mode {ClusteringMode} [({SiloPort}, {GatewayPort})]",
                    siloSettings.ClusteringConfiguration.ClusteringMode,
                    siloPort,
                    gatewayPort)
                sb.ConfigureEndpoints(siloPort, gatewayPort, listenOnAnyHostAddress = true)
            | _ ->
                logger.LogInformation(
                    "Configuring Endpoints and Silo Address for clustering mode {ClusteringMode} [{SiloAddress}:({SiloPort}, {GatewayPort})]",
                    siloSettings.ClusteringConfiguration.ClusteringMode,
                    siloAddress,
                    siloPort,
                    gatewayPort)
                sb.ConfigureEndpoints(siloAddress, siloPort, gatewayPort, listenOnAnyHostAddress = true)

        siloBuilder.Configure configureClusterOptions
        |> configureClusterMembership
        |> configureEndpoints

    abstract ConfigureStorageProvider : IConfiguration -> UniversalSiloConfiguration -> ISiloBuilder -> ISiloBuilder
    default __.ConfigureStorageProvider configuration siloSettings siloBuilder =
        try
            match siloSettings.StorageProviderConfiguration.PersistenceMode with
            | PersistenceModes.AzureTable ->
                siloBuilder.AddAzureTableGrainStorageAsDefault(fun (option : AzureTableStorageOptions) ->
                    option.ConnectionString <- siloSettings.StorageProviderConfiguration.ConnectionString)
            | PersistenceModes.AzureBlob ->
                siloBuilder.AddAzureBlobGrainStorageAsDefault(fun (option : AzureBlobStorageOptions) ->
                    option.ConnectionString <- siloSettings.StorageProviderConfiguration.ConnectionString)
            | PersistenceModes.InMemory | _ ->
                siloBuilder.AddMemoryGrainStorageAsDefault()
        finally
            logger.LogInformation(
                "Configuring Persistence for {PersistenceMode} [{ConnectionString}]",
                siloSettings.StorageProviderConfiguration.PersistenceMode,
                siloSettings.StorageProviderConfiguration.ConnectionString)

    abstract ConfigureReminderService : IConfiguration -> UniversalSiloConfiguration -> ISiloBuilder -> ISiloBuilder
    default __.ConfigureReminderService configuration siloSettings siloBuilder =
        try
            match siloSettings.StorageProviderConfiguration.PersistenceMode with
            | PersistenceModes.AzureTable | PersistenceModes.AzureBlob ->
                siloBuilder.UseAzureTableReminderService(fun (option : AzureTableReminderStorageOptions) ->
                    option.ConnectionString <- siloSettings.StorageProviderConfiguration.ConnectionString)
            | PersistenceModes.InMemory | _ ->
                siloBuilder.UseInMemoryReminderService()
        finally
            logger.LogInformation(
                "Configuring Reminders with persistence {PersistenceMode} [{ConnectionString}]",
                siloSettings.StorageProviderConfiguration.PersistenceMode,
                siloSettings.StorageProviderConfiguration.ConnectionString)

    abstract ConfigureApplicationInsights : IConfiguration -> UniversalSiloConfiguration -> ISiloBuilder -> ISiloBuilder
    default __.ConfigureApplicationInsights configuration siloSettings siloBuilder =
        siloBuilder.AddApplicationInsightsTelemetryConsumer siloSettings.TelemetryConfiguration.ApplicationInsightsKey

    abstract ConfigureDashboard : IConfiguration -> UniversalSiloConfiguration -> ISiloBuilder -> ISiloBuilder
    default __.ConfigureDashboard configuration siloSettings siloBuilder =
        match siloSettings.ClusteringConfiguration.ClusteringMode with
        | ClusteringModes.HostLocal ->
            logger.LogInformation(
                "No dashboard available for clustering mode {ClusteringMode}",
                siloSettings.ClusteringConfiguration.ClusteringMode)
            siloBuilder
        | _ ->
            logger.LogInformation(
                "Starting dashboard for clustering mode {ClusteringMode}",
                siloSettings.ClusteringConfiguration.ClusteringMode);
            siloBuilder.UseDashboard();

    abstract ConfigureProcessExitHandlingOptions : IConfiguration -> UniversalSiloConfiguration -> ISiloBuilder -> ISiloBuilder
    default __.ConfigureProcessExitHandlingOptions configuration siloSettings siloBuilder =
        let fastKillOnProcessExit =
            match siloSettings.ClusteringConfiguration.ClusteringMode with
            | ClusteringModes.Kubernetes | ClusteringModes.Docker -> true
            | _ -> false

        logger.LogInformation(
            "`FastKillOnProcessExit = {FastKillOnProcessExit}` for clustering mode {ClusteringMode}",
            fastKillOnProcessExit,
            siloSettings.ClusteringConfiguration.ClusteringMode)

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
        let clusteringConfiguration =
            this.ClusteringConfiguration
            |> BuildClusteringConfiguration logger hostBuilderContext.Configuration

        let storageProviderConfiguration =
            this.StorageProviderConfiguration
            |> BuildStorageProviderConfiguration logger hostBuilderContext.Configuration

        let siloConfiguration =
            this.SiloConfiguration
            |> BuildSiloConfiguration logger hostBuilderContext.Configuration

        let telemetryConfiguration =
            this.TelemetryConfiguration
            |> BuildTelemetryConfiguration logger hostBuilderContext.Configuration

        let configuration = {
            ClusteringConfiguration = clusteringConfiguration
            StorageProviderConfiguration = storageProviderConfiguration
            SiloConfiguration = siloConfiguration
            TelemetryConfiguration = telemetryConfiguration
        }

        siloBuilder
        |> this.ConfigureServices                   hostBuilderContext.Configuration configuration
        |> this.ConfigureClustering                 hostBuilderContext.Configuration configuration
        |> this.ConfigureStorageProvider            hostBuilderContext.Configuration configuration
        |> this.ConfigureApplicationInsights        hostBuilderContext.Configuration configuration
        |> this.ConfigureDashboard                  hostBuilderContext.Configuration configuration
        |> this.ConfigureProcessExitHandlingOptions hostBuilderContext.Configuration configuration
        |> this.ConfigureApplicationParts
        |> ignore
end
