namespace Orleans.Contrib.UniversalSilo.Configuration

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open System
open System.IO
open System.Net
open System.Runtime.CompilerServices

open Orleans.Contrib.UniversalSilo.Utilities
open Microsoft.Extensions.Hosting

[<AutoOpen>]
module Keys =

    let [<Literal>] ENV_SILO_ADDRESS          = "ENV_SILO_ADDRESS"

    let [<Literal>] ENV_CLUSTER_ID            = "ENV_CLUSTER_ID"
    let [<Literal>] ENV_SERVICE_ID            = "ENV_SERVICE_ID"
    let [<Literal>] ENV_SILO_PORT             = "ENV_SILO_PORT"
    let [<Literal>] ENV_GATEWAY_PORT          = "ENV_GATEWAY_PORT"

    let [<Literal>] ENV_CLUSTER_MODE          = "ENV_CLUSTER_MODE"
    let [<Literal>] ENV_CLUSTER_AZURE_STORAGE = "ENV_CLUSTER_AZURE_STORAGE"
    let [<Literal>] ENV_CLUSTER_DEV_STORAGE   = "ENV_CLUSTER_DEV_STORAGE"

    let [<Literal>] ENV_PERSIST_MODE          = "ENV_PERSIST_MODE"
    let [<Literal>] ENV_PERSIST_AZURE_STORAGE = "ENV_PERSIST_AZURE_STORAGE"
    let [<Literal>] ENV_PERSIST_DEV_STORAGE   = "ENV_PERSIST_DEV_STORAGE"

    let [<Literal>] ENV_APPINSIGHTS_KEY       = "APPINSIGHTS_INSTRUMENTATIONKEY"

[<AutoOpen>]
module Enumerations =
    type ClusteringModes =
    | HostLocal   = 0
    | Azure       = 1
    | Docker      = 2
    | Kubernetes  = 3

    type PersistenceModes =
    | InMemory    = 0
    | AzureTable  = 1
    | AzureBlob   = 2

module private StringOps =
    let [<Literal>] TruncatedLength = 16
    let truncateSecretString (s : String) =
        if (String.IsNullOrWhiteSpace s) then
            String.Empty
        else if (s.Length <= TruncatedLength + 1) then
            s
        else
            sprintf "%s..." s.[0..TruncatedLength]

type SiloConfiguration() = class
    member val ClusterId   = "development"    with get, set
    member val ServiceId   = "universal-silo" with get, set
    member val SiloPort    = 11111            with get, set
    member val GatewayPort = 30000            with get, set

    override this.ToString() =
        sprintf "{ ClusterId : '%s'; ServiceId : '%s'; SiloPort : %d; GatewayPort : %d }"
            this.ClusterId
            this.ServiceId
            this.SiloPort
            this.GatewayPort
end

type ClusteringConfiguration() = class
    member val ClusteringMode   = ClusteringModes.HostLocal    with get, set
    member val ConnectionString = "UseDevelopmentStorage=true" with get, set
    member val SiloAddress      = IPAddress.None               with get, set

    override this.ToString() =
        sprintf "{ ClusteringMode : %A; SiloAddress : %O; ConnectionString : '%s' }"
            this.ClusteringMode
            this.SiloAddress
            (StringOps.truncateSecretString this.ConnectionString)
end

type StorageProviderConfiguration() = class
    member val PersistenceMode  = PersistenceModes.InMemory    with get, set
    member val ConnectionString = "UseDevelopmentStorage=true" with get, set

    override this.ToString() =
        sprintf "{ PersistenceMode : %A; ConnectionString : '%s' }"
            this.PersistenceMode
            (StringOps.truncateSecretString this.ConnectionString)
end

type TelemetryConfiguration() = class
    member val ApplicationInsightsKey = String.Empty with get, set

    override this.ToString() =
        sprintf "{ ApplicationInsightsKey : '%s' }"
            (StringOps.truncateSecretString this.ApplicationInsightsKey)
end

type UniversalSiloConfiguration = {
    SiloConfiguration : SiloConfiguration
    ClusteringConfiguration : ClusteringConfiguration
    StorageProviderConfiguration : StorageProviderConfiguration
    TelemetryConfiguration : TelemetryConfiguration
}

[<AutoOpen>]
[<Extension>]
module Extensions =
    type IHostBuilder with
        member this.ConfigureHostConfigurationDefaults() =
            this.ConfigureHostConfiguration (fun cb ->
                cb
                    .SetBasePath(Directory.GetCurrentDirectory())
                |> ignore)

        member this.ApplyHostConfigurationFunc f =
            f this

    let [<Extension>] inline ApplyHostConfigurationFunc (_this : IHostBuilder) (_func : Func<IHostBuilder, IHostBuilder>) =
        _this.ApplyHostConfigurationFunc (fun hb -> _func.Invoke hb)

    let [<Extension>] inline ConfigureHostConfigurationDefaults (_this : IHostBuilder) =
        _this.ConfigureHostConfigurationDefaults()

    let [<Extension>] inline Apply (_this : 'a) (func : Func<'a, 'a>) : 'a =
        func.Invoke _this

    let [<Extension>] inline With (_this : 'a) (action : Action<'a>) : 'a =
        action.Invoke _this
        _this

[<AutoOpen>]
module Configuration =
    type IConfiguration with
        // clustering settings
        member private this.SiloAddress = this.[ENV_SILO_ADDRESS]
        member private this.ClusteringMode  def =
            def
            |> this.[ENV_CLUSTER_MODE].EnumOrDefault

        member private this.AzureClusteringConnectionString def =
            def
            |> this.[ENV_CLUSTER_DEV_STORAGE].StringOrDefault
            |> this.[ENV_CLUSTER_AZURE_STORAGE].StringOrDefault

        // storage provider settings
        member private this.PersistenceMode def =
            def
            |> this.[ENV_PERSIST_MODE].EnumOrDefault

        member private this.AzureStorageConnectionString def =
            def
            |> this.[ENV_PERSIST_DEV_STORAGE].StringOrDefault
            |> this.[ENV_PERSIST_AZURE_STORAGE].StringOrDefault

        // silo settings
        member private this.ClusterId       def = def |> this.[ENV_CLUSTER_ID].StringOrDefault
        member private this.ServiceId       def = def |> this.[ENV_SERVICE_ID].StringOrDefault
        member private this.SiloPort        def = def |> this.[ENV_SILO_PORT].IntOrDefault
        member private this.GatewayPort     def = def |> this.[ENV_GATEWAY_PORT].IntOrDefault

        // telemetry settings
        member private this.AppInsightsKey  def = def |> this.[ENV_APPINSIGHTS_KEY].StringOrDefault

    let private applyValuesFromConfigurationFiles (config : IConfiguration) (localConfiguration : 'a) =
        let localConfigurationFile = config.GetSection (typeof<'a>.Name)
        if localConfigurationFile.Exists () then
            localConfigurationFile.Bind(localConfiguration)
        localConfiguration

    let private printConfiguration (logger : ILogger) prefix (c : 'a) =
        logger.LogInformation ("<{ConfigurationType}> {Message} : Configuration: {Configuration}", (typeof<'a>.Name), prefix, (sprintf "%A" c))
        c

    let private buildConfiguration applyValuesFromEnvironment (logger : ILogger) (config : IConfiguration) (defaultSettings : 'a) =
        defaultSettings
        |> printConfiguration logger "Initial State"
        |> applyValuesFromConfigurationFiles config
        |> printConfiguration logger "Applied values from settings files"
        |> applyValuesFromEnvironment
        |> printConfiguration logger "Applied values from environment"

    let BuildClusteringConfiguration (logger : ILogger) (config : IConfiguration) (defaultSettings : ClusteringConfiguration) =
        let applyValuesFromEnvironment (localConfiguration : ClusteringConfiguration) =
            localConfiguration.ClusteringMode   <- config.ClusteringMode localConfiguration.ClusteringMode
            localConfiguration.ConnectionString <- config.AzureClusteringConnectionString localConfiguration.ConnectionString
            localConfiguration

        let adjustValuesBasedOnContext (localConfiguration : ClusteringConfiguration) =
            let connectionString =
                match localConfiguration.ClusteringMode with
                | ClusteringModes.Azure  ->
                    logger.LogInformation("Building connection string for Azure Clustering")
                    config.AzureClusteringConnectionString localConfiguration.ConnectionString
                | _ ->
                    logger.LogInformation("Blanking connection string because we are not using Azure Clustering")
                    String.Empty

            let clusteringMode =
                match localConfiguration.ClusteringMode with
                | ClusteringModes.HostLocal ->
                    if (File.Exists("/.dockerenv")) then
                        logger.LogInformation("Deduced we are running within a Docker environment. Resetting ClusteringMode to Docker")
                        ClusteringModes.Docker
                    else if (Directory.Exists("/var/run/secrets/kubernetes.io")) then
                        logger.LogInformation("Deduced we are running within a Kubernetes environment. Resetting ClusteringMode to Kubernetes")
                        ClusteringModes.Kubernetes
                    else
                        localConfiguration.ClusteringMode
                | _ ->
                    localConfiguration.ClusteringMode
            localConfiguration.ClusteringMode   <- clusteringMode
            localConfiguration.ConnectionString <- connectionString
            localConfiguration

        let setSiloAddress (localConfiguration : ClusteringConfiguration) =
            let siloAddress =
                match IPAddress.TryParse config.SiloAddress with
                | (true, result) ->
                    logger.LogInformation("A silo address was specified in the $ENV_SILO_ADDRESS$ environment variable or command line argument. {SiloAddress}", result)
                    result
                | _ ->
                    match localConfiguration.ClusteringMode with
                    | ClusteringModes.Docker ->
                        logger.LogInformation("Computing a stable IP address for a Docker environment")
                        StableIpAddress

                    | ClusteringModes.Azure  | ClusteringModes.Kubernetes ->
                        logger.LogInformation("No computed IP address for Azure or Kubernetes clustering!")
                        IPAddress.None

                    | ClusteringModes.HostLocal
                    | _ ->
                        logger.LogInformation("Using the loopback address for [{ClusteringMode}]", localConfiguration.ClusteringMode)
                        IPAddress.Loopback
            localConfiguration.SiloAddress <- siloAddress
            localConfiguration

        defaultSettings
        |> buildConfiguration applyValuesFromEnvironment logger config
        |> adjustValuesBasedOnContext
        |> printConfiguration logger "Adjusted values based on context"
        |> setSiloAddress
        |> printConfiguration logger "Final State"

    let BuildStorageProviderConfiguration (logger : ILogger) (config : IConfiguration) (defaultSettings : StorageProviderConfiguration) =
        let applyValuesFromEnvironment (localConfiguration : StorageProviderConfiguration) =
            localConfiguration.PersistenceMode  <- config.PersistenceMode localConfiguration.PersistenceMode
            let connectionString =
                match localConfiguration.PersistenceMode with
                | PersistenceModes.AzureTable | PersistenceModes.AzureBlob ->
                    logger.LogInformation("Obtained ConnectionString from Envirornment because PersistenceMode is {PersistenceMode}", localConfiguration.PersistenceMode)
                    config.AzureStorageConnectionString localConfiguration.ConnectionString
                | _ | PersistenceModes.InMemory ->
                    logger.LogInformation("Blanking connection string because PersistenceMode is {PersistenceMode}", localConfiguration.PersistenceMode)
                    String.Empty
            localConfiguration.ConnectionString <- connectionString
            localConfiguration

        defaultSettings
        |> buildConfiguration applyValuesFromEnvironment logger config
        |> printConfiguration logger "Final State"

    let BuildSiloConfiguration (logger : ILogger) (config : IConfiguration) (defaultSettings : SiloConfiguration) =
        let applyValuesFromEnvironment (localConfiguration : SiloConfiguration) =
            localConfiguration.ClusterId   <- config.ClusterId   localConfiguration.ClusterId
            localConfiguration.ServiceId   <- config.ServiceId   localConfiguration.ServiceId
            localConfiguration.SiloPort    <- config.SiloPort    localConfiguration.SiloPort
            localConfiguration.GatewayPort <- config.GatewayPort localConfiguration.GatewayPort
            localConfiguration

        defaultSettings
        |> buildConfiguration applyValuesFromEnvironment logger config
        |> printConfiguration logger "Final State"

    let BuildTelemetryConfiguration (logger : ILogger) (config : IConfiguration) (defaultSettings : TelemetryConfiguration) =
        let applyValuesFromEnvironment (localConfiguration : TelemetryConfiguration) =
            localConfiguration.ApplicationInsightsKey  <- config.AppInsightsKey localConfiguration.ApplicationInsightsKey
            localConfiguration

        defaultSettings
        |> buildConfiguration applyValuesFromEnvironment logger config
        |> printConfiguration logger "Final State"
