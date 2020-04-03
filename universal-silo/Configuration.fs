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
    let [<Literal>] ENV_CLUSTER_ID                             = "ENV_CLUSTER_ID"
    let [<Literal>] ENV_SERVICE_ID                             = "ENV_SERVICE_ID"
    let [<Literal>] ENV_SILO_PORT                              = "ENV_SILO_PORT"
    let [<Literal>] ENV_GATEWAY_PORT                           = "ENV_GATEWAY_PORT"
    let [<Literal>] ENV_CLUSTER_MODE                           = "ENV_CLUSTER_MODE"
    let [<Literal>] ENV_CLUSTER_AZURE_STORAGE                  = "ENV_CLUSTER_AZURE_STORAGE"
    let [<Literal>] ENV_DEFAULT_CLUSTER_AZURE_STORAGE          = "ENV_DEFAULT_CLUSTER_AZURE_STORAGE"
    let [<Literal>] ENV_PERSISTENCE_AZURE_TABLE                = "ENV_PERSISTENCE_AZURE_TABLE"
    let [<Literal>] ENV_DEFAULT_PERSISTENCE_AZURE_TABLE        = "ENV_DEFAULT_PERSISTENCE_AZURE_TABLE"
    let [<Literal>] ENV_PERSISTENCE_AZURE_BLOB                 = "ENV_PERSISTENCE_AZURE_BLOB"
    let [<Literal>] ENV_DEFAULT_PERSISTENCE_AZURE_BLOB         = "ENV_DEFAULT_PERSISTENCE_AZURE_BLOB"
    let [<Literal>] ENV_DATA_STORAGE_CONNECTION_STRING         = "ENV_DATA_STORAGE_CONNECTION_STRING"
    let [<Literal>] ENV_DEFAULT_DATA_STORAGE_CONNECTION_STRING = "ENV_DEFAULT_DATA_STORAGE_CONNECTION_STRING"
    let [<Literal>] ENV_DATA_STORAGE_COLLECTION_NAME           = "ENV_DATA_STORAGE_COLLECTION_NAME"
    let [<Literal>] ENV_APPINSIGHTS_KEY                        = "APPINSIGHTS_INSTRUMENTATIONKEY"

[<AutoOpen>]
module Enumerations =
    type ClusteringModes =
    | HostLocal   = 0
    | Azure       = 1
    | Docker      = 2
    | Kubernetes  = 3

    type PersistenceModes =
    | InMemory             = 0
    | AzureTable           = 1
    | AzureBlob            = 2
    //| CosmosDB             = 3


[<AutoOpen>]
[<Extension>]
module Extensions =
    type internal IConfiguration with
        member this.AzureClusteringConnectionString =
            "UseDevelopmentStorage=true"
            |> this.[ENV_DEFAULT_CLUSTER_AZURE_STORAGE].StringOrDefault
            |> this.[ENV_CLUSTER_AZURE_STORAGE].StringOrDefault

        member this.AzureTableStorageConnectionString =
            String.Empty
            |> this.[ENV_DEFAULT_PERSISTENCE_AZURE_TABLE].StringOrDefault
            |> this.[ENV_PERSISTENCE_AZURE_TABLE].StringOrDefault

        member this.AzureBlobStorageConnectionString =
            String.Empty
            |> this.[ENV_DEFAULT_PERSISTENCE_AZURE_BLOB].StringOrDefault
            |> this.[ENV_PERSISTENCE_AZURE_BLOB].StringOrDefault

        member this.SiloAddress    = this.["silo-address"]
        member this.ClusteringMode = ClusteringModes.HostLocal |> this.[ENV_CLUSTER_MODE].EnumOrDefault
        member this.SiloPort       = 11111                     |> this.[ENV_SILO_PORT].IntOrDefault
        member this.GatewayPort    = 30000                     |> this.[ENV_GATEWAY_PORT].IntOrDefault
        member this.ClusterId      = "development"             |> this.[ENV_CLUSTER_ID].StringOrDefault
        member this.ServiceId      = "universal-silo"          |> this.[ENV_SERVICE_ID].StringOrDefault
        member this.AppInsightsKey = String.Empty              |> this.[ENV_APPINSIGHTS_KEY].StringOrDefault

    /// Returns a silo address set in the $silo-address$ environment variable or command-line argument
    let [<Extension>] inline GetSiloAddress                     (_this : IConfiguration) = _this.SiloAddress

    /// Returns the clustering mode set in the $ENV_CLUSTER_MODE$ environment variable, or ClusteringModes.HostLocal if not set
    let [<Extension>] inline GetClusteringMode                  (_this : IConfiguration) = _this.ClusteringMode

    /// Returns the silo port set in the $ENV_SILO_PORT$ environment variable, or 11111 if not set
    let [<Extension>] inline GetSiloPort                        (_this : IConfiguration) = _this.SiloPort

    /// Returns the gateway port set in the $ENV_GATEWAY_PORT$ environment variable, or 30000 if not set
    let [<Extension>] inline GetGatewayPort                     (_this : IConfiguration) = _this.GatewayPort

    /// Returns the cluster id set in the $ENV_CLUSTER_ID$ environment variable, or 'development' if not set
    let [<Extension>] inline GetClusterId                       (_this : IConfiguration) = _this.ClusterId

    /// Returns the service id set in the $ENV_SERVICE_ID$ environment variable, or 'universal-silo' if not set
    let [<Extension>] inline GetServiceId                       (_this : IConfiguration) = _this.ServiceId

    /// Returns the service id set in the $ENV_APPINSIGHTS_KEY$ environment variable, or the empty string if not set
    let [<Extension>] inline GetAppInsightsKey                  (_this : IConfiguration) = _this.AppInsightsKey

    /// Returns the connection string to be used when the ClusteringMode is set to ClusteringModes.Azure.
    /// Attempts to return the value set in the $ENV_CLUSTER_AZURE_STORAGE$ environment variable. If not set,
    /// attempts to return the value set in the $ENV_DEFAULT_CLUSTER_AZURE_STORAGE$ environment variable. If not set,
    /// returns 'UseDevelopmentStorage=true', pointing to the local storage emulator
    let [<Extension>] inline GetAzureClusteringConnectionString (_this : IConfiguration) = _this.AzureClusteringConnectionString

    /// Returns the connection string to be used when the PersistenceMode is set to PersistenceModes.AzureTable.
    /// Attempts to return the value set in the $ENV_PERSISTENCE_AZURE_TABLE$ environment variable. If not set,
    /// attempts to return the value set in the $ENV_DEFAULT_PERSISTENCE_AZURE_TABLE$ environment variable. If not set,
    /// returns the empty string
    let [<Extension>] inline GetAzureTableStorageConnectionString (_this : IConfiguration) = _this.AzureClusteringConnectionString

    /// Returns the connection string to be used when the PersistenceMode is set to PersistenceModes.AzureTable.
    /// Attempts to return the value set in the $ENV_PERSISTENCE_AZURE_BLOB$ environment variable. If not set,
    /// attempts to return the value set in the $ENV_DEFAULT_PERSISTENCE_AZURE_BLOB$ environment variable. If not set,
    /// returns the empty string
    let [<Extension>] inline GetAzureBlobStorageConnectionString (_this : IConfiguration) = _this.AzureClusteringConnectionString

    /// applies `_action` on `this` as an extension method. returns `_this` for further chaining.
    let [<Extension>] inline ApplyConfiguration(_this : 'a, _action : Func<'a, 'a>) =
        _action.Invoke _this

    let [<Extension>] inline ApplyAppConfiguration(_this : IHostBuilder) =
        let configureAppConfiguration (cb : IConfigurationBuilder) =
            cb
            |> (fun c -> c.SetBasePath(Directory.GetCurrentDirectory()))
            |> (fun c -> c.AddJsonFile("clustering.json",  optional = true, reloadOnChange = false))
            |> (fun c -> c.AddJsonFile("persistence.json", optional = true, reloadOnChange = false))
            |> ignore

        _this.ConfigureAppConfiguration configureAppConfiguration


[<AutoOpen>]
module Clustering =
    type internal ClusteringConfigurationObject() = class
        member val ClusteringMode = ClusteringModes.HostLocal with get, set
        member val RunInLocalEnvironment = false with get, set
    end

    type ClusteringConfiguration (Logger : ILogger, Configuration : IConfiguration) = class
        let localConfiguration = new ClusteringConfigurationObject ()
        do
            try
                let localConfigurationFile = Configuration.GetSection (nameof(ClusteringConfiguration))
                if localConfigurationFile.Exists () then
                    localConfigurationFile.Bind(localConfiguration)
                    localConfiguration.RunInLocalEnvironment <- true
                    Logger.LogInformation("Deducing that we are running locally because a clustering configuration was found.")
                else
                    localConfiguration.ClusteringMode <- Configuration.ClusteringMode;
                    Logger.LogInformation("Not running locally. Clustering mode from environment is: {ClusteringMode}", localConfiguration.ClusteringMode)

                    if (File.Exists("/.dockerenv")) then
                        localConfiguration.ClusteringMode <- ClusteringModes.Docker
                        Logger.LogInformation("Deduced we are running within a Docker environment.")

                    if (Directory.Exists("/var/run/secrets/kubernetes.io")) then
                        localConfiguration.ClusteringMode <- ClusteringModes.Kubernetes
                        Logger.LogInformation("Deduced we are running within a Kubernetes environment.")
            finally
                Logger.LogInformation("Finally configuring with clustering mode [{ClusteringMode}]",        localConfiguration.ClusteringMode)
                Logger.LogInformation("Finally setting up to run in local dev environment [{LocalDevelopmentEnvironment}]", localConfiguration.RunInLocalEnvironment)

        /// Returns the ClusteringMode detected from any specified configuration, defaulting to ClusteringModes.HostLocal
        member val ClusteringMode = localConfiguration.ClusteringMode with get, set

        /// True if a local configuration file was specified - helpful for local Docker and K8s environments. False otherwise.
        member val RunInLocalEnvironment = localConfiguration.RunInLocalEnvironment with get, set

        /// Returns the IP Address the Silo declares that it is hosted at
        /// If the silo is hosted in a Docker container, a stable, non-loopback IP address is detected and used
        /// If the clustering configuration is set to ClusteringModes.Azure or ClusteringModes.Kubernetes, no IP address is returned as this should be set by the runtime
        /// If the clustering configuration is set to ClusteringModes.HostLocal, then the loopback IP address is used
        member this.GetSiloAddress() =
            match IPAddress.TryParse Configuration.SiloAddress with
            | (true, result) ->
                Logger.LogInformation("A silo address was specified in the $silo-address$ environment variable or command line argument. {SiloAddress}", result)
                result
            | _ ->
                match this.ClusteringMode with
                | ClusteringModes.Docker ->
                    Logger.LogInformation("Computing a stable IP address for a Docker environment")
                    StableIpAddress

                | ClusteringModes.Azure ->
                    Logger.LogInformation("No computed IP address for Azure or Kubernetes clustering!")
                    IPAddress.None

                | ClusteringModes.Kubernetes ->
                    Logger.LogInformation("No computed IP address for Azure or Kubernetes clustering!")
                    IPAddress.None

                | ClusteringModes.HostLocal
                | _ ->
                    Logger.LogInformation("Using the loopback address for [{ClusteringMode}]", this.ClusteringMode);
                    IPAddress.Loopback
    end

[<AutoOpen>]
module StorageProvider =
    type StorageProviderConfigurationObject() = class
        member val PersistenceMode : PersistenceModes = PersistenceModes.InMemory with get, set
        member val ConnectionString : string = "InMemory" with get, set
    end

    type StorageProviderConfiguration (Logger : ILogger, Configuration : IConfiguration) = class
        let localConfiguration = new StorageProviderConfigurationObject()
        do
            try
                let localConfigurationFile = Configuration.GetSection (nameof(StorageProviderConfiguration))

                if localConfigurationFile.Exists () then
                    localConfigurationFile.Bind(localConfiguration)
                    Logger.LogInformation("Deducing that we are running locally because a persistence configuration was found.")

                else if (not <| String.IsNullOrWhiteSpace Configuration.AzureTableStorageConnectionString) then
                    localConfiguration.PersistenceMode  <- PersistenceModes.AzureTable
                    localConfiguration.ConnectionString <- Configuration.AzureTableStorageConnectionString
                else if (not <| String.IsNullOrWhiteSpace Configuration.AzureBlobStorageConnectionString) then
                    localConfiguration.PersistenceMode  <- PersistenceModes.AzureBlob
                    localConfiguration.ConnectionString <- Configuration.AzureBlobStorageConnectionString
                else
                    localConfiguration.PersistenceMode  <- PersistenceModes.InMemory
                    localConfiguration.ConnectionString <- "InMemory"
            finally
                Logger.LogInformation
                    ("Finally configuring with persistence mode [{PersistenceMode}] with connection string [{ConnectionString}]",
                    localConfiguration.PersistenceMode,
                    localConfiguration.ConnectionString)

        /// Returns the PersistenceMode detected from any specified configuration, defaulting to PersistenceModes.InMemory
        member val PersistenceMode = localConfiguration.PersistenceMode with get, set
        /// Returns the connection string detected from any specified configuration, defaulting to the literal string 'InMemory' signifying in memory storage
        member val ConnectionString = localConfiguration.ConnectionString with get, set
    end